using Unity.VisualScripting;
using UnityEngine;

public class Building : MonoBehaviour
{
    // Stats
    public int stoneCost;
    public int woodCost;
    public int buildingArrayPosition;

    // Current state
    private bool isColliding = false;
    private bool isBuilding = false;
    bool isHovered = false;
    public bool built = false;

    // Game Manager
    public GameManager manager;
    
    // Object data to edit
    private Color spriteColor = Color.white;
    public SpriteRenderer rendererSystem;
    public Rigidbody2D rb;
    private Vector3 location;
    private Vector2 size = new Vector2(1, 1);

    // Check it it has been built recently
    private bool recentlyBuilt = true;
    float timer = 0f;

    // UI
    public Canvas wait;
    public BuildMenu buildMenu;

    // Settings object
    public Settings settings;

    // Get settings object
    private void Awake()
    {
        settings = GameObject.Find("Settings").GetComponent<Settings>();
    }

    // Updates data
    public void StartBuild()
    {
        wait.enabled = false;
        isBuilding = true;
        manager.demoMode = false;
        manager.currentlyBuilding = true;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    // Check if we can build the thing and, if so, build it
    public void EndBuild()
    {
        isBuilding = false;
        buildMenu.isUp = true;

        // If any issues, don't build
        if (isColliding)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        // Update values
        manager.wood -= woodCost;
        manager.stone -= stoneCost;
        manager.buildings[buildingArrayPosition]++;
        location = transform.position;
        manager.buildingsList.Add(this);

        // Update state
        built = true;
        manager.demoMode = true;
    }

    // Destroy building
    public void DestroyBuilding()
    {
        // Update Values
        if (built)
        {
            manager.wood += (int)(woodCost / (2 * settings.difficultyScaler));
            manager.stone += (int)(stoneCost / (2 * settings.difficultyScaler));
            manager.buildings[buildingArrayPosition]--;
            manager.food -= 10;
        }

        // Destroy
        GameObject.Destroy(gameObject);
    }

    // If attacked
    public void AttackBuilding()
    {
        // Destroy
        manager.buildings[buildingArrayPosition]--;
        manager.buildingsList.Remove(this);
        GameObject.Destroy(gameObject);
    }

    // When it collides with something, make a note
    public void OnCollisionEnter2D(Collision2D collision) { isColliding = true; }
    public void OnCollisionStay2D(Collision2D collision) { isColliding = true; }

    // When it stops colliding with something, make a note
    public void OnCollisionExit2D(Collision2D collision) { isColliding = false; }

    // Log current collision status
    // Go to mouse if currently building
    private void Update()
    {
        // Timer to check if built recently
        if (recentlyBuilt && !isBuilding)
        {
            timer += Time.deltaTime;

            if (timer > 0.25)
            {
                recentlyBuilt = false;
            }
        }

        // Lock location
        transform.position = location;

        // Go to mouse pointer while building
        if (isBuilding)
        {
            Vector3 mousePosition = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition = new Vector3(((int)(mousePosition.x * (1 / manager.scaleFactor)) * manager.scaleFactor), ((int)(mousePosition.y * (1 / manager.scaleFactor)) * manager.scaleFactor), 0f);
            transform.position = mousePosition;

            // Build when mouse clicked
            if (Input.GetMouseButtonDown(0))
            {
                EndBuild();
                manager.currentlyBuilding = false;
            }
        }

        // Check click
        if (Input.GetMouseButtonDown(0) && isHovered)
        {
            // Check all the reasons a play may not be able to destroy it
            bool canDestroy = true;

            // Don't destroy only house
            if (buildingArrayPosition == 0 || buildingArrayPosition == 1 || buildingArrayPosition == 2)
            {
                int numHouses = manager.buildings[0] + manager.buildings[1] + manager.buildings[2];
                if (numHouses < 2)
                {
                    canDestroy = false;
                }
            }

            // Don't destroy if not demo mode
            if (!manager.demoMode)
            {
                canDestroy = false;
            }

            // Don't destroy if loading in
            if (recentlyBuilt)
            {
                canDestroy = false;
            }

            if (manager.food < 10)
            {
                canDestroy = false;
            }

            // Destroy
            if (canDestroy)
            {
                wait.enabled = true;
                manager.demoMode = false;
            }
        }

        //Colors to differentiate
        Color newColor;

        // If colliding with something or hovered
        if (isColliding || (isHovered && manager.demoMode && !manager.currentlyBuilding))
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

    // Check if hovered
    private void OnMouseOver()
    {
        isHovered = true;

        if (manager.demoMode)
        {
            settings.cHammer = true;
        }
    }
    private void OnMouseExit()
    {
        isHovered = false;
        settings.cHammer = false;
    }

    // Set Building type
    // 0-2 - S, M, L Houses
    // 3-5 - S, M, L Farms
    // 6-8 - S, M, L Trade Posts
    // 9-11 - S, M, L Towers
    public void setBuildingType(int type)
    {
        buildingArrayPosition = type;

        switch (type)
        {
            case 0:
                stoneCost = (int)(10 * settings.difficultyScaler);
                woodCost = (int)(10 * settings.difficultyScaler);
                spriteColor = new Color(1.0f, 0.75f, 0.0f);
                size = new Vector2(3, 3);
                break;

            case 1:
                stoneCost = (int)(20 * settings.difficultyScaler);
                woodCost = (int)(20 * settings.difficultyScaler);
                spriteColor = new Color(0.75f, 0.5f, 0.0f);
                size = new Vector2(4, 4);
                break;

            case 2:
                stoneCost = (int)(50 * settings.difficultyScaler);
                woodCost = (int)(50 * settings.difficultyScaler);
                spriteColor = new Color(0.5f, 0.25f, 0.0f);
                size = new Vector2(5, 5);
                break;

            case 3:
                stoneCost = 0;
                woodCost = (int)(10 * settings.difficultyScaler);
                spriteColor = new Color(.5f, 1f, .5f);
                size = new Vector2(4, 4);
                break;

            case 4:
                stoneCost = 0;
                woodCost = (int)(20 * settings.difficultyScaler);
                spriteColor = new Color(.25f, .75f, .25f);
                size = new Vector2(5, 5);
                break;

            case 5:
                stoneCost = 0;
                woodCost = (int)(50 * settings.difficultyScaler);
                spriteColor = new Color(0f, .5f, 0f);
                size = new Vector2(6, 6);
                break;

            case 6:
                stoneCost = (int)(5 * settings.difficultyScaler);
                woodCost = (int)(10 * settings.difficultyScaler);
                spriteColor = new Color(1f, .5f, .5f);
                size = new Vector2(1, 2);
                break;

            case 7:
                stoneCost = (int)(10 * settings.difficultyScaler);
                woodCost = (int)(20 * settings.difficultyScaler);
                spriteColor = new Color(.75f, .25f, .25f);
                size = new Vector2(2, 3);
                break;

            case 8:
                stoneCost = (int)(25 * settings.difficultyScaler);
                woodCost = (int)(50 * settings.difficultyScaler);
                spriteColor = new Color(.5f, 0f, 0f);
                size = new Vector2(3, 4);
                break;

            case 9:
                stoneCost = (int)(10 * settings.difficultyScaler);
                woodCost = 0;
                spriteColor = new Color(.75f, .75f, .75f);
                size = new Vector2(1, 1);
                break;

            case 10:
                stoneCost = (int)(20 * settings.difficultyScaler);
                woodCost = 0;
                spriteColor = new Color(.5f, .5f, .5f);
                size = new Vector2(2, 2);
                break;

            case 11:
                stoneCost = (int)(50 * settings.difficultyScaler);
                woodCost = 0;
                spriteColor = new Color(.25f, .25f, .25f);
                size = new Vector2(3, 3);
                break;
        }
    }

    // Since one building is built before game start
    public void preBuilt()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        isBuilding = false;
        manager.demoMode = true;
        location = transform.position;
        built = true;
        wait.enabled = false;
    }

    // For 'do you want to destroy'
    public void YesDestroy()
    {
        wait.enabled = false;
        manager.demoMode = true;
        DestroyBuilding();
    }

    // For 'do you want to destroy'
    public void NoDestroy()
    {
        wait.enabled = false;
        manager.demoMode = true;
    }
}
