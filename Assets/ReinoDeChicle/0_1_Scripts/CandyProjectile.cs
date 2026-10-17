using UnityEngine;
namespace CoopBeam {
public class CandyProjectile:MonoBehaviour {
 public Vector2 velocity;public float lifetime=5;public float gravity;SpriteRenderer art;
 void Awake(){art=GetComponent<SpriteRenderer>();}
 void Update(){if(!GameManager.Instance||!GameManager.Instance.Running){Destroy(gameObject);return;}lifetime-=Time.deltaTime;if(lifetime<=0){Destroy(gameObject);return;}velocity+=Vector2.down*gravity*Time.deltaTime;transform.position+=(Vector3)(velocity*Time.deltaTime);transform.Rotate(0,0,150*Time.deltaTime);}
 void OnTriggerEnter2D(Collider2D c){var h=c.GetComponent<PlayerHealth>();if(h){h.Hit(velocity.normalized);BeamFX.Burst(transform.position,new Color(1,.7f,.8f),4);Destroy(gameObject);}}
 public static CandyProjectile Fire(Vector2 point,Vector2 velocity,Sprite sprite,Material material,float size=.3f){var g=new GameObject("Sugar pearl");g.transform.position=point;g.transform.localScale=Vector3.one*size;var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=material;sr.color=new Color(1,.7f,.82f);sr.sortingOrder=8;var c=g.AddComponent<CircleCollider2D>();c.radius=.4f;c.isTrigger=true;var body=g.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;var p=g.AddComponent<CandyProjectile>();p.velocity=velocity;return p;}
}}
