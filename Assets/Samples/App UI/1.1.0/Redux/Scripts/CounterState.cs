namespace Unity.AppUI.Samples.Redux
{
    // This is a state that will be stored in the store
    // It can be retrieved by any reducer from the "counter" slice of state
    // NOTE: The new record type is a C# 9 feature that allows us to create immutable types with a concise syntax
    public record CounterState(int value)
    {
        public int value { get; set; } = value;
    }
}
