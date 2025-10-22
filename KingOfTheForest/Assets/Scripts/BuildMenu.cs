using TMPro;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    // Diffrent Position
    private const int UP_POSITION = -255;
    private const int DOWN_POSITION = -445;
    public bool isUp = false;
    public UnityEngine.UI.Image basePanel;
    public UnityEngine.UI.Image tabPanel;
    public UnityEngine.UI.Image buyButton;

    // Button Colors
    private Color GREEN = new Color(.5f, .75f, .5f);
    private Color RED = new Color(.75f, .5f, .5f);

    // Used to edit text
    private int woodCost;
    private int stoneCost;
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

    // To turn off demo mode when building
    public UnityEngine.UI.Toggle demoModeToggle;

    // Constantly update information
    private void Update()
    {
        // Change position based on editor
        int actualUp = UP_POSITION;
        int actualDown = DOWN_POSITION;

        if (isEditor)
        {
            actualUp += 50;
            actualDown += 50;
        }

        // Set position
        if (isUp)
        {
            basePanel.transform.localPosition = new Vector3(0f, actualUp, 0f);
            tabPanel.transform.localPosition = new Vector3(-460f, actualUp + 120, 0f);
            tabText.text = "Close";
        }
        else
        {
            basePanel.transform.localPosition = new Vector3(0f, actualDown, 0f);
            tabPanel.transform.localPosition = new Vector3(-460f, actualDown + 120, 0f);
            tabText.text = "Open";
        }

        // Switch selection
        switch (buildingSelection)
        {
            case 0: 
                woodCost = 10;
                stoneCost = 10;
                titleText.text = "Small House";
                effectText.text = "Max Population +5";
                break;

            case 1:
                woodCost = 20;
                stoneCost = 20;
                titleText.text = "Medium House";
                effectText.text = "Max Population +10";
                break;

            case 2:
                woodCost = 50;
                stoneCost = 50;
                titleText.text = "Large House";
                effectText.text = "Max Population +25";
                break;

            case 3:
                woodCost = 10;
                stoneCost = 0;
                titleText.text = "Small Farm";
                effectText.text = "Nightly Food Increase +5";
                break;

            case 4:
                woodCost = 20;
                stoneCost = 0;
                titleText.text = "Medium Farm";
                effectText.text = "Nightly Food Increase +10";
                break;

            case 5:
                woodCost = 50;
                stoneCost = 0;
                titleText.text = "Large Farm";
                effectText.text = "Nightly Food Increase +25";
                break;

            case 6:
                woodCost = 10;
                stoneCost = 5;
                titleText.text = "Small Trade Post";
                effectText.text = "Unlocks Trading\n\nNightly Gold Earnings +5";
                break;

            case 7:
                woodCost = 20;
                stoneCost = 10;
                titleText.text = "Medium Trade Post";
                effectText.text = "Unlocks Trading\n\nNightly Gold Earnings +10";
                break;

            case 8:
                woodCost = 50;
                stoneCost = 25;
                titleText.text = "Large Trade Post";
                effectText.text = "Unlocks Trading\n\nNightly Gold Earnings +25";
                break;

            case 9:
                woodCost = 0;
                stoneCost = 10;
                titleText.text = "Small Guard Tower";
                effectText.text = "Defense +5";
                break;

            case 10:
                woodCost = 0;
                stoneCost = 20;
                titleText.text = "Medium Guard Tower";
                effectText.text = "Defense +10";
                break;

            case 11:
                woodCost = 0;
                stoneCost = 50;
                titleText.text = "Large Guard Tower";
                effectText.text = "Defense +25";
                break;
        }

        costText.text = "Wood: " + woodCost + "\nStone: " + stoneCost;

        // Can the player buy it
        if (manager.wood < woodCost || manager.stone < stoneCost)
        {
            canBuy = false;
            buyButton.enabled = false;
            buyButton.color = RED;
        }
        else
        {
            canBuy = true;
            buyButton.enabled = true;
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
            demoModeToggle.isOn = false;
            manager.demoMode = false;
            isUp = false;

            // Create and start building
            GameObject r = Instantiate(building);
            r.GetComponent<Building>().manager = this.manager;
            r.GetComponent<Building>().setBuildingType(buildingSelection);
            r.GetComponent<Building>().StartBuild();
        }
    }
}
