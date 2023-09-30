using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 3f;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            transform.position = new Vector3(player.transform.position.x, 0f, player.transform.position.z);
        }
    }

    private void Update()
    {
        float moveX = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            moveX -= 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveX += 1f;
        }

        float moveZ = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            moveZ += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveZ -= 1f;
        }

        float newX = transform.position.x + moveX * moveSpeed * Time.deltaTime;
        newX = Mathf.Clamp(newX, -Platform.PLATFORM_WIDTH / 2f, Platform.PLATFORM_WIDTH / 2f);
        float newZ = transform.position.z + moveZ * moveSpeed * Time.deltaTime;
        newZ = Mathf.Clamp(newZ, -Platform.PLATFORM_HEIGHT / 2f, Platform.PLATFORM_HEIGHT / 2f);
        transform.position = new Vector3(newX, transform.position.y, newZ);
    }
}
