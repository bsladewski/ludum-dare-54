using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public static readonly int PLATFORM_WIDTH = 7;

    public static readonly int PLATFORM_HEIGHT = 7;

    [SerializeField]
    private PlatformChunk platformChunkPrefab;

    [SerializeField]
    private Player playerPrefab;

    [SerializeField]
    private Player botPrefab;

    private PlatformChunk[,] platformChunks;

    private struct GridPosition
    {
        public int x;
        public int y;

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private List<GridPosition> playerSpawnPositions;

    void Awake()
    {
        platformChunks = new PlatformChunk[PLATFORM_WIDTH, PLATFORM_HEIGHT];
        playerSpawnPositions = new List<GridPosition>();

        // generate platform
        for (int x = 0; x < PLATFORM_WIDTH; x++)
        {
            for (int y = 0; y < PLATFORM_HEIGHT; y++)
            {
                Vector3 worldPosition = GetWorldPositionFromGridPosition(new GridPosition(x, y));
                PlatformChunk platformChunk = Instantiate(platformChunkPrefab, worldPosition, Quaternion.identity);
                RandomizeRotation(platformChunk.transform);
                platformChunk.transform.SetParent(transform);
                platformChunks[x, y] = platformChunk;

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

        // spawn bots
        while (playerSpawnPositions.Count > 0)
        {
            GridPosition botSpawnPosition = GetRandomSpawnPosition();
            Vector3 botWorldPosition = GetWorldPositionFromGridPosition(botSpawnPosition);
            Player bot = Instantiate(botPrefab, botWorldPosition, Quaternion.identity);
            bot.transform.position += Vector3.up * bot.GetOffsetY();
        }
    }

    private Vector3 GetWorldPositionFromGridPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x - PLATFORM_WIDTH / 2f, 0f, gridPosition.y - PLATFORM_HEIGHT / 2f);
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
}
