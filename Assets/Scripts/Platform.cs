using System.Collections.Generic;
using System.Linq;
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

    public List<GridPosition> GetPlayerMoves(Player player, bool includeUnstable = false)
    {
        List<GridPosition> playerMoves = new List<GridPosition>();
        GridPosition position = player.GetGridPosition();

        GridPosition positionN = new GridPosition(position.x, position.y + 1);
        if (IsTileAtGridPosition(positionN, includeUnstable)) playerMoves.Add(positionN);

        GridPosition positionE = new GridPosition(position.x + 1, position.y);
        if (IsTileAtGridPosition(positionE, includeUnstable)) playerMoves.Add(positionE);

        GridPosition positionS = new GridPosition(position.x, position.y - 1);
        if (IsTileAtGridPosition(positionS, includeUnstable)) playerMoves.Add(positionS);

        GridPosition positionW = new GridPosition(position.x - 1, position.y);
        if (IsTileAtGridPosition(positionW, includeUnstable)) playerMoves.Add(positionW);

        return playerMoves;
    }

    public Vector3 GetWorldPositionFromGridPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x - PLATFORM_WIDTH / 2f, 0f, gridPosition.y - PLATFORM_HEIGHT / 2f);
    }

    public void MarkUnstableTiles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (stablePlatformTiles.Count <= 1)
            {
                break;
            }

            int tileIndex = Mathf.FloorToInt(Random.value * stablePlatformTiles.Count);
            PlatformTile tile = stablePlatformTiles.ElementAt(tileIndex);
            unstablePlatformTiles.Add(tile);
            stablePlatformTiles.Remove(tile);
            tile.SetIsShaking(true);
        }
    }

    public bool IsTileUnstable(GridPosition gridPosition)
    {
        foreach (PlatformTile tile in unstablePlatformTiles)
        {
            if (gridPosition.x == tile.GetGridPosition().x && gridPosition.y == tile.GetGridPosition().y)
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
            // TODO: PlatformTile function to destroy and spawn particles
        }
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

    private bool IsTileAtGridPosition(GridPosition gridPosition, bool includeUnstable)
    {
        foreach (PlatformTile tile in stablePlatformTiles)
        {
            if (gridPosition.x == tile.GetGridPosition().x && gridPosition.y == tile.GetGridPosition().y)
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
            if (gridPosition.x == tile.GetGridPosition().x && gridPosition.y == tile.GetGridPosition().y)
            {
                return true;
            }
        }

        return false;
    }
}
