using UnityEngine;
using UnityEngine.UI;

public class NextTurnButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    private void Awake()
    {
        GameSystem.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    public void ExecuteTurn()
    {
        GameSystem.Instance.AdvanceState();
    }

    private void OnGameStateChanged(object sender, GameState gameState)
    {
        if (gameState == GameState.MoveSelection)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }
}
