using UnityEngine;
namespace CoopBeam {
[RequireComponent(typeof(Rigidbody2D),typeof(Collider2D))]
public class ActorPhysics2D:MonoBehaviour {
 PhysicsMaterial2D material;
 void Awake(){material=new PhysicsMaterial2D("Candy actors - no wall grip"){friction=0,bounciness=0};GetComponent<Collider2D>().sharedMaterial=material;var body=GetComponent<Rigidbody2D>();body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;body.linearDamping=0;}
 void OnDestroy(){if(material)Destroy(material);}
}}
