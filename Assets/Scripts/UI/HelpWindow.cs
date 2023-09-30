using UnityEngine;

public class HelpWindow : MonoBehaviour
{
    public void OpenHelpWindow()
    {
        gameObject.SetActive(true);
    }

    public void CloseHelpWindow()
    {
        gameObject.SetActive(false);
    }
}
