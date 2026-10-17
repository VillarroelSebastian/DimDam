using System.Collections;
using UnityEngine;
namespace CoopBeam {
// Adds punctuation to existing movement. Level one deliberately keeps the original lesson.
public class CandyEnemyTactics:MonoBehaviour {
 EnemyBase enemy;Rigidbody2D body;float timer;bool busy;Vector2 origin;
 void Awake(){enemy=GetComponent<EnemyBase>();body=GetComponent<Rigidbody2D>();origin=transform.position;timer=2.5f+Mathf.Abs(transform.position.x)*.17f;}
 void Update(){var game=GameManager.Instance;if(!game||!game.Running||enemy.Dead)return;if(!game.level||game.level.number<2||game.level.iceKingdom||game.level.fireKingdom)return;if(busy)return;timer-=Time.deltaTime;if(timer>0)return;timer=5.5f+((int)enemy.kind%3);if(enemy.kind==EnemyKind.Cannon)return;StartCoroutine(Pattern());}
 IEnumerator Pattern(){busy=true;enemy.externalControl=true;enemy.combatTelegraph=true;var game=GameManager.Instance;var target=game.players[game.Wave%2];if(target.Dead)target=game.players[1-game.Wave%2];Vector2 aim=target.transform.position;float sign=Mathf.Sign(aim.x-transform.position.x);body.linearVelocity=Vector2.zero;BeamFX.Burst(transform.position,new Color(1,.75f,.2f),5);yield return new WaitForSeconds(.8f);enemy.combatTelegraph=false;if(enemy.Dead||!game.Running){Release();yield break;}enemy.combatAttack=true;
  if(enemy.kind==EnemyKind.Flyer){Vector2 start=body.position;for(float t=0;t<.65f;t+=Time.fixedDeltaTime){if(enemy.Dead||!game.Running){Release();yield break;}body.MovePosition(Vector2.Lerp(start,aim,Mathf.Sin(t/.65f*Mathf.PI*.5f)));yield return new WaitForFixedUpdate();}for(float t=0;t<.6f;t+=Time.fixedDeltaTime){if(enemy.Dead||!game.Running){Release();yield break;}body.MovePosition(Vector2.Lerp(aim,origin,t/.6f));yield return new WaitForFixedUpdate();}}
  else if(enemy.kind==EnemyKind.Jumper){body.linearVelocity=new Vector2(sign*3,10);yield return new WaitForSeconds(.55f);if(!enemy.Dead&&game.Running)EncounterHazard.Spawn(new Vector2(Mathf.Clamp(aim.x,-10,10),-4.05f),new Vector2(1.7f,.4f),.85f,.65f,Vector2.zero,new Color(1,.5f,.7f),this);}
  else if(enemy.kind==EnemyKind.Armored){for(int i=-1;i<=1;i+=2)EncounterHazard.Spawn((Vector2)transform.position+new Vector2(i*.8f,-.3f),new Vector2(.5f,.45f),.2f,1.5f,new Vector2(i*4,0),new Color(.9f,.65f,.4f),this,true);}
  else {for(float t=0;t<.45f;t+=Time.fixedDeltaTime){if(enemy.Dead||!game.Running){Release();yield break;}float x=body.position.x;body.linearVelocity=new Vector2(x<-10?3:x>10?-3:sign*(enemy.kind==EnemyKind.Chaser?7:4),body.linearVelocity.y);yield return new WaitForFixedUpdate();}}
  if(!enemy.Dead)body.linearVelocity=new Vector2(0,body.linearVelocity.y);enemy.combatAttack=false;yield return new WaitForSeconds(.8f);Release();
 }
 void Release(){if(enemy){enemy.externalControl=false;enemy.combatAttack=enemy.combatTelegraph=false;}busy=false;}
 void OnDisable(){StopAllCoroutines();Release();EncounterHazard.ClearOwned(this);}
}
}
