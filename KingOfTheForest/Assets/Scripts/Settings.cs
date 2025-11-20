using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    // Used to set the difficulty
    // Lower = Easier
    public float difficultyScaler = 1.25f;

    // Set Cursor Texture
    public Texture2D cursorTextureHand;
    public Texture2D cursorTextureAxe;
    public Texture2D cursorTexturePick;
    public Texture2D cursorTextureHammer;

    // Switch cursor
    public bool cAxe = false;
    public bool cPick = false;
    public bool cHammer = false;

    // Tutorial on/off
    public bool tutorialEnabled = true;

    // Villager data
    public float maxTargetDistance = 5.0f;
    public int foodGeneration = 1;
    public int moneyGeneration = 1;
    public int populationIncrease = 1;
    public int vFoodCap = 5;
    public int vMoneyCap = 5;
    public int vPopulationCap = 3;
    public float actionDelay = 2.0f;
    public float vSpeed = 100.0f;

    // Bandit Data
    public int bFoodCap = 3; // The amount of food 1 bandit can steal
    public int bWoodCap = 3; // The amount of wood 1 bandit can steal
    public int bStoneCap = 3; // The amount of stone 1 bandit can steal
    public int bMoneyCap = 3; // The amount of money 1 bandit can steal
    public int buildingsCap = 1; // The number of buildings 1 bandit can help destroy
    public float buildingDestructionTime = 5.0f; // How long it takes to destroy a building
    public int buildingDestructionThreshhold = 3; // How many bandits are needed to destroy a building
    public float destructionWaitTime = 15.0f; // How long a bandit will wait for other bandits to come help destroy a building 
    public float bSpeed = 100.0f;

    // Load delay
    float timer = 0f;

    // Persistent
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("LoadScene"))
        {
            timer += Time.deltaTime; // Increment timer
            if (timer >= 0.5)
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        if (cHammer)
        {
            setCursorTexture(3);
        }
        else if (cPick)
        {
            setCursorTexture(2);
        }
        else if (cAxe)
        {
            setCursorTexture(1);
        }
        else
        {
            setCursorTexture(0);
        }
    }

    // 0 - Hand
    // 1 - Axe
    // 2 - Pick
    // 3 - Hammer
    public void setCursorTexture(int texture)
    {
        Vector2 cursorTarget = new Vector2(10f, 10f);

        switch(texture)
        {
            case 1:
                Cursor.SetCursor(cursorTextureAxe, cursorTarget, CursorMode.ForceSoftware);
                break;

            case 2:
                Cursor.SetCursor(cursorTexturePick, cursorTarget, CursorMode.ForceSoftware);
                break;

            case 3:
                Cursor.SetCursor(cursorTextureHammer, cursorTarget, CursorMode.ForceSoftware);
                break;

            case 0:
            default:
                Cursor.SetCursor(cursorTextureHand, cursorTarget, CursorMode.ForceSoftware);
                break;
        }
    }
}
