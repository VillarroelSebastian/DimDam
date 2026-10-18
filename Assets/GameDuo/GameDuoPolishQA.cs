#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CoopBeam;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public sealed class GameDuoPolishQA:MonoBehaviour {
 readonly List<string> lines=new List<string>();Keyboard keyboard;bool finished;int failures;
 void Check(bool ok,string text){lines.Add((ok?"PASS ":"FAIL ")+text);if(!ok)failures++;}
 void Keys(params Key[] keys){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
 IEnumerator Start(){
  DontDestroyOnLoad(gameObject);keyboard=InputSystem.AddDevice<Keyboard>("GameDuo QA Keyboard");
  foreach(string atlas in new[]{"GumRaiders_32","IceKing_32","GumUncle_32"})Check(Resources.Load<Texture2D>("GameDuo/"+atlas),"Atlas imported in Resources: "+atlas);
  for(int stage=0;stage<GameDuoCampaign.Scenes.Length;stage++){
   SceneManager.LoadScene(GameDuoCampaign.Scenes[stage]);yield return null;yield return null;yield return new WaitForFixedUpdate();
   Check(GameDuoCampaign.Active&&GameDuoCampaign.Instance.Stage==stage,"Chapter "+stage+" controller installed");
   var game=GameManager.Instance;
   if(game){
    Check(Mathf.Abs(game.players[0].transform.localScale.y-GameDuoHeroIdentity.LargeScale)<.01f&&Mathf.Abs(game.players[1].transform.localScale.y-GameDuoHeroIdentity.SmallScale)<.01f,"Chapter "+stage+" large and small physics proportions");
    float large=game.players[0].GetComponent<PlayerController2D>().sprite.bounds.size.y,small=game.players[1].GetComponent<PlayerController2D>().sprite.bounds.size.y;
    Check(large/small>1.55f&&large/small<1.85f,"Chapter "+stage+" visible hero height ratio: "+(large/small).ToString("F2"));
    Check(Camera.main&&Camera.main.targetTexture==null,"Chapter "+stage+" renders to display without offscreen camera warning");
    if(stage==0||stage==2||stage==4){
     int index=stage==2?6:stage==4?game.enemyPrefabs.Length-1:0;
     var enemy=Instantiate(game.enemyPrefabs[index],new Vector2(0,-2.65f),Quaternion.identity);
     yield return null;yield return null;
     string expected=stage==4?"PinkKing_":"GumRaiders_32";
     Check(stage==0?enemy.GetComponent<TutorialSootEnemy>()&&!enemy.sprite.enabled:stage==2?enemy.GetComponent<GameDuoIcePoseFrames>()&&enemy.GetComponent<GameDuoIcePoseFrames>().FrameCount==32:enemy.sprite.sprite.name.StartsWith(expected),"Chapter "+stage+" uses new animated enemy art");
     Destroy(enemy.gameObject);
    }
   }else if(stage==3){
    var large=GameObject.Find("Mimo");var small=GameObject.Find("Pipro");
    Check(large&&small&&large.GetComponent<SpriteRenderer>().bounds.size.y>small.GetComponent<SpriteRenderer>().bounds.size.y*1.55f,"Penguin round preserves hero proportions");
   }
   if(stage==2&&game){
    game.BeginPractice();var director=game.GetComponent<CoopInteractionDirector>();director.enabled=false;
    foreach(var player in game.players)player.Protect(60);
    yield return null;
    for(int i=0;i<2;i++){
     var player=game.players[i];var shape=player.GetComponent<CapsuleCollider2D>();
     player.ReviveAt(new Vector2(i==0?-6:6,-1.575f+shape.bounds.extents.y+.025f));
    }
    Physics2D.SyncTransforms();
    for(int frame=0;frame<10;frame++){Keys();yield return new WaitForFixedUpdate();}
    float[] highest={-10,-10};
    for(int frame=0;frame<90;frame++){
     if(frame<43)Keys(Key.W,Key.UpArrow);else Keys();
     yield return new WaitForFixedUpdate();
     for(int i=0;i<2;i++)highest[i]=Mathf.Max(highest[i],game.players[i].GetComponent<Collider2D>().bounds.min.y);
    }
    for(int i=0;i<2;i++)Check(highest[i]>1.4f&&game.players[i].GetComponent<Collider2D>().bounds.min.y>1.2f,"Hero "+i+" jumps from gallery and lands on second ice platform");
    Keys();yield return null;
    director.BeginDance();director.enabled=true;
    int last=-1;float until=Time.realtimeSinceStartup+16;
    while(director.Dancing&&Time.realtimeSinceStartup<until){
     if(director.DanceBeat!=last&&director.BeatAge>.29f&&director.BeatAge<.55f){
      int key=director.DanceKey,player=director.DancePlayer;
      Key[] mimo={Key.W,Key.A,Key.S,Key.D},pipro={Key.UpArrow,Key.LeftArrow,Key.DownArrow,Key.RightArrow};
      Keys((player==0?mimo:pipro)[key]);last=director.DanceBeat;
     }else Keys();
     yield return null;
    }
    Keys();Check(director.DanceHits==12,"Dance accepts twelve timed presses from the indicated player");
    Check(!director.Dancing,"Dance completes and returns player control");
   }
  }
  Finish();
 }
 void Finish(){if(finished)return;finished=true;if(keyboard!=null)InputSystem.RemoveDevice(keyboard);Time.timeScale=1;lines.Insert(0,failures==0?"PASS / GameDuo Play Mode QA":"FAIL / "+failures+" checks");File.WriteAllLines("Assets/GameDuo/PolishPlayModeQA.txt",lines);Debug.Log(lines[0]);EditorApplication.ExitPlaymode();}
 void OnDestroy(){if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);}
}

#endif
