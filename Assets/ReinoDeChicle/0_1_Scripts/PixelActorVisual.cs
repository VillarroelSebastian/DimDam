using UnityEngine;
namespace CoopBeam {
[DefaultExecutionOrder(500)]
public class PixelActorVisual:MonoBehaviour {
 SpriteRenderer art;Rigidbody2D body;Vector3 offset;EnemyBase enemy;PlayerHealth health;PlayerController2D player;float previousHealth,flash,echoAt;MaterialPropertyBlock properties;
 void Awake(){art=GetComponent<SpriteRenderer>();body=GetComponentInParent<Rigidbody2D>();enemy=GetComponentInParent<EnemyBase>();health=GetComponentInParent<PlayerHealth>();player=GetComponentInParent<PlayerController2D>();offset=transform.localPosition;properties=new MaterialPropertyBlock();previousHealth=enemy?enemy.Health:health?health.Hearts:0;}
 void LateUpdate(){if(!transform.parent)return;Vector3 desired=transform.parent.TransformPoint(offset);desired.x=Mathf.Round(desired.x*32)/32;desired.y=Mathf.Round(desired.y*32)/32;transform.position=desired;var s=transform.localScale;s.x=Mathf.Round(s.x*16)/16;s.y=Mathf.Round(s.y*16)/16;transform.localScale=s;transform.localRotation=Quaternion.identity;if(body&&Mathf.Abs(body.linearVelocity.x)>.2f)art.flipX=body.linearVelocity.x<0;
  flash-=Time.deltaTime;float current=enemy?enemy.Health:health?health.Hearts:0;if(current<previousHealth)flash=.085f;previousHealth=current;properties.SetFloat("_Flash",flash>0?1:0);art.SetPropertyBlock(properties);
  if(player&&player.Dashing&&Time.time>=echoAt){echoAt=Time.time+.04f;PixelDashEcho.Spawn(art,player.playerIndex==0?new Color(.72f,.4f,1):new Color(.6f,1,.3f));}
 }
}}

