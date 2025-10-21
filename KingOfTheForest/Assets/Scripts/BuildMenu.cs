using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    private const int UP_POSITION = -220;
    private const int DOWN_POSITION = -500;
    public bool isUp = false;
    public UnityEngine.UI.Image movable;

    private void Update()
    {
        if (isUp)
        {
            movable.transform.localPosition = new Vector3(0f, UP_POSITION, 0f);
        }
        else
        {
            movable.transform.localPosition = new Vector3(0f, DOWN_POSITION, 0f);
        }
    }

}
