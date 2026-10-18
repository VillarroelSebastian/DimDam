using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CoopBeam {
public sealed class GameDuoTutorial:MonoBehaviour {
 GameManager game;readonly bool[] ready=new bool[2];bool active;float oldScale;public int Step{get;private set;}
 public bool Confirmed(int player)=>ready[player];
 public static IEnumerator Practice(GameManager game){
  var guide=game.GetComponent<GameDuoTutorial>();if(!guide)guide=game.gameObject.AddComponent<GameDuoTutorial>();
  guide.game=game;guide.active=true;guide.oldScale=Time.timeScale;Time.timeScale=0;
  for(guide.Step=0;guide.Step<4&&game.Running;guide.Step++){
   guide.ready[0]=guide.ready[1]=false;
   while(!(guide.ready[0]&&guide.ready[1])&&game.Running)yield return null;
  }
  guide.active=false;Time.timeScale=guide.oldScale;Destroy(guide);
 }
 void Update(){if(GameDuoCinema.Busy)return;if(!active)return;var k=Keyboard.current;if(k==null)return;if(k.fKey.wasPressedThisFrame)ready[0]=true;if(k.enterKey.wasPressedThisFrame||k.numpadEnterKey.wasPressedThisFrame)ready[1]=true;}
 void OnGUI(){
  if(!active)return;GUI.depth=-60;var matrix=GUI.matrix;var color=GUI.color;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/640f,Screen.height/360f,1));
  GUI.color=new Color(.04f,.025f,.10f,.64f);GUI.DrawTexture(new Rect(0,0,640,360),Texture2D.whiteTexture);GUI.color=Color.white;
  CoopPromptUI.Panel(new Rect(72,76,496,232),CoopPromptUI.Mint);
  CoopPromptUI.Text(new Rect(90,86,460,25),new[]{"DOS PRINCIPES / UN SOLO EQUIPO","RESCATA A TU COMPANERO","COMBINACION DE AURAS","SUPER PODER EN EQUIPO"}[Step],18);
  CoopPromptUI.Text(new Rect(92,119,456,32),new[]{"Ponganse de acuerdo: uno a cada lado del enemigo.\nEl lazo entre ambos hace dano al atravesarlo.","Si uno cae, el otro debe acercarse a su cuerpo.\nMantengan el contacto 1.5 segundos para revivirlo.","Si uno tiene aura, acerquense durante 0.6 segundos.\nLuego separense para activar la combinacion del lazo.","La barra celeste se carga con el tiempo y haciendo dano.\nAl llenarse: Mimo Q y Pipro SHIFT DERECHO, sin apuro."}[Step],12);
  for(int i=0;i<2;i++){
   float x=92+i*238;var c=CoopPresentation.PlayerColor(i);
   CoopPromptUI.Text(new Rect(x,161,218,22),i==0?"MIMO":"PIPRO",14,c);
   if(Step==0)CoopPromptUI.Text(new Rect(x,188,218,37),i==0?"A / D mover  |  W saltar\nS bajar":"FLECHAS mover y saltar\nABAJO bajar",11);
   CoopPromptUI.Panel(new Rect(x,244,218,35),ready[i]?CoopPromptUI.Mint:c);
   CoopPromptUI.Text(new Rect(x+4,251,210,21),ready[i]?"CONFIRMADO":i==0?"CONFIRMAR  [ F ]":"CONFIRMAR  [ ENTER ]",13,ready[i]?CoopPromptUI.Mint:c);
  }
  if(Step>0){
   float separation=Step==2?Mathf.Lerp(18,90,(Mathf.Sin(Time.unscaledTime*1.8f)+1)*.5f):35;
   var left=new Rect(285-separation,185,32,44);var right=new Rect(323+separation,193,25,36);
   if(Step==1){left=new Rect(275,215,44,14);right=new Rect(330,190,25,36);CoopPromptUI.Ring(new Vector2(320,211),29,(Time.unscaledTime%1.5f)/1.5f,CoopPromptUI.Mint);}
   if(Step==2){CoopPresentation.DrawAura(left.center.x,left.center.y,29,CoopPresentation.PlayerColor(0),Time.unscaledTime);CoopPresentation.DrawAura(right.center.x,right.center.y,29,CoopPresentation.PlayerColor(1),Time.unscaledTime);}
   if(Step==3){CoopPromptUI.Key(new Rect(230,186,55,32),"Q",Color.cyan,true);CoopPromptUI.Key(new Rect(340,186,85,32),"SHIFT DER",Color.cyan,true);}
   else {CoopPromptUI.Hero(left,0);CoopPromptUI.Hero(right,1);}
   CoopPromptUI.Text(new Rect(105,224,430,17),Step==1?"ACERCARSE + PERMANECER = REVIVIR":Step==2?"ACERCARSE > COMPARTIR > SEPARARSE":"TECLAS POR TURNOS > EXPLOSION > LAZO MAS FUERTE",10,CoopPromptUI.Mint);
  }
  CoopPromptUI.Text(new Rect(92,284,456,16),"PASO "+(Step+1)+" / 4 - Ambos confirman para continuar.",10);
  GUI.color=color;GUI.matrix=matrix;
 }
 void OnDestroy(){if(active)Time.timeScale=oldScale;}
}
}
