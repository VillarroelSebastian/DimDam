#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace CoopBeam {
// Explicit QA component, never saved into the gameplay scene.
public class BubblegumPlayQA:MonoBehaviour {
 InputSettings.EditorInputBehaviorInPlayMode oldBehavior; InputSettings.BackgroundBehavior oldBackground;bool oldRun;
 BubblegumLevel l;Keyboard keyboard,original;readonly List<string> results=new List<string>();int failed;KeyboardState desired;
 void SetInput(KeyboardState state){desired=state;}
 void BeforeInput(){if(InputState.currentUpdateType==InputUpdateType.Dynamic&&keyboard!=null&&keyboard.added)InputSystem.QueueStateEvent(keyboard,desired);}
 void AfterInput(){if(InputState.currentUpdateType==InputUpdateType.Dynamic&&keyboard!=null&&keyboard.added)keyboard.MakeCurrent();}
 void Check(bool condition,string label){results.Add((condition?"PASS ":"FAIL ")+label);if(!condition)failed++;Debug.Log(results[results.Count-1]);}
 void Place(int p,float x,float y){l.game.players[p].ReviveAt(new Vector2(x,y));Physics2D.SyncTransforms();}
 IEnumerator Hold(Key key,float seconds){int frames=0;for(float t=0;t<seconds||frames<8;t+=Time.deltaTime,frames++){SetInput(new KeyboardState(key));yield return null;}SetInput(new KeyboardState());yield return new WaitForFixedUpdate();}
 IEnumerator JumpTo(int player,float target){
 SetInput(new KeyboardState());yield return new WaitForSeconds(.15f);
 var pc=l.game.players[player].GetComponent<PlayerController2D>();
 for(float t=0;t<1.6f;t+=Time.deltaTime){float dx=target-pc.transform.position.x;var keys=new List<Key>();if(t<.8f)keys.Add(player==0?Key.W:Key.UpArrow);if(Mathf.Abs(dx)>.12f)keys.Add(player==0?(dx>0?Key.D:Key.A):(dx>0?Key.RightArrow:Key.LeftArrow));SetInput(new KeyboardState(keys.ToArray()));yield return null;if(t>.4f&&pc.Grounded)break;}
 SetInput(new KeyboardState());yield return new WaitForSeconds(.15f);Debug.Log("JUMP_TRACE player="+player+" target="+target+" position="+pc.transform.position+" grounded="+pc.Grounded);
 }
 IEnumerator Start(){
 oldBackground=InputSystem.settings.backgroundBehavior;oldRun=Application.runInBackground;Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
 l=FindAnyObjectByType<BubblegumLevel>();original=Keyboard.current;keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();InputSystem.onBeforeUpdate+=BeforeInput;InputSystem.onAfterUpdate+=AfterInput;yield return new WaitForSeconds(.5f);
 Check(l.game.Running&&l.game.players.Length==2,"Two existing animated players active immediately");
 Place(1,1.5f,.5f);Check(!l.entryRock.TryMove(new Vector2(1,1.1f))&&l.entryRock.transform.position.x<0,"Swept mint refuses to crush small player");l.ResetTogether();
 float x=l.game.players[0].transform.position.x;yield return Hold(Key.D,.6f);Check(l.game.players[0].transform.position.x>x+.3f,"Original walking controls");
 l.ResetTogether();Place(1,-7,.5f);yield return Hold(Key.RightArrow,.6f);Check(l.game.players[1].transform.position.x>-6,"Pipo enters auxiliary tunnel");
 Place(0,-7,.8f);yield return Hold(Key.D,.3f);Check(l.game.players[0].transform.position.x<-6.7f,"Large player cannot enter auxiliary tunnel");
 Place(1,-5,.5f);Check(!l.Interact(-1)&&!l.Interact(2),"Invalid interaction indices rejected");Check(l.Interact(1)&&l.AuxiliaryOn,"Small auxiliary validated");Check(!l.Interact(1),"Repeated interaction debounced");
 Place(0,-2.5f,.8f);Check(l.Interact(0)&&l.LeverOn,"Lever A validated");
 Place(1,5,.5f);Place(0,-2.5f,.8f);yield return Hold(Key.D,2.3f);Check(l.DoorAOpen&&l.entryRock.transform.position.x>2.7f,"Mimo physically pushes mint to open first ascent");
 for(int player=0;player<2;player++){Place(player,8,1);yield return new WaitForSeconds(.4f);foreach(float target in new[]{8f,5.8f,8f,5.8f,4f})yield return JumpTo(player,target);Check(l.game.players[player].transform.position.y>10,"Player "+player+" traverses full first vertical staircase with real inputs");}
 Place(0,-8,11);Place(1,-7,11);yield return new WaitForSeconds(.3f);Check(l.Checkpoint==1,"Both players commit first floor checkpoint");
 l.runaway.Place(new Vector2(-2,12.1f));yield return new WaitForSeconds(3.5f);Check(l.FailedRoutes==1&&!l.Rolling,"Wrong fork returns mint");
 Place(1,-1,10.6f);Check(l.Interact(1)&&l.RouteUp,"Fork becomes green");l.routeBlock.Place(new Vector2(4,10.65f));Place(1,-7,11);Place(0,-8,11);l.runaway.Place(new Vector2(-2,12.1f));yield return new WaitForSeconds(9);Check(l.DoorBOpen&&!l.Rolling,"Correct route and mark activate physical pressure sensor");
 for(int player=0;player<2;player++){Place(player,-8,11);yield return new WaitForSeconds(.4f);foreach(float target in new[]{-8f,-5.8f,-8f,-8f,-8f})yield return JumpTo(player,target);Check(l.game.players[player].transform.position.y>20,"Player "+player+" traverses second staircase with real inputs");}
 Place(0,-8,21);Place(1,-7,21);yield return new WaitForSeconds(.25f);Check(l.Checkpoint==2,"Garden checkpoint requires both players");Place(0,-8,-5);yield return new WaitForSeconds(.25f);Check(l.game.players[0].transform.position.y>20&&l.DoorBOpen,"Fall restores both and keeps solved doors");
 Place(1,-5,20.5f);Check(!l.Interact(1)&&!l.BubblesOn,"Switch refuses missing step");
 float block=l.step.transform.position.x;Place(1,-1.3f,20.5f);yield return Hold(Key.RightArrow,.35f);Check(Mathf.Abs(l.step.transform.position.x-block)<.02f,"Small player cannot push heavy block");
 Place(1,-7,21);Place(0,-1.5f,20.8f);yield return Hold(Key.D,1.3f);Check(l.step.transform.position.x>1.6f,"Large player physically places step");
 Place(1,-5,20.5f);Check(l.Interact(1)&&l.BubblesOn,"Garden switch accepts correctly placed step");yield return new WaitForSeconds(1.3f);Check(System.Array.TrueForAll(l.bubbles,b=>b.activeSelf),"Bubble animation completes with all colliders active");
 // Interaction key must never start the shared controller's platform-drop coroutine here.
 Place(1,4,23);yield return new WaitForSeconds(.7f);float y=l.game.players[1].transform.position.y;bool standing=l.game.players[1].GetComponent<PlayerController2D>().Grounded&&y>22;yield return Hold(Key.DownArrow,.1f);yield return new WaitForSeconds(.4f);Check(standing&&l.game.players[1].transform.position.y>y-.1f,"Interaction key does not drop grounded player through active bubble");
 for(int player=0;player<2;player++){Place(player,-1.2f,21);yield return new WaitForSeconds(.3f);foreach(float target in new[]{2f,4f,1f,-2f,-4f})yield return JumpTo(player,target);Check(l.game.players[player].transform.position.y>28,"Player "+player+" climbs step and final bubble garden using original inputs");}
 Place(0,-4,29);Place(1,-3,29);yield return new WaitForSeconds(.3f);Check(l.Checkpoint==3,"Treasure checkpoint commits cooperatively");
 Place(0,-1,29);Place(1,-7,29);yield return new WaitForSeconds(.2f);Check(!l.Collected,"One player cannot collect fragment");Place(1,-.5f,29);yield return new WaitForSeconds(.2f);Check(l.Collected,"Both players collect fragment");l.ResetTogether();Check(!l.fragment.gameObject.activeSelf&&l.BubblesOn,"Reset during reward preserves fragment and bubble state");
 Place(0,3.6f,29);Place(1,-7,29);yield return new WaitForSeconds(.2f);Check(!l.Completed,"Exit waits for second player");Place(1,3.8f,29);yield return new WaitForSeconds(.2f);Check(l.Completed,"Both players complete tower");
 results.Insert(0,"Vertical Bubblegum Play Mode QA: "+failed+" failures. Real input for pushing and staircase traversal; explicit placement for isolated interactions/checkpoints.");File.WriteAllLines(Application.dataPath+"/BubblegumKingdom/QA/PlayMode-results.txt",results);Debug.Log(string.Join("\n",results));Cleanup();Destroy(this);
 }
 void Cleanup(){InputSystem.onBeforeUpdate-=BeforeInput;InputSystem.onAfterUpdate-=AfterInput;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;InputSystem.settings.backgroundBehavior=oldBackground;Application.runInBackground=oldRun;if(keyboard!=null){InputSystem.RemoveDevice(keyboard);keyboard=null;}if(original!=null&&original.added)original.MakeCurrent();}
 void OnDestroy(){Cleanup();}
}}
#endif




