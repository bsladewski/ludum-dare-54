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

    [SerializeField]
    private float turnMoveTime = 1f;

    public event EventHandler<GameState> OnGameStateChanged;

    private HashSet<SelectionHint> selectionHints;

    private GameState gameState;

    private float moveStartTime;

    private List<Player> collisionsToResolve;

    private float collisionStartTime;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Singleton GameSystem already exists!");
        }
        Instance = this;
        gameState = GameState.TurnInit;
        selectionHints = new HashSet<SelectionHint>();
        collisionsToResolve = new List<Player>();
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
                    if (selectionHint == null)
                    {
                        return;
                    }

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

        if (gameState == GameState.MoveExecution)
        {
            Player player = platform.GetPlayer();
            MovePlayer(player);

            List<Player> bots = platform.GetBots();
            foreach (Player bot in bots)
            {
                MovePlayer(bot);
            }

            if (Time.time - moveStartTime >= 1f)
            {
                AdvanceState();
            }
        }

        if (gameState == GameState.CollisionResolution)
        {
            if (collisionsToResolve.Count == 0)
            {
                AdvanceState();
            }

            foreach (Player player in collisionsToResolve)
            {
                MovePlayer(player);
            }

            if (Time.time - moveStartTime >= 1f)
            {
                CalculateCollisionResolutions();
                collisionStartTime = Time.time;
            }
        }
    }

    public void AdvanceState()
    {
        switch (gameState)
        {
            case GameState.TurnInit:
                HandleTurnInitEnd();
                break;
            case GameState.MoveSelection:
                HandleMoveSelectionEnd();
                break;
            case GameState.MoveExecution:
                HandleMoveExecutionEnd();
                break;
            case GameState.CollisionResolution:
                HandleCollisionResolutionEnd();
                break;
        }
    }

    private void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
        OnGameStateChanged?.Invoke(this, gameState);
    }

    private void HandleTurnInitEnd()
    {
        platform.MarkUnstableTiles(unstableTilesPerTurn);
        CalculateBotMoves();
        ShowPlayerMoveOptions();
        SetGameState(GameState.MoveSelection);
    }

    private void HandleMoveSelectionEnd()
    {
        ClearSelectionHints();
        moveStartTime = Time.time;
        SetGameState(GameState.MoveExecution);
    }

    private void HandleMoveExecutionEnd()
    {
        CalculateCollisionResolutions();
        collisionStartTime = Time.time;
        SetGameState(GameState.CollisionResolution);
    }

    private void HandleCollisionResolutionEnd()
    {
        UpdatePlayerPosition(platform.GetPlayer());
        foreach (Player bot in platform.GetBots())
        {
            UpdatePlayerPosition(bot);
        }

        EndTurn();
        SetGameState(GameState.TurnInit);
        AdvanceState(); // barf
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
        foreach (GridPosition move in moves)
        {
            Vector3 moveWorldPosition = platform.GetWorldPositionFromGridPosition(move);
            SelectionHint selectionHint = Instantiate(selectionHintPrefab, moveWorldPosition, Quaternion.identity);
            selectionHint.SetGridPosition(move);
            selectionHints.Add(selectionHint);
        }
    }

    private void MovePlayer(Player player)
    {
        if (!player.GetSelectedMove().HasValue)
        {
            return;
        }

        Vector3 targetPosition = platform.GetWorldPositionFromGridPosition(player.GetSelectedMove().Value);
        targetPosition += Vector3.up * player.GetOffsetY();
        float time = Time.time - moveStartTime;
        player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, time / turnMoveTime);
    }

    private void CalculateCollisionResolutions()
    {
        collisionsToResolve.Clear();

        // TODO: find collisions and update selected move to resolve collisions (use map so each player only collides with a single other player)
    }

    private void UpdatePlayerPosition(Player player)
    {
        GridPosition? selectedMove = player.GetSelectedMove();
        if (!selectedMove.HasValue)
        {
            return;
        }

        player.SetGridPosition(selectedMove.Value);
        player.SetSelectedMove(null);
    }

    private void EndTurn()
    {
        platform.DestroyUnstableTiles();
        Player player = platform.GetPlayer();
        if (!platform.ValidatePlayerPosition(player))
        {
            platform.EliminatePlayer(player);
            // TODO: end game state, player loses
        }

        foreach (Player bot in platform.GetBots())
        {
            if (!platform.ValidatePlayerPosition(bot))
            {
                platform.EliminatePlayer(bot);
            }
        }
    }
}
