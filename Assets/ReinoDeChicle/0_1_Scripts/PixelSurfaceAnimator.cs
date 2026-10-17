using UnityEngine;
namespace CoopBeam {
public class PixelSurfaceAnimator:MonoBehaviour {
 public Sprite[] frames;public float framesPerSecond=6;SpriteRenderer art;
 void Awake(){art=GetComponent<SpriteRenderer>();}
 void Update(){if(frames==null||frames.Length==0)return;Vector2 size=art.size;art.sprite=frames[(int)(Time.time*framesPerSecond)%frames.Length];art.size=size;}
}}
