using UnityEngine;
using UnityEngine.InputSystem;
namespace CoopBeam {
public class NeonLevel:MonoBehaviour {
 public const float CoreStepDuration=25; public const string ScenePath="Assets/ReinoNeon/Scenes/ReinoNeon.unity";
 public GameManager game; public NeonDevice[] crossed; public NeonDevice entryGate,relayGate,finalGate;
 public BubblegumHeavy battery; public Transform socketA,socketB,terminal,coreA,coreB,coreC,padA,padB,fragment,exit;
 public GameObject relayBridge,coreBridge;public SpriteRenderer reactor; public Sprite[] reactorFrames;
 public int Checkpoint{get;private set;} public int CoreStep{get;private set;} public bool SocketA{get;private set;}public bool Routed{get;private set;}public bool RelayDone{get;private set;} public bool Restored{get;private set;}public bool Collected{get;private set;}public bool Completed{get;private set;}
 public float Remaining=>Mathf.Max(0,deadline-Time.time); public int CrossedCount{get{int n=0;foreach(var d in crossed)if(d.powered)n++;return n;}}
 public float RoundRemaining{get;private set;}=240; bool roundStarted; float deadline,charge,rewardTime;string notice="Restablece la energia del nucleo trabajando en equipo";float noticeUntil=12;readonly float[] last={-1,-1};
 void Start(){game.BeginExploration();relayBridge.SetActive(false);coreBridge.SetActive(false);battery.maxX=18;}
 public bool Near(int p,Transform t,float r=1.35f)=>p>=0&&p<2&&t&&!game.players[p].Dead&&Vector2.Distance(game.players[p].transform.position,t.position)<r;
 bool Reach(int p,Transform t,float r=1.35f)=>Near(p,t,r)&&!Physics2D.Linecast(game.players[p].transform.position,t.position,1<<8).collider;
 void Say(string s){notice=s;noticeUntil=Time.time+6;}
 public bool Interact(int p){
 if(!roundStarted||p<0||p>1||!game.Running||Time.time-last[p]<.2f)return false;
 foreach(var d in crossed)if(d.player==p&&!d.powered&&Reach(p,d.transform)){d.Set(true);d.target.Set(true);last[p]=Time.time;Say(p==0?"Mimo abre el camino de Pipo":"Pipo abre el camino de Mimo");return true;}
 bool ok=false;
 if(p==1&&SocketA&&Reach(p,terminal)){Routed=!Routed;Say(Routed?"Circuito B listo. Mimo: bateria al segundo zocalo":"Circuito A. Puedes volver a cambiar la ruta");ok=true;}
 else if(RelayDone&&!Restored){
 if(p==0&&CoreStep==0&&Reach(p,coreA)){CoreStep=1;deadline=Time.time+CoreStepDuration;coreBridge.SetActive(true);Say("Pipo cruza y cambia la direccion. 25 segundos");ok=true;}
 else if(p==1&&CoreStep==1&&Reach(p,coreB)){CoreStep=2;deadline=Time.time+CoreStepDuration;Say("Ahora Pipo sostiene el puente. Mimo: panel derecho");ok=true;}
 else if(p==0&&CoreStep==2&&Reach(p,coreC)){CoreStep=3;deadline=Time.time+CoreStepDuration;Say("Intercambio listo. Cada uno a su placa de carga");ok=true;}
 }
 if(ok){last[p]=Time.time;BeamFX.Burst(game.players[p].transform.position,Color.cyan,10);game.Tone(660,.1f);}return ok;
 }
 void Update(){
 var k=Keyboard.current;if(k!=null){if(!roundStarted&&k.enterKey.wasPressedThisFrame)roundStarted=true;if(k.rKey.wasPressedThisFrame){ResetTogether();return;}if(k.mKey.wasPressedThisFrame){CandyCampaign.Map();return;}if(k.sKey.wasPressedThisFrame)Interact(0);if(k.downArrowKey.wasPressedThisFrame)Interact(1);}
 if(!roundStarted)return; if(!Completed){RoundRemaining=Mathf.Max(0,RoundRemaining-Time.deltaTime); if(RoundRemaining<=0){ResetTogether();Say("TIEMPO AGOTADO / NUEVO INTENTO DESDE EL CONTROL");return;}}
 foreach(var p in game.players)if(p.Dead||p.transform.position.y< -3){ResetTogether();return;}
 if(CrossedCount==6&&!entryGate.powered){entryGate.Set(true);Say("Circuitos cruzados completos. Reunanse en el relevo");}
 if(Checkpoint<1&&entryGate.powered&&BothBeyond(13)){Checkpoint=1;Say("Control 1 / Mimo empuja; Pipo toma el elevador");}
 SocketA=Mathf.Abs(battery.transform.position.x-socketA.position.x)<.32f;
 relayBridge.SetActive(SocketA||RelayDone);battery.maxX=Routed?22:18;
 if(!RelayDone&&Routed&&Mathf.Abs(battery.transform.position.x-socketB.position.x)<.32f){RelayDone=true;battery.Locked=true;relayGate.Set(true);Say("Relevo completo. El nucleo espera a los dos");}
 if(Checkpoint<2&&RelayDone&&BothBeyond(35)){Checkpoint=2;Say("Control 2 / Mimo inicia; Pipo invierte; Mimo confirma");}
 if(!Restored&&CoreStep>0){if(Remaining<=0){ResetCore();Say("Se corto la corriente. Intenten otra vez desde el control");}else if(CoreStep==3){charge=Near(0,padA,1)&&Near(1,padB,1)?Mathf.Min(2.5f,charge+Time.deltaTime):Mathf.Max(0,charge-Time.deltaTime*.5f);if(charge>=2.5f){Restored=true;Checkpoint=3;finalGate.Set(true);rewardTime=Time.time;Say("Nucleo restaurado. El corazon los espera juntos");game.Tone(990,.3f);}}}
 reactor.sprite=reactorFrames[Restored?Mathf.Min(3,1+(int)((Time.time-rewardTime)*1.5f)):CoreStep>0?1:0];
 if(Restored&&Time.time-rewardTime<3&&Time.frameCount%8==0)BeamFX.Burst(reactor.transform.position,Color.cyan,3);
 if(Restored&&!Collected&&Near(0,fragment,1.8f)&&Near(1,fragment,1.8f)){Collected=true;fragment.gameObject.SetActive(false);PlayerPrefs.SetInt("ReinoNeon.FriendshipFragment",1);PlayerPrefs.Save();BeamFX.Burst(fragment.position,Color.magenta,30);Say("FRAGMENTO DEL CORAZON RECUPERADO / JUNTOS BRILLAMOS");}
 if(Collected&&!Completed&&Near(0,exit,1.5f)&&Near(1,exit,1.5f)){Completed=true;Say("CIRCUITOS CRUZADOS COMPLETADOS / M PARA VOLVER AL MAPA");game.Tone(1320,.4f);}
 }
 bool BothBeyond(float x)=>game.players[0].transform.position.x>x&&game.players[1].transform.position.x>x;
 void PlaceTogether(Vector2 pos){for(int i=0;i<2;i++)game.players[i].ReviveAt(pos+Vector2.right*i*(Checkpoint==0?-.9f:1.5f));game.BeginExploration();Physics2D.SyncTransforms();}
 void ResetCore(){CoreStep=0;charge=0;deadline=0;coreBridge.SetActive(false);PlaceTogether(new Vector2(37,1));}
 public void ResetTogether(){
 RoundRemaining=Checkpoint==0?240:Checkpoint==1?180:120;
 if(Checkpoint==0){foreach(var d in crossed){d.Set(false);d.target.Set(false);}entryGate.Set(false);}
 if(Checkpoint<2){battery.Restore();Routed=RelayDone=false;SocketA=false;relayGate.Set(false);battery.maxX=18;}
 if(!Restored){CoreStep=0;charge=0;coreBridge.SetActive(false);}else coreBridge.SetActive(true);
 PlaceTogether(Checkpoint==0?new Vector2(-8,1):Checkpoint==1?new Vector2(14.5f,1):Checkpoint==2?new Vector2(37,1):new Vector2(54,1));last[0]=last[1]=-1;Say("Equipo recuperado. Se conserva el ultimo control");
 }
 void OnGUI(){GUI.depth=-10;PixelFont.Begin();PixelFont.Panel(new Rect(2,2,636,38),PixelFont.Ink,Color.cyan);PixelFont.Text(12,8,"THE NEON CORE / CIRCUITOS CRUZADOS",1,PixelFont.Cream);string objective=Completed?"JUNTOS BRILLAMOS / M VOLVER AL MAPA":Collected?"REUNANSE EN EL PORTAL":Restored?"RECOJAN JUNTOS EL FRAGMENTO":RelayDone?"NUCLEO / MIMO > PIPO > MIMO > DOS PLACAS":entryGate.powered?"RELEVO / BATERIA A > TERMINAL PIPO > BATERIA B":"MI ACCION ABRE TU CAMINO / CIRCUITOS "+CrossedCount+"/6";PixelFont.Text(12,25,objective,1,Color.cyan);
 PixelFont.Text(320,76,"TIEMPO "+Mathf.CeilToInt(RoundRemaining)+" S",2,RoundRemaining<30?Color.magenta:Color.cyan,true);if(!roundStarted){PixelFont.Panel(new Rect(80,125,480,100),PixelFont.Ink,Color.cyan);PixelFont.Text(320,140,"CIRCUITOS EN EQUIPO / 240 SEGUNDOS",1,PixelFont.Cream,true);PixelFont.Text(320,162,"COORDINEN SUS RUTAS. CADA CONTROL SE CONSERVA.\nEL NUCLEO DA 25 S POR PASO.",1,Color.cyan,true);if(PixelFont.Button(new Rect(180,190,280,28),"ENTER / EMPEZAR",true))roundStarted=true;}
 PixelFont.Panel(new Rect(2,328,636,30),PixelFont.Ink,Color.magenta);PixelFont.Text(10,334,"MIMO A D / W SALTO / S USAR | PIPO FLECHAS / ABAJO USAR",1,PixelFont.Cream);PixelFont.Text(10,347,"R RECUPERAR / M MAPA / CONTROL "+Checkpoint+"/3",1,Color.cyan);
 if(Time.time<noticeUntil){PixelFont.Panel(new Rect(8,300,624,22),PixelFont.Ink,Color.cyan);PixelFont.Text(14,307,notice.ToUpperInvariant(),1,PixelFont.Cream);}
 if(CoreStep>0&&!Restored){PixelFont.Text(320,49,"ESTABILIDAD "+Mathf.CeilToInt(Remaining)+" / CARGA "+Mathf.RoundToInt(charge/2.5f*100)+"%",1,Color.cyan,true);PixelFont.Fill(new Rect(220,62,200*charge/2.5f,4),Color.cyan);}
 if(Completed&&PixelFont.Button(new Rect(180,80,280,28),"VOLVER AL MAPA",true))CandyCampaign.Map();
 }
}
}
