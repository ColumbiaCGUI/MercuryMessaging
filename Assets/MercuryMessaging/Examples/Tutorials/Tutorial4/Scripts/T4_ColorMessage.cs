// Copyright (c) 2017-2025, Columbia University
// Tutorial 4: Custom Messages Example
// This demonstrates how to create a custom message with payload data.
//
// Key Points:
// 1. Extend MmMessage
// 2. Define custom payload fields
// 3. Implement Copy() for message routing
// 4. Implement Serialize()/Deserialize() for networking
// 5. Use custom MmMethod IDs (1000+) and MmMessageType IDs (1100+)

using System;
using UnityEngine;
using MercuryMessaging;

namespace MercuryMessaging.Examples.Tutorials
{
    /// <summary>
    /// Custom method IDs for Tutorial 4.
    /// Use values >= 1000 to avoid conflicts with built-in methods.
    /// </summary>
    public static class T4_Methods
    {
        public const int ChangeColor = 1000;
    }

    /// <summary>
    /// Custom message type IDs for Tutorial 4.
    /// Use values >= 1100 to avoid conflicts with built-in types.
    /// </summary>
    public static class T4_MessageTypes
    {
        public const int ColorMessage = 1100;
    }

    /// <summary>
    /// Custom message that carries a Color payload.
    /// Demonstrates proper custom message implementation with serialization.
    /// </summary>
    public class T4_ColorMessage : MmMessage
    {
        /// <summary>
        /// The color payload.
        /// </summary>
        public Color value;

        /// <summary>
        /// Default constructor (required for deserialization).
        /// </summary>
        public T4_ColorMessage() : base()
        {
            MmMethod = (MmMethod)T4_Methods.ChangeColor;
            MmMessageType = (MmMessageType)T4_MessageTypes.ColorMessage;
        }

        /// <summary>
        /// Convenience constructor with color value.
        /// </summary>
        /// <param name="color">The color to send.</param>
        public T4_ColorMessage(Color color) : this()
        {
            value = color;
        }

        /// <summary>
        /// Full constructor with routing control.
        /// </summary>
        /// <param name="color">The color to send.</param>
        /// <param name="metadataBlock">Routing control parameters.</param>
        public T4_ColorMessage(Color color, MmMetadataBlock metadataBlock)
            : base(metadataBlock, (MmMessageType)T4_MessageTypes.ColorMessage)
        {
            value = color;
            MmMethod = (MmMethod)T4_Methods.ChangeColor;
        }

        /// <summary>
        /// Copy constructor for message routing.
        /// </summary>
        /// <param name="original">Message to copy.</param>
        public T4_ColorMessage(T4_ColorMessage original) : base(original)
        {
            value = original.value;
        }

        /// <summary>
        /// REQUIRED: Creates a copy of this message for routing.
        /// Called when message needs to be sent to multiple destinations.
        /// </summary>
        public override MmMessage Copy()
        {
            return new T4_ColorMessage(this);
        }

        /// <summary>
        /// Serialize the message to object array for network transmission.
        /// Order must match Deserialize().
        /// </summary>
        public override object[] Serialize()
        {
            // Get base serialized data (method, type, netId, metadata)
            object[] baseSerialized = base.Serialize();

            // Allocate result: base + 4 (r, g, b, a)
            object[] result = new object[baseSerialized.Length + 4];

            // Copy base data
            Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

            // Serialize color components
            result[baseSerialized.Length + 0] = value.r;
            result[baseSerialized.Length + 1] = value.g;
            result[baseSerialized.Length + 2] = value.b;
            result[baseSerialized.Length + 3] = value.a;

            return result;
        }

        /// <summary>
        /// Deserialize the message from object array received from network.
        /// Order must match Serialize().
        /// </summary>
        public override int Deserialize(object[] data)
        {
            // Deserialize base fields first
            int index = base.Deserialize(data);

            // Deserialize color components (same order as Serialize)
            float r = Convert.ToSingle(data[index++]);
            float g = Convert.ToSingle(data[index++]);
            float b = Convert.ToSingle(data[index++]);
            float a = Convert.ToSingle(data[index++]);
            value = new Color(r, g, b, a);

            return index;
        }
    }
}