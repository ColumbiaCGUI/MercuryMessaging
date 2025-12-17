/// <summary>
/// Tutorial 4: Custom message type IDs.
///
/// Message Type ID ranges:
/// - 0-99: Standard MmMessageType (MmVoid, MmBool, MmInt, etc.)
/// - 1100+: Your custom message types
///
/// NOTE: Custom METHOD IDs (Tutorial 3) start at 1000.
/// Custom MESSAGE TYPE IDs start at 1100 to avoid confusion.
/// </summary>
public static class T4_MyMessageTypes
{
    // Custom message types start at 1100 to avoid conflicts
    public const int ColorIntensity = 1100;
    public const int EnemyState = 1101;
    public const int PlayerStats = 1102;
}
