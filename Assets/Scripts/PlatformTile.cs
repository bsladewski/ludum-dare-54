using UnityEngine;

public class PlatformTile : MonoBehaviour
{
    [SerializeField]
    private Transform platformTileVisual;

    [SerializeField]
    private float shakeAmount = 0.02f;

    private bool isShaking;

    private Vector3 platformTileVisualPosition;

    private GridPosition gridPosition;

    private void Start()
    {
        platformTileVisualPosition = platformTileVisual.transform.position;
    }

    private void Update()
    {
        if (isShaking)
        {
            Vector3 shakePosition = platformTileVisualPosition;
            shakePosition += Random.insideUnitSphere * shakeAmount;
            platformTileVisual.transform.position = shakePosition;
        }
    }

    public void SetGridPosition(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public void SetIsShaking(bool isShaking = true)
    {
        this.isShaking = isShaking;
    }

    public bool GetIsShaking()
    {
        return isShaking;
    }
}
