using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButtons : MonoBehaviour
{
    // Text to edit
    public TextMeshProUGUI difficultyText;
    //public TextMeshProUGUI tutorialText;

    // Access game settings
    public Settings settings;

    // Buttons
    public Toggle tutorialOn;

    public Slider maxDistanceSlider;
    public TextMeshProUGUI maxDistanceText;

    public Slider foodGenerationSlider;
    public TextMeshProUGUI foodGenerationText;

    public Slider moneyGenerationSlider;
    public TextMeshProUGUI moneyGenerationText;

    public Slider popGenerationSlider;
    public TextMeshProUGUI popGenerationText;

    public Slider foodCapSlider;
    public TextMeshProUGUI foodCapText;

    public Slider moneyCapSlider;
    public TextMeshProUGUI moneyCapText;

    public Slider popCapSlider;
    public TextMeshProUGUI popCapText;

    public Slider actionSpeedSlider;
    public TextMeshProUGUI actionSpeedText;

    public Slider vSpeedSlider;
    public TextMeshProUGUI vSpeedText;

    public Slider bFoodCapSlider;
    public TextMeshProUGUI bFoodCapText;

    public Slider bWoodCapSlider;
    public TextMeshProUGUI bWoodCapText;

    public Slider bStoneCapSlider;
    public TextMeshProUGUI bStoneCapText;

    public Slider bMoneyCapSlider;
    public TextMeshProUGUI bMoneyCapText;

    public Slider buildingsCapSlider;
    public TextMeshProUGUI buildingsCapText;

    public Slider buildingDestrcutionTimeSlider;
    public TextMeshProUGUI buildingDestrcutionTimeText;

    public Slider buildingDestrcutionThreshholdSlider;
    public TextMeshProUGUI buildingDestrcutionThreshholdText;

    public Slider buildingDestrcutionWaitTimeSlider;
    public TextMeshProUGUI buildingDestrcutionWaitTimeText;

    public Slider bSpeedSlider;
    public TextMeshProUGUI bSpeedText;

    // 0 - Very Easy
    // 1 - Easy
    // 2 - Normal
    // 3 - Hard
    // 4 - Impossible
    int difficulty = 2;

    // Get settings component
    private void Awake()
    {
        settings = GameObject.Find("Settings").GetComponent<Settings>();
        maxDistanceSlider.value = settings.maxTargetDistanceSteps;
        foodGenerationSlider.value = settings.foodGeneration;
        moneyGenerationSlider.value = settings.moneyGeneration;
        popGenerationSlider.value = settings.populationIncrease;
        foodCapSlider.value = settings.vFoodCap;
        moneyCapSlider.value = settings.vMoneyCap;
        popCapSlider.value = settings.vPopulationCap;
        actionSpeedSlider.value = settings.actionDelay;
        vSpeedSlider.value = settings.vSpeed;
        bFoodCapSlider.value = settings.bFoodCap;
        bWoodCapSlider.value = settings.bWoodCap;
        bStoneCapSlider.value = settings.bStoneCap;
        bMoneyCapSlider.value = settings.bMoneyCap;
        buildingsCapSlider.value = settings.buildingsCap;
        buildingDestrcutionTimeSlider.value = settings.buildingDestructionTime;
        buildingDestrcutionThreshholdSlider.value = settings.buildingDestructionThreshhold;
        buildingDestrcutionWaitTimeSlider.value = settings.destructionWaitTime;
        bSpeedSlider.value = settings.bSpeed;
    }

    private void Start()
    {
        if (settings.difficultyScaler == 0.75f)
        {
            difficulty = 0;
        }
        else if (settings.difficultyScaler == 1.0f)
        {
            difficulty = 1;
        }
        else if (settings.difficultyScaler == 1.25f)
        {
            difficulty = 2;
        }
        else if (settings.difficultyScaler == 1.5f)
        {
            difficulty = 3;
        }
        else 
        {
            difficulty = 4;
        }
    }

    private void Update()
    {
        switch (difficulty)
        {
            case 0:
                difficultyText.text = "Difficulty: Beginner";
                settings.difficultyScaler = 0.75f;
                break;

            case 1:
                difficultyText.text = "Difficulty: Easy";
                settings.difficultyScaler = 1.0f;
                break;

            case 3:
                difficultyText.text = "Difficulty: Hard";
                settings.difficultyScaler = 1.50f;
                break;

            case 4:
                difficultyText.text = "Difficulty: Impossible";
                settings.difficultyScaler = 2f;
                break;

            case 2:
            default:
                difficultyText.text = "Difficulty: Normal";
                settings.difficultyScaler = 1.25f;
                break;
        }

        settings.tutorialEnabled = tutorialOn.isOn;

        maxDistanceText.text = maxDistanceSlider.value.ToString();
        settings.maxTargetDistanceSteps = (int)maxDistanceSlider.value;
        maxDistanceSlider.value = settings.maxTargetDistanceSteps;

        foodGenerationText.text = foodGenerationSlider.value.ToString();
        settings.foodGeneration = (int)foodGenerationSlider.value;
        foodGenerationSlider.value = settings.foodGeneration;

        moneyGenerationText.text = moneyGenerationSlider.value.ToString();
        settings.moneyGeneration = (int)moneyGenerationSlider.value;
        moneyGenerationSlider.value = settings.moneyGeneration;

        popGenerationText.text = popGenerationSlider.value.ToString();
        settings.populationIncrease = (int)popGenerationSlider.value;
        popGenerationSlider.value = settings.populationIncrease;

        foodCapText.text = foodCapSlider.value.ToString();
        settings.vFoodCap = (int)foodCapSlider.value;
        foodCapSlider.value = settings.vFoodCap;

        moneyCapText.text = moneyCapSlider.value.ToString();
        settings.vMoneyCap = (int)moneyCapSlider.value;
        moneyCapSlider.value = settings.vMoneyCap;

        popCapText.text = popCapSlider.value.ToString();
        settings.vPopulationCap = (int)popCapSlider.value;
        popCapSlider.value = settings.vPopulationCap;

        float actionSpeedSliderRounded = (float)(int)(actionSpeedSlider.value * 1000) / 1000;
        actionSpeedText.text = actionSpeedSliderRounded.ToString();
        settings.actionDelay = actionSpeedSliderRounded;
        actionSpeedSlider.value = settings.actionDelay;

        float vSpeedSliderRounded = (float)(int)(vSpeedSlider.value * 1000) / 1000;
        vSpeedText.text = vSpeedSliderRounded.ToString();
        settings.vSpeed = vSpeedSliderRounded;
        vSpeedSlider.value = settings.vSpeed;

        bFoodCapText.text = bFoodCapSlider.value.ToString();
        settings.bFoodCap = (int)bFoodCapSlider.value;
        bFoodCapSlider.value = settings.bFoodCap;

        bWoodCapText.text = bWoodCapSlider.value.ToString();
        settings.bWoodCap = (int)bWoodCapSlider.value;
        bWoodCapSlider.value = settings.bWoodCap;

        bStoneCapText.text = bStoneCapSlider.value.ToString();
        settings.bStoneCap = (int)bStoneCapSlider.value;
        bStoneCapSlider.value = settings.bStoneCap;

        bMoneyCapText.text = bMoneyCapSlider.value.ToString();
        settings.bMoneyCap = (int)bMoneyCapSlider.value;
        bMoneyCapSlider.value = settings.bMoneyCap;

        buildingsCapText.text = buildingsCapSlider.value.ToString();
        settings.buildingsCap = (int)buildingsCapSlider.value;
        buildingsCapSlider.value = settings.buildingsCap;

        float buildingDestrcutionTimeSliderRounded = (float)(int)(buildingDestrcutionTimeSlider.value * 1000) / 1000;
        buildingDestrcutionTimeText.text = buildingDestrcutionTimeSliderRounded.ToString();
        settings.buildingDestructionTime = buildingDestrcutionTimeSliderRounded;
        buildingDestrcutionTimeSlider.value = settings.buildingDestructionTime;

        buildingDestrcutionThreshholdText.text = buildingDestrcutionThreshholdSlider.value.ToString();
        settings.buildingDestructionThreshhold = (int)buildingDestrcutionThreshholdSlider.value;
        buildingDestrcutionThreshholdSlider.value = settings.buildingDestructionThreshhold;

        float buildingDestrcutionWaitTimeSliderRounded = (float)(int)(buildingDestrcutionWaitTimeSlider.value * 1000) / 1000;
        buildingDestrcutionWaitTimeText.text = buildingDestrcutionWaitTimeSliderRounded.ToString();
        settings.destructionWaitTime = buildingDestrcutionWaitTimeSliderRounded;
        buildingDestrcutionWaitTimeSlider.value = settings.destructionWaitTime;

        float bSpeedSliderRounded = (float)(int)(bSpeedSlider.value * 1000) / 1000;
        bSpeedText.text = bSpeedSliderRounded.ToString();
        settings.bSpeed = bSpeedSliderRounded;
        bSpeedSlider.value = settings.bSpeed;
    }

    public void changeDifficulty()
    {
        difficulty++;
        if (difficulty > 4) difficulty = 0;
    }
    public void resetToDefault()
    {
        settings.maxTargetDistanceSteps = 200;
        maxDistanceSlider.value = settings.maxTargetDistanceSteps;

        settings.foodGeneration = 1;
        foodGenerationSlider.value = settings.foodGeneration;

        settings.moneyGeneration = 1;
        moneyGenerationSlider.value = settings.moneyGeneration;

        settings.populationIncrease = 1;
        popGenerationSlider.value = settings.populationIncrease;

        settings.vFoodCap = 5;
        foodCapSlider.value = settings.vFoodCap;

        settings.vMoneyCap = 5;
        moneyCapSlider.value = settings.vMoneyCap;

        settings.vPopulationCap = 1;
        popCapSlider.value = settings.vPopulationCap;

        settings.actionDelay = 2.0f;
        actionSpeedSlider.value = settings.actionDelay;

        settings.vSpeed = 0.01f;
        vSpeedSlider.value = settings.vSpeed;

        settings.bFoodCap = 3;
        bFoodCapSlider.value = settings.bFoodCap;

        settings.bWoodCap = 3;
        bWoodCapSlider.value = settings.bWoodCap;

        settings.bStoneCap = 3;
        bStoneCapSlider.value = settings.bStoneCap;

        settings.bMoneyCap = 3;
        bMoneyCapSlider.value = settings.bMoneyCap;

        settings.buildingsCap = 1;
        buildingsCapSlider.value = settings.buildingsCap;

        settings.buildingDestructionTime = 5.0f;
        buildingDestrcutionTimeSlider.value = settings.buildingDestructionTime;

        settings.buildingDestructionThreshhold = 3;
        buildingDestrcutionThreshholdSlider.value = settings.buildingDestructionThreshhold;

        settings.destructionWaitTime = 15.0f;
        buildingDestrcutionWaitTimeSlider.value = settings.destructionWaitTime;

        settings.bSpeed = 0.01f;
        bSpeedSlider.value = settings.bSpeed;
    }
}
