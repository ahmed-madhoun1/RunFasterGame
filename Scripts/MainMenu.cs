using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;


public class MainMenu : MonoBehaviour
{

    [SerializeField] private Button[] levelsButtons;
    public Text percentLoadedText;
    [SerializeField] public GameObject progressDialog;
    private AsyncOperation loadingOperation = null;

    private void Start()
    {
        for (int i = 0; i < levelsButtons.Length; i++)
        {
            Button button = levelsButtons[i];
            int buttonIndex = i;
            button.onClick.AddListener(() => ButtonLevelClickedAsync(buttonIndex));
        }
    }
    private void Update()
    {
        if(loadingOperation != null)
        {
            float progressValue = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            percentLoadedText.text = Mathf.Round(progressValue * 100) + "%";
        }
    }
    void ButtonLevelClickedAsync(int index)
    {
        progressDialog.SetActive(true);
        loadingOperation = SceneManager.LoadSceneAsync(Levels.FIRST_LEVEL + index);
    }


   public void ExitGame()
    {
        Application.Quit();
    }

    void OpenAboutScreen()
    {

    }
    
    void OpenSettingsScreen()
    {

    }
}
