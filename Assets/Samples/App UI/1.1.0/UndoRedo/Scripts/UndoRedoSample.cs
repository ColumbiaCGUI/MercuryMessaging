using System;
using System.Linq;
using Unity.AppUI.Redux;
using Unity.AppUI.UI;
using Unity.AppUI.Undo;
using UnityEngine;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.Samples.UndoRedo
{
    public class UndoRedoSample : MonoBehaviour
    {
        const string k_AppStateSlice = "app";
        
        const string k_SetColorAction = "app/SetColor";
        
        const string k_SetTextAction = "app/SetText";
     
        static readonly ActionCreator<Color> setColorAction = Store.CreateAction<Color>(k_SetColorAction);
        
        static readonly ActionCreator<string> setTextAction = Store.CreateAction<string>(k_SetTextAction);
        
        Unsubscriber m_Unsubscriber;
        
        void Start()
        {
            var uiDocument = GetComponent<UIDocument>();  
            var colorInput = uiDocument.rootVisualElement.Q<ColorField>("colorInput"); 
            var textInput = uiDocument.rootVisualElement.Q<TextField>("textInput");
            var result = uiDocument.rootVisualElement.Q<Text>("result");
            var undoButton = uiDocument.rootVisualElement.Q<ActionButton>("undoButton");
            var redoButton = uiDocument.rootVisualElement.Q<ActionButton>("redoButton");
            var historyListView = uiDocument.rootVisualElement.Q<ListView>("history");
            
            undoButton.SetEnabled(false);
            redoButton.SetEnabled(false);
            
            colorInput.SetValueWithoutNotify(Color.white);

            var store = new Store();
            var undoStack = new UndoStack();
            var initialState = new AppState
            {
                color = Color.white,
                text = "Result"
            };
            store.CreateSlice(k_AppStateSlice, initialState, builder =>
            {
                builder.Add<Color>(k_SetColorAction, (state, action) =>
                {
                    state = state with { color = action.payload };
                    return state;
                });
                
                builder.Add<string>(k_SetTextAction, (state, action) =>
                {
                    state = state with { text = action.payload };
                    return state;
                });
            });

            m_Unsubscriber = store.Subscribe<AppState>(k_AppStateSlice, state =>
            {
                result.style.color = state.color;
                result.text = state.text;
                
                undoButton.SetEnabled(undoStack.canUndo);
                redoButton.SetEnabled(undoStack.canRedo);
                
                if (colorInput.value != state.color)
                    colorInput.SetValueWithoutNotify(state.color);
                if (textInput.value != state.text)
                    textInput.SetValueWithoutNotify(state.text);
            });

            colorInput.RegisterValueChangingCallback(evt =>
            {
                store.Dispatch(setColorAction.Invoke(evt.newValue));
            });

            colorInput.RegisterValueChangedCallback(evt =>
            {
                var cmd = new SetColorCommand("Set Color", evt.previousValue, evt.newValue, store);
                undoStack.Push(cmd);
            });
            
            textInput.RegisterValueChangingCallback(evt =>
            {
                store.Dispatch(setTextAction.Invoke(evt.newValue));
            });
            
            textInput.RegisterValueChangedCallback(evt =>
            {
                var cmd = new SetTextCommand("Set Text", evt.previousValue, evt.newValue, store);
                undoStack.Push(cmd);
            });
            
            historyListView.selectionType = SelectionType.Single;
            
            historyListView.makeItem = () =>
            {
                var item = new Label();
                item.AddToClassList("history-item");
                return item;
            };
            
            historyListView.bindItem = (element, idx) =>
            {
                var item = (Label)element;
                var cmd = historyListView.itemsSource[idx] as UndoCommand;
                item.text = cmd?.name;
            };
            
#if UNITY_2022_2_OR_NEWER
            historyListView.selectedIndicesChanged += (indices) =>
#else
            historyListView.onSelectedIndicesChange += (indices) =>
#endif
            {
                foreach (var index in indices)
                {
                    if (undoStack.index != index)
                        undoStack.index = index;
                    break;
                }
            };
            
            undoStack.indexChanged += (idx) =>
            {
                var list = undoStack.commands.ToList();
                historyListView.itemsSource = list;
                historyListView.RefreshItems();
                historyListView.SetSelectionWithoutNotify(new []{ idx });
                
                undoButton.SetEnabled(undoStack.canUndo);
                redoButton.SetEnabled(undoStack.canRedo);
            };
            
            undoButton.clickable.clicked += () =>
            {
                undoStack.Undo();
            };
            
            redoButton.clickable.clicked += () =>
            {
                undoStack.Redo();
            };
        }

        void OnDestroy()
        {
            m_Unsubscriber?.Invoke();
            m_Unsubscriber = null;
        }

        public record AppState
        {
            public Color color { get; set; } = Color.white;
            
            public string text { get; set; } = "Result";
        }
        
        public class SetColorCommand : UndoCommand
        {
            public Color previousColor { get; private set; }
            
            public Color newColor { get; private set; }
            
            public Store store { get; private set; }
            
            public SetColorCommand(string name, Color previousColor, Color newColor, Store store)
                : base(name)
            { 
                this.previousColor = previousColor;
                this.newColor = newColor;
                this.store = store;
            }

            public override string id => k_SetColorAction;

            public override ulong memorySize => 16;

            public override void Undo()
            {
                store.Dispatch(setColorAction.Invoke(previousColor));
            }

            public override void Redo()
            {
                store.Dispatch(setColorAction.Invoke(newColor));
            }
            
            public override bool MergeWith(UndoCommand command)
            {
                if (command.id == id)
                {
                    newColor = ((SetColorCommand)command).newColor;
                    return true;
                }
                
                return false;
            }
            
            public override void OnFlush()
            {
                store = null;
            }
        }
        
        public class SetTextCommand : UndoCommand
        {
            public string previousText { get; private set; }
            
            public string newText { get; private set; }
            
            public Store store { get; private set; }
            
            public SetTextCommand(string name, string previousText, string newText, Store store)
                : base(name)
            { 
                this.previousText = previousText;
                this.newText = newText;
                this.store = store;
            }

            public override string id => k_SetTextAction;
            
            public override ulong memorySize => 16;

            public override void Undo()
            {
                store.Dispatch(setTextAction.Invoke(previousText));
            }

            public override void Redo()
            {
                store.Dispatch(setTextAction.Invoke(newText));
            }
            
            public override bool MergeWith(UndoCommand command)
            {
                if (command.id == id)
                {
                    newText = ((SetTextCommand)command).newText;
                    return true;
                }
                
                return false;
            }

            public override void OnFlush()
            {
                store = null;
            }
        }
    }
}