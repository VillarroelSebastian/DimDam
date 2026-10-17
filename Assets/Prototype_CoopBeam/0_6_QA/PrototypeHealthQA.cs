using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace CoopBeam {
public class PrototypeHealthQA:MonoBehaviour {
 readonly List<string> results=new List<string>();
 void Check(string name,bool ok){results.Add((ok?"PASS ":"FAIL ")+name);Debug.Log(results[results.Count-1]);}
 IEnumerator Start(){DontDestroyOnLoad(gameObject);var game=GameManager.Instance;game.Begin();yield return new WaitForSeconds(2);
  FindAnyObjectByType<CoopBeam>().enabled=false;foreach(var e in game.enemies)e.gameObject.SetActive(false);
  var r=game.players[0];var b=game.players[1];Check("Players start with three hearts",r.Hearts==3&&b.Hearts==3);
  for(int i=0;i<4;i++){var e=Instantiate(game.enemyPrefabs[i],new Vector2(-7+i*4,-3.7f),Quaternion.identity);float x=e.transform.position.x;yield return new WaitForSeconds(.4f);Check(((EnemyKind)i)+" moves under physics",Mathf.Abs(e.transform.position.x-x)>.05f);Destroy(e.gameObject);}
  r.transform.position=new Vector2(-3,-3.7f);r.GetComponent<PlayerController2D>().ResetMotion();var contact=Instantiate(game.enemyPrefabs[0],new Vector2(-3.65f,-3.7f),Quaternion.identity);yield return new WaitForSeconds(.3f);
  Check("Enemy contact hurts a player",r.Hearts<3);Destroy(contact.gameObject);
  int hearts=r.Hearts;r.Hit(Vector2.right);Check("Invulnerability blocks repeated contact",r.Hearts==hearts);
  while(!r.Dead){yield return new WaitForSeconds(1.5f);r.Hit(Vector2.right);}
  Check("Death disables player collider",!r.GetComponent<Collider2D>().enabled&&game.Running);yield return new WaitForSeconds(3.2f);
  Check("Player respawns with restored hearts",!r.Dead&&r.Hearts==3&&r.GetComponent<Collider2D>().enabled);
  yield return new WaitForSeconds(2.1f);
  for(int i=0;i<3;i++){r.Hit(Vector2.right);b.Hit(Vector2.left);if(i<2)yield return new WaitForSeconds(1.5f);}
  Check("Both players dead produce Game Over",!game.Running&&!game.Victory&&r.Dead&&b.Dead);
  ScreenCapture.CaptureScreenshot("Assets/Prototype_CoopBeam/0_6_QA/Results/Defeat.png");yield return new WaitForSeconds(.3f);
  game.Restart();yield return new WaitForSeconds(1);
  game=GameManager.Instance;Check("Scene restart restores menu and three hearts",game&&!game.Started&&!game.Running&&game.players[0].Hearts==3&&game.players[1].Hearts==3);
  Directory.CreateDirectory("Assets/Prototype_CoopBeam/0_6_QA/Results");File.WriteAllLines("Assets/Prototype_CoopBeam/0_6_QA/Results/HealthAndRestart.txt",results);Debug.Log("COOP_QA_COMPLETE HealthAndRestart.txt");Destroy(gameObject);
 }
}}

