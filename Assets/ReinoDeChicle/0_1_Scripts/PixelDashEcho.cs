using UnityEngine;
namespace CoopBeam {
public class PixelDashEcho:MonoBehaviour {
 float life=.18f;SpriteRenderer art;
 public static void Spawn(SpriteRenderer source,Color tint){var g=new GameObject("Pixel dash echo");g.transform.position=source.transform.position;g.transform.localScale=source.transform.lossyScale;var r=g.AddComponent<SpriteRenderer>();r.sprite=source.sprite;r.sharedMaterial=source.sharedMaterial;r.flipX=source.flipX;r.sortingOrder=source.sortingOrder-1;r.color=new Color(tint.r,tint.g,tint.b,.4f);g.AddComponent<PixelDashEcho>().art=r;}
 void Update(){life-=Time.deltaTime;if(life<=0){Destroy(gameObject);return;}var c=art.color;c.a=life/.18f*.4f;art.color=c;}
}}
