using UnityEngine;
using UnityEngine.SceneManagement;
namespace CoopBeam {
// Tutorial throne illustration, matching collision surfaces and frightened royal animation.
[DefaultExecutionOrder(1400)]
public sealed class TutorialFactory : MonoBehaviour {
 public static bool IsTutorial => SceneManager.GetActiveScene().path==GameDuoCampaign.Scenes[0];
 static Sprite disc, tile, haze;
 public static Color Ink=new Color(.13f,.105f,.20f);
 [SerializeField] Transform king; [SerializeField] Transform[] gears=new Transform[8]; [SerializeField] SpriteRenderer[] motes=new SpriteRenderer[28];
 public static Sprite Shape(int type){
  if(type==0&&tile)return tile;if(type==1&&disc)return disc;if(type==2&&haze)return haze;
  var saved=Resources.Load<Sprite>("GameDuo/FactoryShape"+type);if(saved){if(type==0)tile=saved;else if(type==1)disc=saved;else haze=saved;return saved;}
  int n=type==0?2:128;var t=new Texture2D(n,n,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),Vector2.one*n*.5f)/(n*.5f);float a=type==0?1:type==1?Mathf.Clamp01((1-d)*n*.5f):Mathf.Pow(Mathf.Clamp01(1-d),2)*.45f;t.SetPixel(x,y,new Color(1,1,1,a));}
  t.Apply();var s=Sprite.Create(t,new Rect(0,0,n,n),Vector2.one*.5f,n);s.name="Factory shape "+type;if(type==0)tile=s;else if(type==1)disc=s;else haze=s;return s;
 }
 public static SpriteRenderer Part(Transform parent,string name,float x,float y,float w,float h,Color color,int order,int shape=0,float angle=0){
  var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.localPosition=new Vector3(x,y,0);o.transform.localScale=new Vector3(w,h,1);o.transform.localRotation=Quaternion.Euler(0,0,angle);var r=o.AddComponent<SpriteRenderer>();r.sprite=Shape(shape);r.color=color;r.sortingOrder=order;return r;
 }
 static Color C(float r,float g,float b)=>new Color(r,g,b);
 static readonly int[] FearSequence={0,1,0,2,2,1,3,4,4,3,1,0};
 [SerializeField] Sprite[] fear;
 [SerializeField] SpriteRenderer royal;
 [SerializeField] Vector3 royalRest;
 public static Vector2 ImagePoint(float x,float y)=>new Vector2((x/1672f-.5f)*26.65f,7.0f-y/941f*15f);
 static Sprite Art(string name){var sprite=Resources.Load<Sprite>("GameDuo/Throne/"+name);if(!sprite)throw new System.InvalidOperationException("Missing throne art: "+name);return sprite;}
 void Start(){if(!royal||fear==null||fear.Length!=5||!transform.Find("Salon del trono"))Build();}
 public void Build(){
  foreach(Transform child in transform)child.gameObject.SetActive(false);
  foreach(var r in FindObjectsByType<SpriteRenderer>())if(!r.GetComponentInParent<PlayerController2D>()&&!r.GetComponentInParent<EnemyBase>())r.enabled=false;
  foreach(var b in FindObjectsByType<CandyAmbientLife>())b.enabled=false;
  foreach(var l in FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>())if(l.lightType!=UnityEngine.Rendering.Universal.Light2D.LightType.Global)l.gameObject.SetActive(false);
  var bg=new GameObject("Salon del trono");bg.transform.SetParent(transform,false);bg.transform.localPosition=new Vector3(0,-.5f,0);
  var backdrop=bg.AddComponent<SpriteRenderer>();backdrop.sprite=Art("Hall");backdrop.sortingOrder=-100;
  bg.transform.localScale=new Vector3(26.65f/backdrop.sprite.bounds.size.x,15/backdrop.sprite.bounds.size.y,1);
  // Physical surfaces use the exact same image-to-world mapping as the painted steps.
  foreach(var box in FindObjectsByType<BoxCollider2D>()){
   if(box.gameObject.layer!=8&&box.gameObject.layer!=9)continue;
   if(box.isTrigger)continue;
   box.enabled=false;
  }
  Surface("Suelo del salon",0,1672,709,false);
  Surface("Escalon trono inferior",650,958,645,true);
  Surface("Escalon trono medio",678,930,615,true);
  Surface("Escalon trono superior",704,902,588,true);
  Surface("Escalera derecha 1",1330,1672,652,true);
  Surface("Escalera derecha 2",1375,1672,622,true);
  Surface("Escalera derecha 3",1420,1672,592,true);
  Surface("Escalera derecha 4",1460,1672,563,true);
  Surface("Escalera derecha 5",1480,1672,539,true);
  Surface("Pedestal izquierdo",0,176,551,true);
  Platform("Plataforma oeste",-8.2f,-1.9f,3.3f);
  Platform("Plataforma oeste alta",-4.8f,.1f,3.0f);
  Platform("Plataforma central alta",.6f,1.7f,3.2f);
  Platform("Plataforma este alta",5f,.1f,3.0f);
  Platform("Plataforma este",8.7f,-1.9f,3.3f);
  king=new GameObject("Rey Chicle / asustado").transform;king.SetParent(transform,false);
  royalRest=ImagePoint(750,588)+Vector2.up*1.35f;king.localPosition=royalRest;
  royal=king.gameObject.AddComponent<SpriteRenderer>();royal.sortingOrder=-20;
  fear=new Sprite[5];for(int i=0;i<5;i++)fear[i]=Art("Fear"+i);royal.sprite=fear[0];king.localScale=Vector3.one*(2.9f/fear[0].bounds.size.y);
  var cam=Camera.main;if(cam){var framing=cam.GetComponent<ArenaCamera>();if(framing)framing.enabled=false;cam.orthographicSize=7.5f;cam.transform.position=new Vector3(0,-.5f,cam.transform.position.z);}
 }
 void Surface(string name,float left,float right,float top,bool oneWay){
  var a=ImagePoint(left,top);var b=ImagePoint(right,top);
  var o=new GameObject(name);o.layer=oneWay?9:8;o.transform.SetParent(transform,false);o.transform.position=new Vector3((a.x+b.x)*.5f,a.y-.12f,0);
  var box=o.AddComponent<BoxCollider2D>();box.size=new Vector2(b.x-a.x,.24f);
  if(oneWay){box.usedByEffector=true;var eff=o.AddComponent<PlatformEffector2D>();eff.useOneWay=true;eff.surfaceArc=175;}
 }
 void Platform(string name,float x,float y,float width){
  var root=new GameObject(name);root.layer=9;root.transform.SetParent(transform,false);root.transform.localPosition=new Vector3(x,y-.18f,0);
  var box=root.AddComponent<BoxCollider2D>();box.size=new Vector2(width,.36f);box.usedByEffector=true;var eff=root.AddComponent<PlatformEffector2D>();eff.useOneWay=true;eff.surfaceArc=175;
  Part(root.transform,"Chocolate / borde",0,0,width,.36f,new Color(.28f,.12f,.17f),1);
  Part(root.transform,"Galleta",0,-.04f,width-.08f,.22f,new Color(.77f,.44f,.29f),2);
  for(float at=-width*.5f+.15f;at<width*.5f;at+=.34f)Part(root.transform,"Relieve de galleta",at,-.055f,.025f,.18f,new Color(.48f,.24f,.22f),3,0,15);
  Part(root.transform,"Glaseado rosa",0,.1f,width,.14f,new Color(1,.57f,.70f),4);
  Part(root.transform,"Borde dorado transitable",0,.17f,width,.02f,new Color(1,.85f,.51f),5);
  for(int side=-1;side<=1;side+=2)Part(root.transform,"Soporte de caramelo",side*(width*.5f-.24f),-.27f,.2f,.23f,new Color(.88f,.61f,.31f),1,1);
 }
 void Update(){
  if(!king||fear==null||royal==null)return;
  bool safe=GameManager.Instance&&GameManager.Instance.Victory;
  float t=Time.time;
  royal.sprite=fear[safe?0:FearSequence[(int)(t*2.5f)%FearSequence.Length]];
  float tremble=safe?0:Mathf.Sin(t*29)*.018f;
  king.localPosition=royalRest+new Vector3(tremble,Mathf.Sin(t*4)*.012f,0);
  king.localRotation=Quaternion.Euler(0,0,safe?0:Mathf.Sin(t*23)*.65f);
 }
}
}
