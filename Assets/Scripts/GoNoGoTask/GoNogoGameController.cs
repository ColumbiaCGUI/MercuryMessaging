// GoNogoColorMessage.cs
using UnityEngine;
using MercuryMessaging;

public class GoNogoGameController : MonoBehaviour
{
    public float gameDuration = 60.0f;
    private float timer;
    public int score = 0;
    private bool gameStarted = false;  // To control the start of the game

    void Update()
    {
        if (gameStarted && timer > 0)
        {
            timer -= Time.deltaTime;
            // Add existing game logic here
        }
        else if (timer <= 0)
        {
            Debug.Log("Game Over! Your score: " + score);
            this.enabled = false;  // Disable further updates
        }
    }

    public void StartGame()
    {
        timer = gameDuration;
        gameStarted = true;  // Start the game only when this method is called
    }

    public void AdjustScore(bool correct)
    {
        if (correct)
        {
            score += 10;
        }
        else
        {
            score -= 10;
        }
        Debug.Log("Current Score: " + score);
    }
}