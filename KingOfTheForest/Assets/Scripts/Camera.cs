using UnityEngine;
using UnityEngine.InputSystem;

public class Camera : MonoBehaviour
{
    public GameManager manager;

    void Update()
    {
        int minX = 0;
        int maxX = Screen.width;
        int minY = 0;
        int maxY = Screen.height;

        Vector3 mousePos = Input.mousePosition;
        //Vector2 actualPos = new Vector2(UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition).x, UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        Debug.Log(mousePos);

        if (mousePos.x < minX + 10 && transform.position.x > manager.leftX + 8)
        {
            transform.position = new Vector3(transform.position.x - 0.01f, transform.position.y, -10f);
            Debug.Log("Move left");
        }
        else if (mousePos.x > maxX - 10 && transform.position.x < manager.rightX - 8)
        {
            transform.position = new Vector3(transform.position.x + 0.01f, transform.position.y, -10f);
            Debug.Log("Move right");
        }

        if (mousePos.y < minY + 10 && transform.position.y > manager.bottomY + 3)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.01f, -10f);
            Debug.Log("Move down");
        }
        else if (mousePos.y > maxY - 10 && transform.position.y < manager.topY - 3)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.01f, -10f);
            Debug.Log("Move up");
        }
    }
}
