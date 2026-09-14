using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    [SerializeField] float m_DelaybeforeLoading = 0f;
    [SerializeField] bool m_LoadAutomatic=false;
    [SerializeField] string m_PredefineScene = "";

    private void Start()
    {
        if(!m_LoadAutomatic)
        {
            return;
        }    
        if(m_PredefineScene.Length<1)
        {
            return ;
        }
        StartCoroutine(ChangeScene(m_PredefineScene));
    }

    public void Scenechange(string Scenename)
    {
        StartCoroutine(ChangeScene(Scenename));
    }
    IEnumerator ChangeScene(string SceneName)
    {
        yield return new WaitForSeconds(m_DelaybeforeLoading);
        SceneManager.LoadScene(SceneName);
        StopCoroutine(ChangeScene(SceneName));
    }
    
}
