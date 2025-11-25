//
// Coded by Jordan Coolbeth
// Last Modified 11/25/2025
// Coded for CSI-281 Final Project
//

using System.Collections.Generic;
using UnityEngine;

public class Bandit : MonoBehaviour
{
    // Game manager and settings to access data
    public GameManager gameManager;
    public Settings settings;

    // For interaction
    float timer = 0.0f;
    float stopInteractingAt;
    bool interacting = false;
    TargetObject interactionType;
    int animationDirection = 0;

    // For animation
    Vector2 baseSize;

    // To make sure it's not doing too much
    int foodCollected = 0;
    int moneyCollected = 0;
    int peopleMade = 0;

    // For hiding
    public bool isHiding = false;

    // For changing state
    public Collider2D colliderSystem;
    public SpriteRenderer renderSystem;

    // For bandits only
    StealTarget currentStealTarget = StealTarget.None;
    bool currentlyLeaving = false;

    // Sprites
    public Sprite villager;
    public Sprite bandit;

    // Set base data
    private void Awake()
    {
        // Set objects
        settings = GameObject.Find("Settings").GetComponent<Settings>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Set size and location
        transform.localScale = new Vector3(gameManager.scaleFactor, gameManager.scaleFactor, 1.0f);
        baseSize = transform.localScale;
    }

    private void Update()
    {
        // Keep track of time
        timer += Time.deltaTime;

        // Reset necessary data during the oposite time
        if (!gameManager.isDay)
        {
            foodCollected = 0;
            moneyCollected = 0;
            peopleMade = 0;
        }
        else
        {
            isHiding = false;
        }

        // Dissappear and turn of collisions
        if (isHiding)
        {
            //colliderSystem.enabled = false;
            renderSystem.enabled = false;
        }
        else
        {
            //colliderSystem.enabled = true;
            renderSystem.enabled = true;
        }

        // When done with interaction animation
        if (timer >= stopInteractingAt && interacting)
        {
            interacting = false;

            switch (interactionType)
            {
                // Destroy a building
                case (TargetObject.BuildingToDestroy):
                    findNearest(TargetObject.BuildingToDestroy).GetComponent<Building>().AttackBuilding();
                    break;

                // Steal from a trade post
                case (TargetObject.TradePost):
                    switch (currentStealTarget)
                    {
                        case (StealTarget.Wood):
                            if (gameManager.wood > 0)
                                gameManager.wood--;
                            break;

                        case (StealTarget.Stone):
                            if (gameManager.stone > 0)
                                gameManager.stone--;
                            break;

                        case (StealTarget.Money):
                            if (gameManager.money > 0)
                                gameManager.money--;
                            break;
                    }
                    break;

                // Steal from a farm
                case (TargetObject.Farm):
                    if (gameManager.food > 0)
                        gameManager.food--;
                    break;
            }
        }

        // Play animation
        if (interacting)
        {
            if (animationDirection < 50)
            {
                transform.localScale = new Vector3(transform.localScale.x + (Time.deltaTime / 5), transform.localScale.y + (Time.deltaTime / 5), 1);
                animationDirection++;
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x - (Time.deltaTime / 5), transform.localScale.y - (Time.deltaTime / 5), 1);
                animationDirection++;
                if (animationDirection > 100) animationDirection = 0;
            }
        }
        else
        {
            transform.localScale = new Vector3(gameManager.scaleFactor, gameManager.scaleFactor, 1.0f);
            animationDirection = 0;
        }
    }

    // Accepts a give location and tells the NPC to move to that location
    // If any coordinates are out of bounds it will move them in bounds
    public void MoveToward(Vector2 location)
    {
        if (location.x > gameManager.rightX) { location.x = gameManager.rightX; }
        if (location.x < gameManager.leftX) { location.x = gameManager.leftX; }
        if (location.y > gameManager.topY) { location.y = gameManager.topY; }
        if (location.y < gameManager.bottomY) { location.y = gameManager.bottomY; }

        Vector2 targetVector = new Vector2(location.x - transform.position.x, location.y - transform.position.y);
        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);

        Vector2 movement = new Vector2((targetVector.x / distanceTotal) * getSpeed(), (targetVector.y / distanceTotal) * getSpeed());
        transform.position = new Vector2(transform.position.x + movement.x, transform.position.y + movement.y);

        int angle = (int)((180 / Mathf.PI) * Mathf.Atan2(movement.x, movement.y)) + 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Returns a reference to the nearest object of a given type
    public GameObject findNearest(TargetObject target)
    {
        GameObject foundObject = null;

        if (target == TargetObject.BuildingToDestroy)
        {
            if (gameManager.buildingsList.Count < 1) { return null; }

            float minDistance = getDistance(gameManager.buildingsList[0].gameObject);
            foundObject = gameManager.buildingsList[0].gameObject;
            for (int i = 1; i < gameManager.buildingsList.Count; i++)
            {
                float checkDistance = getDistance(gameManager.buildingsList[i].gameObject);
                if (checkDistance < minDistance)
                {
                    minDistance = checkDistance;
                    foundObject = gameManager.buildingsList[i].gameObject;
                }
            }
        }
        else if (target == TargetObject.Villager)
        {
            List<GameObject> allObjects = new List<GameObject>();

            for (int i = 0; i < gameManager.NPCsList.Count; i++)
            {
                allObjects.Add(gameManager.NPCsList[i].gameObject);
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
        else if (target == TargetObject.Bandit)
        {
            List<GameObject> allObjects = new List<GameObject>();

            for (int i = 0; i < gameManager.banditsList.Count; i++)
            {
                if (gameManager.banditsList[i] != this)
                    allObjects.Add(gameManager.banditsList[i].gameObject);
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

    // Returns the distance between this object and another
    public float getDistance(GameObject other)
    {
        Vector3 otherLocation = other.transform.position;
        Vector2 connectingVector = new Vector2(otherLocation.x - transform.position.x, otherLocation.y - transform.position.y);
        float distanceTotal = Mathf.Sqrt(connectingVector.x * connectingVector.x + connectingVector.y * connectingVector.y);
        return distanceTotal;
    }

    // Triggers and interaction between the NPC and a target
    // Will not work if the nearest instance of that target is more than five steps away
    public void interact(TargetObject target)
    {
        if (getDistance(findNearest(target)) > (getSpeed() * 5))
            return;

        interacting = true;
        stopInteractingAt = timer + getActionDelay();
        interactionType = target;
    }

    // Immediately kill this npc
    public void kill()
    {
        gameManager.banditsList.Remove(this);
        Destroy(gameObject);
    }

    // Set what the NPC wants to steal right now
    public void wantToSteal(StealTarget target) { currentStealTarget = target; }

    // Use these to access necessary variables from gameManager
    public int getMaxTargetDistance() { return settings.maxTargetDistanceSteps; }
    public int getFoodGeneration() { return settings.foodGeneration; }
    public int getMoneyGeneration() { return settings.moneyGeneration; }
    public int getPopulationIncrease() { return settings.populationIncrease; }
    public int getFoodCap() { return settings.bFoodCap; }
    public int getMoneyCap() { return settings.bMoneyCap; }
    public float getSpeed() { return settings.bSpeed; }
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
