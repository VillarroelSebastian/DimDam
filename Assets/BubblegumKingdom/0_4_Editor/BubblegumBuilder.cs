#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
public static class BubblegumBuilder {
 const string Art="Assets/ReinoDeChicle/0_3_Art/PixelArt/Tiles/";
 const string Own="Assets/BubblegumKingdom/Art/";
 static Material material;static BubblegumLevel level;static Transform geometry,decor;
 static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>((name.StartsWith("BG_")?Own:Art)+name+".png");
 static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
 static GameObject Visual(string name,Vector2 p,Vector2 size,string sprite,string color,int order,Transform parent=null){var g=new GameObject(name);g.transform.position=p;g.transform.SetParent(parent);var r=g.AddComponent<SpriteRenderer>();r.sprite=Sprite(sprite);if(!r.sprite)throw new Exception("Missing art: "+sprite);r.sharedMaterial=material;r.color=C(color);r.sortingOrder=order;g.transform.localScale=new Vector3(size.x/r.sprite.bounds.size.x,size.y/r.sprite.bounds.size.y,1);return g;}
 static GameObject Solid(string name,Vector2 p,Vector2 size,string sprite="Caramel",bool oneWay=false){
 var g=new GameObject(name);g.transform.SetParent(geometry);g.transform.position=p;g.layer=oneWay?9:8;var c=g.AddComponent<BoxCollider2D>();c.size=size;
 var v=Visual(name+" art",p,size,sprite,"FFFFFF",0,g.transform);
 if(sprite=="Caramel"||sprite=="Chocolate"||sprite=="Marshmallow"){var r=v.GetComponent<SpriteRenderer>();v.transform.localScale=Vector3.one;r.drawMode=SpriteDrawMode.Tiled;r.size=size;}
 if(oneWay){c.usedByEffector=true;var e=g.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.useOneWayGrouping=true;}
 return g;}
 static Transform Marker(string name,float x,float y,string art="BG_Switch_0"){
 var g=Visual(name,new Vector2(x,y),Vector2.one*.7f,art,"FFFFFF",3,geometry);
 if(art=="BG_Switch_0"){var anim=g.AddComponent<PixelSurfaceAnimator>();anim.frames=Enumerable.Range(0,4).Select(i=>Sprite("BG_Switch_"+i)).ToArray();anim.framesPerSecond=4;}
 return g.transform;}
 static void Label(string text,float x,float y,float scale=.09f){var g=new GameObject(text);g.transform.SetParent(decor);g.transform.position=new Vector3(x,y,0);g.AddComponent<BubblegumWorldLabel>().text=text;}
 static BubblegumHeavy Heavy(string name,float x,float y,Vector2 size,float min,float max,string art){var g=Solid(name,new Vector2(x,y),size,art);var rb=g.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;var h=g.AddComponent<BubblegumHeavy>();h.minX=min;h.maxX=max;h.pusher=level.game.players[0].GetComponent<PlayerController2D>();return h;}
 static Transform Gate(string name,float floor){return Solid(name,new Vector2(8.8f,floor+1.5f),new Vector2(.5f,3),"BG_Gate").transform;}
 static void Tunnel(float y){Solid("Techo auxiliar: solo Pipo",new Vector2(-5,y+1.7f),new Vector2(3,1.2f),"Marshmallow");Solid("Fondo auxiliar",new Vector2(-3.5f,y+.5f),new Vector2(.25f,1));Label("PIPO",-5,y+2.6f);}
 static GameObject Bubble(float x,float y,int index){var g=Solid("Burbuja escalon "+index,new Vector2(x,y),new Vector2(2.6f,.4f),"BG_Bubble_0",true);var r=g.transform.GetChild(0).GetComponent<SpriteRenderer>();var a=r.gameObject.AddComponent<PixelSurfaceAnimator>();a.frames=Enumerable.Range(0,4).Select(i=>Sprite("BG_Bubble_"+i)).ToArray();a.framesPerSecond=5;return g;}
 static GameObject[] Stairs(float floor,float side=1){return new[]{Bubble(side*8,floor+2,0),Bubble(side*5.8f,floor+4,1),Bubble(side*8,floor+6,2),Bubble(side*(side<0?8:5.8f),floor+8,3)};}
 [MenuItem("Coop Beam/Build Bubblegum Kingdom")]
 public static void Build(){
 if(EditorApplication.isPlaying)throw new Exception("Stop Play mode before building");
 if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save the current scene before building");
 MakeArt();material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_CoopBeam/0_3_Art/SpriteUnlit.mat");
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
 geometry=new GameObject("01 - Torre cooperativa / cuatro pisos").transform;decor=new GameObject("02 - Reino de chicle / pixel scenery").transform;
 var manager=new GameObject("03 - Cooperative level controller");var game=manager.AddComponent<GameManager>();game.explorationOnly=true;game.particle=Sprite("PixelSpark");game.players=new PlayerHealth[2];
 for(int i=0;i<2;i++){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype_CoopBeam/0_2_Prefabs/"+(i==0?"Coral":"Celeste")+".prefab");var p=(GameObject)PrefabUtility.InstantiatePrefab(prefab);p.name=i==0?"Mimo - Mora / fuerza":"Pipo - Lima / mecanismos";p.transform.position=new Vector2(-8+i*1.5f,1);p.transform.localScale=Vector3.one*(i==0?1.35f:.8f);game.players[i]=p.GetComponent<PlayerHealth>();game.players[i].spawn=p.transform.position;var pc=p.GetComponent<PlayerController2D>();pc.interactionOnlyDown=true;PrefabUtility.RecordPrefabInstancePropertyModifications(pc);PrefabUtility.RecordPrefabInstancePropertyModifications(p.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(game.players[i]);}
 level=manager.AddComponent<BubblegumLevel>();level.game=game;
 var camera=new GameObject("04 - Shared vertical pixel camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6.2f;cam.backgroundColor=C("50345F");cam.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(0,3,-10);var follow=camera.AddComponent<BubblegumCamera>();follow.large=game.players[0].transform;follow.small=game.players[1].transform;camera.AddComponent<PixelViewport>();
 Solid("Base de la torre",new Vector2(0,-.4f),new Vector2(20,.8f));
 Solid("Piso del taller",new Vector2(0,9.7f),new Vector2(19,.6f),"Caramel",true);
 Solid("Piso del jardin",new Vector2(0,19.7f),new Vector2(19,.6f),"Marshmallow",true);
 Solid("Camara del corazon",new Vector2(-2,27.7f),new Vector2(14,.6f),"Marshmallow",true);
 Solid("Pared izquierda",new Vector2(-10,16),new Vector2(1,36),"Chocolate");Solid("Pared derecha",new Vector2(10,16),new Vector2(1,36),"Chocolate");
 Tunnel(0);level.auxiliary=Marker("Auxiliar Pipo",-5,.5f);level.leverA=Marker("Palanca A",-2.5f,.7f);level.gateA=Gate("Compuerta A",0);
 level.entryRock=Heavy("Mentita de chicle / entrada",-1,1.1f,new Vector2(2,2.2f),-1,0,"BG_Mint");Label("01 / LA MENTITA ATASCADA",0,5.7f);Label("MIMO  >  MARCA",2,3.4f);Marker("Destino mentita",3,.12f,"BG_Mark");level.ascentA=Stairs(0);
 Solid("Pedestal de salida mentita",new Vector2(-5,10.5f),new Vector2(3,1));
 level.runaway=Heavy("Mentita fugitiva",-5,12.1f,new Vector2(2,2.2f),-5,-2,"BG_Mint");
 level.leverB=Marker("Desvio verde / retorno",-1,10.65f);level.routeBlock=Heavy("Bombon guia",2,10.65f,new Vector2(1.2f,1.3f),1.5f,4.1f,"BG_Block");level.routeMark=Marker("Marca bloque",4,10.65f,"BG_Mark");
 level.plate=Visual("Placa de presion",new Vector2(7,10.12f),new Vector2(2.8f,.35f),"BG_Plate","FFFFFF",2,geometry).transform;
 var sensor=level.plate.gameObject.AddComponent<BoxCollider2D>();sensor.isTrigger=true;sensor.size=new Vector2(2.8f/level.plate.localScale.x,.6f/level.plate.localScale.y);
 level.gateB=Gate("Compuerta B",10);level.gateB.position=new Vector2(-8.8f,11.5f);level.ascentB=Stairs(10,-1);Label("02 / EL DESVIO DE AZUCAR",0,15.8f);Label("DESVIO",-1,12);Label("BLOQUE > MARCA",3,12.3f);
 // Dotted upper candy rail communicates the path without adding collision traps.
 for(int i=0;i<12;i++){float t=i/11f;Vector2 pos=t<.5f?Vector2.Lerp(new Vector2(0,11.2f),new Vector2(4,13.2f),t*2):Vector2.Lerp(new Vector2(4,13.2f),new Vector2(7,11.2f),(t-.5f)*2);Visual("Rail menta",pos,Vector2.one*.14f,"MintCore","94EBC5",-2,decor);}
 Tunnel(20);level.bubbleSwitch=Marker("Interruptor jardin Pipo",-5,20.5f);level.step=Heavy("Escalon de bombon",0,20.7f,new Vector2(1.6f,1.4f),-.2f,2.1f,"BG_Block");level.stepMark=Marker("Marca escalon",2,20.7f,"BG_Mark");
 level.bubbles=new[]{Bubble(4,22,0),Bubble(1,24,1),Bubble(-2,26,2)};Label("03 / JARDIN DE BURBUJAS",0,25);Label("MIMO > MARCA",1,22.5f);
 var root=new GameObject("Primer fragmento del Corazon de la Amistad");root.transform.SetParent(geometry);root.transform.position=new Vector2(-1,29.1f);Visual("Corazon pixel",root.transform.position,Vector2.one*1.1f,"BG_Heart","FFFFFF",5,root.transform);root.AddComponent<BubblegumFloat>();level.fragment=root.transform;
 level.exit=Marker("Salida cooperativa / The Neon Core",3.6f,28.9f,"BG_Gate");level.exit.localScale*=2;Label("04 / EL CORAZON DE LA AMISTAD",-2,32);Label("SALIDA PARA DOS",3,30.6f);
 // Reuse the actual kingdom backdrop and palette; tile terrain at native pixel density.
 for(int layer=0;layer<3;layer++){var bg=Visual("Reino de chicle / fondo original",new Vector2(0,5+layer*12),new Vector2(22,12.4f),"SugarCloud","FFFFFF",-30,decor).GetComponent<SpriteRenderer>();bg.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ReinoDeChicle/0_3_Art/PixelArt/Backgrounds/Background_01.png");bg.transform.localScale=new Vector3(22/bg.sprite.bounds.size.x,12.4f/bg.sprite.bounds.size.y,1);bg.color=new Color(.8f,.73f,.88f);}
 for(int floor=0;floor<4;floor++)for(int i=0;i<5;i++){float x=-8+i*4;var v=Visual("Piruleta de chicle",new Vector2(x,floor*9+2),new Vector2(1.4f,3.5f),"Lollipop",i%2==0?"E879AF":"94EBC5",-8,decor);v.GetComponent<SpriteRenderer>().color*=new Color(1,1,1,.35f);var b=Visual("Burbuja ambiental",new Vector2(x+.7f,floor*9+5),Vector2.one*.65f,"BG_Bubble_0","FFFFFF",-9,decor);b.AddComponent<BubblegumFloat>();}
 EditorSceneManager.SaveScene(scene,BubblegumLevel.ScenePath);
 var builds=EditorBuildSettings.scenes.ToList();if(!builds.Any(s=>s.path==BubblegumLevel.ScenePath))builds.Add(new EditorBuildSettingsScene(BubblegumLevel.ScenePath,true));EditorBuildSettings.scenes=builds.ToArray();AssetDatabase.SaveAssets();Debug.Log("BUBBLEGUM_BUILD_OK: vertical tower 20x34, mint candy assets, safe cooperative rails.");
 }
 // Small original pixel assets extending the kingdom's existing procedural tile system.
 static void MakeArt(){Directory.CreateDirectory(Own);
 Save("BG_Mint",32,32,(x,y)=>{float dx=x-15.5f,dy=y-15.5f,r=Mathf.Sqrt(dx*dx+dy*dy);if(r>15)return Color.clear;if(r>13)return C("39253E");if(r>11.8f)return C("FFF0CE");float a=Mathf.Atan2(dy,dx)+r*.2f;bool stripe=Mathf.Sin(a*5)>0;Color c=C(stripe?"E879AF":"94EBC5");if(y<9)c=C(stripe?"B65A91":"55AC9F");if(x>=8&&x<=12&&y>=22&&y<=25)c=C("FFF0CE");return c;});
 Save("BG_Block",24,24,(x,y)=>{if(x<1||x>22||y<1||y>22)return Color.clear;if(x<3||x>20||y<3||y>20)return C("39253E");if(y>18)return C("FFF0CE");if((x+y)%10<3)return C("E879AF");return C(y<7?"704862":"A66A88");});
 Save("BG_Mark",24,16,(x,y)=>{bool edge=x<2||x>21||y<2||y>13;return edge?C("F9CE7A"):(x+y)%6<2?new Color(.98f,.81f,.48f,.3f):Color.clear;});
 Save("BG_Plate",32,8,(x,y)=>y<2||x<2||x>29?C("39253E"):C(y>5?"FFF0CE":(x/4)%2==0?"94EBC5":"55AC9F"));
 Save("BG_Gate",16,32,(x,y)=>{bool edge=x<2||x>13||y<2||y>29;return edge?C("39253E"):C((x+y)/4%2==0?"E879AF":"FFF0CE");});
 Save("BG_Heart",24,24,(x,y)=>{bool fill=y<14?Mathf.Abs(x-11.5f)<y*.85f:(x-6)*(x-6)+(y-15)*(y-15)<35||(x-17)*(x-17)+(y-15)*(y-15)<35;if(!fill)return Color.clear;if(y<5||x<3||x>20)return C("704862");return C(x<10&&y>15?"FFF0CE":y<10?"B65A91":"E879AF");});
 for(int f=0;f<4;f++){int frame=f;Save("BG_Bubble_"+f,32,12,(x,y)=>{float dx=(x-15.5f)/15,dy=(y-5.5f)/5;float r=dx*dx+dy*dy;if(r>1)return Color.clear;if(r>.72f)return C(y>5?"FFF0CE":"704862");if(y==8&&(x+frame*3)%13<5)return C("FFF0CE");return C(y>5?"94EBC5":"72B8C0");});
 Save("BG_Switch_"+f,16,20,(x,y)=>{if(y<7)return x>2&&x<13?C(y<2?"39253E":"F9CE7A"):Color.clear;if(x>6&&x<10&&y<16)return C("704862");int dx=x-8,dy=y-15;return dx*dx+dy*dy<20?C(dx*dx+dy*dy>10?"39253E":frame%2==0?"94EBC5":"FFF0CE"):Color.clear;});}
 }
 static void Save(string name,int w,int h,Func<int,int,Color> pixel){var t=new Texture2D(w,h,TextureFormat.RGBA32,false);for(int y=0;y<h;y++)for(int x=0;x<w;x++)t.SetPixel(x,y,pixel(x,y));t.Apply();string path=Own+name+".png";File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spritePixelsPerUnit=16;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.alphaIsTransparency=true;imp.npotScale=TextureImporterNPOTScale.None;var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);imp.SaveAndReimport();}
}
#endif

