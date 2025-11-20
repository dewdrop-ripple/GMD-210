using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    // Game Mananger
    public GameManager manager;

    // HUD stuff to edit
    //public UnityEngine.UI.Image dayNightTracker;
    public TextMeshProUGUI dayNumber;
    public TextMeshProUGUI population;
    public TextMeshProUGUI wood;
    public TextMeshProUGUI stone;
    public TextMeshProUGUI food;
    public TextMeshProUGUI money;
    public TextMeshProUGUI defense;

    // Constantly update variables
    void Update()
    {
        // Is day
        if (manager.isDay)
        {
            //dayNightTracker.color = new Color(0.25f, 0.75f, 1.0f);
            dayNumber.text = "Day " + manager.day;
        }
        else
        {
            //dayNightTracker.color = new Color(0.1f, 0.0f, 0.25f);
            dayNumber.text = "Night " + manager.day;
        }

        // Basic variables
        population.text = "Population: " + manager.population + "/" + manager.maxPopulation;
        wood.text = "Wood:       " + manager.wood;
        stone.text = "Stone:       " + manager.stone;
        food.text = "Food:       " + manager.food;
        money.text = "Money: " + manager.money + "g";
        defense.text = "Defense: " + manager.defense;
    }
}
