#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
[InitializeOnLoad]
public static class GameDuoCandyEnemyQA {
 static GameDuoCandyEnemyQA(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("CandyEnemyQA",false)){SessionState.SetBool("CandyEnemyQA",false);new GameObject("Candy enemy QA").AddComponent<GameDuoCandyEnemyPlayQA>();}};}
 [MenuItem("GameDuo/Probar nuevos enemigos de caramelo")]
 public static void Run(){EditorSceneManager.OpenScene(GameDuoCampaign.Scenes[4]);SessionState.SetBool("CandyEnemyQA",true);EditorApplication.EnterPlaymode();}
}
#endif
