using UnityEngine;

public class FadeFromBlack : MonoBehaviour
{
    float timer = 0f; // Used to slowly fade
    public UnityEngine.UI.Image overlayColor; // Set color

    private void Update()
    {
        timer += Time.deltaTime; // Update time

        float opacity = Mathf.Pow(0.5f, timer - 0.5f); // Slowly fade out
        if (opacity < 0.15) GameObject.Destroy(gameObject); // Delete
        overlayColor.color = new Color(0, 0, 0, opacity);
    }
}
