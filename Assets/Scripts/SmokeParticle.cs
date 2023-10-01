using UnityEngine;

public class SmokeParticle : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 2f);
    }
}
