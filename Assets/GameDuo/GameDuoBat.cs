using UnityEngine;
namespace CoopBeam {
public sealed class GameDuoBat : MonoBehaviour {
 public GameDuoCandyKing Owner {get;private set;}
 static Sprite[] frames;SpriteRenderer art;Rigidbody2D body;float speed,lane,age;int direction;
 public static GameDuoBat Spawn(GameDuoCandyKing owner,float x,float y,int direction,float speed,Material material){
  if(frames==null||!System.Array.TrueForAll(frames,frame=>frame))frames=GameDuoCandyKing.LoadFrames("Bats",1.3f,false);
  if(frames==null)return null;
  var go=new GameObject("Murcielago invocado / esquivar");go.transform.position=new Vector3(x,y,0);
  var art=go.AddComponent<SpriteRenderer>();art.sprite=frames[0];art.sharedMaterial=material;art.sortingOrder=9;art.flipX=direction>0;
  var shape=go.AddComponent<CircleCollider2D>();shape.radius=.29f;shape.isTrigger=true;
  var body=go.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
  var bat=go.AddComponent<GameDuoBat>();bat.Owner=owner;bat.art=art;bat.body=body;bat.lane=y;bat.speed=speed;bat.direction=direction;return bat;
 }
 void FixedUpdate(){
  if(!Owner||!Owner.enabled||Owner.GetComponent<EnemyBase>().Dead||!GameManager.Instance||!GameManager.Instance.Running){Destroy(gameObject);return;}
  age+=Time.fixedDeltaTime;
  body.MovePosition(new Vector2(body.position.x+direction*speed*Time.fixedDeltaTime,lane+Mathf.Sin(age*6)*.12f));
  art.sprite=frames[(int)(age*12)%8];
  if(age>7||Mathf.Abs(body.position.x)>14)Destroy(gameObject);
 }
 void OnTriggerEnter2D(Collider2D other){Hit(other);}
 void OnTriggerStay2D(Collider2D other){Hit(other);}
 void Hit(Collider2D other){var player=other.GetComponentInParent<PlayerHealth>();if(!player||player.Dead||player.Invulnerable)return;player.Hit(new Vector2(direction,.3f));BeamFX.Burst(transform.position,new Color(.7f,1,.5f),5);Destroy(gameObject);}
}
}
