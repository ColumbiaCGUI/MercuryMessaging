// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmUIMessages.cs - Standard Library UI Message Types
// Part of DSL Overhaul Phase 9
//
// UI messages use MmMethod values 100-199 (Standard Library UI range)
// These are handled by MmUIResponder, not MmExtendableResponder

using System.Linq;
using UnityEngine;

namespace MercuryMessaging.StandardLibrary.UI
{
    #region Enums

    /// <summary>
    /// Standard Library UI methods (100-199 range).
    /// Use these with MmUIResponder for type-safe UI event handling.
    /// </summary>
    public enum MmUIMethod
    {
        /// <summary>UI click/tap event</summary>
        Click = 100,

        /// <summary>UI hover enter/exit event</summary>
        Hover = 101,

        /// <summary>UI drag start/move/end event</summary>
        Drag = 102,

        /// <summary>UI scroll event</summary>
        Scroll = 103,

        /// <summary>UI focus gained/lost event</summary>
        Focus = 104,

        /// <summary>UI selection changed event</summary>
        Select = 105,

        /// <summary>UI submit/confirm action</summary>
        Submit = 106,

        /// <summary>UI cancel action</summary>
        Cancel = 107
    }

    /// <summary>
    /// UI message types for serialization (2001+ range).
    /// </summary>
    public enum MmUIMessageType : short
    {
        UIClick = 2001,
        UIHover = 2002,
        UIDrag = 2003,
        UIScroll = 2004,
        UIFocus = 2005,
        UISelect = 2006,
        UISubmit = 2007,
        UICancel = 2008
    }

    /// <summary>
    /// Drag phase for drag events.
    /// </summary>
    public enum MmDragPhase
    {
        Begin,
        Move,
        End
    }

    #endregion

    #region Click Message

    /// <summary>
    /// UI click/tap message.
    /// Sent when a UI element is clicked or tapped.
    /// </summary>
    /// <example>
    /// <code>
    /// // Send click message
    /// relay.Send(new MmUIClickMessage(Input.mousePosition, 1)).Execute();
    ///
    /// // Handle in MmUIResponder
    /// protected override void ReceivedClick(MmUIClickMessage msg)
    /// {
    ///     Debug.Log($"Clicked at {msg.Position}, count: {msg.ClickCount}");
    /// }
    /// </code>
    /// </example>
    public class MmUIClickMessage : MmMessage
    {
        /// <summary>Screen position of the click</summary>
        public Vector2 Position;

        /// <summary>Number of clicks (1 = single, 2 = double)</summary>
        public int ClickCount;

        /// <summary>Mouse button index (0 = left, 1 = right, 2 = middle)</summary>
        public int Button;

        /// <summary>Returns true if this is a double-click</summary>
        public bool IsDoubleClick => ClickCount >= 2;

        /// <summary>Returns true if this is a right-click</summary>
        public bool IsRightClick => Button == 1;

        public MmUIClickMessage() : base((MmMethod)MmUIMethod.Click, (MmMessageType)MmUIMessageType.UIClick)
        {
        }

        public MmUIClickMessage(Vector2 position, int clickCount = 1, int button = 0, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmUIMethod.Click, (MmMessageType)MmUIMessageType.UIClick, metadataBlock)
        {
            Position = position;
            ClickCount = clickCount;
            Button = button;
        }

        public MmUIClickMessage(MmUIClickMessage message) : base(message)
        {
            Position = message.Position;
            ClickCount = message.ClickCount;
            Button = message.Button;
        }

        public override MmMessage Copy()
        {
            return new MmUIClickMessage(this);
        }

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Position = new Vector2((float)data[index++], (float)data[index++]);
            ClickCount = (int)data[index++];
            Button = (int)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { Position.x, Position.y, ClickCount, Button }).ToArray();
        }
    }

    #endregion

    #region Hover Message

    /// <summary>
    /// UI hover message.
    /// Sent when pointer enters or exits a UI element.
    /// </summary>
    public class MmUIHoverMessage : MmMessage
    {
        /// <summary>Screen position of the pointer</summary>
        public Vector2 Position;

        /// <summary>True if pointer entered, false if exited</summary>
        public bool IsEnter;

        public MmUIHoverMessage() : base((MmMethod)MmUIMethod.Hover, (MmMessageType)MmUIMessageType.UIHover)
        {
        }

        public MmUIHoverMessage(Vector2 position, bool isEnter, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmUIMethod.Hover, (MmMessageType)MmUIMessageType.UIHover, metadataBlock)
        {
            Position = position;
            IsEnter = isEnter;
        }

        public MmUIHoverMessage(MmUIHoverMessage message) : base(message)
        {
            Position = message.Position;
            IsEnter = message.IsEnter;
        }

        public override MmMessage Copy()
        {
            return new MmUIHoverMessage(this);
        }

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Position = new Vector2((float)data[index++], (float)data[index++]);
            IsEnter = (bool)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { Position.x, Position.y, IsEnter }).ToArray();
        }
    }

    #endregion

    #region Drag Message

    /// <summary>
    /// UI drag message.
    /// Sent during drag operations (begin, move, end).
    /// </summary>
    public class MmUIDragMessage : MmMessage
    {
        /// <summary>Current screen position</summary>
        public Vector2 Position;

        /// <summary>Movement delta since last frame</summary>
        public Vector2 Delta;

        /// <summary>Drag phase (Begin, Move, End)</summary>
        public MmDragPhase Phase;

        public MmUIDragMessage() : base((MmMethod)MmUIMethod.Drag, (MmMessageType)MmUIMessageType.UIDrag)
        {
        }

        public MmUIDragMessage(Vector2 position, Vector2 delta, MmDragPhase phase, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmUIMethod.Drag, (MmMessageType)MmUIMessageType.UIDrag, metadataBlock)
        {
            Position = position;
            Delta = delta;
            Phase = phase;
        }

        public MmUIDragMessage(MmUIDragMessage message) : base(message)
        {
            Position = message.Position;
            Delta = message.Delta;
            Phase = message.Phase;
        }

        public override MmMessage Copy()
        {
            return new MmUIDragMessage(this);
        }

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Position = new Vector2((float)data[index++], (float)data[index++]);
            Delta = new Vector2((float)data[index++], (float)data[index++]);
            Phase = (MmDragPhase)(int)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { Position.x, Position.y, Delta.x, Delta.y, (int)Phase }).ToArray();
        }
    }

    #endregion

    #region Scroll Message

    /// <summary>
    /// UI scroll message.
    /// Sent when scroll wheel is used or touch scrolling occurs.
    /// </summary>
    public class MmUIScrollMessage : MmMessage
    {
        /// <summary>Screen position of the pointer</summary>
        public Vector2 Position;

        /// <summary>Scroll delta (x = horizontal, y = vertical)</summary>
        public Vector2 ScrollDelta;

        public MmUIScrollMessage() : base((MmMethod)MmUIMethod.Scroll, (MmMessageType)MmUIMessageType.UIScroll)
        {
        }

        public MmUIScrollMessage(Vector2 position, Vector2 scrollDelta, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmUIMethod.Scroll, (MmMessageType)MmUIMessageType.UIScroll, metadataBlock)
        {
            Position = position;
            ScrollDelta = scrollDelta;
        }

        public MmUIScrollMessage(MmUIScrollMessage message) : base(message)
        {
            Position = message.Position;
            ScrollDelta = message.ScrollDelta;
        }

        public override MmMessage Copy()
        {
            return new MmUIScrollMessage(this);
        }

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Position = new Vector2((float)data[index++], (float)data[index++]);
            ScrollDelta = new Vector2((float)data[index++], (float)data[index++]);
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { Position.x, Position.y, ScrollDelta.x, ScrollDelta.y }).ToArray();
        }
    }

    #endregion

    #region Focus Message

    /// <summary>
    /// UI focus message.
    /// Sent when a UI element gains or loses focus.
    /// </summary>
    public class MmUIFocusMessage : MmMessage
    {
        /// <summary>True if focus gained, false if lost</summary>
        public bool IsFocused;

        /// <summary>Optional ID of the focused element</summary>
        public string ElementId;

        public MmUIFocusMessage() : base((MmMethod)MmUIMethod.Focus, (MmMessageType)MmUIMessageType.UIFocus)
        {
        }

        public MmUIFocusMessage(bool isFocused, string elementId = null, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmUIMethod.Focus, (MmMessageType)MmUIMessageType.UIFocus, metadataBlock)
        {
            IsFocused = isFocused;
            ElementId = elementId ?? string.Empty;
        }

        public MmUIFocusMessage(MmUIFocusMessage message) : base(message)
        {
            IsFocused = message.IsFocused;
            ElementId = message.ElementId;
        }

        public override MmMessage Copy()
        {
            return new MmUIFocusMessage(this);
        }

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            IsFocused = (bool)data[index++];
            ElementId = (string)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { IsFocused, ElementId ?? string.Empty }).ToArray();
        }
    }

    #endregion

    #region Select Message

    /// <summary>
    /// UI selection message.
    /// Sent when a selection changes in a list, dropdown, etc.
    /// </summary>
    public class MmUISelectMessage : MmMessage
    {
        /// <summary>Index of selected item (-1 if none)</summary>
        public int SelectedIndex;

        /// <summary>Value/label of selected item</summary>
        public string SelectedValue;

        /// <summary>Previous selection index (-1 if none)</summary>
        public int PreviousIndex;

        public MmUISelectMessage() : base((MmMethod)MmUIMethod.Select, (MmMessageType)MmUIMessageType.UISelect)
        {
        }

        public MmUISelectMessage(int selectedIndex, string selectedValue = null, int previousIndex = -1, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmUIMethod.Select, (MmMessageType)MmUIMessageType.UISelect, metadataBlock)
        {
            SelectedIndex = selectedIndex;
            SelectedValue = selectedValue ?? string.Empty;
            PreviousIndex = previousIndex;
        }

        public MmUISelectMessage(MmUISelectMessage message) : base(message)
        {
            SelectedIndex = message.SelectedIndex;
            SelectedValue = message.SelectedValue;
            PreviousIndex = message.PreviousIndex;
        }

        public override MmMessage Copy()
        {
            return new MmUISelectMessage(this);
        }

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            SelectedIndex = (int)data[index++];
            SelectedValue = (string)data[index++];
            PreviousIndex = (int)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { SelectedIndex, SelectedValue ?? string.Empty, PreviousIndex }).ToArray();
        }
    }

    #endregion

    #region Submit/Cancel Messages

    /// <summary>
    /// UI submit message.
    /// Sent when user confirms an action (Enter key, OK button, etc).
    /// </summary>
    public class MmUISubmitMessage : MmMessage
    {
        /// <summary>Optional data associated with the submit action</summary>
        public string Data;

        public MmUISubmitMessage() : base((MmMethod)MmUIMethod.Submit, (MmMessageType)MmUIMessageType.UISubmit)
        {
        }

        public MmUISubmitMessage(string data = null, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmUIMethod.Submit, (MmMessageType)MmUIMessageType.UISubmit, metadataBlock)
        {
            Data = data ?? string.Empty;
        }

        public MmUISubmitMessage(MmUISubmitMessage message) : base(message)
        {
            Data = message.Data;
        }

        public override MmMessage Copy()
        {
            return new MmUISubmitMessage(this);
        }

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Data = (string)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { Data ?? string.Empty }).ToArray();
        }
    }

    /// <summary>
    /// UI cancel message.
    /// Sent when user cancels an action (Escape key, Cancel button, etc).
    /// </summary>
    public class MmUICancelMessage : MmMessage
    {
        /// <summary>Optional reason for cancellation</summary>
        public string Reason;

        public MmUICancelMessage() : base((MmMethod)MmUIMethod.Cancel, (MmMessageType)MmUIMessageType.UICancel)
        {
        }

        public MmUICancelMessage(string reason = null, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmUIMethod.Cancel, (MmMessageType)MmUIMessageType.UICancel, metadataBlock)
        {
            Reason = reason ?? string.Empty;
        }

        public MmUICancelMessage(MmUICancelMessage message) : base(message)
        {
            Reason = message.Reason;
        }

        public override MmMessage Copy()
        {
            return new MmUICancelMessage(this);
        }

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Reason = (string)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { Reason ?? string.Empty }).ToArray();
        }
    }

    #endregion
}
