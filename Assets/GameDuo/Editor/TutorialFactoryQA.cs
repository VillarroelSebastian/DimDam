using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
[InitializeOnLoad]
public static class TutorialFactoryRunner {
 static TutorialFactoryRunner(){EditorApplication.delayCall+=HideTutorialGizmos;EditorSceneManager.sceneOpened+=(scene,mode)=>HideTutorialGizmos();EditorApplication.playModeStateChanged+=s=>{HideTutorialGizmos();if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("Factory.QA",false)){SessionState.SetBool("Factory.QA",false);Debug.Log("Throne QA: entered Play Mode");new GameObject("Tutorial QA").AddComponent<TutorialFactoryPlayQA>();}};}
 [MenuItem("GameDuo/Validar tutorial fabrica completo")]
 public static void Run(){EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.OpenScene(GameDuoCampaign.Scenes[0]);SessionState.SetBool("Factory.QA",true);EditorApplication.EnterPlaymode();}
 public static void HideTutorialGizmos(){
  if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=GameDuoCampaign.Scenes[0])return;
  foreach(SceneView view in SceneView.sceneViews)view.drawGizmos=false;
  foreach(var window in Resources.FindObjectsOfTypeAll<EditorWindow>())if(window.GetType().Name=="GameView"){
   var property=window.GetType().GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
   if(property!=null&&property.CanWrite)property.SetValue(window,false);
   window.Repaint();
  }
 }
 public static void VisualChecksBatch(){
  EditorSceneManager.OpenScene(GameDuoCampaign.Scenes[0]);
  var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
  if(type==null)throw new InvalidOperationException("GameView type unavailable");
  var view=EditorWindow.GetWindow(type);HideTutorialGizmos();
  var property=type.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
  if(property==null||(bool)property.GetValue(view))throw new InvalidOperationException("GameView gizmos remain visible");
  File.AppendAllText("Assets/GameDuo/TutorialFactoryQA.txt","PASS Game View hides audio and light gizmo icons\n");
  GameDuoValidation.Run();Debug.Log("Tutorial polish: editor gizmos disabled; all five scenes valid");EditorApplication.Exit(0);
 }
 public static void BakeBatch(){Bake();EditorApplication.Exit(0);}
 public static void Batch(){SessionState.SetBool("Factory.Batch",true);Run();}
 [MenuItem("GameDuo/Guardar fabrica en escena tutorial")]
 public static void Bake(){
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before saving factory art.");
  var scene=EditorSceneManager.OpenScene(GameDuoCampaign.Scenes[0]);
  foreach(var factory in UnityEngine.Object.FindObjectsByType<TutorialFactory>())UnityEngine.Object.DestroyImmediate(factory.gameObject);
  foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>())if(t&&t.name.StartsWith("Factory platform / "))UnityEngine.Object.DestroyImmediate(t.gameObject);
  for(int i=0;i<3;i++){
   var sprite=TutorialFactory.Shape(i);string path="Assets/GameDuo/Resources/GameDuo/FactoryShape"+i;
   if(!AssetDatabase.Contains(sprite.texture))AssetDatabase.CreateAsset(sprite.texture,path+"Texture.asset");
   if(!AssetDatabase.Contains(sprite))AssetDatabase.CreateAsset(sprite,path+".asset");
  }
  new GameObject("Tutorial / Fabrica de caramelos").AddComponent<TutorialFactory>().Build();
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Tutorial factory saved with editable scenery and collision-aligned platforms.");
 }
}
