using UnityEngine;
namespace CoopBeam {
// Telegraph geometry follows the real hazard footprint, and trails never deal damage.
public sealed class GameDuoIceAttackVisual:MonoBehaviour {
 EncounterHazard hazard;LineRenderer outline,trail;SpriteRenderer art;
 Vector3[] history=new Vector3[9];float sample;Material material;
 SpriteRenderer crystal;float spin;bool refreshed;
 void Start(){
  hazard=GetComponent<EncounterHazard>();art=GetComponent<SpriteRenderer>();
  refreshed=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="03_ReyHielo";
  if(refreshed){var go=new GameObject("Cristal / giro visual independiente");go.transform.SetParent(transform,false);crystal=go.AddComponent<SpriteRenderer>();crystal.sprite=IllustratedWorldLibrary.Sprite(1,27)??ExpansionSprites.Hazard(true,false);crystal.sortingOrder=10;art.enabled=false;}
  material=new Material(Shader.Find("Sprites/Default"));
  outline=Line("Contorno de peligro",5);outline.useWorldSpace=false;outline.loop=true;
  outline.SetPositions(new[]{new Vector3(-.5f,-.5f),new Vector3(-.5f,.5f),new Vector3(.5f,.5f),new Vector3(.5f,-.5f),new Vector3(-.5f,-.5f)});
  trail=Line("Estela de escarcha",history.Length);trail.startWidth=.11f;trail.endWidth=.01f;
  for(int i=0;i<history.Length;i++)history[i]=transform.position;
 }
 LineRenderer Line(string name,int count){var go=new GameObject(name);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.positionCount=count;line.startWidth=line.endWidth=.045f;line.sortingOrder=12;line.numCapVertices=3;return line;}
 void LateUpdate(){
  if(!hazard)return;
  if(crystal){
   art.enabled=false;bool moving=hazard.velocity.sqrMagnitude>.1f;
   spin+=Time.deltaTime*(moving?320:35);crystal.transform.localRotation=Quaternion.Euler(0,0,moving?spin:Mathf.Sin(Time.time*3)*3);
   float grow=hazard.Active?(moving?.7f:1):.35f+.15f*Mathf.Sin(Time.time*8);var bounds=crystal.sprite.bounds.size;
   crystal.transform.localScale=new Vector3(grow/bounds.x,grow/bounds.y,1);crystal.color=hazard.Active?new Color(.65f,.95f,1):new Color(1,.72f,.2f,.6f);
  }
  bool active=hazard.Active;Color c=active?new Color(.55f,.95f,1,.9f):CoopPromptUI.Amber;
  outline.startColor=outline.endColor=c;outline.enabled=!active||hazard.velocity.sqrMagnitude<.1f;
  if(!active){trail.enabled=false;art.color=new Color(1,.7f,.2f,.15f+Mathf.PingPong(Time.time*2,.15f));return;}
  if(hazard.velocity.sqrMagnitude<.1f){
   art.sprite=IllustratedWorldLibrary.Sprite(1,21)??art.sprite;art.color=new Color(.6f,.9f,1);
   if(Time.frameCount%12==0)BeamFX.Burst(transform.position,Color.cyan,2);
  }else{
   trail.enabled=true;sample+=Time.deltaTime;
   if(sample>.035f){sample=0;for(int i=history.Length-1;i>0;i--)history[i]=history[i-1];history[0]=transform.position;}
   trail.SetPositions(history);trail.startColor=new Color(.7f,1,1,.7f);trail.endColor=new Color(.3f,.6f,1,0);
  }
 }
 void OnDestroy(){if(material)Destroy(material);}
}

public sealed class GameDuoIceBossAura:MonoBehaviour {
 IceKingBoss king;LineRenderer ring,aim;Material material;float burstAt;bool previousAttack;
 void Start(){king=GetComponent<IceKingBoss>();material=new Material(Shader.Find("Sprites/Default"));ring=Make("Corona / energia",49);aim=Make("Direccion del cetro",2);ring.loop=true;}
 LineRenderer Make(string name,int count){var go=new GameObject(name);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.positionCount=count;line.startWidth=line.endWidth=.055f;line.sortingOrder=11;return line;}
 void LateUpdate(){
  if(!king)return;var game=GameManager.Instance;bool live=game&&game.Running&&!king.Defeated;
  ring.enabled=live&&(king.Telegraphing||king.Vulnerable);aim.enabled=live&&king.Telegraphing&&king.PatternIndex==0;
  Vector3 center=transform.position+Vector3.up*.8f;
  float radius=king.Vulnerable?.7f:Mathf.Lerp(.45f,1.3f,king.TelegraphProgress);
  Color c=king.Vulnerable?CoopPromptUI.Mint:CoopPromptUI.Amber;
  for(int i=0;i<49;i++){float angle=(i/48f*Mathf.PI*2+Time.time*.5f);ring.SetPosition(i,center+new Vector3(Mathf.Cos(angle),Mathf.Sin(angle))*radius);}
  ring.startColor=ring.endColor=c;
  aim.SetPosition(0,center);aim.SetPosition(1,king.AimPoint);aim.startColor=new Color(1,.8f,.3f,.7f);aim.endColor=new Color(1,.8f,.3f,.15f);
  if(king.Attacking&&!previousAttack){BeamFX.Burst(center,Color.cyan,22);burstAt=Time.time;}
  if(live&&king.Attacking&&Time.time-burstAt>.22f){burstAt=Time.time;BeamFX.Burst(center,Color.cyan,3);}
  previousAttack=king.Attacking;
 }
 void OnDestroy(){if(material)Destroy(material);}
}
}
