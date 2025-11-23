//
// Coded by Jordan Coolbeth
// Last Modified 11/23/2025
// Coded for CSI-281 Final Project
//

using System.Collections.Generic;
using UnityEngine;

// Used to declare what object an NPC is currently trying to interact with
public enum TargetObject
{
    None,
    House, 
    TradePost, 
    Defense, 
    Farm, 
    Villager, 
    Bandit,
    BuildingToDestroy
}

// Used to declare what kind of NPC this is
public enum NPCType
{
    None,
    Villager,
    Bandit
}

// Used to declare what a bandit is currently trying to steal
public enum StealTarget
{
    None,
    Wood,
    Stone,
    Food,
    Money
}

public class NPC : MonoBehaviour
{
    // Game manager and settings to access data
    public GameManager gameManager;
    public Settings settings;

    // NPC type
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
        targetLocation = transform.position;
        baseSize = transform.localScale;
    }

    private void Update()
    {
        // Keep track of time
        timer += Time.deltaTime;

        // Set sprite
        if (type == NPCType.Villager)
        {
            renderSystem.sprite = villager;
        }
        else
        {
            renderSystem.sprite = bandit;
        }

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
            interactWhenFound = TargetObject.None;

            // For villagers
            if (type == NPCType.Villager)
            {
                switch (interactionType)
                {
                    // Collect food at farms
                    case (TargetObject.Farm):
                        gameManager.food += getFoodGeneration();
                        foodCollected++;
                        break;

                    // Collect money at trade posts
                    case (TargetObject.TradePost):
                        gameManager.money += getMoneyGeneration();
                        moneyCollected++;
                        break;

                    // Have a child
                    case (TargetObject.Villager):
                        if (gameManager.population < gameManager.maxPopulation)
                        {
                            gameManager.population += getPopulationIncrease();
                            peopleMade++;
                        }
                        break;

                    // Hide in a house
                    case (TargetObject.House):
                        isHiding = true;
                        break;
    
                    // Attack a bandit
                    case (TargetObject.Bandit):
                        int winChance = 100 * gameManager.defense / (gameManager.defense + gameManager.attackStrength);
                        bool win = Random.Range(0, 100) <= winChance;

                        if (win)
                            findNearest(TargetObject.Bandit).GetComponent<NPC>().kill();
                        else
                            kill();

                        break;
                }
            }

            // For bandits
            if (type == NPCType.Bandit)
            {
                switch(interactionType)
                {
                    // Destroy a building
                    case (TargetObject.BuildingToDestroy):
                        findNearest(TargetObject.BuildingToDestroy).GetComponent<Building>().AttackBuilding();
                        break;

                    // Steal from a trade post
                    case (TargetObject.TradePost):
                        switch(currentStealTarget)
                        {
                            case(StealTarget.Wood):
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
        }

        if (shouldMove) takeStep();

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

        // Wander randomly (for testing)
        if (!interacting && !shouldMove)
        {
            int random = Random.Range(0, 10);

            if (random == 0)
            {
                MoveToRandomLocation();
            }
            else if (random == 7 && foodCollected < getFoodCap())
            {
                findAndInteract(TargetObject.Farm);
            }
            else if (random == 8 && moneyCollected < getMoneyCap())
            {
                findAndInteract(TargetObject.TradePost);
            }
            else if (random == 9 && peopleMade < getPopulationCap())
            {
                findAndInteract(TargetObject.Villager);
            }
        }

        // Use number to play certain actions (for testing)
        if(gameManager.cheats)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                MoveToRandomLocation();
                //Debug.Log("Random Movement");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                findAndInteract(TargetObject.Farm);
                //Debug.Log("Interact with farm");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                findAndInteract(TargetObject.TradePost);
                //Debug.Log("Interact with trade post");
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                findAndInteract(TargetObject.Villager);
                //Debug.Log("Interact with villager");
            }
        }
    }

    // Accepts a give location and tells the NPC to move to that location
    // If any coordinates are out of bounds it will move them in bounds
    public void MoveTo(Vector2 location)
    {
        if (location.x > gameManager.rightX) { location.x = gameManager.rightX; }
        if (location.x < gameManager.leftX) { location.x = gameManager.leftX; }
        if (location.y > gameManager.topY) { location.y = gameManager.topY; }
        if (location.y < gameManager.bottomY) { location.y = gameManager.bottomY; }
        targetLocation = location;
        //Debug.Log("Moving from " + transform.position + " to " + location);
        shouldMove = true;
    }

    // Take one step in the direction of the current targeted location
    private void takeStep()
    {
        Vector2 targetVector = new Vector2(targetLocation.x - transform.position.x, targetLocation.y - transform.position.y);
        //Debug.Log("Target Vector: " + targetVector);
        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);
        //Debug.Log("Distance: " + distanceTotal);

        //Debug.Log("Speed = " + Screen.width + " / 50000 = " + getSpeed());

        if (distanceTotal < (getSpeed() * 5))
        {
            shouldMove = false;
            if (currentlyLeaving)
            {
                kill();
            }
            if (interactWhenFound != TargetObject.None)
            {
                interact(interactWhenFound);
            }
            return;
        }

        Vector2 movement = new Vector2((targetVector.x / distanceTotal) * getSpeed(), (targetVector.y / distanceTotal) * getSpeed());
        transform.position = new Vector2(transform.position.x + movement.x, transform.position.y + movement.y);
        //Debug.Log("Step taken - New location: " + transform.position);

        int angle = (int) ((180 / Mathf.PI) * Mathf.Atan2 (movement.x, movement.y)) + 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Move to a random location in range
    public void MoveToRandomLocation()
    {
        float angle = Random.Range(0, 6.28f);
        float distance = Random.Range(0, getMaxTargetDistance() * getSpeed());
        Vector2 randomLocation = new Vector2(transform.position.x + (Mathf.Cos(angle) * distance), transform.position.y + (Mathf.Sin(angle) * distance));
        MoveTo(randomLocation);
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
        else if (target == TargetObject.Villager || target == TargetObject.Bandit)
        {
            List<GameObject> allObjects = new List<GameObject>();

            for (int i = 0; i < gameManager.NPCsList.Count; i++)
            {
                switch(target)
                {
                    case (TargetObject.Villager):
                        if (gameManager.NPCsList[i].type == NPCType.Villager && gameManager.NPCsList[i] != this && !gameManager.NPCsList[i].isHiding)
                            allObjects.Add(gameManager.NPCsList[i].gameObject);
                        else if (gameManager.NPCsList[i].type != NPCType.Villager && !gameManager.NPCsList[i].isHiding)
                            allObjects.Add(gameManager.NPCsList[i].gameObject);
                        break;

                    case (TargetObject.Bandit):
                        if (gameManager.NPCsList[i].type == NPCType.Bandit && gameManager.NPCsList[i] != this)
                            allObjects.Add(gameManager.NPCsList[i].gameObject);
                        else if (gameManager.NPCsList[i].type != NPCType.Bandit)
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

    // Finds the nearest instance of a target and automatically interacts upon reaching it
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

    // Immediately kill this npc
    public void kill()
    {
        if (type == NPCType.Villager)
        {
            gameManager.NPCsList.Remove(this);
            gameManager.population--;
            Destroy(gameObject);
        }
        else
        {
            gameManager.banditsList.Remove(this);
            Destroy(gameObject);
        }
    }

    // Used by bandits
    // Walk to the edge of the map and despawn
    public void leaveVillage()
    {
        Vector2 placeToLeave = new Vector2(0, 0);

        if (transform.position.y < 0) placeToLeave.y = gameManager.bottomY;
        else placeToLeave.y = gameManager.topY;

        if (transform.position.x < 0) placeToLeave.x = gameManager.leftX;
        else placeToLeave.x = gameManager.rightX;

        MoveTo(placeToLeave);
    }

    // Set what the NPC wants to steal right now
    public void wantToSteal(StealTarget target) { currentStealTarget = target; }

    // Change the current NPC type
    private void setType(NPCType newType) { type = newType; }

    // Use these to access necessary variables from gameManager
    public int getMaxTargetDistance() { return settings.maxTargetDistanceSteps; }
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
