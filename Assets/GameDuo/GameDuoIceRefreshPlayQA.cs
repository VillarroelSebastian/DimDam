#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace CoopBeam {
public sealed class GameDuoIceRefreshPlayQA:MonoBehaviour {
 readonly List<string> report=new List<string>();int failures;
 void Check(bool pass,string message){report.Add((pass?"PASS ":"FAIL ")+message);if(!pass)failures++;}
 IEnumerator Start(){
  yield return null;yield return null;
  var game=GameManager.Instance;Check(game!=null,"Ice level loaded");if(!game){Finish();yield break;}
  game.BeginPractice();var director=game.GetComponent<CoopInteractionDirector>();if(director)director.enabled=false;
  foreach(var player in game.players)player.Protect(180);
  var enemy=Instantiate(game.enemyPrefabs[6],new Vector2(0,-2.65f),Quaternion.identity);var king=enemy.GetComponent<IceKingBoss>();
  yield return null;yield return null;yield return null;
  var motion=enemy.GetComponent<GameDuoIceKingMotion>();Check(motion,"Continuous boss animation installed");
  var poses=enemy.GetComponent<GameDuoIcePoseFrames>();Check(poses&&poses.FrameCount==32,"32 additional boss attack poses loaded");
  int before=poses?poses.DisplayedFrame:-1;yield return new WaitForSeconds(.8f);
  Check(poses&&poses.DisplayedFrame!=before,"Boss changes drawings during anticipation");
  for(int role=0;role<6;role++){
   var sample=Instantiate(game.enemyPrefabs[role],new Vector2(-8+role*3,-3.6f),Quaternion.identity);yield return null;yield return null;
   var extra=sample.GetComponent<GameDuoIcePoseFrames>();Check(extra&&extra.FrameCount==48,"Additional drawn frames installed for ice enemy "+role);Destroy(sample.gameObject);
  }
  int shelves=0;foreach(var shape in FindObjectsByType<BoxCollider2D>())foreach(var r in shape.GetComponentsInChildren<SpriteRenderer>())if(r.name.StartsWith("Plataforma ilustrada")){shelves++;Check(Mathf.Abs(r.transform.position.y-shape.bounds.max.y)<.01f,"Shelf art anchored to collider: "+shape.name);}
  Check(shelves>=7,"Foundation, galleries and accessible steps use new assets");
  var patterns=new HashSet<int>();var phases=new HashSet<int>();bool hazardSeen=false,locked=false;float timeout=Time.time+125;
  while(king&&!king.Defeated&&Time.time<timeout){
   phases.Add(king.Phase);if(king.Attacking)patterns.Add(king.PatternIndex);
   foreach(var hazard in FindObjectsByType<EncounterHazard>())if(hazard.owner==king){hazardSeen=true;Check(hazard.GetComponent<GameDuoIceAttackVisual>(),"Real boss hazard has animated visual");break;}
   if(!king.Vulnerable&&!locked){float hp=enemy.Health;enemy.Damage(1);Check(enemy.Health==hp,"Closed core blocks damage");locked=true;}
   if(king.Vulnerable&&(patterns.Count==4||king.Phase>1))enemy.Damage(enemy.maxHealth);
   foreach(var player in game.players)player.Protect(5);
   yield return new WaitForSeconds(.2f);
  }
  Check(patterns.Count==4,"All four attack patterns executed");Check(phases.Count==3,"All three combat phases reached");Check(hazardSeen,"Boss projectiles spawned");Check(king&&king.Defeated,"Boss defeat reached through exposed core damage");
  yield return new WaitForSeconds(2);Check(EncounterHazard.LiveCount==0,"Defeat clears hazards");Finish();
 }
 void Finish(){report.Add("Failures: "+failures);File.WriteAllLines("Assets/GameDuo/IceRefreshPlayQA.txt",report);Debug.Log("ICE_REFRESH_QA: "+failures+" failures");UnityEditor.EditorApplication.isPlaying=false;}
}
}
#endif
