using System.Collections;
using UnityEngine;
namespace CoopBeam {
public class CandyLevelExit:MonoBehaviour {
 public GameManager game;public float celebrationSeconds=1.6f,ascentSeconds=2.2f;
 public bool Arriving{get;private set;}public bool Departing{get;private set;}
 bool triggered;
 IEnumerator Start(){if(!game)game=GetComponent<GameManager>();if(!CandyCampaign.ArrivalPending)yield break;CandyCampaign.ArrivalPending=false;Arriving=true;
  var targets=new Vector3[2];for(int i=0;i<2;i++){targets[i]=game.players[i].transform.position;Lock(i,true);}
  for(float t=0;t<.9f;t+=Time.deltaTime){for(int i=0;i<2;i++)game.players[i].transform.position=targets[i]+Vector3.down*2*(1-Mathf.SmoothStep(0,1,t/.9f));yield return null;}
  for(int i=0;i<2;i++){game.players[i].transform.position=targets[i];Lock(i,false);}Arriving=false;game.Begin();
 }
 void Update(){if(game&&game.Victory&&!triggered){triggered=true;StartCoroutine(Depart());}}
 void Lock(int i,bool locked){var h=game.players[i];h.GetComponent<PlayerController2D>().enabled=!locked;var rb=h.GetComponent<Rigidbody2D>();rb.linearVelocity=Vector2.zero;rb.simulated=!locked;foreach(var c in h.GetComponents<Collider2D>())c.enabled=!locked;if(!locked)h.GetComponent<PlayerController2D>().ResetMotion();}
 IEnumerator Depart(){Departing=true;var origins=new Vector3[2];for(int i=0;i<2;i++){if(game.players[i].Dead)game.players[i].Revive();Lock(i,true);origins[i]=game.players[i].transform.position;}
  for(float t=0;t<celebrationSeconds;t+=Time.deltaTime){for(int i=0;i<2;i++)game.players[i].transform.position=origins[i]+Vector3.up*Mathf.Abs(Mathf.Sin(t*7+i*.4f))*.24f;yield return null;}
  for(float t=0;t<ascentSeconds;t+=Time.deltaTime){float p=Mathf.Clamp01(t/ascentSeconds);for(int i=0;i<2;i++){var end=new Vector3(i==0?-1.1f:1.1f,9,0);game.players[i].transform.position=Vector3.Lerp(origins[i],end,p*p)+Vector3.right*Mathf.Sin(t*5+i*Mathf.PI)*.13f;}yield return null;}
  CandyCampaign.Advance(game.level.number);
 }
}}
