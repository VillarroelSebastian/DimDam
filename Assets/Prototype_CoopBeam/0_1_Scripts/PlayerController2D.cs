using UnityEngine;
using UnityEngine.InputSystem;
namespace CoopBeam {
[RequireComponent(typeof(Rigidbody2D),typeof(CapsuleCollider2D))]
public class PlayerController2D : MonoBehaviour {
 public int playerIndex; public float speed=7, acceleration=55, jumpSpeed=13, dashSpeed=17, dashDuration=.14f, dashCooldown=.65f;
 public Transform visual; public SpriteRenderer sprite; public PlayerHealth health; public bool Grounded {get;private set;} public bool Dashing=>dashTime>0;
 Rigidbody2D body; Collider2D shape; float coyote,buffer,dashTime,cooldown,axis,face=1,dropUntil; bool jumpHeld; readonly RaycastHit2D[] hits=new RaycastHit2D[12];
 void Awake(){body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();}
 void Update(){
  if(!health || health.Dead || !GameManager.Instance || !GameManager.Instance.Running||CoopInteractionDirector.Locked(health))return;
  var k=Keyboard.current;if(k==null)return;
  axis=playerIndex==0?(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0):(k.rightArrowKey.isPressed?1:0)-(k.leftArrowKey.isPressed?1:0);
  var jump=playerIndex==0?k.wKey:k.upArrowKey; jumpHeld=jump.isPressed;if(jump.wasPressedThisFrame)buffer=.13f;
  if(axis!=0)face=Mathf.Sign(axis);
  if((playerIndex==0?k.leftShiftKey:k.rightCtrlKey).wasPressedThisFrame && cooldown<=0){dashTime=dashDuration;cooldown=dashCooldown;BeamFX.Burst(transform.position, sprite.color,5);}
  if((playerIndex==0?k.sKey:k.downArrowKey).wasPressedThisFrame&&!CoopInteractionDirector.Locked(health))StartCoroutine(Drop());
  buffer-=Time.deltaTime;cooldown-=Time.deltaTime;
 }
 System.Collections.IEnumerator Drop(){
  var platforms=Physics2D.OverlapBoxAll((Vector2)transform.position+Vector2.down*.57f,new Vector2(.65f,.25f),0,1<<9);
  foreach(var p in platforms)Physics2D.IgnoreCollision(shape,p,true);
  dropUntil=Time.time+.25f;body.position+=Vector2.down*.14f;body.linearVelocity=new Vector2(body.linearVelocity.x,-3);
  yield return new WaitForSeconds(.35f);foreach(var p in platforms)if(p&&shape)Physics2D.IgnoreCollision(shape,p,false);
 }
 void FixedUpdate(){
  if(!health||health.Dead||!GameManager.Instance||!GameManager.Instance.Running||CoopInteractionDirector.Locked(health)){body.linearVelocity=Vector2.zero;body.gravityScale=0;return;}
  var filter=new ContactFilter2D();filter.SetLayerMask((1<<8)|(1<<9));filter.useTriggers=false;
  Grounded=false;int n=shape.Cast(Vector2.down,filter,hits,.09f);for(int i=0;i<n;i++)if(hits[i].normal.y>.5f&&hits[i].collider.bounds.max.y<=shape.bounds.min.y+.13f&&body.linearVelocity.y<=.5f&&Time.time>dropUntil)Grounded=true;
  coyote=Grounded?.12f:coyote-Time.fixedDeltaTime;
  if(dashTime>0){dashTime-=Time.fixedDeltaTime;body.gravityScale=0;body.linearVelocity=new Vector2(face*dashSpeed,0);return;}
  body.gravityScale=3;
  if(health.Stunned)return;
  var v=body.linearVelocity;v.x=Mathf.MoveTowards(v.x,axis*speed,acceleration*Time.fixedDeltaTime);
  if(buffer>0&&coyote>0){v.y=jumpSpeed;buffer=0;coyote=0;BeamFX.Burst(transform.position+Vector3.down*.5f,Color.white,5);}
  if(!jumpHeld&&v.y>0)v.y-=38*Time.fixedDeltaTime;
  v.y=Mathf.Max(v.y,-22);body.linearVelocity=v;
  if(transform.position.y< -7)health.Hit(Vector2.up);
 }
 void LateUpdate(){if(!visual)return;if(CoopInteractionDirector.Active&&CoopInteractionDirector.Active.Dancing&&CoopInteractionDirector.Locked(health)){visual.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*8+playerIndex*3)*18);visual.localScale=new Vector3(1+Mathf.Sin(Time.time*10)*.08f,1-Mathf.Sin(Time.time*10)*.08f,1);sprite.color=Color.white;return;}float t=Time.time;Vector3 scale=Vector3.one;
  if(health.Dead)scale=new Vector3(1.25f,.12f,1);
  else if(Dashing)scale=new Vector3(1.35f,.65f,1);
  else if(!Grounded)scale=new Vector3(.88f,body.linearVelocity.y>0?1.12f:.95f,1);
  else {float s=Mathf.Sin(t*(Mathf.Abs(axis)>.1f?20:4))*(Mathf.Abs(axis)>.1f?.07f:.025f);scale=new Vector3(1-s,1+s,1);}
  visual.localScale=scale;visual.localRotation=Quaternion.Euler(0,0,Dashing?-face*12:-body.linearVelocity.x*1.2f);
  sprite.color=health.Invulnerable&&Mathf.Sin(t*45)>0?new Color(1,1,1,.3f):Color.white;
 }
 public void ResetMotion(){body.linearVelocity=Vector2.zero;dashTime=0;buffer=0;axis=0;}
}}
