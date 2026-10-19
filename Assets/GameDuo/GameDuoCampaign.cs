using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoopBeam {
// Only GameDuo scene copies participate; the original standalone worlds keep their exits.
public sealed class GameDuoCampaign : MonoBehaviour {
 public static readonly string[] Scenes = {
  "Assets/GameDuo/Scenes/01_Tutorial_Chicle.unity",
  "Assets/GameDuo/Scenes/02_ReinoNeon.unity",
  "Assets/GameDuo/Scenes/03_ReyHielo.unity",
  "Assets/GameDuo/Scenes/04_RescatePinguinos.unity",
  "Assets/GameDuo/Scenes/05_JefeChicle.unity"
 };
 public static readonly string[] Titles = { "La ceremonia interrumpida", "Los desafios del Reino Neon", "El enfrentamiento contra el Rey Hielo", "Los pinguinos del Rey Hielo", "La batalla final" };
 public static GameDuoCampaign Instance { get; private set; }
 public static bool Active => Instance != null;
 public static bool InCampaignScene => SceneManager.GetActiveScene().path.StartsWith("Assets/GameDuo/Scenes/",System.StringComparison.Ordinal);
 public int Stage { get; private set; }
 public bool Completed { get; private set; }
 GameManager game; NeonLevel neon; BubblegumLevel mechanisms; IcePenguinRound penguins;
 bool menu, finishing; float previousScale=1, finishAt;
 const string SaveKey="GameDuo.Unlocked";

 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 static void Install() { SceneManager.sceneLoaded-=OnSceneLoaded; SceneManager.sceneLoaded+=OnSceneLoaded; }
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void InitialScene(){OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
 static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
  int stage=System.Array.IndexOf(Scenes,scene.path);
  if(stage<0)return;
  if(Instance&&Instance.gameObject.scene==scene)return;
  var host=new GameObject("GameDuo / Progresion narrativa");
  var campaign=host.AddComponent<GameDuoCampaign>();
  campaign.Stage=stage;
  campaign.Configure();
 }
 void Awake(){Instance=this;}
 void Configure(){
  CandyCampaign.ArrivalPending=false;
  game=FindAnyObjectByType<GameManager>();neon=FindAnyObjectByType<NeonLevel>();
  mechanisms=FindAnyObjectByType<BubblegumLevel>();penguins=FindAnyObjectByType<IcePenguinRound>();
  foreach(var exit in FindObjectsByType<CandyLevelExit>())exit.enabled=false;
  if(game&&game.level){
   // Runtime copy: title changes must never alter the original level asset.
   game.level=Instantiate(game.level);game.level.title=Titles[Stage];
   if(Stage==4){game.level.lesson="Rompan las defensas de caramelo y enfrenten al rey.\nLos destellos avisan los ataques. Las torretas fijan la punteria antes de disparar.\nDurante el combate, destruyan los refuerzos y crucen el nucleo cuando brille.";
    game.level.waves=new[]{
     new CandyWave{enemyTypes=new[]{0,4,5},spawnPositions=new[]{new Vector2(-7,-3.5f),new Vector2(3,.5f),new Vector2(8,-3.5f)}},
     new CandyWave{enemyTypes=new[]{1,2,5},spawnPositions=new[]{new Vector2(-4,1),new Vector2(5,-3.5f),new Vector2(-8,-3.5f)}},
     new CandyWave{enemyTypes=new[]{6,0,1,4},spawnPositions=new[]{new Vector2(0,-2.9f),new Vector2(-8,-3.4f),new Vector2(6,2),new Vector2(-4,3)}}};
   }
   if(Stage==2){var wave=game.level.waves[2];wave.enemyTypes=new[]{6,0,2,4};wave.spawnPositions=new[]{new Vector2(0,-2.65f),new Vector2(-7,-3.5f),new Vector2(7,-3.5f),new Vector2(-5,2.5f)};}
   if(Stage==0){game.level.waves=new[]{
    new CandyWave{enemyTypes=new[]{0,0,2,1,4,4},spawnPositions=new[]{new Vector2(-8,-3.5f),new Vector2(8,-3.5f),new Vector2(-5,-3.5f),new Vector2(5,-3.5f),new Vector2(-6,2.4f),new Vector2(6,2.4f)}},
    new CandyWave{enemyTypes=new[]{0,2,2,1,1,0,4,4},spawnPositions=new[]{new Vector2(-8,-3.5f),new Vector2(8,-3.5f),new Vector2(-5,-3.5f),new Vector2(5,-3.5f),new Vector2(-2,-3.5f),new Vector2(2,-3.5f),new Vector2(-6,2.8f),new Vector2(6,2.8f)}},
    new CandyWave{enemyTypes=new[]{0,2,2,1,1,0,2,4,4,4},spawnPositions=new[]{new Vector2(-9,-3.5f),new Vector2(9,-3.5f),new Vector2(-6,-3.5f),new Vector2(6,-3.5f),new Vector2(-3,-3.5f),new Vector2(3,-3.5f),new Vector2(0,-3.5f),new Vector2(-7,2.8f),new Vector2(0,3.4f),new Vector2(7,2.8f)}}};game.level.lesson="MIMO Y PIPRO / DEFIENDAN EL SALON\nEl rey tiembla de miedo junto a su trono.\nEl lazo entre ambos hace dano.\nPractiquen y protejan al rey de las tres oleadas.";hostFactory();}
  }
 }
 void Start(){if(Stage==1)GameDuoNeonPortal.Install(neon);if(Stage==0&&game&&!game.Started)game.Begin();if(Stage==0&&!GameDuoCinema.IntroPlayed)GameDuoCinema.Play(new[]{0},null);}
 void hostFactory(){if(!FindAnyObjectByType<TutorialFactory>())gameObject.AddComponent<TutorialFactory>();}
 void Update(){
  if(menu||Completed||GameDuoCinema.Busy)return;
  bool won=game&&game.Victory&&!game.Practice || neon&&neon.Completed || mechanisms&&mechanisms.Completed || penguins&&penguins.Finished;
  if(won&&!finishing){finishing=true;finishAt=Time.time+2;PlayerPrefs.SetInt(SaveKey,Mathf.Max(PlayerPrefs.GetInt(SaveKey,0),Mathf.Min(Stage+1,Scenes.Length-1)));PlayerPrefs.Save();}
  if(finishing&&Time.time>=finishAt)Advance();
 }
 public static bool TryAdvance(){if(!Active)return false;if(Instance.finishing&&!GameDuoCinema.Busy)Instance.Advance();return true;}
 public static bool TryMenu(){if(!Active)return false;if(!GameDuoCinema.Busy)Instance.OpenMenu();return true;}
 void Advance(){
  if(GameDuoCinema.Busy)return;
  int[][] movies={new[]{1,2},new[]{3},new[]{4},new[]{5,6},new[]{7,8}};
  if(Stage==Scenes.Length-1){GameDuoCinema.Play(movies[Stage],null,()=>{Completed=true;finishing=false;});return;}
  GameDuoCinema.Play(movies[Stage],Scenes[Stage+1]);
 }
 void Load(int stage){
  Time.timeScale=1;menu=false;CandyCampaign.ArrivalPending=false;
  GameDuoCinema.Play(stage==0?new[]{0}:new int[0],Scenes[stage]);
 }
 void OpenMenu(){if(menu)return;menu=true;previousScale=Time.timeScale;Time.timeScale=0;}
 void Resume(){menu=false;Time.timeScale=previousScale;}
 void OnGUI(){
  if(GameDuoCinema.Busy||!menu&&!Completed&&!finishing)return;
  GUI.depth=-100;var matrix=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/640f,Screen.height/360f,1));
  if(menu||Completed){
   GUI.Box(new Rect(85,65,470,240),GUIContent.none);
   var style=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,wordWrap=true,fontSize=18};
   GUI.Label(new Rect(100,80,440,60),Completed?"Mimo y Pipro recuperaron su hogar":"GAME DUO / "+(Stage+1)+" DE "+Scenes.Length+"\n"+Titles[Stage],style);
   if(!Completed&&GUI.Button(new Rect(130,145,380,30),"Continuar jugando"))Resume();
   if(GUI.Button(new Rect(130,183,380,30),"Reintentar este capitulo"))Load(Stage);
   if(GUI.Button(new Rect(130,221,380,30),"Empezar desde el tutorial"))Load(0);
   int unlocked=Mathf.Clamp(PlayerPrefs.GetInt(SaveKey,0),0,Scenes.Length-1);
   if(unlocked>Stage&&!Completed&&GUI.Button(new Rect(130,259,380,30),"Continuar desde el ultimo capitulo desbloqueado"))Load(unlocked);
  } else {
   GUI.Box(new Rect(90,265,460,45),"Capitulo completado\n"+(Stage<Scenes.Length-1?"Siguiente: "+Titles[Stage+1]:"El Reino Chicle vuelve a estar a salvo"));
  }
  GUI.matrix=matrix;
 }
 void OnDestroy(){if(menu)Time.timeScale=previousScale;if(game&&game.level)Destroy(game.level);if(Instance==this)Instance=null;}
}
}



