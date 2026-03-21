using UnityEngine;
using TMPro;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Displays task instructions as a dockable Unity Editor window
    /// or in-game overlay during the study.
    /// </summary>
    public class TaskInstructionPanel : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI instructionText;
        public TextMeshProUGUI taskTitleText;
        public TextMeshProUGUI conditionLabel;

        public void ShowInstructions(string title, string condition, string instructions)
        {
            if (taskTitleText != null) taskTitleText.text = title;
            if (conditionLabel != null) conditionLabel.text = condition;
            if (instructionText != null) instructionText.text = instructions;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
