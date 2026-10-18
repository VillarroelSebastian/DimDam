using UnityEngine;
namespace CoopBeam {
public enum EnemyKind {Walker,Jumper,Chaser,Armored,Flyer,Cannon,Boss}
public class EnemyBase:MonoBehaviour {
 public EnemyKind kind;public float maxHealth=28,speed=1.8f;public SpriteRenderer sprite;public Transform visual;public float Health{get;private set;}
 public bool externalControl,combatTelegraph,combatAttack; public bool Dead{get;private set;}public bool Grounded{get;private set;}public bool Telegraphing=>combatTelegraph||charge>.45f;public bool Attacking=>combatAttack||charge>0&&charge<=.45f;
 Rigidbody2D body;Collider2D shape;float direction=1,timer=1,targetTimer,flash,charge,avoidTimer,turnCooldown,progressTimer;int target;Vector2 lastProgress;Vector3 baseScale;
 void Awake(){body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();Health=maxHealth;baseScale=visual.localScale;lastProgress=body.position;}
 void Start(){Health=maxHealth;direction=transform.position.x>0?-1:1;if(!GetComponent<IceEnemyBrain>()&&!GetComponent<FireEnemyBrain>()&&!GetComponent<IceKingBoss>()&&kind!=EnemyKind.Boss&&!GetComponent<CandyEnemyTactics>())gameObject.AddComponent<CandyEnemyTactics>();}
 public void Damage(float amount){if(Dead||amount<=0)return;var boss=GetComponent<CandyBoss>();if(boss&&!boss.Vulnerable)return;var ice=GetComponent<IceKingBoss>();if(ice)amount=ice.FilterDamage(amount,Health);var fire=GetComponent<FireForgeBoss>();if(fire)amount=fire.FilterDamage(amount,Health);var fireEnemy=GetComponent<FireEnemyBrain>();if(fireEnemy)amount=fireEnemy.FilterDamage(amount);if(kind==EnemyKind.Armored&&!fireEnemy&&!GetComponent<IceEnemyBrain>()&&GameManager.Instance){var ps=GameManager.Instance.players;bool flanked=!ps[0].Dead&&!ps[1].Dead&&(ps[0].transform.position.x-transform.position.x)*(ps[1].transform.position.x-transform.position.x)<0;if(!flanked)amount*=.4f;}if(amount<=0)return;Health-=amount;flash=.09f;if(Health>0)return;if(ice){Health=0;ice.Defeat();return;}if(fire){Health=0;fire.Defeat();return;}FinishDeath();}
 public void FinishDeath(){if(Dead)return;
  Dead=true;if(body.bodyType!=RigidbodyType2D.Static)body.linearVelocity=Vector2.zero;body.simulated=false;foreach(var c in GetComponentsInChildren<Collider2D>())c.enabled=false;
  var special=GetComponent<CandyEnemySpecial>();if(special)special.enabled=false;var boss=GetComponent<CandyBoss>();if(boss)boss.enabled=false;
  BeamFX.Burst(transform.position,new Color(1,.8f,.65f),12);if(GameManager.Instance)GameManager.Instance.EnemyKilled(this);Destroy(gameObject,.42f);
 }
 void FixedUpdate(){if(Dead)return;if(!GameManager.Instance||!GameManager.Instance.Running){if(body.bodyType!=RigidbodyType2D.Static)body.linearVelocity=Vector2.zero;return;}
  if(externalControl||kind==EnemyKind.Flyer||kind==EnemyKind.Cannon||kind==EnemyKind.Boss)return;
  float dt=Time.fixedDeltaTime;timer-=dt;targetTimer-=dt;avoidTimer-=dt;turnCooldown-=dt;
  var players=GameManager.Instance.players;if(targetTimer<=0){target=1-target;targetTimer=3;}if(players[target].Dead)target=1-target;
  var bounds=shape.bounds;Vector2 pos=body.position;float halfH=bounds.extents.y,halfW=bounds.extents.x;int terrain=(1<<8)|(1<<9);
  Grounded=Physics2D.Raycast(pos+Vector2.down*(halfH-.025f),Vector2.down,.14f,terrain);
  float limit=11.4f-halfW-.1f;float targetX=Mathf.Clamp(players[target].transform.position.x,-limit+.2f,limit-.2f);
  bool canJump=!Physics2D.Raycast(pos,Vector2.up,halfH+.25f,1<<8);
  if(kind==EnemyKind.Chaser&&avoidTimer<=0&&Mathf.Abs(targetX-pos.x)>.25f)direction=Mathf.Sign(targetX-pos.x);
  if(kind==EnemyKind.Jumper&&Grounded&&timer<=0){float d=targetX-pos.x;if(Mathf.Abs(d)>.25f)direction=Mathf.Sign(d);if(canJump){body.linearVelocity=new Vector2(direction*speed,12);timer=1.8f;}}
  if(kind==EnemyKind.Chaser&&Grounded&&timer<=0&&canJump&&players[target].transform.position.y>pos.y+1){body.linearVelocity=new Vector2(direction*speed,11);timer=1.3f;}
  bool wall=Physics2D.Raycast(pos,Vector2.right*direction,halfW+.16f,1<<8);
  Vector2 ledge=pos+new Vector2(direction*(halfW+.16f),-halfH+.12f);
  bool edge=!Physics2D.Raycast(ledge,Vector2.down,.35f,terrain);
  if((wall||(Grounded&&edge&&(kind==EnemyKind.Walker||kind==EnemyKind.Armored)))&&turnCooldown<=0){direction=-direction;turnCooldown=.24f;avoidTimer=.7f;charge=0;if(kind==EnemyKind.Chaser&&Grounded&&canJump){body.linearVelocity=new Vector2(direction*speed,10);timer=1.2f;}}
  if(pos.x>=limit){direction=-1;avoidTimer=.6f;}else if(pos.x<=-limit){direction=1;avoidTimer=.6f;}
  float actual=speed;
  if(kind==EnemyKind.Armored){if(timer<=0&&Grounded&&!edge&&!wall){charge=.9f;timer=3.8f;float d=targetX-pos.x;if(Mathf.Abs(d)>.3f)direction=Mathf.Sign(d);}if(charge>0){charge-=dt;actual=charge>.45f?0:5;}}
  var v=body.linearVelocity;v.x=direction*actual;v.y=Mathf.Max(v.y,-22);body.linearVelocity=v;
  progressTimer+=dt;if(progressTimer>.65f){if(actual>.5f&&Grounded&&Mathf.Abs(pos.x-lastProgress.x)<.08f){direction=-direction;avoidTimer=.9f;turnCooldown=.25f;charge=0;if((kind==EnemyKind.Jumper||kind==EnemyKind.Chaser)&&canJump)body.linearVelocity=new Vector2(direction*speed,10);}lastProgress=pos;progressTimer=0;}
  if(pos.y< -7){body.position=new Vector2(Mathf.Clamp(pos.x,-9,9),-3.6f);body.linearVelocity=Vector2.zero;}
 }
 void Update(){if(Dead)return;flash-=Time.deltaTime;sprite.color=flash>0?new Color(1,.65f,.7f):kind==EnemyKind.Armored&&Telegraphing?new Color(1,.4f,.4f):Color.white;visual.localScale=Vector3.Scale(baseScale,new Vector3(1+Mathf.Sin(Time.time*8)*.025f,1-Mathf.Sin(Time.time*8)*.025f,1));}
 void OnCollisionEnter2D(Collision2D c){ContactDamage(c);}void OnCollisionStay2D(Collision2D c){ContactDamage(c);}
 void ContactDamage(Collision2D c){if(Dead)return;var h=c.gameObject.GetComponent<PlayerHealth>();if(h)h.Hit(c.transform.position-transform.position);}
}}

