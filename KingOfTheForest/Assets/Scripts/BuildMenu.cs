using TMPro;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    private const int UP_POSITION = -255;
    private const int DOWN_POSITION = -445;
    private Color GREEN = new Color(.5f, .75f, .5f);
    private Color RED = new Color(.75f, .5f, .5f);
    private int woodCost;
    private int stoneCost;
    public bool isUp = false;
    public UnityEngine.UI.Image basePanel;
    public UnityEngine.UI.Image tabPanel;
    public UnityEngine.UI.Image buyButton;
    public TextMeshProUGUI tabText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI titleText;
    public int buildingSelection = 0;
    public GameManager manager;
    bool canBuy = false;

    private void Update()
    {
        if (isUp)
        {
            basePanel.transform.localPosition = new Vector3(0f, UP_POSITION, 0f);
            tabPanel.transform.localPosition = new Vector3(-460f, UP_POSITION + 120, 0f);
            tabText.text = "Close";
        }
        else
        {
            basePanel.transform.localPosition = new Vector3(0f, DOWN_POSITION, 0f);
            tabPanel.transform.localPosition = new Vector3(-460f, DOWN_POSITION + 120, 0f);
            tabText.text = "Open";
        }

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

        if (manager.wood < woodCost || manager.stone < stoneCost)
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

    public void toggleOpen()
    {
        isUp = !isUp;
    }

    public void target(int t)
    {
        buildingSelection = t;
    }

    public void buyTarget()
    {
        if (canBuy)
        {
            manager.wood -= woodCost;
            manager.stone -= stoneCost;
        }
    }
}
