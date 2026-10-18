#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace CoopBeam {
public sealed class GameDuoCandyEnemyPlayQA:MonoBehaviour {
 readonly List<string> report=new List<string>();int failures;
 void Check(bool ok,string message){report.Add((ok?"PASS ":"FAIL ")+message);if(!ok)failures++;Debug.Log(report[report.Count-1]);}
 IEnumerator Start(){
  yield return null;yield return null;var game=GameManager.Instance;
  Check(game.level.waves[0].enemyTypes.Length==3&&game.level.waves[1].enemyTypes.Length==3,"Three enemies per introductory wave");
  foreach(var name in new[]{"Wasp","Donut","Bear","Cookie","Turret"}){var frames=GameDuoCandyKing.LoadFrames(name,1.5f,false);Check(frames!=null&&frames.Length==8,name+" atlas resolves");if(frames!=null)foreach(var f in frames)Destroy(f);}
  game.BeginPractice();var director=game.GetComponent<CoopInteractionDirector>();if(director)director.enabled=false;
  FindAnyObjectByType<CoopBeam>().enabled=false;
  foreach(var player in game.players)player.Protect(180);
  int[] kinds={4,1,0,2,5};
  foreach(int kind in kinds){
   var enemy=Instantiate(game.enemyPrefabs[kind],new Vector2(0,kind==1||kind==4?0:-3.4f),Quaternion.identity);yield return null;yield return null;
   var control=enemy.GetComponent<GameDuoCandyEnemy>();Check(control,"Controller installed for kind "+kind);
   Check(enemy.sprite.enabled&&enemy.sprite.sprite&&enemy.sprite.color.a>.9f&&enemy.sprite.bounds.size.y>.5f,"Visible art for kind "+kind);
   Check(enemy.sprite.sprite.name.StartsWith(control.Type.ToString()+"_"),"Correct replacement sprite for kind "+kind);
   Check(enemy.externalControl&&!enemy.GetComponent<CandyEnemyTactics>().enabled,"Legacy tactics disabled for kind "+kind);
   // Place the target beside each melee body to exercise its actual attack range.
   if(kind==0||kind==2){var hero=game.players[0];hero.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;hero.transform.position=enemy.transform.position+Vector3.right*1.8f;}
   bool warning=false;float until=Time.time+8;
   while(Time.time<until&&control.Attacks==0){warning|=control.Telegraphing;yield return null;}
   Check(warning&&control.Attacks>0,"Warning precedes attack for kind "+kind);
   if(kind==1||kind==5)Check(control.Shots>0,"Candy projectiles released for kind "+kind);
   float health=enemy.Health;var beam=FindAnyObjectByType<CoopBeam>();Vector2 center=enemy.GetComponent<Collider2D>().bounds.center;
   beam.DamageSegment(center+Vector2.left*2,center+Vector2.right*2,.1f);Check(enemy.Health<health,"Beam damages kind "+kind);
   enemy.Damage(10000);yield return new WaitForSeconds(1);
  }
  var king=Instantiate(game.enemyPrefabs[6],new Vector2(0,-2.9f),Quaternion.identity);yield return null;yield return null;var motion=king.GetComponent<GameDuoCandyKing>();var boss=king.GetComponent<CandyBoss>();
  while(!boss.Vulnerable)yield return null;king.Damage(king.maxHealth*.7f);yield return new WaitForFixedUpdate();Check(boss.Phase==3,"Final phase retains vulnerable core");
  float end=Time.time+36;int peak=0;bool summonedTurret=false;
  bool captured=false;
  while(Time.time<end){int alive=0;foreach(var guard in FindObjectsByType<GameDuoCandyEnemy>())if(guard.Owner==motion){alive++;summonedTurret|=guard.Type==GameDuoCandyEnemy.Species.Turret;}peak=Mathf.Max(peak,alive);if(!captured&&alive>=2){yield return new WaitForSeconds(.35f);Capture();captured=true;}yield return null;}
  Check(motion.Reinforcements>=2&&summonedTurret,"Boss invokes turret and additional species");Check(peak<=3,"Boss summon cap respected");Check(boss.LastAttackTime>0,"Sugar rain retained");
  while(!boss.Vulnerable)yield return null;king.Damage(10000);yield return null;yield return null;
  Check(FindObjectsByType<GameDuoCandyEnemy>().Length==0,"Boss death clears reinforcements");
  report.Insert(0,failures==0?"PASS / Candy enemy integration":"FAIL / "+failures);File.WriteAllLines("Assets/GameDuo/CandyEnemyQA.txt",report);
  if(Application.isBatchMode)EditorApplication.Exit(failures==0?0:1);else EditorApplication.ExitPlaymode();
 }
 void Capture(){
  if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
  var cam=Camera.main;var previous=cam.targetTexture;var target=RenderTexture.GetTemporary(1280,720,24);var active=RenderTexture.active;
  cam.targetTexture=target;cam.Render();RenderTexture.active=target;var image=new Texture2D(1280,720,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("Assets/GameDuo/CandyEnemyEncounter.png",image.EncodeToPNG());Destroy(image);RenderTexture.active=active;cam.targetTexture=previous;RenderTexture.ReleaseTemporary(target);
 }
}
}
#endif
