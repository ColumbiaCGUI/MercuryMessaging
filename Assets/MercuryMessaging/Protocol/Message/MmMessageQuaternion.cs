using System;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// MmMessage with Quaternion payload
    /// </summary>
    public class MmMessageQuaternion : MmMessage
    {
        /// <summary>
        /// Quaternion payload
        /// </summary>
        public Quaternion value;

        /// <summary>
        /// Creates a basic MmMessageQuaternion
        /// </summary>
        public MmMessageQuaternion() { }

        /// <summary>
        /// Creates a basic MmMessageQuaternion, with a control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
        public MmMessageQuaternion(MmMetadataBlock metadataBlock = null)
            : base(metadataBlock, MmMessageType.MmQuaternion)
        {
        }

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and a Quaternion
        /// </summary>
        /// <param name="iVal">Quaternion payload</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
        public MmMessageQuaternion(Quaternion iVal,
            MmMethod mmMethod = default(MmMethod),
            MmMetadataBlock metadataBlock = null)
            : base(mmMethod, MmMessageType.MmQuaternion, metadataBlock)
        {
            value = iVal;
        }

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
        public MmMessageQuaternion(MmMessageQuaternion message) :
            base(message)
        { }

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Duplicate of MmMessage</returns>
        public override MmMessage Copy()
        {
            MmMessageQuaternion newMessage = new MmMessageQuaternion(this);
            newMessage.value = value;

            return newMessage;
        }

        /// <summary>
        /// Deserialize the MmMessageQuaternion
        /// </summary>
        /// <param name="data">Object array representation of a MmMessageQuaternion</param>
        /// <returns>The index of the next element to be read from data</returns>
        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            value = new Quaternion(
                (float)data[index++],
                (float)data[index++],
                (float)data[index++],
                (float)data[index++]
            );
            return index;
        }

        /// <summary>
        /// Serialize the MmMessageQuaternion
        /// </summary>
        /// <returns>Object array representation of a MmMessageQuaternion</returns>
        public override object[] Serialize()
        {
            object[] baseSerialized = base.Serialize();

            // Pre-allocate combined array: base + 4 payload (x, y, z, w)
            object[] result = new object[baseSerialized.Length + 4];

            // Copy base data using Array.Copy (no LINQ)
            Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

            // Fill payload directly
            int idx = baseSerialized.Length;
            result[idx++] = value.x;
            result[idx++] = value.y;
            result[idx++] = value.z;
            result[idx] = value.w;

            return result;
        }
    }
}
