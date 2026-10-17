using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace CoopBeam {
public class CandyMotionQA:MonoBehaviour {
 readonly List<string> results=new List<string>();Keyboard keyboard;GameManager game;const string Output="Assets/ReinoDeChicle/0_6_QA/Results/AnimationAndMotion.txt";
 void Check(string text,bool ok){results.Add((ok?"PASS ":"FAIL ")+text);Debug.Log(results[results.Count-1]);File.WriteAllLines(Output,results);}
 IEnumerator Keys(float time,params Key[] keys){float end=Time.time+time;while(Time.time<end){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));yield return null;}InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;}
 void Place(PlayerHealth h,Vector2 pos){h.transform.position=pos;h.GetComponent<Rigidbody2D>().position=pos;h.GetComponent<PlayerController2D>().ResetMotion();Physics2D.SyncTransforms();}
 IEnumerator Start(){Application.runInBackground=true;game=GameManager.Instance;FindAnyObjectByType<CoopBeam>().enabled=false;keyboard=InputSystem.AddDevice<Keyboard>();game.Begin();yield return new WaitForSeconds(2);foreach(var e in game.enemies)e.gameObject.SetActive(false);
  var red=game.players[0];var blue=game.players[1];var animator=red.GetComponentInChildren<PixelSpriteAnimator>();Check("Hero has 20 real sprite frames",animator&&animator.animationSet.frames.Length==20);
  var frames=new HashSet<int>();float end=Time.time+1.5f;while(Time.time<end){frames.Add(animator.CurrentFrame);yield return null;}Check("Idle visibly cycles through breathing and blink frames",frames.Count>=3);
  frames.Clear();end=Time.time+.5f;while(Time.time<end){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D,Key.LeftArrow));frames.Add(animator.CurrentFrame);yield return null;}yield return Keys(.1f);Check("Running changes actual sprite frames",frames.Count>=3);
  Place(red,new Vector2(-11,-.5f));yield return new WaitForFixedUpdate();float y=red.GetComponent<Rigidbody2D>().position.y;yield return Keys(.6f,Key.A);float wallY=red.GetComponent<Rigidbody2D>().position.y;Check("Pushing into wall does not suspend player (from "+y.ToString("F2")+" to "+wallY.ToString("F2")+")",wallY<y-1);
  float x=red.transform.position.x;yield return Keys(.5f,Key.D);Check("Player can leave wall without getting stuck",red.transform.position.x>x+2);
  Place(red,new Vector2(-6,-3.7f));Place(blue,new Vector2(6,-3.7f));yield return Keys(.3f);y=red.transform.position.y;yield return Keys(.16f,Key.W,Key.UpArrow);Check("Both players still jump independently",red.transform.position.y>y+.5f&&blue.transform.position.y>-3.1f);
  yield return Keys(.08f,Key.LeftShift,Key.RightCtrl);Check("Dash selects its dedicated pose",animator.State=="Dash"&&animator.CurrentFrame==12);
  yield return Keys(.7f);red.Hit(Vector2.right);yield return null;yield return new WaitForEndOfFrame();Check("Damage selects recoil frame",animator.State=="Hurt"&&animator.CurrentFrame==13);
  red.GetComponent<Rigidbody2D>().simulated=false;blue.GetComponent<Rigidbody2D>().simulated=false;
  for(int kind=0;kind<7;kind++){var enemy=Instantiate(game.enemyPrefabs[kind],new Vector2(0,-3.5f),Quaternion.identity);var ea=enemy.GetComponentInChildren<PixelSpriteAnimator>();Check(((EnemyKind)kind)+" has 8 animation frames",ea&&ea.animationSet.frames.Length==8);Destroy(enemy.gameObject);yield return null;}
  var walker=Instantiate(game.enemyPrefabs[0],new Vector2(-3.8f,-1.35f),Quaternion.identity);yield return new WaitForSeconds(.25f);x=walker.transform.position.x;yield return new WaitForSeconds(1.2f);Check("Walker turns away from an island edge and keeps moving",walker.transform.position.x<x-.3f&&walker.transform.position.y>-1.6f);Destroy(walker.gameObject);
  var armor=Instantiate(game.enemyPrefabs[3],new Vector2(-4,-3.65f),Quaternion.identity);yield return new WaitForSeconds(.35f);Check("Armored enemy detects ground using its own collider height",armor.Grounded);Destroy(armor.gameObject);
  var chaser=Instantiate(game.enemyPrefabs[2],new Vector2(10.8f,-3.7f),Quaternion.identity);blue.transform.position=new Vector2(11.1f,-3.7f);red.transform.position=blue.transform.position;yield return new WaitForSeconds(.2f);x=chaser.transform.position.x;yield return new WaitForSeconds(.9f);Check("Chaser does not remain oscillating against arena wall",chaser.transform.position.x<x-.15f||Mathf.Abs(chaser.transform.position.x-10.65f)<.35f);Destroy(chaser.gameObject);
  var dying=Instantiate(game.enemyPrefabs[0],new Vector2(0,-3.7f),Quaternion.identity);yield return null;dying.Damage(1000);yield return new WaitForSeconds(.12f);Check("Enemy death plays before removal and disables contact",dying&&dying.Dead&&!dying.GetComponent<Collider2D>().enabled&&dying.GetComponentInChildren<PixelSpriteAnimator>().State=="Death");yield return new WaitForSeconds(.4f);Check("Enemy corpse is cleaned up after its animation",!dying);
  Check("World contains living ambient decoration",FindAnyObjectByType<CandyAmbientLife>()&&FindAnyObjectByType<CandyAmbientLife>().transform.childCount>10);
  InputSystem.RemoveDevice(keyboard);keyboard=null;Debug.Log("CANDY_MOTION_QA_COMPLETE");game.Restart();
 }
 void OnDestroy(){if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);}
}}
