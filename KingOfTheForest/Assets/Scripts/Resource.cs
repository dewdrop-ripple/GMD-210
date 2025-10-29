using UnityEngine;
using UnityEngine.UIElements;

public class Resource : MonoBehaviour
{
    public int regenTime = 5;
    public int dayDestroyed = 0;
    public int respawnDay = 0;
    public int resourceMin = 2;
    public int resourceMax = 5;
    public int resourceType = 0; // 0 for wood, 1 for stone
    public bool isDestroyed = false;
    public bool isColliding = true;
    private bool isHovered = false;
    public Vector2 location;
    private Vector3 actualLocation;
    public GameManager manager;
    public Collider2D colliderSystem;
    public SpriteRenderer rendererSystem;
    private Color spriteColor = Color.white;
    public Rigidbody2D rb;
    private Vector2 size = new Vector2(1, 1);
    public Building startingHouse;
    //bool checkCollision = true;
    float timer = 0f;
    //bool checkingRespawn = false;
    //bool readyToRespawn = false;
    bool isPlaced = false;

    // Set basic variables
    public void GenerateResource()
    {
        //checkCollision = true;
        //timer = 0f;

        colliderSystem.enabled = true;
        rendererSystem.enabled = true;

        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        Move();
    }

    private void Move()
    {
        location = new Vector3(Random.Range(0.0f, Screen.width), Random.Range(0.0f, Screen.height - 100), 0.0f);
        location = new Vector3(((int)(location.x * (1 / manager.scaleFactor)) * manager.scaleFactor), ((int)(location.y * (1 / manager.scaleFactor)) * manager.scaleFactor), 0f);
        actualLocation = Camera.main.ScreenToWorldPoint(location);
        actualLocation.z = 0.0f;
        transform.position = actualLocation;
    }

    // Destroy item
    public void DestroyResource()
    {
        if (isDestroyed) return;

        dayDestroyed = manager.day;
        isDestroyed = true;
        respawnDay = dayDestroyed + regenTime;

        if (resourceType == 0) { manager.wood += (int)Random.Range(resourceMin, resourceMax); }
        else { manager.stone += (int)Random.Range(resourceMin, resourceMax); }
        manager.food--;

        colliderSystem.enabled = false;
        rendererSystem.enabled = false;

        Debug.Log("Destroyed");
    }

    // Manage Respawn
    public void Update()
    {
        if (!isPlaced)
        {
            timer += Time.deltaTime; // Increment timer
            if (timer >= 2)
            {
                isPlaced = true;
            }
        }
        /*       if (checkCollision)
        {
            timer += Time.deltaTime; // Increment timer
            if (timer >= 0.25)
            {
                checkCollision = false;

                if (checkingRespawn)
                {
                    checkingRespawn = false;
                    readyToRespawn = true;
                }
            }
        }
        */

        transform.position = actualLocation;

        if (manager.day == respawnDay && isDestroyed)
        {
            colliderSystem.enabled = true;
            rendererSystem.enabled = true;
            isDestroyed = false;
        }

        if (!isDestroyed && !manager.currentlyBuilding && isColliding && isPlaced)
        {
            isDestroyed = true;
            colliderSystem.enabled = false;
            rendererSystem.enabled = false;
            respawnDay += regenTime;
        }

        /*
        if (manager.day == respawnDay && isDestroyed && !checkingRespawn && !readyToRespawn)
        {
            colliderSystem.enabled = true;
            checkingRespawn = true;
            timer = 0;
        }

        if (readyToRespawn)
        {
            if (!isColliding)
            {
                rendererSystem.enabled = true;
                isDestroyed = false;
            }
            else
            {
                colliderSystem.enabled = false;
                respawnDay += regenTime;
            }

            readyToRespawn = false;  
        }
        */

        // Check click
        if (Input.GetMouseButtonDown(0) && isHovered)
        {
            if (manager.demoMode)
            {
                DestroyResource();
            }
        }

        //Colors to differentiate
        rendererSystem.color = spriteColor;
        transform.localScale = new Vector3(manager.scaleFactor * size.x, manager.scaleFactor * size.y, 1);
    }

    // When it collides with something, make a note
    public void OnCollisionEnter2D(Collision2D collision) 
    {
        isColliding = true;
        //if (checkCollision) Move();
        if (!manager.currentlyBuilding && !isPlaced) { Move(); }
    }
    public void OnCollisionStay2D(Collision2D collision)
    {
        isColliding = true;
        //if (checkCollision) Move();
        if (!manager.currentlyBuilding && !isPlaced) { Move(); }
    }

    // When it stops colliding with something, make a note
    public void OnCollisionExit2D(Collision2D collision) { isColliding = false; }

    // Check if hovered
    private void OnMouseOver()
    {
        isHovered = true;
    }
    private void OnMouseExit()
    {
        isHovered = false;
    }

    // Set resource type
    // 0-2 = S, M, L Tree
    // 3-5 = S, M, L Rock
    public void setResourceType(int type)
    {
        switch (type)
        {
            case 0:
                regenTime = 5;
                resourceMin = 2;
                resourceMax = 5;
                resourceType = 0;
                spriteColor = new Color(.5f, 1f, .5f);
                size = new Vector2(1, 1);
                break;

            case 1:
                regenTime = 10;
                resourceMin = 5;
                resourceMax = 8;
                resourceType = 0;
                spriteColor = new Color(.25f, .75f, .25f);
                size = new Vector2(2, 2);
                break;

            case 2:
                regenTime = 15;
                resourceMin = 8;
                resourceMax = 12;
                resourceType = 0;
                spriteColor = new Color(0f, .5f, 0f);
                size = new Vector2(3, 3);
                break;

            case 3:
                regenTime = 15;
                resourceMin = 2;
                resourceMax = 5;
                resourceType = 1;
                spriteColor = new Color(.75f, .75f, .75f);
                size = new Vector2(1, 2);
                break;

            case 4:
                regenTime = 20;
                resourceMin = 5;
                resourceMax = 8;
                resourceType = 1;
                spriteColor = new Color(.5f, .5f, .5f);
                size = new Vector2(2, 3);
                break;

            case 5:
                regenTime = 25;
                resourceMin = 8;
                resourceMax = 12;
                resourceType = 1;
                spriteColor = new Color(.25f, .25f, .25f);
                size = new Vector2(3, 4);
                break;
        }
    }
}
