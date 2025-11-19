using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Player Stats
    // These are automatically set to the base stats
    public int population = 5;
    public int maxPopulation = 5;
    public int day = 1;
    public int wood = 0;
    public int stone = 0;
    public int food = 50;
    public int money = 0;
    public int defense = 0;

    // States
    public bool isDay = true;
    public bool canTrade = false;
    int resourceNumber = 0;
    public bool freePlay = false;

    // When to enable/disable demo mode
    public bool demoMode = true;
    public bool buildMenuOpen = false;
    public bool pauseOverlayOpen = false;
    public bool nightScreenOpen = false;
    public bool buildingOverlayOpen = false;
    public bool currentlyBuilding = false;
    public bool tutorialTextOpen = false;
    public bool bannedFromMining = false;

    public bool canScroll = true;

    // Game objects to access
    public GameObject resource;
    public Building startingHouse;
    public Advisor advisor;

    // So that I can quickly change this for testing purposes
    private const int WIN_POPULATION = 500;

    // Building Tracker - Each array location represents a building and
    // the value represents how many the player has
    // 0-2 - S, M, L Houses
    // 3-5 - S, M, L Farms
    // 6-8 - S, M, L Trade Posts
    // 9-11 - S, M, L Towers
    public int[] buildings = new int[12];

    private int[] prevBuildings = new int[12];
    private int lameDays = 0;

    // List of all building objects
    public List<Building> buildingsList;

    // So that I can quickly adjust building stats for testing purposes
    private const int SMALL_HOUSE_CAPACITY = 5;
    private const int MED_HOUSE_CAPACITY = 25;
    private const int LARGE_HOUSE_CAPACITY = 80;
    private const int SMALL_TOWER_DEFENSE = 5;
    private const int MED_TOWER_DEFENSE = 25;
    private const int LARGE_TOWER_DEFENSE = 80;
    private const int SMALL_FARM_FOOD = 7;
    private const int MED_FARM_FOOD = 32;
    private const int LARGE_FARM_FOOD = 120;
    private const int SMALL_TRADE_MONEY = 5;
    private const int MED_TRADE_MONEY = 25;
    private const int LARGE_TRADE_MONEY = 80;

    // Used to track attacks
    private int consecSafeNights = 0;
    private int consecBadNights = 0;

    // All possible nightly states for display text
    private bool goodMoney = false;
    private bool goodFood = false;
    private bool popGrowthBig = false;
    private bool popGrowthSmall = false;
    private bool foodShortage = false;
    private bool popAfraid = false;
    private bool popTiny = false;
    private bool notWorthAttack = false;
    private bool scaredOff = false;
    private bool bigAttack = false;
    private bool overpopulated = false;
    private bool lame = false;

    // Cheats
    private bool cheats = false;

    // Night overlay data
    public Canvas nightOverlay;
    public TextMeshProUGUI nText;
    public TextMeshProUGUI nPeople;
    public TextMeshProUGUI nFood;
    public TextMeshProUGUI nMoney;

    // Attack overlay data
    public Canvas attackOverlay;
    public TextMeshProUGUI aText;
    public TextMeshProUGUI aPeople;
    public TextMeshProUGUI aWood;
    public TextMeshProUGUI aStone;
    public TextMeshProUGUI aFood;
    public TextMeshProUGUI aMoney;
    public TextMeshProUGUI aBuildings;

    // "Pause" Menu
    public Canvas pauseOverlay;

    // Win/Loss
    public Canvas winScreen;
    public Canvas loseScreen;

    // Build menu
    public Canvas buildMenu;

    // Vingette
    public Image vingette;

    // Since my grid size =/= unity grid size
    public float scaleFactor = 0.05f;
    public int leftX = -20;
    public int rightX = 20;
    public int bottomY = -5;
    public int topY = 5;

    public TextBox textBox;

    // Text options
    private string[] nightTextOptions =
    {
        "It's a quiet night.",
        "Your traders have been working hard all night. Business is booming!",
        "Your people celebrated the harvest with a feast in your honor!",
        "People are attracted to such a safe kingdom. Your town grows rapidly overnight.",
        "The population grows steadily.",
        "As food supplies run short, your people starve, and many more leave in the night.",
        "The recent attacks have spread fear through the town. People leave town in the night.",
        "Your ‘kingdom’ is a ghost town. The council scoffs at your efforts.",
        "Some people tried to move in overnight, but the kingdom is completely full.",
        "The lack of recent growth in your kingdom has caused you to fade from public view."
    };

    private string[] attackTextOptions =
    {
        "The bandits couldn’t be bothered to attack such a small kingdom. Your people slept soundly through the night.",
        "A group of bandits wandered the forest nearby, but were scared off by your defenses and avoided the town. Your people slept soundly through the night.",
        "A group of bandits attacked the town in the night. Your people are scared, but the damage was minimal.",
        "A large group of bandits attacked the town in the night. Your people will bury their dead this morning, but they may not stay the night."
    };

    // Settings object
    public Settings settings;

    // Trade buttons
    public Button b1;
    public Button b2;
    public Button b3;
    public Button b4;
    public Button b5;
    public Button b6;

    public Button nextDay;

    // Set settings object
    private void Awake()
    {
        settings = GameObject.Find("Settings").GetComponent<Settings>();
    }

    // Initialize all variables
    private void Start()
    {
        // UIs
        attackOverlay.enabled = false;
        nightOverlay.enabled = false;
        pauseOverlay.enabled = false;
        winScreen.enabled = false;
        loseScreen.enabled = false;

        // Start with 1 small house and nothing else
        buildings[0] = 1;
        prevBuildings[0] = 1;
        for (int i = 1; i < 12; i++)
        {
            buildings[i] = 0;
            prevBuildings[i] = 0;
        }

        // Set starting house data
        startingHouse.setBuildingType(0);
        startingHouse.preBuilt();
        buildingsList.Add(startingHouse);

        // Generate all resources
        GenerateResources();
    }

    // Generate all resources
    private void GenerateResources()
    {
        // Resources
        for (int i = 0; i < 120; i++)
        {
            // Instantiate
            GameObject r = Instantiate(resource);

            // Set variables
            r.GetComponent<Resource>().manager = this;
            r.GetComponent<Resource>().startingHouse = startingHouse;
            r.GetComponent<Resource>().typeID = resourceNumber % 6;
            r.GetComponent<Resource>().GenerateResource();
            r.GetComponent<Resource>().textBox = textBox;

            // Not random, one of each
            resourceNumber++;
        }
    }

    // Constantly update stats
    private void Update()
    {
        UpdateStats();

        float vingetteOpacity = (100.0f - ((float)food * 3.0f)) / 100.0f;
        if (vingetteOpacity < 0) { vingetteOpacity = 0; }
        vingette.color = new Color(0, 0, 0, vingetteOpacity);
        Debug.Log(vingetteOpacity);

        // Cheats
        if (Input.GetKeyDown(KeyCode.B)) { cheats = !cheats; }
        if (Input.GetKeyDown(KeyCode.N) && cheats) { food += 100; }
        if (Input.GetKeyDown(KeyCode.M) && cheats) { money += 100; }
        if (Input.GetKeyDown(KeyCode.K) && cheats)
        {
            food -= 100;
            if (food < 0) food = 0;
        }
        if (Input.GetKeyDown(KeyCode.L) && cheats)
        {
            money -= 100;
            if (money < 0) money = 0;
        }

        if (buildMenuOpen || pauseOverlayOpen || nightScreenOpen || buildingOverlayOpen || currentlyBuilding || tutorialTextOpen || bannedFromMining)
        {
            demoMode = false;
        }
        else
        {
            demoMode = true;
        }

        if (buildMenuOpen || pauseOverlayOpen || nightScreenOpen || buildingOverlayOpen)
        {
            canScroll = false;
        }
        else
        {
            canScroll = true;
        }

        if (canTrade)
        {
            b1.interactable = true;
            b2.interactable = true;
            b3.interactable = true;
            b4.interactable = true;
            b5.interactable = true;
            b6.interactable = true;

            if (cheats)
            {
                if (money < 50)
                {
                    b2.interactable = false;
                    b4.interactable = false;
                }
                if (money < 100)
                {
                    b6.interactable= false;
                }
                if (wood < 50)
                {
                    b1.interactable = false;
                }
                if (stone < 50)
                {
                    b3.interactable = false;
                }
                if (food < 100)
                {
                    b5.interactable = false;
                }
            }
            else
            {
                if (money < 10)
                {
                    b2.interactable = false;
                    b4.interactable = false;
                    b6.interactable = false;
                }
                if (wood < 10)
                {
                    b1.interactable = false;
                }
                if (stone < 10)
                {
                    b3.interactable = false;
                }
                if (food < 10)
                {
                    b5.interactable = false;
                }
            }
        }
        else
        {
            b1.interactable = false;
            b2.interactable = false;
            b3.interactable = false;    
            b4.interactable = false;
            b5.interactable = false;
            b6.interactable = false;
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

        // Make sure no negative stats
        if (money < 0) { money = 0; }
        if (food < 0) { food = 0; }
        if (wood < 0) { wood = 0; }
        if (stone < 0) { stone = 0; }
    }

    // Calculate nightly changes based on current stats
    public void NightlyResourceChange()
    {
        nightOverlay.enabled = true;

        // Check if player did nothing that day
        bool didNothing = true;
        for (int i = 0; i < 12; i++)
        {
            if (prevBuildings[i] != buildings[i])
            {
                didNothing = false;
            }
        }

        // Basic building changes
        int foodChange = 0;
        foodChange += (int)((buildings[3] * SMALL_FARM_FOOD) + (buildings[4] * MED_FARM_FOOD) + (buildings[5] * LARGE_FARM_FOOD) * (1 / settings.difficultyScaler));
        foodChange -= population;
        if (foodChange > (int) (food * 0.2)) {  goodFood = true; }

        int moneyChange = 0;
        moneyChange += (int)((buildings[6] * SMALL_TRADE_MONEY) + (buildings[7] * MED_TRADE_MONEY) + (buildings[8] * LARGE_TRADE_MONEY) * (1 / settings.difficultyScaler));
        if (moneyChange > (int)(money * 0.2)) {  goodMoney = true; }

        int popChange = 0;

        // Give the player some help early on, then make it harder
        if (population < 50)
        {
            foodChange += 25;
            popChange += 2;
        }
        else if (population < 100)
        {
            foodChange += 15;
        }
        else if (population > 400)
        {
            foodChange -= 125;
        }
        else if (population > 200)
        {
            foodChange -= 50;
        }

        // Are people starving?
        int newFood = food + foodChange;
        if (newFood < 0)
        {
            foodShortage = true;
            popChange += (int)(newFood *  settings.difficultyScaler); // Subtract from population (+ b/c food is negative)
            popChange -= (int)((float)population * 0.025);
            newFood = 0; // Reset
            foodChange = food;
        }

        // If things are bad
        if (consecBadNights > 2)
        {
            popChange -= consecBadNights * consecBadNights;
            popAfraid = true;
        }
        // If not
        else
        {
            // Add one person for every house or farm
            for (int i = 0; i < 6; i++)
            {
                popChange += buildings[i];
            }

            // If things are really good
            if (consecSafeNights > 2)
            {
                popChange += consecSafeNights * consecSafeNights;
            }
        }

        if ((population + popChange) < 0)
        {
            popChange = 0 - population;
        }

        if ((population + popChange) > maxPopulation)
        {
            popChange = maxPopulation - population;
            overpopulated = true;
        }

        // Check population change
        if (popChange > 0 && popChange < (int) (population * 0.2)) {  popGrowthSmall = true; }
        else if (popChange > (int)(population * 0.2)) { popGrowthBig = true; }

        if (population + popChange == 1)
        {
            popChange--;
        }

        if (didNothing)
        {
            popChange -= lameDays * (int)(day / 3);
            moneyChange -= lameDays * (int)(day / 3);
            foodChange += lameDays * (int)(day / 3);
            lameDays++;
        }
        else
        {
            lameDays = 0;
        }

        if (lameDays > 3)
        {
            lame = true;
        }

        // Actually change stats
        food += foodChange;
        money += moneyChange;
        population += popChange;

        if (population <= 3) { popTiny = true; }

        // Update text
        if (foodChange < 0) nFood.SetText(foodChange + " Food");
        else nFood.SetText("+" + foodChange + " Food");

        if (moneyChange < 0) nMoney.SetText(moneyChange + "g");
        else nMoney.SetText("+" + moneyChange + "g");

        if (popChange < 0) nPeople.SetText(popChange + " People");
        else nPeople.SetText("+" + popChange + " People");

        if (popTiny) { nText.SetText(nightTextOptions[7]); }
        else if (foodShortage) { nText.SetText(nightTextOptions[5]); }
        else if (popAfraid) { nText.SetText(nightTextOptions[6]); }
        else if (lame) { nText.SetText(nightTextOptions[9]); }
        else if (overpopulated) { nText.SetText(nightTextOptions[8]); }
        else if (popGrowthBig) { nText.SetText(nightTextOptions[3]); }
        else if (goodFood) { nText.SetText(nightTextOptions[2]); }
        else if (goodMoney) { nText.SetText(nightTextOptions[1]); }
        else if (popGrowthSmall) { nText.SetText(nightTextOptions[4]); }
        else { nText.SetText(nightTextOptions[0]); }
    }

    // Calculate effects of attack based on current stats
    public void Attack()
    {
        nightOverlay.enabled = false;
        attackOverlay.enabled = true;

        // Set strength
        int attackStrength;

        // If tiny pop, don't bother
        if (population <= 10 && day < 5)
        {
            attackStrength = 0;
            notWorthAttack = true;
        }
        else
        {
            attackStrength = population;
            attackStrength += (int)(day / 3);
            for (int i = 0; i < 12; i++)
            {
                attackStrength += buildings[i];
            }
            attackStrength = (int)(attackStrength * ((Mathf.Cos(day + 3.14f) + 2) * 0.5));
            attackStrength = (int)(attackStrength * settings.difficultyScaler);
        }

        if (population >= 500)
        {
            attackStrength = (int)(attackStrength / 4);
        }

        // Increase difficulty over time
        if (population < 50)
        {
            attackStrength -= 10;
        }
        else if (population < 100)
        {
            attackStrength -= 5;
        }
        else if (population > 400)
        {
            attackStrength += 50;
        }
        else if (population > 200)
        {
            attackStrength += 25;
        }

        int numBuildings = 0;
        for (int i = 0; i < 12; i++)
        {
            numBuildings += buildings[i];
        }

        attackStrength -= (int)(1.75 * defense);

        if (attackStrength > 0) attackStrength = (int)Mathf.Pow(attackStrength, 0.9f);
        else attackStrength = 0;

        //Debug.Log("Attack Strength = ((" + population + " - (2.5 * " + defense + ") + (" + day + " / 3 ) + " + numBuildings + ") * " + ((Mathf.Cos(day + 3.14f) + 2) * 0.5) + " * " + settings.difficultyScaler + " * " + Mathf.Pow(attackStrength, 0.9f) + " = " + attackStrength);

        // Variables
        int peopleKilled = 0;
        int woodStolen = 0;
        int stoneStolen = 0;
        int foodStolen = 0;
        int moneyStolen = 0;

        if (attackStrength > 0)
        {
            peopleKilled = (int)((attackStrength / 100f) * population);
            woodStolen = (int)((attackStrength / 100f) * wood);
            stoneStolen = (int)((attackStrength / 100f) * stone);
            foodStolen = (int)((attackStrength / 100f) * food);
            moneyStolen = (int)((attackStrength / 100f) * money);
        }
        else
        {
            consecSafeNights = 0;
            consecBadNights = 0;

            if (!notWorthAttack)
            {
                scaredOff = true;
                consecSafeNights++;
                consecBadNights = 0;
            }
        }

        // Set text
        aPeople.SetText(peopleKilled + " People Killed");
        aWood.SetText(woodStolen + " Wood Stolen");
        aStone.SetText(stoneStolen + " Stone Stolen");
        aFood.SetText(foodStolen + " Food Stolen");
        aMoney.SetText(moneyStolen + "g Stolen");

        // Update data
        population -= peopleKilled;
        wood -= woodStolen;
        stone -= stoneStolen;
        food -= foodStolen;
        money -= moneyStolen;

        // Buildings destroyed
        string buildingsText = "";

        // Destroy buildings
        if (attackStrength > 10)
        {
            consecSafeNights = 0;
            consecBadNights++;

            bigAttack = true;

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

        // Update building text
        if (buildingsText == "") aBuildings.SetText("None");
        else aBuildings.SetText(buildingsText);

        UpdateStats();

        // Set blurb
        if (notWorthAttack) { aText.SetText(attackTextOptions[0]); }
        else if (scaredOff) { aText.SetText(attackTextOptions[1]); }
        else if (bigAttack) { aText.SetText(attackTextOptions[3]); }
        else { aText.SetText(attackTextOptions[2]); }
    }

    // Trade functions
    public void SellWood()
    {
        if (cheats)
        {
            if (wood < 50) return;
            wood -= 50;
            money += 50;
        }
        else
        {
            if (!canTrade) return;
            if (wood < 10) return;
            wood -= 10;
            money += 10;
        }    
    }
    public void BuyWood()
    {
        if (cheats)
        {
            if (money < 50) return;
            wood += 50;
            money -= 50;
        }
        else
        {
            if (!canTrade) return;
            if (money < 10) return;
            wood += 10;
            money -= 10;
        }
    }
    public void SellStone()
    {
        if (cheats)
        {
            if (stone < 50) return;
            stone -= 50;
            money += 50;
        }
        else
        {
            if (!canTrade) return;
            if (stone < 10) return;
            stone -= 10;
            money += 10;
        }
    }
    public void BuyStone()
    {
        if (cheats)
        {
            if (money < 50) return;
            stone += 50;
            money -= 50;
        }
        else
        {
            if (!canTrade) return;
            if (money < 10) return;
            stone += 10;
            money -= 10;
        }
    }
    public void SellFood()
    {
        if (cheats)
        {
            if (food < 100) return;
            food -= 100;
            money += 50;
        }
        else
        {
            if (!canTrade) return;
            if (food < 10) return;
            food -= 10;
            money += 5;
        }
    }
    public void BuyFood()
    {
        if (cheats)
        {
            if (money < 100) return;
            food += 50;
            money -= 100;
        }
        else
        {
            if (!canTrade) return;
            if (money < 10) return;
            food += 5;
            money -= 10;
        }
    }

    // To main menu
    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Close quit menu
    public void DontQuit()
    {
        pauseOverlayOpen = false;
        pauseOverlay.enabled = false;
    }

    // Open quit menu
    public void Pause()
    {
        pauseOverlayOpen = true;
        pauseOverlay.enabled = true;
    }

    // Start end of day
    public void EndDay()
    {
        nightScreenOpen = true;

        // Reset text options
        isDay = false;
        goodMoney = false;
        goodFood = false;
        popGrowthBig = false;
        popGrowthSmall = false;
        foodShortage = false;
        popAfraid = false;
        popTiny = false;
        notWorthAttack = false;
        scaredOff = false;
        bigAttack = false;
        overpopulated = false;
        lame = false;

        // Start nightly resource change
        NightlyResourceChange();
    }

    // New day
    public void StartNewDay()
    {
        // Has the player won or lost the game?
        int state = CheckWin();

        if (state == 1)
        {
            // No win screen in free play
            if (!freePlay)
            {
                advisor.gameWon();
            }
        }
        else if (state == 2)
        {
            loseScreen.enabled = true;
        }

        for (int i = 0; i < 12; i++)
        {
            prevBuildings[i] = buildings[i];
        }

        // Close menu
        attackOverlay.enabled = false;

        // New day
        isDay = true;
        day++;

        nightScreenOpen = false;
    }

    // Start free play mode
    public void StartFreePlay()
    {
        // Close win screen and turn on free play
        freePlay = true;
        winScreen.enabled = false;
    }

    // Reload level
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
