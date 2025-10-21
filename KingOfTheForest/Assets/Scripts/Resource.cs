using UnityEngine;

public class Resource : MonoBehaviour
{
    public int regenTime;
    public int dayDestroyed = 0;
    public int respawnDay = 0;
    public int resourceMin;
    public int resourceMax;
    public int resourceType; // 0 for wood, 1 for stone
    public bool isDestroyed = false;
    public bool isColliding = false;
    public Vector2 location;
    public GameManager manager;
    public Collider2D colliderSystem;
    public SpriteRenderer rendererSystem;

    // Set basic variables
    private void Start()
    {
        regenTime = 1;
        resourceMin = 1;
        resourceMax = 1;
        resourceType = 0;
        colliderSystem.enabled = true;
        rendererSystem.enabled = true;

        do
        {
            location = new Vector3(Random.Range(0.0f, Screen.width), Random.Range(0.0f, Screen.height), 0.0f);
            Vector3 actualLocation = Camera.main.ScreenToWorldPoint(location);
            actualLocation.z = 0.0f;
            transform.position = actualLocation;
        }
        while (isColliding);

        Debug.Log("Placed");
    }

    // Destroy item
    public void Destroy()
    {
        if (isDestroyed) return;

        dayDestroyed = manager.day;
        isDestroyed = true;
        respawnDay = dayDestroyed + regenTime;

        if (resourceType == 0) { manager.wood += (int)Random.Range(resourceMin, resourceMax); }
        else { manager.stone += (int)Random.Range(resourceMin, resourceMax); }

        colliderSystem.enabled = false;
        rendererSystem.enabled = false;

        Debug.Log("Destroyed");
    }

    // Manage Respawn
    public void Update()
    {
        if (manager.day == respawnDay && isDestroyed)
        {
            colliderSystem.enabled = true;

            if (isColliding)
            {
                colliderSystem.enabled = false;
                respawnDay += regenTime;
                return;
            }

            rendererSystem.enabled = true;
            isDestroyed = false;

            Debug.Log("Respawned");
        }

        //Test();
    }

    // When it collides with something, make a note
    public void OnCollisionEnter2D(Collision2D collision) { isColliding = true; }
    public void OnCollisionStay2D(Collision2D collision) { isColliding = true; }

    // When it stops colliding with something, make a note
    public void OnCollisionExit2D(Collision2D collision) { isColliding = false; }

    /*
    private void Test()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Key Pressed");
            Destroy();
        }
    }
    */
}
