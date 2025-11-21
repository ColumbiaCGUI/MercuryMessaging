// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met: 
// 
//  * Redistributions of source code must retain the above copyright notice, 
//    this list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//  * Neither the name of Columbia University nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE. 
//  
// =============================================================
// Authors: 
// Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
//  
//  
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using MercuryMessaging.Support.Extensions;
using MercuryMessaging.Support.Data;
using MercuryMessaging.Task;
using UnityEngine;


namespace MercuryMessaging
{
    /// <summary>
    /// Base routing node of Mercury XM.
    /// Contains a list of MmResponders and sends MmMessages
    /// to responders in list, following the specifications of 
    /// control blocks to MmInvoke method.
    /// </summary>
	public class MmRelayNode : MmResponder, IMmNode
    {
        #region Member Variables

        /// <summary>
        /// Performance Mode - disables debug message tracking overhead.
        /// Enable this during performance testing to remove UpdateMessages() overhead.
        /// When true: messageBuffer and message history tracking are disabled.
        /// When false: Full debug visualization and message tracking enabled.
        /// </summary>
        [Header("Performance Settings")]
        [Tooltip("Enable during performance tests to disable debug overhead (UpdateMessages tracking)")]
        public static bool PerformanceMode = false;

        /// <summary>
        /// Global toggle for routing performance profiling.
        /// When enabled, routing methods (HandleAdvancedRouting, ResolvePathTargets) will log timing data.
        /// Can be overridden per-message via MmRoutingOptions.EnableProfiling.
        /// Default: false (zero overhead when disabled)
        /// </summary>
        [Tooltip("Enable to profile routing performance globally")]
        public static bool EnableRoutingProfiler = false;

        /// <summary>
        /// Global threshold for routing profiling logs (milliseconds).
        /// Routing operations slower than this threshold will be logged.
        /// Can be overridden per-message via MmRoutingOptions.ProfilingThresholdMs.
        /// Default: 1.0ms
        /// </summary>
        [Tooltip("Log routing operations that exceed this threshold (ms)")]
        public static float ProfilingThresholdMs = 1.0f;

        // The position of the graph node in the graph view.
        public Vector2 nodePosition = new Vector2(0, 0);

        public int layer;

        /// <summary>
        /// Size of the message history circular buffers.
        /// Controls how many recent incoming and outgoing messages are tracked for debugging.
        /// Valid range: 10-10000. Default: 100.
        /// </summary>
        [Header("Message History Settings")]
        [Tooltip("Number of recent messages to keep in history for debugging (10-10000)")]
        [Range(10, 10000)]
        public int messageHistorySize = 100;

        public MmCircularBuffer<string> messageInList;

        public MmCircularBuffer<string> messageOutList;

        /// <summary>
        /// Maximum number of relay hops a message can make before being dropped.
        /// Prevents infinite loops in circular or complex hierarchies.
        /// Valid range: 5-1000. Default: 50.
        /// Set to 0 to disable hop limit checking (not recommended).
        /// </summary>
        [Header("Loop Prevention Settings")]
        [Tooltip("Maximum relay hops before message is dropped (prevents infinite loops). 0 = disabled")]
        [Range(0, 1000)]
        public int maxMessageHops = 50;

        /// <summary>
        /// Enable cycle detection by tracking visited nodes.
        /// When enabled, messages track which nodes they've visited and stop if they revisit a node.
        /// More aggressive than hop counting, but uses more memory.
        /// </summary>
        [Tooltip("Track visited nodes to detect and prevent circular message paths")]
        public bool enableCycleDetection = true;

        public Transform positionOffset;

        private Color colorA = new Color (93f/255f, 58f/255f, 155f/255f);
        private Color colorB = new Color (230f/255f, 97f/255f, 0f);
        private Color colorC = new Color (64f/255f, 176f/255f, 166f/255f);
        private Color colorD = new Color (230f/255f, 97f/255f, 0f);

        private List<MmMessage> messageBuffer = new List<MmMessage>();

        /// <summary>
        ///  Queue of MmResponders to add once list is no longer in use
        /// by an MmInvoke
        /// </summary>
		public Queue<MmRoutingTableItem> MmRespondersToAdd =
			new Queue<MmRoutingTableItem>();

        /// <summary>
        /// Queue of Memessages to route if serialExecution is enabled 
        /// and messages are received while another message is being executed.
        /// </summary>
        protected Queue<KeyValuePair<MmMessageType, MmMessage>> SerialExecutionQueue =
            new Queue<KeyValuePair<MmMessageType, MmMessage>>();

		/// <summary>
		/// Parents of this relay node.
		/// </summary>
		private List<MmRelayNode> MmParentList =
			new List<MmRelayNode>(); 

        /// <summary>
        /// Flag to protect priority list from being modified while it's being iterated over
        /// </summary>
		public bool doNotModifyRoutingTable;

        /// <summary>
        /// Does the node convert the message to a local message from networked 
        /// on post send in order to guarantee that it does not send it over the network 
        /// deeper in the hierarchy.
        /// </summary>
        public bool FlipNetworkFlagOnSend = false;

        /// <summary>
        /// Allows Node to pass a Network-only message into the hierarchy
        /// </summary>
        public bool AllowNetworkPropagationLocally = false;

        /// <summary>
        /// Indicates whether cloned MmResponders should be added as children
        /// to the MmRelayNode's GameObject.
        /// </summary>
        public bool ReparentClonedRespondersToSelf = true;

        /// <summary>
        /// Associated MmNetworkResponder.
        /// If an MmNetworkResponder is attached o the same GameObject,
        ///     it will automatically attach to this MmRelayNode.
        ///     This turns all MmMethod invocations into networked
        ///         MmMethod invocations, with no additional effort.
        /// </summary>
        public IMmNetworkResponder MmNetworkResponder { get; private set; }

        /// <summary>
        /// Experimental: Timestamp of last message.
        /// Can be serialized.
        /// </summary>
        private string _prevMessageTime;

        /// <summary>
        /// Indicates whether MmInvoke is currently executing a call.
        /// </summary>
        private bool _executing;

        /// <summary>
        /// Indicates whether the MmRelayNode is ready for use
        /// This gets set either in Awake or on the first 
        /// MmInvoke.
        /// </summary>
        public bool Initialized;

        /// <summary>
        /// Indicates whether the message was adjusted.
        /// </summary>
        private bool dirty;

        /// <summary>
        /// Experimental - Allows forced order on 
        /// MmInvocations received simultaneously
        /// </summary>
        private bool serialExecution = false;

        /// <summary>
        /// There may be an issue where a message is received before 
        /// self responders have been added to the list.
        /// In order to resolve that issue, we allow the node
        /// to automatically grab all responders.
        /// The consequence here is that you cannot have responders
        /// on a node that do not automatically get added to the list.
        /// </summary>
        public bool AutoGrabAttachedResponders = true;

        /// <summary>
        /// List of associated Mercury Responders.
        /// Each Mercury Routing Table Item contains:
        ///     MmResponder,
        ///     Name,
        ///     Level (Self, Child, Parent),
        ///     Cloneable (Indicates whether the Responder
        ///         should be cloned when MmRelayNode is awoken),
        ///     MmTag (Multi-tag filter supported by Mercury XM).
        /// </summary>
        [Header("Mercury Routing Table")]
        [ReorderableList]
        public MmRoutingTable RoutingTable;

        #endregion

        #region Properties

        /// <summary>
        /// MmRelayNode name: returns GameObject name.
        /// </summary>
        public string Name
        {
            get { return gameObject.name; }
        }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Creates an empty MmResponder list on construction.
        /// </summary>
		public MmRelayNode()
		{
            //This was needed at a certain point to deal with usage
            //  of the lists before Awake occured. However,
            //  this causes an issue with the list
            //  present in the editor.
			RoutingTable = new MmRoutingTable ();
		}

        /// <summary>
        /// Grab attached MmNetworkResponder, if present.
        /// Detect and refresh parents.
        /// Instantiate any cloneable MmResponders.
        /// </summary>
		public override void Awake()
        {
            MmNetworkResponder = GetComponent<IMmNetworkResponder>();

            // Initialize message history circular buffers with validated size
            int validatedSize = Mathf.Clamp(messageHistorySize, 10, 10000);
            if (validatedSize != messageHistorySize)
            {
                MmLogger.LogFramework($"Message history size {messageHistorySize} out of range. Clamped to {validatedSize}.");
                messageHistorySize = validatedSize;
            }
            messageInList = new MmCircularBuffer<string>(messageHistorySize);
            messageOutList = new MmCircularBuffer<string>(messageHistorySize);

            InitializeNode();
            InstantiateSubResponders();

			base.Awake ();

            MmLogger.LogFramework(gameObject.name + " MmRelayNode Awake called.");
        }

        /// <summary>
        /// Calls MmOnStartComplete through MmResponder Start.
        /// </summary>
        public override void Start()
        {
			base.Start ();

            MmLogger.LogFramework(gameObject.name + " MmRelayNode Start called.");

            int colorIndex = layer % 4;
            Color currentColor;
            if(colorIndex == 0)
            {
                currentColor = colorA;
            }
            else if(colorIndex == 1)
            {
                currentColor = colorB;
            }
            else if(colorIndex == 2)
            {
                currentColor = colorC;
            }
            else {
                currentColor = colorD;
            }

            if(positionOffset == null)
            {
                positionOffset = gameObject.transform;
            }
        }

        public void UpdateMessages(MmMessage message)   
        {
            List<MmRoutingTableItem> items = new List<MmRoutingTableItem>();

            // Determine which level to filter by
            if (message.MetadataBlock.LevelFilter == MmLevelFilter.Parent)
            {
                items = RoutingTable.GetMmRoutingTableItems(MmRoutingTable.ListFilter.All, MmLevelFilter.Parent);
            }
            else if (message.MetadataBlock.LevelFilter == MmLevelFilter.Child)
            {
                items = RoutingTable.GetMmRoutingTableItems(MmRoutingTable.ListFilter.All, MmLevelFilter.Child);
            }

            // Update items and propagate the message recursively
            foreach (MmRoutingTableItem item in items)
            {
                if (item.Tags == message.MetadataBlock.Tag || message.MetadataBlock.Tag == (MmTag)(-1))
                {
                    UpdateItemAndPropagate(item, message);
                }
            }
        }

        private void UpdateItemAndPropagate(MmRoutingTableItem item, MmMessage message)
        {
            System.DateTime currentTime = System.DateTime.Now;
            // Update the messageOutList for the current node
            messageOutList.Insert(0, item.Name + " : " + message.MmMessageType.ToString() + "\n" + currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
            // No truncation needed - circular buffer handles it automatically

            // If the item has a responder that is a MmRelayNode, update its messageInList
            if (item.Responder is MmRelayNode relayNode && item.Responder != this)
            {
                relayNode.messageInList.Insert(0, gameObject.name + " : " + message.MmMessageType.ToString() + "\n" + currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
                // No truncation needed - circular buffer handles it automatically

                // Recursively update the child nodes of the current relay node
                relayNode.UpdateMessages(message);
            }
        }

        // ---------------------------------------------------------
        

        protected void InitializeNode()
        {
            //Todo: Needs to happen via the editor or through a manual invocation in code.
            if (!Initialized)
            {
                RefreshParents();
                Initialized = true;

                if (AutoGrabAttachedResponders)
                    MmRefreshResponders();
            }
        }

        /// <summary>
        /// Iterate over RoutingTable items and instantiate any GameObjects that were
        /// marked as needing instantiating by the MmRoutingTable's creator.
        /// </summary>
        private void InstantiateSubResponders()
        {
            for (int i = 0; i < RoutingTable.Count; i++)
            {
                if (RoutingTable[i].Clone)
                {
                    GameObject cloneGameObject = RoutingTable[i].CloneGameObject();

                    if (ReparentClonedRespondersToSelf)
                    {
                        cloneGameObject.transform.parent = this.transform;
                    }
                }
            }
        }

        #endregion

        #region IMmResponder Routing Table Management Methods

        /// <summary>
        /// Add an MmResponder to the MmRoutingTable, with level designation.
        /// </summary>
        /// <param name="mmResponder">MmResponder to be added.</param>
        /// <param name="level">Level designation of responder.</param>
        /// <returns>Reference to new MmRoutingTable item.</returns>
        public virtual MmRoutingTableItem MmAddToRoutingTable(MmResponder mmResponder, MmLevelFilter level)
        {
			var routingTableItem = new MmRoutingTableItem (mmResponder.name, mmResponder) {
				Level = level,
				Tags = mmResponder.Tag  // Copy tag from responder to routing table item
			};

            if (RoutingTable.Contains(mmResponder))
                return null; // Already in list

            //If there is an MmInvoke executing, add it to the
            //  MmRespondersToAdd queue.
            if (doNotModifyRoutingTable)
            {
                MmRespondersToAdd.Enqueue(routingTableItem);
            }
            else
            {
				RoutingTable.Add(routingTableItem);
            }

            return routingTableItem;
        }

        /// <summary>
        /// Add an MmResponder to the MmRoutingTable, with level designation.
        /// </summary>
        /// <param name="mmResponder">MmResponder to be added.</param>
        /// <param name="newName">Name of MmResponder as it will appear in list.</param>
        /// <returns>Reference to new MmRoutingTable item.</returns>
        public virtual MmRoutingTableItem MmAddToRoutingTable(MmResponder mmResponder, string newName)
        {
            var level = (mmResponder.gameObject == gameObject)
                ? MmLevelFilter.Self
                : MmLevelFilter.Child;

            var routingTableItem = MmAddToRoutingTable(mmResponder, level);

            if (mmResponder is MmRelayNode)
                (mmResponder as MmRelayNode).AddParent(this);

            return routingTableItem;
        }

        /// <summary>
        /// Grab all MmResponders attached to the same GameObject.
        /// Does not grab any other MmRelayNodes attached to the same GameObject.
        /// </summary>
        public void MmRefreshResponders()
        {
            // Get all MmResponders and filter out MmRelayNodes (removed LINQ for performance)
            var components = GetComponents<MmResponder>();
            List<MmResponder> responders = new List<MmResponder>(components.Length);
            foreach (var component in components)
            {
                if (!(component is MmRelayNode))
                {
                    responders.Add(component);
                }
            }

            // Add own implementations of IMmResponder to priority list
            // Also update Tags for existing responders (in case tags changed)
            foreach (var responder in responders)
            {
				if (!RoutingTable.Contains(responder))
				{
                	MmAddToRoutingTable(responder, MmLevelFilter.Self);
				}
				else
				{
					// Update Tags for existing routing table item
					var existingItem = RoutingTable[responder];
					if (existingItem != null)
					{
						existingItem.Tags = responder.Tag;
					}
				}
            }

            for (int i = RoutingTable.Count - 1; i >= 0; i--)
            {
                if (RoutingTable[i].Responder == null)
                    RoutingTable.RemoveAt(i);
            }
        }

        /// <summary>
        /// Iterates through RoutingTable list and assigns
        /// this MmRelayNode as a parent to child MmResponders.
        /// </summary>
        public void RefreshParents()
        {
			MmLogger.LogFramework("Refreshing parents on MmRelayNode: " + gameObject.name);

            // Iterate through routing table and process child nodes (removed LINQ for performance)
            foreach (var child in RoutingTable)
            {
                if (child.Level == MmLevelFilter.Child)
                {
                    var childNode = child.Responder.GetRelayNode();
                    childNode.AddParent(this);
                    childNode.RefreshParents();
                }
            }

            // Update parent entries in routing table (removed LINQ for performance)
            foreach (var parent in MmParentList)
            {
				// Find parent in routing table (manual search avoids LINQ allocation)
				MmRoutingTableItem foundItem = null;
				foreach (var item in RoutingTable)
				{
					if (item.Responder == parent)
					{
						foundItem = item;
						break;
					}
				}

				if (foundItem == null) {
					MmAddToRoutingTable (parent, MmLevelFilter.Parent);
				}
				else
				{
					foundItem.Level = MmLevelFilter.Parent;
				}
            }
		}

        /// <summary>
        /// Given a GameObject, extract an MmRelayNode if present
        /// and add it to this MmRelayNode's Responder list.
        /// </summary>
        /// <param name="go">GameObject that should have an MmRelayNode attached.</param>
        public virtual void MmAddNodeToList(GameObject go)
        {
			var relayNode = go.GetComponent<MmRelayNode>();

            if (relayNode != null)
                MmAddToRoutingTable(relayNode, go.name);
            else
                MmLogger.LogError("No MmRelayNode present on " + go.name);
        }

        /// <summary>
        /// Given an MmRelayNode, add it as parent to this 
        /// instance's MmRoutingTable.
        /// </summary>
        /// <param name="parent">MmRelayNode to add as a parent.</param>
		public virtual void AddParent(MmRelayNode parent)
		{
			if (!MmParentList.Contains (parent)) {
				MmParentList.Add (parent);
				MmAddToRoutingTable (parent, MmLevelFilter.Parent);
			}
		}

        #endregion

        #region MmInvoke methods

        /// <summary>
        /// Invoke an MmMethod. 
        /// </summary>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.
        /// </param>
        /// <param name="message">The message to send.
        /// This class builds on UNET's MessageBase so it is
        /// Auto [de]serialized by UNET.</param>
        public override void MmInvoke(MmMessage message)
        {
            // Debug tracking - disabled in PerformanceMode
            if (!PerformanceMode)
            {
                messageBuffer.Add(message);
                // Prevent unbounded growth - clear buffer periodically
                if (messageBuffer.Count > 1000)
                {
                    messageBuffer.Clear();
                }
                UpdateMessages(message);
            }

            MmMessageType msgType = message.MmMessageType;
            //If the MmRelayNode has not been initialized, initialize it here,
            //  and refresh the parents - to ensure proper routing can occur.
            InitializeNode();

            // Check hop limit to prevent infinite loops
            if (maxMessageHops > 0)
            {
                if (message.HopCount >= maxMessageHops)
                {
                    MmLogger.LogFramework($"[HOP LIMIT] Message dropped at '{name}' after {message.HopCount} hops (max: {maxMessageHops}). Method: {message.MmMethod}");
                    return;
                }

                // Increment hop counter for this relay
                message.HopCount++;
            }

            // Check for cycles if cycle detection is enabled
            if (enableCycleDetection)
            {
                int nodeInstanceId = gameObject.GetInstanceID();

                // Initialize visited nodes set if not already done
                if (message.VisitedNodes == null)
                {
                    message.VisitedNodes = new System.Collections.Generic.HashSet<int>();
                }

                // Check if we've already visited this node (circular path detected)
                if (message.VisitedNodes.Contains(nodeInstanceId))
                {
                    MmLogger.LogFramework($"[CYCLE DETECTED] Message dropped at '{name}' - circular path detected. Method: {message.MmMethod}, Hops: {message.HopCount}");
                    return;
                }

                // Mark this node as visited
                message.VisitedNodes.Add(nodeInstanceId);
            }

            doNotModifyRoutingTable = true;
            MmNetworkFilter networkFilter = message.MetadataBlock.NetworkFilter;
            
            //	If an MmNetworkResponder is attached to this object, and the MmMessage has not already been deserialized
            //	then call the MmNetworkResponder's network message invocation function.
            if (MmNetworkResponder != null &&
                message.MetadataBlock.NetworkFilter != MmNetworkFilter.Local &&
                !message.IsDeserialized)
            {
                //if (!dirty)
                //{
                //    dirty = true;
                //    message.TimeStamp = DateTime.UtcNow.ToShortTimeString();
                //    _prevMessageTime = message.TimeStamp;
                //}
                
                //This will ensure that beyond the point at which a message is determined to be sendable,
                //  it will not be treated as networ
                networkFilter = NetworkFilterAdjust(ref message);

                MmNetworkResponder.MmInvoke (message);
			}

            
            //Lazy message copying: Only create copies if actually needed for multiple directions
            //This reduces 20-30% overhead from unnecessary message allocations

            MmLevelFilter levelFilter = message.MetadataBlock.LevelFilter;
            MmActiveFilter activeFilter = ActiveFilterAdjust(ref message);
            MmSelectedFilter selectedFilter = SelectedFilterAdjust(ref message);

            //If this message was a network-only message and
            //  this node does not allow for propagation of network messages,
            //  then return.
            if (!AllowNetworkPropagationLocally && !message.IsDeserialized &&
                 message.MetadataBlock.NetworkFilter == MmNetworkFilter.Network)
            {
                return;
            }

            // First pass: determine which directions are needed
            bool needsParent = false;
            bool needsChild = false;
            bool needsSelf = false;

            foreach (var routingTableItem in RoutingTable)
            {
                MmLevelFilter responderLevel = routingTableItem.Level;

                if ((responderLevel & MmLevelFilter.Parent) > 0)
                    needsParent = true;
                else if ((responderLevel & MmLevelFilter.Child) > 0)
                    needsChild = true;
                else
                    needsSelf = true;
            }

            // Create messages lazily based on what's needed
            MmMessage upwardMessage = null;
            MmMessage downwardMessage = null;

            // If we need multiple directions, we need to copy
            int directionsNeeded = (needsParent ? 1 : 0) + (needsChild ? 1 : 0) + (needsSelf ? 1 : 0);

            if (directionsNeeded > 1)
            {
                // Need copies for multiple directions
                if (needsParent)
                {
                    upwardMessage = message.Copy();
                    upwardMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndParents;
                }

                if (needsChild)
                {
                    downwardMessage = message.Copy();
                    downwardMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
                }

                // needsSelf uses original message
            }
            else if (directionsNeeded == 1)
            {
                // Only one direction needed - reuse original message
                if (needsParent)
                {
                    message.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndParents;
                    upwardMessage = message;
                }
                else if (needsChild)
                {
                    message.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
                    downwardMessage = message;
                }
                // If needsSelf, message already has correct filter
            }

            // Second pass: invoke responders with appropriate messages
            foreach (var routingTableItem in RoutingTable) {
				var responder = routingTableItem.Responder;
				MmLevelFilter responderLevel = routingTableItem.Level;

				//Check individual responder level and then call the right param.
				MmMessage responderSpecificMessage;
				if((responderLevel & MmLevelFilter.Parent) > 0)
				{
					responderSpecificMessage = upwardMessage;
				}
				else if((responderLevel & MmLevelFilter.Child) > 0)
				{
					responderSpecificMessage = downwardMessage;
				}
				else
				{
					responderSpecificMessage = message;
				}

				//MmLogger.LogFramework (gameObject.name + "observing " + responder.MmGameObject.name);

                bool checkPassed = ResponderCheck (levelFilter, activeFilter, selectedFilter, networkFilter,
                    routingTableItem, responderSpecificMessage);

                if (checkPassed) {
					responder.MmInvoke (responderSpecificMessage);
				}
			}

            // Phase 2.1: Advanced Routing - Handle new level filters
            HandleAdvancedRouting(message, levelFilter);

			//if (dirty && _prevMessageTime == message.TimeStamp)
			//{
			//    dirty = false;
			//}

            doNotModifyRoutingTable = false;

            // Process queued responders (replaced Any() with Count for performance)
            while (MmRespondersToAdd.Count > 0)
            {
                var routingTableItem = MmRespondersToAdd.Dequeue();

                MmAddToRoutingTable(routingTableItem.Responder, routingTableItem.Level);

                if (ResponderCheck(levelFilter, activeFilter, selectedFilter, networkFilter,
                    routingTableItem, message))
                {
                    routingTableItem.Responder.MmInvoke(message);
                }
            }

            //if (serialExecution)
            //{
            //    if (SerialExecutionQueue.Count != 0)
            //    {
            //        MmLogger.LogFramework("%%%%%%%%%%%Dequeueing%%%%%%%%%");
            //        KeyValuePair<MmMessageType, MmMessage> DequeuedMessage = SerialExecutionQueue.Dequeue();
            //        MmInvoke(DequeuedMessage.Key, DequeuedMessage.Value);
            //    }

            //    _executing = false;
            //}
        }

        #region MmInvoke utility methods

        /// <summary>
        /// Invoke an MmMethod with no parameter. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod,
			MmMetadataBlock metadataBlock = null)
        {
			MmMessage msg = new MmMessage (mmMethod, MmMessageType.MmVoid, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
        }

        /// Invoke a general MmMethod with parameter: MmMessage. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: MmMessage. <see cref="MmMessage"/> </param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod,
            MmMessage param,
            MmMessageType msgType,
            MmMetadataBlock metadataBlock = null)
        {
            MmMessage msg = param.Copy();
            msg.MmMethod = mmMethod;
            msg.MetadataBlock = metadataBlock;
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
        }

        /// <summary>
        /// Invoke an MmMethod with parameter: bool. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: bool.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
            bool param,
            MmMetadataBlock metadataBlock = null)
        {
			MmMessage msg = new MmMessageBool (param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
        }

        /// <summary>
        /// Invoke an MmMethod with parameter: int. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: int.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
            int param,
            MmMetadataBlock metadataBlock = null)
        {
			MmMessage msg = new MmMessageInt(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
        }

        /// <summary>
        /// Invoke an MmMethod with parameter: float. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: float.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
            float param,
            MmMetadataBlock metadataBlock = null)
        {
			MmMessage msg = new MmMessageFloat(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
        }

        /// <summary>
        /// Invoke an MmMethod with parameter: Vector3.  
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: Vector3.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
			Vector3 param,
            MmMetadataBlock metadataBlock = null)
        {
			MmMessage msg = new MmMessageVector3(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
		}

        /// <summary>
        /// Invoke an MmMethod with parameter: Vector4.  
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: Vector4.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
			Vector4 param,
            MmMetadataBlock metadataBlock = null)
        {
			MmMessage msg = new MmMessageVector4(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
		}

        /// <summary>
        /// Invoke an MmMethod with parameter: string. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: string.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
			string param,
            MmMetadataBlock metadataBlock = null)
        {
			MmMessage msg = new MmMessageString(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
		}

        /// <summary>
        /// Invoke an MmMethod with parameter: byte array.  
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: byte array.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
			byte[] param,
            MmMetadataBlock metadataBlock = null)
        {
			MmMessage msg = new MmMessageByteArray(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
		}

        /// <summary>
        /// Invoke an MmMethod with parameter: MmTransform. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: MmTransform. <see cref="MmTransform"/></param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
			MmTransform param,
			MmMetadataBlock metadataBlock = null)
		{
			MmMessage msg = new MmMessageTransform(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
			MmInvoke(msg);
		}

        /// <summary>
        /// Invoke an MmMethod with parameter: List<MmTransform>. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: List<MmTransform>.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod, 
			List<MmTransform> param,
			MmMetadataBlock metadataBlock = null)
		{
			MmMessage msg = new MmMessageTransformList(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
			MmInvoke(msg);
		}

        /// <summary>
        /// Invoke an MmMethod with parameter: IMmSerializable. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: IMmSerializable. <see cref="IMmSerializable"/> </param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod,
            IMmSerializable param,
            MmMetadataBlock metadataBlock = null)
        {
            MmMessage msg = new MmMessageSerializable(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
        }

        /// Invoke an MmMethod with parameter: GameObject. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: GameObject. <see cref="MmMessage"/> </param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod,
            GameObject param,
            MmMetadataBlock metadataBlock = null)
        {
            MmMessage msg = new MmMessageGameObject(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
        }

        /// <summary>
        /// Invoke an MmMethod with parameter: Quaternion.  
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: Quaternion.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        public virtual void MmInvoke(MmMethod mmMethod,
            Quaternion param,
            MmMetadataBlock metadataBlock = null)
        {
            MmMessage msg = new MmMessageQuaternion(param, mmMethod, metadataBlock);
            messageBuffer.Add(msg);
            // UpdateMessages(msg);
            MmInvoke(msg);
        }

        /// <summary>
        /// Invokes a message using path specification for hierarchical routing.
        /// Path format: "parent/sibling/child" with wildcard support (*).
        /// Part of Phase 2.1: Advanced Message Routing.
        /// </summary>
        /// <param name="path">Path string like "parent/sibling/child"</param>
        /// <param name="mmMethod">Method to invoke on target nodes</param>
        /// <param name="metadataBlock">Optional metadata (levelFilter will be overridden)</param>
        public virtual void MmInvokeWithPath(string path, MmMethod mmMethod, MmMetadataBlock metadataBlock = null)
        {
            // Resolve path to target nodes
            List<MmRelayNode> targetNodes = ResolvePathTargets(path);

            // Create metadata if not provided
            if (metadataBlock == null)
            {
                // Use ActiveFilter.All for path-based routing
                // (Path resolution already found exact targets, active state shouldn't block delivery)
                metadataBlock = new MmMetadataBlock(
                    MmTagHelper.Everything, // Tag comes FIRST when using tags!
                    MmLevelFilter.Self,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.All
                );
            }

            // Create message
            MmMessage message = new MmMessage(mmMethod, MmMessageType.MmVoid, metadataBlock);

            // Forward to each target with transformed level filter
            foreach (var targetNode in targetNodes)
            {
                if (targetNode != null)
                {
                    // Transform level filter to Self to prevent re-propagation
                    // (Path resolution already found exact targets)
                    var forwardedMessage = message.Copy();
                    forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
                    targetNode.MmInvoke(forwardedMessage);
                }
            }
        }

        /// <summary>
        /// Invokes a message with boolean parameter using path specification.
        /// </summary>
        public virtual void MmInvokeWithPath(string path, MmMethod mmMethod, bool param, MmMetadataBlock metadataBlock = null)
        {
            List<MmRelayNode> targetNodes = ResolvePathTargets(path);
            if (metadataBlock == null)
            {
                metadataBlock = new MmMetadataBlock(
                    MmTagHelper.Everything, // Tag comes FIRST when using tags!
                    MmLevelFilter.Self,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.All
                );
            }

            MmMessage message = new MmMessageBool(param, mmMethod, metadataBlock);

            foreach (var targetNode in targetNodes)
            {
                if (targetNode != null)
                {
                    var forwardedMessage = message.Copy();
                    forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
                    targetNode.MmInvoke(forwardedMessage);
                }
            }
        }

        /// <summary>
        /// Invokes a message with int parameter using path specification.
        /// </summary>
        public virtual void MmInvokeWithPath(string path, MmMethod mmMethod, int param, MmMetadataBlock metadataBlock = null)
        {
            List<MmRelayNode> targetNodes = ResolvePathTargets(path);
            if (metadataBlock == null)
            {
                metadataBlock = new MmMetadataBlock(
                    MmTagHelper.Everything, // Tag comes FIRST when using tags!
                    MmLevelFilter.Self,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.All
                );
            }

            MmMessage message = new MmMessageInt(param, mmMethod, metadataBlock);

            foreach (var targetNode in targetNodes)
            {
                if (targetNode != null)
                {
                    var forwardedMessage = message.Copy();
                    forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
                    targetNode.MmInvoke(forwardedMessage);
                }
            }
        }

        /// <summary>
        /// Invokes a message with string parameter using path specification.
        /// </summary>
        public virtual void MmInvokeWithPath(string path, MmMethod mmMethod, string param, MmMetadataBlock metadataBlock = null)
        {
            List<MmRelayNode> targetNodes = ResolvePathTargets(path);
            if (metadataBlock == null)
            {
                metadataBlock = new MmMetadataBlock(
                    MmTagHelper.Everything, // Tag comes FIRST when using tags!
                    MmLevelFilter.Self,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.All
                );
            }

            MmMessage message = new MmMessageString(param, mmMethod, metadataBlock);

            foreach (var targetNode in targetNodes)
            {
                if (targetNode != null)
                {
                    var forwardedMessage = message.Copy();
                    forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
                    targetNode.MmInvoke(forwardedMessage);
                }
            }
        }

        /// <summary>
        /// Invokes a pre-created message using path specification.
        /// </summary>
        public virtual void MmInvokeWithPath(string path, MmMessage message)
        {
            List<MmRelayNode> targetNodes = ResolvePathTargets(path);

            foreach (var targetNode in targetNodes)
            {
                if (targetNode != null)
                {
                    var forwardedMessage = message.Copy();
                    forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
                    // Use ActiveFilter.All for path-based routing (active state shouldn't block delivery)
                    forwardedMessage.MetadataBlock.ActiveFilter = MmActiveFilter.All;
                    targetNode.MmInvoke(forwardedMessage);
                }
            }
        }

        #endregion

        #endregion

        #region Support Methods 

        /// <summary>
        /// If the level filter is designated 'Child', then it is recorded locally, 
        /// but converted to a 'Child+Self' for use by the RoutingTable 
        /// (which need to pass the message on to all children, but still need to be able
        /// to execute the message on their own responders, otherwise, it just goes
        /// to the terminal points of the graph without ever executing).
        /// </summary>
        /// <param name="message">MmMessage to be adjusted.</param>
        /// <param name="direction">Intended direction of message</param>
        /// <returns>Base implementation returns messages's level filter.</returns>
        protected virtual MmLevelFilter LevelFilterAdjust(ref MmMessage message, 
            MmLevelFilter direction)
		{
            return message.MetadataBlock.LevelFilter;
        }

        /// <summary>
        /// Allows modification of active filter in message
        /// as it gets passed between MmRelayNodes.
        /// </summary>
        /// <param name="message">MmMessage to be adjusted.</param>
        /// <returns>Base implementation returns messages's active filter.</returns>
		protected virtual MmActiveFilter ActiveFilterAdjust(ref MmMessage message)
		{
			return message.MetadataBlock.ActiveFilter;
		}

        /// <summary>
        /// Allows modification of selected filter in message
        /// as it gets passed between MmRelayNodes.
        /// </summary>
        /// <param name="message">MmMessage to be adjusted.</param>
        /// <returns>Base implementation returns messages's selected filter.</returns>
		protected virtual MmSelectedFilter SelectedFilterAdjust(ref MmMessage message)
		{
			return message.MetadataBlock.SelectedFilter;
		}

        /// <summary>
        /// Allows modification of network filter in message
        /// as it gets passed between MmRelayNodes.
        /// </summary>
        /// <param name="message">MmMessage to be adjusted.</param>
        /// <returns>Base implementation returns messages's network filter.</returns>
        protected virtual MmNetworkFilter NetworkFilterAdjust(ref MmMessage message)
        {
            MmNetworkFilter original = message.MetadataBlock.NetworkFilter;

            if (FlipNetworkFlagOnSend &&
                message.MetadataBlock.NetworkFilter != MmNetworkFilter.Local)
            {
                message.MetadataBlock.NetworkFilter = MmNetworkFilter.Local;
            }

            return original;
        }

        /// <summary>
        /// This method determines if a particular MmResponder should 
        /// receive a message via MmInvoke.
        /// This performs 4 checks: Tag Check, Level Check, Active Check, & Selected Check.
        /// </summary>
        /// <param name="levelFilter">Extracted message level filter - before adjust.</param>
        /// <param name="activeFilter">Extracted message active filter - before adjust.</param>
        /// <param name="selectedFilter">Extracted message selected filter - before adjust.</param>
        /// <param name="networkFilter">Extracted message network filter - before adjust.</param>
        /// <param name="mmRoutingTableItem">RoutingTableItem of currently observed MmResponder</param>
        /// <param name="message">MmMessage to be checked.</param>
        /// <returns>Returns whether responder has passed all checks.</returns>
        protected virtual bool ResponderCheck(MmLevelFilter levelFilter, MmActiveFilter activeFilter,
			MmSelectedFilter selectedFilter, MmNetworkFilter networkFilter, 
            MmRoutingTableItem mmRoutingTableItem, MmMessage message)
		{
		    if (!TagCheck(mmRoutingTableItem, message)) return false; // Failed TagCheck

		    return LevelCheck (levelFilter, mmRoutingTableItem.Responder, mmRoutingTableItem.Level)
				&& ActiveCheck (activeFilter, mmRoutingTableItem.Responder)
				&& SelectedCheck(selectedFilter, mmRoutingTableItem.Responder)
                && NetworkCheck(mmRoutingTableItem, message);
		}

        /// <summary>
        /// Determine if MmResponder passes MmRelayNode tag filter check using value embedded in MmMessage.
        /// </summary>
        /// <param name="mmRoutingTableItem">RoutingTableItem of currently observed MmResponder</param>
        /// <param name="message">MmMessage to be checked.</param>
        /// <returns>Returns whether observed MmResponder has passed tag check.</returns>
        protected virtual bool TagCheck(MmRoutingTableItem mmRoutingTableItem, MmMessage message)
        {
            //var text = string.Format("Tag Check (GO: {0}, ListItem: {1}, msgType:{2}, msgTag={3}, responderTag={4}",
            //    gameObject.name,
            //    mmRoutingTableItem.Name,
            //    param.MmMethod,
            //    MmTagHelper.ToString(param.MetadataBlock.Tag),
            //    MmTagHelper.ToString(mmRoutingTableItem.Tags));

            // Responder's TagCheck toggle is not enabled, this passes
            if (!mmRoutingTableItem.Responder.TagCheckEnabled)
            {
                //Debug.Log(text + ") Passed -- TagCheckEnabled: FALSE");
                return true;
            }

            var msgTag = message.MetadataBlock.Tag;   // This is "Everything" by default, will by-pass Tag-Check
            var responderTag = mmRoutingTableItem.Tags; // This is "Nothing" by default, if a message *has* a specific tag,
                                                   // i.e. something other than "Everything", it won't pass
                                                   // unless it has that tag's flag set to 1

            // This message applies to everyone, this passes
            if (msgTag == MmTagHelper.Everything)
            {
                //Debug.Log(text + ") Passed -- msgTag = Everything");
                return true;
            }

            // This message has a tag, other than "Everything", but it matches responder's tag, so it passes
            if ((msgTag & responderTag) > 0)
            {
                //Debug.Log(text + ") Passed -- tag match");
                return true;
            }

            // This message has a tag, other than "Everything", and it doesn't match responder's tag, so it fails
            //Debug.Log(text + ") FAILED");
            return false;
        }

        /// <summary>
        /// Determine if MmResponder passes MmRelayNode level filter check using value embedded in MmMessage.
        /// </summary>
        /// <param name="levelFilter">Level filter value extracted from MmMessage.</param>
        /// <param name="responder">Observed MmResponder.</param>
        /// <param name="responderLevel">Observed MmResponder Level.</param>
        /// <returns>Returns whether observed MmResponder has passed level filter check.</returns>
        protected virtual bool LevelCheck(MmLevelFilter levelFilter, 
            IMmResponder responder, MmLevelFilter responderLevel)
		{
			return (levelFilter & responderLevel) > 0;
		}

        /// <summary>
        /// Determine if MmResponder passes MmRelayNode active filter check using value embedded in MmMessage.
        /// </summary>
        /// <param name="activeFilter">Active filter value extracted from MmMessage.</param>
        /// <param name="responder">Observed MmResponder</param>
        /// <returns>Returns whether observed MmResponder has passed active filter check.</returns>
		protected virtual bool ActiveCheck(MmActiveFilter activeFilter, 
            IMmResponder responder)
		{
			return ((activeFilter == MmActiveFilter.All)
				|| (activeFilter == MmActiveFilter.Active && responder.MmGameObject.activeInHierarchy));
		}

        /// <summary>
        /// Determine if MmResponder passes MmRelayNode selected filter check using value embedded in MmMessage.
        /// </summary>
        /// <param name="selectedFilter">Selected filter value extracted from MmMessage.</param>
        /// <param name="responder">Observed MmResponder</param>
        /// <returns>Returns whether observed MmResponder has passed selected filter check.
        /// In base MmRelayNode, this always returns true.</returns>
		protected virtual bool SelectedCheck(MmSelectedFilter selectedFilter, 
            IMmResponder responder)
		{
			return true;
		}

        /// <summary>
        /// Checks if a responder should recieve a message based on 
        /// the network flag in a control block.
        /// Network messages can go to other nodes, but not to self responders.
        /// </summary>
        /// <param name="mmRoutingTableItem">Observed MmResponder</param>
        /// <param name="message">Input message.</param>
        /// <returns></returns>
        protected virtual bool NetworkCheck(MmRoutingTableItem mmRoutingTableItem, MmMessage message)
        {
            //Need to check if the message is executing locally on a host (server + client).
            //If this occurs, two instances of a message will be seen in this node:
            //  1. The message originally passed in, that has just been sent over the network
            //  2. The message handled by the network and received by the client instance.
            //The first instance of a message should not execute locally. Instead, only the second instance
            //  should. The Node needs to know if this is executing locally as a host, and if the 
            //  message has been deserialized (indicating receipt by the client instance).

            if ((mmRoutingTableItem.Level == MmLevelFilter.Self &&
                message.MetadataBlock.NetworkFilter == MmNetworkFilter.Network &&
                !message.IsDeserialized) ||
                (message.MetadataBlock.NetworkFilter != MmNetworkFilter.Local && 
                 MmNetworkResponder != null &&
                 MmNetworkResponder.OnClient && 
                 MmNetworkResponder.OnServer && 
                 !message.IsDeserialized))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Implementation of IMmResponder's GetRelayNode, which returns this.
        /// </summary>
        /// <returns>Returns this MmRelayNode.</returns>
		public override MmRelayNode GetRelayNode()
		{
			return this;
		}

		#endregion

        #region Phase 2.1: Advanced Routing

        /// <summary>
        /// Handles advanced routing for Phase 2.1 level filters.
        /// Processes Siblings, Cousins, Descendants, Ancestors, and Custom filters.
        /// </summary>
        /// <param name="message">Message to route</param>
        /// <param name="levelFilter">Level filter from message</param>
        protected virtual void HandleAdvancedRouting(MmMessage message, MmLevelFilter levelFilter)
        {
            // Check if any advanced filters are present
            bool hasSiblings = (levelFilter & MmLevelFilter.Siblings) != 0;
            bool hasCousins = (levelFilter & MmLevelFilter.Cousins) != 0;
            bool hasDescendants = (levelFilter & MmLevelFilter.Descendants) != 0;
            bool hasAncestors = (levelFilter & MmLevelFilter.Ancestors) != 0;
            bool hasCustom = (levelFilter & MmLevelFilter.Custom) != 0;

            // If no advanced filters, skip (BEFORE profiling for zero overhead)
            if (!hasSiblings && !hasCousins && !hasDescendants && !hasAncestors && !hasCustom)
                return;

            // Validate MmRoutingOptions if needed
            MmRoutingOptions options = message.MetadataBlock.Options;

            // START PROFILING (only for slow path with advanced filters)
            Stopwatch profiler = null;
            int siblingCount = 0, cousinCount = 0, descendantCount = 0, ancestorCount = 0, customCount = 0;

            bool enableProfiling = (options != null && options.EnableProfiling) || EnableRoutingProfiler;
            if (enableProfiling)
            {
                profiler = Stopwatch.StartNew();
            }

            // Validate lateral routing (siblings/cousins) requires AllowLateralRouting
            if ((hasSiblings || hasCousins) && (options == null || !options.AllowLateralRouting))
            {
                MmLogger.LogFramework($"[ROUTING] Lateral routing (Siblings/Cousins) requires MmRoutingOptions.AllowLateralRouting = true at '{name}'");
                return;
            }

            // Validate custom filter requires CustomFilter predicate
            if (hasCustom && (options == null || options.CustomFilter == null))
            {
                MmLogger.LogFramework($"[ROUTING] Custom filter requires MmRoutingOptions.CustomFilter predicate at '{name}'");
                return;
            }

            // Handle lateral routing (siblings/cousins) with counting
            if (hasSiblings || hasCousins)
            {
                if (enableProfiling)
                {
                    // Count nodes for profiling
                    List<MmRelayNode> tempNodes = new List<MmRelayNode>();
                    if (hasSiblings)
                    {
                        CollectSiblings(tempNodes);
                        siblingCount = tempNodes.Count;
                    }
                    if (hasCousins)
                    {
                        List<MmRelayNode> cousins = new List<MmRelayNode>();
                        CollectCousins(cousins);
                        cousinCount = cousins.Count;
                    }
                }
                RouteLateral(message, hasSiblings, hasCousins);
            }

            // Handle recursive descendants with counting
            if (hasDescendants)
            {
                if (enableProfiling)
                {
                    List<MmRelayNode> tempDescendants = new List<MmRelayNode>();
                    CollectDescendants(tempDescendants);
                    descendantCount = tempDescendants.Count;
                }
                RouteRecursive(message, useDescendants: true);
            }

            // Handle recursive ancestors with counting
            if (hasAncestors)
            {
                if (enableProfiling)
                {
                    List<MmRelayNode> tempAncestors = new List<MmRelayNode>();
                    CollectAncestors(tempAncestors);
                    ancestorCount = tempAncestors.Count;
                }
                RouteRecursive(message, useDescendants: false);
            }

            // Handle custom predicate filtering with counting
            if (hasCustom && options != null && options.CustomFilter != null)
            {
                var filteredNodes = ApplyCustomFilter(options.CustomFilter);
                if (enableProfiling)
                {
                    customCount = filteredNodes.Count;
                }
                foreach (var node in filteredNodes)
                {
                    if (node != null)
                    {
                        // Transform level filter to Self only (custom filter explicitly selected targets)
                        var forwardedMessage = message.Copy();
                        forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
                        node.MmInvoke(forwardedMessage);
                    }
                }
            }

            // END PROFILING
            if (profiler != null)
            {
                profiler.Stop();
                double elapsedMs = profiler.Elapsed.TotalMilliseconds;

                float threshold = (options != null && options.EnableProfiling)
                    ? options.ProfilingThresholdMs
                    : ProfilingThresholdMs;

                if (elapsedMs >= threshold)
                {
                    // Build filter list for log
                    List<string> filters = new List<string>();
                    if (hasSiblings) filters.Add("Siblings");
                    if (hasCousins) filters.Add("Cousins");
                    if (hasDescendants) filters.Add("Descendants");
                    if (hasAncestors) filters.Add("Ancestors");
                    if (hasCustom) filters.Add("Custom");

                    int totalNodes = siblingCount + cousinCount + descendantCount + ancestorCount + customCount;

                    MmLogger.LogFramework(
                        $"[ROUTING-PERF] HandleAdvancedRouting | Node='{gameObject.name}' | " +
                        $"Filters={string.Join("|", filters)}, " +
                        $"Siblings={siblingCount}, Cousins={cousinCount}, " +
                        $"Descendants={descendantCount}, Ancestors={ancestorCount}, " +
                        $"Custom={customCount}, Total={totalNodes} | " +
                        $"Time={elapsedMs:F3}ms"
                    );
                }
            }
        }

        /// <summary>
        /// Collects sibling relay nodes (nodes sharing the same parent).
        /// Used for lateral routing (MmLevelFilter.Siblings).
        /// </summary>
        /// <param name="siblings">Output list to populate with sibling nodes</param>
        protected virtual void CollectSiblings(List<MmRelayNode> siblings)
        {
            siblings.Clear();

            // If no parents, no siblings possible
            if (MmParentList == null || MmParentList.Count == 0)
                return;

            // Collect siblings from each parent
            foreach (var parent in MmParentList)
            {
                if (parent == null) continue;

                // Get parent's children (our siblings + self)
                foreach (var routingItem in parent.RoutingTable)
                {
                    if (routingItem.Level != MmLevelFilter.Child)
                        continue;

                    var siblingNode = routingItem.Responder.GetRelayNode();
                    if (siblingNode == null || siblingNode == this)
                        continue; // Skip self

                    if (!siblings.Contains(siblingNode))
                        siblings.Add(siblingNode);
                }
            }
        }

        /// <summary>
        /// Collects cousin relay nodes (parent's siblings' children).
        /// Used for extended family routing (MmLevelFilter.Cousins).
        /// </summary>
        /// <param name="cousins">Output list to populate with cousin nodes</param>
        protected virtual void CollectCousins(List<MmRelayNode> cousins)
        {
            cousins.Clear();

            // If no parents, no cousins possible
            if (MmParentList == null || MmParentList.Count == 0)
                return;

            // For each parent, collect their siblings' children
            foreach (var parent in MmParentList)
            {
                if (parent == null) continue;

                // Collect parent's siblings
                List<MmRelayNode> parentSiblings = new List<MmRelayNode>();
                parent.CollectSiblings(parentSiblings);

                // For each parent's sibling, collect their children (our cousins)
                foreach (var parentSibling in parentSiblings)
                {
                    if (parentSibling == null) continue;

                    foreach (var routingItem in parentSibling.RoutingTable)
                    {
                        if (routingItem.Level != MmLevelFilter.Child)
                            continue;

                        var cousinNode = routingItem.Responder.GetRelayNode();
                        if (cousinNode == null || cousinNode == this)
                            continue; // Skip self

                        if (!cousins.Contains(cousinNode))
                            cousins.Add(cousinNode);
                    }
                }
            }
        }

        /// <summary>
        /// Collects all descendant relay nodes recursively (all children, grandchildren, etc.).
        /// Used for deep tree traversal (MmLevelFilter.Descendants).
        /// </summary>
        /// <param name="descendants">Output list to populate with descendant nodes</param>
        /// <param name="visited">Set of visited nodes to prevent infinite loops</param>
        protected virtual void CollectDescendants(List<MmRelayNode> descendants, HashSet<int> visited = null)
        {
            // Initialize visited set on first call
            if (visited == null)
            {
                descendants.Clear();
                visited = new HashSet<int>();
                visited.Add(gameObject.GetInstanceID()); // Mark self as visited
            }

            // Collect direct children
            foreach (var routingItem in RoutingTable)
            {
                if (routingItem.Level != MmLevelFilter.Child)
                    continue;

                var childNode = routingItem.Responder.GetRelayNode();
                if (childNode == null) continue;

                int childId = childNode.gameObject.GetInstanceID();
                if (visited.Contains(childId))
                    continue; // Already visited (circular prevention)

                visited.Add(childId);
                descendants.Add(childNode);

                // Recursively collect grandchildren
                childNode.CollectDescendants(descendants, visited);
            }
        }

        /// <summary>
        /// Collects all ancestor relay nodes recursively (all parents, grandparents, etc.).
        /// Used for upward tree traversal (MmLevelFilter.Ancestors).
        /// </summary>
        /// <param name="ancestors">Output list to populate with ancestor nodes</param>
        /// <param name="visited">Set of visited nodes to prevent infinite loops</param>
        protected virtual void CollectAncestors(List<MmRelayNode> ancestors, HashSet<int> visited = null)
        {
            // Initialize visited set on first call
            if (visited == null)
            {
                ancestors.Clear();
                visited = new HashSet<int>();
                visited.Add(gameObject.GetInstanceID()); // Mark self as visited
            }

            // If no parents, we're done
            if (MmParentList == null || MmParentList.Count == 0)
                return;

            // Collect direct parents
            foreach (var parent in MmParentList)
            {
                if (parent == null) continue;

                int parentId = parent.gameObject.GetInstanceID();
                if (visited.Contains(parentId))
                    continue; // Already visited (circular prevention)

                visited.Add(parentId);
                ancestors.Add(parent);

                // Recursively collect grandparents
                parent.CollectAncestors(ancestors, visited);
            }
        }

        /// <summary>
        /// Routes message to lateral relatives (siblings/cousins).
        /// Part of Phase 2.1: Advanced Message Routing.
        /// </summary>
        /// <param name="message">Message to route</param>
        /// <param name="includeSiblings">Include sibling nodes</param>
        /// <param name="includeCousins">Include cousin nodes</param>
        protected virtual void RouteLateral(MmMessage message, bool includeSiblings, bool includeCousins)
        {
            List<MmRelayNode> lateralNodes = new List<MmRelayNode>();

            if (includeSiblings)
                CollectSiblings(lateralNodes);

            if (includeCousins)
            {
                List<MmRelayNode> cousins = new List<MmRelayNode>();
                CollectCousins(cousins);
                lateralNodes.AddRange(cousins);
            }

            // Route message to each lateral node with transformed filter
            foreach (var node in lateralNodes)
            {
                if (node != null)
                {
                    // Transform level filter so target can process its Self responders
                    var forwardedMessage = message.Copy();
                    forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
                    node.MmInvoke(forwardedMessage);
                }
            }
        }

        /// <summary>
        /// Routes message to recursive descendants or ancestors.
        /// Part of Phase 2.1: Advanced Message Routing.
        /// </summary>
        /// <param name="message">Message to route</param>
        /// <param name="useDescendants">True for descendants, false for ancestors</param>
        protected virtual void RouteRecursive(MmMessage message, bool useDescendants)
        {
            List<MmRelayNode> nodes = new List<MmRelayNode>();

            if (useDescendants)
                CollectDescendants(nodes);
            else
                CollectAncestors(nodes);

            // Route message to each node with transformed filter
            foreach (var node in nodes)
            {
                if (node != null)
                {
                    // Transform level filter to Self only to prevent re-propagation
                    // (CollectDescendants/Ancestors already found ALL targets recursively)
                    var forwardedMessage = message.Copy();
                    forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
                    node.MmInvoke(forwardedMessage);
                }
            }
        }

        /// <summary>
        /// Applies custom predicate filtering to routing table items.
        /// Part of Phase 2.1: Advanced Message Routing.
        /// </summary>
        /// <param name="customFilter">Predicate function to apply</param>
        /// <returns>Filtered list of relay nodes</returns>
        protected virtual List<MmRelayNode> ApplyCustomFilter(System.Func<MmRelayNode, bool> customFilter)
        {
            List<MmRelayNode> filteredNodes = new List<MmRelayNode>();

            if (customFilter == null)
                return filteredNodes;

            // Apply predicate to all nodes in routing table
            foreach (var routingItem in RoutingTable)
            {
                var node = routingItem.Responder.GetRelayNode();
                if (node != null && customFilter(node))
                {
                    filteredNodes.Add(node);
                }
            }

            return filteredNodes;
        }

        #region Phase 2.1: Path Specification

        /// <summary>
        /// Resolves a path specification string to a list of target relay nodes.
        /// Path format: "parent/sibling/child" with wildcard support (*).
        /// Part of Phase 2.1: Advanced Message Routing.
        /// </summary>
        /// <param name="path">Path string like "parent/sibling/child"</param>
        /// <returns>List of relay nodes matching the path</returns>
        /// <exception cref="MmInvalidPathException">If path is invalid</exception>
        public virtual List<MmRelayNode> ResolvePathTargets(string path)
        {
            // Parse the path
            ParsedPath parsedPath = MmPathSpecification.Parse(path);

            // START PROFILING
            Stopwatch profiler = null;
            int wildcardCount = 0;

            if (EnableRoutingProfiler)
            {
                profiler = Stopwatch.StartNew();
            }

            // Start with current node
            List<MmRelayNode> currentNodes = new List<MmRelayNode> { this };

            // Track visited nodes to prevent infinite loops
            HashSet<int> visited = new HashSet<int>();
            visited.Add(gameObject.GetInstanceID());

            // Navigate through each segment
            bool expandNext = false; // Wildcard flag

            for (int i = 0; i < parsedPath.Segments.Length; i++)
            {
                PathSegment segment = parsedPath.Segments[i];

                // Handle wildcard
                if (segment == PathSegment.Wildcard)
                {
                    expandNext = true;
                    if (profiler != null) wildcardCount++;
                    continue;
                }

                // If previous segment was wildcard, expand current nodes to ALL their children
                if (expandNext)
                {
                    List<MmRelayNode> expandedNodes = new List<MmRelayNode>();

                    foreach (var node in currentNodes)
                    {
                        if (node == null) continue;

                        // Get ALL children of this node
                        foreach (var routingItem in node.RoutingTable)
                        {
                            if (routingItem.Level == MmLevelFilter.Child)
                            {
                                var childNode = routingItem.Responder.GetRelayNode();
                                if (childNode != null)
                                {
                                    int childId = childNode.gameObject.GetInstanceID();
                                    if (!visited.Contains(childId))
                                    {
                                        expandedNodes.Add(childNode);
                                        visited.Add(childId);
                                    }
                                }
                            }
                        }
                    }

                    currentNodes = expandedNodes;
                    expandNext = false;
                }

                // Navigate to next set of nodes
                List<MmRelayNode> nextNodes = new List<MmRelayNode>();

                foreach (var node in currentNodes)
                {
                    if (node == null) continue;

                    // Get nodes for this segment
                    List<MmRelayNode> segmentNodes = NavigateSegment(node, segment, visited);

                    // Add to next nodes
                    foreach (var targetNode in segmentNodes)
                    {
                        if (targetNode != null && !nextNodes.Contains(targetNode))
                        {
                            nextNodes.Add(targetNode);

                            // Mark as visited
                            int nodeId = targetNode.gameObject.GetInstanceID();
                            visited.Add(nodeId);
                        }
                    }
                }

                currentNodes = nextNodes;
            }

            // END PROFILING
            if (profiler != null)
            {
                profiler.Stop();
                double elapsedMs = profiler.Elapsed.TotalMilliseconds;

                if (elapsedMs >= ProfilingThresholdMs)
                {
                    MmLogger.LogFramework(
                        $"[ROUTING-PERF] ResolvePathTargets | Node='{gameObject.name}' | " +
                        $"Path='{path}', Segments={parsedPath.Segments.Length}, " +
                        $"Wildcards={wildcardCount}, Visited={visited.Count}, " +
                        $"Targets={currentNodes.Count} | " +
                        $"Time={elapsedMs:F3}ms"
                    );
                }
            }

            return currentNodes;
        }

        /// <summary>
        /// Navigates from a node according to a path segment.
        /// </summary>
        private List<MmRelayNode> NavigateSegment(MmRelayNode node, PathSegment segment, HashSet<int> visited)
        {
            List<MmRelayNode> results = new List<MmRelayNode>();

            switch (segment)
            {
                case PathSegment.Self:
                    results.Add(node);
                    break;

                case PathSegment.Parent:
                    // Get immediate parents
                    if (node.MmParentList != null)
                    {
                        foreach (var parent in node.MmParentList)
                        {
                            if (parent != null)
                            {
                                int parentId = parent.gameObject.GetInstanceID();
                                if (!visited.Contains(parentId))
                                {
                                    results.Add(parent);
                                }
                            }
                        }
                    }
                    break;

                case PathSegment.Child:
                    // Get immediate children
                    foreach (var routingItem in node.RoutingTable)
                    {
                        if (routingItem.Level == MmLevelFilter.Child)
                        {
                            var childNode = routingItem.Responder.GetRelayNode();
                            if (childNode != null)
                            {
                                int childId = childNode.gameObject.GetInstanceID();
                                if (!visited.Contains(childId))
                                {
                                    results.Add(childNode);
                                }
                            }
                        }
                    }
                    break;

                case PathSegment.Sibling:
                    // Use existing CollectSiblings method
                    node.CollectSiblings(results);
                    // Filter out already visited
                    results.RemoveAll(n => n == null || visited.Contains(n.gameObject.GetInstanceID()));
                    break;

                case PathSegment.Ancestor:
                    // Use existing CollectAncestors method
                    node.CollectAncestors(results, visited);
                    break;

                case PathSegment.Descendant:
                    // Use existing CollectDescendants method
                    node.CollectDescendants(results, visited);
                    break;

                case PathSegment.Wildcard:
                    // This shouldn't happen (handled above), but include for completeness
                    MmLogger.LogError("Wildcard segment should not reach NavigateSegment");
                    break;
            }

            return results;
        }

        #endregion

        #endregion

        #region Static Methods

        /// <summary>
        /// Given an MmResponder, extract an MmRelayNode from it's GameObject, if one is present.
        /// </summary>
        /// <param name="iMmResponder">Observed MmResponder</param>
        /// <returns>an MmRelayNode if one is present on the same GameObject.</returns>
        public static MmRelayNode GetRelayNode(IMmResponder iMmResponder)
        {
            var relayNode = ((MonoBehaviour)iMmResponder).GetComponent<MmRelayNode>();
            if (relayNode != null)
            {
                return relayNode;
            }
            else
            {
                MmLogger.LogError("Could not get MmRelayNode");
                return null;
            }
        }

        /// <summary>
        /// Given a GameObject, extract the first MmRelayNode, if any are present.
        /// </summary>
        /// <param name="go">Observed GameObject.</param>
        /// <returns>First MmRelayNode on object, if any present.</returns>
        public static MmRelayNode GetRelayNode(GameObject go)
        {
            var relayNode = go.GetComponent<MmRelayNode>();
            if (relayNode != null)
            {
                return relayNode;
            }
            return null;
        }
        #endregion
    }
}