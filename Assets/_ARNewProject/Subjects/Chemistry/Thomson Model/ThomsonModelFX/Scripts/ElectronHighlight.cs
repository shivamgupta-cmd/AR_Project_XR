using UnityEngine;
using System.Collections;
public class ElectronHighlight : MonoBehaviour
{
    public Renderer targetRenderer;
    [ColorUsage(true,true)] public Color highlightColor = new Color(0.1f,1.2f,3f,1f);
    public float intensity = 4f;
    public float duration = .6f;
    Material mat; static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    void Awake(){ if(targetRenderer==null) targetRenderer=GetComponentInChildren<Renderer>(); if(targetRenderer!=null){mat=targetRenderer.material; mat.EnableKeyword("_EMISSION");} }
    public void Highlight(){ if(gameObject.activeInHierarchy) StartCoroutine(Run()); }
    IEnumerator Run(){ if(mat==null) yield break; float t=0; while(t<duration){t+=Time.deltaTime; float k=Mathf.Sin((t/duration)*Mathf.PI); mat.SetColor(EmissionColor,highlightColor*intensity*k); yield return null;} mat.SetColor(EmissionColor,Color.black); }
}
