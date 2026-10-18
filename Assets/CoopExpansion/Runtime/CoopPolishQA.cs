using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace CoopBeam {
public class CoopPolishQA:MonoBehaviour {
 readonly List<string> report=new List<string>();GameManager g;CoopInteractionDirector d;LinkPowers p;CoopUltimateModes modes;
 const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 void Set(object obj,string field,object value){obj.GetType().GetField(field,Flags).SetValue(obj,value);}
 object Get(object obj,string field)=>obj.GetType().GetField(field,Flags).GetValue(obj);
 void Check(bool passed,string text){report.Add((passed?"PASS ":"FAIL ")+text);}
 void Resume(){if(d){Set(d,"lesson",null);((Queue<string>)Get(d,"lessons")).Clear();}Time.timeScale=1;}
 IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end){Resume();foreach(var h in g.players)if(!h.Dead)h.Protect(100);yield return null;}}
 GameObject Platform(string name,float y,bool unstable){var o=new GameObject(name);o.layer=8;o.transform.position=new Vector3(0,y,0);o.AddComponent<BoxCollider2D>().size=new Vector2(2,.3f);if(unstable){var env=o.AddComponent<CandyEnvironment>();env.kind=CandySurfaceKind.Bounce;env.enabled=false;}return o;}
 void ClearLoot(){foreach(var x in FindObjectsByType<LinkPickup>()){x.gameObject.SetActive(false);Destroy(x.gameObject);}}
 IEnumerator Start(){g=GameManager.Instance;yield return new WaitForSeconds(1.5f);g.Begin();g.StopAllCoroutines();d=g.GetComponent<CoopInteractionDirector>();p=g.GetComponent<LinkPowers>();modes=g.GetComponent<CoopUltimateModes>();Resume();
  foreach(var h in g.players){h.GetComponent<PlayerController2D>().enabled=false;h.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;h.GetComponent<Rigidbody2D>().simulated=false;}
  g.players[0].transform.position=new Vector3(-9,3);g.players[1].transform.position=new Vector3(9,3);
  Check(FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>().Length>=7,"Five environment lights plus two player lights exist");
  var shader=Shader.Find("Coop/PixelEmission");Check(shader&&shader.isSupported,"Pixel emission shader is supported by actual renderer");Check(Resources.Load<Material>("CoopPixelEmission"),"Emission material is referenced through Resources for builds");
  var glow=GameObject.Find("Resplandor visible");Check(glow&&glow.GetComponent<SpriteRenderer>().sortingOrder>0,"Emission renders above background art");
  var stable=Platform("QA stable rescue floor",2.5f,false);var spring=Platform("QA excluded spring",3.5f,true);Physics2D.SyncTransforms();
  Vector2 rest;Collider2D support;Check(DownedBody.TryFindRest(new Vector2(0,4.5f),out rest,out support)&&support==stable.GetComponent<Collider2D>(),"Rest selection skips a spring and chooses stable platform below");
  foreach(var kind in new[]{CandySurfaceKind.Moving,CandySurfaceKind.Conveyor,CandySurfaceKind.Crumbly}){spring.GetComponent<CandyEnvironment>().kind=kind;Check(DownedBody.TryFindRest(new Vector2(0,4.5f),out rest,out support)&&support==stable.GetComponent<Collider2D>(),"Rest selection excludes "+kind);}
  var h0=g.players[0];h0.GetComponent<Rigidbody2D>().simulated=true;h0.transform.position=new Vector3(0,4.5f);for(int i=0;i<3;i++){Set(h0,"immuneUntil",0f);h0.Hit(Vector2.right);}float deathY=h0.transform.position.y;yield return Wait(1.3f);var down=h0.GetComponent<DownedBody>();Check(down&&down.Settled&&h0.transform.position.y<deathY-1.2f,"Airborne dead player falls and settles below unstable platforms");
  float restingY=h0.transform.position.y;yield return Wait(.7f);Check(Mathf.Abs(h0.transform.position.y-restingY)<.02f,"Dead body remains still instead of bouncing");Check(!h0.GetComponent<Rigidbody2D>().simulated,"Downed body cannot be carried or bounced by environment physics");
  g.players[1].transform.position=h0.transform.position;yield return Wait(1.8f);Check(!h0.Dead&&h0.GetComponent<Rigidbody2D>().simulated,"Proximity rescue restores living body physics");h0.GetComponent<Rigidbody2D>().simulated=false;Destroy(stable);Destroy(spring);
  ClearLoot();g.players[0].transform.position=new Vector3(-9,3);g.players[1].transform.position=new Vector3(9,3);Set(p,"<NextLootAt>k__BackingField",0f);Set(p,"eligibleKills",0);
  p.OnKill(new Vector3(0,-3));p.OnKill(new Vector3(0,-3));Check(FindObjectsByType<LinkPickup>().Length==0,"Two eligible kills do not drop a power");p.OnKill(new Vector3(0,-3));Check(FindObjectsByType<LinkPickup>().Length==1,"Third eligible kill drops one power");for(int i=0;i<30;i++)p.OnKill(Vector3.zero);Check(FindObjectsByType<LinkPickup>().Length==1,"Thirty further kills cannot accumulate pickups");Check(!LinkPickup.Create(Vector3.zero,LinkPickup.Kind.Solar),"Factory rejects a second simultaneous pickup");ClearLoot();
  p.ApplyShared(LinkPickup.Kind.Prism);p.ApplyShared(LinkPickup.Kind.Chain);Check(p.Empowered&&!p.ChainActive,"An active prism rejects overlapping chain bonus");for(int i=0;i<30;i++)p.OnKill(Vector3.zero);Check(FindObjectsByType<LinkPickup>().Length==0,"No loot spawns while a bonus is active");yield return Wait(8.2f);Check(!p.ActiveBonus&&!p.CanDrop,"Expired bonus is followed by a quiet period");yield return Wait(7.1f);Check(p.CanDrop,"Loot can return after bonus and quiet period end");
  modes.Select(CoopUltimateModes.Mode.Danza);p.AddCharge(LinkPowers.ChargeRequired);yield return Wait(.1f);Check(!p.Confirm(0)&&p.Confirm(1),"Two players confirm selected ultimate");yield return Wait(.3f);Check(d.Dancing&&CoopInteractionDirector.Locked(h0),"Danza locks both players and enters aura sequence");Check(GameObject.Find("Aura de energia 0").GetComponent<SpriteRenderer>().enabled,"Danza enables visible energy aura");yield return Wait(10);Check(!d.Dancing&&p.UltimatesUsed>=1,"Danza completes without requiring every cue");
  Check(modes.Select(CoopUltimateModes.Mode.Tirachinas),"Tirachinas can be selected");modes.BeginSelected();yield return Wait(1.1f);modes.TickSling(2.2f,true,0,false,false,false,false);Check(modes.Tension>=.99f&&modes.FreezePlayers,"Mora builds sling tension while movement is locked");modes.TickSling(0,false,0,false,false,true,false);Check(modes.Active,"One sling release input cannot fire alone");modes.TickSling(0,false,0,false,false,false,true);Check(!modes.Active&&FindAnyObjectByType<CoopRicochet>(),"Partner confirmation fires actual ricochet wave");Check(!CoopInteractionDirector.Locked(h0),"Sling restores movement after release");yield return Wait(3);
  modes.Select(CoopUltimateModes.Mode.Constelacion);modes.BeginSelected();yield return Wait(.1f);Check(modes.Active&&!CoopInteractionDirector.Locked(h0),"Constellation allows normal player movement");
  g.players[0].transform.position=modes.Target(0);g.players[1].transform.position=new Vector3(9,3);yield return Wait(.5f);Check(modes.CompletedPairs==0,"One star alone does not complete a pair");for(int i=0;i<3;i++){g.players[0].transform.position=modes.Target(0);g.players[1].transform.position=modes.Target(1);yield return Wait(.5f);}Check(!modes.Active&&modes.CompletedPairs==3,"Three jointly occupied star pairs trigger finisher");
  var enemy=Instantiate(g.enemyPrefabs[0],new Vector3(9,3),Quaternion.identity);enemy.kind=EnemyKind.Boss;enemy.GetComponent<Rigidbody2D>().simulated=false;yield return null;float hp=enemy.Health;CoopUltimateModes.ApplyUltimateDamage(enemy,10000);Check(enemy.Health>0&&hp-enemy.Health<=enemy.maxHealth*.241f,"Ultimate damage cap prevents one-shotting a full-health boss");Destroy(enemy.gameObject);
  System.IO.File.WriteAllLines(Application.dataPath+"/CoopExpansion/Documentation/CoopPolishQA.txt",report);Debug.Log(string.Join("\n",report));Resume();
 }
}
}
