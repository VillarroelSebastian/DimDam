using UnityEngine;
namespace CoopBeam {
[DefaultExecutionOrder(350)]
public class PixelSpriteAnimator:MonoBehaviour {
 public PixelAnimationSet animationSet;public int CurrentFrame{get;private set;}public string State{get;private set;}
 SpriteRenderer art;PlayerController2D player;PlayerHealth health;EnemyBase enemy;CandyEnemySpecial special;CandyBoss boss;Rigidbody2D body;
 bool wasGrounded;float stateTime,hurtUntil,landUntil,previousHealth,diedAt=-1;string previousState="";
 void Awake(){art=GetComponent<SpriteRenderer>();player=GetComponentInParent<PlayerController2D>();health=GetComponentInParent<PlayerHealth>();enemy=GetComponentInParent<EnemyBase>();special=GetComponentInParent<CandyEnemySpecial>();boss=GetComponentInParent<CandyBoss>();body=GetComponentInParent<Rigidbody2D>();previousHealth=health?health.Hearts:enemy?enemy.Health:0;}
 void LateUpdate(){if(!animationSet||animationSet.frames==null||animationSet.frames.Length==0)return;
  float hp=health?health.Hearts:enemy?enemy.Health:0;if(hp<previousHealth)hurtUntil=Time.time+.17f;previousHealth=hp;
  bool dead=health?health.Dead:enemy&&enemy.Dead;bool grounded=player?player.Grounded:enemy&&enemy.Grounded;
  if(grounded&&!wasGrounded)landUntil=Time.time+.12f;wasGrounded=grounded;
  if(dead&&diedAt<0)diedAt=Time.time;if(!dead)diedAt=-1;
  State=player&&GameManager.Instance&&GameManager.Instance.Victory?"Victory":dead?"Death":Time.time<hurtUntil?"Hurt":player&&player.Dashing?"Dash":player&&!grounded&&GameManager.Instance&&GameManager.Instance.Running?(body.linearVelocity.y>0?"Jump":"Fall"):Time.time<landUntil&&player?"Land":enemy&&enemy.Telegraphing||special&&special.Telegraphing||boss&&boss.WindingUp?"Anticipate":enemy&&enemy.Attacking||boss&&Time.time-boss.LastAttackTime<.25f||special&&Time.time-special.LastShotTime<.2f?"Attack":Mathf.Abs(body.linearVelocity.x)>.2f?"Run":"Idle";
  if(player&&CoopInteractionDirector.Active&&CoopInteractionDirector.Active.Dancing&&CoopInteractionDirector.Locked(health))State="Dance";
  if(State!=previousState){stateTime=Time.time;previousState=State;}float elapsed=Time.time-stateTime;int frame;
  if(animationSet.hero){frame=State=="Dance"?16+(int)(elapsed*8)%4:State=="Victory"?16+(int)(elapsed*6)%4:State=="Death"?(Time.time-diedAt<.2f?14:15):State=="Hurt"?13:State=="Dash"?12:State=="Jump"?(elapsed<.1f?8:9):State=="Fall"?10:State=="Land"?11:State=="Run"?4+(int)(elapsed*animationSet.runFPS)%4:(int)(elapsed*animationSet.idleFPS)%4;if(dead||State=="Victory")transform.localScale=Vector3.one;if(State=="Victory")art.color=Color.white;}
  else frame=State=="Dance"?16+(int)(elapsed*8)%4:State=="Victory"?16+(int)(elapsed*6)%4:State=="Death"?7:State=="Hurt"?6:State=="Anticipate"?4:State=="Attack"?5:(int)(elapsed*(State=="Run"?animationSet.runFPS:animationSet.idleFPS))%4;
  CurrentFrame=Mathf.Clamp(frame,0,animationSet.frames.Length-1);if(animationSet.frames[CurrentFrame])art.sprite=animationSet.frames[CurrentFrame];
  if(enemy&&dead){var c=art.color;c.a=1-Mathf.Clamp01((Time.time-diedAt-.18f)/.24f);art.color=c;}
 }
 // Only the visual child is deformed. Controller physics, collision and input remain authoritative.
 public void ApplyJellyPose(){if(!player||!animationSet||!animationSet.jellyStyle)return;float t=Time.time-stateTime;float wave=Mathf.Sin(t*10+player.playerIndex*.8f);Vector3 scale=transform.localScale;float angle=transform.localEulerAngles.z;
  if(State=="Death"){scale=new Vector3(1.25f,.14f,1);angle=0;}
  else if(State=="Land"){float squash=1-Mathf.Clamp01(t/.12f);scale=new Vector3(1+.22f*squash,1-.2f*squash,1);}
  else if(State=="Hurt"){scale=new Vector3(1.14f,.86f,1);angle=wave*12;}
  else if(State=="Dance"){scale=new Vector3(1+wave*.12f,1-wave*.12f,1);angle=wave*16;art.flipX=Mathf.Sin(t*5)<0;}
  else if(State=="Victory"){scale=new Vector3(1-wave*.08f,1+wave*.08f,1);angle=wave*8;}
  transform.localScale=scale;transform.localRotation=Quaternion.Euler(0,0,angle);
  if(transform.parent&&art.sprite&&(State=="Death"||player.Grounded&&State!="Dance"&&State!="Victory"))transform.position+=transform.parent.TransformVector(Vector3.up*art.sprite.bounds.extents.y*(scale.y-1));
 }
}}

