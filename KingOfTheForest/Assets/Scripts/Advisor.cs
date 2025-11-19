using TMPro;
using UnityEngine;

public class Advisor : MonoBehaviour
{
    // Text objects
    public Canvas textCanvas;
    public TextMeshProUGUI text;

    // For game state
    public GameManager gameManager;

    // Settings object
    public Settings settings;

    // Which text options have happened so far?
    public bool startDone = false;
    public bool miningFoodDone = false;
    public bool miningDone = false;
    public bool buildingFood1Done = false;
    public bool gardenDone = false;
    public bool cottageDone = false;
    public bool buildingFood2Done = false;
    public bool towerDone = false;
    public bool firstDayDone = false;
    public bool tradeDone = false;
    public bool populationMilestoneDone = false;

    // States
    public bool miningTutorialActive = false;
    public bool buildingTutorialActive = false;

    // Should the text switch?
    public int textTarget = 1;
    public bool continueText = false;

    // Set settings object
    private void Awake()
    {
        settings = GameObject.Find("Settings").GetComponent<Settings>();
    }

    private void Start()
    {
        textCanvas.enabled = false;
    }

    // Check to start tutorial sections
    private void Update()
    {
        gameManager.tutorialTextOpen = textCanvas.enabled;

        if (continueText) 
        { 
            textTarget++; 
            continueText = false;
        }

        if (settings.tutorialEnabled && gameManager.isDay)
        {
            if (!startDone) { startTutorial(); }
            else if (!miningFoodDone && miningTutorialActive && gameManager.food <= 0) { miningFood(); }
            else if (!miningDone && startDone && gameManager.wood >= (30 * settings.difficultyScaler) && gameManager.stone >= (30 * settings.difficultyScaler)) { mining(); }
            else if (!buildingFood1Done && buildingTutorialActive && gameManager.food <= 0) { buildingFood1(); }
            else if (!buildingFood2Done && buildingTutorialActive && gameManager.food <= 0) { buildingFood2(); }
            else if (!gardenDone && miningDone && (gameManager.buildings[3] + gameManager.buildings[4] + gameManager.buildings[5]) > 0) { garden(); }
            else if (!cottageDone && gardenDone && gameManager.maxPopulation > 5) { cottage(); }
            else if (!towerDone && cottageDone && gameManager.defense > 0) { tower(); }
            else if (!firstDayDone && towerDone && gameManager.day > 1) { firstDay(); }
            else if (!tradeDone && gameManager.canTrade) { trade(); }
            else if (!populationMilestoneDone && gameManager.population >= 200) { populationMilestone(); }
        }
        else if (!settings.tutorialEnabled)
        {
            miningTutorialActive = false;
            buildingTutorialActive = false;
            textCanvas.enabled = false;
            gameManager.tutorialTextOpen = false;
            gameManager.bannedFromMining = false;
            gameManager.nextDay.enabled = true;
        }
    }

    private void startTutorial()
    {
        textCanvas.enabled = true;
        gameManager.nextDay.enabled = false;

        switch(textTarget)
        {
            case (1):
                text.text = "Congratulations! All your pestering finally paid off and the council has decided to give you your own kingdom!";
                break;

            case (2):
                text.text = "Um, in a way.";
                break;

            case (3):
                text.text = "This is the old forest kingdom. It used to be pretty big, but it was abandoned years ago.";
                break;

            case (4):
                text.text = "Now all that’s left is this one hut and a bunch of trees. And rocks.";
                break;

            case (5):
                text.text = "Technically, it’s not considered a kingdom right now. The population is too small.";
                break;

            case (6):
                text.text = "Not to worry! I’m sure you’ll get it fixed up in no time!";
                break;

            case (7):
                text.text = "As this place grows, more and more people will move in! And if enough people decide to live here, you’ll officially be inducted into the council!";
                break;

            case (8):
                text.text = "You’ll need a population of about 500 people for that, though. Better get to work!";
                break;

            case (9):
                text.text = "Why don’t we start by mining some trees and rocks? Just keep an eye on your food, you don’t want to overwork your people.";
                break;

            case (10):
                text.text = "You should need… about " + (30 * settings.difficultyScaler) + " wood and " + (30 * settings.difficultyScaler) + " stone.";
                break;

            default:
                textCanvas.enabled = false;
                startDone = true;
                miningTutorialActive = true;
                textTarget = 1;
                break;
        }
    }

    private void miningFood()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "See? I warned you.";
                break;

            case (2):
                text.text = "Take this, and stop mining. You still have some building to do.";
                break;

            default:
                gameManager.food += 20;
                textCanvas.enabled = false;
                miningFoodDone = true;
                gameManager.bannedFromMining = true;
                textTarget = 1;
                break;
        }
    }

    private void mining()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "Perfect! Now let's build a garden. We need to feed these people!";
                break;

            case (2):
                text.text = "Keep an eye on your food still! Building takes energy.";
                break;

            case (3):
                text.text = "If you aren’t happy with where you placed it, you can always destroy it and try again, but that costs food. Also, you won’t get everything back.";
                break;

            default:
                textCanvas.enabled = false;
                miningDone = true;
                textTarget = 1;
                break;
        }
    }

    private void buildingFood1()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "What did I tell you?";
                break;

            case (2):
                text.text = "Take this food, and finish building. I’m not giving you any more.";
                break;

            default:
                textCanvas.enabled = false;
                gameManager.food += 20;
                buildingFood1Done = true;
                textTarget = 1;
                break;
        }
    }

    private void garden()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "What a lovely garden!";
                break;

            case (2):
                text.text = "Now, people may start moving in, and we need places to house them.";
                break;

            case (3):
                text.text = "Why don’t you build another cottage?";
                break;

            default:
                textCanvas.enabled = false;
                miningTutorialActive = false;
                buildingTutorialActive = true;
                gardenDone = true;
                textTarget = 1;
                break;
        }
    }

    private void cottage()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "Excellent!";
                break;

            case (2):
                text.text = "As your kingdom grows, you may be attacked by bandits in the night. They may steal resources from you, kill your people, and even smash buildings!";
                break;

            case (3):
                text.text = "You should build an archer tower to defend against them.";
                break;

            default:
                textCanvas.enabled = false;
                cottageDone = true;
                textTarget = 1;
                break;
        }
    }

    private void buildingFood2()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "You are ridiculous!";
                break;

            case (2):
                text.text = "You don’t want to listen to me? You know better than me?";
                break;

            case (3):
                text.text = "Fine! Do what you want! I don’t care! I quit!";
                break;

            default:
                textCanvas.enabled = false;
                buildingFood2Done = true;
                settings.tutorialEnabled = false;
                textTarget = 1;
                break;
        }
    }

    private void tower()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "What a lovely little kingdom!";
                break;

            case (2):
                text.text = "I think you’re ready to end the day.";
                break;

            default:
                textCanvas.enabled = false;
                gameManager.nextDay.enabled = true;
                buildingTutorialActive = false;
                towerDone = true;
                textTarget = 1;
                break;
        }
    }

    private void firstDay()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "It seems like you’re doing well.";
                break;

            case (2):
                text.text = "I have other work to attend to, but I’ll check in occasionally. Good luck!";
                break;

            default:
                textCanvas.enabled = false;
                gameManager.bannedFromMining = false;
                firstDayDone = true;
                textTarget = 1;
                break;
        }
    }

    private void trade()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "Just checking in!";
                break;

            case (2):
                text.text = "Wow! You’ve really started building the economy of this place.";
                break;

            case (3):
                text.text = "Now that you can trade, you can buy and sell wood, stone, and food.";
                break;

            case (4):
                text.text = "Wood and stone cost 1g each and can be bought and sold in bundles of 10.";
                break;

            case (5):
                text.text = "Food costs 2g to buy and sells for half of a gold piece, it can be bought in bundles of 5 and sold in bundles of 10.";
                break;

            case (6):
                text.text = "It seems like you're doing well! I’ll be letting the council know of your progress.";
                break;

            default:
                textCanvas.enabled = false;
                tradeDone = true;
                textTarget = 1;
                break;
        }
    }

    private void populationMilestone()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "Goodness! You’ve been working hard.";
                break;

            case (2):
                text.text = "At this rate, you’ll be inducted in no time!";
                break;

            default:
                textCanvas.enabled = false;
                populationMilestoneDone = true;
                textTarget = 1;
                break;
        }
    }

    public void gameWon()
    {
        textCanvas.enabled = true;

        switch (textTarget)
        {
            case (1):
                text.text = "Hello again! I just wanted to-";
                break;

            case (2):
                text.text = "Oh my! This place really looks like a kingdom now.";
                break;

            case (3):
                text.text = "How many people live here? " + gameManager.population + "!";
                break;

            case (4):
                text.text = "Well congratulations! I’ll be letting the council know of this immediately!";
                break;

            case (5):
                text.text = "It has been an honor, my liege.";
                break;

            default:
                textCanvas.enabled = false;
                gameManager.winScreen.enabled = true;
                textTarget = 1;
                break;
        }
    }

    // Player wants to move on to the next thing
    public void continueButtonPressed()
    {
        continueText = true;
    }
}
