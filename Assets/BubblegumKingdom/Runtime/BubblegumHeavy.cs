using UnityEngine;
namespace CoopBeam {
// Rail-constrained heavy objects: only the large player's physical approach can push.
// Kinematic bodies prevent dashes, the small player and falling objects bypassing strength.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class BubblegumHeavy : MonoBehaviour {
 public PlayerController2D pusher;
 public float minX,maxX,speed=2.5f;
 public bool Locked;
 public Vector2 Home {get;private set;}
 Rigidbody2D body; Collider2D shape;
 void Awake(){body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();Home=body.position;}
 void FixedUpdate(){if(Locked||!pusher||!GameManager.Instance||!GameManager.Instance.Running)return;
 var pc=pusher.GetComponent<Collider2D>().bounds;var b=shape.bounds;
 float vx=pusher.GetComponent<Rigidbody2D>().linearVelocity.x;
 bool side=pc.max.y>b.min.y+.15f&&pc.min.y<b.max.y-.15f;
 bool right=pc.center.x<b.center.x&&b.min.x-pc.max.x<.18f&&vx>.1f;
 bool left=pc.center.x>b.center.x&&pc.min.x-b.max.x<.18f&&vx<-.1f;
 if(side&&(right||left))body.MovePosition(new Vector2(Mathf.Clamp(body.position.x+Mathf.Sign(vx)*speed*Time.fixedDeltaTime,minX,maxX),Home.y));
 }
 public void Restore(){Locked=false;body.position=Home;body.linearVelocity=Vector2.zero;transform.rotation=Quaternion.identity;}
 public void Place(Vector2 p){body.position=p;transform.position=p;}
}}
