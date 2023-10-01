using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public static readonly int PLATFORM_WIDTH = 7;

    public static readonly int PLATFORM_HEIGHT = 7;

    [SerializeField]
    private PlatformTile platformTilePrefab;

    [SerializeField]
    private Player playerPrefab;

    [SerializeField]
    private Player botPrefab;

    [SerializeField]
    private SmokeParticle smokeParticlePrefab;

    private PlatformTile[,] platformTiles;

    private List<GridPosition> playerSpawnPositions;

    private HashSet<PlatformTile> stablePlatformTiles;

    private HashSet<PlatformTile> unstablePlatformTiles;

    private Player player;

    private HashSet<Player> bots;

    void Awake()
    {
        platformTiles = new PlatformTile[PLATFORM_WIDTH, PLATFORM_HEIGHT];
        playerSpawnPositions = new List<GridPosition>();
        stablePlatformTiles = new HashSet<PlatformTile>();
        unstablePlatformTiles = new HashSet<PlatformTile>();
        bots = new HashSet<Player>();

        // generate platform
        for (int x = 0; x < PLATFORM_WIDTH; x++)
        {
            for (int y = 0; y < PLATFORM_HEIGHT; y++)
            {
                GridPosition platformTileGridPosition = new GridPosition(x, y);
                Vector3 worldPosition = GetWorldPositionFromGridPosition(platformTileGridPosition);
                PlatformTile platformTile = Instantiate(platformTilePrefab, worldPosition, Quaternion.identity);
                platformTile.SetGridPosition(platformTileGridPosition);
                RandomizeRotation(platformTile.transform);
                platformTile.transform.SetParent(transform);
                platformTiles[x, y] = platformTile;
                stablePlatformTiles.Add(platformTile);

                // mark possible player spawn positions
                if (
                    (x == 0 || x == PLATFORM_WIDTH / 2 || x == PLATFORM_WIDTH - 1) &&
                    (y == 0 || y == PLATFORM_HEIGHT / 2 || y == PLATFORM_HEIGHT - 1)
                )
                {
                    playerSpawnPositions.Add(new GridPosition(x, y));
                }
            }
        }

        // spawn player
        GridPosition playerSpawnPosition = GetRandomSpawnPosition();
        Vector3 playerWorldPosition = GetWorldPositionFromGridPosition(playerSpawnPosition);
        Player player = Instantiate(playerPrefab, playerWorldPosition, Quaternion.identity);
        player.transform.position += Vector3.up * player.GetOffsetY();
        player.SetGridPosition(playerSpawnPosition);
        this.player = player;

        // spawn bots
        while (playerSpawnPositions.Count > 0)
        {
            GridPosition botSpawnPosition = GetRandomSpawnPosition();
            Vector3 botWorldPosition = GetWorldPositionFromGridPosition(botSpawnPosition);
            Player bot = Instantiate(botPrefab, botWorldPosition, Quaternion.identity);
            bot.transform.position += Vector3.up * bot.GetOffsetY();
            bot.SetGridPosition(botSpawnPosition);
            bots.Add(bot);
        }
    }

    public Player GetPlayer()
    {
        return player;
    }

    public List<Player> GetBots()
    {
        return bots.ToList();
    }

    public void EliminatePlayer(Player player)
    {
        player.SetIsEliminated();
        if (bots.Contains(player))
        {
            bots.Remove(player);
        }
    }

    public bool ValidatePlayerPosition(Player player)
    {
        return IsTileAtGridPosition(player.GetGridPosition(), false);
    }

    public List<GridPosition> GetPlayerMoves(Player player, bool includeUnstable, bool includesOccupied)
    {
        List<GridPosition> playerMoves = new List<GridPosition>();
        List<GridPosition> neighbors = GetNeighbors(player.GetGridPosition());

        foreach (GridPosition gridPosition in neighbors)
        {
            if (IsTileAtGridPosition(gridPosition, includeUnstable) && (includesOccupied || !IsPlayerAtGridPosition(gridPosition)))
            {
                playerMoves.Add(gridPosition);
            }
        }

        return playerMoves;
    }

    public Vector3 GetWorldPositionFromGridPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x - PLATFORM_WIDTH / 2f, 0f, gridPosition.y - PLATFORM_HEIGHT / 2f);
    }

    public void MarkUnstableTiles(int count)
    {
        HashSet<PlatformTile> checkedTiles = new HashSet<PlatformTile>();
        while (unstablePlatformTiles.Count < count && stablePlatformTiles.Count > 1)
        {
            HashSet<PlatformTile> candidateTiles = new HashSet<PlatformTile>();
            candidateTiles.UnionWith(stablePlatformTiles);
            candidateTiles.ExceptWith(checkedTiles);

            int tileIndex = Mathf.FloorToInt(Random.value * candidateTiles.Count);
            PlatformTile tile = candidateTiles.ElementAt(tileIndex);
            checkedTiles.Add(tile);
            if (candidateTiles.Count - unstablePlatformTiles.Count > count)
            {
                if (CheckCreatesIsland(tile.GetGridPosition()))
                {
                    continue;
                }
            }

            unstablePlatformTiles.Add(tile);
            stablePlatformTiles.Remove(tile);
            tile.SetIsShaking(true);
        }
    }

    public bool IsTileUnstable(GridPosition gridPosition)
    {
        foreach (PlatformTile tile in unstablePlatformTiles)
        {
            if (gridPosition.Equals(tile.GetGridPosition()))
            {
                return true;
            }
        }

        return false;
    }

    public void DestroyUnstableTiles()
    {
        foreach (PlatformTile tile in unstablePlatformTiles)
        {
            Instantiate(smokeParticlePrefab, tile.transform.position, Quaternion.identity);
            Destroy(tile.gameObject);
        }

        unstablePlatformTiles.Clear();
    }

    private void RandomizeRotation(Transform transform)
    {
        for (int i = 0; i < 3; i++)
        {
            if (Random.value > 0.5f) break;
            transform.Rotate(new Vector3(90f, 0f, 0f));
        }

        for (int i = 0; i < 3; i++)
        {
            if (Random.value > 0.5f) break;
            transform.Rotate(new Vector3(0f, 90f, 0f));
        }

        for (int i = 0; i < 3; i++)
        {
            if (Random.value > 0.5f) break;
            transform.Rotate(new Vector3(0f, 0f, 90f));
        }
    }

    private GridPosition GetRandomSpawnPosition()
    {
        int spawnPositionIndex = Mathf.FloorToInt(Random.value * playerSpawnPositions.Count);
        GridPosition spawnPosition = playerSpawnPositions[spawnPositionIndex];
        playerSpawnPositions.RemoveAt(spawnPositionIndex);
        return spawnPosition;
    }

    private List<GridPosition> GetNeighbors(GridPosition position)
    {
        return new List<GridPosition>() {
            new GridPosition(position.x, position.y + 1),
            new GridPosition(position.x, position.y - 1),
            new GridPosition(position.x + 1, position.y),
            new GridPosition(position.x - 1, position.y)
        };
    }

    private bool IsTileAtGridPosition(GridPosition gridPosition, bool includeUnstable)
    {
        foreach (PlatformTile tile in stablePlatformTiles)
        {
            if (gridPosition.Equals(tile.GetGridPosition()))
            {
                return true;
            }
        }

        if (!includeUnstable)
        {
            return false;
        }

        foreach (PlatformTile tile in unstablePlatformTiles)
        {
            if (gridPosition.Equals(tile.GetGridPosition()))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPlayerAtGridPosition(GridPosition gridPosition)
    {
        if (gridPosition.Equals(player.GetGridPosition()))
        {
            return true;
        }

        foreach (Player bot in bots)
        {
            if (gridPosition.Equals(bot.GetGridPosition()))
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckCreatesIsland(GridPosition gridPosition)
    {
        List<GridPosition> neighbors = GetNeighbors(gridPosition);
        neighbors = neighbors.Where(neighbor => IsTileAtGridPosition(neighbor, false)).ToList();
        if (neighbors.Count == 0)
        {
            return false;
        }

        HashSet<GridPosition> visited = new HashSet<GridPosition>() { gridPosition };
        AddAllConnectedTiles(neighbors[0], visited);
        return neighbors.Any(neighbor => !visited.Contains(neighbor));
    }

    private void AddAllConnectedTiles(GridPosition gridPosition, HashSet<GridPosition> visited)
    {
        List<GridPosition> neighbors = GetNeighbors(gridPosition);
        neighbors = neighbors.Where(neighbor => !visited.Contains(neighbor) && IsTileAtGridPosition(neighbor, false)).ToList();
        foreach (GridPosition neighbor in neighbors)
        {
            if (visited.Contains(neighbor))
            {
                continue;
            }

            visited.Add(neighbor);
            AddAllConnectedTiles(neighbor, visited);
        }
    }
}
