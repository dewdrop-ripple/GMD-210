using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public enum TargetObject
{
    None,
    House, 
    TradePost, 
    Defense, 
    Farm, 
    Villager, 
    Bandit
}

public enum NPCType
{
    None,
    Villager,
    Bandit
}

public class NPC : MonoBehaviour
{
    // Game manager and settings to access data
    public GameManager gameManager;
    public Settings settings;

    NPCType type = NPCType.Villager;

    // For movement
    Vector2 targetLocation;
    bool shouldMove = false;

    // For interaction
    float timer = 0.0f;
    float stopInteractingAt;
    bool interacting = false;
    TargetObject interactionType;
    int animationDirection = 0;

    // For find and interact
    TargetObject interactWhenFound = TargetObject.None;

    // For animation
    Vector2 baseSize;

    // To make sure it's not doing too much
    int foodCollected = 0;
    int moneyCollected = 0;
    int peopleMade = 0;

    // Set objects to necessary objects
    private void Awake()
    {
        settings = GameObject.Find("Settings").GetComponent<Settings>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        transform.localScale = new Vector3(gameManager.scaleFactor, gameManager.scaleFactor, 1.0f);
        targetLocation = transform.position;
        baseSize = transform.localScale;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (!gameManager.isDay)
        {
            foodCollected = 0;
            moneyCollected = 0;
            peopleMade = 0;
        }

        if (timer >= stopInteractingAt && interacting)
        {
            interacting = false;
            interactWhenFound = TargetObject.None;

            switch (interactionType)
            {
                case (TargetObject.Farm):
                    gameManager.food += getFoodGeneration();
                    foodCollected++;
                    break;

                case (TargetObject.TradePost):
                    gameManager.money += getMoneyGeneration();
                    moneyCollected++;
                    break;

                case (TargetObject.Villager):
                    if (gameManager.population < gameManager.maxPopulation)
                    {
                        gameManager.population += getPopulationIncrease();
                        peopleMade++;
                    }
                    break;
            }
        }

        if (shouldMove) takeStep();

        if (interacting)
        {
            if (animationDirection < 200)
            {
                transform.localScale = new Vector3(transform.localScale.x - (Time.deltaTime / 10), transform.localScale.y - (Time.deltaTime / 10), 1);
                animationDirection++;
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x + (Time.deltaTime / 10), transform.localScale.y + (Time.deltaTime / 10), 1);
                animationDirection++;
                if (animationDirection > 400) animationDirection = 0;
            }
        }
        else
        {
            transform.localScale = new Vector3(gameManager.scaleFactor, gameManager.scaleFactor, 1.0f);
        }

        if (!interacting && !shouldMove)
        {
            int random = Random.Range(0, 4);

            if (random == 0)
            {
                MoveToRandomLocation();
                Debug.Log("Random Movement");
            }
            else if (random == 1 && foodCollected < getFoodCap())
            {
                findAndInteract(TargetObject.Farm);
            }
            else if (random == 2 && moneyCollected < getMoneyCap())
            {
                findAndInteract(TargetObject.TradePost);
            }
            else if (random == 3 && peopleMade < getPopulationCap())
            {
                findAndInteract(TargetObject.Villager);
            }
        }
    }

    public void MoveTo(Vector2 location)
    {
        if (location.x > gameManager.rightX) { location.x = gameManager.rightX; }
        if (location.x < gameManager.leftX) { location.x = gameManager.leftX; }
        if (location.y > gameManager.topY) { location.x = gameManager.topY; }
        if (location.y < gameManager.bottomY) { location.x = gameManager.bottomY; }
        targetLocation = location;
        shouldMove = true;
    }

    private void takeStep()
    {
        Vector2 targetVector = new Vector2(targetLocation.x - transform.position.x, targetLocation.y - transform.position.y);
        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);

        if (distanceTotal < 1.0f)
        {
            shouldMove = false;
            if (interactWhenFound != TargetObject.None)
            {
                interact(interactWhenFound);
            }
            return;
        }

        float stepLength = Screen.width * getSpeed();
        Vector2 movement = new Vector2(targetVector.x / (distanceTotal /  stepLength), targetVector.y / (distanceTotal / (stepLength)));
        transform.position = new Vector2(transform.position.x + movement.x, transform.position.y + movement.y);
    }

    public void MoveToRandomLocation()
    {
        float angle = Random.Range(0, 6.28f);
        float distance = Random.Range(0, getMaxTargetDistance());
        Vector2 randomLocation = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
        MoveTo(randomLocation);
    }

    public GameObject findNearest(TargetObject target)
    {
        GameObject foundObject = null;

        if (target == TargetObject.Villager || target == TargetObject.Bandit)
        {
            List<GameObject> allObjects = new List<GameObject>();

            for (int i = 0; i < gameManager.NPCsList.Count; i++)
            {
                switch(target)
                {
                    case (TargetObject.Villager):
                        if (gameManager.NPCsList[i].type == NPCType.Villager && gameManager.NPCsList[i] != this)
                            allObjects.Add(gameManager.NPCsList[i].gameObject);
                        break;

                    case (TargetObject.Bandit):
                        if (gameManager.NPCsList[i].type == NPCType.Bandit && gameManager.NPCsList[i] != this)
                            allObjects.Add(gameManager.NPCsList[i].gameObject);
                        break;
                }
            }

            if (allObjects.Count < 1) { return null; }

            float minDistance = getDistance(allObjects[0]);
            foundObject = allObjects[0];
            for (int i = 1; i < allObjects.Count; i++)
            {
                float checkDistance = getDistance(allObjects[i]);
                if (checkDistance < minDistance) 
                { 
                    minDistance = checkDistance;
                    foundObject = allObjects[i];
                }
            }
        }
        else
        {
            List<GameObject> allObjects = new List<GameObject>();

            for (int i = 0; i < gameManager.buildingsList.Count; i++)
            {
                switch (target)
                {
                    case (TargetObject.House):
                        if (gameManager.buildingsList[i].buildingArrayPosition == 0 || gameManager.buildingsList[i].buildingArrayPosition == 1 || gameManager.buildingsList[i].buildingArrayPosition == 2)
                            allObjects.Add(gameManager.buildingsList[i].gameObject);
                        break;

                    case (TargetObject.Farm):
                        if (gameManager.buildingsList[i].buildingArrayPosition == 3 || gameManager.buildingsList[i].buildingArrayPosition == 4 || gameManager.buildingsList[i].buildingArrayPosition == 5)
                            allObjects.Add(gameManager.buildingsList[i].gameObject);
                        break;

                    case (TargetObject.TradePost):
                        if (gameManager.buildingsList[i].buildingArrayPosition == 6 || gameManager.buildingsList[i].buildingArrayPosition == 7 || gameManager.buildingsList[i].buildingArrayPosition == 8)
                            allObjects.Add(gameManager.buildingsList[i].gameObject);
                        break;

                    case (TargetObject.Defense):
                        if (gameManager.buildingsList[i].buildingArrayPosition == 9 || gameManager.buildingsList[i].buildingArrayPosition == 10 || gameManager.buildingsList[i].buildingArrayPosition == 11)
                            allObjects.Add(gameManager.buildingsList[i].gameObject);
                        break;
                }
            }

            if (allObjects.Count < 1) { return null; }

            float minDistance = getDistance(allObjects[0]);
            foundObject = allObjects[0];
            for (int i = 1; i < allObjects.Count; i++)
            {
                float checkDistance = getDistance(allObjects[i]);
                if (checkDistance < minDistance)
                {
                    minDistance = checkDistance;
                    foundObject = allObjects[i];
                }
            }
        }

        return foundObject;
    }

    public float getDistance(GameObject other)
    {
        Vector3 otherLocation = other.transform.position;
        Vector2 connectingVector = new Vector2(otherLocation.x - transform.position.x, otherLocation.y - transform.position.y);
        float distanceTotal = Mathf.Sqrt(connectingVector.x * connectingVector.x + connectingVector.y * connectingVector.y);
        return distanceTotal;
    }

    public void interact(TargetObject target)
    {
        interacting = true;
        stopInteractingAt = timer + getActionDelay();
        interactionType = target;
    }

    public void findAndInteract(TargetObject target)
    {
        GameObject toFind = findNearest(target);
        
        if (toFind != null)
        {
            interactWhenFound = target;
            Vector2 newTarget = toFind.transform.position;
            MoveTo(newTarget);
        }
    }

    // Change the current NPC type
    private void setType(NPCType newType) { type = newType; }

    // Use these to access necessary variables from gameManager
    public float getMaxTargetDistance() { return settings.maxTargetDistance; }
    public int getFoodGeneration() { return settings.foodGeneration; }
    public int getMoneyGeneration() { return settings.moneyGeneration; }
    public int getPopulationIncrease() { return settings.populationIncrease; }
    public int getFoodCap()
    {
        if (type == NPCType.Villager) return settings.vFoodCap;
        else return settings.bFoodCap;
    }
    public int getMoneyCap()
    {
        if (type == NPCType.Villager) return settings.vMoneyCap;
        else return settings.bMoneyCap;
    }
    public float getSpeed()
    {
        if (type == NPCType.Villager) return settings.vSpeed;
        else return settings.bSpeed;
    }
    public int getPopulationCap() { return settings.vPopulationCap; }
    public float getActionDelay() { return settings.actionDelay; }
    public int getWoodCap() { return settings.bWoodCap; }
    public int getStoneCap() { return settings.bStoneCap; }
    public int getBuildingsCap() { return settings.buildingsCap; }
    public float getBuildingDestructionTime() { return settings.buildingDestructionTime; }
    public int getBuildingDestructionThreshhold() { return settings.buildingDestructionThreshhold; }
    public float getDestructionWaitTime() { return settings.destructionWaitTime; }
    public bool currentlyDay() { return gameManager.isDay; }
}
