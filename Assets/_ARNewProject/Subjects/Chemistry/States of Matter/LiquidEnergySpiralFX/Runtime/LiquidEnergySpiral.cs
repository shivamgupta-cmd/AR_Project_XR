using UnityEngine;
[ExecuteAlways]
public class LiquidEnergySpiral : MonoBehaviour {
 public LineRenderer core, glow;
 [Range(16,160)] public int points=90;
 public float radius=.72f,height=1.55f,turns=2.25f,rotationSpeed=38f,wave=.045f,waveSpeed=2.2f;
 void Update(){
  if(!core||!glow)return;
  core.positionCount=glow.positionCount=points;
  float ph=Time.time*rotationSpeed*Mathf.Deg2Rad;
  for(int i=0;i<points;i++){
   float t=i/(float)(points-1), a=t*turns*Mathf.PI*2+ph;
   float rr=radius+Mathf.Sin(t*Mathf.PI*6+Time.time*waveSpeed)*wave;
   Vector3 v=new Vector3(Mathf.Cos(a)*rr,(t-.5f)*height,Mathf.Sin(a)*rr);
   core.SetPosition(i,v); glow.SetPosition(i,v);
  }
 }
}