using System.Collections.Generic;
using UnityEngine;
namespace CoopBeam {
// Announced amber footprint, then cyan damaging footprint. No invisible hitboxes.
public class EncounterHazard:MonoBehaviour {
 public Vector2 velocity;public float warning=.85f,duration=1,gravity;public bool clearable,snowball,fiery;public int fireStyle;public Object owner;public bool Active=>age>=warning;public static int LiveCount{get;private set;}
 float age;bool burst;SpriteRenderer art;Collider2D shape;Color color;Vector3 scale;
 public static EncounterHazard Spawn(Vector2 position,Vector2 size,float warning,float duration,Vector2 velocity,Color color,Object owner=null,bool clearable=false){if(LiveCount>=24)return null;var g=new GameObject("Announced hazard");g.transform.position=position;g.transform.localScale=new Vector3(size.x,size.y,1);var r=g.AddComponent<SpriteRenderer>();r.sprite=ExpansionSprites.Square;r.sortingOrder=9;var c=g.AddComponent<BoxCollider2D>();c.isTrigger=true;c.enabled=false;var rb=g.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;var h=g.AddComponent<EncounterHazard>();h.art=r;h.shape=c;h.color=color;h.scale=g.transform.localScale;h.warning=warning;h.duration=duration;h.velocity=velocity;h.owner=owner;h.clearable=clearable;LiveCount++;return h;}
 void Update(){var game=GameManager.Instance;if(!game||!game.Running){Clear();return;}age+=Time.deltaTime;if(age>=warning+duration){Clear();return;}shape.enabled=Active;if(!Active){art.color=fiery?new Color(.2f,1,1,.4f+Mathf.PingPong(age*3,.35f)):new Color(1,.72f,.2f,.3f+Mathf.PingPong(age*3,.35f));return;}if(fiery&&!burst){burst=true;FireFX.Burst(transform.position,new Color(1,.65f,.15f),5);}art.sprite=fiery?FireSprites.Frame(fireStyle,(int)(age*12)%4):ExpansionSprites.Hazard(clearable,snowball);art.color=color;velocity+=Vector2.down*gravity*Time.deltaTime;transform.position+=(Vector3)(velocity*Time.deltaTime);if(clearable&&(!fiery||fireStyle==0))transform.Rotate(0,0,150*Time.deltaTime);if(transform.position.y< -6||Mathf.Abs(transform.position.x)>13)Clear();}
 void OnTriggerEnter2D(Collider2D c){Hit(c);}void OnTriggerStay2D(Collider2D c){Hit(c);}void Hit(Collider2D c){if(!Active)return;var h=c.GetComponent<PlayerHealth>();if(h&&!h.Dead){h.Hit((Vector2)(h.transform.position-transform.position));if(clearable)Clear();}}
 public void Clear(){if(!this)return;Destroy(gameObject);}
 public static void ClearOwned(Object owner){foreach(var h in FindObjectsByType<EncounterHazard>())if(h.owner==owner)h.Clear();}
 void OnDestroy(){LiveCount=Mathf.Max(0,LiveCount-1);}
}
}
