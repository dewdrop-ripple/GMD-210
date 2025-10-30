using UnityEngine;

public class FadeFromBlack : MonoBehaviour
{
    float timer = 0f;
    public UnityEngine.UI.Image overlayColor;

    private void Update()
    {
        timer += Time.deltaTime;

        float opacity = Mathf.Pow(0.5f, timer - 1);
        if (opacity < 0.15) GameObject.Destroy(gameObject);
        overlayColor.color = new Color(0, 0, 0, opacity);
    }
}
