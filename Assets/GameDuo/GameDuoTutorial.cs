using System.Collections;
using UnityEngine;
namespace CoopBeam {
// Instruction pages belong only to the first campaign arena.
public sealed class GameDuoTutorial:MonoBehaviour {
 readonly CoopPromptUI.StartGate gate=new CoopPromptUI.StartGate();
 Sprite[] waspFrames;
 bool review; public bool Reviewing => review; public bool AwaitingConfirmation => active&&!review;
 public void OpenControls(){review=true;active=true;Step=0;}
 bool active;float oldScale;public int Step{get;private set;}
 public bool Confirmed(int player)=>gate.Confirmed(player);
 public static IEnumerator Practice(GameManager game){
  var guide=game.GetComponent<GameDuoTutorial>();if(!guide)guide=game.gameObject.AddComponent<GameDuoTutorial>();
  guide.active=true;guide.oldScale=GameDuoCinema.Busy?1:Time.timeScale;Time.timeScale=0;
  for(guide.Step=0;guide.Step<4&&game.Running;guide.Step++){
   guide.gate.Reset();while(game.Running&&(GameDuoCinema.Busy||!guide.gate.Poll()))yield return null;
   // Do not reuse the key press that confirmed the previous page.
   yield return null;
  }
  guide.active=false;Time.timeScale=guide.oldScale;Destroy(guide);
 }
 void OnGUI(){if(!active||GameDuoCinema.Busy)return;GUI.depth=-60;var matrix=GUI.matrix;var color=GUI.color;
  GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/640f,Screen.height/360f,1));
  CoopPromptUI.Panel(new Rect(40,18,560,330),CoopPromptUI.Mint);
  string[] titles={"MOVERSE Y COMBATIR","REVIVIR AL COMPANERO","COMPARTIR LAS AURAS","ULTI EN EQUIPO"};
  string[] instructions={
   "MIMO: A / D mover, W saltar, S bajar, SHIFT IZQ dash.\nPIPRO: flechas mover y saltar, ABAJO bajar, CTRL DER dash.\nRodeen al enemigo: el lazo entre ambos le hace dano.",
   "Sin corazones, el personaje queda tumbado en el suelo.\nEl otro se acerca y mantiene el contacto 1.5 s para revivirlo.\nSi ambos caen, termina la partida.",
   "Cuando uno tenga un aura, juntense durante 0.6 segundos.\nDespues separense para activar el poder compartido.\nEl beneficio se aplica a los dos.",
   "La barra se carga con el tiempo y al hacer dano.\nMimo activa con Q y Pipro con SHIFT DERECHO.\nRespondan por turnos con WASD y flechas: al final explota."
  };
  int page=Mathf.Clamp(Step,0,3);
  CoopPromptUI.Text(new Rect(60,26,520,28),titles[page],18,CoopPromptUI.Mint);
  CoopPromptUI.Text(new Rect(58,61,524,65),instructions[page],12);
  Demonstrate(page);
  if(review){
   if(GUI.Button(new Rect(65,285,140,30),"Anterior"))Step=(Step+3)%4;
   if(GUI.Button(new Rect(250,285,140,30),"Volver al menu")){review=false;active=false;}
   if(GUI.Button(new Rect(435,285,140,30),"Siguiente"))Step=(Step+1)%4;
  }else gate.Draw(282,false);
  CoopPromptUI.Text(new Rect(90,332,460,14),"PASO "+(page+1)+" / 4",10);
  GUI.color=color;GUI.matrix=matrix;
 }
 void Hero(float x,float floor,int player,bool down=false){
  var c=GUI.color;GUI.color=down?new Color(.75f,.65f,.75f):Color.white;
  CoopPromptUI.Hero(new Rect(x-18,floor-(down?9:player==0?46:32),down?42:36,down?9:player==0?46:32),player);GUI.color=c;
 }
 void Wasp(Rect r,float alpha=1){
  if(waspFrames==null)waspFrames=GameDuoCandyKing.LoadFrames("Wasp",1.25f,false);
  if(waspFrames==null)return;var sprite=waspFrames[(int)(Time.unscaledTime*10)%4];var uv=sprite.textureRect;
  var c=GUI.color;GUI.color=new Color(1,1,1,alpha);GUI.DrawTextureWithTexCoords(r,sprite.texture,new Rect(uv.x/sprite.texture.width,uv.y/sprite.texture.height,uv.width/sprite.texture.width,uv.height/sprite.texture.height));GUI.color=c;
 }
 void Aura(float x,float y,int player,float clock){CoopPresentation.DrawAura(x,y,48,CoopPresentation.PlayerColor(player),clock);}
 void Demonstrate(int page){
  float clock=Time.unscaledTime,cycle=Mathf.Repeat(clock/2.8f,1),bounce=Mathf.Sin(cycle*Mathf.PI);
  string[][] captions={
   new[]{"MOVER Y SALTAR","UNO A CADA LADO","ESQUIVAR EL AVISO"},
   new[]{"SIN CORAZONES / CAIDO","ACERCARSE / 1.5 S","VUELVEN A COMBATIR"},
   new[]{"ESTE BRILLO ES EL AURA","JUNTOS / 0.6 S","SEPARARSE / PODER"},
   new[]{"BARRA LLENA / Q + SHIFT","WASD Y FLECHAS","EXPLOSION EN EL MAPA"}
  };
  for(int i=0;i<3;i++){
   float x=58+i*177,center=x+85,floor=228;
   CoopPromptUI.Panel(new Rect(x,133,170,135),new Color(.3f,.27f,.45f));
   CoopPromptUI.Text(new Rect(x+4,137,162,17),(i+1)+" / EJEMPLO",10,CoopPromptUI.Mint);
   CoopPromptUI.Fill(new Rect(x+9,230,152,2),new Color(.35f,.32f,.48f));
   if(page==0){
    if(i==0){Hero(center-24+bounce*16,floor-bounce*22,0);Hero(center+25-bounce*9,floor-bounce*14,1);CoopPromptUI.Key(new Rect(x+10,158,24,22),"W",CoopPresentation.PlayerColor(0),true);CoopPromptUI.Key(new Rect(x+133,158,24,22),"",CoopPresentation.PlayerColor(1),true,0);}
    else if(i==1){Hero(center-52,floor,0);Hero(center+52,floor,1);Wasp(new Rect(center-17,185,34,30));CoopPromptUI.Line(new Vector2(center-48,204),new Vector2(center+48,210),3,CoopPromptUI.Mint);}
    else{CoopPromptUI.Fill(new Rect(center-20,216,45,9),new Color(1,.7f,.2f,.6f));Hero(center-48+bounce*50,floor-bounce*34,0);Wasp(new Rect(center+28,166,33,27));}
   }else if(page==1){
    if(i==0){Hero(center,floor,0,true);CoopPromptUI.Text(new Rect(x+12,165,146,27),"0 CORAZONES",12,new Color(1,.4f,.45f));}
    else if(i==1){Hero(center-12,floor,0,true);Hero(center+45-cycle*35,floor,1);CoopPromptUI.Ring(new Vector2(center-8,212),25,cycle,CoopPromptUI.Mint,3);CoopPromptUI.Text(new Rect(x+22,157,126,17),"REVIVIENDO",10,CoopPromptUI.Mint);}
    else{Hero(center-30,floor,0);Hero(center+31,floor,1);CoopPromptUI.Line(new Vector2(center-29,207),new Vector2(center+29,213),3,CoopPromptUI.Mint);CoopPromptUI.Text(new Rect(x+24,157,122,17),"CORAZONES LLENOS",10,CoopPromptUI.Mint);}
   }else if(page==2){
    float separation=i==0?40:i==1?14:30+cycle*23;
    Aura(center-separation,205,0,clock);if(i>0)Aura(center+separation,207,1,clock);
    Hero(center-separation,floor,0);Hero(center+separation,floor,1);
    if(i==1)CoopPromptUI.Ring(new Vector2(center,207),37,cycle,CoopPromptUI.Mint,3);
    if(i==2)CoopPromptUI.Line(new Vector2(center-separation,205),new Vector2(center+separation,212),5,CoopPromptUI.Mint);
   }else{
    Aura(center-30,205,0,clock);Aura(center+30,207,1,clock);Hero(center-30,floor-bounce*5,0);Hero(center+30,floor-bounce*5,1);
    if(i==0){CoopPromptUI.Fill(new Rect(x+17,159,136,7),Color.cyan);CoopPromptUI.Key(new Rect(x+9,180,24,23),"Q",CoopPresentation.PlayerColor(0),true);CoopPromptUI.Key(new Rect(x+129,180,34,23),"SHIFT",CoopPresentation.PlayerColor(1),true);}
    else if(i==1){bool turn=cycle<.5f;CoopPromptUI.Key(new Rect(x+29,158,26,24),"W",CoopPresentation.PlayerColor(0),turn);CoopPromptUI.Key(new Rect(x+115,158,26,24),"",CoopPresentation.PlayerColor(1),!turn,0);}
    else{Wasp(new Rect(x+7,159,30,26),1-cycle);Wasp(new Rect(x+131,159,30,26),1-cycle);CoopPromptUI.Ring(new Vector2(center,203),cycle*73,1,new Color(1,1,1,1-cycle),5);}
   }
   CoopPromptUI.Text(new Rect(x+3,242,164,23),captions[page][i],10,CoopPromptUI.Mint);
  }
 }
 void OnDestroy(){if(active&&!review)Time.timeScale=oldScale;if(waspFrames!=null)foreach(var frame in waspFrames)if(frame)Destroy(frame);}
}
}
