using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace CoopBeam {
// Explicitly launched by the editor; never attached to the delivered scene.
public class BubblegumPlayQA:MonoBehaviour {
 #if UNITY_EDITOR
 InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
 #endif
 BubblegumLevel l;Keyboard keyboard,original;readonly List<string> results=new List<string>();int failed;
 void Check(bool condition,string label){results.Add((condition?"PASS ":"FAIL ")+label);if(!condition)failed++;}
 void Place(int p,float x,float y){var h=l.game.players[p];h.ReviveAt(new Vector2(x,y));Physics2D.SyncTransforms();}
 IEnumerator Hold(Key key,float seconds){for(float t=0;t<seconds;t+=Time.deltaTime){InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));yield return null;}InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForFixedUpdate();}
 IEnumerator JumpTo(int player,float target){
 InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSeconds(.12f);
 var pc=l.game.players[player].GetComponent<PlayerController2D>();
 for(float t=0;t<1.4f;t+=Time.deltaTime){
 float dx=target-pc.transform.position.x;var keys=new List<Key>();if(t<.65f)keys.Add(player==0?Key.W:Key.UpArrow);
 if(Mathf.Abs(dx)>.12f)keys.Add(player==0?(dx>0?Key.D:Key.A):(dx>0?Key.RightArrow:Key.LeftArrow));
 InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys.ToArray()));yield return null;if(t>.3f&&pc.Grounded)break;
 }
 InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSeconds(.15f);
 }
 IEnumerator Start(){
 #if UNITY_EDITOR
 oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
 #endif
 l=FindAnyObjectByType<BubblegumLevel>();original=Keyboard.current;keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();yield return new WaitForSeconds(.4f);
 Check(l.game.Running&&l.game.players.Length==2,"Exactly two existing players active immediately");
 Check(l.game.players[0].GetComponent<PlayerController2D>().Grounded,"Mimo grounded on starting bridge");
 float x=l.game.players[0].transform.position.x;yield return Hold(Key.D,.4f);Check(l.game.players[0].transform.position.x>x+.5f,"Original keyboard movement");float y=l.game.players[0].transform.position.y;yield return Hold(Key.W,.25f);Check(l.game.players[0].transform.position.y>y+.5f,"Original keyboard jump");
 l.ResetTogether();Place(1,4.2f,.5f);yield return Hold(Key.RightArrow,.4f);Check(l.game.players[1].transform.position.x>5,"Pipo fits through physical tunnel");
 Place(0,4.2f,.75f);yield return Hold(Key.D,.4f);Check(l.game.players[0].transform.position.x<4.7f,"Mimo cannot fit through physical tunnel");
 Place(0,6.5f,.5f);Check(!l.Interact(0)&&!l.AuxiliaryOn,"Large player cannot operate small auxiliary");Place(1,6.5f,.5f);Check(l.Interact(1)&&l.AuxiliaryOn,"Small auxiliary activates");
 Place(0,8.8f,.75f);Check(l.Interact(0)&&l.LeverOn,"Existing S interaction semantics open lever");
 float rock=l.entryRock.transform.position.x;yield return Hold(Key.D,4.8f);Check(l.entryRock.transform.position.x>rock+4&&l.DoorAOpen,"Mimo physically pushes rock through gate to unlock A");
 // Reproducible fork cases: controlled placement isolates route logic from traversal.
 l.runaway.Place(new Vector2(28,3.15f));yield return new WaitForSeconds(5.5f);Check(l.FailedRoutes==1&&!l.Rolling&&Vector2.Distance(l.runaway.transform.position,l.runaway.Home)<.1f,"Wrong route returns rock safely");
 Place(1,30,.6f);l.Interact(1);l.routeBlock.Place(new Vector2(35,.65f));l.runaway.Place(new Vector2(28,3.15f));yield return new WaitForSeconds(7);Check(l.DoorBOpen&&!l.Rolling,"Correct fork with guide block activates pressure plate and door");
 Place(0,44,1);Place(1,45.5f,1);yield return new WaitForSeconds(.2f);Check(l.Checkpoint==1,"Both players commit checkpoint after B");Place(0,44,-5);yield return new WaitForSeconds(.2f);Check(l.game.players[0].transform.position.y>0&&l.game.players[1].transform.position.x>44&&l.DoorBOpen,"Fall restores both and preserves solved gates");
 Place(1,46.5f,.45f);Check(!l.Interact(1)&&!l.BubblesOn,"Bubble switch requires heavy step positioned");
 float block=l.step.transform.position.x;Place(1,48.8f,.5f);yield return Hold(Key.RightArrow,.5f);Check(Mathf.Abs(l.step.transform.position.x-block)<.01f,"Pipo cannot push heavy step");
 Place(0,48.7f,.75f);yield return Hold(Key.D,1.5f);Check(l.step.transform.position.x>51.6f,"Mimo pushes heavy step to mark");
 Place(1,46.5f,.45f);Check(l.Interact(1)&&l.BubblesOn,"Pipo activates bubbles after Mimo positions step");yield return new WaitForSeconds(1.5f);Check(System.Array.TrueForAll(l.bubbles,b=>b.activeSelf),"All bubble platforms appear and persist");
 Place(0,54,5);yield return new WaitForSeconds(.8f);Check(l.game.players[0].GetComponent<PlayerController2D>().Grounded&&l.game.players[0].transform.position.y>3,"Large player lands on bubble collider");
 Place(1,54,5);yield return new WaitForSeconds(.8f);Check(l.game.players[1].GetComponent<PlayerController2D>().Grounded&&l.game.players[1].transform.position.y>3,"Small player lands on bubble collider");
 for(int player=0;player<2;player++){Place(player,50.5f,.8f);yield return new WaitForSeconds(.3f);foreach(float target in new[]{52f,54f,56.7f,59.4f,62.5f})yield return JumpTo(player,target);Check(l.game.players[player].transform.position.y>6,"Player "+player+" climbs from floor to refuge using original jump controls");}
 Place(0,65,7.6f);Place(1,66.5f,7.6f);yield return new WaitForSeconds(.2f);Check(l.Checkpoint==2,"Final checkpoint requires both upstairs");
 Place(0,70,7.6f);yield return new WaitForSeconds(.2f);Check(!l.Collected,"One player cannot collect fragment");Place(1,70.8f,7.6f);yield return new WaitForSeconds(1.3f);Check(l.Collected&&!l.fragment.gameObject.activeSelf,"Both collect fragment with reward animation");
 Place(0,76,7.5f);yield return new WaitForSeconds(.2f);Check(!l.Completed,"Exit waits for second player");Place(1,76.5f,7.5f);yield return new WaitForSeconds(.2f);Check(l.Completed,"Both players complete with future scene fallback");
 l.ResetTogether();Check(l.Checkpoint==2&&l.Collected&&l.BubblesOn,"Final reset preserves coherent completed progression");
 results.Insert(0,"Bubblegum Play Mode QA: "+failed+" failures. Input tests plus explicit placement for isolated mechanism tests; not a full manual traversal.");File.WriteAllLines(Application.dataPath+"/BubblegumKingdom/QA/PlayMode-results.txt",results);Debug.Log(string.Join("\n",results));Cleanup();Destroy(this);
 }
 void Cleanup(){
 #if UNITY_EDITOR
 InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;
 #endif
if(keyboard!=null){InputSystem.RemoveDevice(keyboard);keyboard=null;}if(original!=null&&original.added)original.MakeCurrent();}
 void OnDestroy(){Cleanup();}
}}




