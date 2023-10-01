using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI titleText;

    public void SetTitleText(string text)
    {
        titleText.text = text;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
