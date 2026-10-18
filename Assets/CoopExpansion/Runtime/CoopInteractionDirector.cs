using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CoopBeam {
// Scene-local orchestration. No serialized scenes or prefab GUIDs are replaced.
public class CoopInteractionDirector:MonoBehaviour {
 GameManager g; LinkPowers powers;
 public static CoopInteractionDirector Active;
 public bool HasPending=>pending;
 public bool Dancing=>dancing||(GetComponent<CoopUltimateModes>()&&GetComponent<CoopUltimateModes>().Active);
 public bool TutorialOpen=>lesson!=null; public float DancePower=>success/12f;
 public void Explain(string id,string text){Teach(id,text);}
 bool pending,shared,dancing,opening,gateOpen;int carrier,gateCarrier=-1;LinkPickup.Kind cargo;
 float shareTime,rescueTime,gateTime,danceTime,beatTime,flash;int beat,success;bool answered;string outcome="";int promptPlayer,promptKey;
 readonly Queue<string> lessons=new Queue<string>();readonly HashSet<string> seen=new HashSet<string>();
 string lesson;float oldScale=1;Vector3[] original=new Vector3[2];
 SpriteRenderer[] auras=new SpriteRenderer[2];SpriteRenderer gate,left,right,key;int pads;float padTime;
 static readonly string[] letters={"W","A","S","D","ARRIBA","IZQUIERDA","ABAJO","DERECHA"};
 public static bool Locked(PlayerHealth h)=>Active&&((Active.dancing||Active.GetComponent<CoopUltimateModes>()&&Active.GetComponent<CoopUltimateModes>().FreezePlayers)||Active.gateCarrier>=0&&Active.g.players[Active.gateCarrier]==h&&Active.opening&&Active.gate&&Active.Near(Active.gateCarrier,Active.gate.transform.position,1.1f)&&Active.Hold(Active.gateCarrier));
 void Awake(){Active=this;g=GetComponent<GameManager>();powers=GetComponent<LinkPowers>();for(int i=0;i<2;i++){auras[i]=Mark("Aura cooperativa",g.players[i].transform.position, i==0?new Color(.8f,.35f,1):new Color(.35f,1,.4f),.9f);auras[i].enabled=false;}if(!GetComponent<CoopPresentation>())gameObject.AddComponent<CoopPresentation>();if(!GetComponent<CoopUltimateModes>())gameObject.AddComponent<CoopUltimateModes>(); }

 static Sprite auraArt;
 static Sprite AuraArt(){if(auraArt)return auraArt;var t=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};for(int y=0;y<32;y++)for(int x=0;x<32;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f));t.SetPixel(x,y,d>10&&d<14?Color.white:Color.clear);}t.Apply();return auraArt=Sprite.Create(t,new Rect(0,0,32,32),Vector2.one*.5f,32);}
 static Sprite glowArt;
 static Sprite Glow(){if(glowArt)return glowArt;var t=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};for(int y=0;y<32;y++)for(int x=0;x<32;x++){float a=Mathf.Floor(Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/16)*5)/5;t.SetPixel(x,y,new Color(1,1,1,a));}t.Apply();return glowArt=Sprite.Create(t,new Rect(0,0,32,32),Vector2.one*.5f,16);}
 void AddLights(){for(int i=0;i<5;i++){var o=new GameObject("Luz ambiental cooperativa");o.transform.SetParent(transform);o.transform.position=new Vector3(-8+i*4,-3,0);var l=o.AddComponent<UnityEngine.Rendering.Universal.Light2D>();l.lightType=UnityEngine.Rendering.Universal.Light2D.LightType.Point;l.pointLightOuterRadius=3.5f;l.pointLightInnerRadius=.4f;l.intensity=.45f;l.color=g.level&&g.level.fireKingdom?new Color(1,.35f,.08f):g.level&&g.level.iceKingdom?new Color(.3f,.65f,1):new Color(1,.4f,.7f);var glow=o.AddComponent<SpriteRenderer>();glow.sprite=Glow();glow.color=new Color(l.color.r,l.color.g,l.color.b,.2f);glow.sortingOrder=0;o.transform.localScale=Vector3.one*3;}}
 static Sprite ObjectiveArt(bool key){var t=new Texture2D(24,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};for(int y=0;y<32;y++)for(int x=0;x<24;x++){bool shape=key?((x-8)*(x-8)+(y-24)*(y-24)<23&&(x-8)*(x-8)+(y-24)*(y-24)>6)||(x>=7&&x<=9&&y>=5&&y<23)||(y>=5&&y<=7&&x>=7&&x<=15)||(y>=11&&y<=13&&x>=7&&x<=12):x>2&&x<21&&y<27;bool edge=!key&&(x<5||x>18||y<3||y>24);t.SetPixel(x,y,shape?(edge?new Color(.4f,.25f,.5f):Color.white):Color.clear);}t.Apply();return Sprite.Create(t,new Rect(0,0,24,32),Vector2.one*.5f,24);}
 SpriteRenderer Mark(string name,Vector3 pos,Color color,float size){var obj=new GameObject(name);obj.transform.SetParent(transform);obj.transform.position=pos;obj.transform.localScale=Vector3.one*size;var s=obj.AddComponent<SpriteRenderer>();s.sprite=name.Contains("Cerradura")?ObjectiveArt(false):name.Contains("Llave")?ObjectiveArt(true):AuraArt();s.color=color;s.sortingOrder=18;return s;}
 void Teach(string id,string text){if(seen.Add(id))lessons.Enqueue(text);}
 public IEnumerator Opening(){
  // Only the introductory arena of each kingdom adds the objective; later arenas preserve pacing.
  if(!g.level||g.level.number!=1)yield break;
  opening=true;left=Mark("Sello oeste",new Vector3(-5,-3.5f),Color.cyan,1.1f);right=Mark("Sello este",new Vector3(5,-3.5f),Color.cyan,1.1f);gate=Mark("Cerradura central",new Vector3(0,-3.4f),Color.yellow,1.3f);
  Teach("gate","SELLOS Y LLAVE\n\nCada jugador pisa un sello distinto. Mantengan ambos 1 segundo.\nLa llave aparece: uno la recoge y va al centro.\nEse jugador mantiene S (Mora) o ABAJO (Lima) para abrir.\nEl otro protege con el lazo. Soltar permite esquivar.\nEl progreso se conserva si deben rescatarse.");
  while(!gateOpen&&g.Running){if(pads<2){bool opposite=(Near(0,left.transform.position,1.15f)&&Near(1,right.transform.position,1.15f))||(Near(1,left.transform.position,1.15f)&&Near(0,right.transform.position,1.15f));padTime=opposite?padTime+Time.deltaTime:0;if(padTime>=1){pads=2;key=Mark("Llave compartida",left.transform.position+Vector3.up*.6f,Color.yellow,.7f);}}
   else if(gateCarrier<0){for(int i=0;i<2;i++)if(Near(i,key.transform.position,1.2f)){gateCarrier=i;Destroy(key.gameObject);break;}}
   else {int c=gateCarrier;if(g.players[c].Dead){gateCarrier=-1;key=Mark("Llave recuperable",gate.transform.position+Vector3.up,Color.yellow,.7f);}else TryChannel(c,Hold(c),Time.deltaTime);}
   if(gateCarrier>=0&&g.enemies.Count==0&&!gateOpen){ // A small, finite defense encounter, independent of campaign waves.
    if(!defenseSpawned){defenseSpawned=true;for(int i=0;i<2;i++){var e=Instantiate(g.enemyPrefabs[0],new Vector3(i==0?-8:8,-2.8f),Quaternion.identity);g.enemies.Add(e);}}}
   yield return null;
  }
  opening=false;gateCarrier=-1;if(left)Destroy(left.gameObject);if(right)Destroy(right.gameObject);if(gate)Destroy(gate.gameObject);
 }
 public bool TryChannel(int player,bool held,float dt){if(!opening||gateOpen||gateCarrier!=player||!held||!gate||!Near(player,gate.transform.position,1.1f))return false;gateTime+=Mathf.Clamp(dt,0,5);if(gateTime>=5)gateOpen=true;return true;}
 bool defenseSpawned;
 bool Hold(int i){var k=Keyboard.current;return k!=null&&(i==0?k.sKey.isPressed:k.downArrowKey.isPressed);}
 bool Near(int i,Vector3 pos,float radius)=>!g.players[i].Dead&&Vector2.Distance(g.players[i].transform.position,pos)<radius;
 public int Collector;
 public void Receive(LinkPickup.Kind kind){if(pending)return;pending=true;shared=false;shareTime=0;cargo=kind;carrier=Collector;if(g.players[carrier].Dead)carrier=1-carrier;
 Teach("power","PODERES EN PAREJA\n\n1. La aureola indica quien lleva el poder.\n2. Acerquense: junten las dos aureolas durante 0.6 segundos.\n3. Separense para encender el lazo mejorado.\nEl efecto empieza entonces y beneficia a ambos.\nPRISMA: mas dano. CHISPA: cadena a dos enemigos cercanos.\nANILLO: pulsos junto al lazo. BURBUJA: un golpe protegido.\nCORAZON: una vida para cada jugador vivo.");}
 void Update(){var k=Keyboard.current;if(lesson!=null){if(k!=null&&k.escapeKey.wasPressedThisFrame){lesson=null;Time.timeScale=oldScale;}return;}
 if(lessons.Count>0&&g.Running){lesson=lessons.Dequeue();oldScale=Time.timeScale;Time.timeScale=0;return;}if(!g.Running)return;
 for(int i=0;i<2;i++){auras[i].enabled=dancing||pending&&(shared||carrier==i)||g.players[i].Dead;auras[i].transform.position=g.players[i].transform.position;auras[i].transform.localScale=Vector3.one*(dancing?1.5f+success*.12f:1.1f+Mathf.Sin(Time.time*5)*.08f);}
 int dead=g.players[0].Dead?0:g.players[1].Dead?1:-1;
 if(dead>=0&&!dancing){Teach("rescue","RESCATA A TU COMPANERO\n\nSu cuerpo cae hasta suelo o una plataforma estable.\nAcercate y manten el contacto durante 1.5 segundos.\nLos cuerpos atraviesan rebotes y plataformas inestables.\nNo revive solo. Si ambos caen, termina la partida.");var h=g.players[dead];var down=h.GetComponent<DownedBody>();if((!down||down.Settled)&&Near(1-dead,h.transform.position,1.4f)){rescueTime+=Time.deltaTime;if(rescueTime>=1.5f){h.ReviveAt(g.players[1-dead].transform.position+Vector3.up*.4f);rescueTime=0;}}else rescueTime=0;}else rescueTime=0;
 if(pending&&!dancing&&!g.players[0].Dead&&!g.players[1].Dead){float distance=Vector2.Distance(g.players[0].transform.position,g.players[1].transform.position);if(!shared){shareTime=distance<1.5f?shareTime+Time.deltaTime:0;if(shareTime>=.6f)shared=true;}else if(distance>2.8f){pending=false;powers.ApplyShared(cargo);}}
 if(g.enemies.Exists(e=>e&&e.kind==EnemyKind.Armored&&!e.GetComponent<FireEnemyBrain>()&&!e.GetComponent<IceEnemyBrain>()))Teach("guard","CACAO ACORAZADO\n\nUno atrae su embestida. El otro se coloca a su espalda.\nCon un jugador a cada lado, el lazo atraviesa su defensa.\nSi ambos estan del mismo lado, su armadura reduce el dano.");
 if(!Dancing&&powers.Charge>=LinkPowers.ChargeRequired)Teach("dance","DANZA DEL ENLACE\n\nElijan 1 DANZA / 2 TIRACHINAS / 3 CONSTELACION.\nMora propone con Q. Lima confirma con SHIFT DERECHO en 2 s.\nAmbos levitan: el combate sigue, pero estan protegidos.\nSolo responde el jugador indicado: una tecla nueva por pulso.\nPulsar varias teclas o adelantarse desperdicia ese pulso.\nCada acierto fortalece a AMBOS. Los fallos no cancelan la ulti.\nAl final explota: los jefes conservan sus limites de fase.");
 if(dancing)Dance(k);flash=Mathf.Max(0,flash-Time.deltaTime);
 }
 public void BeginDance(){if(dancing)return;dancing=true;danceTime=0;beat=success=0;for(int i=0;i<2;i++){original[i]=g.players[i].transform.position;g.players[i].Protect(10);g.players[i].GetComponent<PlayerController2D>().ResetMotion();}NextBeat();}
 void NextBeat(){beatTime=0;answered=false;outcome="";promptPlayer=beat%2;promptKey=Random.Range(0,4);}
 void Dance(Keyboard k){danceTime+=Time.deltaTime;beatTime+=Time.deltaTime;for(int i=0;i<2;i++){g.players[i].transform.position=Vector3.Lerp(original[i],new Vector3(i==0?-3:3,1.5f+Mathf.Sin(Time.time*5+i)*.15f),Mathf.Clamp01(danceTime*2));if(g.players[i].GetComponent<PlayerController2D>().visual)g.players[i].GetComponent<PlayerController2D>().visual.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*8+i*3)*18);}
 if(!answered&&k!=null){bool[] pressed=promptPlayer==0?new[]{k.wKey.wasPressedThisFrame,k.aKey.wasPressedThisFrame,k.sKey.wasPressedThisFrame,k.dKey.wasPressedThisFrame}:new[]{k.upArrowKey.wasPressedThisFrame,k.leftArrowKey.wasPressedThisFrame,k.downArrowKey.wasPressedThisFrame,k.rightArrowKey.wasPressedThisFrame};int count=0;foreach(bool b in pressed)if(b)count++;if(count>0){SubmitDanceInput(promptPlayer,pressed[promptKey]?promptKey:-1,count);}}
 if(beatTime>=.65f){float remainder=beatTime-.65f;beat++;if(beat>=12){Explode();return;}NextBeat();beatTime=remainder;}}
 public bool SubmitDanceInput(int player,int keyCode,int count){if(!dancing||answered||player!=promptPlayer)return false;answered=true;bool ok=count==1&&keyCode==promptKey&&beatTime>=.12f&&beatTime<=.53f;outcome=ok?"BIEN!":"FALLO";if(ok){success++;g.Tone(600+success*65,.07f);}return ok;}
 void Explode(){dancing=false;flash=1;float damage=65+success*12;foreach(var e in g.enemies.ToArray()){if(!e||e.Dead)continue;bool boss=e.kind==EnemyKind.Boss||e.GetComponent<FireForgeBoss>()||e.GetComponent<IceKingBoss>();e.Damage(boss?Mathf.Min(damage,e.maxHealth*.24f):damage);FireFX.Burst(LinkPowers.TargetPoint(e),new Color(.35f,1,.4f),18);BeamFX.Burst(LinkPowers.TargetPoint(e),new Color(.85f,.4f,1),12);}foreach(var h in FindObjectsByType<EncounterHazard>())if(h.clearable)h.Clear();foreach(var p in FindObjectsByType<CandyProjectile>())Destroy(p.gameObject);for(int i=0;i<2;i++){g.players[i].transform.position=original[i];g.players[i].Protect(1.5f);FireFX.Burst(original[i],i==0?new Color(.8f,.3f,1):Color.green,28);}powers.FinishDance();powers.Message("DANZA / "+success+" ACIERTOS DE 12");g.Tone(130,.4f);}
 void OnGUI(){if(!g||!g.Running)return;GUI.depth=-20;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/640f,Screen.height/360f,1));
 if(dancing){GUI.color=new Color(.02f,.01f,.09f,.72f);GUI.DrawTexture(new Rect(0,0,640,360),Texture2D.whiteTexture);GUI.color=Color.white;for(int i=0;i<2;i++){var v=Camera.main.WorldToViewportPoint(g.players[i].transform.position);float size=48+success*3;GUI.color=i==0?new Color(.8f,.3f,1,.65f):new Color(.3f,1,.4f,.65f);CoopPresentation.DrawAura(v.x*640,(1-v.y)*360,size,GUI.color,Time.time);var sr=g.players[i].GetComponent<PlayerController2D>().sprite;if(sr&&sr.sprite){var s=sr.sprite;var rect=s.textureRect;GUI.color=Color.white;GUI.DrawTextureWithTexCoords(new Rect(v.x*640-13,(1-v.y)*360-17,26,34),s.texture,new Rect(rect.x/s.texture.width,rect.y/s.texture.height,rect.width/s.texture.width,rect.height/s.texture.height));}}GUI.color=Color.white;Box(180,90,280,85,"DANZA DEL ENLACE\n"+(promptPlayer==0?"MORA: ":"LIMA: ")+letters[promptKey+promptPlayer*4]+"\nPULSO "+(beat+1)+"/12   ACIERTOS "+success+"\n"+(answered?outcome:beatTime<.12f?"ESPERA...":"AHORA!"));}
 if(flash>0){GUI.color=new Color(.4f,1,.6f,flash*.65f);GUI.DrawTexture(new Rect(0,0,640,360),Texture2D.whiteTexture);GUI.color=Color.white;}
 if(!Dancing){string msg=opening?(pads<2?"UN JUGADOR EN CADA SELLO":gateCarrier<0?"RECOJAN LA LLAVE":(gateCarrier==0?"MORA: S":"LIMA: ABAJO")+" EN EL CENTRO / "+Mathf.RoundToInt(gateTime/5*100)+"%") :pending?(shared?"AHORA SEPARENSE": "JUNTEN SUS AURAS / "+LinkPickup.Label(cargo)):"";if(rescueTime>0)msg="RESCATANDO "+Mathf.RoundToInt(rescueTime/1.5f*100)+"%";if(msg!="")Box(130,312,380,36,msg);}
 if(lesson!=null){GUI.color=new Color(0,0,0,.85f);GUI.DrawTexture(new Rect(0,0,640,360),Texture2D.whiteTexture);GUI.color=Color.white;Box(55,55,530,250,lesson+"\n\nESC PARA CONTINUAR"); // Reference diagram: apart -> together -> apart.
 Diagram();}
 }
 void Diagram(){for(int stage=0;stage<3;stage++){float x=145+stage*125;GUI.color=new Color(.8f,.35f,1);GUI.DrawTexture(new Rect(x,263,14,14),AuraArt().texture);GUI.color=Color.green;GUI.DrawTexture(new Rect(x+(stage==1?16:50),263,14,14),AuraArt().texture);GUI.color=Color.white;var style=new GUIStyle(GUI.skin.label){fontSize=9,alignment=TextAnchor.MiddleCenter};GUI.Label(new Rect(x-15,282,100,18),lesson.StartsWith("SELLOS")?(stage==0?"DOS SELLOS":stage==1?"LLAVE": "DEFENDER"):lesson.StartsWith("RESCATA")?(stage==0?"COMPANERO CAIDO":stage==1?"ACERCARSE":"RESCATE"):lesson.StartsWith("DANZA")?(stage==0?"ACUERDO":stage==1?"DANZA":"EXPLOSION"):stage==0?"ACERCARSE":stage==1?"COMPARTIR":"SEPARARSE",style);}}
 void Box(float x,float y,float w,float h,string text){GUI.Box(new Rect(x,y,w,h),GUIContent.none);var style=new GUIStyle(GUI.skin.label){fontSize=11,alignment=TextAnchor.MiddleCenter,wordWrap=true};style.normal.textColor=Color.white;GUI.Label(new Rect(x+8,y+4,w-16,h-8),text,style);}
 void OnDestroy(){if(lesson!=null)Time.timeScale=oldScale;if(Active==this)Active=null;}
}
}

