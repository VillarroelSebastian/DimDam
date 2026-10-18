using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace CoopBeam {
// Planar steering keeps herding readable with the existing side-facing character art.
public class IcePenguinRound : MonoBehaviour {
 public const string ScenePath="Assets/Minigames/IceRescue/Scenes/IceRescue.unity";
 public Sprite arenaArt;
 public Sprite[] penguinFrames, mimoFrames, pipoFrames;
 public Sprite[] mimoDirections,pipoDirections;
 public const int FlockSize=16;
 public const float Duration=100;
 public float Remaining {get;private set;}=Duration;
 public int Rescued {get;private set;}
 public bool Started {get;private set;}
 public bool Finished=>Rescued==FlockSize||Remaining<=0;
 class Actor {public Vector2 p,v;public SpriteRenderer art,shadow; public bool saved;public float phase;}
 Actor[] birds=new Actor[FlockSize],heroes=new Actor[2]; Sprite tile;Texture2D texture;bool paused;float intro=3,clock;
 Actor Make(string name,Sprite sprite,Vector2 p,float size){var g=new GameObject(name);g.transform.SetParent(transform);var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;g.transform.position=p;g.transform.localScale=Vector3.one*size;return new Actor{p=p,art=r,phase=Random.value*6.28f};}
 void Rect(string name,Vector2 p,Vector2 size,Color color,int order){var a=Make(name,tile,p,1);a.art.transform.localScale=new Vector3(size.x,size.y,1);a.art.color=color;a.art.sortingOrder=order;}
 void Start(){texture=new Texture2D(1,1);texture.SetPixel(0,0,Color.white);texture.Apply();tile=Sprite.Create(texture,new Rect(0,0,1,1),Vector2.one*.5f,1);if(arenaArt){var background=Make("Pista ilustrada / refugio polar",arenaArt,Vector2.zero,1);background.art.sortingOrder=-100;background.art.transform.localScale=new Vector3(20/arenaArt.bounds.size.x,12/arenaArt.bounds.size.y,1);}else{Rect("Pista helada",Vector2.zero,new Vector2(19,10),new Color(.16f,.48f,.66f),-100);for(int i=0;i<28;i++)Rect("Brillo del hielo",new Vector2(Random.Range(-8.5f,8.5f),Random.Range(-4f,4f)),new Vector2(Random.Range(.2f,.9f),.025f),new Color(.5f,.9f,1,.3f),-99);
 Rect("Muro norte",new Vector2(0,4.6f),new Vector2(19,.35f),Color.cyan,-90);for(int s=-1;s<=1;s+=2){Rect("Muro lateral",new Vector2(s*9,0),new Vector2(.35f,9.5f),Color.cyan,-90);Rect("Borde de salida",new Vector2(s*5.55f,-4.6f),new Vector2(7,.35f),Color.cyan,-90);}Rect("Refugio / salida inferior",new Vector2(0,-4.65f),new Vector2(4,1),new Color(.3f,1,.72f),-95);
 }ResetRound();}
 public void ResetRound(){foreach(var a in birds)if(a!=null&&a.art)Destroy(a.art.gameObject);foreach(var a in heroes)if(a!=null&&a.art)Destroy(a.art.gameObject);Rescued=0;Remaining=Duration;Started=false;paused=false;intro=3;clock=0;
 for(int i=0;i<FlockSize;i++)birds[i]=Make("Pinguino "+i,penguinFrames[0],new Vector2(-7+(i%4)*4.5f,-1.8f+(i/4)*1.6f),.75f);
 heroes[0]=Make("Mimo",mimoDirections!=null&&mimoDirections.Length>0?mimoDirections[0]:mimoFrames[0],new Vector2(-3,3),1.2f);heroes[1]=Make("Pipo",pipoDirections!=null&&pipoDirections.Length>0?pipoDirections[0]:pipoFrames[0],new Vector2(3,3),.95f);foreach(var a in birds)AddShadow(a);foreach(var a in heroes)AddShadow(a);}
 void AddShadow(Actor a){var g=new GameObject("Sombra de contacto");g.transform.SetParent(a.art.transform,false);a.shadow=g.AddComponent<SpriteRenderer>();a.shadow.sprite=tile;a.shadow.color=new Color(.025f,.12f,.22f,.25f);g.transform.localPosition=new Vector3(0,-.12f,.01f);g.transform.localScale=new Vector3(.65f,.12f,1);a.shadow.sortingOrder=-80;}
 void Update(){var k=Keyboard.current;if(k!=null){if(k.mKey.wasPressedThisFrame){CandyCampaign.Map();return;}if(k.rKey.wasPressedThisFrame){ResetRound();return;}if(k.escapeKey.wasPressedThisFrame&&Started)paused=!paused;if(!Started&&k.enterKey.wasPressedThisFrame)Started=true;}
 if(!Started||paused||Finished)return;float dt=Time.deltaTime;if(intro>0){intro-=dt;return;}clock+=dt;Remaining=Mathf.Max(0,Remaining-dt);
 for(int i=0;i<2;i++){Vector2 input=k==null?Vector2.zero:i==0?new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0)):new Vector2((k.rightArrowKey.isPressed?1:0)-(k.leftArrowKey.isPressed?1:0),(k.upArrowKey.isPressed?1:0)-(k.downArrowKey.isPressed?1:0));var a=heroes[i];a.v=Vector2.MoveTowards(a.v,input.normalized*4.8f,dt*22);a.p+=a.v*dt;a.p=new Vector2(Mathf.Clamp(a.p.x,-8.3f,8.3f),Mathf.Clamp(a.p.y,-3.45f,3.9f));Animate(a,i==0?mimoFrames:pipoFrames,i==0?1.2f:.95f,dt);}
 for(int i=0;i<birds.Length;i++){var a=birds[i];if(a.saved)continue;Vector2 away=Vector2.zero;foreach(var h in heroes){Vector2 d=a.p-h.p;float distance=d.magnitude;if(distance<2.7f)away+=d.normalized*(1-distance/2.7f);}
 Vector2 desired=away.sqrMagnitude>.001f?away.normalized*2.65f:new Vector2(Mathf.Cos(clock*.7f+a.phase),Mathf.Sin(clock*.9f+a.phase))*.5f;
 for(int j=0;j<birds.Length;j++){if(i==j||birds[j].saved)continue;Vector2 d=a.p-birds[j].p;if(d.sqrMagnitude<.3f)desired+=d.normalized*.8f;}
 // Slide along walls instead of becoming trapped by a corner.
 if(Mathf.Abs(a.p.x)>8.15f&&desired.x*Mathf.Sign(a.p.x)>0){desired.x=-Mathf.Sign(a.p.x)*.6f;desired.y+=a.p.y>0?-1:1;}
 if(a.p.y>3.7f&&desired.y>0){desired.y=-.5f;desired.x+=a.p.x>0?-1:1;}
 a.v=Vector2.MoveTowards(a.v,desired,dt*7);a.p+=a.v*dt;
 if(a.p.y< -3.4f&&Mathf.Abs(a.p.x)<1.2f){a.saved=true;Rescued++;a.art.gameObject.SetActive(false);continue;}
 a.p=new Vector2(Mathf.Clamp(a.p.x,-8.4f,8.4f),Mathf.Clamp(a.p.y,-3.45f,3.9f));Animate(a,penguinFrames,.75f,dt);}
 }
 void Animate(Actor a,Sprite[] frames,float size,float dt){int f=(int)(clock*(a.v.sqrMagnitude>1?9:3)+a.phase)%Mathf.Min(4,frames.Length);a.art.sprite=frames[f];a.art.flipX=a.v.x<0;var directions=a==heroes[0]?mimoDirections:a==heroes[1]?pipoDirections:null;if(directions!=null&&directions.Length>=4&&(Mathf.Abs(a.v.y)>Mathf.Abs(a.v.x)||a.v.sqrMagnitude<.05f)){a.art.sprite=directions[a.v.y>.1f?1:0];a.art.flipX=false;}a.art.sortingOrder=20-Mathf.RoundToInt(a.p.y*10);a.art.transform.position=a.p+Vector2.up*(Mathf.Abs(Mathf.Sin(clock*12+a.phase))*.06f*Mathf.Clamp01(a.v.magnitude));a.art.transform.localScale=new Vector3(size*(1+Mathf.Sin(clock*12+a.phase)*.035f),size*(1-Mathf.Sin(clock*12+a.phase)*.035f),1);a.art.transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(clock*10+a.phase)*Mathf.Min(a.v.magnitude,2)*3);}
 void OnDestroy(){if(tile)Destroy(tile);if(texture)Destroy(texture);}
 void OnGUI(){PixelFont.Begin();PixelFont.Panel(new Rect(4,4,632,42),PixelFont.Ink,Color.cyan);PixelFont.Text(14,12,"HIELO / RESCATE DE PINGUINOS",1,PixelFont.Cream);PixelFont.Text(14,29,"A SALVO "+Rescued+" / "+FlockSize,1,Color.cyan);PixelFont.Text(540,12,Mathf.CeilToInt(Remaining)+" S",2,Remaining<20?PixelFont.Pink:Color.cyan,true);PixelFont.Fill(new Rect(360,34,260*Remaining/Duration,4),Remaining<20?PixelFont.Pink:Color.cyan);
 PixelFont.Panel(new Rect(4,326,632,30),PixelFont.Ink,Color.cyan);PixelFont.Text(14,333,"MIMO WASD / PIPO FLECHAS / ESC PAUSA / R REINTENTO / M MAPA",1,PixelFont.Cream);PixelFont.Text(320,305,"V V V  REFUGIO  V V V",1,PixelFont.Mint,true);
 if(!Started||paused||Finished||intro>0){PixelFont.Panel(new Rect(70,108,500,140),PixelFont.Ink,Color.cyan);string title=!Started?"GUIEN A TODOS AL REFUGIO":paused?"PAUSA":Finished?(Rescued==FlockSize?"RESCATE COMPLETADO!":"SE ACABO EL TIEMPO"):(Mathf.CeilToInt(intro)).ToString();PixelFont.Text(320,124,title,2,PixelFont.Cream,true);PixelFont.Text(320,155,"SE ALEJAN DE USTEDES. RODEENLOS POR ARRIBA.\nUNO REUNE EL GRUPO Y EL OTRO CUBRE LOS LADOS.\nLLEVEN LOS 16 A LA PUERTA INFERIOR EN 100 S.",1,Color.cyan,true);if(!Started&&PixelFont.Button(new Rect(170,203,300,28),"ENTER / EMPEZAR",true))Started=true;if(Finished&&PixelFont.Button(new Rect(170,203,300,28),"R / INTENTAR OTRA VEZ",true))ResetRound();}}
}
}
