using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CoopBeam {
// Opt-in integration suite, never attached to delivered content.
public class CandyWorldQA:MonoBehaviour {
 readonly List<string> results=new List<string>();GameManager game;CoopBeam beam;string folder="Assets/ReinoDeChicle/0_6_QA/Results/";
 void Check(string text,bool ok){results.Add((ok?"PASS ":"FAIL ")+text);Debug.Log(results[results.Count-1]);Save();}
 void Save(){Directory.CreateDirectory(folder);File.WriteAllLines(folder+"WorldIntegration.txt",results);}
 void FreezeEnemies(){foreach(var e in game.enemies){e.enabled=false;e.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;var special=e.GetComponent<CandyEnemySpecial>();if(special)special.enabled=false;}}
 void Place(PlayerHealth h,Vector2 pos){var rb=h.GetComponent<Rigidbody2D>();rb.position=pos;rb.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
 IEnumerator Start(){DontDestroyOnLoad(gameObject);Application.runInBackground=true;CandyCampaign.Unlocked=1;Directory.CreateDirectory(folder);
 for(int level=1;level<=5;level++){
  if(SceneManager.GetActiveScene().path!=CandyCampaign.ScenePath(level)){CandyCampaign.Load(level);yield return new WaitForSeconds(.5f);}
  game=GameManager.Instance;beam=FindAnyObjectByType<CoopBeam>();Check("Level "+level+" loads with two players and beam",game&&game.players.Length==2&&beam&&game.level.number==level);
  Check("Level "+level+" begins locked to its own progression",CandyCampaign.Unlocked==level);
  game.Begin();yield return new WaitForSeconds(2);beam.enabled=false;FreezeEnemies();
  ScreenCapture.CaptureScreenshot(folder+"Level_0"+level+".png");yield return new WaitForSeconds(.25f);
  var red=game.players[0];var rb=red.GetComponent<Rigidbody2D>();red.GetComponent<PlayerController2D>().enabled=false;game.players[1].GetComponent<PlayerController2D>().enabled=false;
  if(level==2){var spring=System.Array.Find(FindObjectsByType<CandyEnvironment>(),e=>e.kind==CandySurfaceKind.Bounce);Place(red,(Vector2)spring.transform.position+Vector2.up*1.2f);rb.linearVelocity=Vector2.down*3;float maxY=0;float until=Time.time+.65f;while(Time.time<until){maxY=Mathf.Max(maxY,rb.linearVelocity.y);yield return new WaitForFixedUpdate();}Check("Elastic landing provides predictable upward impulse",maxY>14);}
  if(level==3){var belt=System.Array.Find(FindObjectsByType<CandyEnvironment>(),e=>e.kind==CandySurfaceKind.Conveyor&&e.strength>0);Place(red,(Vector2)belt.transform.position+Vector2.up*.79f);yield return new WaitForSeconds(.15f);float x=rb.position.x;yield return new WaitForSeconds(.6f);Check("Conveyor carries stationary player",rb.position.x>x+.5f);
   var moving=System.Array.Find(FindObjectsByType<CandyEnvironment>(),e=>e.kind==CandySurfaceKind.Moving);Place(red,(Vector2)moving.transform.position+Vector2.up*.78f);yield return new WaitForSeconds(.2f);x=rb.position.x;float px=moving.transform.position.x;float largestMove=0,largestError=0;float carryUntil=Time.time+1.3f;while(Time.time<carryUntil){largestMove=Mathf.Max(largestMove,Mathf.Abs(rb.position.x-x));largestError=Mathf.Max(largestError,Mathf.Abs((rb.position.x-x)-(moving.transform.position.x-px)));yield return new WaitForFixedUpdate();}Check("Moving platform carries its rider (distance="+largestMove.ToString("F2")+", error="+largestError.ToString("F2")+")",largestMove>.1f&&largestError<.5f);
   var hazard=FindAnyObjectByType<CandyHazard>();while(hazard.Active)yield return null;while(red.Invulnerable)yield return null;Place(red,hazard.transform.position);int hearts=red.Hearts;float deadline=Time.time+6;while(Time.time<deadline&&red.Hearts==hearts)yield return null;Check("Telegraphed press damages only during its danger window",red.Hearts<hearts);
  }
  if(level==4){var crumble=System.Array.Find(FindObjectsByType<CandyEnvironment>(),e=>e.kind==CandySurfaceKind.Crumbly);Place(red,(Vector2)crumble.transform.position+Vector2.up*.81f);yield return new WaitForSeconds(1.2f);Check("Marshmallow disappears after its warning",!crumble.GetComponent<Collider2D>().enabled);yield return new WaitForSeconds(3.1f);Check("Marshmallow rebuilds to avoid permanent dead ends",crumble.GetComponent<Collider2D>().enabled);}
  foreach(var p in game.players){p.GetComponent<Rigidbody2D>().simulated=false;p.transform.position=new Vector3(0,7,0);}
  int seen=0;bool bossTested=false;float timeout=Time.time+45;
  while(game.Running&&Time.time<timeout){if(game.enemies.Count>0&&game.Wave!=seen){seen=game.Wave;Check("Level "+level+" wave "+seen+" roster",game.enemies.Count==game.level.waves[seen-1].enemyTypes.Length);}
   foreach(var enemy in game.enemies.ToArray()){
    if(!enemy)continue;var boss=enemy.GetComponent<CandyBoss>();
    if(boss){if(!bossTested){float health=enemy.Health;Physics2D.SyncTransforms();Vector2 body=enemy.transform.position;beam.DamageSegment(body+new Vector2(-2,-1),body+new Vector2(2,-1),.1f);Check("Boss body is protected from beam damage",enemy.Health==health);
      while(!boss.Vulnerable)yield return null;Vector2 weak=boss.weakSpot.bounds.center;Physics2D.SyncTransforms();beam.DamageSegment(weak+Vector2.left,weak+Vector2.right,.1f);Check("Boss mint core receives beam damage when open",enemy.Health<health);
      enemy.Damage(enemy.maxHealth*.36f);yield return new WaitForFixedUpdate();Check("Boss changes to phase two",boss.Phase==2);
      while(!boss.Vulnerable)yield return null;enemy.Damage(enemy.maxHealth*.34f);yield return new WaitForFixedUpdate();Check("Boss changes to phase three",boss.Phase==3);bossTested=true;}
     if(boss.Vulnerable){Vector2 weak=boss.weakSpot.bounds.center;Physics2D.SyncTransforms();beam.DamageSegment(weak+Vector2.left*2,weak+Vector2.right*2,20);}
    }else {Physics2D.SyncTransforms();Vector2 point=enemy.transform.position;beam.DamageSegment(point+Vector2.left*2,point+Vector2.right*2,20);}
   }
   yield return new WaitForSeconds(.1f);
  }
  Check("Level "+level+" wins after all three waves",game.Victory&&!game.Running&&seen==3);
  Check("Level "+level+" unlocks its successor",CandyCampaign.Unlocked==Mathf.Min(5,level+1));
  ScreenCapture.CaptureScreenshot(folder+"Victory_0"+level+".png");yield return new WaitForSeconds(.3f);
 }
 CandyCampaign.Map();yield return new WaitForSeconds(.5f);Check("World map returns with all five levels unlocked",FindAnyObjectByType<CandyWorldMap>()&&CandyCampaign.Unlocked==5);Save();Debug.Log("CANDY_WORLD_QA_COMPLETE");Destroy(gameObject);
 }
}}

