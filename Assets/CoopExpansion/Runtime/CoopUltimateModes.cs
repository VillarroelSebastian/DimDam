using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CoopBeam {
public class CoopUltimateModes:MonoBehaviour {
 public enum Mode{Danza,Tirachinas,Constelacion}
 public Mode Selected {get;private set;}
 public bool Active {get;private set;}
 public bool FreezePlayers=>Active&&Selected==Mode.Tirachinas;
 public float Power=>Selected==Mode.Tirachinas?tension:pairs/3f;
 public int CompletedPairs=>pairs;
 public float Tension=>tension;
 public int Releases {get;private set;}
 GameManager g;CoopInteractionDirector director;LinkPowers powers;
 float elapsed,tension,angle,release0=-10,release1=-10,pairTime;int pairs,facing=1;
 readonly List<Vector3> trace=new List<Vector3>();
 Vector3[] original=new Vector3[2];SpriteRenderer[] stars=new SpriteRenderer[2];LineRenderer constellation;
 public Vector2 Target(int player){Vector2 rest;Collider2D support;float[] spots={5,2,7};float x=(player==0?-1:1)*spots[Mathf.Clamp(pairs,0,2)];if(DownedBody.TryFindRest(new Vector2(x,g.players[player].spawn.y+1),out rest,out support))return rest+Vector2.up*.15f;return g.players[player].spawn;}
 void Awake(){g=GetComponent<GameManager>();director=GetComponent<CoopInteractionDirector>();powers=GetComponent<LinkPowers>();}
 public bool Select(Mode mode){if(Active||director.Dancing||mode<Mode.Danza||mode>Mode.Constelacion)return false;if(Selected!=mode)powers.ResetConfirmations();Selected=mode;return true;}
 void Update(){if(!g.Running){if(Active)Cancel();return;}if(director.TutorialOpen||Time.timeScale<=0)return;var k=Keyboard.current;
  if(g.Practice&&!Active&&!director.Dancing)powers.AddCharge(LinkPowers.ChargeRequired);

  if(!Active)return;if(g.players[0].Dead||g.players[1].Dead){Cancel();return;}elapsed+=Time.deltaTime;
  if(Selected==Mode.Tirachinas){TickSling(Time.deltaTime,k!=null&&k.aKey.isPressed,k==null?0:(k.upArrowKey.isPressed?1:0)-(k.downArrowKey.isPressed?1:0),k!=null&&k.leftArrowKey.isPressed,k!=null&&k.rightArrowKey.isPressed,k!=null&&k.dKey.wasPressedThisFrame,k!=null&&k.rightCtrlKey.wasPressedThisFrame);
   if(!Active)return;
   for(int i=0;i<2;i++)g.players[i].transform.position=Vector3.Lerp(original[i],new Vector3(i==0?-3-tension:3+tension,1.2f+Mathf.Sin(Time.time*4+i)*.1f,0),Mathf.Clamp01(elapsed*2));
   if(Active&&elapsed>=6.5f)ReleaseSling();
  }else{bool together=Vector2.Distance(g.players[0].transform.position,Target(0))<1.05f&&Vector2.Distance(g.players[1].transform.position,Target(1))<1.05f;pairTime=together?pairTime+Time.deltaTime:0;if(pairTime>=.35f){trace.Add(Target(0));trace.Add(Target(1));pairs++;pairTime=0;for(int i=0;i<2;i++)FireFX.Burst(g.players[i].transform.position,CoopPresentation.PlayerColor(i),12);if(pairs>=3){ReleaseConstellation();return;}UpdateStars();}if(elapsed>=8)ReleaseConstellation();}
 }
 public void BeginSelected(){if(Active||director.Dancing)return;if(Selected==Mode.Danza){director.BeginDance();return;}Active=true;trace.Clear();elapsed=tension=pairTime=0;pairs=0;angle=0;facing=1;release0=release1=-10;for(int i=0;i<2;i++){original[i]=g.players[i].transform.position;g.players[i].Protect(11);if(FreezePlayers)g.players[i].GetComponent<PlayerController2D>().ResetMotion();}
  if(Selected==Mode.Tirachinas)director.Explain("sling","TIRACHINAS COOPERATIVO\n\nMimo mantiene A para tensar el lazo.\nPipro apunta con ARRIBA / ABAJO y elige lado con IZQ / DER.\nMimo pulsa D y Pipro CTRL DERECHO dentro de 0.6 s para soltar.\nLa onda rebota hasta tres veces; cada enemigo recibe un impacto.\nTienen 6.5 s. Si no sueltan, sale una onda de potencia alcanzada.");
  else {CreateStars();director.Explain("stars","CONSTELACION EN PAREJA\n\nCada jugador alcanza la estrella de su color.\nMantengan ambas 0.35 s: aparece el siguiente par.\nHay tres pares y 8 s para dibujar la constelacion.\nPueden moverse con sus controles normales y estan protegidos.\nCada par refuerza la explosion final; siempre hay un remate.");}
 }
 public void TickSling(float dt,bool stretch,float aim,bool left,bool right,bool mora,bool lima){if(!Active||Selected!=Mode.Tirachinas)return;if(stretch)tension=Mathf.Clamp01(tension+Mathf.Max(0,dt)/2.2f);angle=Mathf.Clamp(angle+aim*65*dt,-20,60);if(left)facing=-1;if(right)facing=1;if(mora)release0=Time.time;if(lima)release1=Time.time;if(elapsed>=1&&Time.time-release0<=.6f&&Time.time-release1<=.6f)ReleaseSling();}
 void ReleaseSling(){Vector2 origin=(original[0]+original[1])*.5f+Vector3.up*.2f;var direction=new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad)*facing,Mathf.Sin(angle*Mathf.Deg2Rad));CoopRicochet.Create(g,origin,direction,100+110*tension);Releases++;Finish(true,"TIRACHINAS / POTENCIA "+Mathf.RoundToInt(tension*100)+"%");}
 void ReleaseConstellation(){float damage=80+pairs*60;foreach(var e in g.enemies.ToArray())if(e&&!e.Dead){ApplyUltimateDamage(e,damage);FireFX.Burst(LinkPowers.TargetPoint(e),CoopPresentation.PlayerColor(pairs%2),16);}ClearShots();Releases++;Finish(false,"CONSTELACION / "+pairs+" DE 3 PARES");}
 public static void ApplyUltimateDamage(EnemyBase e,float damage){if(!e||e.Dead)return;bool boss=e.kind==EnemyKind.Boss||e.GetComponent<FireForgeBoss>()||e.GetComponent<IceKingBoss>();if(boss)e.Damage(Mathf.Min(damage,e.maxHealth*.24f));else e.FinishDeath();}
 static void ClearShots(){foreach(var h in FindObjectsByType<EncounterHazard>())if(h.clearable)h.Clear();foreach(var p in FindObjectsByType<CandyProjectile>())Destroy(p.gameObject);}
 void Finish(bool restore,string text){if(!Active)return;Active=false;for(int i=0;i<2;i++){if(restore)g.players[i].transform.position=original[i];g.players[i].Protect(1.5f);FireFX.Burst(g.players[i].transform.position,CoopPresentation.PlayerColor(i),24);}Cleanup();powers.FinishDance();powers.Message(text);g.Tone(150,.3f);}
 void Cancel(){if(FreezePlayers)for(int i=0;i<2;i++)if(g.players[i]&&!g.players[i].Dead)g.players[i].transform.position=original[i];Active=false;Cleanup();}
 void CreateStars(){for(int i=0;i<2;i++){var o=new GameObject("Estrella de "+(i==0?"Mimo":"Pipro"));o.transform.SetParent(transform);stars[i]=o.AddComponent<SpriteRenderer>();stars[i].sprite=CoopPresentation.Star;stars[i].color=CoopPresentation.PlayerColor(i);stars[i].sortingOrder=45;}var line=new GameObject("Trazo de constelacion");line.transform.SetParent(transform);constellation=line.AddComponent<LineRenderer>();constellation.material=new Material(Shader.Find("Sprites/Default"));constellation.startColor=CoopPresentation.PlayerColor(0);constellation.endColor=CoopPresentation.PlayerColor(1);constellation.startWidth=constellation.endWidth=.09f;constellation.sortingOrder=44;constellation.positionCount=2;UpdateStars();}
 void UpdateStars(){for(int i=0;i<2;i++){stars[i].transform.position=Target(i);stars[i].transform.localScale=Vector3.one*.8f;}if(constellation){constellation.positionCount=trace.Count+2;for(int i=0;i<trace.Count;i++)constellation.SetPosition(i,trace[i]);constellation.SetPosition(trace.Count,Target(0));constellation.SetPosition(trace.Count+1,Target(1));}}
 void Cleanup(){for(int i=0;i<2;i++)if(stars[i])Destroy(stars[i].gameObject);if(constellation){Destroy(constellation.material);Destroy(constellation.gameObject);}}
 void OnDestroy(){Cleanup();}
 void OnGUI(){if(!g||!g.Running||director.TutorialOpen)return;GUI.depth=-12;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/640f,Screen.height/360f,1));var style=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=11,wordWrap=true};style.normal.textColor=Color.white;
  if(!Active&&!director.Dancing)return;
  if(!Active)return;if(Selected==Mode.Tirachinas){GUI.color=new Color(.015f,.01f,.08f,.7f);GUI.DrawTexture(new Rect(0,0,640,360),Texture2D.whiteTexture);GUI.color=Color.white;for(int i=0;i<2;i++){var v=Camera.main.WorldToViewportPoint(g.players[i].transform.position);CoopPresentation.DrawAura(v.x*640,(1-v.y)*360,65+tension*35,CoopPresentation.PlayerColor(i),Time.time);}GUI.Box(new Rect(100,90,440,64),GUIContent.none);GUI.Label(new Rect(105,93,430,56),"TIRACHINAS / "+Mathf.RoundToInt(tension*100)+"%\nMIMO: MANTEN A / PIPRO: FLECHAS PARA APUNTAR\nSOLTAR: MIMO D + PIPRO CTRL DER / "+Mathf.CeilToInt(6.5f-elapsed)+"S",style);var aimVector=new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad)*facing,-Mathf.Sin(angle*Mathf.Deg2Rad));for(int dot=1;dot<=15;dot++){var at=new Vector2(320,265)+aimVector*dot*5;GUI.color=dot==15?Color.white:Color.cyan;GUI.DrawTexture(new Rect(at.x-2,at.y-2,4,4),Texture2D.whiteTexture);}GUI.color=Color.white;var mid=new Vector2(320,220);GUI.Label(new Rect(mid.x-90,mid.y,180,25),"ANGULO "+Mathf.RoundToInt(angle)+" / "+(facing==1?"DERECHA":"IZQUIERDA"),style);}
  else{GUI.Box(new Rect(140,90,360,44),GUIContent.none);GUI.Label(new Rect(145,94,350,37),"CONSTELACION "+pairs+"/3 / "+Mathf.CeilToInt(8-elapsed)+"S\nCADA JUGADOR A SU ESTRELLA",style);}
 }
}
public class CoopRicochet:MonoBehaviour {
 GameManager g;Vector2 velocity;float damage,life;int bounces;readonly HashSet<EnemyBase> hit=new HashSet<EnemyBase>();
 public static CoopRicochet Create(GameManager game,Vector2 pos,Vector2 direction,float damage){var o=new GameObject("Onda cooperativa de rebote");o.transform.position=pos;var p=o.AddComponent<CoopRicochet>();p.g=game;p.velocity=direction.normalized*15;p.damage=damage;p.life=2.8f;var s=o.AddComponent<SpriteRenderer>();s.sprite=CoopPresentation.Star;s.color=Color.white;s.sortingOrder=45;o.transform.localScale=Vector3.one*.85f;return p;}
 void Update(){if(!g||!g.Running){Destroy(gameObject);return;}float dt=Time.deltaTime;if(dt<=0)return;life-=dt;Vector2 old=transform.position,step=velocity*dt;foreach(var h in Physics2D.CircleCastAll(old,.4f,step.normalized,step.magnitude,1<<10)){var e=h.collider.GetComponentInParent<EnemyBase>();if(e&&hit.Add(e)){CoopUltimateModes.ApplyUltimateDamage(e,damage);FireFX.Burst(LinkPowers.TargetPoint(e),CoopPresentation.PlayerColor(hit.Count%2),18);}}Vector2 next=old+step;
  if(next.x< -10.2f||next.x>10.2f){next.x=Mathf.Clamp(next.x,-10.2f,10.2f);velocity.x=-velocity.x;bounces++;}if(next.y< -3.65f||next.y>4.1f){next.y=Mathf.Clamp(next.y,-3.65f,4.1f);velocity.y=-velocity.y;bounces++;}transform.position=next;FireFX.Burst(next,CoopPresentation.PlayerColor(Time.frameCount%2),1);if(life<=0||bounces>3)Destroy(gameObject);
 }
}
}
