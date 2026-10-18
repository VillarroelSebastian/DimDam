using UnityEngine;
namespace CoopBeam {
public static class FireSprites {
 static Sprite[,] frames=new Sprite[3,4];
 public static Sprite Frame(int style,int frame){var illustrated=IllustratedWorldLibrary.Sprite(2,style==0?26:27);if(illustrated)return illustrated;style=Mathf.Clamp(style,0,2);frame%=4;if(frames[style,frame])return frames[style,frame];var t=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};for(int y=0;y<32;y++)for(int x=0;x<32;x++){float u=(x/2*2-15f)/16,v=(y/2*2)/31f;float bend=Mathf.Sin(v*9+frame*1.6f)*.14f;bool fill=style==0?u*u+(v-.5f)*(v-.5f)*4<.8f:Mathf.Abs(u-bend)<(1-v)*.9f&&v<.96f;float core=Mathf.Abs(u-bend)+(v*.5f);Color c=!fill?Color.clear:core<.24f?new Color(1,1,.75f):core<.5f?new Color(1,.85f,.18f):core<.72f?new Color(1,.35f,.06f):new Color(.8f,.08f,.12f);t.SetPixel(x,y,c);}t.Apply();return frames[style,frame]=Sprite.Create(t,new Rect(0,0,32,32),Vector2.one*.5f,32);}
}
public class FireFX:MonoBehaviour {
 static int live;SpriteRenderer art;Vector3 velocity;float age,duration=.55f;Color color;
 public static void Burst(Vector3 point,Color color,int count){for(int i=0;i<count&&live<180;i++){var g=new GameObject("Pixel ember");g.transform.position=point;g.transform.localScale=Vector3.one*Random.Range(.06f,.18f);var r=g.AddComponent<SpriteRenderer>();r.sprite=FireSprites.Frame(1,i%4);r.sortingOrder=16;var f=g.AddComponent<FireFX>();f.art=r;f.color=color;f.velocity=(Vector3)(Random.insideUnitCircle*3)+Vector3.up*2;live++;}}
 void Update(){age+=Time.deltaTime;if(age>=duration){Destroy(gameObject);return;}transform.position+=velocity*Time.deltaTime;velocity+=Vector3.down*4*Time.deltaTime;var c=color;c.a=1-age/duration;art.color=c;art.sprite=FireSprites.Frame(1,(int)(age*12)%4);}
 void OnDestroy(){live=Mathf.Max(0,live-1);}
}
public static class FireHazard {
 public static EncounterHazard Spawn(Vector2 point,Vector2 size,float warning,float duration,Vector2 velocity,Object owner,int style=0,bool clearable=true,float gravity=0){var h=EncounterHazard.Spawn(point,size,warning,duration,velocity,Color.white,owner,clearable);if(h){h.fiery=true;h.fireStyle=style;h.gravity=gravity;}return h;}
}
}
