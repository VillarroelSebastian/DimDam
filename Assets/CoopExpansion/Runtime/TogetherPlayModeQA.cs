using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace CoopBeam {
public class TogetherPlayModeQA:MonoBehaviour {
 readonly List<string> results=new List<string>();GameManager g;CoopInteractionDirector d;LinkPowers p;
 const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
 void Set(object o,string field,object value){o.GetType().GetField(field,flags).SetValue(o,value);}
 object Get(object o,string field)=>o.GetType().GetField(field,flags).GetValue(o);
 void Check(bool yes,string text){results.Add((yes?"PASS ":"FAIL ")+text);}
 void Resume(){Set(d,"lesson",null);((Queue<string>)Get(d,"lessons")).Clear();Time.timeScale=1;}
 IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end){Resume();foreach(var h in g.players)if(!h.Dead)h.Protect(100);yield return null;}}
 IEnumerator Start(){g=GameManager.Instance;yield return new WaitForSeconds(1.5f);g.Begin();d=g.GetComponent<CoopInteractionDirector>();p=g.GetComponent<LinkPowers>();yield return null;Check(d&&Time.timeScale==0,"Contextual lesson pauses the game");Resume();
 var controllers=new[]{g.players[0].GetComponent<PlayerController2D>(),g.players[1].GetComponent<PlayerController2D>()};foreach(var c in controllers){c.enabled=false;c.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;}
 g.players[0].transform.position=new Vector3(-5,-3.5f);g.players[1].transform.position=new Vector3(5,-3.5f);yield return Wait(1.2f);Check((int)Get(d,"pads")==2,"Two different players activate the two seals");
 g.players[0].transform.position=new Vector3(-5,-2.9f);yield return Wait(.1f);Check((int)Get(d,"gateCarrier")==0,"Key can be collected");Check(g.enemies.Count==2,"Key acquisition triggers two-enemy defense");Check(!CoopInteractionDirector.Locked(g.players[0]),"Key carrier can travel before channeling");Check(!d.TryChannel(0,true,1),"Cannot open away from center");g.players[0].transform.position=new Vector3(0,-3.4f);Check(d.TryChannel(0,true,2),"Carrier channels at center");float progress=(float)Get(d,"gateTime");Check(!d.TryChannel(0,false,1)&&(float)Get(d,"gateTime")==progress,"Releasing preserves opening progress");d.TryChannel(0,true,3.1f);yield return Wait(.2f);Check(!(bool)Get(d,"opening"),"Opening releases campaign waves");
 g.players[0].transform.position=new Vector3(-4,-3);g.players[1].transform.position=new Vector3(4,-3);d.Receive(LinkPickup.Kind.Prism);yield return Wait(.1f);Check(d.HasPending&&!p.Empowered,"Pickup waits for sharing");g.players[1].transform.position=new Vector3(-3.2f,-3);yield return Wait(.8f);Check((bool)Get(d,"shared")&&!p.Empowered,"Proximity shares without starting timer");g.players[1].transform.position=new Vector3(4,-3);yield return Wait(.1f);Check(!d.HasPending&&p.Empowered,"Separating activates shared bonus");
 var h0=g.players[0];Set(h0,"immuneUntil",0f);for(int i=0;i<3;i++){Set(h0,"immuneUntil",0f);h0.Hit(Vector2.right);}Check(h0.Dead&&g.Running,"One death keeps match alive");yield return Wait(3.2f);Check(h0.Dead,"No automatic revival after old delay");g.players[1].transform.position=h0.transform.position;yield return Wait(1.7f);Check(!h0.Dead,"Partner proximity completes rescue");
 g.players[0].transform.position=new Vector3(-4,-3);g.players[1].transform.position=new Vector3(4,-3);int used=p.UltimatesUsed;Set(p,"<Charge>k__BackingField",LinkPowers.ChargeRequired);Check(!p.Confirm(0),"One player cannot activate ultimate alone");Check(p.Confirm(1),"Both confirmations activate dance");int who=(int)Get(d,"promptPlayer"),keyCode=(int)Get(d,"promptKey");Set(d,"beatTime",.3f);Check(!d.SubmitDanceInput(who,keyCode,2),"Multiple keys fail the pulse");Set(d,"answered",false);Set(d,"beatTime",.05f);Check(!d.SubmitDanceInput(who,keyCode,1),"Early input fails the pulse");Set(d,"answered",false);Set(d,"beatTime",.3f);Check(d.SubmitDanceInput(who,keyCode,1),"Correct timed single key increases shared power");Check(!d.SubmitDanceInput(who,keyCode,1),"Repeated key cannot score twice");Check(CoopInteractionDirector.Locked(h0),"Dance locks normal movement");yield return Wait(9f);Check(!d.Dancing&&p.UltimatesUsed==used+1,"Dance completes after scored and missed pulses");Check(!CoopInteractionDirector.Locked(h0),"Dance restores normal controls");
 foreach(var h in g.players){for(int i=0;i<3;i++){Set(h,"immuneUntil",0f);h.Hit(Vector2.right);}}Check(!g.Running&&g.players[0].Dead&&g.players[1].Dead,"Both deaths end the match");
 System.IO.File.WriteAllLines(Application.dataPath+"/CoopExpansion/Documentation/TogetherPlayMode.txt",results);Debug.Log(string.Join("\n",results));foreach(var c in controllers){c.enabled=true;c.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeRotation;}Time.timeScale=1;
 }
}
}
