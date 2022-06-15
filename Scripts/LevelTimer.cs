using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelTimer : MonoBehaviour
{
    public float timeRemaining = 60;
    public float startTimeRemaining = 3;
    public bool timerIsRunning = false;
    public Text timeText;
    public Text startTimeText;
    [SerializeField]
    private PlayerMovementAdvanced playerMovementAdvanced;

    private void Start()
    {
        playerMovementAdvanced.enabled = false;
        startTimeText.enabled = true;
        timeText.enabled = false;
    }
    void Update()
    {
        if (startTimeRemaining > 0)
        {
            startTimeRemaining -= Time.deltaTime;
            DisplayStartTime(startTimeRemaining);
        }
        else
        {
            startTimeRemaining = 0;
            startTimeText.enabled = false;
            timeText.enabled = true;
            // Starts the timer automatically
            timerIsRunning = true;
            playerMovementAdvanced.enabled = true;
        }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                timeText.text = DisplayFormatedTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                playerMovementAdvanced.DestroyPlayer();
            }
        }
    }
    public string DisplayFormatedTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void DisplayStartTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        startTimeText.text = string.Format("{00}", seconds);
    }
}