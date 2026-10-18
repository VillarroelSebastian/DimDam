#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace CoopBeam {
// Attached explicitly for validation only. Placement isolates state tests; input exercises real movement.
public class NeonPlayQA:MonoBehaviour {
 NeonLevel l;Keyboard keyboard,original;KeyboardState desired;List<string> report=new List<string>();int failed;bool cleaned;
 InputSettings.BackgroundBehavior oldBackground;InputSettings.EditorInputBehaviorInPlayMode oldBehavior;bool oldRun;int stored;
 void Before(){if(InputState.currentUpdateType==InputUpdateType.Dynamic&&keyboard!=null)InputSystem.QueueStateEvent(keyboard,desired);}
 void After(){if(InputState.currentUpdateType==InputUpdateType.Dynamic&&keyboard!=null)keyboard.MakeCurrent();}
 void Check(bool condition,string message){report.Add((condition?"PASS ":"FAIL ")+message);if(!condition)failed++;Debug.Log(report[report.Count-1]);}
 void Place(int p,float x,float y){l.game.players[p].ReviveAt(new Vector2(x,y));Physics2D.SyncTransforms();}
 IEnumerator Hold(float seconds,params Key[] keys){for(float t=0;t<seconds;t+=Time.deltaTime){desired=new KeyboardState(keys);yield return null;}desired=new KeyboardState();yield return new WaitForFixedUpdate();}
 IEnumerator Walk(int p,float x,float seconds=5){for(float t=0;t<seconds;t+=Time.deltaTime){float dx=x-l.game.players[p].transform.position.x;if(Mathf.Abs(dx)<.12f)break;desired=new KeyboardState(p==0?(dx>0?Key.D:Key.A):(dx>0?Key.RightArrow:Key.LeftArrow));yield return null;}desired=new KeyboardState();yield return new WaitForSeconds(.15f);}
 IEnumerator Start(){
 l=FindAnyObjectByType<NeonLevel>();stored=PlayerPrefs.GetInt("ReinoNeon.FriendshipFragment",0);oldBackground=InputSystem.settings.backgroundBehavior;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;oldRun=Application.runInBackground;Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;original=Keyboard.current;keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();InputSystem.onBeforeUpdate+=Before;InputSystem.onAfterUpdate+=After;
 yield return new WaitForSeconds(.6f);Check(l.game.Running&&l.game.players.Length==2,"Both original players active immediately");Check(!l.Interact(-1)&&!l.Interact(2),"Invalid players rejected");
 Place(0,-8,1);Place(1,-9,.6f);yield return Hold(.3f,Key.D);Check(l.game.players[0].transform.position.x> -7,"Mimo original walking input");
 // Jump onto the approach and upper walkway using the unchanged Pipo controls.
 Place(1,-8.4f,.6f);yield return new WaitForSeconds(.3f);yield return Hold(.35f,Key.UpArrow,Key.RightArrow);yield return Hold(.5f);Check(l.game.players[1].transform.position.y>1.6f,"Pipo reaches approach step with original jump");
 yield return Hold(.4f,Key.UpArrow,Key.RightArrow);yield return Hold(.5f);Check(l.game.players[1].transform.position.y>3.3f,"Pipo reaches upper reciprocal route with real input");
 for(int i=0;i<3;i++){
 float x=-3+i*5;Place(0,x,.8f);Place(1,x,3.7f);yield return new WaitForSeconds(.25f);
 Check(!l.crossed[i*2].target.powered&&!l.crossed[i*2+1].target.powered,"Pair "+i+" starts blocked");
 Check(l.Interact(0)&&l.crossed[i*2].target.powered&&!l.crossed[i*2+1].target.powered,"Mimo opens only Pipo route "+i);Check(!l.Interact(0),"Debounce "+i);Check(l.Interact(1)&&l.crossed[i*2+1].target.powered,"Pipo reciprocates "+i);
 yield return Walk(0,x+3);yield return Walk(1,x+3);Check(l.game.players[0].transform.position.x>x+2.4f&&l.game.players[1].transform.position.x>x+2.4f,"Both physically cross linked gates "+i);
 }
 Check(l.CrossedCount==6&&l.entryGate.powered,"All six controls release energy gate");Place(0,14,1);Place(1,15,1);yield return new WaitForSeconds(.2f);Check(l.Checkpoint==1,"Joint relay checkpoint");Place(0,14,-4);yield return new WaitForSeconds(.2f);Check(l.Checkpoint==1&&l.CrossedCount==6&&l.game.players[0].transform.position.y>0,"Fall retains solved crossed circuits");
 Place(0,14.7f,.8f);Place(1,28,.6f);yield return Walk(0,17.5f,2);Check(l.SocketA&&l.battery.transform.position.x>17.65f,"Mimo physically pushes battery into socket A");
 float bx=l.battery.transform.position.x;Place(1,bx-1.1f,.6f);yield return Hold(.4f,Key.RightArrow);Check(Mathf.Abs(l.battery.transform.position.x-bx)<.03f,"Pipo cannot push heavy battery");
 // Lift and vent entry are verified separately from routing state.
 Place(1,20,1.2f);yield return new WaitForSeconds(3);Check(l.game.players[1].transform.position.y>1.3f,"Powered lift carries Pipo");
 Place(1,22.4f,3.65f);yield return Walk(1,24.8f,1.5f);Check(l.game.players[1].transform.position.x>24.4f,"Pipo traverses narrow vent");
 Place(0,21,3.85f);yield return Hold(.7f,Key.D);Check(l.game.players[0].transform.position.x<21.7f,"Mimo cannot enter Pipo vent");
 Place(1,24.8f,3.65f);yield return new WaitForSeconds(.25f);Check(l.Interact(1)&&l.Routed,"Pipo routes battery circuit B");Place(0,16.6f,.8f);yield return Walk(0,21.5f,3);Check(l.RelayDone&&l.battery.Locked&&l.relayGate.powered,"Mimo physically delivers battery B; relay complete");
 Place(0,37,1);Place(1,38,1);yield return new WaitForSeconds(.2f);Check(l.Checkpoint==2,"Core checkpoint requires both");Place(0,39,.8f);yield return new WaitForSeconds(.25f);Check(l.Interact(0)&&l.CoreStep==1,"Mimo initiates temporary channel");
 yield return new WaitForSeconds(18.3f);Check(l.CoreStep==0&&!l.coreBridge.activeSelf&&l.Checkpoint==2&&l.RelayDone,"Timeout restores core attempt and preserves relay");
 Place(0,39,.8f);Place(1,40,.6f);yield return new WaitForSeconds(.25f);Check(l.Interact(0),"Core can be retried");yield return Walk(1,47,2);Check(l.game.players[1].transform.position.x>46.6f&&l.Interact(1)&&l.CoreStep==2,"Pipo crosses physical temporary bridge and reverses power");
 yield return Walk(0,49,2.5f);Check(l.game.players[0].transform.position.x>48.6f&&l.Interact(0)&&l.CoreStep==3,"Role exchange: Mimo crosses with Pipo power and confirms");
 Place(0,51,.8f);Place(1,53,.6f);yield return new WaitForSeconds(2.9f);Check(l.Restored&&l.finalGate.powered&&l.Checkpoint==3,"Both plates charge reactor and open reward");
 Place(0,59,1.2f);Place(1,54,.6f);yield return new WaitForSeconds(.2f);Check(!l.Collected,"Solo reward refused");Place(1,59,1.1f);yield return new WaitForSeconds(.2f);Check(l.Collected&&!l.fragment.gameObject.activeSelf,"Joint reward persists and hides fragment");l.ResetTogether();Check(l.Collected&&l.Restored&&!l.fragment.gameObject.activeSelf,"Recovery retains collected state");
 Place(0,62,1);Place(1,58,.6f);yield return new WaitForSeconds(.2f);Check(!l.Completed,"Solo exit refused");Place(1,62,.6f);yield return new WaitForSeconds(.2f);Check(l.Completed,"Joint cooperative completion");
 Check(Application.CanStreamedLevelBeLoaded(NeonLevel.ScenePath)&&Application.CanStreamedLevelBeLoaded(CandyCampaign.Root+"Candy_WorldMap.unity"),"Scene and return map registered in build");
 report.Insert(0,"Neon Play Mode QA: "+failed+" failures. Real keyboard input for movement, jumps, gate crossing, pushing, vent and timed bridge. Explicit placement isolates mechanisms, checkpoints and joint triggers.");File.WriteAllLines(Application.dataPath+"/ReinoNeon/QA/PlayMode-results.txt",report);Debug.Log(string.Join("\n",report));Cleanup();Destroy(this);
 }
 void Cleanup(){if(cleaned)return;cleaned=true;InputSystem.onBeforeUpdate-=Before;InputSystem.onAfterUpdate-=After;InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;Application.runInBackground=oldRun;if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(original!=null&&original.added)original.MakeCurrent();PlayerPrefs.SetInt("ReinoNeon.FriendshipFragment",stored);PlayerPrefs.Save();}
 void OnDestroy(){Cleanup();}
}
}
#endif
