using UnityEngine;
namespace CoopBeam {
// Presentation only: state logic, attack telegraphs and collider transforms remain untouched.
[DefaultExecutionOrder(650)]
public class IllustratedMotion:MonoBehaviour {
 public bool surface;EnemyBase enemy;SpriteRenderer art;Vector3 rest;float seed;
 void Awake(){enemy=GetComponentInParent<EnemyBase>();art=GetComponent<SpriteRenderer>();rest=transform.localScale;seed=transform.name.GetHashCode()%19;}
 void LateUpdate(){if(Time.deltaTime<=0)return;float wave=Mathf.Sin(Time.time*(enemy&&enemy.Attacking?14:5)+seed);if(enemy){float squash=enemy.Telegraphing?.1f:enemy.Attacking?.12f:.025f;var scale=rest;if(enemy.Dead)scale=Vector3.Scale(rest,new Vector3(1.12f,.65f,1));else scale=Vector3.Scale(rest,new Vector3(1+wave*squash,1-wave*squash,1));transform.localScale=scale;transform.localRotation=Quaternion.Euler(0,0,enemy.Telegraphing?wave*4:enemy.Attacking?wave*7:0);}else if(surface&&art){var c=art.color;c.a=1;art.color=c;}}
}
}
