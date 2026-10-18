using UnityEngine;
namespace CoopBeam {
// Manual descent keeps rescue bodies independent from spring, belt and crumble physics.
public class DownedBody : MonoBehaviour {
 public bool Settled { get; private set; }
 PlayerHealth health; Rigidbody2D body; Collider2D support; float speed; bool wasSimulated; float feet;
 public static bool TryFindRest(Vector2 from, out Vector2 rest, out Collider2D surface) {
  rest=from; surface=null; float best=float.NegativeInfinity;
  foreach(var hit in Physics2D.RaycastAll(from+Vector2.up*.05f,Vector2.down,30,(1<<8)|(1<<9))) {
   var c=hit.collider;
   if(!c||c.isTrigger||!c.enabled||hit.normal.y<.7f||c.bounds.size.x<.9f||c.GetComponentInParent<CandyEnvironment>())continue;
   if(c.attachedRigidbody&&c.attachedRigidbody.bodyType!=RigidbodyType2D.Static)continue;
   float top=c.bounds.max.y;if(top>from.y+.1f||top<=best)continue;
   best=top;surface=c;rest=new Vector2(Mathf.Clamp(from.x,c.bounds.min.x+.3f,c.bounds.max.x-.3f),top+.51f);
  }
  return surface;
 }
 public void Begin() {
  health=GetComponent<PlayerHealth>();body=GetComponent<Rigidbody2D>();wasSimulated=body.simulated;
  body.linearVelocity=Vector2.zero;body.simulated=false;speed=0;Settled=false;support=null;feet=.51f;enabled=true;
 }
 void FixedUpdate() {
  if(!health||!health.Dead)return;
  if(!GameManager.Instance||!GameManager.Instance.Running)return;
  Vector2 point=transform.position;Vector2 rest;Collider2D next;
  if(Settled&&support&&support.enabled&&support.gameObject.activeInHierarchy){point.y=support.bounds.max.y+feet;transform.position=point;return;}
  Settled=false;
  if(!TryFindRest(point,out rest,out next)) {
   point.x=Mathf.Clamp(point.x,-9.5f,9.5f);
   if(!TryFindRest(point,out rest,out next)) {
    point=health.spawn+Vector2.up*1.5f;
    if(!TryFindRest(point,out rest,out next)){transform.position=point;return;}
   }
   transform.position=point;
  }
  speed=Mathf.Min(14,speed+24*Time.fixedDeltaTime);
  point.y=Mathf.MoveTowards(point.y,rest.y,speed*Time.fixedDeltaTime);point.x=rest.x;
  if(Mathf.Abs(point.y-rest.y)<.025f){point=rest;Settled=true;support=next;speed=0;}
  transform.position=point;
 }
 public void Release(){if(body){body.simulated=wasSimulated;body.linearVelocity=Vector2.zero;body.gravityScale=3;}enabled=false;Settled=false;}
}
}
