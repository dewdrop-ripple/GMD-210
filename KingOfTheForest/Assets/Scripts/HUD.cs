using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    public GameManager manager;
    public UnityEngine.UI.Image dayNightTracker;
    public TextMeshProUGUI dayNumber;
    public TextMeshProUGUI population;
    public TextMeshProUGUI wood;
    public TextMeshProUGUI stone;
    public TextMeshProUGUI food;
    public TextMeshProUGUI money;
    public TextMeshProUGUI defense;
    public UnityEngine.UI.Toggle demoModeToggle;

    // Constantly update variables
    void Update()
    {
        if (manager.isDay)
        {
            dayNightTracker.color = new Color(0.25f, 0.75f, 1.0f);
            dayNumber.text = "Day " + manager.day;
        }
        else
        {
            dayNightTracker.color = new Color(0.1f, 0.0f, 0.25f);
            dayNumber.text = "Night " + manager.day;
        }

        population.text = "Population: " + manager.population + "/" + manager.maxPopulation;
        wood.text = "Wood:       " + manager.wood;
        stone.text = "Stone:       " + manager.stone;
        food.text = "Food: " + manager.food;
        money.text = "Money: " + manager.money + "g";
        defense.text = "Defense: " + manager.defense;

        if (demoModeToggle.isOn) manager.demoMode = true;
        else manager.demoMode = false;
    }
}
