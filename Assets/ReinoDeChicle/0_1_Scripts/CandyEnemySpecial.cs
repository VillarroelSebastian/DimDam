using UnityEngine;
namespace CoopBeam {
public class CandyEnemySpecial:MonoBehaviour {
 public Sprite projectile;public float flightWidth=2,flightHeight=.85f,shotInterval=3.8f;public Transform telegraph;public bool Telegraphing{get;private set;}
 public float LastShotTime{get;private set;}=-100;
 EnemyBase enemy;Rigidbody2D body;Vector2 origin;float clock,shotTime;int target;
 void Awake(){enemy=GetComponent<EnemyBase>();body=GetComponent<Rigidbody2D>();origin=transform.position;shotTime=shotInterval;}
 void FixedUpdate(){if(enemy.Dead||enemy.externalControl||!GameManager.Instance||!GameManager.Instance.Running)return;clock+=Time.fixedDeltaTime;
  if(enemy.kind==EnemyKind.Flyer){body.MovePosition(origin+new Vector2(Mathf.Sin(clock*1.2f)*flightWidth,Mathf.Sin(clock*2)*flightHeight));return;}
  shotTime-=Time.fixedDeltaTime;Telegraphing=shotTime<1;
  if(telegraph)telegraph.localScale=Vector3.one*(Telegraphing?.2f+Mathf.PingPong(clock*1.5f,.25f):.12f);
  if(shotTime<=0){target=1-target;var players=GameManager.Instance.players;if(players[target].Dead)target=1-target;Vector2 direction=((Vector2)players[target].transform.position-body.position).normalized;CandyProjectile.Fire(body.position+direction*.7f,direction*4.5f,projectile,enemy.sprite.sharedMaterial);shotTime=shotInterval;LastShotTime=Time.time;}
 }
}}
