#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace CoopBeam {
[DefaultExecutionOrder(2000)]
public sealed class GameDuoFinalPlayQA : MonoBehaviour {
 bool captureRequested;
 readonly List<string> report=new List<string>();int failures;
 void Check(bool ok,string message){report.Add((ok?"PASS ":"FAIL ")+message);if(!ok)failures++;}
 IEnumerator Start(){
  yield return null;yield return null;
  var game=GameManager.Instance;
  Check(GameDuoCampaign.Scenes.Length==5&&GameDuoCampaign.Instance.Stage==4,"Final is chapter five");
  Check(FindAnyObjectByType<GameDuoFinalArena>(),"New throne room installed");
  foreach(string name in new[]{"PinkKing","Bats"}){var texture=Resources.Load<Texture2D>("GameDuo/Final/"+name);Check(texture&&texture.GetPixel(0,0).a<.1f,name+" transparent atlas imported");}
  var backdrop=GameObject.Find("Salon del trono / final.png");Check(backdrop&&backdrop.GetComponent<SpriteRenderer>().sprite,"Throne room sprite resolves in final scene");
  game.BeginPractice();var director=game.GetComponent<CoopInteractionDirector>();if(director)director.enabled=false;
  foreach(var player in game.players)player.Protect(120);
  var enemy=Instantiate(game.enemyPrefabs[game.enemyPrefabs.Length-1],new Vector2(0,-2.65f),Quaternion.identity);
  yield return null;yield return null;
  var boss=enemy.GetComponent<CandyBoss>();var motion=enemy.GetComponent<GameDuoCandyKing>();
  Check(boss&&motion,"Original combat and pink animation installed");Check(enemy.sprite.sprite.name.StartsWith("PinkKing_"),"Pink boss pose displayed");
  var vertical=GameDuoBat.Spawn(motion,-6,6,1,5,enemy.sprite.sharedMaterial);vertical.SetVertical();
  yield return new WaitForSeconds(.2f);Check(vertical&&vertical.transform.position.y<5.5f&&Mathf.Abs(vertical.transform.position.x+6)<.2f,"Vertical bats descend in their announced column");if(vertical)Destroy(vertical.gameObject);
  var small=Instantiate(game.enemyPrefabs[0],new Vector2(9,4),Quaternion.identity);small.maxHealth=10000;game.enemies.Add(small);yield return null;
  CoopUltimateModes.ApplyUltimateDamage(small,1);Check(small.Dead&&small.Health==0&&!game.enemies.Contains(small),"Ultimate kills high-health small enemies and removes them from wave tracking");
  bool wind=false,bats=false,sugar=false,captured=false;float until=Time.time+12;
  while(Time.time<until){wind|=motion.Summoning&&!motion.Releasing;bats|=FindAnyObjectByType<GameDuoBat>();if(bats&&!captured){captureRequested=true;captured=true;}sugar|=boss.LastAttackTime>0;yield return null;}
  Check(wind&&bats,"Hands telegraph precedes animated bat flock");Check(sugar,"Original sugar rain still fires");
  while(!boss.Vulnerable)yield return null;
  enemy.Damage(enemy.maxHealth*.36f);yield return new WaitForFixedUpdate();Check(boss.Phase==2,"Phase two retained");
  while(!boss.Vulnerable)yield return null;
  enemy.Damage(enemy.maxHealth*.34f);yield return new WaitForFixedUpdate();Check(boss.Phase==3,"Phase three retained");
  // Exercise the actual trigger and PlayerHealth path, independently of the random encounter.
  var hero=game.players[0];hero.Revive();hero.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;
  hero.transform.position=new Vector3(-7,2,0);yield return new WaitForSeconds(2.1f);
  var before=hero.Hearts;var bat=GameDuoBat.Spawn(motion,-8,2,1,6,enemy.sprite.sharedMaterial);
  yield return new WaitForSeconds(.4f);Check(hero.Hearts==before-1,"Bat contact removes one heart");
  yield return new WaitForSeconds(1.6f);hero.transform.position=new Vector3(-7,4,0);before=hero.Hearts;
  GameDuoBat.Spawn(motion,-8,2,1,6,enemy.sprite.sharedMaterial);yield return new WaitForSeconds(.4f);Check(hero.Hearts==before,"Changing height avoids flock lane");
  while(!boss.Vulnerable)yield return null;enemy.Damage(10000);yield return null;yield return null;
  Check(enemy.Dead&&FindObjectsByType<GameDuoBat>().Length==0,"Boss defeat removes remaining bats");
  typeof(GameDuoCampaign).GetMethod("Advance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(GameDuoCampaign.Instance,null);
  Check(GameDuoCampaign.Instance.Completed,"Chapter five ends campaign");
  report.Insert(0,failures==0?"PASS / GameDuo final encounter":"FAIL / "+failures);
  File.WriteAllLines("Assets/GameDuo/FinalPlayModeQA.txt",report);Debug.Log(report[0]);
  if(Application.isBatchMode)EditorApplication.Exit(failures==0?0:1);else EditorApplication.ExitPlaymode();
 }
 void LateUpdate(){if(captureRequested){captureRequested=false;var enemy=FindAnyObjectByType<GameDuoCandyKing>().GetComponent<EnemyBase>();Check(enemy.sprite.bounds.size.y<4.2f,"Boss art remains aligned with combat body");Capture();}}
 void Capture(){
  if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
  var cam=Camera.main;var previous=cam.targetTexture;var rt=RenderTexture.GetTemporary(1280,720,24);
  cam.targetTexture=rt;cam.Render();var active=RenderTexture.active;RenderTexture.active=rt;
  var image=new Texture2D(1280,720,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
  File.WriteAllBytes("Assets/GameDuo/FinalEncounter.png",image.EncodeToPNG());Destroy(image);RenderTexture.active=active;cam.targetTexture=previous;RenderTexture.ReleaseTemporary(rt);
 }
}
}
#endif
