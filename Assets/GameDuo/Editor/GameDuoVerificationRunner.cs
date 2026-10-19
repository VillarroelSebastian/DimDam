#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
[InitializeOnLoad]
public static class GameDuoVerificationRunner {
 const string Key="GameDuo.FullVerification";
 static GameDuoVerificationRunner(){EditorApplication.update+=CheckRequest;EditorApplication.playModeStateChanged+=Changed;}
 static void CheckRequest(){if(!File.Exists("Temp/CampaignQA.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;File.Delete("Temp/CampaignQA.request");Run();}
 [MenuItem("GameDuo/Verificar campana completa y cinematics")]
 public static void Run(){
  if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;
  if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty){Debug.LogError("Guarda la escena antes de ejecutar la verificacion completa.");return;}
  SessionState.SetString(Key+".Scene",UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
  EditorSceneManager.OpenScene(GameDuoMainMenu.ScenePath);SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();
 }
 static void Changed(PlayModeStateChange state){
  if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))new GameObject("QA / Campaign verification").AddComponent<GameDuoCampaignVerification>();
  if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key,false)){SessionState.SetBool(Key,false);string original=SessionState.GetString(Key+".Scene","");if(!string.IsNullOrEmpty(original))EditorSceneManager.OpenScene(original);}
 }
}
#endif
