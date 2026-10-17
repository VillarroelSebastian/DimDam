using System.Collections;
using UnityEngine;
namespace CoopBeam {
public class IceEnemyBrain:MonoBehaviour {
 public enum Role{Snowman,PolarBear,Penguin,Owl,Turret,Imp}public Role role;public float interval=4;public int AttacksPerformed{get;private set;}
 EnemyBase enemy;Rigidbody2D body;Vector2 origin,lockedAim;float clock,timer;bool busy;int target;
 void Awake(){enemy=GetComponent<EnemyBase>();body=GetComponent<Rigidbody2D>();origin=transform.position;enemy.externalControl=true;timer=2+Mathf.Abs(transform.position.x)*.15f;}
 void FixedUpdate(){var game=GameManager.Instance;if(!game||!game.Running||enemy.Dead){if(!enemy.Dead)body.linearVelocity=Vector2.zero;return;}clock+=Time.fixedDeltaTime;if(busy)return;timer-=Time.fixedDeltaTime;if(timer<=0){timer=interval;StartCoroutine(Attack());return;}
  if(role==Role.Owl){body.MovePosition(origin+new Vector2(Mathf.Sin(clock)*2,Mathf.Sin(clock*1.7f)*.6f));return;}if(role==Role.Turret||role==Role.Snowman){body.linearVelocity=new Vector2(0,body.linearVelocity.y);return;}
  var targetPlayer=game.players[target];float dir=targetPlayer.Dead?1:Mathf.Sign(targetPlayer.transform.position.x-body.position.x);float speed=role==Role.PolarBear?1.1f:role==Role.Penguin?1.8f:2.2f;float half=GetComponent<Collider2D>().bounds.extents.x;if(Physics2D.Raycast(body.position,Vector2.right*dir,half+.2f,1<<8))dir=-dir;body.linearVelocity=new Vector2(dir*speed,body.linearVelocity.y);if(body.position.y< -6)body.position=new Vector2(Mathf.Clamp(body.position.x,-9,9),-3.5f);
 }
 IEnumerator Attack(){busy=true;var game=GameManager.Instance;target=1-target;if(game.players[target].Dead)target=1-target;lockedAim=game.players[target].transform.position;enemy.combatTelegraph=true;body.linearVelocity=Vector2.zero;BeamFX.Burst(transform.position,new Color(1,.75f,.2f),6);float dir=Mathf.Sign(lockedAim.x-body.position.x);yield return new WaitForSeconds(role==Role.PolarBear?1.15f:.9f);enemy.combatTelegraph=false;if(enemy.Dead||!game.Running){busy=false;yield break;}enemy.combatAttack=true;AttacksPerformed++;
  if(role==Role.Snowman){for(int i=-1;i<=1;i++){var h=Shot(body.position+Vector2.up*.5f,new Vector2(dir*(3+i*.35f),6+i*.6f),.4f,3);if(h){h.gravity=7;h.snowball=true;}}}
  else if(role==Role.Turret){Vector2 d=(lockedAim-body.position).normalized;for(int i=-1;i<=1;i++){Vector2 v=Quaternion.Euler(0,0,i*22)*d;Shot(body.position+v*.7f,v*4,.32f,3);}}
  else if(role==Role.Owl){Vector2 start=body.position,end=new Vector2(Mathf.Clamp(lockedAim.x,-9,9),Mathf.Max(-3.3f,lockedAim.y));for(float t=0;t<1.1f;t+=Time.fixedDeltaTime){if(enemy.Dead||!game.Running)yield break;float a=t<.5f?t/.5f:1-(t-.5f)/.6f;body.MovePosition(Vector2.Lerp(start,end,Mathf.Clamp01(a)));yield return new WaitForFixedUpdate();}}
  else if(role==Role.Imp){body.linearVelocity=new Vector2(dir*4,11);yield return new WaitForSeconds(.55f);if(!enemy.Dead&&game.Running)EncounterHazard.Spawn(new Vector2(Mathf.Clamp(lockedAim.x,-9,9),-4.05f),new Vector2(1.5f,.45f),.9f,1.2f,Vector2.zero,Color.cyan,this);}
  else {float duration=role==Role.PolarBear?.65f:1.05f;for(float t=0;t<duration;t+=Time.fixedDeltaTime){if(enemy.Dead||!game.Running)yield break;float x=body.position.x;if(Mathf.Abs(x)>10||Physics2D.Raycast(body.position,Vector2.right*dir,.9f,1<<8))break;body.linearVelocity=new Vector2(dir*(role==Role.PolarBear?8:9),body.linearVelocity.y);yield return new WaitForFixedUpdate();}body.linearVelocity=new Vector2(0,body.linearVelocity.y);}
  enemy.combatAttack=false;yield return new WaitForSeconds(role==Role.PolarBear?1.5f:1);busy=false;
 }
 EncounterHazard Shot(Vector2 pos,Vector2 velocity,float size,float seconds)=>EncounterHazard.Spawn(pos,Vector2.one*size,.05f,seconds,velocity,new Color(.55f,.95f,1),this,true);
 void OnDisable(){StopAllCoroutines();EncounterHazard.ClearOwned(this);}
}
}
