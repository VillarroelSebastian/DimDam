using UnityEngine;
namespace CoopBeam {
public class FireSpriteAnimator:MonoBehaviour {
 public Sprite[] frames;public bool boss;EnemyBase enemy;FireForgeBoss forge;SpriteRenderer art;float hp,hurtUntil,deadAt=-1;
 void Awake(){enemy=GetComponentInParent<EnemyBase>();forge=GetComponentInParent<FireForgeBoss>();art=GetComponent<SpriteRenderer>();hp=enemy.Health;}
 void LateUpdate(){if(frames==null||frames.Length==0)return;if(enemy.Health<hp)hurtUntil=Time.time+.16f;hp=enemy.Health;int frame;if(boss)frame=forge.Defeated?Mathf.Min(15,14+(int)(forge.DefeatAge*1.2f)):Time.time<hurtUntil?13:forge.Vulnerable?12:forge.Telegraphing?4+(int)(Time.time*6)%4:forge.Attacking?8+forge.PatternIndex%4:(int)(Time.time*4)%4;else{if(enemy.Dead&&deadAt<0)deadAt=Time.time;frame=enemy.Dead?7:Time.time<hurtUntil?6:enemy.Telegraphing?4:enemy.Attacking?5:(int)(Time.time*7)%4;}art.sprite=frames[Mathf.Clamp(frame,0,frames.Length-1)];if(!boss&&enemy.Dead){var c=Color.white;c.a=Mathf.Clamp01(1-(Time.time-deadAt)/.42f);art.color=c;}else if(boss)art.color=forge.Phase==3?new Color(1,.8f,.65f):Color.white;}
}
}
