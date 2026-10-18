#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEditor;
using CoopBeam;

// Exercises the real scenes and transitions. Controlled positioning isolates mechanisms;
// accelerated movie seeks and damage shorten the run without replacing completion callbacks.
public sealed class GameDuoCampaignVerification : MonoBehaviour {
 readonly List<string> report=new List<string>();
 readonly List<string> movies=new List<string>();
 readonly HashSet<VideoClip> sought=new HashSet<VideoClip>();
 Keyboard keyboard,original; KeyboardState desired; int failures; bool done; float deadline;
 bool oldRun,oldPause; float oldScale; int savedUnlock,savedFragment;
 InputSettings.BackgroundBehavior oldBackground;
 InputSettings.EditorInputBehaviorInPlayMode oldInput;
 void Check(bool pass,string label){report.Add((pass?"PASS ":"FAIL ")+label);if(!pass)failures++;Debug.Log(report[report.Count-1]);File.WriteAllLines("Temp/CampaignQA-progress.txt",report);}
 void Error(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception){report.Add("FAIL runtime: "+message+"\n"+stack);failures++;}}
 void Before(){if(keyboard!=null&&InputState.currentUpdateType==InputUpdateType.Dynamic)InputSystem.QueueStateEvent(keyboard,desired);}
 void After(){if(keyboard!=null&&InputState.currentUpdateType==InputUpdateType.Dynamic)keyboard.MakeCurrent();}
 void Update(){
  if(done)return;
  if(deadline>0&&Time.realtimeSinceStartup>deadline){Check(false,"Suite timeout");Finish();return;}
  if(GameDuoCinema.Busy){
   var player=FindAnyObjectByType<GameDuoCinema>().GetComponent<VideoPlayer>();
   if(player&&player.isPlaying&&player.time>.2&&player.time<.6)desired=new KeyboardState(Key.R,Key.M,Key.F,Key.Enter);
   if(player&&player.isPlaying&&player.time>.75&&player.clip&&!sought.Contains(player.clip)){
    desired=new KeyboardState();sought.Add(player.clip);movies.Add(player.clip.name);
    Check(player.isPrepared&&player.texture&&player.texture.width>16,"Movie decodes real frames: "+player.clip.name);
    if(player.length>2)player.time=player.length-.25;
   }
  }
 }
 IEnumerator Start(){
  DontDestroyOnLoad(gameObject);oldRun=Application.runInBackground;oldPause=AudioListener.pause;oldScale=Time.timeScale;
  oldBackground=InputSystem.settings.backgroundBehavior;oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;
  savedUnlock=PlayerPrefs.GetInt("GameDuo.Unlocked",0);savedFragment=PlayerPrefs.GetInt("ReinoNeon.FriendshipFragment",0);
  Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  original=Keyboard.current;keyboard=InputSystem.AddDevice<Keyboard>("Campaign verification");
  InputSystem.onBeforeUpdate+=Before;InputSystem.onAfterUpdate+=After;Application.logMessageReceived+=Error;
  deadline=Time.realtimeSinceStartup+600;
  var stack=new Stack<IEnumerator>();stack.Push(Run());
  while(stack.Count>0&&!done){
   var top=stack.Peek();bool more=false;object next=null;Exception failure=null;
   try{more=top.MoveNext();if(more)next=top.Current;}catch(Exception e){failure=e;}
   if(failure!=null){Check(false,"Unhandled verification exception: "+failure);break;}
   if(!more){stack.Pop();continue;}
   if(next is IEnumerator nested){stack.Push(nested);continue;}
   yield return next;
  }
  if(!done)Finish();
 }
 IEnumerator Wait(Func<bool> ready,float seconds,string label){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);if(!ready())throw new Exception("Timed out: "+label);}
 IEnumerator Settle(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
 IEnumerator Guide(){float end=Time.realtimeSinceStartup+12;int frame=0;while(FindAnyObjectByType<GameDuoTutorial>()&&Time.realtimeSinceStartup<end){desired=frame++%3==0?new KeyboardState(Key.F,Key.Enter):new KeyboardState();yield return null;}desired=new KeyboardState();Check(!FindAnyObjectByType<GameDuoTutorial>(),"Both-player tutorial confirmations finish");}
 IEnumerator Run(){
  var library=Resources.Load<GameDuoMovieLibrary>("GameDuoMovies");Check(library&&library.clips.Length==9&&library.clips.All(c=>c&&c.length>0),"Nine imported movie references included in Resources");
  yield return Wait(()=>!GameDuoCinema.Busy,65,"Intro movie finishes and releases overlay");
  yield return Guide();
  yield return Movement();
  yield return Combat(0);
  yield return Wait(()=>SceneManager.GetActiveScene().path==GameDuoCampaign.Scenes[1]&&!GameDuoCinema.Busy,65,"Tutorial -> movies 2,3 -> Neon");
  yield return Neon();
  yield return Wait(()=>SceneManager.GetActiveScene().path==GameDuoCampaign.Scenes[2]&&!GameDuoCinema.Busy,45,"Portal -> movie 4 -> Ice King");
  yield return Combat(2);
  yield return Wait(()=>SceneManager.GetActiveScene().path==GameDuoCampaign.Scenes[3]&&!GameDuoCinema.Busy,45,"Ice King -> movie 5 -> Penguins");
  yield return Penguins();
  yield return Wait(()=>SceneManager.GetActiveScene().path==GameDuoCampaign.Scenes[4]&&!GameDuoCinema.Busy,65,"Penguins -> movies 6,7 -> Candy King");
  yield return Combat(4);
  yield return Wait(()=>GameDuoCampaign.Instance.Completed&&!GameDuoCinema.Busy,65,"Candy King -> movie 8 -> the final -> completed");
  Check(string.Join(",",movies)=="1,2,3,4,5,6,7,8,the final","Exact complete movie order: "+string.Join(",",movies));
  Check(Time.timeScale==1&&!AudioListener.pause,"Final transition restores simulation and audio");
  // Re-enter chapters to exercise cached runtime sprites after scene destruction.
  for(int stage=0;stage<5;stage++){
   SceneManager.LoadScene(GameDuoCampaign.Scenes[stage]);yield return null;yield return null;yield return null;
   Check(GameDuoCampaign.Active&&GameDuoCampaign.Instance.Stage==stage,"Re-entry chapter "+(stage+1));
   if(stage==0)yield return Guide();
   if(stage!=3)yield return Samples(stage);
   else {var round=FindAnyObjectByType<IcePenguinRound>();desired=new KeyboardState(Key.Enter);yield return null;yield return null;desired=new KeyboardState();typeof(IcePenguinRound).GetProperty("Remaining").SetValue(round,.05f);yield return Settle(3.3f);Check(round.Finished&&round.Rescued==0,"Penguin timeout ends round without claiming a rescue");}
  }
  yield return RescueDefeatRestart();
  var cache=GameDuoIcePoseFrames.Load("EnemyExtraFrames",6,1.5f);Destroy(cache[0]);yield return null;
  var recovered=GameDuoIcePoseFrames.Load("EnemyExtraFrames",6,1.5f);
  Check(recovered!=null&&recovered.All(f=>f),"Ice animation cache rebuilds destroyed sprites (Play Mode re-entry regression)");
 }
 void Protect(GameManager game){foreach(var p in game.players)if(p){if(p.Dead)p.Revive();p.Protect(10);}}
 bool Visible(EnemyBase enemy){
  return enemy.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.enabled&&r.gameObject.activeInHierarchy&&r.sprite&&r.color.a>.05f&&r.bounds.size.y>.15f&&!float.IsNaN(r.bounds.size.y)&&!float.IsInfinity(r.bounds.size.y));
 }
 IEnumerator Samples(int stage){
  var game=GameManager.Instance;
  for(int k=0;k<game.enemyPrefabs.Length;k++){
   var e=Instantiate(game.enemyPrefabs[k],new Vector2(-7+(k%5)*3,k==4?1:-2.5f),Quaternion.identity);
   yield return null;yield return null;yield return null;
   Check(Visible(e),"Chapter "+(stage+1)+" enemy "+k+" has visible replacement art");
   Check(e.GetComponents<GameDuoCandyEnemy>().Length<=1,"No duplicate enemy control "+stage+"/"+k);
   if(stage==2){yield return Settle(.15f);Check(Visible(e),"Ice enemy stays visible across pose changes "+k);}
   Destroy(e.gameObject);
  }
 }
 IEnumerator Combat(int stage){
  var game=GameManager.Instance;Check(game&&game.players.Length==2,"Two playable heroes in combat chapter "+(stage+1));
  if(!game.Started)game.Begin();yield return Guide();
  yield return Samples(stage);
  float end=Time.realtimeSinceStartup+160;var patterns=new HashSet<int>();var phases=new HashSet<int>();
  var warmed=new HashSet<EnemyBase>();int observedWave=0;bool hitChecked=false;float kingAt=-1;bool summonSeen=false;
  while(!game.Victory&&Time.realtimeSinceStartup<end){
   Protect(game);observedWave=Mathf.Max(observedWave,game.Wave);
   if(FindAnyObjectByType<GameDuoTutorial>()){yield return Guide();continue;}
   foreach(var enemy in game.enemies.ToArray()){
    if(!enemy||enemy.Dead)continue;
    if(warmed.Add(enemy)){yield return null;yield return null;yield return null;}
    var ice=enemy.GetComponent<IceKingBoss>();var candy=enemy.GetComponent<CandyBoss>();
    if(ice){
     phases.Add(ice.Phase);if(ice.Attacking)patterns.Add(ice.PatternIndex);
     if(!ice.Vulnerable&&!hitChecked){float hp=enemy.Health;enemy.Damage(1);Check(enemy.Health==hp,"Closed ice core rejects damage");hitChecked=true;}
     if(ice.Vulnerable&&(patterns.Count==4||ice.Phase>1))enemy.Damage(100000);
    }else if(candy){
     if(kingAt<0)kingAt=Time.time;
     var king=enemy.GetComponent<GameDuoCandyKing>();summonSeen|=king&&king.Reinforcements>0;
     foreach(var guard in FindObjectsByType<GameDuoCandyEnemy>())if(guard.Owner==king&&!guard.GetComponent<EnemyBase>().Dead)Check(Visible(guard.GetComponent<EnemyBase>()),"Summoned guard remains visible");
     if(candy.Vulnerable&&Time.time-kingAt>15)enemy.Damage(enemy.maxHealth*.4f);
    }else {
     Check(Visible(enemy),"Live wave enemy visible: chapter "+(stage+1)+" kind "+enemy.kind);
     var center=enemy.GetComponent<Collider2D>().bounds.center;float hp=enemy.Health;
     FindAnyObjectByType<global::CoopBeam.CoopBeam>().DamageSegment(center+Vector3.left*2,center+Vector3.right*2,.1f);
     Check(enemy.Health<hp,"Real beam collision damages chapter "+(stage+1)+" enemy "+enemy.kind);
     enemy.Damage(100000);
    }
   }
   yield return Settle(.15f);
  }
  Check(game.Victory&&observedWave==3,"All three combat waves finish in chapter "+(stage+1));
  if(stage==2){Check(patterns.Count==4&&phases.Count==3,"Ice King executes four patterns and three phases");Check(EncounterHazard.LiveCount==0,"Ice boss defeat cleans hazards");}
  if(stage==4)Check(summonSeen,"Candy King summons reinforcements before defeat");
  Capture("chapter-"+(stage+1));
 }
 void HitHero(PlayerHealth hero){var immune=typeof(PlayerHealth).GetField("immuneUntil",BindingFlags.NonPublic|BindingFlags.Instance);for(int n=0;n<3;n++){immune.SetValue(hero,-1f);hero.Hit(Vector2.up);}}
 IEnumerator RescueDefeatRestart(){
  var game=GameManager.Instance;game.BeginPractice();yield return null;
  HitHero(game.players[0]);Check(game.players[0].Dead&&game.Running,"One fallen hero keeps cooperative game running");
  yield return Settle(.8f);game.players[1].ReviveAt(game.players[0].transform.position+Vector3.right*.35f);
  yield return Settle(2.2f);Check(!game.players[0].Dead,"Partner contact revives fallen hero");
  HitHero(game.players[0]);HitHero(game.players[1]);Check(game.players.All(p=>p.Dead)&&!game.Running,"Both fallen heroes trigger defeat");
  game.Restart();yield return null;yield return null;yield return null;
  Check(GameManager.Instance!=game&&GameManager.Instance.players.All(p=>!p.Dead),"Restart rebuilds chapter and restores both heroes");
 }
 IEnumerator Movement(){
  var game=GameManager.Instance;Protect(game);
  for(int i=0;i<2;i++){
   var hero=game.players[i];float x=hero.transform.position.x;
   desired=new KeyboardState(i==0?Key.D:Key.LeftArrow);yield return Settle(.25f);desired=new KeyboardState();
   Check(Mathf.Abs(hero.transform.position.x-x)>.3f,"Hero "+i+" moves with real keyboard input");
   yield return Settle(.4f);float y=hero.transform.position.y;desired=new KeyboardState(i==0?Key.W:Key.UpArrow);yield return Settle(.2f);desired=new KeyboardState();
   Check(hero.transform.position.y>y+.3f,"Hero "+i+" jumps with real keyboard input");yield return Settle(.8f);
  }
 }
 void Place(NeonLevel level,int player,Transform target){level.game.players[player].ReviveAt(target.position);Physics2D.SyncTransforms();}
 IEnumerator Neon(){
  var level=FindAnyObjectByType<NeonLevel>();Check(level&&level.exit.GetComponent<GameDuoNeonPortal>(),"Vertical portal installed");
  desired=new KeyboardState(Key.Enter);yield return null;yield return null;desired=new KeyboardState();
  yield return Settle(.2f);
  for(int i=0;i<level.crossed.Length;i++){
   var device=level.crossed[i];Place(level,device.player,device.transform);yield return Settle(.25f);
   Check(level.Interact(device.player)&&device.powered&&device.target.powered,"Reciprocal neon control "+i);
  }
  level.battery.transform.position=level.socketA.position;yield return Settle(.2f);
  Place(level,1,level.terminal);yield return Settle(.1f);Check(level.Interact(1)&&level.Routed,"Battery A enables Pipro routing");
  level.battery.transform.position=level.socketB.position;yield return Settle(.2f);Check(level.RelayDone,"Battery B releases relay");
  Place(level,0,level.coreA);yield return Settle(.3f);Check(level.Interact(0)&&level.CoreStep==1,"Mimo starts core");
  Place(level,1,level.coreB);yield return Settle(.3f);Check(level.Interact(1)&&level.CoreStep==2,"Pipro reverses core");
  Place(level,0,level.coreC);yield return Settle(.3f);Check(level.Interact(0)&&level.CoreStep==3,"Mimo confirms core");
  float until=Time.realtimeSinceStartup+4;while(!level.Restored&&Time.realtimeSinceStartup<until){Place(level,0,level.padA);Place(level,1,level.padB);yield return null;}
  Check(level.Restored&&level.finalGate.powered,"Both plates restore reactor");
  Place(level,0,level.fragment);Place(level,1,level.fragment);yield return Settle(.2f);Check(level.Collected,"Both heroes collect heart fragment");
  Place(level,0,level.exit);level.game.players[1].ReviveAt(level.exit.position+Vector3.left*5);yield return Settle(.15f);Check(!level.Completed,"Portal rejects one player alone");
  Capture("neon-portal");Place(level,0,level.exit);Place(level,1,level.exit);yield return Settle(.2f);Check(level.Completed,"Both heroes enter portal");
 }
 IEnumerator Penguins(){
  var round=FindAnyObjectByType<IcePenguinRound>();desired=new KeyboardState(Key.Enter);yield return null;yield return null;desired=new KeyboardState();
  yield return Settle(3.3f);Check(round.Started&&!round.Finished,"Penguins start and countdown");
  // Controlled setup at the refuge exercises the actual rescue/counter/finish logic.
  var birds=(Array)typeof(IcePenguinRound).GetField("birds",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(round);
  foreach(var bird in birds)bird.GetType().GetField("p").SetValue(bird,new Vector2(0,-3.5f));
  yield return Settle(.2f);Check(round.Rescued==IcePenguinRound.FlockSize&&round.Finished,"All sixteen penguins enter refuge through real update logic");Capture("penguins");
 }
 void Capture(string label){
  var cam=Camera.main;if(!cam||SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
  var prior=cam.targetTexture;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1280,720,24);
  try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();Directory.CreateDirectory("Temp/CampaignQA");File.WriteAllBytes("Temp/CampaignQA/"+label+".png",tex.EncodeToPNG());Destroy(tex);}finally{cam.targetTexture=prior;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
 }
 void Finish(){
  if(done)return;done=true;Cleanup();report.Insert(0,"Campaign Play Mode verification: "+failures+" failures / "+DateTime.Now.ToString("s"));
  report.Add("Scope: real scenes, rendering, video decoding/end events, beam damage, boss phases, controlled neon mechanisms and refuge positioning. Movie playback seeks near end after decoding; this is not a manual full-duration playthrough.");
  File.WriteAllLines("Assets/GameDuo/CampaignVerification.txt",report);Debug.Log(report[0]);EditorApplication.ExitPlaymode();
 }
 void Cleanup(){Application.logMessageReceived-=Error;InputSystem.onBeforeUpdate-=Before;InputSystem.onAfterUpdate-=After;if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(original!=null&&original.added)original.MakeCurrent();InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;Application.runInBackground=oldRun;AudioListener.pause=oldPause;Time.timeScale=1;PlayerPrefs.SetInt("GameDuo.Unlocked",savedUnlock);PlayerPrefs.SetInt("ReinoNeon.FriendshipFragment",savedFragment);PlayerPrefs.Save();}
 void OnDestroy(){if(!done)Cleanup();}
}
#endif

