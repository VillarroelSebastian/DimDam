#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
public static class MeltTogetherPlayerArt {
 const string Root="Assets/Characters/MeltTogether2D/Art/";
 const string Atlas=Root+"MimoPipo_Sprites_v1.png";
 [MenuItem("Coop Beam/Characters/Apply Mimo and Pipo")]
 public static void Apply(){
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before updating character assets.");
  var previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(previous.isDirty)throw new InvalidOperationException("Save the open scene first.");
  var importer=(TextureImporter)AssetImporter.GetAtPath(Atlas);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.isReadable=true;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=4096;importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Atlas);var pixels=texture.GetPixels32();var all=new Sprite[2][];
  if(!AssetDatabase.IsValidFolder(Root+"Frames"))AssetDatabase.CreateFolder(Root.TrimEnd('/'),"Frames");
  float[] rows={0,.29f,.56f,.795f,1};
  for(int player=0;player<2;player++){
   all[player]=new Sprite[8];for(int pose=0;pose<8;pose++){int row=player*2+pose/4,col=pose%4;int x0=Mathf.RoundToInt(texture.width*col/4f),x1=Mathf.RoundToInt(texture.width*(col+1)/4f);int y0=Mathf.RoundToInt(texture.height*(1-rows[row+1])),y1=Mathf.RoundToInt(texture.height*(1-rows[row]));var rect=LargestShape(pixels,texture.width,x0,y0,x1,y1);var sprite=Sprite.Create(texture,rect,Vector2.one*.5f,rect.height/1.3f,0,SpriteMeshType.FullRect);sprite.name=(player==0?"Mimo":"Pipo")+"_"+pose;string path=Root+"Frames/"+sprite.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing){EditorUtility.CopySerialized(sprite,existing);UnityEngine.Object.DestroyImmediate(sprite);all[player][pose]=existing;}else{AssetDatabase.CreateAsset(sprite,path);all[player][pose]=sprite;}}
  }
  var sets=new PixelAnimationSet[2];for(int i=0;i<2;i++){string path=PrefabPath(i);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);sets[i]=prefab.GetComponentInChildren<PixelSpriteAnimator>(true).animationSet;var poses=all[i];
   // Preserve the existing 20-frame animation contract and all animator state transitions.
   int[] map={0,0,0,0,4,5,6,7,5,5,7,0,6,0,0,0,0,2,0,3};sets[i].frames=map.Select(n=>poses[n]).ToArray();sets[i].directions=poses.Take(4).ToArray();sets[i].hero=true;sets[i].jellyStyle=true;sets[i].idleFPS=4;sets[i].runFPS=10;EditorUtility.SetDirty(sets[i]);
   var root=PrefabUtility.LoadPrefabContents(path);try{Patch(root.GetComponent<PlayerController2D>(),sets[i]);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}}
  int sceneCount=0,playerCount=0;
  foreach(string path in EditorBuildSettings.scenes.Select(s=>s.path).Distinct()){
   var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);bool opened=!scene.IsValid()||!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);bool changed=false;
   foreach(var root in scene.GetRootGameObjects())foreach(var p in root.GetComponentsInChildren<PlayerController2D>(true)){if(p.playerIndex<0||p.playerIndex>1)continue;Patch(p,sets[p.playerIndex]);playerCount++;changed=true;}
   foreach(var root in scene.GetRootGameObjects())foreach(var r in root.GetComponentsInChildren<IcePenguinRound>(true)){r.mimoFrames=sets[0].frames.Skip(4).Take(4).ToArray();r.pipoFrames=sets[1].frames.Skip(4).Take(4).ToArray();r.mimoDirections=sets[0].directions;r.pipoDirections=sets[1].directions;EditorUtility.SetDirty(r);changed=true;}
   if(changed){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);sceneCount++;}if(opened)EditorSceneManager.CloseScene(scene,true);
  }
  AssetDatabase.SaveAssets();if(previous.IsValid()&&previous.isLoaded)UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
  Debug.Log("MELT_TOGETHER_ART_OK: "+playerCount+" players, "+sceneCount+" scenes, 20 animation slots per player. Physics unchanged.");
 }
 static string PrefabPath(int i)=>"Assets/Prototype_CoopBeam/0_2_Prefabs/"+(i==0?"Coral":"Celeste")+".prefab";
 static void Patch(PlayerController2D p,PixelAnimationSet set){if(!p||!p.visual||!p.sprite)return;var animator=p.sprite.GetComponent<PixelSpriteAnimator>();if(!animator)animator=p.sprite.gameObject.AddComponent<PixelSpriteAnimator>();animator.animationSet=set;if(!p.sprite.GetComponent<PixelActorVisual>())p.sprite.gameObject.AddComponent<PixelActorVisual>();p.sprite.sprite=set.frames[0];p.sprite.color=Color.white;p.sprite.drawMode=SpriteDrawMode.Simple;
  var shape=p.GetComponent<CapsuleCollider2D>();if(shape){var v=p.visual.localPosition;v.y=shape.offset.y-shape.size.y*.5f+set.frames[0].bounds.extents.y;p.visual.localPosition=v;}EditorUtility.SetDirty(animator);EditorUtility.SetDirty(p.sprite);EditorUtility.SetDirty(p.visual);if(PrefabUtility.IsPartOfPrefabInstance(p)){PrefabUtility.RecordPrefabInstancePropertyModifications(animator);PrefabUtility.RecordPrefabInstancePropertyModifications(p.sprite);PrefabUtility.RecordPrefabInstancePropertyModifications(p.visual);}}
 // Ignore isolated translucent generation specks: crop to the largest opaque connected shape.
 static Rect LargestShape(Color32[] pixels,int width,int x0,int y0,int x1,int y1){int w=x1-x0,h=y1-y0;var visited=new bool[w*h];var queue=new int[w*h];int best=0,bx0=x0,by0=y0,bx1=x1-1,by1=y1-1;
  for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++){int start=(y-y0)*w+x-x0;if(visited[start]||pixels[y*width+x].a<180)continue;int read=0,count=1;queue[0]=start;visited[start]=true;int minx=x,maxx=x,miny=y,maxy=y;while(read<count){int index=queue[read++],px=x0+index%w,py=y0+index/w;minx=Math.Min(minx,px);maxx=Math.Max(maxx,px);miny=Math.Min(miny,py);maxy=Math.Max(maxy,py);for(int d=0;d<4;d++){int nx=px+(d==0?-1:d==1?1:0),ny=py+(d==2?-1:d==3?1:0);if(nx<x0||nx>=x1||ny<y0||ny>=y1)continue;int next=(ny-y0)*w+nx-x0;if(visited[next]||pixels[ny*width+nx].a<180)continue;visited[next]=true;queue[count++]=next;}}if(count>best){best=count;bx0=minx;bx1=maxx;by0=miny;by1=maxy;}}
  if(best<100)throw new InvalidOperationException("No character silhouette found in atlas cell.");bx0=Math.Max(x0,bx0-2);by0=Math.Max(y0,by0-2);bx1=Math.Min(x1-1,bx1+2);by1=Math.Min(y1-1,by1+2);return new Rect(bx0,by0,bx1-bx0+1,by1-by0+1);
 }
}
#endif
