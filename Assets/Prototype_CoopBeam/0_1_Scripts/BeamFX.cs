using UnityEngine;
namespace CoopBeam {
public class BeamFX:MonoBehaviour {
 public static Sprite particle; Vector3 velocity;float life=.45f;SpriteRenderer sr;
 public static void Burst(Vector3 point,Color color,int count){if(!particle)return;for(int i=0;i<count;i++){var g=new GameObject("Light mote");g.transform.position=point;g.transform.localScale=Vector3.one*Random.Range(.04f,.12f);var r=g.AddComponent<SpriteRenderer>();r.sprite=particle;r.color=color;r.sortingOrder=12;var f=g.AddComponent<BeamFX>();f.sr=r;f.velocity=new Vector3(Random.Range(-3f,3f),Random.Range(.2f,4f),0);}}
 void Update(){life-=Time.deltaTime;if(life<=0){Destroy(gameObject);return;}transform.position+=velocity*Time.deltaTime;velocity+=Vector3.down*8*Time.deltaTime;var c=sr.color;c.a=life/.45f;sr.color=c;}
}}
