using UnityEngine;
namespace CoopBeam {
public class IceSpriteAnimator:MonoBehaviour {
 public Sprite[] frames;public bool boss;EnemyBase enemy;IceKingBoss king;SpriteRenderer art;float previous,hurtUntil,deadAt=-1;
 void Awake(){enemy=GetComponentInParent<EnemyBase>();king=GetComponentInParent<IceKingBoss>();art=GetComponent<SpriteRenderer>();previous=enemy.Health;}
 void LateUpdate(){if(frames==null||frames.Length==0)return;if(enemy.Health<previous)hurtUntil=Time.time+.15f;previous=enemy.Health;int f;
 if(boss){f=king.Defeated?Mathf.Min(15,14+(int)(king.DefeatAge*1.4f)):Time.time<hurtUntil?13:king.Vulnerable?12:king.Telegraphing?4+(int)(Time.time*5)%4:king.Attacking?8+(int)(Time.time*7)%4:(int)(Time.time*3)%4;}
 else{if(enemy.Dead&&deadAt<0)deadAt=Time.time;f=enemy.Dead?7:Time.time<hurtUntil?6:enemy.Telegraphing?4:enemy.Attacking?5:(int)(Time.time*7)%4;}
 art.sprite=frames[Mathf.Clamp(f,0,frames.Length-1)];if(!boss&&enemy.Dead){var c=art.color;c.a=Mathf.Clamp01(1-(Time.time-deadAt)/.42f);art.color=c;}}
}
}
