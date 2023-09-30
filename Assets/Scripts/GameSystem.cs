using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    [SerializeField]
    private Platform platform;

    [SerializeField]
    private int unstableTilesPerTurn = 2;

    private GameState gameState;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Singleton GameSystem already exists!");
        }
        Instance = this;
        gameState = GameState.TurnInit;
    }

    private void Update()
    {
        switch (gameState)
        {
            case GameState.TurnInit:
                HandleTurnInit();
                break;
        }
    }

    private void HandleTurnInit()
    {
        platform.MarkUnstableTiles(unstableTilesPerTurn);
        gameState = GameState.MoveSelection;
    }
}
