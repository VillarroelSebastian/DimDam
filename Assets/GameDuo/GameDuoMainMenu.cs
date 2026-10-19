using UnityEngine;
namespace CoopBeam {
public sealed class GameDuoMainMenu : MonoBehaviour {
 public const string ScenePath="Assets/GameDuo/Scenes/00_Menu.unity";
 Texture2D background; GameDuoTutorial controls; AudioSource music;
 void Awake(){
  Time.timeScale=1;AudioListener.pause=false;
  background=Resources.Load<Texture2D>("MainMenu");
  controls=gameObject.AddComponent<GameDuoTutorial>();
  var cameraObject=new GameObject("Menu camera");
  cameraObject.AddComponent<AudioListener>();
  var camera=cameraObject.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
  music=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.loop=true;
  music.spatialBlend=0;music.ignoreListenerPause=true;music.volume=0;
  music.clip=Resources.Load<AudioClip>("MenuTheme");if(music.clip)music.Play();
 }
 void Update(){
  if(music)music.volume=Mathf.MoveTowards(music.volume,GameDuoCinema.Busy?0:.38f,Time.unscaledDeltaTime*.95f);
 }
 public void StartGame(){if(!GameDuoCinema.Busy)GameDuoCinema.Play(new[]{0},GameDuoCampaign.Scenes[0]);}
 void OnGUI(){
  GUI.depth=100;
  GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.blackTexture);
  float scale=Mathf.Min(Screen.width/1672f,Screen.height/941f);
  var matrix=GUI.matrix;
  GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1672*scale)/2,(Screen.height-941*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
  if(background)GUI.DrawTexture(new Rect(0,0,1672,941),background);
  if(!controls.Reviewing&&!GameDuoCinema.Busy){
   if(GUI.Button(new Rect(306,800,270,85),GUIContent.none,GUIStyle.none))controls.OpenControls();
   if(GUI.Button(new Rect(624,788,423,108),GUIContent.none,GUIStyle.none))StartGame();
   var quit=new GUIStyle(GUI.skin.button){fontSize=27,alignment=TextAnchor.MiddleCenter};
   quit.normal.textColor=new Color(.55f,.9f,1);
   if(GUI.Button(new Rect(1122,832,246,53),"SALIR",quit))Quit();
  }
  GUI.matrix=matrix;
 }
 static void Quit(){
#if UNITY_EDITOR
  UnityEditor.EditorApplication.isPlaying=false;
#else
  Application.Quit();
#endif
 }
}
}
