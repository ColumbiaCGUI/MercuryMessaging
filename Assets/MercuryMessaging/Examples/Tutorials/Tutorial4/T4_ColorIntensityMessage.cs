using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 4: Custom message carrying Color and intensity.
/// Demonstrates how to extend MmMessage for custom data payloads.
///
/// Key requirements:
/// 1. Default constructor setting MmMethod and MmMessageType
/// 2. Copy() method for message routing
/// 3. Serialize()/Deserialize() for networking (object[] pattern)
/// </summary>
public class T4_ColorIntensityMessage : MmMessage
{
    // Custom payload fields
    public Color color;
    public float intensity;

    /// <summary>
    /// Default constructor - REQUIRED for deserialization.
    /// Sets the method and message type IDs.
    /// </summary>
    public T4_ColorIntensityMessage() : base()
    {
        MmMethod = (MmMethod)T3_MyMethods.ChangeColor;
        MmMessageType = (MmMessageType)T4_MyMessageTypes.ColorIntensity;
    }

    /// <summary>
    /// Convenience constructor with payload values.
    /// </summary>
    public T4_ColorIntensityMessage(Color color, float intensity) : this()
    {
        this.color = color;
        this.intensity = intensity;
    }

    /// <summary>
    /// REQUIRED: Copy method for message routing.
    /// Creates a deep copy of the message including all fields.
    /// </summary>
    public override MmMessage Copy()
    {
        var copy = new T4_ColorIntensityMessage
        {
            // Copy base fields
            MmMethod = this.MmMethod,
            MmMessageType = this.MmMessageType,
            MetadataBlock = this.MetadataBlock,
            NetId = this.NetId,
            TimeStamp = this.TimeStamp,
            HopCount = this.HopCount,

            // Copy custom fields
            color = this.color,
            intensity = this.intensity
        };
        return copy;
    }

    /// <summary>
    /// REQUIRED for networking: Serialize to object array.
    /// Order must match Deserialize() exactly.
    /// </summary>
    public override object[] Serialize()
    {
        // Get base serialized data
        object[] baseSerialized = base.Serialize();

        // Allocate result: base + 5 (color r,g,b,a + intensity)
        object[] result = new object[baseSerialized.Length + 5];

        // Copy base data
        System.Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

        // Write custom fields (same order as Deserialize!)
        int idx = baseSerialized.Length;
        result[idx++] = color.r;
        result[idx++] = color.g;
        result[idx++] = color.b;
        result[idx++] = color.a;
        result[idx++] = intensity;

        return result;
    }

    /// <summary>
    /// REQUIRED for networking: Deserialize from object array.
    /// Order must match Serialize() exactly.
    /// </summary>
    public override int Deserialize(object[] data)
    {
        // Deserialize base fields first
        int index = base.Deserialize(data);

        // Read custom fields (same order as Serialize!)
        float r = System.Convert.ToSingle(data[index++]);
        float g = System.Convert.ToSingle(data[index++]);
        float b = System.Convert.ToSingle(data[index++]);
        float a = System.Convert.ToSingle(data[index++]);
        color = new Color(r, g, b, a);
        intensity = System.Convert.ToSingle(data[index++]);

        return index;
    }
}
