using System;
using UnityEngine;
namespace CoopBeam {
[Serializable] public class CandyWave {public int[] enemyTypes; public Vector2[] spawnPositions;}
[CreateAssetMenu(menuName="Coop Beam/Candy Level")]
public class CandyLevelDefinition:ScriptableObject {
 public int number;public string title;[TextArea]public string lesson;public CandyWave[] waves;public bool boss;public bool iceKingdom,fireKingdom;
 public int Realm=>fireKingdom?2:iceKingdom?1:0;
}
public static class CandyCampaign {
 public static bool ArrivalPending,WorldCleared,IceCleared,FireCleared,IceSelected,FireSelected;
 public static int Unlocked=1,IceUnlocked=1,FireUnlocked=1;
 public const string Root="Assets/ReinoDeChicle/0_0_Scenes/";
 public static int SelectedRealm=>FireSelected?2:IceSelected?1:0;
 public static bool CanEnter(int realm)=>realm==0||realm==1&&WorldCleared||realm==2&&IceCleared;
 public static int UnlockedFor(int realm)=>realm==2?FireUnlocked:realm==1?IceUnlocked:Unlocked;
 public static void SelectRealm(int realm){IceSelected=realm==1;FireSelected=realm==2;}
 public static string ScenePath(int number)=>ScenePath(number,0);
 public static string ScenePath(int number,bool ice)=>ScenePath(number,ice?1:0);
 public static string ScenePath(int number,int realm){string root=realm==2?"Assets/ReinoDeFuego/0_0_Scenes/":realm==1?"Assets/ReinoDeHielo/0_0_Scenes/":Root;string prefix=realm==2?"Fire":realm==1?"Ice":"Candy";return root+prefix+(number==5?"_Level_05_Boss":"_Level_0"+number)+".unity";}
 public static void Complete(int number){int realm=GameManager.Instance&&GameManager.Instance.level?GameManager.Instance.level.Realm:0;int next=Mathf.Min(5,number+1);if(realm==2)FireUnlocked=Mathf.Max(FireUnlocked,next);else if(realm==1)IceUnlocked=Mathf.Max(IceUnlocked,next);else Unlocked=Mathf.Max(Unlocked,next);if(number==5){if(realm==2)FireCleared=true;else if(realm==1)IceCleared=true;else WorldCleared=true;}}
 public static void Load(int number){if(GameDuoCampaign.TryAdvance())return;int realm=SelectedRealm;if(number>=1&&number<=UnlockedFor(realm)&&CanEnter(realm))UnityEngine.SceneManagement.SceneManager.LoadScene(ScenePath(number,realm));}
 public static void Advance(int number){if(GameDuoCampaign.TryAdvance())return;SelectRealm(GameManager.Instance&&GameManager.Instance.level?GameManager.Instance.level.Realm:0);if(number<5){ArrivalPending=true;UnityEngine.SceneManagement.SceneManager.LoadScene(ScenePath(number+1,SelectedRealm));}else Map();}
 public static void Map(){if(GameDuoCampaign.TryMenu())return;ArrivalPending=false;UnityEngine.SceneManagement.SceneManager.LoadScene(Root+"Candy_WorldMap.unity");}
}
}
