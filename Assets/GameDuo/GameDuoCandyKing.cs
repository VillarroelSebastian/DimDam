using UnityEngine;
namespace CoopBeam {
// The final GameDuo chapter adds an attack without replacing CandyBoss combat rules.
[DefaultExecutionOrder(1500)]
public sealed class GameDuoCandyKing : MonoBehaviour {
 public bool Summoning => summonAge>=0;
 public bool Releasing => summonAge>=1.4f;
 public int Summons {get;private set;}
 CandyBoss boss; EnemyBase enemy; SpriteRenderer art; Sprite[] frames;
 float cooldown=5,summonAge=-1,nextBat,health,hurtUntil; int emitted; bool fromLeft;
 public int Reinforcements {get;private set;}
 Vector2 summonPoint;
 public static Sprite[] LoadFrames(string name,float height,bool feet){
  var texture=Resources.Load<Texture2D>("GameDuo/Final/"+name);if(!texture)return null;
  var result=new Sprite[8];var pixels=texture.GetPixels32();
  float ppu=texture.height/2f/height;
  for(int n=0;n<8;n++){
   int x0=n%4*texture.width/4,x1=(n%4+1)*texture.width/4;
   int y0=(1-n/4)*texture.height/2,y1=(2-n/4)*texture.height/2;
   int l=x1,r=x0,b=y1,t=y0;
   for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*texture.width+x].a>40){l=Mathf.Min(l,x);r=Mathf.Max(r,x);b=Mathf.Min(b,y);t=Mathf.Max(t,y);}
   if(r<l){l=x0;r=x1-1;b=y0;t=y1-1;}
   var rect=new Rect(l,b,r-l+1,t-b+1);
   result[n]=Sprite.Create(texture,rect,new Vector2(.5f,feet?0:.5f),ppu,0,SpriteMeshType.FullRect);result[n].name=name+"_"+n;
  }
  return result;
 }
 void Start(){
  boss=GetComponent<CandyBoss>();enemy=GetComponent<EnemyBase>();art=enemy.sprite;health=enemy.Health;
  frames=LoadFrames("PinkKing",3.9f,true);
  foreach(var motion in art.GetComponents<IllustratedMotion>())motion.enabled=false;
  foreach(var animation in art.GetComponents<PixelSpriteAnimator>())animation.enabled=false;
 }
 void Update(){
  if(!enemy||enemy.Dead){ClearBats();return;}
  if(!GameManager.Instance||!GameManager.Instance.Running)return;
  if(summonAge<0){cooldown-=Time.deltaTime;if(cooldown<=0&&LivingSummons()<3){summonAge=0;emitted=0;nextBat=1.4f;fromLeft=Summons%2==0;Summons++;summonPoint=new Vector2(fromLeft?-8:8,-3.4f);EncounterHazard.Spawn(summonPoint,new Vector2(1.6f,.12f),1.4f,.01f,Vector2.zero,new Color(1,.7f,.8f),this);GameManager.Instance.Tone(170,.2f);}}
  else {
   summonAge+=Time.deltaTime;
   int count=boss.Phase==3?2:1;
   if(summonAge>=nextBat&&emitted<count){
    int[] rotation={5,4,0,1,2};int kind=rotation[(Summons-1+emitted)%rotation.Length];
    if(LivingSummons()<3)SpawnGuard(kind,emitted==0?summonPoint:new Vector2(-summonPoint.x,-3.4f));
    if(emitted==0&&count>1)EncounterHazard.Spawn(new Vector2(-summonPoint.x,-3.4f),new Vector2(1.6f,.12f),1.1f,.01f,Vector2.zero,new Color(1,.7f,.8f),this);
    nextBat+=1.1f;emitted++;
   }
   if(emitted>=count&&summonAge>nextBat+.4f){summonAge=-1;cooldown=boss.Phase==1?9:boss.Phase==2?8:7;}
  }
 }
 void LateUpdate(){
  if(frames==null||!art)return;
  if(enemy.Health<health)hurtUntil=Time.time+.28f;health=enemy.Health;
  bool dead=enemy.Dead;float t=Time.time;
  int frame=dead?0:t<hurtUntil?0:Summoning?(summonAge<.45f?2:summonAge<.9f?3:4+(int)((summonAge-.9f)*7)%3):Time.time-boss.LastAttackTime<.5f?3:boss.WindingUp?2:boss.Vulnerable?7:1;
  art.sprite=frames[frame];
  float breath=Mathf.Sin(t*4)*.015f,squash=Summoning?Mathf.Sin(summonAge*9)*.035f:breath;
  var parent=art.transform.parent;float sx=parent?Mathf.Abs(parent.lossyScale.x):1,sy=parent?Mathf.Abs(parent.lossyScale.y):1;
  float fall=dead?Mathf.Max(.2f,art.color.a):1;
  art.transform.localScale=new Vector3((1+squash)/sx,fall*(1-squash)/sy,1);
  var shape=GetComponent<Collider2D>();var p=art.transform.position;
  p.x=transform.position.x;if(shape&&shape.enabled)p.y=shape.bounds.min.y;art.transform.position=p;
  art.transform.localRotation=Quaternion.Euler(0,0,t<hurtUntil?Mathf.Sin(t*65)*5:0);art.flipX=false;
 }
 int LivingSummons(){int count=0;foreach(var guard in FindObjectsByType<GameDuoCandyEnemy>())if(guard.Owner==this&&!guard.GetComponent<EnemyBase>().Dead)count++;return count;}
 void SpawnGuard(int kind,Vector2 point){
  var game=GameManager.Instance;if(kind==1||kind==4)point.y=.6f;
  else {var floor=Physics2D.Raycast(point+Vector2.up,Vector2.down,4,(1<<8)|(1<<9));if(floor)point.y=floor.point.y+.65f;}
  var guard=Instantiate(game.enemyPrefabs[kind],point,Quaternion.identity);guard.maxHealth=kind==5?20:kind==0?28:18;
  guard.kind=kind==0?EnemyKind.Walker:guard.kind;
  var control=guard.gameObject.AddComponent<GameDuoCandyEnemy>();control.Owner=this;
  game.enemies.Add(guard);
  foreach(var other in FindObjectsByType<EnemyBase>())if(other!=guard)Physics2D.IgnoreCollision(guard.GetComponent<Collider2D>(),other.GetComponent<Collider2D>(),true);
  // Reward beam kills normally; ClearBats retires survivors when the king dies.
  Reinforcements++;BeamFX.Burst(point,new Color(1,.6f,.8f),10);
 }
 void ClearBats(){foreach(var bat in FindObjectsByType<GameDuoBat>())if(bat.Owner==this)Destroy(bat.gameObject);foreach(var guard in FindObjectsByType<GameDuoCandyEnemy>())if(guard.Owner==this)guard.Retire();EncounterHazard.ClearOwned(this);}
 void OnDisable(){summonAge=-1;ClearBats();}
 void OnDestroy(){if(frames!=null)foreach(var frame in frames)if(frame)Destroy(frame);}
}
}
