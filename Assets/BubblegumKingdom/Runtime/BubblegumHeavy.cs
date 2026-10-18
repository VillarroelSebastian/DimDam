using UnityEngine;
namespace CoopBeam {
// Rails move only into free space. The visual rolls, never the collision box.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class BubblegumHeavy : MonoBehaviour {
 public PlayerController2D pusher;
 public float minX,maxX,speed=2.5f;
 public bool Locked;
 public Vector2 Home {get;private set;}
 Rigidbody2D body; Collider2D shape; Transform art; float angle;
 readonly RaycastHit2D[] hits=new RaycastHit2D[16];
 void Awake(){body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();Home=body.position;if(transform.childCount>0)art=transform.GetChild(0);}
 void FixedUpdate(){
 if(Locked||!pusher||!GameManager.Instance||!GameManager.Instance.Running||pusher.health.Dead||pusher.Dashing)return;
 var pc=pusher.GetComponent<Collider2D>().bounds;var b=shape.bounds;
 float axis=pusher.MoveAxis;
 bool side=pc.max.y>b.min.y+.2f&&pc.min.y<b.max.y-.2f;
 float gap=axis>0?b.min.x-pc.max.x:pc.min.x-b.max.x;
 bool facing=axis>0?pc.center.x<b.center.x:pc.center.x>b.center.x;
 if(side&&facing&&gap>=-.08f&&gap<.2f&&Mathf.Abs(axis)>.1f){
 float x=Mathf.Clamp(body.position.x+Mathf.Sign(axis)*speed*Time.fixedDeltaTime,minX,maxX);
 TryMove(new Vector2(x,Home.y));
 }
 }
 public bool TryMove(Vector2 target){
 Vector2 delta=target-body.position;if(delta.sqrMagnitude<.000001f)return true;
 var filter=new ContactFilter2D();filter.SetLayerMask((1<<8)|(1<<9)|(1<<11));filter.useTriggers=false;
 int n=shape.Cast(delta.normalized,filter,hits,delta.magnitude+.035f);
 for(int i=0;i<n;i++){
 // Ground contact is tangential; only a surface facing the movement blocks it.
 if(hits[i].collider&&Vector2.Dot(hits[i].normal,delta.normalized)<-.1f)return false;
 }
 // Player layers are project-dependent: explicitly sweep their colliders as well.
 foreach(var player in GameManager.Instance.players){if(!player||player.Dead)continue;
 var c=player.GetComponent<Collider2D>();var bounds=shape.bounds;bounds.center+=(Vector3)delta;bounds.Expand(new Vector3(-.04f,-.04f,0));
 if(bounds.Intersects(c.bounds))return false;
 }
 body.MovePosition(target);angle-=delta.x*90;
 if(art)art.localRotation=Quaternion.Euler(0,0,Mathf.Round(angle/15)*15);return true;
 }
 public void Restore(){Locked=false;body.position=Home;transform.position=Home;body.linearVelocity=Vector2.zero;angle=0;if(art)art.localRotation=Quaternion.identity;}
 // Explicit placement is reserved for restoring and QA; gameplay uses the swept move.
 public void Place(Vector2 p){body.position=p;transform.position=p;}
}}
