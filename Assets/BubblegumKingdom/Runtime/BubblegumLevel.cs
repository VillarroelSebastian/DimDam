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
 public GameObject[] bubbles,ascentA,ascentB;
 public string nextScene="ReinoNeon";
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
 Vector3 gateAHome,gateBHome;float bubbleTime;readonly float[] interacted={-1,-1};
 string notice="";float noticeUntil;
 void Start(){if(!GetComponent<GameDuoMechanisms>())gameObject.AddComponent<GameDuoMechanisms>();game.BeginExploration();gateAHome=gateA.position;gateBHome=gateB.position;SetGroup(bubbles,false);SetGroup(ascentA,false);SetGroup(ascentB,false);}
 static void SetGroup(GameObject[] group,bool on){if(group!=null)foreach(var b in group)if(b)b.SetActive(on);}
 public bool Near(int i,Transform t,float radius=1.4f)=>i>=0&&i<game.players.Length&&t&&!game.players[i].Dead&&Vector2.Distance(game.players[i].transform.position,t.position)<radius;
 bool Reach(int i,Transform target,float radius=1.4f){
 if(!Near(i,target,radius))return false;
 return !Physics2D.Linecast(game.players[i].transform.position,target.position,1<<8).collider;
 }
 public bool Interact(int player){
 if(player<0||player>1||!game.Running||Completed||Time.time-interacted[player]<.2f)return false;
 bool ok=false;
 if(player==1&&!AuxiliaryOn&&Reach(1,auxiliary,1)){AuxiliaryOn=true;Lamp(auxiliary,true);Say("Auxiliar listo. Mimo: palanca y mentita.");ok=true;}
 else if(!DoorAOpen&&Reach(player,leverA)){LeverOn=!LeverOn;Lamp(leverA,LeverOn);ok=true;}
 else if(DoorAOpen&&!DoorBOpen&&Reach(player,leverB)){RouteUp=!RouteUp;Lamp(leverB,RouteUp);Say(RouteUp?"Desvio verde. Bloque sobre la marca dorada.":"Desvio de retorno: la mentita volvera.");ok=true;}
 else if(player==1&&DoorBOpen&&!BubblesOn&&Reach(1,bubbleSwitch,1)){
 if(!OnMark(step,stepMark)){Say("Mimo debe colocar el escalon en la marca.");return false;}
 BubblesOn=true;step.Locked=true;bubbleTime=Time.time;Lamp(bubbleSwitch,true);Celebrate(bubbleSwitch.position);Say("Burbujas listas. Suban juntos al corazon.");ok=true;
 }
 if(ok){interacted[player]=Time.time;game.Tone(LeverOn||RouteUp?660:440,.08f);}return ok;
 }
 static bool OnMark(BubblegumHeavy obj,Transform mark)=>Mathf.Abs(obj.transform.position.x-mark.position.x)<.45f&&Mathf.Abs(obj.transform.position.y-mark.position.y)<.25f;
 bool BothAbove(float y)=>game.players[0].transform.position.y>y&&game.players[1].transform.position.y>y&&game.players[0].GetComponent<PlayerController2D>().Grounded&&game.players[1].GetComponent<PlayerController2D>().Grounded;
 void Update(){
 var k=Keyboard.current;
 if(k!=null){if(k.rKey.wasPressedThisFrame){ResetTogether();return;}if(k.mKey.wasPressedThisFrame){CandyCampaign.Map();return;}if(k.sKey.wasPressedThisFrame)Interact(0);if(k.downArrowKey.wasPressedThisFrame)Interact(1);}
 if(game.players[0].transform.position.y< -4||game.players[1].transform.position.y< -4||game.players[0].Dead||game.players[1].Dead){ResetTogether();return;}
 entryRock.maxX=AuxiliaryOn&&LeverOn?3:0;
 if(!DoorAOpen&&AuxiliaryOn&&LeverOn&&entryRock.transform.position.x>=2.75f){DoorAOpen=true;entryRock.Locked=true;SetGroup(ascentA,true);Celebrate(entryRock.transform.position);Say("Piso 1 abierto. Suban por las burbujas de la derecha.");}
 MoveGate(gateA,gateAHome,DoorAOpen);
 if(DoorAOpen&&Checkpoint<1&&BothAbove(10)){Checkpoint=1;Say("Control 1: taller de la mentita fugitiva.");}
 if(DoorAOpen&&!DoorBOpen&&!Rolling&&runaway.transform.position.x>=-2.2f){Rolling=true;runaway.Locked=true;StartCoroutine(Roll());}
 MoveGate(gateB,gateBHome,DoorBOpen);
 if(DoorBOpen&&Checkpoint<2&&BothAbove(20)){Checkpoint=2;Say("Control 2: jardin de burbujas.");}
 if(BubblesOn)for(int i=0;i<bubbles.Length;i++)if(Time.time-bubbleTime>i*.35f)bubbles[i].SetActive(true);
 if(BubblesOn&&Checkpoint<3&&BothAbove(28)){Checkpoint=3;Say("Control 3: camara del corazon.");}
 if(!Collected&&BubblesOn&&Near(0,fragment,2)&&Near(1,fragment,2)){Collected=true;PlayerPrefs.SetInt("BubblegumKingdom.FirstFriendshipFragment",1);PlayerPrefs.Save();StartCoroutine(Reward());}
 if(Collected&&!Completed&&Near(0,exit,1.5f)&&Near(1,exit,1.5f)){Completed=true;if(GameDuoCampaign.Active)return;if(nextScene=="Level_TheNeonCore")nextScene="ReinoNeon";if(Application.CanStreamedLevelBeLoaded(nextScene))SceneManager.LoadScene(nextScene);else Say("Fragmento recuperado. The Neon Core: proxima aventura.",60);}
 }
 void MoveGate(Transform gate,Vector3 home,bool open){var c=gate.GetComponent<Collider2D>();if(c)c.enabled=!open;gate.position=Vector3.MoveTowards(gate.position,home+Vector3.up*(open?3.5f:0),Time.deltaTime*3);Lamp(gate,open);}
 static void Lamp(Transform t,bool on){var s=t.GetComponent<SpriteRenderer>();if(s)s.color=on?new Color(.58f,.92f,.77f):new Color(.91f,.47f,.69f);}
 // Fixed-step swept motion pauses when a player is in the path, then returns safely.
 IEnumerator Travel(Vector2 target,float speed){float blocked=0;
 while(Vector2.Distance(runaway.transform.position,target)>.04f){
 yield return new WaitForFixedUpdate();
 if(runaway.TryMove(Vector2.MoveTowards(runaway.transform.position,target,speed*Time.fixedDeltaTime)))blocked=0;
 else {blocked+=Time.fixedDeltaTime;if(blocked>.35f)Say("Apartense de la mentita para dejarla pasar.",1);if(blocked>2){routeAborted=true;yield break;}}
 }}
 bool routeAborted;
 IEnumerator Roll(){
 routeAborted=false;Say("Mentita en marcha: Pipro cambia el desvio; Mimo coloca el bloque.");
 yield return Travel(new Vector2(0,11.2f),2);
 bool success=!routeAborted&&RouteUp&&OnMark(routeBlock,routeMark);
 // The candy rolls on a separate upper rail, above the guide block and players.
 if(success){yield return Travel(new Vector2(0,13.2f),2.8f);if(!routeAborted)yield return Travel(new Vector2(7,13.2f),2.8f);if(!routeAborted)yield return Travel(new Vector2(7,11.2f),2.8f);
 Physics2D.SyncTransforms();success=!routeAborted&&plate.GetComponent<Collider2D>().bounds.Intersects(runaway.GetComponent<Collider2D>().bounds);
 }
 if(success){DoorBOpen=true;routeBlock.Locked=true;Lamp(plate,true);SetGroup(ascentB,true);Celebrate(plate.position);Say("Placa activada. Siguiente piso: jardin de burbujas.");}
 else{FailedRoutes++;Say("Ruta incompleta o bloqueada. La mentita vuelve al inicio.");yield return new WaitForSeconds(.5f);
 foreach(var p in game.players)if(Vector2.Distance(p.transform.position,runaway.Home)<2.5f)p.ReviveAt(new Vector2(-8+p.GetComponent<PlayerController2D>().playerIndex*1.5f,11));
 runaway.Restore();}
 Rolling=false;
 }
 IEnumerator Reward(){Celebrate(fragment.position);game.Tone(990,.3f);var art=fragment.GetChild(0);Vector3 initial=art.localScale;
 for(float t=0;t<1;t+=Time.deltaTime){art.localScale=initial*(1+Mathf.Sin(t*Mathf.PI)*.5f);yield return null;}
 art.localScale=initial;fragment.gameObject.SetActive(false);Lamp(exit,true);Say("Corazon recuperado. Reunanse en la salida.",12);
 }
 void Celebrate(Vector3 p){BeamFX.Burst(p,new Color(1,.6f,.9f),24);BeamFX.Burst(p,Color.cyan,16);game.Tone(740,.2f);}
 void Say(string s,float seconds=5){notice=s;noticeUntil=Time.time+seconds;}
 public void ResetTogether(){
 StopAllCoroutines();Rolling=false;Completed=false;
 if(Checkpoint==0){AuxiliaryOn=LeverOn=RouteUp=DoorAOpen=DoorBOpen=false;entryRock.Restore();routeBlock.Restore();SetGroup(ascentA,false);SetGroup(ascentB,false);gateA.position=gateAHome;gateB.position=gateBHome;Lamp(auxiliary,false);Lamp(leverA,false);Lamp(leverB,false);Lamp(plate,false);}
 if(Checkpoint<2){runaway.Restore();if(Checkpoint==1){DoorBOpen=false;routeBlock.Restore();RouteUp=false;Lamp(leverB,false);Lamp(plate,false);SetGroup(ascentB,false);}}
 if(Checkpoint<3){BubblesOn=false;step.Restore();SetGroup(bubbles,false);Lamp(bubbleSwitch,false);}
 // A reset during the reward coroutine must not leave a visible collected fragment.
 fragment.gameObject.SetActive(!Collected);
 Vector2 spawn=Checkpoint==0?new Vector2(-8,1):Checkpoint==1?new Vector2(-8,11):Checkpoint==2?new Vector2(-8,21):new Vector2(-4,29);
 for(int i=0;i<2;i++)game.players[i].ReviveAt(spawn+Vector2.right*(i*1.5f));
 interacted[0]=interacted[1]=-1;game.BeginExploration();Physics2D.SyncTransforms();Say("Equipo a salvo. Se conserva el ultimo piso alcanzado.");
 }
 void OnGUI(){
 GUI.depth=0;PixelFont.Begin();PixelFont.Panel(new Rect(2,2,636,36),PixelFont.Ink,PixelFont.Pink);
 PixelFont.Text(12,8,"CAMINO DEL CASTILLO / ENGRANAJES",1,PixelFont.Cream);
 string objective=Collected?"CORAZON RECUPERADO / REUNANSE EN LA SALIDA":!DoorAOpen?"A / AUXILIAR + PALANCA + EMPUJAR MENTITA":!DoorBOpen?"B / DESVIO VERDE + BLOQUE EN LA MARCA":!BubblesOn?"C / ESCALON EN MARCA + INTERRUPTOR DE PIPRO":"D / SUBAN JUNTOS PARA RECUPERAR EL CORAZON";
 PixelFont.Text(12,24,objective,1,PixelFont.Mint);
 if(!GameDuoCampaign.InCampaignScene){PixelFont.Panel(new Rect(2,326,636,32),PixelFont.Ink,PixelFont.Pink);
 PixelFont.Text(12,332,"MIMO A/D W SALTO S USAR | PIPRO FLECHAS ABAJO USAR",1,PixelFont.Cream);
 PixelFont.Text(12,346,"R RECUPERAR EQUIPO / M MAPA / CONTROL "+Checkpoint+"/3",1,PixelFont.Mint);}
 if(Time.time<noticeUntil){PixelFont.Panel(new Rect(20,298,600,22),PixelFont.Ink,PixelFont.Mint);PixelFont.Text(30,306,notice.ToUpperInvariant(),1,PixelFont.Cream);}
 for(int i=0;i<2;i++){Transform nearest=null;float distance=1.45f;
 foreach(var t in new[]{auxiliary,leverA,leverB,bubbleSwitch}){float d=Vector2.Distance(game.players[i].transform.position,t.position);if(d<distance&&Reach(i,t,distance)){distance=d;nearest=t;}}
 if(nearest)PixelFont.Text(i==0?12:465,46,(i==0?"S":"ABAJO")+" / USAR MECANISMO",1,PixelFont.Mint);
 }
 }
}}
