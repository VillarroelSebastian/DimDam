using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CoopBeam {
// Opt-in regression through the real wave and campaign controllers; damage is accelerated.
public class CampaignTraversalQA:MonoBehaviour {
 List<string> lines=new List<string>();int failed;
 void Check(bool ok,string text){lines.Add((ok?"PASS ":"FAIL ")+text);if(!ok)failed++;}
 IEnumerator Start(){DontDestroyOnLoad(gameObject);Time.timeScale=8;CandyCampaign.ArrivalPending=false;CandyCampaign.WorldCleared=false;CandyCampaign.IceCleared=false;CandyCampaign.FireCleared=false;CandyCampaign.Unlocked=1;CandyCampaign.IceUnlocked=1;CandyCampaign.FireUnlocked=1;Check(!CandyCampaign.CanEnter(1)&&!CandyCampaign.CanEnter(2),"Later kingdoms start locked");
  for(int kingdom=0;kingdom<3;kingdom++)for(int n=1;n<=5;n++){bool ice=kingdom==1;string label=kingdom==2?"Fire ":ice?"Ice ":"Candy ";string expected=CandyCampaign.ScenePath(n,kingdom);if(SceneManager.GetActiveScene().path!=expected)SceneManager.LoadScene(expected);yield return null;yield return null;yield return new WaitForSeconds(1.3f);var game=GameManager.Instance;game.GetComponent<CandyLevelExit>().enabled=false;foreach(var h in game.players){h.Protect(2000);h.GetComponent<PlayerController2D>().enabled=false;h.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;h.transform.position=new Vector3(h==game.players[0]?-10:10,5,0);}game.Begin();float deadline=Time.time+180;
   while(game&&game.Running&&Time.time<deadline){var beam=FindAnyObjectByType<CoopBeam>();foreach(var e in game.enemies.ToArray()){if(!e||e.Dead)continue;var king=e.GetComponent<IceKingBoss>();if(king&&(!king.Vulnerable||king.Defeated))continue;var forge=e.GetComponent<FireForgeBoss>();if(forge&&(!forge.Vulnerable||forge.Defeated))continue;var old=e.GetComponent<CandyBoss>();if(old&&!old.Vulnerable)continue;Vector2 center=LinkPowers.TargetPoint(e);beam.DamageSegment(center+Vector2.left*3,center+Vector2.right*3,100);}yield return new WaitForSeconds(.15f);}
   Check(game&&game.Victory&&game.Wave==3,label+n+" all waves reach victory");if(!game||!game.Victory){Write();yield break;}Check(CandyCampaign.UnlockedFor(kingdom)>=Mathf.Min(5,n+1),label+n+" unlocks next level");CandyCampaign.Advance(n);yield return null;yield return null;Check(SceneManager.GetActiveScene().path==(n<5?CandyCampaign.ScenePath(n+1,kingdom):CandyCampaign.Root+"Candy_WorldMap.unity"),label+n+" transitions to expected destination");
   if(n==5&&kingdom<2)Check(CandyCampaign.CanEnter(kingdom+1),label+"boss unlocks the next kingdom");
  }Check(CandyCampaign.WorldCleared&&CandyCampaign.IceCleared&&CandyCampaign.FireCleared,"All three kingdoms marked completed");Write();Time.timeScale=1;
 }
 void Write(){Time.timeScale=1;lines.Insert(0,"Campaign traversal QA: "+(lines.Count-failed)+" PASS, "+failed+" FAIL. Real wave/campaign flow with accelerated beam damage and protected players; not a human playthrough.");File.WriteAllLines(Application.dataPath+"/ReinoDeFuego/0_6_QA/CampaignTraversal.txt",lines);Debug.Log("CAMPAIGN_QA_DONE "+lines[0]);}
 void OnDestroy(){Time.timeScale=1;}
}
}
