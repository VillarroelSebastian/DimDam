using System;
using UnityEngine;
namespace CoopBeam {
[Serializable] public class CandyWave {public int[] enemyTypes; public Vector2[] spawnPositions;}
[CreateAssetMenu(menuName="Coop Beam/Candy Level")]
public class CandyLevelDefinition:ScriptableObject {
 public int number;public string title;[TextArea]public string lesson;public CandyWave[] waves;public bool boss;
}
public static class CandyCampaign {
 public static bool ArrivalPending,WorldCleared;public static int Unlocked=1;public const string Root="Assets/ReinoDeChicle/0_0_Scenes/";
 public static string ScenePath(int number)=>Root+(number==5?"Candy_Level_05_Boss":"Candy_Level_0"+number)+".unity";
 public static void Complete(int number){Unlocked=Mathf.Max(Unlocked,Mathf.Min(5,number+1));}
 public static void Load(int number){if(number>=1&&number<=Unlocked)UnityEngine.SceneManagement.SceneManager.LoadScene(ScenePath(number));}
 public static void Advance(int number){if(number<5){ArrivalPending=true;UnityEngine.SceneManagement.SceneManager.LoadScene(ScenePath(number+1));}else{WorldCleared=true;Map();}}
 public static void Map(){ArrivalPending=false;UnityEngine.SceneManagement.SceneManager.LoadScene(Root+"Candy_WorldMap.unity");}
}
}
