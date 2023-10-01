using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI flavorText;

    public void SetTitleText(string text)
    {
        titleText.text = text;
    }

    public void SetFlavorText(string text)
    {
        flavorText.text = text;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
