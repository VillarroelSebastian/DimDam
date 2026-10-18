using UnityEngine;
using UnityEngine.SceneManagement;
namespace CoopBeam {
public sealed class GameDuoIceCourt:MonoBehaviour {
 Transform[] banners=new Transform[2];SpriteRenderer[] lamps=new SpriteRenderer[4];float clock;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 static void Install(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void InitialScene(){Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
 static void Loaded(Scene scene,LoadSceneMode mode){var game=FindAnyObjectByType<GameManager>();if(game&&game.level&&game.level.iceKingdom&&game.level.boss&&!game.GetComponent<GameDuoIceCourt>())game.gameObject.AddComponent<GameDuoIceCourt>();}
 SpriteRenderer Art(string name,Vector2 position,Vector2 size,Sprite sprite,Color color,int order){
  var go=new GameObject(name);go.transform.SetParent(transform);go.transform.position=position;var art=go.AddComponent<SpriteRenderer>();art.sprite=sprite?sprite:ExpansionSprites.Square;art.color=color;art.sortingOrder=order;go.transform.localScale=new Vector3(size.x/art.sprite.bounds.size.x,size.y/art.sprite.bounds.size.y,1);return art;
 }
 void Start(){
  if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="03_ReyHielo"&&Resources.Load<Texture2D>("GameDuo/IceRefresh/Cavern")){RefreshCavern();return;}
  var ice=new Color(.35f,.66f,.85f);var distant=new Color(.25f,.4f,.65f,.8f);
  // Three depth planes extend the existing illustrated panorama into a royal courtyard.
  for(int i=-1;i<=1;i++)Art("Palacio / arco real "+i,new Vector2(i*8,1.8f),new Vector2(5.2f,8),IllustratedWorldLibrary.Sprite(1,29),distant,-27);
  for(int side=-1;side<=1;side+=2){
   for(int n=0;n<2;n++)Art("Columnas de la corte",new Vector2(side*(9.8f-n*2.6f),.3f),new Vector2(.65f,9),IllustratedWorldLibrary.Sprite(1,21),ice,-19);
   var banner=Art("Estandarte de la corona",new Vector2(side*8.5f,2),new Vector2(1.2f,2.9f),ExpansionSprites.Square,new Color(.16f,.16f,.4f),-15);banners[side<0?0:1]=banner.transform;
   Art("Emblema de cristal",new Vector2(side*8.5f,2.2f),new Vector2(.6f,.85f),IllustratedWorldLibrary.Sprite(1,27),Color.cyan,-14);
   // An optional intermediate foothold also makes the route readable for short jumps.
   Vector2 p=new Vector2(side*8.7f,-.3f);var platform=new GameObject("Escalon real / ruta accesible");platform.layer=9;platform.transform.SetParent(transform);platform.transform.position=p;
   var shape=platform.AddComponent<BoxCollider2D>();shape.size=new Vector2(2,.35f);shape.usedByEffector=true;
   var effector=platform.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.useOneWayGrouping=true;
   Art("Escalon de hielo tallado",p,new Vector2(2,.35f),IllustratedWorldLibrary.Sprite(1,24),Color.white,0);
  }
  Art("Trono real",new Vector2(0,-.1f),new Vector2(3.8f,6.2f),IllustratedWorldLibrary.Sprite(1,29),new Color(.6f,.85f,1),-13);
  for(int i=0;i<4;i++){float x=-9+i*6;Art("Pedestal de luz",new Vector2(x,-3.7f),new Vector2(.5f,1.1f),IllustratedWorldLibrary.Sprite(1,21),Color.white,-6);lamps[i]=Art("Llama de aurora",new Vector2(x,-2.95f),new Vector2(.45f,.65f),IllustratedWorldLibrary.Sprite(1,27),Color.cyan,-5);}
 }
 void RefreshCavern(){
  foreach(var r in FindObjectsByType<SpriteRenderer>()){
   string name=r.name.ToLowerInvariant();
   if(name.Contains("backdrop")||name.Contains("background")||name.Contains("panorama"))r.enabled=false;
  }
  var texture=Resources.Load<Texture2D>("GameDuo/IceRefresh/Cavern");
  var background=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,100);
  // Cover the camera without distorting the supplied panorama.
  var cam=Camera.main;float h=cam?cam.orthographicSize*2:15,w=h*(cam?cam.aspect:1.7778f);float scale=Mathf.Max(w/background.bounds.size.x,h/background.bounds.size.y);
  Art("Caverna de cristal / nuevo fondo",cam?(Vector2)cam.transform.position:new Vector2(0,1),new Vector2(background.bounds.size.x*scale,background.bounds.size.y*scale),background,Color.white,-70);
  int index=0;
  foreach(var shape in FindObjectsByType<BoxCollider2D>()){
   if(shape.isTrigger||(shape.gameObject.layer!=8&&shape.gameObject.layer!=9)||shape.bounds.size.x<1.5f||shape.bounds.size.y>1)continue;
   foreach(var old in shape.GetComponentsInChildren<SpriteRenderer>())old.enabled=false;
   AddShelf(shape,index++%4);
  }
  for(int side=-1;side<=1;side+=2){
   var go=new GameObject("Escalon real / ruta accesible");go.layer=9;go.transform.SetParent(transform);go.transform.position=new Vector2(side*8.7f,-.3f);
   var shape=go.AddComponent<BoxCollider2D>();shape.size=new Vector2(2,.35f);shape.usedByEffector=true;var effector=go.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.useOneWayGrouping=true;AddShelf(shape,side<0?2:3);
  }
 }
 void AddShelf(BoxCollider2D shape,int index){
  var texture=Resources.Load<Texture2D>("GameDuo/IceRefresh/Platform"+index);if(!texture)return;
  // Use only the snowy ledge below the crystal cluster. Its top is the collision top.
  float[] surface={570,510,575,585};
  float top=texture.height*(1-surface[index]/1280f);
  // Trim transparent margins at the actual walking edge, not the original canvas.
  int row=Mathf.Clamp(Mathf.FloorToInt(top)-1,0,texture.height-1),run=0,start=0,bestStart=0,bestLength=0;
  for(int x=0;x<texture.width;x++){
   if(texture.GetPixel(x,row).a>.8f){if(run==0)start=x;run++;if(run>bestLength){bestLength=run;bestStart=start;}}else run=0;
  }
  if(bestLength<texture.width/2){bestStart=Mathf.RoundToInt(texture.width*.1f);bestLength=Mathf.RoundToInt(texture.width*.8f);}
  var sprite=Sprite.Create(texture,new Rect(bestStart,0,bestLength,row+1),new Vector2(.5f,1),100);
  var go=new GameObject("Plataforma ilustrada / superficie alineada");go.transform.SetParent(shape.transform,true);go.transform.position=new Vector3(shape.bounds.center.x,shape.bounds.max.y,0);
  var art=go.AddComponent<SpriteRenderer>();art.sprite=sprite;art.sortingOrder=0;
  float s=shape.bounds.size.x/sprite.bounds.size.x;go.transform.localScale=new Vector3(s/shape.transform.lossyScale.x,s/shape.transform.lossyScale.y,1);
 }
 void Update(){clock+=Time.deltaTime;for(int i=0;i<2;i++)if(banners[i])banners[i].localRotation=Quaternion.Euler(0,0,Mathf.Sin(clock*1.5f+i)*2);for(int i=0;i<4;i++)if(lamps[i])lamps[i].color=Color.Lerp(new Color(.25f,.6f,1),new Color(.65f,1,1),.5f+Mathf.Sin(clock*3+i)*.5f);}
}
}
