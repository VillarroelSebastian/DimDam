using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace CoopBeam {
public class BubblegumLevel : MonoBehaviour {
 public const string ScenePath="Assets/BubblegumKingdom/0_0_Scenes/Level_BubblegumKingdom.unity";
 public GameManager game;
 public BubblegumHeavy entryRock,runaway,routeBlock,step;
 public Transform auxiliary,leverA,leverB,bubbleSwitch,stepMark,routeMark,plate,fragment,exit;
 public Transform gateA,gateB;
 public GameObject[] bubbles;
 public string nextScene="Level_TheNeonCore";
 public bool AuxiliaryOn {get;private set;}
 public bool LeverOn {get;private set;}
 public bool RouteUp {get;private set;}
 public bool DoorAOpen {get;private set;}
 public bool DoorBOpen {get;private set;}
 public bool BubblesOn {get;private set;}
 public bool Collected {get;private set;}
 public bool Completed {get;private set;}
 public bool Rolling {get;private set;}
 public int Checkpoint {get;private set;}
 public int FailedRoutes {get;private set;}
 Vector3 gateAHome,gateBHome,plateHome;Coroutine roll;float started,bubbleTime;
 string notice="";float noticeUntil;
 void Start(){game.BeginExploration();gateAHome=gateA.position;gateBHome=gateB.position;plateHome=plate.position;started=Time.time;foreach(var b in bubbles)b.SetActive(false);}
 public bool Near(int i,Transform t,float radius=1.4f)=>!game.players[i].Dead&&Vector2.Distance(game.players[i].transform.position,t.position)<radius;
 public bool Interact(int player){
 if(player<0||player>1)return false;
 if(player==1&&Near(1,auxiliary,1)){AuxiliaryOn=true;Lamp(auxiliary,true);Say("Mecanismo auxiliar conectado. Mimo: palanca y roca.");return true;}
 if(Near(player,leverA)){LeverOn=!LeverOn;Lamp(leverA,LeverOn);return true;}
 if(Near(player,leverB)){RouteUp=!RouteUp;Lamp(leverB,RouteUp);Say(RouteUp?"Desvio hacia la placa. Coloca el bloque en la marca.":"Desvio hacia el barranco: la roca regresara.");return true;}
 if(player==1&&DoorBOpen&&Near(1,bubbleSwitch,1)){
 if(Mathf.Abs(step.transform.position.x-stepMark.position.x)>.45f){Say("Mimo debe colocar el escalon en la marca dorada.");return false;}
 BubblesOn=true;step.Locked=true;bubbleTime=Time.time;Lamp(bubbleSwitch,true);Say("Burbujas activadas: aparecen y permanecen. Suban juntos.");return true;}
 return false;
 }
 void Update(){
 var k=Keyboard.current;
 if(k!=null){if(k.rKey.wasPressedThisFrame)ResetTogether();if(k.mKey.wasPressedThisFrame){CandyCampaign.Map();return;}if(k.sKey.wasPressedThisFrame)Interact(0);if(k.downArrowKey.wasPressedThisFrame)Interact(1);}
 if(game.players[0].transform.position.y< -4||game.players[1].transform.position.y< -4||game.players[0].Dead||game.players[1].Dead)ResetTogether();
 // Before release, the rail ends at the closed sluice. Both independent controls are required.
 entryRock.maxX=AuxiliaryOn&&LeverOn?17:11.3f;
 if(!DoorAOpen&&AuxiliaryOn&&LeverOn&&entryRock.transform.position.x>=16.7f){DoorAOpen=true;entryRock.Locked=true;Celebrate(entryRock.transform.position);Say("Puerta desbloqueada. Siguiente: la roca fugitiva.");}
 MoveGate(gateA,gateAHome,DoorAOpen||AuxiliaryOn&&LeverOn);
 if(DoorAOpen&&!DoorBOpen&&!Rolling&&runaway.transform.position.x>=27.8f){Rolling=true;runaway.Locked=true;roll=StartCoroutine(Roll());}
 MoveGate(gateB,gateBHome,DoorBOpen);
 if(DoorBOpen){plate.position=Vector3.Lerp(plate.position,plateHome+Vector3.down*.18f,Time.deltaTime*5);if(Checkpoint<1&&BothPast(43)){Checkpoint=1;Say("Punto de control: jardin de burbujas.");}}
 if(BubblesOn)for(int i=0;i<bubbles.Length;i++)if(Time.time-bubbleTime>i*.45f)bubbles[i].SetActive(true);
 if(Checkpoint<2&&BubblesOn&&BothPast(64)&&game.players[0].transform.position.y>6&&game.players[1].transform.position.y>6){Checkpoint=2;Say("Punto de control: camara del tesoro.");}
 if(!Collected&&BubblesOn&&Near(0,fragment,2.2f)&&Near(1,fragment,2.2f)){Collected=true;PlayerPrefs.SetInt("BubblegumKingdom.FirstFriendshipFragment",1);PlayerPrefs.Save();StartCoroutine(Reward());}
 if(Collected&&!Completed&&Near(0,exit,1.5f)&&Near(1,exit,1.5f)){Completed=true;if(Application.CanStreamedLevelBeLoaded(nextScene))SceneManager.LoadScene(nextScene);else Say("Primer fragmento recuperado. The Neon Core: proxima aventura.",60);}
 }
 bool BothPast(float x)=>game.players[0].transform.position.x>x&&game.players[1].transform.position.x>x;
 void MoveGate(Transform gate,Vector3 home,bool open){gate.position=Vector3.MoveTowards(gate.position,home+Vector3.up*(open?12:0),Time.deltaTime*4);Lamp(gate,open);}
 static void Lamp(Transform t,bool on){var s=t.GetComponent<SpriteRenderer>();if(s)s.color=on?new Color(.5f,1,.7f):new Color(1,.55f,.65f);}
 IEnumerator Travel(Vector2 target,float speed){while(Vector2.Distance(runaway.transform.position,target)>.03f){runaway.Place(Vector2.MoveTowards(runaway.transform.position,target,speed*Time.deltaTime));runaway.transform.Rotate(0,0,-160*Time.deltaTime);yield return null;}}
 IEnumerator Roll(){
 Say("¡Roca en marcha! Pipo: desvio. Mimo: bloque sobre la marca.");
 yield return Travel(new Vector2(31,1.2f),2.1f);
 // Evaluate at the fork, giving both players time to adjust the route.
 bool success=RouteUp&&Mathf.Abs(routeBlock.transform.position.x-routeMark.position.x)<.5f;
 if(success){yield return Travel(new Vector2(35,1.2f),2.5f);yield return Travel(new Vector2(39,1.2f),3);
 Physics2D.SyncTransforms();DoorBOpen=plate.GetComponent<Collider2D>().bounds.Intersects(runaway.GetComponent<Collider2D>().bounds);Lamp(plate,DoorBOpen);routeBlock.Locked=true;Celebrate(plate.position);Say("¡Placa activada! El castillo esta abierto.");
 yield return Travel(new Vector2(39,1.65f),3);yield return Travel(new Vector2(39,1.2f),3);
 }else{yield return Travel(new Vector2(34,-3),3);FailedRoutes++;Say("La roca vuelve a salvo. Desvio verde + bloque en la marca.");yield return new WaitForSeconds(.6f);runaway.Restore();}
 Rolling=false;roll=null;
 }
 IEnumerator Reward(){Celebrate(fragment.position);game.Tone(990,.3f);Vector3 initial=fragment.localScale;
 for(float t=0;t<1;t+=Time.deltaTime){fragment.localScale=initial*(1+Mathf.Sin(t*Mathf.PI)*.6f);yield return null;}
 fragment.gameObject.SetActive(false);Lamp(exit,true);Say("Primer fragmento recuperado. Reunanse en la salida.",12);
 }
 void Celebrate(Vector3 p){BeamFX.Burst(p,new Color(1,.6f,.9f),30);BeamFX.Burst(p,Color.cyan,20);game.Tone(740,.25f);}
 void Say(string s,float seconds=5){notice=s;noticeUntil=Time.time+seconds;}
 public void ResetTogether(){
 StopAllCoroutines();roll=null;Rolling=false;Completed=false;
 if(Checkpoint==0){AuxiliaryOn=LeverOn=RouteUp=DoorAOpen=DoorBOpen=false;entryRock.Restore();runaway.Restore();routeBlock.Restore();gateA.position=gateAHome;gateB.position=gateBHome;plate.position=plateHome;Lamp(auxiliary,false);Lamp(leverA,false);Lamp(leverB,false);}
 if(Checkpoint<2){BubblesOn=false;step.Restore();foreach(var b in bubbles)b.SetActive(false);Lamp(bubbleSwitch,false);}
 if(Collected)fragment.gameObject.SetActive(false);
 Vector2 spawn=Checkpoint==0?new Vector2(1,1):Checkpoint==1?new Vector2(44,1):new Vector2(65,7.8f);
 for(int i=0;i<2;i++)game.players[i].ReviveAt(spawn+Vector2.right*(i*1.5f));
 game.BeginExploration();Physics2D.SyncTransforms();Say("Ambos a salvo. R recupera al equipo y los objetos.");
 }
 void OnGUI(){
 GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/960f,Screen.height/540f,1));
 var style=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true};style.normal.textColor=new Color(1,.96f,.85f);
 GUI.Box(new Rect(12,12,936,77),GUIContent.none);
 GUI.Label(new Rect(26,18,915,28),"THE BUBBLEGUM KINGDOM  /  LA ROCA FUGITIVA",style);
 string objective=Collected?"Primer fragmento recuperado · Salida cooperativa → The Neon Core":!DoorAOpen?"A · Pipo conecta el auxiliar. Palanca + Mimo empuja la roca.":!DoorBOpen?"B · Mimo libera la roca y coloca el bloque. Pipo cambia el desvio.":!BubblesOn?"C · Mimo coloca el escalon; Pipo activa las burbujas en el tunel.":"D · Suban juntos y encuentren el primer fragmento del Corazon de la Amistad.";
 GUI.Label(new Rect(26,48,915,40),objective,style);
 GUI.Box(new Rect(12,472,936,57),GUIContent.none);style.fontSize=14;
 GUI.Label(new Rect(25,477,915,25),"Mimo / Coral: A D + W saltar · S palanca    |    Pipo / Celeste: ← → + ↑ saltar · ↓ mecanismo",style);
 GUI.Label(new Rect(25,502,915,24),"Empuja caminando contra objetos · R recuperar equipo · M mapa · Punto de control "+Checkpoint+"/2",style);
 if(Time.time<noticeUntil){GUI.Box(new Rect(160,401,640,58),GUIContent.none);GUI.Label(new Rect(173,408,615,49),notice,style);}
 if(Time.time-started<6){GUI.Box(new Rect(230,110,500,65),GUIContent.none);GUI.Label(new Rect(245,118,470,55),"Encuentra el primer fragmento del Corazon de la Amistad.\nLos dos pueden moverse desde el primer instante.",style);}
 }
}}


