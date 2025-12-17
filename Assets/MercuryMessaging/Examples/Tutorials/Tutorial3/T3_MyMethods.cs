using MercuryMessaging;

/// <summary>
/// Tutorial 3: Custom method constants.
///
/// Method ID ranges:
/// - 0-18: Standard MmMethod (Initialize, SetActive, MessageString, etc.)
/// - 100-199: UI Messages (StandardLibrary)
/// - 200-299: Input Messages (StandardLibrary)
/// - 1000+: Your custom methods
/// </summary>
public static class T3_MyMethods
{
    // Custom methods start at 1000 to avoid conflicts
    public const int TakeDamage = 1000;
    public const int ChangeColor = 1001;
    public const int EnableGravity = 1002;
    public const int Heal = 1003;
    public const int PlaySound = 1004;

    // Helper to cast to MmMethod
    public static MmMethod AsMmMethod(int methodId) => (MmMethod)methodId;
}
