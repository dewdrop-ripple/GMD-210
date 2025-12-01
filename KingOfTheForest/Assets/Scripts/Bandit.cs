//
// Coded by Jordan Coolbeth
// Last Modified 11/25/2025
// Coded for CSI-281 Final Project
//

using System.Collections.Generic;
using BehaviorTreeLib;
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
    int stoneStolen = 0;
    int woodStolen = 0;
    int moneyStolen = 0;
    int foodStolen = 0;

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

    // variables for behavior tree
    BehaviorTree tree;

    Vector2 destination;

    bool hasPath = false;

    int interactionTime = 100; // how long (in frames) it takes to interact
    int interactionTimeTaken = 0;



    // Set base data
    private void Awake()
    {
        // Set objects
        settings = GameObject.Find("Settings").GetComponent<Settings>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Set size and location
        transform.localScale = new Vector3(gameManager.scaleFactor, gameManager.scaleFactor, 1.0f);
        baseSize = transform.localScale;

        // get interaction time
        interactionTime = (int)(interactionTime * getActionDelay());

        // build tree
        tree = new BehaviorTree("BanditTree");

        Selector timeOfDay = new Selector("TimeOfDay");

        Sequence night = new Sequence("Night");
        night.AddChild(new Leaf("isNight", new Condition(() => !currentlyDay())));
        //timeOfDay.AddChild(new Leaf("isDay", new Condition(() => currentlyDay())));

        Selector nightRoutine = new Selector("NightRoutine");

        Leaf tradePostExists = new Leaf("TradePostExists", new Condition(() => (findNearest(TargetObject.TradePost) != null)));
        Leaf moveToTradePost = new Leaf("MoveToTradePost", new Move(MoveToward, () => GetObjectLocation(TargetObject.TradePost), transform));
        Leaf onTradePost = new Leaf("OnTradePost", new Condition(() => (Vector2.Distance(findNearest(TargetObject.TradePost).transform.position, transform.position) < 0.1)));

        Leaf farmExists = new Leaf("FarmExists", new Condition(() => (findNearest(TargetObject.Farm) != null)));

        // Steal Stone
        Sequence stealStone = new Sequence("StealStone");
        stealStone.AddChild(new Leaf("StoneNotMaxed", new Condition(() => ((stoneStolen < getStoneCap()) && (gameManager.stone > 0)))));
        stealStone.AddChild(tradePostExists);
        stealStone.AddChild(moveToTradePost);
        stealStone.AddChild(tradePostExists);
        stealStone.AddChild(onTradePost);
        stealStone.AddChild(new Leaf("GetStone", new InteractWithObject(() => Steal(StealTarget.Stone, interactionTime))));

        Sequence stealWood = new Sequence("StealWood");
        stealWood.AddChild(new Leaf("WoodNotMaxed", new Condition(() => ((woodStolen < getWoodCap()) && (gameManager.wood > 0)))));
        stealWood.AddChild(tradePostExists);
        stealWood.AddChild(moveToTradePost);
        stealWood.AddChild(tradePostExists);
        stealWood.AddChild(onTradePost);
        stealWood.AddChild(new Leaf("GetStone", new InteractWithObject(() => Steal(StealTarget.Wood, interactionTime))));

        Sequence stealMoney = new Sequence("StealMoney");
        stealMoney.AddChild(new Leaf("MoneyNotMaxed", new Condition(() => ((moneyStolen < getMoneyCap()) && (gameManager.money > 0)))));
        stealMoney.AddChild(tradePostExists);
        stealMoney.AddChild(moveToTradePost);
        stealMoney.AddChild(tradePostExists);
        stealMoney.AddChild(onTradePost);
        stealMoney.AddChild(new Leaf("GetMoney", new InteractWithObject(() => Steal(StealTarget.Money, interactionTime))));

        Sequence stealFood = new Sequence("StealFood");
        stealFood.AddChild(new Leaf("FoodNotMaxed", new Condition(() => ((foodStolen < getFoodCap()) && (gameManager.food > 0)))));
        stealFood.AddChild(farmExists);
        stealFood.AddChild(new Leaf("MoveToFarm", new Move(MoveToward, () => GetObjectLocation(TargetObject.Farm), transform)));
        stealFood.AddChild(farmExists);
        stealFood.AddChild(new Leaf("OnFarm", new Condition(() => (Vector2.Distance(findNearest(TargetObject.Farm).transform.position, transform.position) < 0.1))));
        stealFood.AddChild(new Leaf("GetFood", new InteractWithObject(() => Steal(StealTarget.Food, interactionTime))));

        // Destroy Building

        // Leave

        nightRoutine.AddChild(stealStone);
        nightRoutine.AddChild(stealWood);
        nightRoutine.AddChild(stealMoney);
        nightRoutine.AddChild(stealFood);

        night.AddChild(nightRoutine);

        timeOfDay.AddChild(night);

        tree.AddChild(timeOfDay);
    }

    private void Update()
    {
            // Keep track of time
            timer += Time.deltaTime;

        //if (findNearest(TargetObject.Villager) == null)
        //{
        //    Debug.Log("Villager Found");
        //}
        //else if (Vector2.Distance(findNearest(TargetObject.Villager).transform.position, transform.position) < 0.5f)
        //{
        //    kill();
        //}

        Node.Status status = tree.Process();
        if (status != Node.Status.RUNNING)
        {
            tree.Reset();
        }
    }

    // Accepts a give location and tells the NPC to move to that location
    // If any coordinates are out of bounds it will move them in bounds
    public void MoveToward(Vector2 location)
    {
        interactionTimeTaken = 0;
        Vector2 targetVector = new Vector2(location.x - transform.position.x, location.y - transform.position.y);
        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);

        Vector2 movement = new Vector2((targetVector.x / distanceTotal) * getSpeed(), (targetVector.y / distanceTotal) * getSpeed());
        transform.position = new Vector2(transform.position.x + movement.x, transform.position.y + movement.y);

        int angle = (int)((180 / Mathf.PI) * Mathf.Atan2(movement.x, movement.y)) + 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (Vector2.Distance(transform.position, location) < 0.1)
        {
            transform.position = new Vector3(location.x, location.y, transform.position.z);
            hasPath = false;
        }
    }

    public Vector2 GetObjectLocation(TargetObject target)
    {
        Debug.Log("Going");
        if (hasPath)
        {
            return destination;
        }
        destination = findNearest(target).transform.position;
        hasPath = true;
        return destination;
    }

    public bool Steal(StealTarget target, int time)
    {
        interactionTimeTaken++;
        if (interactionTimeTaken < time)
        {
            return false;
        }
        switch (target)
        {
            case (StealTarget.Stone):
                gameManager.stone -= 1;
                stoneStolen++;
                break;
            case (StealTarget.Wood):
                gameManager.wood -= 1;
                woodStolen++;
                break;
            case (StealTarget.Money):
                gameManager.money -= 1;
                moneyStolen++;
                break;
            case (StealTarget.Food):
                gameManager.food -= 1;
                foodStolen++;
                break;
        }
        interactionTimeTaken = 0;
        return true;
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
