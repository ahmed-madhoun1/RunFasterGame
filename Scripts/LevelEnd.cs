using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{

    [SerializeField] private GameObject LevelEndMenu;
    [SerializeField] private Text levelFinishedTimeText;
    [SerializeField] private LevelTimer levelTimerScript;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            PlayerMovementAdvanced playerMovementAdvanced = collider.GetComponent<PlayerMovementAdvanced>();
            if (!playerMovementAdvanced.isPlayerKilled)
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                LevelEndMenu.SetActive(true);
                levelFinishedTimeText.text = levelTimerScript.DisplayFormatedTime(levelTimerScript.timeRemaining);
            }
        }

    }
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(Levels.MAIN_MENU);
    }
}
