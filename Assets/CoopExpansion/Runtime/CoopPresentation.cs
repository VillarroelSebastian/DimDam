using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace CoopBeam {
// Visible emission works with the existing unlit sprite art as well as the 2D renderer.
public class CoopPresentation:MonoBehaviour {
 GameManager g;CoopInteractionDirector director;LinkPowers powers;CoopUltimateModes modes;
 SpriteRenderer[] lamps=new SpriteRenderer[5];SpriteRenderer[] aura=new SpriteRenderer[2];Light2D[] auraLights=new Light2D[2];
 Material emission;float moteAt;
 public static Texture2D Glow {get;private set;}
 static Texture2D ring,flame; static Sprite star;
 public static Sprite Star {get {var illustrated=TutorialFactory.IsTutorial?null:IllustratedWorldLibrary.Sprite(0,27);if(illustrated)return illustrated;if(star)return star;var t=new Texture2D(16,16,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};for(int y=0;y<16;y++)for(int x=0;x<16;x++){float dx=Mathf.Abs(x-7.5f),dy=Mathf.Abs(y-7.5f);t.SetPixel(x,y,dx+dy<6.5f||((dx<1||dy<1)&&dx+dy<8)?Color.white:Color.clear);}t.Apply();return star=UnityEngine.Sprite.Create(t,new Rect(0,0,16,16),Vector2.one*.5f,16);}}
 public static Color PlayerColor(int i)=>i==0?new Color(1f,.12f,.59f):new Color(.65f,1f,.06f);
 void Awake(){g=GetComponent<GameManager>();director=GetComponent<CoopInteractionDirector>();powers=GetComponent<LinkPowers>();EnsureTextures();var template=Resources.Load<Material>("CoopPixelEmission");var shader=Shader.Find("Coop/PixelEmission");if(template)emission=new Material(template);else if(shader)emission=new Material(shader);
  for(int i=0;i<(TutorialFactory.IsTutorial?0:5);i++){Vector2 point;Collider2D surface;float x=-8+i*4;if(!DownedBody.TryFindRest(new Vector2(x,4.5f),out point,out surface))point=new Vector2(x,-3.5f);Color c=g.level&&g.level.fireKingdom?new Color(1,.32f,.06f):g.level&&g.level.iceKingdom?new Color(.2f,.65f,1):new Color(1,.3f,.7f);
   var root=new GameObject("Baliza luminosa "+i);root.transform.SetParent(transform);root.transform.position=point;var core=root.AddComponent<SpriteRenderer>();core.sprite=ExpansionSprites.Square;core.color=Color.Lerp(c,Color.white,.65f);core.sortingOrder=25;root.transform.localScale=new Vector3(.13f,.25f,1);
   var light=root.AddComponent<Light2D>();light.lightType=Light2D.LightType.Point;light.pointLightOuterRadius=20;light.intensity=1.1f;light.color=c;
   lamps[i]=Sprite("Resplandor visible",Glow,c,new Vector3(point.x,point.y+.2f,0),3.2f);lamps[i].color=new Color(c.r,c.g,c.b,.42f);
  }
  for(int i=0;i<2;i++){aura[i]=Sprite("Aura de energia "+i,flame,PlayerColor(i),g.players[i].transform.position,2.2f);aura[i].enabled=false;auraLights[i]=aura[i].gameObject.AddComponent<Light2D>();auraLights[i].lightType=Light2D.LightType.Point;auraLights[i].pointLightOuterRadius=1.4f;auraLights[i].intensity=.35f;auraLights[i].color=PlayerColor(i);}
 }
 SpriteRenderer Sprite(string name,Texture2D texture,Color color,Vector3 pos,float size){var o=new GameObject(name);o.transform.SetParent(transform);o.transform.position=pos;o.transform.localScale=Vector3.one*size;var s=o.AddComponent<SpriteRenderer>();s.sprite=UnityEngine.Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,texture.width);s.color=color;s.sortingOrder=40;if(emission)s.sharedMaterial=emission;return s;}
 public static void EnsureTextures(){if(Glow)return;Glow=new Texture2D(64,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};ring=new Texture2D(64,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};flame=new Texture2D(64,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};for(int y=0;y<64;y++)for(int x=0;x<64;x++){float dx=(x-31.5f)/31.5f,dy=(y-31.5f)/31.5f,r=Mathf.Sqrt(dx*dx+dy*dy);float a=Mathf.Pow(Mathf.Clamp01(1-r),1.5f);Glow.SetPixel(x,y,new Color(1,1,1,a));ring.SetPixel(x,y,new Color(1,1,1,Mathf.SmoothStep(0,1,Mathf.Clamp01((r-.66f)/.07f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((.9f-r)/.07f))));float theta=Mathf.Atan2(dy,dx);float edge=.79f+.07f*Mathf.Sin(theta*13)+.035f*Mathf.Sin(theta*23);float f=Mathf.Clamp01((edge-r)/.09f)*Mathf.Clamp01((r-.43f)/.18f);flame.SetPixel(x,y,new Color(1,1,1,f));}Glow.Apply();ring.Apply();flame.Apply();}
 void LateUpdate(){if(!director)director=GetComponent<CoopInteractionDirector>();if(!g||!g.Running){foreach(var s in aura)if(s)s.enabled=false;foreach(var l in auraLights)if(l)l.enabled=false;return;}modes=GetComponent<CoopUltimateModes>();bool active=director&&director.Dancing;float power=active?(modes&&modes.Active?modes.Power:director.DancePower):powers.Charge/LinkPowers.ChargeRequired;
  for(int i=0;i<2;i++){bool on=!g.players[i].Dead&&(active||director.HasAura(i)||powers.Charge>=LinkPowers.ChargeRequired||powers.Ultimate);aura[i].enabled=on;auraLights[i].enabled=on;var body=g.players[i].GetComponent<Collider2D>().bounds;aura[i].transform.position=body.center;float pulse=1+Mathf.Sin(Time.time*11+i)*.045f;aura[i].transform.localScale=new Vector3(Mathf.Max(1.5f,body.size.x*2.5f)/.72f,Mathf.Max(2f,body.size.y*1.8f)/.72f,1)*pulse;var c=PlayerColor(i);aura[i].color=new Color(c.r,c.g,c.b,active?.8f:.48f);}

  if((active||director.HasPending||powers.Charge>=LinkPowers.ChargeRequired||powers.Ultimate)&&Time.timeScale>0&&Time.time>=moteAt){moteAt=Time.time+.09f;for(int i=0;i<2;i++)FireFX.Burst(g.players[i].transform.position+new Vector3(Random.Range(-.7f,.7f),Random.Range(-.3f,1f),0),PlayerColor(i),2);}
  for(int i=0;i<lamps.Length;i++)if(lamps[i]){var c=lamps[i].color;c.a=.34f+Mathf.Sin(Time.time*2+i)*.06f;lamps[i].color=c;}
 }
 public static void DrawAura(float x,float y,float size,Color color,float clock){EnsureTextures();GUI.color=new Color(color.r,color.g,color.b,.3f);GUI.DrawTexture(new Rect(x-size*.85f,y-size*.85f,size*1.7f,size*1.7f),Glow);GUI.color=new Color(color.r,color.g,color.b,.9f);GUI.DrawTexture(new Rect(x-size*.5f,y-size*.65f,size,size*1.3f),flame);GUI.color=Color.Lerp(color,Color.white,.6f);GUI.DrawTexture(new Rect(x-size*.5f,y-size*.5f,size,size),ring);GUI.color=Color.white;}
 void OnGUI(){if(!g||!g.Running)return;GUI.depth=50;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/640f,Screen.height/360f,1));GUI.color=new Color(.025f,.015f,.09f,.12f);GUI.DrawTexture(new Rect(0,0,640,360),Texture2D.whiteTexture);GUI.color=Color.white;}
 void OnDestroy(){if(emission)Destroy(emission);}
}
}
