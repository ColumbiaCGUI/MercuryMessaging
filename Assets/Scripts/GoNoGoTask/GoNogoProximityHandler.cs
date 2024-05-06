using UnityEngine;
using UnityEngine.UI;

public class GoNogoProximityHandler : MonoBehaviour
{
    public GameObject instructionUI;  // Assign the instruction UI element in the inspector
    public GoNogoGameController gameController;  // Reference to the game controller

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Make sure the player GameObject has the tag "Player"
        {
            instructionUI.SetActive(true);  // Show instructions
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            instructionUI.SetActive(false);  // Hide instructions when player moves away
        }
    }

    public void StartGame()
    {
        instructionUI.SetActive(false);  // Optionally hide instructions on game start
        gameController.StartGame();  // Call the method to start the game timer
    }
}