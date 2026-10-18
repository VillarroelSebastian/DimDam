using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
namespace CoopBeam {
// Persistent overlay owns both movie playback and asynchronous chapter changes.
public sealed class GameDuoCinema : MonoBehaviour {
 static GameDuoCinema instance;
 public static bool Busy => instance && instance.busy;
 public static bool IntroPlayed { get; private set; }
 bool busy, failed, ended, showingMovie; float opacity; string error;
 VideoPlayer player; AudioSource audioOutput; RenderTexture target;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
 static void ResetSession(){instance=null;IntroPlayed=false;}
 public static void Play(int[] movies, string scene, Action complete=null) {
  if(Busy)return;
  if(!instance){var host=new GameObject("GameDuo / Cine y transiciones");instance=host.AddComponent<GameDuoCinema>();DontDestroyOnLoad(host);}
  IntroPlayed=true;
  instance.StartCoroutine(instance.Sequence(movies,scene,complete));
 }
 IEnumerator Fade(float to) {
  float from=opacity;
  for(float t=0;t<.4f;t+=Time.unscaledDeltaTime){opacity=Mathf.Lerp(from,to,Mathf.SmoothStep(0,1,t/.4f));if(audioOutput)audioOutput.volume=1-opacity;yield return null;}
  opacity=to;if(audioOutput)audioOutput.volume=1-opacity;
 }
 IEnumerator Sequence(int[] movies,string scene,Action complete) {
  busy=true; float scale=Time.timeScale; bool paused=AudioListener.pause;
  Time.timeScale=0; AudioListener.pause=true;
  yield return Fade(1);
  var library=Resources.Load<GameDuoMovieLibrary>("GameDuoMovies");
  foreach(int index in movies) {
   yield return new WaitForSecondsRealtime(1);
   failed=false;ended=false;error=null;
   if(!library||library.clips==null||index>=library.clips.Length||!library.clips[index]){failed=true;error="No se encontro el video "+(index+1);}
   else {
    if(!player){
     player=gameObject.AddComponent<VideoPlayer>();player.playOnAwake=false;player.isLooping=false;
     player.renderMode=VideoRenderMode.RenderTexture;player.audioOutputMode=VideoAudioOutputMode.AudioSource;
     audioOutput=gameObject.AddComponent<AudioSource>();audioOutput.ignoreListenerPause=true;audioOutput.spatialBlend=0;
     player.errorReceived+=(p,message)=>{failed=true;error=message;Debug.LogError("GameDuo video: "+message);};
     player.loopPointReached+=p=>ended=true;
    }
    player.clip=library.clips[index];
    if(target){player.targetTexture=null;target.Release();Destroy(target);}
    target=new RenderTexture(Mathf.Max(16,(int)player.clip.width),Mathf.Max(16,(int)player.clip.height),0);target.Create();player.targetTexture=target;
    if(player.clip.audioTrackCount>0){player.EnableAudioTrack(0,true);player.SetTargetAudioSource(0,audioOutput);}
    player.Prepare();float deadline=Time.realtimeSinceStartup+30;
    while(!player.isPrepared&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
    if(!player.isPrepared){failed=true;error="No se pudo preparar el video. Revisa su importacion en Unity.";}
   }
   if(!failed){showingMovie=true;player.Play();yield return Fade(0);while(!ended&&!failed)yield return null;yield return Fade(1);}
   if(failed){Debug.LogError(error);while(failed)yield return null;}
   if(player)player.Stop();showingMovie=false;
  }
  yield return new WaitForSecondsRealtime(1);
  if(scene!=null){
   LinkPowers.ResetStoredCharge();CandyCampaign.ArrivalPending=false;
   var load=SceneManager.LoadSceneAsync(scene);
   while(!load.isDone)yield return null;
   yield return null;
  }
  complete?.Invoke();
  yield return Fade(0);
  Time.timeScale=scene!=null?1:scale;AudioListener.pause=paused;busy=false;
 }
 void LateUpdate(){if(busy)Time.timeScale=0;}
 void OnGUI(){
  if(!busy)return;
  GUI.depth=-10000;GUI.matrix=Matrix4x4.identity;GUI.color=Color.white;
  if(showingMovie){GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.blackTexture);if(target)GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),target,ScaleMode.ScaleToFit);}
  GUI.color=new Color(1,1,1,opacity);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.blackTexture);GUI.color=Color.white;
  if(failed){GUI.Label(new Rect(30,Screen.height/2-40,Screen.width-60,50),"No se pudo reproducir la cinematica. "+error);if(GUI.Button(new Rect(Screen.width/2-100,Screen.height/2+20,200,40),"Continuar"))failed=false;}
  // Consume clicks so chapter menus below the full-screen overlay cannot be activated.
  GUI.Box(new Rect(0,0,Screen.width,Screen.height),GUIContent.none,GUIStyle.none);
 }
 void OnDestroy(){if(busy){Time.timeScale=1;AudioListener.pause=false;}if(target){target.Release();Destroy(target);}if(instance==this)instance=null;}
}
}


