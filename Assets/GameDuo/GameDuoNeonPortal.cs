using UnityEngine;
namespace CoopBeam {
// World-space vertical vortex, generated from lines so it stays crisp at any resolution.
public sealed class GameDuoNeonPortal : MonoBehaviour {
 NeonLevel level; LineRenderer[] rings=new LineRenderer[9],bolts=new LineRenderer[5]; Material material;
 public static void Install(NeonLevel neon){if(neon&&neon.exit&&!neon.exit.GetComponent<GameDuoNeonPortal>()){var p=neon.exit.gameObject.AddComponent<GameDuoNeonPortal>();p.level=neon;}}
 LineRenderer Line(string label,int count,float width){var g=new GameObject(label);g.transform.SetParent(transform,false);var l=g.AddComponent<LineRenderer>();l.useWorldSpace=false;l.positionCount=count;l.widthMultiplier=width;l.sharedMaterial=material;l.sortingOrder=90;return l;}
 void Start(){material=new Material(Shader.Find("Sprites/Default"));var old=GetComponent<SpriteRenderer>();if(old)old.enabled=false;
  for(int i=0;i<rings.Length;i++)rings[i]=Line("Anillo de energia",97,.07f);
  for(int i=0;i<bolts.Length;i++)bolts[i]=Line("Rayo",18,.035f);
 }
 void Update(){if(!level)return;float t=Time.time;float intensity=level.Collected?1:.25f;
  for(int r=0;r<rings.Length;r++){float radius=.12f+r*.105f;Color c=r<2?Color.white:Color.HSVToRGB(Mathf.Repeat(r*.115f+t*.08f,1),.5f,1);c*=intensity;c.a=1;rings[r].startColor=rings[r].endColor=c;
   for(int j=0;j<97;j++){float a=j/96f*Mathf.PI*2;float wave=1+.08f*Mathf.Sin(a*3+t*3+r);rings[r].SetPosition(j,new Vector3(Mathf.Cos(a)*radius*wave,Mathf.Sin(a)*radius*1.8f*wave,0));}}
  for(int b=0;b<bolts.Length;b++){Color c=Color.Lerp(Color.cyan,Color.white,.65f)*intensity;c.a=1;bolts[b].startColor=bolts[b].endColor=c;float angle=b*Mathf.PI*2/bolts.Length+t*.3f;
   for(int j=0;j<18;j++){float f=j/17f;float radius=.25f+f*1.05f;float a=angle+Mathf.Sin(j*2.3f+Mathf.Floor(t*12))*.16f*f;bolts[b].SetPosition(j,new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*1.8f,0));}}
 }
 void OnDestroy(){if(material)Destroy(material);}
}
}
