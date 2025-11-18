using Unity.AppUI.Redux;

namespace Unity.AppUI.Samples.Redux
{
    public static class Reducers
    {
        // Here we define the reducers that will handle the actions
        // Thanks to the C# 9.0 "with" expression, we can easily create a new state object based on record type
        
        public static CounterState Increment(CounterState state, Action action)
        {
            return state with { value = state.value + 1 };
        }
        
        public static CounterState Decrement(CounterState state, Action action)
        {
            return state with { value = state.value - 1 };
        }
        
        public static CounterState IncrementBy(CounterState state, Action<int> amount)
        {
            return state with { value = state.value + amount.payload };
        }
        
        public static CounterState Reset(CounterState state, Action action)
        {
            return state with { value = 0 };
        }
    }
}

