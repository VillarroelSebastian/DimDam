using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
public static class PrototypeBuilder {
 const string Root="Assets/Prototype_CoopBeam/";static Material material;static Sprite dot,block;
 [MenuItem("Coop Beam/Build Prototype")]
 public static void Build(){
  if(EditorApplication.isPlaying)throw new Exception("Exit Play mode first");
  if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save the current scene before building the prototype");
  Directory.CreateDirectory(Root+"0_3_Art");Directory.CreateDirectory(Root+"0_2_Prefabs");Directory.CreateDirectory(Root+"0_0_Scenes");
  material=AssetDatabase.LoadAssetAtPath<Material>(Root+"0_3_Art/SpriteUnlit.mat");if(!material){material=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));AssetDatabase.CreateAsset(material,Root+"0_3_Art/SpriteUnlit.mat");}
  dot=Paint("mote",0,Color.white,true);block=Paint("tile",0,Color.white,false,true);
  var sprites=new Sprite[6];Color[] colors={Hex("FF736C"),Hex("68DAF2"),Hex("B4E975"),Hex("CA8EF4"),Hex("FFC75E"),Hex("809ABC")};
  for(int i=0;i<6;i++)sprites[i]=Paint(new[]{"coral","celeste","sprout","jelly","comet","shell"}[i],i,colors[i]);
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var cam=new GameObject("Prototype Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();cam.tag="MainCamera";cam.transform.position=new Vector3(0,1,-10);cam.orthographic=true;cam.orthographicSize=7.5f;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Hex("101B39");
  var fit=cam.gameObject.AddComponent<ArenaCamera>();
  var world=new GameObject("Moonlit Grove");
  for(int i=0;i<3;i++){var hill=Visual("Distant hill",dot,new Vector2((i-1)*9,-1+i*.5f),new Vector2(19,10),Hex(i==0?"193249":i==1?"204252":"244B58"),-25+i);hill.transform.parent=world.transform;}
  Visual("Moon glow",dot,new Vector2(6,5),new Vector2(3.2f,3.2f),new Color(.45f,.85f,.9f,.12f),-20);
  Visual("Moon",dot,new Vector2(6,5),new Vector2(1.7f,1.7f),Hex("D9FFF1"),-19);
  UnityEngine.Random.InitState(431);for(int i=0;i<65;i++)Visual("Firefly",dot,new Vector2(UnityEngine.Random.Range(-12f,12f),UnityEngine.Random.Range(-4f,7f)),Vector2.one*UnityEngine.Random.Range(.025f,.07f),new Color(.55f,1,.8f,.6f),-15);
  for(int side=-1;side<=1;side+=2){Visual("Ancient tree",block,new Vector2(side*11.8f,.3f),new Vector2(.8f,10),Hex("182F43"),-12);for(int i=0;i<5;i++)Visual("Tree canopy",dot,new Vector2(side*(10.5f+i*.3f),4.3f+i*.8f),new Vector2(4,3),Hex(i%2==0?"285968":"244853"),-11);}
  string backgroundPath=Root+"0_3_Art/MoonlitGrove.png";
  if(File.Exists(backgroundPath)){var importer=(TextureImporter)AssetImporter.GetAtPath(backgroundPath);importer.textureType=TextureImporterType.Sprite;importer.spritePixelsPerUnit=100;importer.mipmapEnabled=false;importer.SaveAndReimport();var background=AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);foreach(Transform child in world.transform)UnityEngine.Object.DestroyImmediate(child.gameObject);var image=Visual("Original illustrated grove",background,new Vector2(0,1),Vector2.one,Color.white,-10);image.transform.localScale=new Vector3(26/background.bounds.size.x,15/background.bounds.size.y,1);}
  Platform("Main ground",new Vector2(0,-4.7f),new Vector2(23,.8f),false);
  Platform("Left stepping island",new Vector2(-7,-2.1f),new Vector2(5,.45f),true);
  Platform("Right stepping island",new Vector2(7,-2.1f),new Vector2(5,.45f),true);
  Platform("Center island",new Vector2(0,.2f),new Vector2(5,.45f),true);
  Platform("Upper left island",new Vector2(-6,2.6f),new Vector2(4,.45f),true);
  Platform("Upper right island",new Vector2(6,2.6f),new Vector2(4,.45f),true);
  Wall(-11.7f);Wall(11.7f);
  var manager=new GameObject("Game - Waves - Score").AddComponent<GameManager>();manager.particle=dot;manager.players=new PlayerHealth[2];
  for(int i=0;i<2;i++){var g=new GameObject(i==0?"Coral":"Celeste");g.layer=11;g.transform.position=new Vector2(i==0?-4:4,-3.65f);
   var rb=g.AddComponent<Rigidbody2D>();rb.gravityScale=3;rb.freezeRotation=true;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;rb.interpolation=RigidbodyInterpolation2D.Interpolate;
   var col=g.AddComponent<CapsuleCollider2D>();col.size=new Vector2(.68f,1.02f);
   var v=Visual("Character art",sprites[i],Vector2.zero,new Vector2(1.15f,1.15f),Color.white,5);v.transform.SetParent(g.transform,false);
   var h=g.AddComponent<PlayerHealth>();h.spawn=g.transform.position;var pc=g.AddComponent<PlayerController2D>();pc.playerIndex=i;pc.visual=v.transform;pc.sprite=v.GetComponent<SpriteRenderer>();pc.health=h;manager.players[i]=h;
   PrefabUtility.SaveAsPrefabAsset(g,Root+"0_2_Prefabs/"+g.name+".prefab");
  }
  Physics2D.IgnoreCollision(manager.players[0].GetComponent<Collider2D>(),manager.players[1].GetComponent<Collider2D>(),true);
  manager.enemyPrefabs=new EnemyBase[4];for(int i=0;i<4;i++){
   var g=new GameObject(((EnemyKind)i).ToString());g.layer=10;var rb=g.AddComponent<Rigidbody2D>();rb.freezeRotation=true;rb.gravityScale=3;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;rb.interpolation=RigidbodyInterpolation2D.Interpolate;
   var col=g.AddComponent<CapsuleCollider2D>();col.size=i==3?new Vector2(1.3f,1.3f):new Vector2(.85f,.95f);
   var v=Visual("Enemy art",sprites[i+2],Vector2.zero,Vector2.one*(i==3?1.65f:1.15f),Color.white,4);v.transform.SetParent(g.transform,false);
   var e=g.AddComponent<EnemyBase>();e.kind=(EnemyKind)i;e.maxHealth=new[]{27,48,42,150}[i];e.speed=new[]{1.7f,2.7f,3.6f,.8f}[i];e.sprite=v.GetComponent<SpriteRenderer>();e.visual=v.transform;
   manager.enemyPrefabs[i]=PrefabUtility.SaveAsPrefabAsset(g,Root+"0_2_Prefabs/"+g.name+".prefab").GetComponent<EnemyBase>();UnityEngine.Object.DestroyImmediate(g);
  }
  var beam=new GameObject("Cooperative beam").AddComponent<CoopBeam.CoopBeam>();beam.red=manager.players[0];beam.blue=manager.players[1];beam.halo=Line("Soft halo",beam.transform,.24f,6);beam.core=Line("Luminous core",beam.transform,.055f,7);
  var ui=new GameObject("Arcade HUD").AddComponent<UIManager>();ui.game=manager;
  EditorSceneManager.SaveScene(scene,Root+"0_0_Scenes/Prototype_CoopBeam.unity");AssetDatabase.SaveAssets();Debug.Log("COOP_BUILD_OK: scene, 6 prefabs and original sprites saved.");
 }
 static Color Hex(string hex){ColorUtility.TryParseHtmlString("#"+hex,out Color c);return c;}
 static GameObject Visual(string name,Sprite sprite,Vector2 pos,Vector2 size,Color color,int order){var g=new GameObject(name);g.transform.position=pos;g.transform.localScale=new Vector3(size.x,size.y,1);var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.color=color;r.sharedMaterial=material;r.sortingOrder=order;return g;}
 static void Platform(string name,Vector2 pos,Vector2 size,bool semi){var g=Visual(name,block,pos,size,Hex("42536F"),0);g.layer=semi?9:8;var c=g.AddComponent<BoxCollider2D>();if(semi){c.usedByEffector=true;var e=g.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.useOneWayGrouping=true;e.surfaceArc=165;}
  var rim=Visual("Mint moss rim",block,pos+Vector2.up*(size.y*.5f-.04f),new Vector2(size.x,.12f),Hex("78DDB0"),1);rim.transform.SetParent(g.transform,true);
  for(float x=-size.x*.5f+.3f;x<size.x*.5f;x+=.65f){var tuft=Visual("Moss bead",dot,pos+new Vector2(x,size.y*.5f),new Vector2(.28f,.16f),Hex("A3E8BB"),2);tuft.transform.SetParent(g.transform,true);}
 }
 static void Wall(float x){var g=new GameObject("Arena boundary");g.layer=8;g.transform.position=new Vector2(x,1);g.AddComponent<BoxCollider2D>().size=new Vector2(.5f,13);}
 static LineRenderer Line(string name,Transform parent,float width,int order){var g=new GameObject(name);g.transform.parent=parent;var l=g.AddComponent<LineRenderer>();l.sharedMaterial=material;l.positionCount=2;l.useWorldSpace=true;l.widthMultiplier=width;l.numCapVertices=8;l.sortingOrder=order;return l;}
 static Sprite Paint(string name,int kind,Color color,bool circle=false,bool square=false){
  const int n=256;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);var pixels=new Color[n*n];Color ink=Hex("222C45");
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){
   float u=(x+.5f)/n*2-1,v=(y+.5f)/n*2-1;Color c=Color.clear;
   if(square)c=Color.white;else if(circle){float d=Mathf.Sqrt(u*u+v*v);if(d<.98f)c=new Color(1,1,1,Mathf.Clamp01((.98f-d)*35));}
   else {
    Func<float,float,float,float,float,bool> ell=(cx,cy,rx,ry,pad)=>Mathf.Pow((u-cx)/(rx+pad),2)+Mathf.Pow((v-cy)/(ry+pad),2)<1;
    bool body=ell(0,-.16f,kind==5?.79f:.62f,.61f,0);bool ears=kind<2?(ell(-.39f,.51f,.17f,.4f,0)||ell(.39f,.51f,.17f,.4f,0)):kind==2?(ell(-.18f,.54f,.15f,.27f,0)||ell(.15f,.55f,.24f,.14f,0)):kind==3?(ell(-.45f,.35f,.18f,.3f,0)||ell(.45f,.35f,.18f,.3f,0)):kind==4?ell(-.6f,.27f,.28f,.16f,0):ell(0,.27f,.72f,.48f,0);
    bool feet=ell(-.35f,-.73f,.24f,.16f,0)||ell(.35f,-.73f,.24f,.16f,0);
    if(body||ears||feet)c=ink;
    bool inner=ell(0,-.16f,kind==5?.74f:.57f,.56f,0);if(inner)c=Color.Lerp(color,Color.white,Mathf.Clamp01((v+.7f)*.22f));
    if(ears&&v>.4f)c=Color.Lerp(color,ink,.1f);
    if(kind==5&&v>.1f&&ell(0,.27f,.65f,.41f,0))c=Hex("556B92");
    if(ell(-.23f,.04f,.15f,.22f,0)||ell(.23f,.04f,.15f,.22f,0))c=Color.white;
    if(ell(-.2f,.01f,.075f,.13f,0)||ell(.26f,.01f,.075f,.13f,0))c=ink;
    if(ell(-.22f,.075f,.025f,.04f,0)||ell(.24f,.075f,.025f,.04f,0))c=Color.white;
    if(ell(-.4f,-.2f,.1f,.055f,0)||ell(.4f,-.2f,.1f,.055f,0))c=Color.Lerp(color,Hex("FFB3B5"),.7f);
    if(ell(0,-.28f,.1f,.065f,0))c=ink;
    if(kind==5&&ell(0,-.52f,.28f,.12f,0))c=Hex("D8E69C");
   }pixels[y*n+x]=c;
  }
  tex.SetPixels(pixels);tex.Apply();string path=Root+"0_3_Art/"+name+".png";File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spritePixelsPerUnit=256;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
 }
}

