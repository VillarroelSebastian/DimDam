using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoopBeam {
// Scene-local audio observes combat state without changing any combat behaviour.
[DefaultExecutionOrder(2000)]
public sealed class GameDuoFinalAudio : MonoBehaviour {
 const string ScenePath="Assets/GameDuo/Scenes/05_JefeChicle.unity";
 readonly Dictionary<EnemyBase,State> states=new Dictionary<EnemyBase,State>();
 readonly List<EnemyBase> expired=new List<EnemyBase>();
 readonly AudioSource[] voices=new AudioSource[6];
 readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
 AudioSource music,tension; GameManager game; float nextCue,scanAt; int phase=1; bool wasRunning;
 struct State { public float health,attackAt,hurtAt; public bool warning,attack,summoning,dead; }

 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 static void Install(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Initial(){Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
 static void Loaded(Scene scene,LoadSceneMode mode){
  if(scene.path!=ScenePath)return;
  if(!FindAnyObjectByType<GameDuoFinalAudio>()){
   var go=new GameObject("Audio / batalla final");SceneManager.MoveGameObjectToScene(go,scene);go.AddComponent<GameDuoFinalAudio>();
  }
 }
 AudioSource Source(){var s=gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=0;s.volume=0;s.dopplerLevel=0;return s;}
 void Start(){
  game=FindAnyObjectByType<GameManager>();
  foreach(var name in new[]{"Battle","Tension","Warning","Attack","Hurt","Defeat","Bats","Rain","Phase"}){
   var clip=Resources.Load<AudioClip>("FinalAudio/"+name);if(clip)clips[name]=clip;
   else Debug.LogError("Missing final encounter audio: "+name);
  }
  music=Source();tension=Source();music.loop=tension.loop=true;
  if(clips.TryGetValue("Battle",out var battle)&&clips.TryGetValue("Tension",out var layer)){
   music.clip=battle;tension.clip=layer;
   double at=AudioSettings.dspTime+.15;music.PlayScheduled(at);tension.PlayScheduled(at);
  }
  for(int i=0;i<voices.Length;i++){voices[i]=Source();voices[i].volume=.18f;voices[i].priority=80;}
  music.priority=100;tension.priority=110;
 }
 void Cue(string name,float pitch=1,float gain=1){
  if(Time.unscaledTime<nextCue||!clips.TryGetValue(name,out var clip))return;
  foreach(var voice in voices)if(!voice.isPlaying){
   voice.pitch=pitch;voice.volume=.18f*gain;voice.clip=clip;voice.Play();nextCue=Time.unscaledTime+.065f;return;
  }
 }
 void Update(){
  if(!game||!music)return;
  bool cinematic=GameDuoCinema.Busy;
  bool running=game.Running&&!cinematic&&Time.timeScale>0&&!AudioListener.pause;
  // Listener pause also pauses these sources during movies. Fade before resuming gameplay.
  float target=running?.32f:0;
  music.volume=Mathf.MoveTowards(music.volume,target,Time.unscaledDeltaTime*.32f);
  tension.volume=Mathf.MoveTowards(tension.volume,running?(phase-1)*.085f:0,Time.unscaledDeltaTime*.2f);
  if(!running){
   if(wasRunning||cinematic){foreach(var voice in voices)voice.Stop();}
   if(cinematic){music.volume=0;tension.volume=0;}
   wasRunning=false;return;
  }
  wasRunning=true;
  if(Time.unscaledTime<scanAt)return;scanAt=Time.unscaledTime+.04f;
  foreach(var enemy in game.enemies){
   if(!enemy)continue;
   var boss=enemy.GetComponent<CandyBoss>();var king=enemy.GetComponent<GameDuoCandyKing>();
   if(!states.TryGetValue(enemy,out var old))old=new State{health=enemy.Health,attackAt=boss?boss.LastAttackTime:0};
   float pitch=boss?.75f:enemy.kind==EnemyKind.Flyer?1.5f:enemy.kind==EnemyKind.Cannon?.85f:1.1f;
   if(enemy.Dead&&!old.dead)Cue("Defeat",pitch);
   else if(enemy.Health<old.health&&Time.time>=old.hurtAt){Cue("Hurt",pitch,.7f);old.hurtAt=Time.time+.22f;}
   bool warning=boss?boss.WindingUp:enemy.Telegraphing;
   bool attack=enemy.Attacking;
   if(warning&&!old.warning)Cue("Warning",pitch,.7f);
   if(attack&&!old.attack)Cue("Attack",pitch);
   if(boss){
    if(boss.Phase>phase){phase=boss.Phase;Cue("Phase",1,1.1f);}
    if(boss.LastAttackTime>old.attackAt)Cue("Rain",.85f);
    old.attackAt=boss.LastAttackTime;
   }
   if(king&&king.Summoning&&!old.summoning)Cue("Bats",.8f);
   old.health=enemy.Health;old.warning=warning;old.attack=attack;old.summoning=king&&king.Summoning;old.dead=enemy.Dead;
   states[enemy]=old;
  }
  // EnemyKilled removes enemies immediately; retain their state until the next scan.
  expired.Clear();foreach(var pair in states)if(!pair.Key||pair.Key.Dead||!game.enemies.Contains(pair.Key)){
   if(pair.Key&&pair.Key.Dead&&!pair.Value.dead)Cue("Defeat",pair.Key.kind==EnemyKind.Boss?.7f:1);
   expired.Add(pair.Key);
  }
  foreach(var enemy in expired)states.Remove(enemy);
 }
}
}
