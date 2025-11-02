using Unity.VisualScripting;
using UnityEngine;

public class Resource : MonoBehaviour
{
    // Stats
    public int regenTime = 5;
    public int dayDestroyed = 0;
    public int respawnDay = 0;
    public int resourceMin = 2;
    public int resourceMax = 5;
    public int resourceType = 0; // 0 for wood, 1 for stone
    public int typeID;

    // Current state
    public bool isDestroyed = false;
    public bool isColliding = true;
    private bool isHovered = false;

    // Current location
    public Vector2 location;

    // Game manager
    public GameManager manager;

    // Object data to edit
    public Collider2D colliderSystem;
    public SpriteRenderer rendererSystem;
    private Color spriteColor = Color.white;
    public Rigidbody2D rb;
    private Vector2 size = new Vector2(1, 1);

    // The house the player is given at the start of the game
    public Building startingHouse;

    // Timer to see if the object has recently been placed
    float timer = 0f;
    bool isPlaced = false;

    // Settings
    public Settings settings;

    // Set settings object
    private void Awake()
    {
        settings = GameObject.Find("Settings").GetComponent<Settings>();
    }

    // Set basic variables
    public void GenerateResource()
    {
        // Enable collisions and make sure it doesn't move
        colliderSystem.enabled = true;
        rendererSystem.enabled = true;

        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Go to random location
        Move();
    }

    // Go to a random location
    private void Move()
    {
        location = new Vector3(Random.Range(manager.leftX, manager.rightX), Random.Range(manager.bottomY, manager.topY), 0.0f);
        location = new Vector3(((int)(location.x * (1 / manager.scaleFactor)) * manager.scaleFactor), ((int)(location.y * (1 / manager.scaleFactor)) * manager.scaleFactor), 0f);
        transform.position = location;
    }

    // Destroy item
    public void DestroyResource()
    {
        // If already gone
        if (isDestroyed) return;

        // Check food
        if ((typeID == 0 || typeID == 3) && manager.food < 1) { return; }
        if ((typeID == 1 || typeID == 4) && manager.food < 3) { return; }
        if ((typeID == 2 || typeID == 5) && manager.food < 5) { return; }

        // Set respawn data
        dayDestroyed = manager.day;
        isDestroyed = true;
        respawnDay = dayDestroyed + regenTime;

        // Increase stats
        if (resourceType == 0) { manager.wood += (int)(Random.Range(resourceMin, resourceMax) * (1f / settings.difficultyScaler)); }
        else { manager.stone += (int)(Random.Range(resourceMin, resourceMax) * (1f / settings.difficultyScaler)); }
        
        // Decrease food
        if (typeID == 0 || typeID == 3) { manager.food -= 1; }
        if (typeID == 1 || typeID == 4) { manager.food -= 2; }
        if (typeID == 2 || typeID == 5) { manager.food -= 3; }

        // Hide and disable collisions
        colliderSystem.enabled = false;
        rendererSystem.enabled = false;
    }

    // Manage Respawn
    public void Update()
    {
        // Check if it has been recently spawned in
        if (!isPlaced)
        {
            timer += Time.deltaTime; // Increment timer
            if (timer >= 2)
            {
                isPlaced = true;
            }
        }

        // Update stats
        UpdateStats();

        // Lock location
        transform.position = location;

        // If time to respawn
        if (manager.day == respawnDay && isDestroyed)
        {
            // Appear and turn off collisions
            colliderSystem.enabled = true;
            rendererSystem.enabled = true;
            isDestroyed = false;
        }

        // If not time to respawn yet
        if (!isDestroyed && !manager.currentlyBuilding && isColliding && isPlaced)
        {
            isDestroyed = true;
            colliderSystem.enabled = false;
            rendererSystem.enabled = false;
            respawnDay += regenTime;
        }

        // Check click
        if (Input.GetMouseButtonDown(0) && isHovered)
        {
            if (manager.demoMode)
            {
                DestroyResource(); // Destroy
            }
        }

        //Colors to differentiate
        Color newColor;

        // Turn red when colliding with something or ready to be destroyed
        if (isColliding || (isHovered && manager.demoMode))
        {
            float red = ((spriteColor.r * 3f) + 1.0f) / 4f;
            float green = (spriteColor.g * 3f) / 4f;
            float blue = (spriteColor.b * 3f) / 4f;
            newColor = new Color(red, green, blue);
        }
        else
        {
            newColor = spriteColor;
        }

        // Set color and scale
        rendererSystem.color = newColor;
        transform.localScale = new Vector3(manager.scaleFactor * size.x, manager.scaleFactor * size.y, 1);
    }

    // When it collides with something, make a note
    public void OnCollisionEnter2D(Collision2D collision) 
    {
        isColliding = true;
        if (!manager.currentlyBuilding && !isPlaced) { Move(); } // Move is currently spawning in
    }
    public void OnCollisionStay2D(Collision2D collision)
    {
        isColliding = true;
        if (!manager.currentlyBuilding && !isPlaced) { Move(); } // Move is currently spawning in
    }

    // When it stops colliding with something, make a note
    public void OnCollisionExit2D(Collision2D collision) { isColliding = false; }

    // Check if hovered
    private void OnMouseOver()
    {
        isHovered = true;

        if (manager.demoMode)
        {
            if (resourceType == 0)
            {
                settings.cAxe = true;
            }
            else
            {
                settings.cPick = true;
            }
        }
    }
    private void OnMouseExit()
    {
        isHovered = false;
        settings.cAxe = false;
        settings.cPick = false;
    }

    // Set resource type
    // 0-2 = S, M, L Tree
    // 3-5 = S, M, L Rock
    private void UpdateStats()
    {
        switch (typeID)
        {
            case 0:
                regenTime = (int)(3 * settings.difficultyScaler);
                resourceMin = (int)(2 * (1 / settings.difficultyScaler));
                resourceMax = (int)(5 * (1 / settings.difficultyScaler));
                resourceType = 0;
                spriteColor = new Color(.5f, 1f, .5f);
                size = new Vector2(1, 1);
                break;

            case 1:
                regenTime = (int)(8 * settings.difficultyScaler);
                resourceMin = (int)(5 * (1 / settings.difficultyScaler));
                resourceMax = (int)(8 * (1 / settings.difficultyScaler));
                resourceType = 0;
                spriteColor = new Color(.25f, .75f, .25f);
                size = new Vector2(2, 2);
                break;

            case 2:
                regenTime = (int)(12 * settings.difficultyScaler);
                resourceMin = (int)(8 * (1 / settings.difficultyScaler));
                resourceMax = (int)(12 * (1 / settings.difficultyScaler));
                resourceType = 0;
                spriteColor = new Color(0f, .5f, 0f);
                size = new Vector2(3, 3);
                break;

            case 3:
                regenTime = (int)(8 * settings.difficultyScaler);
                resourceMin = (int)(2 * (1 / settings.difficultyScaler));
                resourceMax = (int)(5 * (1 / settings.difficultyScaler));
                resourceType = 1;
                spriteColor = new Color(.75f, .75f, .75f);
                size = new Vector2(1, 2);
                break;

            case 4:
                regenTime = (int)(12 * settings.difficultyScaler);
                resourceMin = (int)(5 * (1 / settings.difficultyScaler));
                resourceMax = (int)(8 * (1 / settings.difficultyScaler));
                resourceType = 1;
                spriteColor = new Color(.5f, .5f, .5f);
                size = new Vector2(2, 3);
                break;

            case 5:
                regenTime = (int)(18 * settings.difficultyScaler);
                resourceMin = (int)(8 * (1 / settings.difficultyScaler));
                resourceMax = (int)(12 * (1 / settings.difficultyScaler));
                resourceType = 1;
                spriteColor = new Color(.25f, .25f, .25f);
                size = new Vector2(3, 4);
                break;
        }
    }
}
