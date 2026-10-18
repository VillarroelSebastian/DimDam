using UnityEngine;
using UnityEngine.SceneManagement;
namespace CoopBeam {
// Uses the existing EnemyBase health, beam collisions, kill rewards and death effects.
[DefaultExecutionOrder(1600)]
[DisallowMultipleComponent]
public sealed class GameDuoCandyEnemy:MonoBehaviour {
 public enum Species { Wasp, Donut, Bear, Cookie, Turret }
 public static bool InFinalScene => SceneManager.GetActiveScene().path==GameDuoCampaign.Scenes[4];
 public Species Type {get;private set;}
 public GameDuoCandyKing Owner;
 public bool Telegraphing => state==1;
 public int Shots {get;private set;}
 public int Attacks {get;private set;}
 public Vector2 LockedAim {get;private set;}
 EnemyBase enemy; Rigidbody2D body; Collider2D shape; SpriteRenderer art; Sprite[] frames;
 Vector2 home,dash; float age,timer,stateAge,life; int state; bool facesLeft;
 static readonly Sprite[][] cache=new Sprite[5][];
 static readonly string[] names={"Wasp","Donut","Bear","Cookie","Turret"};
 public static Species ForKind(EnemyKind kind){return kind==EnemyKind.Flyer?Species.Wasp:kind==EnemyKind.Jumper?Species.Donut:kind==EnemyKind.Cannon?Species.Turret:kind==EnemyKind.Chaser?Species.Cookie:Species.Bear;}
 void Start(){
  enemy=GetComponent<EnemyBase>();body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();art=enemy.sprite;Type=ForKind(enemy.kind);
  var old=GetComponent<CandyEnemySpecial>();if(old){old.enabled=false;if(old.telegraph)old.telegraph.gameObject.SetActive(false);}
  var tactics=GetComponent<CandyEnemyTactics>();if(tactics)tactics.enabled=false;
  foreach(var motion in GetComponentsInChildren<IllustratedMotion>())motion.enabled=false;
  foreach(var animation in GetComponentsInChildren<PixelSpriteAnimator>())animation.enabled=false;
  enemy.externalControl=true;home=body.position;timer=2.2f+Mathf.Abs(home.x)*.19f;
  bool flying=Type==Species.Wasp||Type==Species.Donut;
  body.bodyType=RigidbodyType2D.Dynamic;body.gravityScale=flying||Type==Species.Turret?0:3;body.constraints=RigidbodyConstraints2D.FreezeRotation;
  if(Type==Species.Turret)body.constraints|=RigidbodyConstraints2D.FreezePosition;
  body.linearVelocity=Vector2.zero;life=Owner?24:0;
  if(Type==Species.Turret){var floor=Physics2D.Raycast(new Vector2(home.x,home.y+1),Vector2.down,4,(1<<8)|(1<<9));if(floor){body.position=new Vector2(home.x,floor.point.y+shape.bounds.extents.y);home=body.position;}}
  if(cache[(int)Type]==null||!System.Array.TrueForAll(cache[(int)Type],frame=>frame))cache[(int)Type]=GameDuoCandyKing.LoadFrames(names[(int)Type],Type==Species.Bear?1.65f:Type==Species.Turret?1.45f:1.25f,false);
  frames=cache[(int)Type];if(frames!=null)art.sprite=frames[0];art.enabled=true;art.sortingOrder=12;art.color=Color.white;
  if(frames==null)Debug.LogError("Missing candy enemy atlas: "+names[(int)Type]);
 }
 PlayerHealth Target(){PlayerHealth best=null;float distance=float.MaxValue;foreach(var player in GameManager.Instance.players){if(!player||player.Dead)continue;float d=((Vector2)player.transform.position-body.position).sqrMagnitude;if(d<distance){best=player;distance=d;}}return best;}
 bool AttackSlot(){int active=0;foreach(var other in FindObjectsByType<GameDuoCandyEnemy>())if(other!=this&&(other.state==1||other.state==2))active++;return active<2;}
 void FixedUpdate(){
  if(!enemy||enemy.Dead)return;var game=GameManager.Instance;
  if(!game||!game.Running){body.linearVelocity=Vector2.zero;return;}
  float dt=Time.fixedDeltaTime;age+=dt;stateAge+=dt;
  if(Owner){life-=dt;if(life<=0||Owner.GetComponent<EnemyBase>().Dead){Retire();return;}}
  var target=Target();if(!target)return;Vector2 delta=(Vector2)target.transform.position-body.position;
  if(state==0){
   facesLeft=delta.x<0;timer-=dt;
   if(Type==Species.Wasp||Type==Species.Donut){Vector2 point=home+new Vector2(Mathf.Sin(age*1.3f)*1.15f,Mathf.Sin(age*2.6f)*.28f);body.linearVelocity=(point-body.position)*3;}
   else if(Type!=Species.Turret){float sign=Mathf.Abs(delta.x)<1.4f?0:Mathf.Sign(delta.x);var bounds=shape.bounds;bool edge=!Physics2D.Raycast(new Vector2(bounds.center.x+sign*(bounds.extents.x+.25f),bounds.min.y+.1f),Vector2.down,.7f,(1<<8)|(1<<9));body.linearVelocity=new Vector2(edge?0:sign*(Type==Species.Cookie?1.9f:1.3f),body.linearVelocity.y);}
   else body.linearVelocity=Vector2.zero;
   if(timer<=0&&AttackSlot()&&(Type==Species.Wasp||Type==Species.Donut||Type==Species.Turret||delta.magnitude<2.6f)){state=1;stateAge=0;LockedAim=target.transform.position;body.linearVelocity=Vector2.zero;BeamFX.Burst(body.position,new Color(1,.8f,.25f),5);}
  }else if(state==1){
   body.linearVelocity=new Vector2(0,body.gravityScale>0?body.linearVelocity.y:0);
   // Lock aim early so players can dodge; never home projectiles after release.
   if(stateAge>=.9f){state=2;stateAge=0;Attacks++;dash=(LockedAim-body.position).normalized;facesLeft=dash.x<0;
    if(Type==Species.Turret||Type==Species.Donut){Shoot(dash);if(Type==Species.Donut){Shoot(Quaternion.Euler(0,0,14)*dash);Shoot(Quaternion.Euler(0,0,-14)*dash);}}
    if(Type==Species.Bear||Type==Species.Cookie)EncounterHazard.Spawn(body.position+new Vector2(Mathf.Sign(dash.x)*.95f,-.25f),new Vector2(1.1f,.65f),0,.22f,Vector2.zero,new Color(1,.5f,.3f),this);
   }
  }else if(state==2){
   if(Type==Species.Wasp)body.linearVelocity=dash*5.2f;
   else if(Type==Species.Cookie)body.linearVelocity=new Vector2(Mathf.Sign(dash.x)*4,body.linearVelocity.y);
   if(stateAge>.42f){state=3;stateAge=0;body.linearVelocity=new Vector2(0,body.gravityScale>0?body.linearVelocity.y:0);}
  }else if(stateAge>1){state=0;stateAge=0;timer=Type==Species.Turret?3.5f:Type==Species.Donut?4.5f:2.8f;}
  enemy.combatTelegraph=state==1;enemy.combatAttack=state==2;
  if(Mathf.Abs(body.position.x)>10.3f)body.position=new Vector2(Mathf.Clamp(body.position.x,-10.3f,10.3f),body.position.y);
  if(body.position.y< -6)body.position=new Vector2(body.position.x,-3.4f);
 }
 void Shoot(Vector2 direction){var special=GetComponent<CandyEnemySpecial>();var sprite=special&&special.projectile?special.projectile:GameManager.Instance.particle;var shot=CandyProjectile.Fire(body.position+direction*.7f,direction*4.5f,sprite,art.sharedMaterial,.32f);shot.lifetime=4;shot.gameObject.AddComponent<GameDuoCandyShot>().Owner=this;Shots++;}
 void LateUpdate(){
  if(!enemy||enemy.Dead||frames==null||!art)return;
  int frame=state==1?4:state==2?5+Mathf.Min(1,(int)(stateAge*7)):state==3?7:(int)(age*9)%4;
  if(Type==Species.Wasp)frame=state==1?5:state==2?4:state==3?7:(int)(age*12)%4;
  if(Type==Species.Donut)frame=state==1?5:state==2?6:state==3?7:(int)(age*10)%4;
  if(Type==Species.Turret){var game=GameManager.Instance;var target=game?Target():null;Vector2 aim=state==1||state==2?LockedAim-body.position:target?(Vector2)target.transform.position-body.position:Vector2.up;frame=state==2?6:aim.y>Mathf.Abs(aim.x)?2:aim.x<0?1:3;art.flipX=state==2&&aim.x<0;}else art.flipX=Type==Species.Cookie?!facesLeft:facesLeft;
  art.sprite=frames[frame];art.enabled=true;art.sortingOrder=12;
  // Normalize against all ancestor scales and anchor to the combat collider every frame.
  var parent=art.transform.parent;art.transform.localScale=new Vector3(1/Mathf.Abs(parent.lossyScale.x),1/Mathf.Abs(parent.lossyScale.y),1);
  art.transform.rotation=Quaternion.identity;art.transform.position=shape.bounds.center;
 }
 public void Retire(){if(!enemy)return;EncounterHazard.ClearOwned(this);foreach(var shot in FindObjectsByType<GameDuoCandyShot>())if(shot.Owner==this)Destroy(shot.gameObject);if(GameManager.Instance)GameManager.Instance.enemies.Remove(enemy);Destroy(gameObject);}
 void OnDisable(){EncounterHazard.ClearOwned(this);}
}
public sealed class GameDuoCandyShot:MonoBehaviour { public GameDuoCandyEnemy Owner;void Update(){if(!Owner||!Owner.enabled||Owner.GetComponent<EnemyBase>().Dead)Destroy(gameObject);} }
}
