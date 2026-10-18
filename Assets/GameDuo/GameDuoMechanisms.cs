using UnityEngine;
namespace CoopBeam {
public sealed class GameDuoMechanisms:MonoBehaviour {
 BubblegumLevel level;LineRenderer[] gears;Material material;
 void Start(){
  level=GetComponent<BubblegumLevel>();if(!level)return;
  material=new Material(Shader.Find("Sprites/Default"));var pivots=new[]{level.leverA,level.leverB,level.bubbleSwitch,level.gateA,level.gateB};gears=new LineRenderer[pivots.Length];
  for(int i=0;i<pivots.Length;i++){
   var go=new GameObject("Engranaje de menta / "+i);go.transform.SetParent(pivots[i],false);go.transform.position=pivots[i].position+new Vector3(i<3?.55f:0,i<3?.65f:1.2f);
   // Counter the old scenery scale: gear teeth have a stable world size.
   var parentScale=pivots[i].lossyScale;go.transform.localScale=new Vector3(1/parentScale.x,1/parentScale.y,1);
   var line=go.AddComponent<LineRenderer>();gears[i]=line;line.sharedMaterial=material;line.useWorldSpace=false;line.loop=true;line.positionCount=48;line.startWidth=line.endWidth=.065f;line.sortingOrder=4;
   for(int n=0;n<48;n++){float a=n*Mathf.PI*2/48;float r=n%4<2?.46f:.35f;line.SetPosition(n,new Vector3(Mathf.Cos(a),Mathf.Sin(a))*r);}
   line.startColor=line.endColor=new Color(.6f,.95f,.8f);
  }
 }
 void Update(){if(!level||gears==null||Time.deltaTime<=0)return;for(int i=0;i<gears.Length;i++){bool on=i==0?level.LeverOn:i==1?level.RouteUp:i==2?level.BubblesOn:i==3?level.DoorAOpen:level.DoorBOpen;if(on)gears[i].transform.Rotate(0,0,(i%2==0?1:-1)*65*Time.deltaTime);gears[i].startColor=gears[i].endColor=on?CoopPromptUI.Mint:new Color(.6f,.55f,.7f);}}
 void OnGUI(){if(!level||!level.game.Running)return;var matrix=GUI.matrix;GUI.depth=-5;PixelFont.Begin();
  for(int i=0;i<2;i++){
   Transform nearest=null;float distance=1.4f;
   foreach(var target in new[]{level.auxiliary,level.leverA,level.leverB,level.bubbleSwitch}){
    if((target==level.auxiliary||target==level.bubbleSwitch)&&i==0)continue;
    float d=Vector2.Distance(level.game.players[i].transform.position,target.position);
    if(d<distance&&!Physics2D.Linecast(level.game.players[i].transform.position,target.position,1<<8).collider){distance=d;nearest=target;}
   }
   if(nearest&&Camera.main){var at=Camera.main.WorldToViewportPoint(nearest.position+Vector3.up*1.2f);if(at.z>0)CoopPromptUI.Key(new Rect(at.x*640-16,(1-at.y)*360-16,32,29),"S",CoopPresentation.PlayerColor(i),true,i==1?2:-1);}
  }
  GUI.matrix=matrix;
 }
 void OnDestroy(){if(material)Destroy(material);}
}
}
