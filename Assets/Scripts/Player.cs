using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float offsetY = 0.5f;

    public float GetOffsetY()
    {
        return offsetY;
    }
}
