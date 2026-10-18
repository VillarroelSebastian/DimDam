#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
public static class IceRescueArtSetup {
 const string Art="Assets/Minigames/IceRescue/Art/";
 [MenuItem("Coop Beam/Ice Rescue/Apply Art")]
 public static void Apply(){
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before applying art.");
  Import("PolarRefuge.png",false);Import("PenguinWaddle.png",true);
  var scene=EditorSceneManager.OpenScene(IcePenguinRound.ScenePath);
  var round=UnityEngine.Object.FindFirstObjectByType<IcePenguinRound>();
  round.arenaArt=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"PolarRefuge.png");
  var sheet=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"PenguinWaddle.png");round.penguinFrames=new Sprite[4];
  for(int i=0;i<4;i++){float w=sheet.width*.5f,h=sheet.height*.5f;var sprite=Sprite.Create(sheet,new Rect((i%2)*w,(1-i/2)*h,w,h),new Vector2(.5f,.17f),w/1.6f,0,SpriteMeshType.FullRect);sprite.name="PenguinWaddle_"+i;string path=Art+"PenguinFrame_"+i+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing){EditorUtility.CopySerialized(sprite,existing);UnityEngine.Object.DestroyImmediate(sprite);round.penguinFrames[i]=existing;}else{AssetDatabase.CreateAsset(sprite,path);round.penguinFrames[i]=sprite;}}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
 }
 static void Import(string file,bool sheet){
  string path=Art+file;var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
  importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=sheet?SpriteImportMode.Multiple:SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;importer.spritePixelsPerUnit=sheet?texture.width*.5f/1.6f:100;
  // Reimport before measuring: the default importer can resize non-power-of-two images.
  importer.SaveAndReimport();texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);importer=(TextureImporter)AssetImporter.GetAtPath(path);
  if(sheet){int width,height;importer.GetSourceTextureWidthAndHeight(out width,out height);importer.spritePixelsPerUnit=width*.5f/1.6f;float w=width*.5f,h=height*.5f;var frames=new SpriteMetaData[4];for(int i=0;i<4;i++)frames[i]=new SpriteMetaData{name="PenguinWaddle_"+i,rect=new Rect((i%2)*w,(1-i/2)*h,w,h),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,.17f)};
#pragma warning disable 0618
   importer.spritesheet=frames;
#pragma warning restore 0618
  }
  importer.SaveAndReimport();
 }
}
#endif
