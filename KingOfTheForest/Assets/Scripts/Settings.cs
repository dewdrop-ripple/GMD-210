using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    // Used to set the difficulty
    // Lower = Easier
    public float difficultyScaler = 1.25f;

    // Set Cursor Texture
    public Texture2D cursorTextureHand;
    public Texture2D cursorTextureAxe;
    public Texture2D cursorTexturePick;
    public Texture2D cursorTextureHammer;

    // Switch cursor
    public bool cAxe = false;
    public bool cPick = false;
    public bool cHammer = false;

    // Load delay
    float timer = 0f;

    // Persistent
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("LoadScene"))
        {
            timer += Time.deltaTime; // Increment timer
            if (timer >= 0.5)
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

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
        Vector2 cursorTarget = new Vector2(10f, 10f);

        switch(texture)
        {
            case 1:
                Cursor.SetCursor(cursorTextureAxe, cursorTarget, CursorMode.ForceSoftware);
                break;

            case 2:
                Cursor.SetCursor(cursorTexturePick, cursorTarget, CursorMode.ForceSoftware);
                break;

            case 3:
                Cursor.SetCursor(cursorTextureHammer, cursorTarget, CursorMode.ForceSoftware);
                break;

            case 0:
            default:
                Cursor.SetCursor(cursorTextureHand, cursorTarget, CursorMode.ForceSoftware);
                break;
        }
    }
}
