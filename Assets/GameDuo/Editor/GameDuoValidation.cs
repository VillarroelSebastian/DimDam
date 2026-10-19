using System;
using System.IO;
using System.Linq;
using CoopBeam;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameDuoValidation {
 [MenuItem("GameDuo/Validar progresion")]
 public static void Run(){
  string original=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
  int checks=0;
  try {
   var controlHeroes=Resources.Load<PixelAnimationSet>("GameDuoControlHeroes");
   Require(controlHeroes&&controlHeroes.frames.Length==2&&controlHeroes.frames.All(s=>s&&s.texture),"Menu controls include both original hero sprites");checks++;
   for(int i=0;i<GameDuoCampaign.Scenes.Length;i++){
    string path=GameDuoCampaign.Scenes[i];
    Require(File.Exists(path),"Scene exists: "+path);checks++;
    Require(EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path==path),"Scene in build: "+path);checks++;
    var scene=EditorSceneManager.OpenScene(path);
    Require(scene.GetRootGameObjects().All(g=>g.GetComponentsInChildren<Transform>(true).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0)),"No missing scripts: "+path);checks++;
    var game=UnityEngine.Object.FindAnyObjectByType<GameManager>();
    if(i!=3){Require(game&&game.players.Length==2&&game.players.All(p=>p&&p.GetComponent<PlayerController2D>()),"Two heroes: "+path);checks++;}
    if(i==0||i==2||i==4){Require(game.level&&game.level.waves.Length==3&&game.level.waves.All(w=>w.enemyTypes.All(t=>t>=0&&t<game.enemyPrefabs.Length&&game.enemyPrefabs[t])),"Valid combat waves: "+path);checks++;}
    if(i==1)Require(UnityEngine.Object.FindAnyObjectByType<NeonLevel>(),"Neon completion source");
    if(i==3)Require(UnityEngine.Object.FindAnyObjectByType<IcePenguinRound>(),"Penguin completion source");
   }
   Require(EditorBuildSettings.scenes.First(s=>s.enabled).path==GameDuoMainMenu.ScenePath,"Menu starts the build");checks++;
   File.WriteAllText("Assets/GameDuo/Validation.txt","PASS: "+checks+" scene/build checks; all five completion sources present.\nGameplay traversal requires Play Mode.\n");
   Debug.Log("GameDuo validation passed: "+checks+" checks");
  }finally{if(!string.IsNullOrEmpty(original))EditorSceneManager.OpenScene(original);}
 }
 static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}
