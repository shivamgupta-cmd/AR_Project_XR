using UnityEngine;
public class SimpleAutoRotate : MonoBehaviour
{
    public Vector3 axis=Vector3.up;
    public float speed=20f;
    public bool rotate=true;
    void Update(){ if(rotate) transform.Rotate(axis.normalized,speed*Time.deltaTime,Space.Self); }
    public void StartRotate(){rotate=true;} public void StopRotate(){rotate=false;}
}