using System.Collections.Generic;
using BehaviorTreeLib;
using UnityEngine;

public class Villager : MonoBehaviour
{
    // Game manager and settings to access data
    public GameManager gameManager;
    public Settings settings;

    // For movement
    Vector2 targetLocation;
    bool shouldMove = false;

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

    // variables for Behavior Trees
    BehaviorTree tree;

    Vector2 destination;

    bool hasPath = false;
    bool farmNearby = false;
    bool tradeNearby = false;
    bool villagerNearby = false;

    bool defenseNearby = false;
    bool houseNearby = false;
    bool banditNearby = false;

    int interactionTime = 100; // how long (in frames) it takes to interact
    int interactionTimeTaken = 0;

    bool canAttack = false;
    
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
        tree = new BehaviorTree("Villager");

        Selector timeOfDay = new Selector("Time");
        
        Sequence day = new Sequence("Day");
        day.AddChild(new Leaf("IsDay", new Condition(() => currentlyDay())));
        
        Sequence interactFarm = new Sequence("InteractFarm", 30);
        interactFarm.AddChild(new Leaf("FarmNearby", new Condition(() => farmNearby)));
        interactFarm.AddChild(new Leaf("FoodNotMaxed", new Condition(() => (foodCollected < getFoodCap()))));
        interactFarm.AddChild(new Leaf("MoveToFarm", new Move(MoveToward, () => GetObjectLocation(TargetObject.Farm), transform)));
        interactFarm.AddChild(new Leaf("GetFood", new InteractWithObject(() => Gather(TargetObject.Farm, interactionTime))));
        interactFarm.AddChild(new Leaf("LeaveFarm", new Move(MoveToward, () => GetRandomLocation(transform, 2f), transform)));
        
        Sequence interactTrade = new Sequence("InteractTrade", 20);
        interactTrade.AddChild(new Leaf("TradeNearby", new Condition(() => tradeNearby)));
        interactTrade.AddChild(new Leaf("MoneyNotMaxed", new Condition(() => (moneyCollected < getMoneyCap()))));
        interactTrade.AddChild(new Leaf("MoveToTrade", new Move(MoveToward, () => GetObjectLocation(TargetObject.TradePost), transform)));
        interactTrade.AddChild(new Leaf("GetMoney", new InteractWithObject(() => Gather(TargetObject.TradePost, interactionTime))));
        interactTrade.AddChild(new Leaf("LeaveTrade", new Move(MoveToward, () => GetRandomLocation(transform, 2f), transform)));
        
        Sequence interactVillager = new Sequence("InteractVillager", 10);
        interactVillager.AddChild(new Leaf("VillagerNearby", new Condition(() => villagerNearby)));
        interactVillager.AddChild(new Leaf("PopNotMaxed", new Condition(() => (peopleMade < getPopulationCap()))));
        interactVillager.AddChild(new Leaf("MoveToVillager", new Move(MoveToward, () => GetObjectLocation(TargetObject.Villager), transform)));
        interactVillager.AddChild(new Leaf("GetVillager", new InteractWithObject(() => Gather(TargetObject.Villager, interactionTime))));
        interactVillager.AddChild(new Leaf("LeaveVillager", new Move(MoveToward, () => GetRandomLocation(transform, 2f), transform)));
        
        Leaf wander = new Leaf("Wander", new Move(MoveToward, () => GetRandomLocation(transform, 1f), transform));
        
        Selector dayRoutine = new Selector("DayRoutine");
        dayRoutine.AddChild(interactFarm);
        dayRoutine.AddChild(interactTrade);
        dayRoutine.AddChild(interactVillager);
        dayRoutine.AddChild(wander);
        
        day.AddChild(dayRoutine);
        
        Sequence night = new Sequence("Night");
        night.AddChild(new Leaf("IsNight", new Condition(() => !currentlyDay())));

        // night stuff here
        Selector nightRoutine = new Selector("NightRoutine");

        UntilFail hide = new UntilFail("Hide");
        hide.AddChild(new Leaf("IsHiding", new Condition(() => isHiding)));

        Sequence attackRoutine = new Sequence("AttackRoutine");
        attackRoutine.AddChild(new Leaf("CanAttack", new Condition(() => canAttack)));

        Sequence attack = new Sequence("Attack");
        attack.AddChild(new Leaf("BanditNearby", new Condition(() => banditNearby)));
        attack.AddChild(new Leaf("MoveToBandit", new Move(MoveToward, () => GetBanditLocation(), transform)));
        //attack.AddChild( attack bandit ?????

        Selector canAttackRoutine = new Selector("CanAttackRoutine");
        canAttackRoutine.AddChild(attack);
        canAttackRoutine.AddChild(wander);

        attackRoutine.AddChild(canAttackRoutine);

        Sequence search = new Sequence("Search");
        search.AddChild(new Leaf("CantAttack", new Condition(() => !canAttack)));

        Selector searchRoutine = new Selector("SearchRoutine");

        Sequence defense = new Sequence("Defense");
        defense.AddChild(new Leaf("DefenseInRange", new Condition(() => defenseNearby)));
        defense.AddChild(new Leaf("MoveToDefense", new Move(MoveToward, () => GetObjectLocation(TargetObject.Defense), transform)));
        defense.AddChild(new Leaf("StartAttacking", new ActionStrategy(() => startAttacking())));

        Sequence house = new Sequence("House");
        house.AddChild(new Leaf("HouseInRange", new Condition(() => houseNearby)));
        house.AddChild(new Leaf("MoveToHouse", new Move(MoveToward, () => GetObjectLocation(TargetObject.House), transform)));
        house.AddChild(new Leaf("StartHiding", new ActionStrategy(() => startHiding())));

        searchRoutine.AddChild(defense);
        searchRoutine.AddChild(house);
        searchRoutine.AddChild(wander);

        search.AddChild(searchRoutine);

        nightRoutine.AddChild(hide);
        nightRoutine.AddChild(attackRoutine);
        nightRoutine.AddChild(search);

        night.AddChild(nightRoutine);

        timeOfDay.AddChild(day);
        timeOfDay.AddChild(night);
        
        tree.AddChild(timeOfDay);
    }

    private void Update()
    {
        // Keep track of time
        timer += Time.deltaTime;

        if (gameManager.isDay) {
            if (checkForNearby(TargetObject.Villager)) {
                villagerNearby = true;
            }
            if (checkForNearby(TargetObject.TradePost)) {
                tradeNearby = true;
            }
            if (checkForNearby(TargetObject.Farm)) {
                farmNearby = true;
            }
        } else {
            if (checkForNearby(TargetObject.Bandit)) {
                banditNearby = true;
            }
            if (checkForNearby(TargetObject.Defense)) {
                defenseNearby = true;
            }
            if (checkForNearby(TargetObject.House)) {
                houseNearby = true;
            }
        }

        Node.Status status = tree.Process();
        if (status != Node.Status.RUNNING) {
            tree.Reset();
        }

        // Reset necessary data during the oposite time
        if (gameManager.isDay)
        {
            isHiding = false;
            canAttack = false;
        }
        else
        {
            foodCollected = 0;
            moneyCollected = 0;
            peopleMade = 0;
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
                        findNearest(TargetObject.Bandit).GetComponent<Bandit>().kill();
                    else
                        kill();

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

    public bool Gather(TargetObject target, int time) {
        interactionTimeTaken++;
        if (interactionTimeTaken < time) {
            return false;
        }
        switch (target){
            case (TargetObject.Farm):
                gameManager.food += getFoodGeneration();
                foodCollected += getFoodGeneration();
                break;
            case (TargetObject.TradePost):
                gameManager.money += getMoneyGeneration();
                moneyCollected += getFoodGeneration();
                break;
            case (TargetObject.Villager):
                gameManager.population += getPopulationIncrease();
                peopleMade += getFoodGeneration();
                break;
        }
        interactionTimeTaken = 0;
        return true;
    }

    public void startHiding() {
        isHiding = true;
    }

    public void startAttacking() {
        canAttack = true;
    }

    // Accepts a give location and tells the NPC to move to that location
    // If any coordinates are out of bounds it will move them in bounds
    public void MoveToward(Vector2 location) {

        interactionTimeTaken = 0;
        Vector2 targetVector = new Vector2(location.x - transform.position.x, location.y - transform.position.y);
        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);

        Vector2 movement = new Vector2((targetVector.x / distanceTotal) * getSpeed(), (targetVector.y / distanceTotal) * getSpeed());
        transform.position = new Vector2(transform.position.x + movement.x, transform.position.y + movement.y);

        int angle = (int)((180 / Mathf.PI) * Mathf.Atan2(movement.x, movement.y)) + 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (Vector2.Distance(transform.position, location) < 0.1) {
            transform.position = new Vector3(location.x, location.y, transform.position.z);
            hasPath = false;
            farmNearby = false;
            tradeNearby = false;
            villagerNearby = false;
        }
    }

    // gets a random location within villager's range
    public Vector2 GetRandomLocation(Transform origin, float distanceModifier) {
        if (hasPath) {
            return destination;
        }
        float xPos = Random.Range(-(float)getMaxTargetDistance() * getSpeed() * distanceModifier, (float)getMaxTargetDistance() * getSpeed() * distanceModifier) + origin.position.x;
        float yPos = Random.Range(-(float)getMaxTargetDistance() * getSpeed() * distanceModifier, (float)getMaxTargetDistance() * getSpeed() * distanceModifier) + origin.position.y;

        // keeps it inbound
        if (xPos > gameManager.rightX) { xPos = gameManager.rightX; }
        if (xPos < -gameManager.rightX) { xPos = -gameManager.rightX; }
        if (yPos > gameManager.topY) { yPos = gameManager.topY; }
        if (yPos < -gameManager.topY) { yPos = -gameManager.topY; }

        destination = new Vector2(xPos, yPos);
        hasPath = true;
        return destination;
    }

    public Vector2 GetObjectLocation(TargetObject target) {
        if (hasPath) {
            return destination;
        }
        destination = findNearest(target).transform.position;
        hasPath = true;
        return destination;
    }

    public Vector2 GetBanditLocation() {
        destination = findNearest(TargetObject.Bandit).transform.position;
        hasPath = true;
        if (destination == null|| Vector2.Distance(destination, transform.position) > 0.1) {
            destination = transform.position;
        }
        return destination;
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
                if (gameManager.NPCsList[i] != this)
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

    public bool checkForNearby(TargetObject target) {
        if (findNearest(target) == null) {
            //Debug.Log("Nothing Found");
            return false;
        }

        if (getDistance(findNearest(target)) > (getSpeed() * getMaxTargetDistance())) {
            return false;
        }
        return true;
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
        gameManager.NPCsList.Remove(this);
        gameManager.population--;
        Destroy(gameObject);
    }

    // Use these to access necessary variables from gameManager
    public int getMaxTargetDistance() { return settings.maxTargetDistanceSteps; }
    public int getFoodGeneration() { return settings.foodGeneration; }
    public int getMoneyGeneration() { return settings.moneyGeneration; }
    public int getPopulationIncrease() { return settings.populationIncrease; }
    public int getFoodCap() { return settings.vFoodCap; }
    public int getMoneyCap() { return settings.vMoneyCap; }
    public float getSpeed() { return settings.vSpeed; }
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
