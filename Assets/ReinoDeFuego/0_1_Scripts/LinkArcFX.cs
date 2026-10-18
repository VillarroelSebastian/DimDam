using UnityEngine;
namespace CoopBeam {
public class LinkArcFX:MonoBehaviour {
 LineRenderer line;Vector2 a,b;Color color;float age;bool pulse;int side;
 public static void Spawn(Vector2 a,Vector2 b,Color color){Create(a,b,color,false,1);}
 public static void Pulse(Vector2 a,Vector2 b){Create(a,b,new Color(1,.65f,.1f),true,1);Create(a,b,new Color(1,.9f,.4f),true,-1);FireFX.Burst((a+b)*.5f,new Color(1,.7f,.2f),10);}
 static void Create(Vector2 a,Vector2 b,Color color,bool pulse,int side){var g=new GameObject(pulse?"Solar link pulse":"Chained spark");var r=g.AddComponent<LineRenderer>();var beam=FindAnyObjectByType<CoopBeam>();if(beam&&beam.core)r.sharedMaterial=beam.core.sharedMaterial;r.positionCount=19;r.widthMultiplier=pulse?.1f:.065f;r.sortingOrder=13;var f=g.AddComponent<LinkArcFX>();f.line=r;f.a=a;f.b=b;f.color=color;f.pulse=pulse;f.side=side;}
 void Update(){age+=Time.deltaTime;if(age>.42f){Destroy(gameObject);return;}Vector2 normal=Vector2.Perpendicular((b-a).normalized);for(int i=0;i<19;i++){float t=i/18f;float displacement=pulse?Mathf.Sin(t*Mathf.PI)*age/ .42f*.95f*side:Mathf.Sin(i*2.8f+Mathf.Floor(age*24))*Mathf.Sin(t*Mathf.PI)*.13f;Vector2 p=Vector2.Lerp(a,b,t)+normal*displacement;line.SetPosition(i,new Vector3(p.x,p.y,-.3f));}var c=color;c.a=1-age/.42f;line.startColor=line.endColor=c;}
}
}
