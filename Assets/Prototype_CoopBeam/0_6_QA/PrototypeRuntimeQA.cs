using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace CoopBeam {
// Opt-in diagnostic. Never attached to the delivered scene or prefabs.
public class PrototypeRuntimeQA:MonoBehaviour {
 readonly List<string> results=new List<string>();Keyboard keyboard;CoopBeam beam;GameManager game;
 void Check(string name,bool ok){results.Add((ok?"PASS ":"FAIL ")+name);Debug.Log(results[results.Count-1]);}
 IEnumerator Keys(float seconds,params Key[] keys){float until=Time.time+seconds;while(Time.time<until){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));yield return null;}InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;}
 IEnumerator Start(){
  game=GameManager.Instance;beam=FindAnyObjectByType<CoopBeam>();keyboard=InputSystem.AddDevice<Keyboard>();game.Begin();yield return new WaitForSeconds(2);
  Check("Wave 1 has three walkers",game.Wave==1&&game.enemies.Count==3);
  foreach(var e in game.enemies)e.gameObject.SetActive(false);
  var r=game.players[0];var b=game.players[1];yield return new WaitForSeconds(.4f);
  float rx=r.transform.position.x,bx=b.transform.position.x;
  yield return Keys(.3f,Key.D,Key.LeftArrow);
  Check("Independent simultaneous opposite movement",r.transform.position.x>rx+.5f&&b.transform.position.x<bx-.5f);
  yield return Keys(.3f);rx=r.transform.position.x;bx=b.transform.position.x;yield return Keys(.2f,Key.A);
  Check("Coral key does not move Celeste",r.transform.position.x<rx&&Mathf.Abs(b.transform.position.x-bx)<.4f);
  yield return Keys(.2f);float ry=r.transform.position.y,by=b.transform.position.y;
  yield return Keys(.16f,Key.W,Key.UpArrow);
  Check("Both players jump simultaneously",r.transform.position.y>ry+.5f&&b.transform.position.y>by+.5f);
  yield return Keys(.08f,Key.LeftShift,Key.RightCtrl);
  Check("Both horizontal dashes activate",r.GetComponent<PlayerController2D>().Dashing&&b.GetComponent<PlayerController2D>().Dashing);
  yield return Keys(.6f);
  Check("Beam anchors follow players",Vector2.Distance(beam.core.GetPosition(0),r.transform.position)<.1f&&Vector2.Distance(beam.core.GetPosition(1),b.transform.position)<.1f);
  r.transform.position=new Vector2(-5.2f,-3.75f);r.GetComponent<PlayerController2D>().ResetMotion();yield return Keys(.3f);
  yield return Keys(.45f,Key.W);yield return Keys(.65f);
  Check("Lower island reachable from ground",r.transform.position.y> -1.5f&&r.GetComponent<PlayerController2D>().Grounded);
  yield return Keys(.65f,Key.W,Key.D);yield return Keys(.5f);
  Check("Center island reachable from lower island",r.transform.position.y>.6f&&r.GetComponent<PlayerController2D>().Grounded);
  yield return Keys(.65f,Key.W,Key.A);yield return Keys(.5f);
  Check("Upper island reachable from center",r.transform.position.y>3&&r.GetComponent<PlayerController2D>().Grounded);
  float dropY=r.transform.position.y;yield return Keys(.1f,Key.S);yield return Keys(.4f);
  Check("Down input drops through semisolid island",r.transform.position.y<dropY-1);
  beam.enabled=false;
  var tests=new List<EnemyBase>();for(int i=0;i<2;i++){var e=Instantiate(game.enemyPrefabs[3],new Vector2(i==0?-1:1,4),Quaternion.identity);e.enabled=false;e.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;tests.Add(e);}
  yield return null;Physics2D.SyncTransforms();float h0=tests[0].Health,h1=tests[1].Health;
  int n=beam.DamageSegment(new Vector2(-3,4),new Vector2(3,4),.1f);
  Check("Horizontal beam damages multiple enemies at DPS times deltaTime",n==2&&Mathf.Abs(tests[0].Health-(h0-3.8f))<.02f&&tests[1].Health<h1);
  h0=tests[0].Health;n=beam.DamageSegment(new Vector2(-3,6),new Vector2(3,6),.1f);
  Check("No damage after losing contact",n==0&&tests[0].Health==h0);
  tests[0].transform.position=new Vector2(0,4);tests[1].transform.position=new Vector2(8,4);Physics2D.SyncTransforms();
  Check("Vertical intersection",beam.DamageSegment(new Vector2(0,3),new Vector2(0,5),.1f)==1);
  Check("Diagonal intersection",beam.DamageSegment(new Vector2(-1,3),new Vector2(1,5),.1f)==1);
  Check("Reversed endpoints",beam.DamageSegment(new Vector2(1,5),new Vector2(-1,3),.1f)==1);
  h0=tests[0].Health;Check("Coincident anchors cause no damage",beam.DamageSegment(new Vector2(0,4),new Vector2(0,4),.5f)==0&&tests[0].Health==h0);
  foreach(var e in tests)Destroy(e.gameObject);yield return null;
  // Clear actual waves using the same beam query, with a long simulated damage interval.
  int seen=1;float deadline=Time.time+22;
  while(game.Running&&Time.time<deadline){if(game.Wave>seen&&game.enemies.Count>0){seen=game.Wave;Check("Wave "+seen+" roster",game.enemies.Count==(seen==2?4:5));}
   foreach(var e in game.enemies.ToArray()){if(!e)continue;e.gameObject.SetActive(true);Physics2D.SyncTransforms();Vector2 p=e.transform.position;beam.DamageSegment(p+Vector2.left*2,p+Vector2.right*2,10);}
   yield return new WaitForSeconds(.1f);
  }
  Check("Three waves advance and victory stops spawning",seen==3&&game.Victory&&!game.Running&&game.enemies.Count==0);
  Check("Shared score and combo reward kills",game.Score>1200);
  InputSystem.RemoveDevice(keyboard);keyboard=null;Save("CombatAndVictory.txt");
 }
 void Save(string file){Directory.CreateDirectory("Assets/Prototype_CoopBeam/0_6_QA/Results");File.WriteAllLines("Assets/Prototype_CoopBeam/0_6_QA/Results/"+file,results);Debug.Log("COOP_QA_COMPLETE "+file);}
 void OnDestroy(){if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);}
}
}

