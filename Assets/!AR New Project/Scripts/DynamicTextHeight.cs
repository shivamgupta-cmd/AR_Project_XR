using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicTextHeight : MonoBehaviour
{
    [Header("References")]
    public TMP_Text descriptionText;
    public RectTransform content;

    private void Start()
    {
        SetText(descriptionText.text);        
    }

    private void Update()
    {
    }

    public void SetText(string text)
    {
        descriptionText.text = text;

        UpdateHeight();
    }

    private void UpdateHeight()
    {
        Canvas.ForceUpdateCanvases();

        descriptionText.ForceMeshUpdate();

        float finalHeight = descriptionText.preferredHeight;

        content.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, finalHeight);

        Debug.Log("Lines: " + descriptionText.textInfo.lineCount + " | Height: " + finalHeight);
    }
}