using UnityEngine;

public class FadeFromBlack : MonoBehaviour
{
    float timer = 0f; // Used to slowly fade
    public UnityEngine.UI.Image overlayColor; // Set color

    private void Update()
    {
        timer += Time.deltaTime; // Update time

        float opacity = Mathf.Pow(0.25f, timer); // Slowly fade out
        if (opacity < 0.25) GameObject.Destroy(gameObject); // Delete
        overlayColor.color = new Color(0, 0, 0, opacity);
    }
}
