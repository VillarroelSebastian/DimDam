using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
public static class CandyAnimationPolish {
 public const string Folder=CandyPixelStyle.Art+"Animations/";
 static readonly string[] Names={"Coral","Celeste","CandyWalker","GummyJumper","ToffeeChaser","CocoaGuard","FlyingGummy","SugarCannon","KingBubble"};
 public static bool Available {get{foreach(var name in Names)if(!File.Exists(Folder+name+"_Sheet.png"))return false;return true;}}
 [MenuItem("Coop Beam/Apply Sprite Animation and Living World")]
 public static void Apply(){if(EditorApplication.isPlaying)throw new Exception("Exit Play first");if(!Available)throw new Exception("Animation sheets are not all present");EditorSceneManager.SaveOpenScenes();
  for(int i=0;i<Names.Length;i++){string path=i<2?"Assets/Prototype_CoopBeam/0_2_Prefabs/"+Names[i]+".prefab":"Assets/ReinoDeChicle/0_2_Prefabs/"+Names[i]+".prefab";var root=PrefabUtility.LoadPrefabContents(path);try{
   var pc=root.GetComponent<PlayerController2D>();var enemy=root.GetComponent<EnemyBase>();var visual=pc?pc.visual:enemy.visual;var renderer=visual.GetComponent<SpriteRenderer>();
   float foot=i==8?1.4f:root.GetComponent<CapsuleCollider2D>().size.y*.5f;var set=BuildSet(i==0?"Mora":i==1?"Lima":Names[i],i<2,foot,visual.localPosition.y,visual.localScale.y);
   var animation=visual.GetComponent<PixelSpriteAnimator>();if(!animation)animation=visual.gameObject.AddComponent<PixelSpriteAnimator>();animation.animationSet=set;renderer.sprite=set.frames[0];
   if(!root.GetComponent<ActorPhysics2D>())root.AddComponent<ActorPhysics2D>();PrefabUtility.SaveAsPrefabAsset(root,path);
  }finally{PrefabUtility.UnloadPrefabContents(root);}}
  var belt=new Sprite[4];var jelly=new Sprite[4];var gears=new Sprite[8];for(int f=0;f<4;f++){belt[f]=SurfaceFrame(false,f);jelly[f]=SurfaceFrame(true,f);}for(int f=0;f<8;f++)gears[f]=Gear(f);var cloud=Cloud();var bubble=Bubble();var spark=AssetDatabase.LoadAssetAtPath<Sprite>(CandyPixelStyle.Art+"Tiles/PixelSpark.png");
  for(int level=1;level<=5;level++){var scene=EditorSceneManager.OpenScene(CandyCampaign.ScenePath(level));var life=UnityEngine.Object.FindAnyObjectByType<CandyAmbientLife>();if(!life)life=new GameObject("Living candy scenery").AddComponent<CandyAmbientLife>();var game=UnityEngine.Object.FindAnyObjectByType<GameManager>();var exit=game.GetComponent<CandyLevelExit>();if(!exit)exit=game.gameObject.AddComponent<CandyLevelExit>();exit.game=game;var hud=UnityEngine.Object.FindAnyObjectByType<CandyPixelHUD>();hud.exit=exit;game.players[0].name="Mora - Purple Drop";game.players[1].name="Lima - Green Drop";life.levelNumber=level;life.spark=spark;life.cloud=cloud;life.bubble=bubble;life.gears=gears;
   foreach(var surface in UnityEngine.Object.FindObjectsByType<CandyEnvironment>())if(surface.kind==CandySurfaceKind.Conveyor||surface.kind==CandySurfaceKind.Bounce){var animator=surface.art.GetComponent<PixelSurfaceAnimator>();if(!animator)animator=surface.art.gameObject.AddComponent<PixelSurfaceAnimator>();animator.frames=surface.kind==CandySurfaceKind.Conveyor?belt:jelly;animator.framesPerSecond=surface.kind==CandySurfaceKind.Conveyor?8:5;}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  }
  AssetDatabase.SaveAssets();if(File.Exists(Folder+"LivingProps_Sheet.png"))CandyLivingPropsBuilder.Apply();EditorSceneManager.OpenScene(CandyCampaign.ScenePath(1));Debug.Log("CANDY_ANIMATION_POLISH_OK: 96 sprite frames, frictionless actors, living environments.");
 }
 static PixelAnimationSet BuildSet(string name,bool hero,float halfHeight,float offset,float scale){string png=Folder+name+"_Sheet.png";var importer=(TextureImporter)AssetImporter.GetAtPath(png);importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(png);int columns=4,rows=hero?5:2,w=texture.width/columns,h=texture.height/rows;string path=Folder+name+"_Animation.asset";var set=AssetDatabase.LoadAssetAtPath<PixelAnimationSet>(path);
  if(set&&set.frames!=null&&set.frames.Length==columns*rows&&set.frames[0]&&set.frames[0].texture==texture&&set.frames[0].rect.width==w&&set.frames[0].rect.height==h)return set;
  if(!set){set=ScriptableObject.CreateInstance<PixelAnimationSet>();AssetDatabase.CreateAsset(set,path);}else foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path))if(asset is Sprite)UnityEngine.Object.DestroyImmediate(asset,true);
  var raw=new Texture2D(2,2,TextureFormat.RGBA32,false);raw.LoadImage(File.ReadAllBytes(png));var pixels=raw.GetPixels32();float ppu=Mathf.Max(w,h)/(hero?1.3f:1);set.hero=hero;set.idleFPS=hero?3:5;set.runFPS=hero?12:9;set.frames=new Sprite[columns*rows];
  for(int index=0;index<set.frames.Length;index++){int x=index%columns*w,y=(rows-1-index/columns)*h;int bottom=h/2,count=0;bool found=false;for(int yy=0;yy<h;yy++)for(int xx=0;xx<w;xx++)if(pixels[(y+yy)*raw.width+x+xx].a>40){count++;if(!found){bottom=yy;found=true;}}
   if(count<30)throw new Exception(name+" frame "+index+" is empty; inspect sprite-sheet grid");float pivotY=(bottom+(halfHeight+offset)*ppu/Mathf.Max(.1f,scale))/h;if(!hero&&name=="FlyingGummy")pivotY=.5f;
   var frame=Sprite.Create(texture,new Rect(x,y,w,h),new Vector2(.5f,Mathf.Clamp01(pivotY)),ppu,0,SpriteMeshType.FullRect);frame.name=name+"_Frame_"+index.ToString("00");AssetDatabase.AddObjectToAsset(frame,set);set.frames[index]=frame;
  }
  UnityEngine.Object.DestroyImmediate(raw);EditorUtility.SetDirty(set);AssetDatabase.SaveAssets();return set;
 }
 static Color Hex(string hex){ColorUtility.TryParseHtmlString("#"+hex,out Color c);return c;}
 static Sprite Save(string name,Texture2D texture,int ppu){texture.Apply();string path=CandyPixelStyle.Art+"Tiles/"+name+".png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);}
 static Sprite SurfaceFrame(bool jelly,int frame){var t=new Texture2D(32,16,TextureFormat.RGBA32,false);for(int y=0;y<16;y++)for(int x=0;x<32;x++){Color c=Hex(jelly?"4EBFA4":"AF7F54");if(y<2)c=Hex("39253E");else if(y>=12+x/4%2)c=Hex("FFF0CE");else if(!jelly&&(x+y+frame*3)%12<4)c=Hex("4F344B");else if(jelly&&y==9&&(x+frame*2)%8<5)c=Hex("94EBC5");t.SetPixel(x,y,c);}return Save((jelly?"JellyLive_":"BeltLive_")+frame,t,32);}
 static Sprite Gear(int frame){var t=new Texture2D(32,32,TextureFormat.RGBA32,false);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float dx=x-15.5f,dy=y-15.5f,r=Mathf.Sqrt(dx*dx+dy*dy),a=Mathf.Atan2(dy,dx)+frame*Mathf.PI/32;float edge=Mathf.Cos(a*8)>0?15:12;Color c=r<edge&&r>5?Hex(r>10?"9C6849":"F4CD85"):Color.clear;if(r<3)c=Hex("67425B");t.SetPixel(x,y,c);}return Save("SugarGear_"+frame,t,32);}
 static Sprite Cloud(){var t=new Texture2D(32,12,TextureFormat.RGBA32,false);for(int y=0;y<12;y++)for(int x=0;x<32;x++){bool fill=(x-8)*(x-8)+(y-5)*(y-5)<30||(x-16)*(x-16)+(y-6)*(y-6)<49||(x-24)*(x-24)+(y-4)*(y-4)<25;t.SetPixel(x,y,fill?Color.white:Color.clear);}return Save("SugarCloud",t,16);}
 static Sprite Bubble(){var t=new Texture2D(16,16,TextureFormat.RGBA32,false);for(int y=0;y<16;y++)for(int x=0;x<16;x++){float r=Vector2.Distance(new Vector2(x,y),new Vector2(7.5f,7.5f));t.SetPixel(x,y,r<7?new Color(1,1,1,r>5?1:.15f):Color.clear);}return Save("GumBubble",t,16);}
}
