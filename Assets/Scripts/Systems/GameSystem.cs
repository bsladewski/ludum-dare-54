using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    private GameOverMenu gameOverMenu;

    public event EventHandler<GameState> OnGameStateChanged;

    public event EventHandler<EventArgs> OnPlayerCollision;

    public event EventHandler<EventArgs> OnPlayerEliminated;

    public event EventHandler<EventArgs> OnTileDestroyed;

    private HashSet<SelectionHint> selectionHints;

    private GameState gameState;

    private float moveStartTime;

    private float collisionStartTime;

    private struct Collision
    {
        public Player player;

        public GridPosition gridPosition;

        public GridPosition selectedMove;

        public Collision(Player player, GridPosition gridPosition, GridPosition selectedMove)
        {
            this.player = player;
            this.gridPosition = gridPosition;
            this.selectedMove = selectedMove;
        }
    }

    private List<Collision> collisionsToResolve;

    private bool gameOver;

    private int turnCounter;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Singleton GameSystem already exists!");
        }
        Instance = this;
        gameState = GameState.TurnInit;
        selectionHints = new HashSet<SelectionHint>();
        collisionsToResolve = new List<Collision>();
    }

    private void Start()
    {
        AdvanceState();
    }

    private void Update()
    {
        if (gameOver)
        {
            return;
        }

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

            foreach (Collision collision in collisionsToResolve)
            {
                MovePlayer(collision.player);
            }

            if (Time.time - collisionStartTime >= 1f)
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
        turnCounter++;
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
            List<GridPosition> moves = platform.GetPlayerMoves(bot, false);
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

        HashSet<Player> used = new HashSet<Player>();
        List<Player> players = platform.GetBots();
        players.Add(platform.GetPlayer());
        foreach (Player p1 in players)
        {
            foreach (Player p2 in players)
            {
                if (p1 == p2 || used.Contains(p2))
                {
                    continue;
                }

                GridPosition? p1Move = p1.GetSelectedMove();
                GridPosition? p2Move = p2.GetSelectedMove();

                GridPosition p1Pos = p1Move.HasValue ? p1Move.Value : p1.GetGridPosition();
                GridPosition p2Pos = p2Move.HasValue ? p2Move.Value : p2.GetGridPosition();
                GridPosition p2PrevPos = p2.GetGridPosition();

                if (!p1Pos.Equals(p2Pos) || !p2Move.HasValue)
                {
                    continue;
                }

                if (p2Pos.x > p2PrevPos.x) // N
                {
                    collisionsToResolve.Add(new Collision(p1, p1Pos, new GridPosition(p1Pos.x + 1, p1Pos.y)));
                    used.Add(p2);
                }
                else if (p2Pos.x < p2PrevPos.x) // S
                {
                    collisionsToResolve.Add(new Collision(p1, p1Pos, new GridPosition(p1Pos.x - 1, p1Pos.y)));
                    used.Add(p2);
                }
                else if (p2Pos.y > p2PrevPos.y) // E
                {
                    collisionsToResolve.Add(new Collision(p1, p1Pos, new GridPosition(p1Pos.x, p1Pos.y + 1)));
                    used.Add(p2);
                }
                else if (p2Pos.y < p2PrevPos.y) // W
                {
                    collisionsToResolve.Add(new Collision(p1, p1Pos, new GridPosition(p1Pos.x, p1Pos.y - 1)));
                    used.Add(p2);
                }
            }
        }

        foreach (Collision collision in collisionsToResolve)
        {
            collision.player.SetGridPosition(collision.gridPosition);
            collision.player.SetSelectedMove(collision.selectedMove);
        }

        foreach (Player player in players)
        {
            if (!collisionsToResolve.Any(collision => collision.player == player))
            {
                UpdatePlayerPosition(player);
            }
        }

        if (collisionsToResolve.Count > 0)
        {
            OnPlayerCollision?.Invoke(this, EventArgs.Empty);
        }
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
        bool playerEliminated = false;
        bool botEliminated = false;

        platform.DestroyUnstableTiles();
        OnTileDestroyed?.Invoke(this, EventArgs.Empty);

        Player player = platform.GetPlayer();
        if (!platform.ValidatePlayerPosition(player))
        {
            platform.EliminatePlayer(player);
            playerEliminated = true;
        }

        foreach (Player bot in platform.GetBots())
        {
            if (!platform.ValidatePlayerPosition(bot))
            {
                platform.EliminatePlayer(bot);
                botEliminated = true;
            }
        }

        if (playerEliminated || botEliminated)
        {
            OnPlayerEliminated?.Invoke(this, EventArgs.Empty);
        }

        if (playerEliminated || platform.GetBots().Count == 0)
        {
            if (!playerEliminated)
            {
                gameOverMenu.SetTitleText("You Won!");
                int highscore = PlayerPrefs.GetInt("highscore");

                string flavorText = string.Format("You won in {0} turns!", turnCounter);
                if (turnCounter > highscore)
                {
                    PlayerPrefs.SetInt("highscore", turnCounter);
                    PlayerPrefs.Save();
                    flavorText += " This is your new record!";
                }
                else
                {
                    flavorText += string.Format(" Your best time is {0} turns!", highscore);
                }

                gameOverMenu.SetFlavorText(flavorText);
            }
            else
            {
                string flavorText = string.Format("You made it {0} turns!", turnCounter);
                flavorText += " Give it another try!";

                gameOverMenu.SetFlavorText(flavorText);
            }
            StartCoroutine(ShowGameOverMenu(2));
            gameOver = true;
            return;
        }

        SetGameState(GameState.TurnInit);
        AdvanceState();
    }

    private IEnumerator ShowGameOverMenu(int delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        gameOverMenu.Show();
    }
}
