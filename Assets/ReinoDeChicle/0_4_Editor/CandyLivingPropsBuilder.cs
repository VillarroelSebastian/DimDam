using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using CoopBeam;
public static class CandyLivingPropsBuilder {
 public static void Apply(){string folder=CandyAnimationPolish.Folder;string png=folder+"LivingProps_Sheet.png";AssetDatabase.ImportAsset(png);var importer=(TextureImporter)AssetImporter.GetAtPath(png);importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(png);
  string path=folder+"LivingProps_Animation.asset";var set=AssetDatabase.LoadAssetAtPath<PixelAnimationSet>(path);if(!set){set=ScriptableObject.CreateInstance<PixelAnimationSet>();AssetDatabase.CreateAsset(set,path);}if(set.frames==null||set.frames.Length!=12){set.frames=new Sprite[12];int w=texture.width/4,h=texture.height/3;for(int i=0;i<12;i++){var frame=Sprite.Create(texture,new Rect(i%4*w,(2-i/4)*h,w,h),new Vector2(.5f,.05f),Mathf.Max(w,h)/2.3f,0,SpriteMeshType.FullRect);frame.name="Scenery_"+i;AssetDatabase.AddObjectToAsset(frame,set);set.frames[i]=frame;}EditorUtility.SetDirty(set);AssetDatabase.SaveAssets();}
  var material=AssetDatabase.LoadAssetAtPath<Material>(CandyPixelStyle.Art+"PixelCharacters.mat");
  for(int level=1;level<=5;level++){var scene=EditorSceneManager.OpenScene(CandyCampaign.ScenePath(level));var old=GameObject.Find("Animated candy props");if(old)Object.DestroyImmediate(old);var root=new GameObject("Animated candy props");int row=level==3?1:level==5?2:0;for(int side=-1;side<=1;side+=2){var g=new GameObject(row==0?"Breathing candy blossoms":row==1?"Working candy pistons":"Flowing caramel fountain");g.transform.parent=root.transform;g.transform.position=new Vector3(side*9.7f,-4.32f,0);g.transform.localScale=new Vector3(side==1?-1:1,1,1);var art=g.AddComponent<SpriteRenderer>();art.sharedMaterial=material;art.sortingOrder=-2;art.sprite=set.frames[row*4];var animation=g.AddComponent<PixelSurfaceAnimator>();animation.frames=new Sprite[4];for(int i=0;i<4;i++)animation.frames[i]=set.frames[row*4+i];animation.framesPerSecond=row==1?7:5;}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
  EditorSceneManager.OpenScene(CandyCampaign.ScenePath(1));
 }
}
