using UnityEngine;
namespace CoopBeam {
public class NeonLift:MonoBehaviour {
 public NeonLevel level;public float travel=2.8f;Vector2 home;Rigidbody2D body;
 void Awake(){body=GetComponent<Rigidbody2D>();home=body.position;}
 void FixedUpdate(){if(!level||(!level.SocketA&&!level.Routed))return;Vector2 next=home+Vector2.up*(level.Routed?1:(.5f-.5f*Mathf.Cos(Time.time*1.2f)))*travel;Vector2 delta=next-body.position;foreach(var p in level.game.players){var c=p.GetComponent<Collider2D>();var b=GetComponent<Collider2D>().bounds;if(c.bounds.min.y>=b.max.y-.12f&&c.bounds.min.y<b.max.y+.16f&&Mathf.Abs(c.bounds.center.x-b.center.x)<b.extents.x+.1f){var rb=p.GetComponent<Rigidbody2D>();rb.position+=delta;}}body.MovePosition(next);}
 }
}
