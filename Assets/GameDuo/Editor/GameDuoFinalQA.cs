#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
[InitializeOnLoad]
public static class GameDuoFinalQA {
 static GameDuoFinalQA(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("GameDuo.FinalQA",false)){SessionState.SetBool("GameDuo.FinalQA",false);new GameObject("Final QA").AddComponent<GameDuoFinalPlayQA>();}};}
 [MenuItem("GameDuo/Probar jefe rosado y refuerzos")]
 public static void Run(){GameDuoValidation.Run();GameDuoCandyEnemyQA.Run();}
}
#endif
