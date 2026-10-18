using System.Collections;
using System.Collections.Generic;
using System.IO;
using CoopBeam;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class GameDuoPolishRunner {
 const string Requested="GameDuo.PolishQA.Requested";
 static GameDuoPolishRunner(){EditorApplication.playModeStateChanged+=Changed;}
 [MenuItem("GameDuo/Probar proporciones, saltos y danza")]
 public static void Run(){
  if(EditorApplication.isPlaying||EditorApplication.isCompiling)return;
  if(!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  GameDuoValidation.Run();
  SessionState.SetBool(Requested,true);EditorApplication.EnterPlaymode();
 }
 static void Changed(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Requested,false))return;SessionState.SetBool(Requested,false);new GameObject("GameDuo / QA temporal").AddComponent<GameDuoPolishQA>();}
}

