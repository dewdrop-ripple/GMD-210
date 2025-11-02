using TMPro;
using UnityEngine;

public class SettingsButtons : MonoBehaviour
{
    // Text to edit
    public TextMeshProUGUI difficultyText;

    // Access game settings
    public Settings settings;

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
    }

    private void Start()
    {
        if (settings.difficultyScaler == 0.5f)
        {
            difficulty = 0;
        }
        else if (settings.difficultyScaler == 0.75f)
        {
            difficulty = 1;
        }
        else if (settings.difficultyScaler == 1.0f)
        {
            difficulty = 2;
        }
        else if (settings.difficultyScaler == 1.25f)
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
                settings.difficultyScaler = 0.5f;
                break;

            case 1:
                difficultyText.text = "Difficulty: Easy";
                settings.difficultyScaler = 0.75f;
                break;

            case 3:
                difficultyText.text = "Difficulty: Hard";
                settings.difficultyScaler = 1.25f;
                break;

            case 4:
                difficultyText.text = "Difficulty: Impossible";
                settings.difficultyScaler = 1.5f;
                break;

            case 2:
            default:
                difficultyText.text = "Difficulty: Normal";
                settings.difficultyScaler = 1.0f;
                break;
        }
    }

    public void changeDifficulty()
    {
        difficulty++;
        if (difficulty > 4) difficulty = 0;
    }
}
