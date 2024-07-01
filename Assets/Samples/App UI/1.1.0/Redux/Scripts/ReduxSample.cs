using UnityEngine;
using Unity.AppUI.Redux;

namespace Unity.AppUI.Samples.Redux
{
    public class ReduxSample : MonoBehaviour
    {
        const string RESET_COUNTER = "counter/Reset";
        
        const string INCREMENT_COUNTER = "counter/Increment";
        
        const string DECREMENT_COUNTER = "counter/Decrement";
        
        const string INCREMENT_COUNTER_BY = "counter/IncrementBy";
        
        const string COUNTER_SLICE = "counter";
        
        void Start()
        {
            // First you need to create a store that will hold the state of your application
            // You should have only one store in your application
            // Afterward you can create slices of state that will be stored in the store to emulate some kind of namespaces
            var store = new Store();
            
            // You can create general purpose Actions that can be dispatched to the store
            // When building reducers, Actions are used to determine which reducer to call
            // You can also create Actions that are specific to a slice of state by using the naming convention "sliceName/actionName"
            var resetAction = Store.CreateAction(RESET_COUNTER);
            
            var incrementAction = Store.CreateAction(INCREMENT_COUNTER);
            
            var decrementAction = Store.CreateAction(DECREMENT_COUNTER);
            
            var incrementByAction = Store.CreateAction<int>(INCREMENT_COUNTER_BY);

            // You can create slices of state that will be stored in the store
            // Each slice of state can have its own reducers and actions
            // It is possible to pass extra reducers that are bound to external actions
            var slice = store.CreateSlice(
                COUNTER_SLICE, 
                new CounterState(0),
                (builder) =>
            {
                builder.Add(INCREMENT_COUNTER, Reducers.Increment);
                builder.Add(DECREMENT_COUNTER, Reducers.Decrement);
                
                // You need to explicitly specify the type of Action payload that the reducer will handle (if any)
                builder.Add<int>(INCREMENT_COUNTER_BY, Reducers.IncrementBy);
            }, 
                (builder) =>
            {
                builder.AddCase(resetAction, Reducers.Reset);
            });

            // You can then retrieve the state of a slice of state from the store
            // The state is immutable, so you can't modify it directly
            // You need to dispatch an action to the store to modify the state
            Debug.Log($"[State]: Counter state is {store.GetState<CounterState>(COUNTER_SLICE)}");
            
            // You can then subscribe to the store to be notified when the state changes
            // The store will notify you of any change to the state, so you need to filter the changes yourself
            // The subscribe method returns an unsubscribe function that you can call to unsubscribe from the store
            Debug.Log($"[Subscribe]: Subscribing to the store for changes to the counter slice");
            var unSub = store.Subscribe(COUNTER_SLICE, (CounterState state) =>
            {
                Debug.Log($"[Notification]: Counter state has changed to {state}");
            });
            
            // You can then dispatch actions to the store
            // The store will call the reducers that are bound to the action
            // The reducers will then return a new state that will be stored in the store
            // The store will then notify all subscribers of the change
            Debug.Log($"[Dispatch]: Dispatching Increment action");
            store.Dispatch(incrementAction.Invoke());
            
            Debug.Log($"[State]: Counter state is {store.GetState<CounterState>(COUNTER_SLICE)}");

            // Call the unsubscribe function to stop receiving notifications
            Debug.Log($"[Unsubscribe]: Unsubscribing from the store");
            unSub();
            
            Debug.Log($"[Dispatch]: Dispatching Reset action");
            store.Dispatch(resetAction.Invoke());
            
            Debug.Log($"[State]: Counter state is {store.GetState<CounterState>(COUNTER_SLICE)}");
            
            Debug.Log($"[Dispatch]: Dispatching IncrementBy action with payload 10");
            store.Dispatch(incrementByAction.Invoke(10));
            
            Debug.Log($"[State]: Counter state is {store.GetState<CounterState>(COUNTER_SLICE)}");
        }
    }
}
