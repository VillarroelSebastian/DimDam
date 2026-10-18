#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
namespace CoopBeam {
public sealed class TutorialFactoryPlayQA:MonoBehaviour {
 List<string> report=new List<string>();Keyboard keyboard;GameManager game;int failures;float deadline;bool done,captured;
 void Check(bool ok,string label){report.Add((ok?"PASS ":"FAIL ")+label);Debug.Log((ok?"PASS ":"FAIL ")+label);if(!ok)failures++;}
 void Log(string message,string trace,LogType type){if(type==LogType.Exception||type==LogType.Error)Check(false,"Runtime error: "+message);}
 void Keys(params Key[] keys){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
 void Position(int i,float x,float foot){var p=game.players[i];var c=p.GetComponent<Collider2D>();p.transform.position+=new Vector3(x-p.transform.position.x,foot-c.bounds.min.y,0);p.GetComponent<PlayerController2D>().ResetMotion();Physics2D.SyncTransforms();}
 IEnumerator Frames(int n,params Key[] keys){for(int i=0;i<n;i++){Keys(keys);yield return null;}}
 IEnumerator Start(){
  Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;Application.logMessageReceived+=Log;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;DontDestroyOnLoad(gameObject);deadline=Time.realtimeSinceStartup+180;keyboard=InputSystem.AddDevice<Keyboard>("Tutorial QA");
  yield return null;yield return null;game=GameManager.Instance;
  Check(game&&game.players.Length==2,"Two original playable heroes");
  Check(FindAnyObjectByType<TutorialFactory>(),"Factory environment installed");
  Check(GameObject.Find("Rey Chicle / asustado"),"Supplied frightened king installed");
  Check(!GameObject.Find("Original candy backdrop").GetComponent<SpriteRenderer>().enabled,"Old backdrop and baked characters hidden");
  game.Begin();foreach(var p in game.players)p.Protect(180);
  yield return Frames(10);
  var guide=game.GetComponent<GameDuoTutorial>();Check(guide&&Time.timeScale==0,"One explanation appears automatically and pauses combat");
  yield return Frames(3);
  yield return Frames(2,Key.F);yield return Frames(2);
  Check(guide&&guide.Confirmed(0)&&!guide.Confirmed(1)&&Time.timeScale==0,"Mimo confirms independently; Pipro still has to confirm");
  yield return Frames(2,Key.Enter);yield return Frames(3);
  Check(guide&&guide.Step==1&&!guide.Confirmed(0)&&!guide.Confirmed(1)&&Time.timeScale==0,"Rescue instruction resets both confirmations");
  for(int step=1;step<4;step++){yield return Frames(2,Key.F);yield return Frames(2);Check(guide&&guide.Confirmed(0)&&!guide.Confirmed(1),"Each tutorial step needs both players / "+step);yield return Frames(2,Key.Enter);yield return Frames(3);}
  Check(!game.GetComponent<GameDuoTutorial>()&&Time.timeScale==1,"Four confirmed instructions resume combat");
  float x0=game.players[0].transform.position.x,x1=game.players[1].transform.position.x;
  yield return Frames(35,Key.D,Key.RightArrow);
  Check(game.players[0].transform.position.x>x0+2&&game.players[1].transform.position.x>x1+2,"Both players move using actual keyboard input");
  yield return Frames(15);
  for(int i=0;i<2;i++)Position(i,i==0?-6:6,-4.28f);
  yield return Frames(12);
  float[] peak={-10,-10};
  for(int n=0;n<95;n++){Keys(n<42?new[]{Key.W,Key.UpArrow}:new Key[0]);yield return null;for(int i=0;i<2;i++)peak[i]=Mathf.Max(peak[i],game.players[i].GetComponent<Collider2D>().bounds.min.y);}
  Check(peak[0]>-1.9f&&peak[1]>-1.9f,"Both heroes jump high enough to reach throne steps");
  yield return Frames(80);
  Check(game.players[0].GetComponent<PlayerController2D>().Grounded&&game.players[1].GetComponent<PlayerController2D>().Grounded,"Both land safely after jumping");
  yield return Frames(5,Key.LeftShift,Key.RightCtrl);
  Check(!game.players[0].GetComponent<PlayerController2D>().Dashing&&!game.players[1].GetComponent<PlayerController2D>().Dashing,"Sprint is disabled for both GameDuo players");
  yield return Frames(25);
  Check(!FindAnyObjectByType<GameDuoTutorial>(),"No movement-gated tutorial panels return");
  yield return Frames(10);
  Check(!GameObject.Find("Cerradura central")&&!GameObject.Find("Llave compartida")&&!GameObject.Find("Sello oeste"),"Tutorial has no door, key or seals");
  Check(GameObject.Find("Escalon trono superior")&&GameObject.Find("Escalera derecha 5"),"Image-aligned throne and right staircase colliders installed");
  string[] surfaces={"Suelo del salon","Escalon trono inferior","Escalon trono medio","Escalon trono superior","Escalera derecha 1","Escalera derecha 2","Escalera derecha 3","Escalera derecha 4","Escalera derecha 5","Pedestal izquierdo"};
  float[] tops={709,645,615,588,652,622,592,563,539,551};
  Physics2D.SyncTransforms();
  for(int n=0;n<surfaces.Length;n++){
   var box=GameObject.Find(surfaces[n]).GetComponent<BoxCollider2D>();
   Check(box.enabled&&Mathf.Abs(box.bounds.max.y-TutorialFactory.ImagePoint(0,tops[n]).y)<.01f,"Collider matches painted surface / "+surfaces[n]);
  }
  int visiblePlatforms=0;
  foreach(var box in FindObjectsByType<BoxCollider2D>()){
   if(box.enabled&&!box.isTrigger&&box.gameObject.layer==8)Check(box.name=="Suelo del salon","No solid invisible internal wall / "+box.name);
   if(box.enabled&&box.name.StartsWith("Plataforma ")){visiblePlatforms++;Check(box.GetComponentsInChildren<SpriteRenderer>().Length>3&&box.GetComponent<PlatformEffector2D>().useOneWay,"Visible one-way wafer platform / "+box.name);}
  }
  Check(visiblePlatforms==5,"Five playable floating platforms");
  Position(0,-8.2f,-1.84f);yield return Frames(20);
  Check(game.players[0].GetComponent<PlayerController2D>().Grounded,"Hero stands on added wafer platform");
  yield return Frames(2,Key.S);yield return Frames(24);
  Check(game.players[0].GetComponent<Collider2D>().bounds.min.y< -2.1f,"Down key drops through added platform");
  Position(0,TutorialFactory.ImagePoint(800,588).x,TutorialFactory.ImagePoint(800,588).y+.06f);
  Position(1,TutorialFactory.ImagePoint(1530,539).x,TutorialFactory.ImagePoint(1530,539).y+.06f);
  yield return Frames(30);
  for(int i=0;i<2;i++)Check(game.players[i].GetComponent<PlayerController2D>().Grounded,"Hero stands on illustrated staircase / "+i);
  Position(0,-6,-4.28f);Position(1,6,-4.28f);
  var royal=GameObject.Find("Rey Chicle / asustado").GetComponent<SpriteRenderer>();
  var pose=royal.sprite;bool changed=false;for(int n=0;n<90;n++){yield return Frames(1);changed|=royal.sprite!=pose;}
  Check(changed,"King changes frightened poses over time");
  var genericPrefab=game.enemyPrefabs[0];
  game.GetComponent<LinkPowers>().AddCharge(LinkPowers.ChargeRequired);yield return Frames(3);
  int auraCount=0;foreach(var a in FindObjectsByType<SpriteRenderer>())if(a.name.StartsWith("Aura de energia")){
   auraCount++;int player=a.name.EndsWith("0")?0:1;
   Check(a.enabled&&a.bounds.size.y>game.players[player].GetComponent<Collider2D>().bounds.size.y&&Mathf.Abs(a.transform.position.y-game.players[player].GetComponent<Collider2D>().bounds.center.y)<.1f,"Energy aura surrounds hero body / "+player);
  }
  Check(auraCount==2&&!game.GetComponent<CoopInteractionDirector>().TutorialOpen,"Power readiness adds no blocking explanation");
  var powers=game.GetComponent<LinkPowers>();
  Check(!powers.Confirm(0)&&powers.IsConfirmed(0),"First ultimate confirmation waits for partner");
  yield return Frames(150);
  Check(powers.IsConfirmed(0)&&!game.GetComponent<CoopInteractionDirector>().Dancing,"Ultimate confirmation persists beyond two seconds");
  Check(powers.Confirm(1)&&game.GetComponent<CoopInteractionDirector>().Dancing,"Partner confirmation starts dance directly");
  yield return Frames(680);
  Check(!game.GetComponent<CoopInteractionDirector>().Dancing&&powers.UltimatesUsed>0&&powers.Empowered,"Minigame ends with explosion and stronger link");
  Check(!GameObject.Find("Baliza luminosa 0"),"No decorative lamp objects obscure tutorial");
  foreach(var w in Resources.FindObjectsOfTypeAll<EditorWindow>())if(w.GetType().Name=="GameView"){
   var prop=w.GetType().GetProperty("drawGizmos",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);
   if(prop!=null)Check(!(bool)prop.GetValue(w),"Game View hides audio and light gizmo icons");
  }
  Keys();var director=game.GetComponent<CoopInteractionDirector>();
  // Verify real beam collision and damage while controlling positioning to cover every wave deterministically.
  int maxWave=0,kills=0;float until=Time.realtimeSinceStartup+75;
  while(!game.Victory&&Time.realtimeSinceStartup<until){
   if(director.TutorialOpen){Keys(Key.Escape);yield return null;Keys();yield return null;continue;}
   maxWave=Mathf.Max(maxWave,game.Wave);
   foreach(var e in game.enemies.ToArray()){
    if(!e||e.Dead)continue;
    yield return null;yield return null;
    if(game.Wave>=1&&!captured){Capture();captured=true;}
    Check(e.GetComponent<TutorialSootEnemy>()&& !e.sprite.enabled,"Bill atlas replaces old art / "+e.kind);
    var c=e.GetComponent<Collider2D>();var center=c.bounds.center;
    float before=e.Health;int hits=FindAnyObjectByType<CoopBeam>().DamageSegment(center+Vector3.left*2,center+Vector3.right*2,.10f);
    Check(hits>0&&e.Health<before,"Real beam linecast damages "+e.kind);
    yield return null;
    var soot=e.GetComponent<TutorialSootEnemy>();
    Check(soot.Art.enabled&&soot.Art.sprite&&soot.Art.bounds.size.y>1f,"Bill has a live, visible sprite / "+e.kind);
    Check(soot.Art.sprite.name.StartsWith("Bill /")&&soot.Art.sharedMaterial.GetFloat("_Hit")>.9f,"Damage flashes red with dedicated hurt expression / "+e.kind);
    Check(Mathf.Abs(soot.Art.bounds.min.y-c.bounds.min.y)<.03f,"Bill feet align with physics feet / "+e.kind);
    for(int n=0;n<120&&e&&!e.Dead;n++){
     var p=c.bounds.center;Position(0,p.x-1.6f,p.y-game.players[0].GetComponent<Collider2D>().bounds.extents.y);Position(1,p.x+1.6f,p.y-game.players[1].GetComponent<Collider2D>().bounds.extents.y);
     if(director.TutorialOpen){Keys(Key.Escape);}else Keys();yield return null;
    }
    if(!e||e.Dead){kills++;if(e){yield return new WaitForSeconds(.16f);Check(soot&&soot.DissolveProgress>0&&soot.Art.sharedMaterial.GetFloat("_Dissolve")>0,"Death progressively disintegrates sprite / "+e.kind);Check(FindObjectsByType<TutorialAsh>().Length>0,"Ash particles separate on death");}}

   }
   yield return null;
  }
  Check(maxWave==3&&game.Victory,"Three waves finished through beam combat; kills="+kills);
  Check(game.Score>0,"Combat score awarded");
  float next=Time.realtimeSinceStartup+5;while(SceneManager.GetActiveScene().path==GameDuoCampaign.Scenes[0]&&Time.realtimeSinceStartup<next)yield return null;
  Check(SceneManager.GetActiveScene().path==GameDuoCampaign.Scenes[1],"Victory advances to Neon chapter");
  var generic=Instantiate(genericPrefab,new Vector3(0,-2,0),Quaternion.identity);yield return Frames(3);generic.FinishDeath();
  yield return Frames(12);Check(generic&&generic.GetComponent<EnemyDisintegration>()&&generic.sprite.sharedMaterial.GetFloat("_Dissolve")>0,"Enemies in other arenas also disintegrate");
  yield return Frames(50);Check(!generic,"Disintegrated enemy is cleaned up");
  SceneManager.LoadScene(GameDuoCampaign.Scenes[0]);yield return null;yield return null;game=GameManager.Instance;game.Begin();
  yield return Frames(10);for(int step=0;step<4;step++){yield return Frames(2,Key.F,Key.Enter);yield return Frames(5); }
  game.StopAllCoroutines();foreach(var e in game.enemies)if(e)Destroy(e.gameObject);game.enemies.Clear();
  for(int i=0;i<2;i++)Position(i,i==0?-2:2,-4.28f);
  for(int n=0;n<3;n++){game.players[0].Hit(Vector2.up);if(n<2)yield return new WaitForSeconds(1.5f);}
  Check(game.players[0].Dead&&game.Running,"One downed hero keeps cooperative round running");
  yield return Frames(5);yield return Frames(2,Key.Escape);yield return Frames(5);
  Position(1,game.players[0].transform.position.x,-4.28f);yield return Frames(200);
  Check(!game.players[0].Dead,"Partner rescues downed hero through contact");
  yield return new WaitForSeconds(2.2f);
  for(int n=0;n<3;n++){game.players[0].Hit(Vector2.up);game.players[1].Hit(Vector2.up);if(n<2)yield return new WaitForSeconds(1.5f);}
  Check(!game.Running&&game.players[0].Dead&&game.players[1].Dead,"Both downed heroes trigger defeat");
  yield return Frames(2,Key.R);yield return Frames(8);
  game=GameManager.Instance;Check(game&&game.Started&&game.GetComponent<GameDuoTutorial>()&&!game.players[0].Dead&&!game.players[1].Dead,"R restarts tutorial with both heroes restored");
  Finish();
 }
 void Capture(){var c=Camera.main;var rt=new RenderTexture(1600,900,24);var previous=c.targetTexture;var previousRect=c.rect;c.rect=new Rect(0,0,1,1);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes("Assets/GameDuo/TutorialFactory-preview.png",tex.EncodeToPNG());c.targetTexture=previous;c.rect=previousRect;RenderTexture.active=old;rt.Release();Destroy(rt);Destroy(tex);}
 void Update(){if(!done&&deadline>0&&Time.realtimeSinceStartup>deadline){Check(false,"QA timeout");Finish();}}
 void Finish(){if(done)return;done=true;Keys();InputSystem.RemoveDevice(keyboard);report.Insert(0,"Tutorial factory QA / failures="+failures);File.WriteAllLines("Assets/GameDuo/TutorialFactoryQA.txt",report);Debug.Log(report[0]);Time.timeScale=1;if(SessionState.GetBool("Factory.Batch",false))EditorApplication.Exit(failures==0?0:1);else EditorApplication.ExitPlaymode();}
 void OnDestroy(){Application.logMessageReceived-=Log;if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);}
}
}
#endif
