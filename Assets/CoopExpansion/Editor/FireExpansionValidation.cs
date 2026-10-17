using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
public static class FireExpansionValidation {
 public static string Run(){var lines=new List<string>();int failed=0;Action<bool,string> check=(ok,label)=>{lines.Add((ok?"PASS ":"FAIL ")+label);if(!ok)failed++;};string original=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
  for(int n=1;n<=5;n++){string path=CandyCampaign.ScenePath(n,2);check(File.Exists(path),"Fire scene "+n+" exists");var scene=EditorSceneManager.OpenScene(path);var game=UnityEngine.Object.FindAnyObjectByType<GameManager>();check(game&&game.level&&game.level.fireKingdom&&game.level.number==n,"Scene "+n+" correct level definition");check(game.enemyPrefabs.Length==7&&game.enemyPrefabs.All(e=>e),"Scene "+n+" all enemy prefab references");check(game.players.Length==2&&game.players.All(p=>p&&p.GetComponent<PlayerController2D>()),"Scene "+n+" two playable heroes");check(game.level.waves.Length==3&&game.level.waves.All(w=>w.enemyTypes.Length==w.spawnPositions.Length&&w.enemyTypes.All(t=>t>=0&&t<7)),"Scene "+n+" valid three waves");check(EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path==path),"Scene "+n+" in build");check(scene.GetRootGameObjects().All(g=>g.GetComponentsInChildren<Transform>(true).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0)),"Scene "+n+" no missing scripts");}
  foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/ReinoDeFuego/0_2_Prefabs"})){string path=AssetDatabase.GUIDToAssetPath(guid);var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);var animation=root.GetComponentInChildren<FireSpriteAnimator>();check(animation&&animation.frames.Length==(animation.boss?16:8)&&animation.frames.All(s=>s),root.name+" animation frames");check(root.GetComponentsInChildren<Transform>(true).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),root.name+" no missing scripts");}
  for(int n=1;n<=5;n++)check(File.Exists(CandyCampaign.ScenePath(n)),"Original candy scene "+n+" retained");if(!string.IsNullOrEmpty(original))EditorSceneManager.OpenScene(original);lines.Insert(0,"Fire asset validation: "+(lines.Count-failed)+" PASS, "+failed+" FAIL");File.WriteAllLines("Assets/ReinoDeFuego/0_6_QA/FireAssets.txt",lines);return lines[0];
 }
}
