using UnityEngine;
namespace CoopBeam {
// A private, continuously deformed sprite mesh. Physics and the exposed core stay independent.
[DefaultExecutionOrder(1300)]
public sealed class GameDuoIceKingMotion:MonoBehaviour {
 const int Grid=24;const float Height=3.6f;
 Sprite sprite;SpriteRenderer art;EnemyBase enemy;IceKingBoss king;Mesh mesh;MeshRenderer skin;Material material;Vector3[] meshVertices;
 Vector2[] rest,vertices;ushort[] triangles;float wind,attack,open,hurt,death,health,attackAge;bool wasAttacking;
 void Start(){
  enemy=GetComponent<EnemyBase>();king=GetComponent<IceKingBoss>();art=enemy.sprite;health=enemy.Health;
  if(Resources.Load<Texture2D>("GameDuo/IceRefresh/KingAttackFrames")){gameObject.AddComponent<GameDuoIcePoseFrames>();enabled=false;return;}
  var texture=Resources.Load<Texture2D>("GameDuo/IceRefresh/King");
  sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,0),texture.height/Height,0,SpriteMeshType.FullRect);sprite.name="IceKing_FluidMesh";
  rest=new Vector2[(Grid+1)*(Grid+1)];vertices=new Vector2[rest.Length];triangles=new ushort[Grid*Grid*6];float width=Height*texture.width/texture.height;
  for(int y=0;y<=Grid;y++)for(int x=0;x<=Grid;x++)rest[y*(Grid+1)+x]=new Vector2((x/(float)Grid-.5f)*width,y/(float)Grid*Height);
  int n=0;for(int y=0;y<Grid;y++)for(int x=0;x<Grid;x++){int a=y*(Grid+1)+x,b=a+1,c=a+Grid+1,d=c+1;triangles[n++]=(ushort)a;triangles[n++]=(ushort)c;triangles[n++]=(ushort)b;triangles[n++]=(ushort)b;triangles[n++]=(ushort)c;triangles[n++]=(ushort)d;}
  sprite.OverrideGeometry(rest,triangles);art.sprite=sprite;
  mesh=new Mesh();mesh.name="Ice King / deformable skin";mesh.MarkDynamic();meshVertices=new Vector3[rest.Length];var uv=new Vector2[rest.Length];
  for(int i=0;i<rest.Length;i++){meshVertices[i]=rest[i];uv[i]=new Vector2(rest[i].x/width+.5f,rest[i].y/Height);}
  var colors=new Color[rest.Length];for(int i=0;i<colors.Length;i++)colors[i]=Color.white;
  var indices=new int[triangles.Length];for(int i=0;i<indices.Length;i++)indices[i]=triangles[i];mesh.vertices=meshVertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=indices;mesh.RecalculateBounds();
  var go=new GameObject("Rey / piel animada");go.transform.SetParent(art.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;skin=go.AddComponent<MeshRenderer>();material=new Material(Shader.Find("Sprites/Default"));material.mainTexture=texture;skin.sharedMaterial=material;skin.sortingOrder=art.sortingOrder;skin.sortingLayerID=art.sortingLayerID;art.enabled=false;
 }
 static float Follow(float value,float target,float dt)=>Mathf.Lerp(value,target,1-Mathf.Exp(-12*dt));
 void LateUpdate(){
  if(!sprite||!art)return;float dt=Time.deltaTime,t=Time.time;
  if(enemy.Health<health)hurt=1;health=enemy.Health;hurt=Mathf.MoveTowards(hurt,0,dt*4);
  wind=Follow(wind,king.Telegraphing?1:0,dt);attack=Follow(attack,king.Attacking?1:0,dt);open=Follow(open,king.Vulnerable?1:0,dt);death=Follow(death,king.Defeated?1:0,dt);
  if(king.Attacking&&!wasAttacking)attackAge=0;attackAge+=dt;wasAttacking=king.Attacking;
  float pulse=Mathf.Exp(-attackAge*5)*attack;
  for(int i=0;i<rest.Length;i++){
   Vector2 p=rest[i];float y=p.y/Height,x=p.x/(Height*.5f),side=Mathf.Sign(x);
   float arm=Mathf.SmoothStep(.25f,.75f,Mathf.Abs(x))*(1-Mathf.SmoothStep(.72f,.9f,y))*Mathf.SmoothStep(.05f,.28f,y);
   float head=Mathf.SmoothStep(.55f,.85f,y),foot=1-Mathf.SmoothStep(.02f,.22f,y);
   float breath=Mathf.Sin(t*2.6f);p.x+=x*.025f*breath*(1-foot);
   p.y+=.028f*breath*(1-foot)+arm*.045f*Mathf.Sin(t*3+side*.7f);
   // Anticipation raises the shoulders, release differs for each actual combat pattern.
   p.y+=wind*(arm*.27f-head*.055f);p.x-=side*arm*wind*.09f;
   float lift=king.PatternIndex==1?.36f:king.PatternIndex==3?-.22f:.10f;
   p.y+=arm*attack*(lift+Mathf.Sin(t*7+side)*.035f)-head*pulse*.13f;
   p.x+=arm*side*pulse*(king.PatternIndex==2?.26f:.13f);
   p.y-=open*(head*.12f+arm*.08f);p.x+=hurt*Mathf.Sin(t*42)*.045f*y;
   p.y*=1-death*.32f;p.x+=death*y*.28f;
   // UVs remain fixed while the mesh bends; the feet stay anchored to the collider.
   vertices[i]=p;
  }
  for(int i=0;i<vertices.Length;i++)meshVertices[i]=vertices[i];mesh.vertices=meshVertices;mesh.RecalculateBounds();var tint=art.color;if(king.Defeated)tint.a*=1-Mathf.Clamp01((king.DefeatAge-1)/1.4f);material.color=tint;art.enabled=false;art.sprite=sprite;art.flipX=false;
  float scale=1/Mathf.Abs(transform.lossyScale.y);art.transform.localScale=new Vector3(scale,scale,1);art.transform.localRotation=Quaternion.identity;
  var shape=GetComponent<Collider2D>();if(shape&&shape.enabled)art.transform.position=new Vector3(transform.position.x,shape.bounds.min.y,art.transform.position.z);
 }
 void OnDestroy(){if(sprite)Destroy(sprite);if(mesh)Destroy(mesh);if(material)Destroy(material);}
}
}
