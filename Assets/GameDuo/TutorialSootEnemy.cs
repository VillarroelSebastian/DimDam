using UnityEngine;
namespace CoopBeam {
// Bottom-pivoted atlas frames follow the physics feet. No rotating image cards.
[DefaultExecutionOrder(1500)]
public sealed class TutorialSootEnemy:MonoBehaviour {
 static Sprite[] frames;static float frameHeight;
 EnemyBase enemy;Rigidbody2D body;Collider2D shape;SpriteRenderer art,shadow;Material material;
 float hurtUntil,deathAt=-1,previous;Vector3 lastFoot;float facing=1;
 public string Pose {get;private set;}
 public float DamageFeedback=>Mathf.Clamp01((hurtUntil-Time.time)/.25f);
 public float DissolveProgress=>deathAt<0?0:Mathf.Clamp01((Time.time-deathAt)/.8f);
 public SpriteRenderer Art=>art;
 static void Load(){
  // Runtime sprites are destroyed when Play Mode ends, even when static fields survive.
  if(frames!=null&&System.Array.TrueForAll(frames,frame=>frame))return;
  frameHeight=0;
  var tex=Resources.Load<Texture2D>("GameDuo/Soot/Bill");if(!tex){frames=null;return;}
  var pixels=tex.GetPixels32();frames=new Sprite[6];int w=tex.width/3,h=tex.height/2;
  for(int n=0;n<6;n++){
   int x0=n%3*w,y0=(1-n/3)*h,minX=w,minY=h,maxX=0,maxY=0;
   for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(pixels[(y0+y)*tex.width+x0+x].a>50){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
   var rect=new Rect(x0+minX,y0+minY,maxX-minX+1,maxY-minY+1);
   frames[n]=Sprite.Create(tex,rect,new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);frames[n].name="Bill / "+n;frameHeight=Mathf.Max(frameHeight,frames[n].bounds.size.y);
  }
 }
 void Start(){
  Load();if(frames==null){enabled=false;return;}enemy=GetComponent<EnemyBase>();body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();previous=enemy.Health;
  var old=GetComponent<GameDuoEnemyVisual>();if(old)old.enabled=false;
  foreach(var a in enemy.visual.GetComponentsInChildren<PixelSpriteAnimator>())a.enabled=false;
  foreach(var a in enemy.visual.GetComponentsInChildren<IllustratedMotion>())a.enabled=false;
  var shader=Resources.Load<Shader>("GameDuo/Soot/SootDissolve");
  if(!shader||!shader.isSupported){Debug.LogWarning("Bill dissolve unavailable; keeping original enemy art.");enabled=false;return;}
  var root=new GameObject("Bill / grounded animation");root.transform.SetParent(transform,false);art=root.AddComponent<SpriteRenderer>();art.sprite=frames[3];art.sortingOrder=12;
  material=new Material(shader);art.sharedMaterial=material;enemy.sprite.enabled=false;
  var s=new GameObject("Bill / contact shadow");s.transform.SetParent(transform,false);shadow=s.AddComponent<SpriteRenderer>();shadow.sprite=TutorialFactory.Shape(2);shadow.sortingOrder=0;
  lastFoot=new Vector3(shape.bounds.center.x,shape.bounds.min.y,0);
 }
 public void NotifyDamage(){hurtUntil=Time.time+.25f;}
 public void BeginDisintegration(){
  if(deathAt>=0)return;deathAt=Time.time;
  if(art)TutorialAsh.Emit(art.bounds.center,28);
 }
 void LateUpdate(){
  if(!art)return;enemy.sprite.enabled=false;
  if(enemy.Health<previous)NotifyDamage();previous=enemy.Health;
  if(enemy.Dead&&deathAt<0)BeginDisintegration();
  bool hurt=Time.time<hurtUntil;var velocity=body.linearVelocity;
  if(!enemy.Dead)lastFoot=new Vector3(shape.bounds.center.x,shape.bounds.min.y,0);
  var hit=Physics2D.Raycast(lastFoot+Vector3.up*.08f,Vector2.down,8,(1<<8)|(1<<9));bool ground=hit&&hit.distance<.2f;
  Pose=enemy.Dead?"Dissolve":hurt?"Hurt":enemy.Telegraphing?"Anticipate":enemy.Attacking?"Attack":!ground?(velocity.y>0?"Jump":"Fall"):Mathf.Abs(velocity.x)>.2f?"Run":"Idle";
  int frame=hurt?4+(int)(Time.time*16)%2:Pose=="Run"?(int)(Time.time*10)%3:3;art.sprite=frames[frame];
  if(Mathf.Abs(velocity.x)>.15f)facing=velocity.x<0?-1:1;art.flipX=facing<0;
  float size=(enemy.kind==EnemyKind.Armored?1.65f:1.4f)/frameHeight;
  float breathe=Pose=="Idle"?Mathf.Sin(Time.time*4)*.012f:0;
  art.transform.localScale=new Vector3(size*(1+breathe)/Mathf.Abs(transform.lossyScale.x),size*(1-breathe)/Mathf.Abs(transform.lossyScale.y),1);
  art.transform.position=lastFoot;art.transform.rotation=Quaternion.identity;
  material.SetFloat("_Hit",hurt?1:0);material.SetFloat("_Dissolve",DissolveProgress);
  shadow.enabled=hit&&!enemy.Dead;if(hit){shadow.transform.position=new Vector3(lastFoot.x,hit.point.y+.015f,0);shadow.transform.localScale=new Vector3(1.25f/Mathf.Abs(transform.lossyScale.x),.18f/Mathf.Abs(transform.lossyScale.y),1);shadow.color=new Color(.12f,.07f,.17f,.65f*Mathf.Clamp01(1-hit.distance/7));}
 }
 void OnDestroy(){if(material)Destroy(material);}
}
public sealed class TutorialAsh:MonoBehaviour {
 SpriteRenderer art;Vector3 velocity;float age,duration;float size;
 public static void Emit(Vector3 origin,int count){
  for(int i=0;i<count;i++){
   var o=new GameObject("Hollin / ceniza");o.transform.position=origin+new Vector3(Random.Range(-.5f,.5f),Random.Range(-.5f,.5f),0);
   var f=o.AddComponent<TutorialAsh>();f.art=o.AddComponent<SpriteRenderer>();f.art.sprite=TutorialFactory.Shape(1);f.art.sortingOrder=15;f.size=Random.Range(.035f,.10f);f.duration=Random.Range(.55f,.95f);f.velocity=new Vector3(Random.Range(-.8f,.8f),Random.Range(.6f,1.8f),0);o.transform.localScale=Vector3.one*f.size;
  }
 }
 void Update(){age+=Time.deltaTime;float life=1-age/duration;if(life<=0){Destroy(gameObject);return;}transform.position+=velocity*Time.deltaTime;velocity*=1-Time.deltaTime*.6f;transform.localScale=Vector3.one*size*life;art.color=Color.Lerp(new Color(.15f,.13f,.22f,0),new Color(.38f,.30f,.42f,.85f),life);}
}
// Shared death presentation for the other arenas too; freezes art above its last contact point.
[DefaultExecutionOrder(2000)]
public sealed class EnemyDisintegration:MonoBehaviour {
 SpriteRenderer art;Material material;Vector3 rest;float age;
 public void Initialize(SpriteRenderer source){
  if(!source)return;art=source;rest=source.transform.position;var shader=Resources.Load<Shader>("GameDuo/Soot/SootDissolve");if(shader&&shader.isSupported){material=new Material(shader);art.sharedMaterial=material;}TutorialAsh.Emit(art.bounds.center,24);
 }
 void LateUpdate(){if(!art)return;age+=Time.deltaTime;art.transform.position=rest;art.transform.rotation=Quaternion.identity;var c=art.color;c.a=1;art.color=c;if(material)material.SetFloat("_Dissolve",Mathf.Clamp01(age/.8f));else {c.a=1-Mathf.Clamp01(age/.8f);art.color=c;}}
 void OnDestroy(){if(material)Destroy(material);}
}

}
