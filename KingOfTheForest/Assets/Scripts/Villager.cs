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

    [SerializeField] Vector2 destination;
    bool hasPath = false;

    // Strategies

    //public class MoveToLocation : IStrategy {
    //    readonly Vector2 location;
    //    Transform villager;
    //    readonly float speed;
    //    public MoveToLocation(Vector2 location, Transform villager, float speed) {
    //        this.location = location;
    //        this.villager = villager;
    //        this.speed = speed;
    //    }
    //
    //    public Node.Status Process() {
    //        if (Vector3.Distance(location, villager.position) < 0.1) {
    //            Debug.Log("Success");
    //
    //            return Node.Status.SUCCESS;
    //        }
    //
    //        Vector2 targetVector = new Vector2(location.x - villager.position.x, location.y - villager.position.y);
    //        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);
    //
    //        Vector2 movement = new Vector2((targetVector.x / distanceTotal) * speed, (targetVector.y / distanceTotal) * speed);
    //        villager.position = new Vector2(villager.position.x + movement.x, villager.position.y + movement.y);
    //
    //        int angle = (int)((180 / Mathf.PI) * Mathf.Atan2(movement.x, movement.y)) + 90;
    //        villager.rotation = Quaternion.Euler(0, 0, angle);
    //
    //        Debug.Log(location);
    //        return Node.Status.RUNNING;
    //    }
    //
    //    public void Reset() { }
    //}
    //
    //public class MoveToRandomLocation : IStrategy {
    //    Transform villager;
    //    readonly float speed;
    //    readonly float maxDistance;
    //    readonly float xBound;
    //    readonly float yBound;
    //    bool locationMade = false;
    //    public MoveToRandomLocation(Transform villager, float speed, float maxDistance, float xBound, float yBound) {
    //        this.villager = villager;
    //        this.speed = speed;
    //        this.maxDistance = maxDistance;
    //        this.xBound = xBound;
    //        this.yBound = yBound;
    //    }
    //
    //    public Node.Status Process() {
    //        Vector2 target = new Vector2(0f, 0f);
    //        if (!locationMade) {
    //            locationMade = true;
    //            float xLoc = Random.Range(-(float)maxDistance, (float)maxDistance) + villager.position.x;
    //            float yLoc = Random.Range(-(float)maxDistance, (float)maxDistance) + villager.position.y;
    //
    //            if (xLoc > xBound) { xLoc = xBound; }
    //            if (xLoc < -xBound) { xLoc = -xBound; }
    //            if (yLoc > yBound) { yLoc = yBound; }
    //            if (yLoc < -yBound) { yLoc = -yBound; }
    //
    //            target = new Vector2(xLoc, yLoc);
    //        }
    //        Debug.Log(target);
    //        Debug.Log(Vector3.Distance(target, villager.position));
    //
    //        if (Vector3.Distance(new Vector3(target.x, target.y, villager.position.z), villager.position) < 0.1) {
    //            Debug.Log("Success");
    //
    //            locationMade = false;
    //            return Node.Status.SUCCESS;
    //        }
    //
    //        Vector2 targetVector = new Vector2(target.x - villager.position.x, target.y - villager.position.y);
    //        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);
    //
    //        Vector2 movement = new Vector2((targetVector.x / distanceTotal) * speed, (targetVector.y / distanceTotal) * speed);
    //        villager.position = new Vector2(villager.position.x + movement.x, villager.position.y + movement.y);
    //
    //        int angle = (int)((180 / Mathf.PI) * Mathf.Atan2(movement.x, movement.y)) + 90;
    //        villager.rotation = Quaternion.Euler(0, 0, angle);
    //        
    //        return Node.Status.RUNNING;
    //    }
    //
    //    public void Reset() { }
    //}
    
    // Set base data
    private void Awake()
    {
        // Set objects
        settings = GameObject.Find("Settings").GetComponent<Settings>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Set size and location
        transform.localScale = new Vector3(gameManager.scaleFactor, gameManager.scaleFactor, 1.0f);
        baseSize = transform.localScale;

        // build tree
        tree = new BehaviorTree("Villager");

        Leaf moveToLocation = new Leaf("MoveToLocation", new Move(() => MoveToward(destination), destination, transform));

        tree.AddChild(moveToLocation);
    }

    private void Update()
    {
        // Keep track of time
        timer += Time.deltaTime;

        // process
        if (!hasPath) {
            hasPath = true;
            destination = GetRandomLocation();
            Debug.Log(destination);
        }
        tree.Process();

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

    // Accepts a give location and tells the NPC to move to that location
    // If any coordinates are out of bounds it will move them in bounds
    public void MoveToward(Vector2 location) {
        Vector2 targetVector = new Vector2(location.x - transform.position.x, location.y - transform.position.y);
        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);

        Vector2 movement = new Vector2((targetVector.x / distanceTotal) * getSpeed(), (targetVector.y / distanceTotal) * getSpeed());
        transform.position = new Vector2(transform.position.x + movement.x, transform.position.y + movement.y);

        int angle = (int)((180 / Mathf.PI) * Mathf.Atan2(movement.x, movement.y)) + 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (Vector2.Distance(transform.position, location) < 0.1) {
            transform.position = new Vector3(location.x, location.y, transform.position.z);
            hasPath = false;
        }
    }

    //public void MoveToward(Vector2 target) {
    //    StartCoroutine(MoveTowardOverTime(target));
    //}
    //
    //public System.Collections.IEnumerator MoveTowardOverTime(Vector2 location)
    //{
    //    while (Vector2.Distance(transform.position, location) > 0.1) {// while not there yet
    //        Vector2 targetVector = new Vector2(location.x - transform.position.x, location.y - transform.position.y);
    //        float distanceTotal = Mathf.Sqrt(targetVector.x * targetVector.x + targetVector.y * targetVector.y);
    //
    //        Vector2 movement = new Vector2((targetVector.x / distanceTotal) * getSpeed(), (targetVector.y / distanceTotal) * getSpeed());
    //        transform.position = new Vector2(transform.position.x + movement.x, transform.position.y + movement.y);
    //
    //        int angle = (int)((180 / Mathf.PI) * Mathf.Atan2(movement.x, movement.y)) + 90;
    //        transform.rotation = Quaternion.Euler(0, 0, angle);
    //
    //        yield return null;
    //    }
    //
    //    Debug.Log("Done Moving");
    //    transform.position = new Vector3(location.x, location.y, transform.position.z);
    //
    //    hasPath = false;
    //}

    public Vector2 GetRandomLocation() {
        float xLoc = Random.Range(-(float)getMaxTargetDistance() * getSpeed(), (float)getMaxTargetDistance() * getSpeed()) + transform.position.x;
        float yLoc = Random.Range(-(float)getMaxTargetDistance() * getSpeed(), (float)getMaxTargetDistance() * getSpeed()) + transform.position.y;

        // keeps it inbound
        if (xLoc > gameManager.rightX) { xLoc = gameManager.rightX; }
        if (xLoc < -gameManager.rightX) { xLoc = -gameManager.rightX; }
        if (yLoc > gameManager.topY) { yLoc = gameManager.topY; }
        if (yLoc < -gameManager.topY) { yLoc = -gameManager.topY; }

        return new Vector2(xLoc, yLoc);
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
