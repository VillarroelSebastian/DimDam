using System.Collections.Generic;
using UnityEngine;
namespace CoopBeam {
// Additional drawn poses, with short blends and fixed foot pivots. No combat transforms change.
[DefaultExecutionOrder(1400)]
[DisallowMultipleComponent]
public sealed class GameDuoIcePoseFrames:MonoBehaviour {
 public int FrameCount=>frames==null?0:frames.Length;
 public int DisplayedFrame{get;private set;}=-1;
 EnemyBase enemy;IceKingBoss king;IceEnemyBrain brain;SpriteRenderer source,current,previous;
 Sprite[] frames,originalFrames;Sprite originalKing;float stateAt,hurtUntil,health,blendAt;int state=-1;bool facingLeft;
 static readonly Dictionary<string,Sprite[]> cache=new Dictionary<string,Sprite[]>();
 public static Sprite[] Load(string name,int rows,float height,int referenceRow=0,int referenceColumn=0,int columns=8){
  string key=name+"/"+rows+"/"+height+"/"+referenceRow+"/"+referenceColumn+"/"+columns;if(cache.TryGetValue(key,out var cached)&&System.Array.TrueForAll(cached,frame=>frame))return cached;
  var texture=Resources.Load<Texture2D>("GameDuo/IceRefresh/"+name);if(!texture)return null;
  var rects=new Rect[rows*columns];var pivots=new Vector2[rects.Length];var pixels=texture.GetPixels32();
  float[] cuts={0,234/1086f,414/1086f,588/1086f,774/1086f,958/1086f,1};
  for(int i=0;i<rects.Length;i++){
   int x0=Mathf.RoundToInt(i%columns*texture.width/(float)columns),x1=Mathf.RoundToInt((i%columns+1)*texture.width/(float)columns);
   // Generated enemy rows have different padding; explicit cuts prevent wing/foot clipping.
   int y0=rows==6?Mathf.RoundToInt((1-cuts[i/columns+1])*texture.height):Mathf.RoundToInt((rows-1-i/columns)*texture.height/(float)rows);
   int y1=rows==6?Mathf.RoundToInt((1-cuts[i/columns])*texture.height):Mathf.RoundToInt((rows-i/columns)*texture.height/(float)rows);
   int left=x1,right=x0,bottom=y1,top=y0;
   for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*texture.width+x].a>38){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
   if(right<left||top<bottom){left=x0;right=x1-1;bottom=y0;top=y1-1;}
   rects[i]=new Rect(left,bottom,right-left+1,top-bottom+1);pivots[i]=new Vector2(Mathf.Clamp01(((x0+x1)*.5f-left)/rects[i].width),0);
  }
  float ppu=rects[referenceRow*columns+referenceColumn].height/height;
  var result=new Sprite[rects.Length];for(int i=0;i<result.Length;i++){result[i]=Sprite.Create(texture,rects[i],pivots[i],ppu,0,SpriteMeshType.FullRect);result[i].name=name+"_"+i;}
  cache[key]=result;return result;
 }
 void Start(){
  enemy=GetComponent<EnemyBase>();king=GetComponent<IceKingBoss>();brain=GetComponent<IceEnemyBrain>();source=enemy.sprite;health=enemy.Health;
  float height=king?3.6f:source.bounds.size.y;frames=Load(king?"KingAttackFrames":"EnemyExtraFrames",king?8:6,height,king?1:Row(),king?3:0,king?4:8);
  if(frames==null){enabled=false;return;}
  if(king){var texture=Resources.Load<Texture2D>("GameDuo/IceRefresh/King");originalKing=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,0),texture.height/height,0,SpriteMeshType.FullRect);originalKing.name="IceKing_OriginalIdle";}
  else {var animator=source.GetComponent<IceSpriteAnimator>();if(animator)originalFrames=animator.frames;}
  foreach(var motion in source.GetComponents<IllustratedMotion>())motion.enabled=false;
  current=Layer("Pose actual");previous=Layer("Transicion de pose");LateUpdate();source.enabled=false;
 }
 SpriteRenderer Layer(string name){var go=new GameObject(name);go.transform.SetParent(source.transform.parent,false);var r=go.AddComponent<SpriteRenderer>();r.sharedMaterial=source.sharedMaterial;r.sortingLayerID=source.sortingLayerID;r.sortingOrder=source.sortingOrder;return r;}
 int Row(){if(!brain)return 0;return brain.role==IceEnemyBrain.Role.Snowman?0:brain.role==IceEnemyBrain.Role.PolarBear?1:brain.role==IceEnemyBrain.Role.Penguin?2:brain.role==IceEnemyBrain.Role.Owl?3:brain.role==IceEnemyBrain.Role.Turret?4:5;}
 void LateUpdate(){
  if(frames==null||!source)return;float t=Time.time;
  if(enemy.Health<health)hurtUntil=t+.28f;health=enemy.Health;
  bool dead=enemy.Dead||king&&king.Defeated;
  int next=dead?4:t<hurtUntil?3:king?(king.Telegraphing?1:king.Attacking?2:0):(enemy.Telegraphing?1:enemy.Attacking?2:0);
  if(state!=next){state=next;stateAt=t;}float age=t-stateAt;int index;
  if(king){
   int row=king.PatternIndex*8;
   index=state==0&&!king.Vulnerable?-2:state==4?28:state==3?29:row+(state==1?Mathf.Min(1,(int)(age*2/1.3f)):state==2?Mathf.Min(7,2+(int)(age*10)):6);
  }else index=Row()*8+(state==1?4:state==2?5+Mathf.Min(1,(int)(age*8)):state==3||state==4?7:(int)(age*10)%4);
  Sprite pose=index==-2?originalKing:frames[index];
  // Keep every original locomotion drawing, adding four new in-between poses.
  if(!king&&state==0&&originalFrames!=null&&originalFrames.Length>=4){int step=(int)(age*10)%8;pose=step%2==0?originalFrames[step/2]:frames[Row()*8+step/2];index=100+step;}
  if(!king&&originalFrames!=null&&originalFrames.Length>=8){
   int original=state==4?7:state==3&&age<.1f?6:state==1&&age<.35f?4:state==2&&age<.08f?5:-1;
   if(original>=0){pose=originalFrames[original];index=200+original;}
  }
  if(index!=DisplayedFrame||!current.sprite){previous.sprite=current.sprite;current.sprite=pose;DisplayedFrame=index;blendAt=t;}
  float blend=previous.sprite?Mathf.SmoothStep(0,1,(t-blendAt)/.055f):1;
  Color tint=source.color;if(dead)tint.a*=1-Mathf.Clamp01((age-.7f)/1.2f);
  current.color=new Color(tint.r,tint.g,tint.b,tint.a*blend);previous.color=new Color(tint.r,tint.g,tint.b,tint.a*(1-blend));
  var collider=GetComponent<Collider2D>();float floor=collider&&collider.enabled?collider.bounds.min.y:current.transform.position.y;
  float breath=1+Mathf.Sin(t*3)*.008f,collapse=dead?1-Mathf.Clamp01(age)*.25f:1;
  float velocity=GetComponent<Rigidbody2D>().linearVelocity.x;if(Mathf.Abs(velocity)>.1f)facingLeft=velocity<0;
  foreach(var layer in new[]{current,previous}){
   layer.transform.position=new Vector3(transform.position.x,floor,source.transform.position.z);
   layer.transform.localScale=new Vector3(breath/Mathf.Abs(layer.transform.parent.lossyScale.x),collapse/breath/Mathf.Abs(layer.transform.parent.lossyScale.y),1);
   layer.flipX=!king&&facingLeft;
  }
  source.sprite=current.sprite;source.enabled=false;
 }
 void OnDestroy(){if(current)Destroy(current.gameObject);if(previous)Destroy(previous.gameObject);if(originalKing)Destroy(originalKing);}
}
}
