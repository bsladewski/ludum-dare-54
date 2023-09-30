using UnityEngine;

public class SelectionHint : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Color selectedColor;

    private bool isSelected;

    private GridPosition gridPosition;

    private Color defaultColor;

    private void Awake()
    {
        defaultColor = meshRenderer.material.GetColor("_Color");
    }

    public bool GetIsSelected()
    {
        return isSelected;
    }

    public void SetIsSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        if (isSelected)
        {
            meshRenderer.material.SetColor("_Color", selectedColor);
        }
        else
        {
            meshRenderer.material.SetColor("_Color", defaultColor);
        }
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public void SetGridPosition(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }
}
