using UnityEngine;
namespace CoopBeam {
[DefaultExecutionOrder(1200)]
[DisallowMultipleComponent]
public sealed class GameDuoEnemyVisual:MonoBehaviour {
 EnemyBase enemy;IceKingBoss ice;CandyBoss candy;SpriteRenderer art;float previous,hurtUntil,stateAt;int state=-1,row;float height;
 static Sprite[] gumFrames,iceFrames,uncleFrames;
 static bool FramesAlive(Sprite[] frames)=>frames!=null&&System.Array.TrueForAll(frames,frame=>frame);
 static Sprite[] Atlas(string name,bool gum){
  var texture=Resources.Load<Texture2D>("GameDuo/"+name);if(!texture)return null;
  var frames=new Sprite[32];float w=texture.width/8f,h=texture.height/4f;
  for(int n=0;n<32;n++){
   int col=n%8,r=n/8;bool elastic=gum&&r!=1&&col==6;
   // Elastic punches deliberately span two atlas cells. Keep the torso pivot stable.
   float x0=Mathf.Round((elastic?5:col)*w),x1=Mathf.Round((elastic?7:col+1)*w);
   float y0=Mathf.Round((3-r)*h),y1=Mathf.Round((4-r)*h);
   var rect=new Rect(x0,y0,x1-x0,y1-y0);
   frames[n]=Sprite.Create(texture,rect,new Vector2(elastic?.25f:.5f,.5f),h,0,SpriteMeshType.FullRect);
   frames[n].name=name+"_"+n;
  }
  return frames;
 }
 void Start(){
  if((GameDuoCandyEnemy.InFinalScene||TutorialFactory.IsTutorial&&GetComponent<EnemyBase>().kind==EnemyKind.Flyer)&&!GetComponent<CandyBoss>()){if(!GetComponent<GameDuoCandyEnemy>())gameObject.AddComponent<GameDuoCandyEnemy>();enabled=false;return;}
  if(TutorialFactory.IsTutorial&&GetComponent<EnemyBase>().kind!=EnemyKind.Flyer&&!GetComponent<IceKingBoss>()&&!GetComponent<CandyBoss>()){if(!GetComponent<TutorialSootEnemy>())gameObject.AddComponent<TutorialSootEnemy>();enabled=false;return;}
  enemy=GetComponent<EnemyBase>();ice=GetComponent<IceKingBoss>();candy=GetComponent<CandyBoss>();art=enemy.sprite;previous=enemy.Health;
  if(ice&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="03_ReyHielo"&&Resources.Load<Texture2D>("GameDuo/IceRefresh/King")){gameObject.AddComponent<GameDuoIceKingMotion>();enabled=false;return;}
  if(candy&&GameDuoCampaign.InCampaignScene){gameObject.AddComponent<GameDuoCandyKing>();enabled=false;return;}
  if(ice){if(!FramesAlive(iceFrames))iceFrames=Atlas("IceKing_32",false);height=3.6f;}
  else if(candy){if(!FramesAlive(uncleFrames))uncleFrames=Atlas("GumUncle_32",false);height=3.4f;}
  else {if(!FramesAlive(gumFrames))gumFrames=Atlas("GumRaiders_32",true);row=enemy.kind==EnemyKind.Armored?1:enemy.kind==EnemyKind.Flyer?2:enemy.kind==EnemyKind.Cannon?3:0;height=row==1?1.7f:row==2?1.45f:1.4f;}
 }
 void LateUpdate(){
  if(!enemy||!art)return;var frames=ice?iceFrames:candy?uncleFrames:gumFrames;if(frames==null)return;
  if(enemy.Health<previous)hurtUntil=Time.time+.24f;previous=enemy.Health;
  bool dead=enemy.Dead||ice&&ice.Defeated;
  bool windup=ice?ice.Telegraphing:candy?candy.WindingUp:enemy.Telegraphing;
  bool attack=ice?ice.Attacking:candy?Time.time-candy.LastAttackTime<.5f:enemy.Attacking;
  bool open=ice?ice.Vulnerable:candy&&candy.Vulnerable;
  int next=dead?5:Time.time<hurtUntil?4:open?3:windup?1:attack?2:0;
  if(next!=state){state=next;stateAt=Time.time;}
  float age=Time.time-stateAt;int frame;
  if(ice||candy){
   frame=state==5?28+Mathf.Min(3,(int)(age*4)):state==4?26+(int)(age*10)%2:state==3?24+(int)(age*5)%2:state==1?8+Mathf.Min(7,(int)(age*7)):state==2?16+(int)(age*10)%8:(int)(age*8)%8;
  }else{
   int gait=Mathf.Abs(GetComponent<Rigidbody2D>().linearVelocity.x)>.2f?2+(int)(age*10)%3:(int)(age*5)%2;
   frame=row*8+(state==5||state==4?7:state==1?(row==1?5:4):state==2?6:gait);
  }
  if(ice&&state==2)frame=16+ice.PatternIndex*2+Mathf.Min(1,(int)(age*7));
  art.sprite=frames[frame];
  float wave=Mathf.Sin(age*(state==2?18:8));float squash=state==1?.065f:state==2?.10f:.018f;
  // Only art changes; the existing combat bodies and weak spots remain authoritative.
  float scale=height/(art.sprite.bounds.size.y*Mathf.Abs(transform.lossyScale.y));
  art.transform.localScale=new Vector3(scale*(1+wave*squash),scale*(1-wave*squash),1);
  art.transform.localRotation=Quaternion.Euler(0,0,state==4?Mathf.Sin(age*35)*7:state==1?wave*2:0);
  if(!(ice||candy))art.flipX=GetComponent<Rigidbody2D>().linearVelocity.x<-.1f;
  else art.flipX=false;
  var shape=GetComponent<Collider2D>();
  if(shape&&shape.enabled){var position=art.transform.position;position.y=shape.bounds.min.y+height*.5f;art.transform.position=position;}
  if(dead){var color=art.color;color.a=ice&&!enemy.Dead?1:Mathf.Clamp01(1-age/.42f);art.color=color;}
 }
}
}
