using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBox : MonoBehaviour
{
    private List<string> text = new List<string>();
    private List<float> deleteTimes = new List<float>();
    public TextMeshProUGUI textBox;

    private float timer = 0;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        string displayText = "";

        if (text.Count > 0)
        {
            for (int i = 0; i < text.Count; i++)
            {
                if (deleteTimes[i] < timer)
                {
                    text.RemoveAt(i);
                    deleteTimes.RemoveAt(i);
                }
            }

            for (int i = 0; i < text.Count; i++)
            {
                displayText += text[i];
                displayText += "\n";
            }
        }

        textBox.text = displayText;
    }

    public void addText(string newText)
    {
        text.Add(newText);
        deleteTimes.Add(timer + 3.0f);
    }
}
