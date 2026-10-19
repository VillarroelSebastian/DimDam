using UnityEngine;
namespace CoopBeam {
// The final GameDuo chapter adds an attack without replacing CandyBoss combat rules.
[DefaultExecutionOrder(1500)]
public sealed class GameDuoCandyKing : MonoBehaviour {
 public bool Summoning => summonAge>=0;
 public bool Releasing => summonAge>=1.4f;
 public int Summons {get;private set;}
 CandyBoss boss; EnemyBase enemy; SpriteRenderer art; Sprite[] frames;
 float cooldown=5,summonAge=-1,nextBat,health,hurtUntil; int emitted;
 public int Reinforcements {get;private set;}
 float reinforcementTimer=3;
 float VerticalLane(int lane)=>(Summons%4==2?-6:6)+(lane==0?-2:2);
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
  reinforcementTimer-=Time.deltaTime;
  if(reinforcementTimer<=0){reinforcementTimer=7; if(LivingSummons()<4&&GameManager.Instance.PendingSpawns==0){int[] kinds={0,1,2,4,5};SpawnGuard(kinds[Reinforcements%kinds.Length],new Vector2(Random.Range(-8f,8f),3.5f));}}
  if(summonAge<0){cooldown-=Time.deltaTime;if(cooldown<=0){summonAge=0;emitted=0;nextBat=1.4f;Summons++;GameManager.Instance.Tone(170,.2f);
   bool vertical=Summons%2==0;
   for(int lane=0;lane<2;lane++)EncounterHazard.Spawn(vertical?new Vector2(VerticalLane(lane),1):new Vector2(0,lane==0?-2.8f:1.7f),vertical?new Vector2(.18f,11):new Vector2(20,.18f),1.4f,0,Vector2.zero,new Color(1,.7f,.8f),this);
  }}else{
   summonAge+=Time.deltaTime;
   int count=3+boss.Phase;
   if(summonAge>=nextBat&&emitted<count){
    bool vertical=Summons%2==0;
    for(int lane=0;lane<2;lane++){
     var bat=GameDuoBat.Spawn(this,vertical?VerticalLane(lane):(Summons%4==1?-11:11),vertical?7:(lane==0?-2.8f:1.7f),Summons%4==1?1:-1,4.5f+boss.Phase*.4f,art.sharedMaterial);
     if(bat&&vertical)bat.SetVertical();
    }
    nextBat+=.5f;emitted++;
   }
   if(emitted>=count&&summonAge>nextBat+.5f){summonAge=-1;cooldown=6-boss.Phase*.5f;}
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
  var game=GameManager.Instance;
  game.StartCoroutine(game.SpawnAnnounced(kind,point,this,guard=>{
   guard.maxHealth=kind==5?20:kind==0?28:18;
   var control=guard.GetComponent<GameDuoCandyEnemy>();if(!control)control=guard.gameObject.AddComponent<GameDuoCandyEnemy>();control.Owner=this;
   Reinforcements++;
  }));
 }
 void ClearBats(){foreach(var bat in FindObjectsByType<GameDuoBat>())if(bat.Owner==this)Destroy(bat.gameObject);foreach(var guard in FindObjectsByType<GameDuoCandyEnemy>())if(guard.Owner==this)guard.Retire();EncounterHazard.ClearOwned(this);}
 void OnDisable(){summonAge=-1;ClearBats();}
 void OnDestroy(){if(frames!=null)foreach(var frame in frames)if(frame)Destroy(frame);}
}
}
