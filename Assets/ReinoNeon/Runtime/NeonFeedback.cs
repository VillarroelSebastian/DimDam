using UnityEngine;
namespace CoopBeam {
// Pure presentation; progress and recovery are owned by NeonLevel.
public class NeonFeedback:MonoBehaviour {
 public NeonLevel level;public NeonDevice relayCable;public SpriteRenderer terminal,socketA,socketB,coreA,coreB,coreC,padA,padB,portal;
 public Sprite panelOff,panelOn;
 void Update(){if(!level)return;
 if(relayCable.powered!=level.SocketA)relayCable.Set(level.SocketA);
 terminal.sprite=level.Routed?panelOn:panelOff;socketA.color=level.SocketA?Color.white:new Color(.4f,.4f,.55f);socketB.color=level.RelayDone?Color.white:new Color(.4f,.4f,.55f);
 coreA.sprite=level.CoreStep>=1?panelOn:panelOff;coreB.sprite=level.CoreStep>=2?panelOn:panelOff;coreC.sprite=level.CoreStep>=3?panelOn:panelOff;
 padA.color=level.Near(0,level.padA,1)?Color.white:new Color(.4f,.4f,.55f);padB.color=level.Near(1,level.padB,1)?Color.white:new Color(.4f,.4f,.55f);
 portal.color=level.Collected?Color.Lerp(Color.white,Color.cyan,.15f+.15f*Mathf.Sin(Time.time*3)):new Color(.3f,.3f,.45f);
 }
}
}
