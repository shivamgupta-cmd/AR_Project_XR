using UnityEngine;

public class KnowledgeSymbolObject : MonoBehaviour
{
    [Header("Object Voice Over")]
    public AudioClip vo;

    [Header("Object Highlighters")]
    public KnowledgeSymbolObjectHighlighter[] highlighters;

    [HideInInspector]
    public KnowledgeSymbolManagerScript manager;

    [HideInInspector]
    public bool alreadyClicked = false;

    public void OnObjectClicked()
    {
        if (alreadyClicked)
            return;

        if (manager != null)
        {
            manager.ObjectClicked(this);
        }
    }

    public void StartHighlight()
    {
        if (highlighters == null)
            return;

        foreach (KnowledgeSymbolObjectHighlighter highlighter in highlighters)
        {
            if (highlighter != null)
            {
                highlighter.StartHighlight();
            }
        }
    }
}