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
using System.Linq;
using MercuryMessaging.Support.Extensions;
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
        ///  Queue of MmResponders to add once list is no longer in use
        /// by an MmInvoke
        /// </summary>
		protected Queue<MmRoutingTableItem> MmRespondersToAdd =
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
		private bool doNotModifyRoutingTable;

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

            //Show all items currently in the RoutingTable list.
            //Debug.Log(gameObject.name + " MmRelayNode start called. With " +
            //    RoutingTable.Count +
            //    " items in the MmResponder List: " +
            //    String.Join("\n", RoutingTable.GetMmNames(MmRoutingTable.ListFilter.All,
            //    MmLevelFilterHelper.SelfAndBidirectional).ToArray()));
        }

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
				Level = level
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
            List<MmResponder> responders = GetComponents<MmResponder>().Where(
                x => (!(x is MmRelayNode))).ToList();

            // Add own implementations of IMmResponder to priority list
            foreach (var responder in responders)
            {
				if (!RoutingTable.Contains(responder))
                	MmAddToRoutingTable(responder, MmLevelFilter.Self);
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

            foreach (var child in RoutingTable.Where(x => x.Level == MmLevelFilter.Child))
            {
                var childNode = child.Responder.GetRelayNode();
                childNode.AddParent(this);
                childNode.RefreshParents();
            }

            //Optimize later
            foreach (var parent in MmParentList)
            {
				//bool foundItem = RoutingTable.Select ((x) => x.Responder).Any (responder => responder == parent);
				MmRoutingTableItem foundItem = RoutingTable.First (x => x.Responder == parent);
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
        public override void MmInvoke(MmMessageType msgType, MmMessage message)
        {
            //If the MmRelayNode has not been initialized, initialize it here,
            //  and refresh the parents - to ensure proper routing can occur.
            InitializeNode();

            //TODO: Switch to using mutex for threaded applications
            doNotModifyRoutingTable = true;
            MmNetworkFilter networkFilter = message.MetadataBlock.NetworkFilter;

            //Experimental: Allow forced serial execution (ordered) of messages.
            //if (serialExecution)
            //{
            //    if (!_executing)
            //    {
            //        _executing = true;
            //    }
            //    else
            //    {
            //        MmLogger.LogFramework("<<<<<>>>>>Queueing<<<<<>>>>>");
            //        KeyValuePair<MmMessageType, MmMessage> newMessage =
            //            new KeyValuePair<MmMessageType, MmMessage>(msgType, message);
            //        SerialExecutionQueue.Enqueue(newMessage);
            //        return;
            //    }
            //}
            
            //MmLogger.LogFramework (gameObject.name + ": MmRelayNode received MmMethod call: " + param.MmMethod.ToString ());
            
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

                MmNetworkResponder.MmInvoke (msgType, message);
			}

            
            //Todo: it's possible to get this to happen only once per graph. Switch Invoke code to support.
            var upwardMessage = message.Copy();
			upwardMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndParents;
			var downwardMessage = message.Copy();
			downwardMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;

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

            foreach (var routingTableItem in RoutingTable) {
				var responder = routingTableItem.Responder;

				//bool isLocalResponder = responder.MmGameObject == this.gameObject;
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

                if (ResponderCheck (levelFilter, activeFilter, selectedFilter, networkFilter,
                    routingTableItem, responderSpecificMessage)) {
					responder.MmInvoke (msgType, responderSpecificMessage);
				}
			}

			//if (dirty && _prevMessageTime == message.TimeStamp)
			//{
			//    dirty = false;
			//}

            doNotModifyRoutingTable = false;

            while (MmRespondersToAdd.Any())
            {
                var routingTableItem = MmRespondersToAdd.Dequeue();

                MmAddToRoutingTable(routingTableItem.Responder, routingTableItem.Level);

                if (ResponderCheck(levelFilter, activeFilter, selectedFilter, networkFilter,
                    routingTableItem, message))
                {
                    routingTableItem.Responder.MmInvoke(msgType, message);
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
			MmMessage msg = new MmMessage (mmMethod, metadataBlock);
            MmInvoke(MmMessageType.MmVoid, msg);
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
            MmInvoke(msgType, msg);
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
            MmInvoke(MmMessageType.MmBool, msg);
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
            MmInvoke(MmMessageType.MmInt, msg);
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
            MmInvoke(MmMessageType.MmFloat, msg);
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
            MmInvoke(MmMessageType.MmVector3, msg);
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
            MmInvoke(MmMessageType.MmVector4, msg);
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
            MmInvoke(MmMessageType.MmString, msg);
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
            MmInvoke(MmMessageType.MmByteArray, msg);
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
			MmInvoke(MmMessageType.MmTransform, msg);
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
			MmInvoke(MmMessageType.MmTransformList, msg);
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
            MmInvoke(MmMessageType.MmSerializable, msg);
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
            MmInvoke(MmMessageType.MmGameObject, msg);
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
