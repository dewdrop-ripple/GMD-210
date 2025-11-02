using Unity.VisualScripting;
using UnityEngine;

public class Settings : MonoBehaviour
{
    // Used to set the difficulty
    // Lower = Easier
    public float difficultyScaler = 1.0f;

    // Set Cursor Texture
    public Texture2D cursorTextureHand;
    public Texture2D cursorTextureAxe;
    public Texture2D cursorTexturePick;
    public Texture2D cursorTextureHammer;

    // Switch cursor
    public bool cAxe = false;
    public bool cPick = false;
    public bool cHammer = false;

    // Persistent
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (cHammer)
        {
            setCursorTexture(3);
        }
        else if (cPick)
        {
            setCursorTexture(2);
        }
        else if (cAxe)
        {
            setCursorTexture(1);
        }
        else
        {
            setCursorTexture(0);
        }
    }

    // 0 - Hand
    // 1 - Axe
    // 2 - Pick
    // 3 - Hammer
    public void setCursorTexture(int texture)
    {
        switch(texture)
        {
            case 1:
                Cursor.SetCursor(cursorTextureAxe, Vector2.zero, CursorMode.ForceSoftware);
                break;

            case 2:
                Cursor.SetCursor(cursorTexturePick, Vector2.zero, CursorMode.ForceSoftware);
                break;

            case 3:
                Cursor.SetCursor(cursorTextureHammer, Vector2.zero, CursorMode.ForceSoftware);
                break;

            case 0:
            default:
                Cursor.SetCursor(cursorTextureHand, Vector2.zero, CursorMode.ForceSoftware);
                break;
        }
    }
}
