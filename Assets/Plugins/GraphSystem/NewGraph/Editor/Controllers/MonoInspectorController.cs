using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NewGraph {
    public class MonoInspectorController : InspectorControllerBase {
        public MonoInspectorController(VisualElement parent) : base(parent) {}

        public override void CreateRenameGraphUI(IGraphModelData graph) {
            Label graphName = inspectorHeader.Q<Label>();
            graphName.text = (graph as MonoGraphModel).name;
        }
        public override void SetupRefreshButton(Button refreshButton) {
            refreshButton.style.display = DisplayStyle.None;
        }
        public override void SetupApplyButton(Button applyButton) {
            applyButton.style.display = DisplayStyle.None;
        }

        public override void SetupCreateButton(Button createButton) {
            createButton.style.display = DisplayStyle.None;
        }

        public override void SetupLoadButton(Button loadButton) {
            loadButton.style.display = DisplayStyle.None;
        }
    }
}
