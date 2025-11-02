using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Game Manager
    public GameManager manager;

    void Update()
    {
        // Camera bounds
        int minX = 0;
        int maxX = Screen.width;
        int minY = 0;
        int maxY = Screen.height;

        // Get position
        Vector3 mousePos = Input.mousePosition;

        // Move left/right
        if (mousePos.x < minX + 10 && transform.position.x > manager.leftX + 8)
        {
            transform.position = new Vector3(transform.position.x - 0.01f, transform.position.y, -10f);
        }
        else if (mousePos.x > maxX - 10 && transform.position.x < manager.rightX - 8)
        {
            transform.position = new Vector3(transform.position.x + 0.01f, transform.position.y, -10f);
        }

        // Move up/down
        if (mousePos.y < minY + 10 && transform.position.y > manager.bottomY + 3)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.01f, -10f);
        }
        else if (mousePos.y > maxY - 10 && transform.position.y < manager.topY - 3)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.01f, -10f);
        }
    }
}
