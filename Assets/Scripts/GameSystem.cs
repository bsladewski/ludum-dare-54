using System.Collections.Generic;
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

    private void Start()
    {
        AdvanceState();
    }

    public void AdvanceState()
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
        CalculateBotMoves();
        gameState = GameState.MoveSelection;
    }

    private void CalculateBotMoves()
    {
        List<Player> bots = platform.GetBots();
        foreach (Player bot in bots)
        {
            List<GridPosition> moves = platform.GetPlayerMoves(bot);
            if (platform.IsTileUnstable(bot.GetGridPosition()) && (moves == null || moves.Count == 0))
            {
                // if the bot is on an unstable tile they should always move, even onto another
                // unstable tile, for the drama
                moves = platform.GetPlayerMoves(bot, true);
            }

            if (moves == null || moves.Count == 0)
            {
                bot.SetSelectedMove(null);
            }
            else
            {
                int moveIndex = Mathf.FloorToInt(Random.value * moves.Count);
                bot.SetSelectedMove(moves[moveIndex]);
            }
        }
    }
}
