using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Player Stats
    // These are automatically set to the base stats
    public int population = 5;
    public int maxPopulation = 5;
    public int day = 1;
    public int wood = 0;
    public int stone = 0; 
    public int food = 25;
    public int money = 0;
    public int defense = 0;
    public bool isDay = true;
    public bool canTrade = false;
    public bool demoMode;
    public GameObject resource;
    int resourceNumber = 0;
    public Building startingHouse;

    // So that I can quickly change this for testing purposes
    private const int WIN_POPULATION = 500;

    // Building Tracker - Each array location represents a building and
    // the value represents how many the player has
    // 0-2 - S, M, L Houses
    // 3-5 - S, M, L Farms
    // 6-8 - S, M, L Trade Posts
    // 9-11 - S, M, L Towers
    public int[] buildings = new int[12];

    // List of all building objects
    public List<Building> buildingsList;

    // So that I can quickly adjust building stats for testing purposes
    private const int SMALL_HOUSE_CAPACITY = 5;
    private const int MED_HOUSE_CAPACITY = 25;
    private const int LARGE_HOUSE_CAPACITY = 50;
    private const int SMALL_TOWER_DEFENSE = 5;
    private const int MED_TOWER_DEFENSE = 25;
    private const int LARGE_TOWER_DEFENSE = 50;
    private const int SMALL_FARM_FOOD = 5;
    private const int MED_FARM_FOOD = 25;
    private const int LARGE_FARM_FOOD = 50;
    private const int SMALL_TRADE_MONEY = 5;
    private const int MED_TRADE_MONEY = 25;
    private const int LARGE_TRADE_MONEY = 50;

    // Used to track attacks
    private int consecSafeNights = 0;
    private int consecBadNights = 0;

    // All possible nightly states for display text
    private bool goodMoney = false;
    private bool goodFood = false;
    private bool popGrowth = false;
    private bool foodShortage = false;

    // Cheats
    private bool cheats = false;

    // Night overlay data
    public Canvas nightOverlay;
    public TextMeshProUGUI nPeople;
    public TextMeshProUGUI nFood;
    public TextMeshProUGUI nMoney;

    // Attack overlay data
    public Canvas attackOverlay;
    public TextMeshProUGUI aPeople;
    public TextMeshProUGUI aWood;
    public TextMeshProUGUI aStone;
    public TextMeshProUGUI aFood;
    public TextMeshProUGUI aMoney;
    public TextMeshProUGUI aBuildings;

    // Initialize all variables
    private void Start()
    {
        attackOverlay.enabled = false;
        nightOverlay.enabled = false;

        // Start with 1 small house and nothing else
        buildings[0] = 1;
        for (int i = 1; i < 12; i++)
        {
            buildings[i] = 0;
        }

        startingHouse.setBuildingType(0);
        startingHouse.preBuilt();
        buildingsList.Add(startingHouse);

        GenerateResources();
    }

    private void GenerateResources()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject r = Instantiate(resource);
            r.GetComponent<Resource>().manager = this;
            r.GetComponent<Resource>().setResourceType(resourceNumber % 6);
            r.GetComponent<Resource>().GenerateResource();
            resourceNumber++;
        }
    }

    // Constantly update stats
    private void Update()
    {
        UpdateStats();

        // Cheats
        if (Input.GetKeyDown(KeyCode.B)) { cheats = !cheats; }
        if (Input.GetKeyDown(KeyCode.N) && cheats) { food += 25; }
        if (Input.GetKeyDown(KeyCode.M) && cheats) { money += 25; }
        if (Input.GetKeyDown(KeyCode.K) && cheats) 
        { 
            food -= 25; 
            if (food < 0) food = 0;
        }
        if (Input.GetKeyDown(KeyCode.L) && cheats) 
        { 
            money -= 25; 
            if (money < 0) money = 0;
        }
    }

    // Check the current state of the game
    // Returns 0 if the game is not over
    // Returns 1 if the game has been won
    // Returns 2 if the game has been lost
    public int CheckWin()
    {
        if (population <= 0) { return 2; }
        if (population >= WIN_POPULATION) { return 1; }
        return 0;
    }

    public void UpdateStats()
    {
        // Update population
        maxPopulation = (buildings[0] * SMALL_HOUSE_CAPACITY) + (buildings[1] * MED_HOUSE_CAPACITY) + (buildings[2] * LARGE_HOUSE_CAPACITY);
        if (population > maxPopulation) { population = maxPopulation; }

        // Update defense
        defense = (buildings[9] * SMALL_TOWER_DEFENSE) + (buildings[10] * MED_TOWER_DEFENSE) + (buildings[11] * LARGE_TOWER_DEFENSE);

        // Can the player trade now?
        if (cheats) { canTrade = true; }
        else { canTrade = (buildings[6] > 0) || (buildings[7] > 0) || (buildings[8] > 0); }

        if (money < 0) { money = 0; }
        if (food < 0) { food = 0; }
        if (wood < 0) { wood = 0; }
        if (stone < 0) { stone = 0; }
    }

    // Calculate nightly changes based on current stats
    public void NightlyResourceChange()
    {
        nightOverlay.enabled = true;

        // Reset variables
        goodMoney = false;
        goodFood = false;
        popGrowth = false;
        foodShortage = false;

        // Basic building changes
        int prevFood = food;
        food += (buildings[3] * SMALL_FARM_FOOD) + (buildings[4] * MED_FARM_FOOD) + (buildings[5] * LARGE_FARM_FOOD);
        food -= population;
        if (food >= prevFood * 1.2) {  goodFood = true; }

        if (food < prevFood) nFood.SetText("-" + (food - prevFood) + " Food");
        else nFood.SetText("+" + (food - prevFood) + " Food");

        int prevMoney = money;
        money += (buildings[6] * SMALL_TRADE_MONEY) + (buildings[7] * MED_TRADE_MONEY) + (buildings[8] * LARGE_TRADE_MONEY);
        if (money >= prevMoney * 1.2) {  goodMoney = true; }

        if (money < prevMoney) nMoney.SetText("-" + (money - prevMoney) + "g");
        else nMoney.SetText("+" + (money - prevMoney) + "g");

        int prevPop = population;

        // Are people starving?
        if (food < 0)
        {
            foodShortage = true;
            population += food; // Subtract from population (+ b/c food is negative)
            food = 0; // Reset
        }

        // If things are bad
        if (consecBadNights > 2)
        {
            population -= consecBadNights * consecBadNights;
        }
        // If not
        else
        {
            // Add one person for every house or farm
            for (int i = 0; i < 6; i++)
            {
                population += buildings[i];
            }

            // If things are really good
            if (consecSafeNights > 2)
            {
                population += consecSafeNights * consecSafeNights;
            }
        }

        // Check population change
        if (population > prevPop) {  popGrowth = true; }

        if (population < prevFood) nPeople.SetText("-" + (population - prevPop) + " People");
        else nPeople.SetText("+" + (population - prevPop) + " People");

        // Make sure data is valid
        UpdateStats();

        // Has the player won or lost the game?
        int state = CheckWin();

        switch (state)
        {
            case 1:
                // GO TO WIN SCREEN - NOT DONE YET
                break;

            case 2:
                // GO TO LOSE SCREEN - NOT DONE YET
                break;

            default:
                // CONTINUE GAME - NOT DONE YET
                break;
        }
    }

    // Calculate effects of attack based on current stats
    public void Attack()
    {
        nightOverlay.enabled = false;
        attackOverlay.enabled = true; 

        int attackStrength = (int) ((population - defense) * Random.Range(0.75f, 1.25f));

        aPeople.SetText((int)((attackStrength / 100f) * population) + " People Killed");
        aWood.SetText((int)((attackStrength / 100f) * wood) + " Wood Stolen");
        aStone.SetText((int)((attackStrength / 100f) * stone) + " Stone Stolen");
        aFood.SetText((int)((attackStrength / 100f) * food) + " Food Stolen");
        aMoney.SetText((int)((attackStrength / 100f) * money) + "g Stolen");

        population -= (int)((attackStrength / 100f) * population);
        wood -= (int)((attackStrength / 100f) * wood);
        stone -= (int)((attackStrength / 100f) * stone);
        food -= (int)((attackStrength / 100f) * food);
        money -= (int)((attackStrength / 100f) * money);

        string buildingsText = "";

        if (attackStrength > 10)
        {
            int canDestroyBuildings = 0;
            List<int> destroyable = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                if (buildings[i] > 1) canDestroyBuildings++;
                destroyable.Add(i);
            }

            if (canDestroyBuildings > 0)
            {
                List<Building> destroyableBuildings = new List<Building>();

                for (int i = 0; i < buildingsList.Count; i++)
                {
                    if (destroyable.Contains(buildingsList[i].buildingArrayPosition))
                    {
                        destroyableBuildings.Add(buildingsList[i]);
                    }
                }

                for (int i = 0; (i < canDestroyBuildings && i < attackStrength / 10); i++)
                {
                    int toDestroy = Random.Range(0, destroyableBuildings.Count - 1);
                    Building destroyed = buildingsList[toDestroy];
                    destroyableBuildings.Remove(buildingsList[toDestroy]);
                    
                    switch(destroyed.buildingArrayPosition)
                    {
                        case 0:
                            buildingsText += "Small House\n";
                            break;

                        case 1:
                            buildingsText += "Medium House\n";
                            break;

                        case 2:
                            buildingsText += "Large House\n";
                            break;

                        case 3:
                            buildingsText += "Small Farm\n";
                            break;

                        case 4:
                            buildingsText += "Medium Farm\n";
                            break;

                        case 5:
                            buildingsText += "Large Farm\n";
                            break;

                        case 6:
                            buildingsText += "Small Trade Post\n";
                            break;

                        case 7:
                            buildingsText += "Medium Trade Post\n";
                            break;

                        case 8:
                            buildingsText += "Large Trade Post\n";
                            break;

                        case 9:
                            buildingsText += "Small Guard Tower\n";
                            break;

                        case 10:
                            buildingsText += "Medium Guard Tower\n";
                            break;

                        case 11:
                            buildingsText += "Large Guard Tower\n";
                            break;
                    }

                    destroyed.AttackBuilding();
                }
            }
        }

        if (buildingsText == "") aBuildings.SetText("None");
        else aBuildings.SetText(buildingsText);

        UpdateStats();
    }

    // Trade functions
    public void SellWood()
    {
        if (!canTrade) return;
        if (wood < 5) return;

        wood -= 5;
        money += 5;
    }
    public void BuyWood()
    {
        if (!canTrade) return;
        if (money < 5) return;

        wood += 5;
        money -= 5;
    }
    public void SellStone()
    {
        if (!canTrade) return;
        if (stone < 5) return;

        stone -= 5;
        money += 5;
    }
    public void BuyStone()
    {
        if (!canTrade) return;
        if (money < 5) return;

        stone += 5;
        money -= 5;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void EndDay()
    {
        isDay = false;
        NightlyResourceChange();
    }

    public void StartNewDay()
    {
        attackOverlay.enabled = false;
        isDay = true;
        day++;
    }
}
