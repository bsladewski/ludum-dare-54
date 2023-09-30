using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    [SerializeField]
    private Platform platform;

    [SerializeField]
    private int unstableTilesPerTurn = 2;

    [SerializeField]
    private SelectionHint selectionHintPrefab;

    [SerializeField]
    private LayerMask selectionLayerMask;

    public event EventHandler<GameState> OnGameStateChanged;

    private HashSet<SelectionHint> selectionHints;

    private GameState gameState;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Singleton GameSystem already exists!");
        }
        Instance = this;
        gameState = GameState.TurnInit;
        selectionHints = new HashSet<SelectionHint>();
    }

    private void Start()
    {
        AdvanceState();
    }

    private void Update()
    {
        if (gameState == GameState.MoveSelection)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, selectionLayerMask))
                {
                    SelectionHint selectionHint = hit.transform.GetComponent<SelectionHint>();
                    foreach (SelectionHint otherSelectionHint in selectionHints)
                    {
                        if (otherSelectionHint != selectionHint)
                        {
                            otherSelectionHint.SetIsSelected(false);
                        }
                    }

                    if (selectionHint.GetIsSelected())
                    {
                        selectionHint.SetIsSelected(false);
                        platform.GetPlayer().SetSelectedMove(null);
                    }
                    else
                    {
                        selectionHint.SetIsSelected(true);
                        platform.GetPlayer().SetSelectedMove(selectionHint.GetGridPosition());
                    }
                }
            }
        }
    }

    public void AdvanceState()
    {
        switch (gameState)
        {
            case GameState.TurnInit:
                HandleTurnInit();
                break;
            case GameState.MoveSelection:
                HandleMoveSelection();
                break;
        }
    }

    private void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
        OnGameStateChanged?.Invoke(this, gameState);
    }

    private void HandleTurnInit()
    {
        platform.MarkUnstableTiles(unstableTilesPerTurn);
        CalculateBotMoves();
        ShowPlayerMoveOptions();
        SetGameState(GameState.MoveSelection);
    }

    private void HandleMoveSelection()
    {
        ClearSelectionHints();
        SetGameState(GameState.MoveExecution);
    }

    private void ClearSelectionHints()
    {
        foreach (SelectionHint selectionHint in selectionHints)
        {
            Destroy(selectionHint.gameObject);
        }

        selectionHints.Clear();
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
                int moveIndex = Mathf.FloorToInt(UnityEngine.Random.value * moves.Count);
                bot.SetSelectedMove(moves[moveIndex]);
            }
        }
    }

    private void ShowPlayerMoveOptions()
    {
        ClearSelectionHints();
        Player player = platform.GetPlayer();
        List<GridPosition> moves = platform.GetPlayerMoves(player, true);
        Debug.Log(moves.Count);
        foreach (GridPosition move in moves)
        {
            Vector3 moveWorldPosition = platform.GetWorldPositionFromGridPosition(move);
            SelectionHint selectionHint = Instantiate(selectionHintPrefab, moveWorldPosition, Quaternion.identity);
            selectionHint.SetGridPosition(move);
            selectionHints.Add(selectionHint);
        }
    }
}
