using UnityEngine;

public class Building : MonoBehaviour
{
    public int stoneCost;
    public int woodCost;
    public int maxNum;
    private bool isColliding = false;
    private bool isBuilding = false;
    int buildingArrayPosition;
    public GameManager manager;
    bool isHovered = false;
    private Color spriteColor = Color.white;
    public SpriteRenderer rendererSystem;
    public bool built = false;
    public Rigidbody2D rb;
    private Vector3 location;

    // Updates data and returns true if it can be built
    public void StartBuild()
    {
        isBuilding = true;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    // Check if we can build the thing and, if so, build it
    public void EndBuild()
    {
        isBuilding = false;

        // If any issues, don't build
        if (isColliding || manager.buildings[buildingArrayPosition] >= maxNum)
        {
            //Debug.Log("Can't Build");
            GameObject.Destroy(gameObject);
            return;
        }

        //Debug.Log("Built");

        // Update values
        manager.wood -= woodCost;
        manager.stone -= stoneCost;
        manager.buildings[buildingArrayPosition]++;
        location = transform.position;

        built = true;
    }

    // Destroy building
    public void DestroyBuilding()
    {
        //Debug.Log("Destroyed");
        // Update Values
        if (built)
        {
            manager.wood += woodCost / 2;
            manager.stone += stoneCost / 2;
            manager.buildings[buildingArrayPosition]--;
            manager.food--;
        }

        // Destroy
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
        transform.position = location;

        // Go to mouse pointer
        if (isBuilding)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            transform.position = mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                EndBuild();
            }
        }

        // Check click
        if (Input.GetMouseButtonDown(0) && isHovered)
        {
            if (manager.demoMode)
            {
                DestroyBuilding();
            }
        }

        //Colors to differentiate
        rendererSystem.color = spriteColor;
    }

    // Check if hovered
    private void OnMouseOver()
    {
        isHovered = true;
    }
    private void OnMouseExit()
    {
        isHovered = false;
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
                stoneCost = 10;
                woodCost = 10;
                maxNum = 10;
                spriteColor = new Color(1.0f, 0.75f, 0.0f);
                break;

            case 1:
                stoneCost = 20;
                woodCost = 20;
                maxNum = 10;
                spriteColor = new Color(0.75f, 0.5f, 0.0f);
                break;

            case 2:
                stoneCost = 50;
                woodCost = 50;
                maxNum = 10;
                spriteColor = new Color(0.5f, 0.25f, 0.0f);
                break;

            case 3:
                stoneCost = 0;
                woodCost = 10;
                maxNum = 10;
                spriteColor = new Color(.5f, 1f, .5f);
                break;

            case 4:
                stoneCost = 0;
                woodCost = 20;
                maxNum = 10;
                spriteColor = new Color(.25f, .75f, .25f);
                break;

            case 5:
                stoneCost = 0;
                woodCost = 50;
                maxNum = 10;
                spriteColor = new Color(0f, .5f, 0f);
                break;

            case 6:
                stoneCost = 5;
                woodCost = 10;
                maxNum = 5;
                spriteColor = new Color(1f, .5f, .5f);
                break;

            case 7:
                stoneCost = 10;
                woodCost = 20;
                maxNum = 5;
                spriteColor = new Color(.75f, .25f, .25f);
                break;

            case 8:
                stoneCost = 25;
                woodCost = 50;
                maxNum = 5;
                spriteColor = new Color(.5f, 0f, 0f);
                break;

            case 9:
                stoneCost = 10;
                woodCost = 0;
                maxNum = 10;
                spriteColor = new Color(.75f, .75f, .75f);
                break;

            case 10:
                stoneCost = 20;
                woodCost = 0;
                maxNum = 10;
                spriteColor = new Color(.5f, .5f, .5f);
                break;

            case 11:
                stoneCost = 50;
                woodCost = 0;
                maxNum = 10;
                spriteColor = new Color(.25f, .25f, .25f);
                break;
        }
    }

    public void preBuilt()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        isBuilding = false;
        location = transform.position;
        built = true;
    }
}
