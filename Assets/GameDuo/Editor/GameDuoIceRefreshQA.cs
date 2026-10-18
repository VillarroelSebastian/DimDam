#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
// Runs against the actual level, boss coroutine, hazards and colliders.
[InitializeOnLoad]
public static class GameDuoIceRefreshQA {
 const string Key="GameDuo.IceRefreshQA";
 static GameDuoIceRefreshQA(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){SessionState.SetBool(Key,false);new GameObject("Ice refresh QA").AddComponent<GameDuoIceRefreshPlayQA>();}};}
 [MenuItem("GameDuo/Probar nuevo Rey Hielo")]
 public static void Run(){if(EditorApplication.isPlaying||EditorApplication.isCompiling)return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/GameDuo/Scenes/03_ReyHielo.unity");SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();}
}
#endif
