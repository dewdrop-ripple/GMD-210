using TMPro;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    // Diffrent Position
    private const int UP_POSITION = 170;
    private const int DOWN_POSITION = 365;
    public bool isUp = false;

    // Panels
    public UnityEngine.UI.Image basePanel;
    public UnityEngine.UI.Image tabPanel;
    public UnityEngine.UI.Image buyButton;
    public UnityEngine.UI.Image lowFoodPanel;

    // Button Colors
    private Color GREEN = new Color(.5f, .75f, .5f);
    private Color RED = new Color(.75f, .5f, .5f);

    // Used to edit text
    private int woodCost;
    private int stoneCost;
    private int foodCost;
    public TextMeshProUGUI description;
    public TextMeshProUGUI tabText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI titleText;
    public int buildingSelection = 0;

    // Game manager to get and edit data
    public GameManager manager;

    // Building so it can actually be built
    public GameObject building;

    // Can the building be purchased?
    private bool canBuy = false;

    // Since the editor's scaling is off
    public bool isEditor = false;

    // Access game settings
    public Settings settings;

    // Get settings component
    private void Awake()
    {
        settings = GameObject.Find("Settings").GetComponent<Settings>();
    }

    // Constantly update information
    private void Update()
    {
        // Change position based on editor
        int actualUp = UP_POSITION;
        int actualDown = DOWN_POSITION;

        if (isEditor)
        {
            actualUp -= 50;
            actualDown -= 50;
        }

        // Set position
        if (isUp)
        {
            basePanel.transform.localPosition = new Vector3(0f, actualUp, 0f);
            tabPanel.transform.localPosition = new Vector3(-370f, actualUp - 125, 0f);

            if (manager.food < 10)
            {
                lowFoodPanel.transform.localPosition = new Vector3(-100f, actualUp - 125, 0f);
            }
            else
            {
                lowFoodPanel.transform.localPosition = new Vector3(-100f, actualUp, 0f);
            }

            manager.buildMenuOpen = true;
            tabText.text = "Close Menu";
        }
        else
        {
            basePanel.transform.localPosition = new Vector3(0f, actualDown, 0f);
            tabPanel.transform.localPosition = new Vector3(-370f, actualDown - 125, 0f);

            if (manager.food < 10)
            {
                lowFoodPanel.transform.localPosition = new Vector3(-100f, actualDown - 125, 0f);
            }
            else 
            {
                lowFoodPanel.transform.localPosition = new Vector3(-100f, actualDown, 0f);
            }

            manager.buildMenuOpen = false;
            tabText.text = "Build Menu";
        }

        
        // Switch selection
        switch (buildingSelection)
        {
            case 0: 
                woodCost = (int) (10 * settings.difficultyScaler);
                stoneCost = (int)(10 * settings.difficultyScaler);
                foodCost = (int)(1 * settings.difficultyScaler);
                titleText.text = "Small Cottage";
                effectText.text = "Max Population +5";
                description.text = "A peaceful little home for a small family.";
                break;

            case 1:
                woodCost = (int)(20 * settings.difficultyScaler);
                stoneCost = (int)(20 * settings.difficultyScaler);
                foodCost = (int)(3 * settings.difficultyScaler);
                titleText.text = "Family Home";
                effectText.text = "Max Population +10";
                description.text = "A nice house for a growing family.";
                break;

            case 2:
                woodCost = (int)(50 * settings.difficultyScaler);
                stoneCost = (int)(20 * settings.difficultyScaler);
                foodCost = (int)(5 * settings.difficultyScaler);
                titleText.text = "Appartment Complex";
                effectText.text = "Max Population +25";
                description.text = "A large complex to house a growing population, crucial to all kingdoms of the council.";
                break;

            case 3:
                woodCost = (int)(20 * settings.difficultyScaler);
                stoneCost = 0;
                foodCost = (int)(1 * settings.difficultyScaler);
                titleText.text = "Garden";
                effectText.text = "Nightly Food Increase +7";
                description.text = "A small plot of land for a family to grow food.";
                break;

            case 4:
                woodCost = (int)(40 * settings.difficultyScaler);
                stoneCost = 0;
                foodCost = (int)(3 * settings.difficultyScaler);
                titleText.text = "Local Farm";
                effectText.text = "Nightly Food Increase +12";
                description.text = "A small farm, perfect for a growing village.";
                break;

            case 5:
                woodCost = (int)(90 * settings.difficultyScaler);
                stoneCost = 0;
                foodCost = (int)(5 * settings.difficultyScaler);
                titleText.text = "Industrial Farm";
                effectText.text = "Nightly Food Increase +30";
                description.text = "A farm fit to feed a kingdom.";
                break;

            case 6:
                woodCost = (int)(20 * settings.difficultyScaler);
                stoneCost = (int)(10 * settings.difficultyScaler);
                foodCost = (int)(1 * settings.difficultyScaler);
                titleText.text = "Trade Post";
                effectText.text = "Unlocks Trading\n\nNightly Gold Earnings +5";
                description.text = "A small stand for locals to sell their produce.";
                break;

            case 7:
                woodCost = (int)(40 * settings.difficultyScaler);
                stoneCost = (int)(10 * settings.difficultyScaler);
                foodCost = (int)(3 * settings.difficultyScaler);
                titleText.text = "Local Market";
                effectText.text = "Unlocks Trading\n\nNightly Gold Earnings +10";
                description.text = "A large market for many locals to sell their products. A great way to attract traders to your village.";
                break;

            case 8:
                woodCost = (int)(100 * settings.difficultyScaler);
                stoneCost = (int)(50 * settings.difficultyScaler);
                foodCost = (int)(5 * settings.difficultyScaler);
                titleText.text = "Bank";
                effectText.text = "Unlocks Trading\n\nNightly Gold Earnings +25";
                description.text = "What better way to expand businesses in your town?";
                break;

            case 9:
                woodCost = 0;
                stoneCost = (int)(20 * settings.difficultyScaler);
                foodCost = (int)(1 * settings.difficultyScaler);
                titleText.text = "Archer Tower";
                effectText.text = "Defense +5";
                description.text = "A small tower local archers can use to defend your town at night.";
                break;

            case 10:
                woodCost = 0;
                stoneCost = (int)(40 * settings.difficultyScaler);
                foodCost = (int)(3 * settings.difficultyScaler);
                titleText.text = "Army Barracks";
                effectText.text = "Defense +10";
                description.text = "Somewhere soldiers can rest while not defending the town. A great way to attrack warriors to your town.";
                break;

            case 11:
                woodCost = 0;
                stoneCost = (int)(100 * settings.difficultyScaler);
                foodCost = (int)(5 * settings.difficultyScaler);
                titleText.text = "Keep";
                effectText.text = "Defense +25";
                description.text = "A large, defendable tower. It times or crisis, your people can hide here to stay safe.";
                break;
        }

        // Set cost text
        costText.text = "Wood: " + woodCost + "\nStone: " + stoneCost + "\nFood: " + foodCost;

        // Can the player buy it
        if (manager.wood < woodCost || manager.stone < stoneCost || manager.food < foodCost)
        {
            canBuy = false;
            buyButton.color = RED;
        }
        else
        {
            canBuy = true;
            buyButton.color = GREEN;
        }
    }

    // Open/close pane;
    public void toggleOpen()
    {
        isUp = !isUp;
    }

    // Switch target
    public void target(int t)
    {
        buildingSelection = t;
    }

    // Purchase target
    public void buyTarget()
    {
        if (canBuy)
        {
            // Turn off demo mode and close panel
            manager.buildMenuOpen = true;
            isUp = false;

            // Create and start building
            GameObject r = Instantiate(building);
            r.GetComponent<Building>().manager = this.manager;
            r.GetComponent<Building>().buildMenu = this;
            r.GetComponent<Building>().setBuildingType(buildingSelection);
            r.GetComponent<Building>().StartBuild();
        }
    }
}
