using UnityEngine;
namespace CoopBeam {
public class CandyBoss:MonoBehaviour {
 public Collider2D weakSpot;public SpriteRenderer weakArt;public Sprite projectile;public int Phase{get;private set;}=1;public bool Vulnerable{get;private set;}public string Hint{get;private set;}
 public bool WindingUp{get;private set;}public float LastAttackTime{get;private set;}=-100;
 EnemyBase enemy;Rigidbody2D body;float clock,attackTimer=2;Vector2 origin;
 void Awake(){enemy=GetComponent<EnemyBase>();body=GetComponent<Rigidbody2D>();origin=transform.position;}
 void FixedUpdate(){if(!GameManager.Instance||!GameManager.Instance.Running)return;clock+=Time.fixedDeltaTime;
  Phase=enemy.Health>enemy.maxHealth*.66f?1:enemy.Health>enemy.maxHealth*.33f?2:3;
  float cycle=clock%(Phase==1?7:6);Vulnerable=cycle>2.1f&&cycle<5.6f;WindingUp=cycle<1.3f;weakSpot.enabled=Vulnerable;
  Hint=Vulnerable?"¡ATRAVIESEN EL NÚCLEO DE MENTA!":cycle<2.1f?"PREPARA UNA LLUVIA DE AZÚCAR":"SE REPOSICIONA";
  weakArt.color=Vulnerable?new Color(.65f,1,.75f):new Color(.5f,.35f,.6f,.4f);
  float y=Phase==1?0:Phase==2?.8f+Mathf.Sin(clock*.7f)*.6f:Mathf.Sin(clock*.8f)*1.1f;
  weakSpot.transform.localPosition=new Vector3(0,Mathf.Clamp(-.2f+y,-.75f,1.3f),0);weakArt.transform.localPosition=weakSpot.transform.localPosition;
  body.MovePosition(origin+new Vector2(Mathf.Sin(clock*(Phase==3?.65f:.35f))*(Phase==1?3:5.5f),0));
  var duo=GetComponent<GameDuoCandyKing>();if(duo&&duo.Summoning){WindingUp=true;Hint="MURCIELAGOS / BUSQUEN EL PASILLO ENTRE LAS LINEAS";}
  attackTimer-=Time.fixedDeltaTime;if(attackTimer<=0){for(int i=-1;i<=1;i++){var shot=CandyProjectile.Fire(body.position+new Vector2(i*.3f,1.4f),new Vector2(i*(Phase+1),5.5f),projectile,enemy.sprite.sharedMaterial,.38f);shot.gravity=5;}attackTimer=Phase==1?4:3;LastAttackTime=Time.time;GameManager.Instance.Tone(210,.12f);}
 }
 void OnDisable(){Vulnerable=false;}
}}
