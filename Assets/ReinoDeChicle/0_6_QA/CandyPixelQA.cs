using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
namespace CoopBeam {
// Temporary opt-in QA, never saved into content.
public class CandyPixelQA:MonoBehaviour {
 readonly List<string> results=new List<string>();Keyboard keys;GameManager game;const string Folder="Assets/ReinoDeChicle/0_6_QA/Results/";
 void Check(string name,bool ok){results.Add((ok?"PASS ":"FAIL ")+name);Debug.Log(results[results.Count-1]);File.WriteAllLines(Folder+"PixelStyleIntegration.txt",results);}
 IEnumerator Press(float seconds,params Key[] pressed){float until=Time.time+seconds;while(Time.time<until){InputSystem.QueueStateEvent(keys,new KeyboardState(pressed));yield return null;}InputSystem.QueueStateEvent(keys,new KeyboardState());yield return null;}
 IEnumerator Start(){DontDestroyOnLoad(gameObject);Application.runInBackground=true;Directory.CreateDirectory(Folder);game=GameManager.Instance;
  Check("Editor Play enters Candy_Level_01",SceneManager.GetActiveScene().path==CandyCampaign.ScenePath(1));var view=Camera.main;Check("Point-filtered pixel viewport is active",view.targetTexture&&view.targetTexture.width==800&&view.targetTexture.height==450&&view.targetTexture.filterMode==FilterMode.Point);
  Check("Pixel HUD replaces old smooth HUD",FindAnyObjectByType<CandyPixelHUD>()&&!FindAnyObjectByType<UIManager>());
  foreach(var h in game.players){var sr=h.GetComponent<PlayerController2D>().sprite;Check(sr.sprite.name+" uses generated art and hit-flash material",sr.sprite.texture.filterMode==FilterMode.Point&&sr.sharedMaterial.shader.name=="CoopBeam/PixelSprite"&&sr.GetComponent<PixelActorVisual>());}
  FindAnyObjectByType<CoopBeam>().enabled=false;game.Begin();yield return new WaitForSeconds(2);foreach(var e in game.enemies)e.gameObject.SetActive(false);keys=InputSystem.AddDevice<Keyboard>();yield return Press(.2f);
  var red=game.players[0];var blue=game.players[1];Vector2 r=red.transform.position,b=blue.transform.position;yield return Press(.3f,Key.D,Key.LeftArrow);Check("New hero art preserves simultaneous independent movement",red.transform.position.x>r.x+.5f&&blue.transform.position.x<b.x-.5f);
  yield return Press(.25f);float ry=red.transform.position.y,by=blue.transform.position.y;yield return Press(.15f,Key.W,Key.UpArrow);Check("Both pixel heroes jump",red.transform.position.y>ry+.5f&&blue.transform.position.y>by+.5f);
  yield return Press(.08f,Key.LeftShift,Key.RightCtrl);Check("Dash creates pixel afterimages",FindObjectsByType<PixelDashEcho>().Length>0);
  red.Hit(Vector2.left);yield return null;yield return new WaitForEndOfFrame();var block=new MaterialPropertyBlock();red.GetComponent<PlayerController2D>().sprite.GetPropertyBlock(block);Check("Hit flash and health remain functional",red.Hearts==2&&block.GetFloat("_Flash")>0);
  InputSystem.RemoveDevice(keys);keys=null;game.Restart();yield return new WaitForSeconds(.5f);Check("Restart returns to candy menu with full hearts",GameManager.Instance&&!GameManager.Instance.Started&&GameManager.Instance.players[0].Hearts==3);ScreenCapture.CaptureScreenshot(Folder+"Pixel_Menu_Final.png");yield return new WaitForSeconds(.2f);Debug.Log("CANDY_PIXEL_QA_COMPLETE");Destroy(gameObject);
 }
 void OnDestroy(){if(keys!=null&&keys.added)InputSystem.RemoveDevice(keys);}
}}
