using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Building : MonoBehaviour
{
    public int stoneCost;
    public int woodCost;
    public int maxNum;
    private bool isColliding = false;
    private bool isBuilding = false;
    int buildingArrayPosition;
    public GameManager manager;

    // Test
    // int testCounter = 0;

    // Set default values
    private void Start()
    {
        stoneCost = 0;
        woodCost = 0;
        maxNum = 100;
        buildingArrayPosition = 0;
    }

    // Updates data and returns true if it can be built
    public void StartBuild()
    {
        isBuilding = true;
    }

    // Check if we can build the thing and, if so, build it
    public void EndBuild()
    {
        isBuilding = false;

        // If any issues, don't build
        if (isColliding || manager.wood < woodCost || manager.stone < stoneCost || manager.buildings[buildingArrayPosition] >= maxNum)
        {
            //Debug.Log("Can't Build");
            GameObject.Destroy(gameObject);
            return;
        }

        //Debug.Log("Built");

        // Update values
        manager.wood -= woodCost;
        manager.stone -= stoneCost;
        manager.buildings[buildingArrayPosition]++;
    }

    // Destroy building
    public void DestroyBuilding()
    {
        //Debug.Log("Destroyed");
        // Update Values
        manager.wood += woodCost / 2;
        manager.stone += stoneCost / 2;
        manager.buildings[buildingArrayPosition]--;

        // Destroy
        GameObject.Destroy(gameObject);
    }

    // When it collides with something, make a note
    public void OnCollisionEnter2D(Collision2D collision) { isColliding = true; }
    public void OnCollisionStay2D(Collision2D collision) { isColliding = true; }

    // When it stops colliding with something, make a note
    public void OnCollisionExit2D(Collision2D collision) { isColliding = false; }

    // Log current collision status
    // Go to mouse if currently building
    private void Update()
    {
        //if (isColliding) { Debug.Log("Colliding"); }
        //else { Debug.Log("Not Colliding"); }

        // Go to mouse pointer
        if (isBuilding)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            transform.position = mousePosition;
        }

        //Test();
    }

    // Test Script
    /*
    private void Test()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            testCounter++;
            //Debug.Log("Key Pressed");

            if (testCounter == 1)
            {
                //Debug.Log("Start Build");
                StartBuild();
            }
            else if (testCounter == 2)
            {
                //Debug.Log("End Build");
                EndBuild();
            }
            else
            {
                //Debug.Log("Destroy");
                Destroy();
            }
        }
    }
    */

}
