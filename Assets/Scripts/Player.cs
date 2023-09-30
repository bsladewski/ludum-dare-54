using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float offsetY = 0.5f;

    private GridPosition gridPosition;

    private GridPosition? selectedMove;

    public float GetOffsetY()
    {
        return offsetY;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public void SetGridPosition(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public GridPosition? GetSelectedMove()
    {
        return selectedMove;
    }

    public void SetSelectedMove(GridPosition? selectedMove)
    {
        this.selectedMove = selectedMove;
    }
}
