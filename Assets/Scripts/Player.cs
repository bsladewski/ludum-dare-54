using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float offsetY = 0.5f;

    private GridPosition gridPosition;

    private GridPosition? selectedMove;

    private bool isEliminated;

    private float speed;

    private float gravity = 0.5f;

    private void Update()
    {
        if (isEliminated)
        {
            speed += gravity * Time.deltaTime;
            transform.position += Vector3.down * speed;
            if (transform.position.y < -100f)
            {
                Destroy(gameObject);
            }
        }
    }

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

    public void SetIsEliminated()
    {
        isEliminated = true;
        animator.SetTrigger("Eliminated");
    }

    public void PlayHitAnimation()
    {
        animator.SetTrigger("Hit");
    }

    public void PlayLungeAnimation()
    {
        animator.SetTrigger("Lunge");
    }
}
