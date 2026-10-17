using System.Collections.Generic;
using UnityEngine;
namespace CoopBeam {
public enum CandySurfaceKind {Bounce,Conveyor,Moving,Crumbly}
public class CandyEnvironment:MonoBehaviour {
 public CandySurfaceKind kind;public float strength=16,travel=2,period=4;public Vector2 moveAxis=Vector2.right;public Transform art;
 Vector2 origin,previous;Rigidbody2D body;Collider2D shape;float crumble=-1,clock;PhysicsMaterial2D supportMaterial;readonly HashSet<Rigidbody2D> riders=new HashSet<Rigidbody2D>();
 void Awake(){origin=previous=transform.position;body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();if(kind==CandySurfaceKind.Moving){supportMaterial=new PhysicsMaterial2D("Moving candy - manual rider transport"){friction=0,bounciness=0};shape.sharedMaterial=supportMaterial;}}
 void FixedUpdate(){if(!GameManager.Instance||!GameManager.Instance.Running)return;
 clock+=Time.fixedDeltaTime;
 if(kind==CandySurfaceKind.Moving){Vector2 next=origin+moveAxis*(Mathf.Sin(clock*Mathf.PI*2/period)*travel);Vector2 delta=next-previous;foreach(var rider in riders)if(rider)rider.position+=delta;body.MovePosition(next);previous=next;}
 if(kind==CandySurfaceKind.Conveyor)foreach(var rider in riders)if(rider)rider.position+=Vector2.right*strength*Time.fixedDeltaTime;
 if(kind==CandySurfaceKind.Crumbly&&crumble>=0){crumble+=Time.fixedDeltaTime;if(crumble>.8f){shape.enabled=false;if(art)art.gameObject.SetActive(false);riders.Clear();}if(crumble>3.8f){shape.enabled=true;if(art)art.gameObject.SetActive(true);crumble=-1;}}
 }
 void Update(){if(!art)return;if(kind==CandySurfaceKind.Bounce)art.localScale=new Vector3(1,1+Mathf.Sin(Time.time*5)*.09f,1);if(kind==CandySurfaceKind.Crumbly&&crumble>0&&crumble<.8f)art.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*60)*3);}
 void OnCollisionEnter2D(Collision2D c){Touch(c);}
 void OnCollisionStay2D(Collision2D c){Touch(c);}
 void Touch(Collision2D c){var pc=c.gameObject.GetComponent<PlayerController2D>();if(!pc||pc.health.Dead||!GameManager.Instance.Running)return;if(c.transform.position.y<transform.position.y+.35f)return;var rb=c.rigidbody;if(!rb)return;riders.Add(rb);
  if(kind==CandySurfaceKind.Bounce&&rb.linearVelocity.y<=.1f){rb.linearVelocity=new Vector2(rb.linearVelocity.x,strength);BeamFX.Burst(c.transform.position,new Color(.7f,1,.8f),7);}
  if(kind==CandySurfaceKind.Crumbly&&crumble<0)crumble=0;
 }
 void OnCollisionExit2D(Collision2D c){if(c.rigidbody)riders.Remove(c.rigidbody);}
 void OnDestroy(){if(supportMaterial)Destroy(supportMaterial);}
}
}
