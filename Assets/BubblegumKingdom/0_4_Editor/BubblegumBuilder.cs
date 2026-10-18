#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
public static class BubblegumBuilder {
 const string Art="Assets/ReinoDeChicle/0_3_Art/PixelArt/Tiles/";
 static Material material;static BubblegumLevel level;static Transform geometry,decor;
 static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png");
 static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
 static GameObject Visual(string name,Vector2 p,Vector2 size,string sprite,string color,int order,Transform parent=null){var g=new GameObject(name);g.transform.position=p;g.transform.SetParent(parent);var r=g.AddComponent<SpriteRenderer>();r.sprite=Sprite(sprite);if(!r.sprite)throw new Exception("Missing art: "+sprite);r.sharedMaterial=material;r.color=C(color);r.sortingOrder=order;g.transform.localScale=new Vector3(size.x/r.sprite.bounds.size.x,size.y/r.sprite.bounds.size.y,1);return g;}
 static GameObject Solid(string name,Vector2 p,Vector2 size,string sprite="Caramel",bool oneWay=false){
 var g=new GameObject(name);g.transform.SetParent(geometry);g.transform.position=p;g.layer=oneWay?9:8;var c=g.AddComponent<BoxCollider2D>();c.size=size;
 var v=Visual(name+" art",p,size,sprite,"FFFFFF",0,g.transform);
 if(oneWay){c.usedByEffector=true;var e=g.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.useOneWayGrouping=true;}
 return g;}
 static Transform Marker(string name,float x,float y,string art="SugarGear_0"){return Visual(name,new Vector2(x,y),Vector2.one*.65f,art,"FF9CB7",3,geometry).transform;}
 static void Label(string text,float x,float y,float scale=.14f){var g=new GameObject(text);g.transform.SetParent(decor);g.transform.position=new Vector3(x,y,0);var t=g.AddComponent<TextMesh>();t.text=text;t.fontSize=40;t.characterSize=scale;t.anchor=TextAnchor.MiddleCenter;t.color=C("50335F");g.GetComponent<MeshRenderer>().sortingOrder=6;}
 static BubblegumHeavy Heavy(string name,float x,float y,Vector2 size,float min,float max,string art){var g=Solid(name,new Vector2(x,y),size,art);var rb=g.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;var h=g.AddComponent<BubblegumHeavy>();h.minX=min;h.maxX=max;h.pusher=level.game.players[0].GetComponent<PlayerController2D>();return h;}
 static Transform Gate(string name,float x,float floor){var g=Solid(name,new Vector2(x,floor+5),new Vector2(.65f,10),"Chocolate");return g.transform;}
 static void Tunnel(float x){Solid("Techo del tunel: solo Pipo cabe debajo",new Vector2(x,1.65f),new Vector2(3,1.2f),"Marshmallow");Solid("Fondo del mecanismo",new Vector2(x+1.5f,.5f),new Vector2(.25f,1));Label("PIPO  ↓",x,2.6f);}
 [MenuItem("Coop Beam/Build Bubblegum Kingdom")]
 public static void Build(){
 if(EditorApplication.isPlaying)throw new Exception("Stop Play mode before building");
 if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save the current scene before building");
 material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_CoopBeam/0_3_Art/SpriteUnlit.mat");
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
 geometry=new GameObject("01 - Gameplay geometry and mechanisms").transform;decor=new GameObject("02 - Bubblegum kingdom scenery").transform;
 var manager=new GameObject("03 - Cooperative level controller");var game=manager.AddComponent<GameManager>();game.explorationOnly=true;game.particle=Sprite("GumBubble");game.players=new PlayerHealth[2];
 for(int i=0;i<2;i++){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype_CoopBeam/0_2_Prefabs/"+(i==0?"Coral":"Celeste")+".prefab");var p=(GameObject)PrefabUtility.InstantiatePrefab(prefab);p.name=i==0?"Mimo - existing Coral prefab":"Pipo - existing Celeste prefab";p.transform.position=new Vector2(1+i*1.5f,1);p.transform.localScale=Vector3.one*(i==0?1.35f:.8f);game.players[i]=p.GetComponent<PlayerHealth>();game.players[i].spawn=p.transform.position;PrefabUtility.RecordPrefabInstancePropertyModifications(p.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(game.players[i]);}
 level=manager.AddComponent<BubblegumLevel>();level.game=game;
 var camera=new GameObject("04 - Shared tracking camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6;cam.backgroundColor=C("F2DDF3");cam.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(7,3,-10);var follow=camera.AddComponent<BubblegumCamera>();follow.large=game.players[0].transform;follow.small=game.players[1].transform;
 var light=new GameObject("Main Light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=.6f;
 Solid("Safe bridge and castle approach",new Vector2(10,-.45f),new Vector2(24,.9f));Solid("Runaway valley floor",new Vector2(32,-.45f),new Vector2(20,.9f));Solid("Bubble garden floor",new Vector2(53,-.45f),new Vector2(22,.9f));
 Solid("Left boundary",new Vector2(-2,7),new Vector2(1,16),"Chocolate");Solid("Right boundary",new Vector2(79,8),new Vector2(1,18),"Chocolate");
 Tunnel(6.5f);level.auxiliary=Marker("Auxiliar - Pipo",6.5f,.45f);level.leverA=Marker("Palanca compuerta",10,.6f);level.gateA=Gate("Compuerta atascada",13,0);
 level.entryRock=Heavy("Roca pesada de entrada",10.5f,1.15f,new Vector2(2.2f,2.3f),9,11.3f,"GumBubble");Label("A · LA PUERTA ATASCADA",10,7);Label("S / ↓  PALANCA",10,3.4f);Label("EMPUJAR →",16,3);
 Solid("Escalon hacia la pendiente",new Vector2(22,.45f),new Vector2(2,.9f));Solid("Inicio de la pendiente",new Vector2(25,1),new Vector2(4,2));
 var ramp=Solid("Pendiente de chicle",new Vector2(29,.7f),new Vector2(5,.4f));ramp.transform.rotation=Quaternion.Euler(0,0,-22);
 level.runaway=Heavy("La roca fugitiva",25,3.15f,new Vector2(2.2f,2.3f),24,28,"GumBubble");level.leverB=Marker("Desvio: rojo barranco / verde placa",30,.55f);
 level.routeBlock=Heavy("Bloque guia",32,.65f,new Vector2(1.2f,1.3f),31.5f,35.2f,"Chocolate");level.routeMark=Marker("Marca bloque guia",35,.05f,"MintCore");
 level.plate=Visual("Plataforma de presion",new Vector2(39,.1f),new Vector2(3,.25f),"Gummy","FFF19A",2,geometry).transform;
 var sensor=level.plate.gameObject.AddComponent<BoxCollider2D>();sensor.isTrigger=true;sensor.size=new Vector2(3/level.plate.localScale.x,.4f/level.plate.localScale.y);
 level.gateB=Gate("Puerta del castillo",42,0);Label("B · LA ROCA FUGITIVA",31,7);Label("DESVIO  S / ↓",30,2.5f);Label("BLOQUE → MARCA",35,3.5f);Label("PLACA",39,2.8f);
 Visual("Ruta incorrecta al barranco",new Vector2(33,-1.7f),new Vector2(1,3.5f),"Chocolate","A47BAC",-1,decor).transform.rotation=Quaternion.Euler(0,0,35);
 Label("↘ RETORNO SEGURO",34,-2.8f);Label("CONTROL 1",44,3);
 Tunnel(46.5f);level.bubbleSwitch=Marker("Interruptor burbujas - Pipo",46.5f,.45f);level.step=Heavy("Escalon pesado",50,.7f,new Vector2(1.6f,1.4f),49.8f,52.2f,"Chocolate");level.stepMark=Marker("Marca del escalon",52,.05f,"MintCore");
 level.bubbles=new GameObject[3];for(int i=0;i<3;i++){var p=new Vector2(54+i*2.7f,3+i*1.25f);var b=Solid("Burbuja estable "+(i+1),p,new Vector2(2.4f,.4f),"GumBubble",true);var rb=b.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;var env=b.AddComponent<CandyEnvironment>();env.kind=CandySurfaceKind.Moving;env.moveAxis=Vector2.up;env.travel=.15f;env.period=4;env.art=b.transform.GetChild(0);level.bubbles[i]=b;}
 Solid("Refugio superior",new Vector2(64,6.5f),new Vector2(6,.5f),"Marshmallow",true);Solid("Camara del tesoro",new Vector2(73,6.5f),new Vector2(12,.5f),"Marshmallow",true);
 Label("C · LAS BURBUJAS FLOTANTES",53,8.8f);Label("MIMO → MARCA",51,2.3f);Label("CONTROL 2",65,8.7f);
 level.fragment=Visual("Primer fragmento del Corazon de la Amistad",new Vector2(70,7.8f),Vector2.one,"MintCore","FFE080",5,geometry).transform;
 level.exit=Marker("Salida cooperativa - The Neon Core",76,7.6f,"SugarGear_0");level.exit.localScale*=2;Label("D · EL PRIMER FRAGMENTO",70,11);Label("THE NEON CORE →\nSALIDA PARA DOS",75,9.5f);
 // Reuse pixel scenery, clearly behind all solid platforms.
 for(int i=0;i<16;i++){float x=1+i*5;Visual("Piruleta",new Vector2(x,2.7f),new Vector2(2.2f,5),"Lollipop",i%2==0?"F6ABC7":"BBE5D9",-8,decor);Visual("Arbusto pastel",new Vector2(x+1,.3f),new Vector2(2,.8f),"SugarCloud","BDE4D3",-5,decor);Visual("Burbuja ambiental",new Vector2(x,9+i%3),Vector2.one*(.4f+i%3*.2f),"GumBubble","FFFFFF",-10,decor);}
 for(int i=0;i<7;i++){float x=42+i*5;Visual("Torre de caramelo",new Vector2(x,6),new Vector2(3,12),"Marshmallow","D6B5DF",-20,decor);for(int j=0;j<3;j++)Visual("Almena de chicle",new Vector2(x-1+j,12.3f),new Vector2(.7f,1),"Caramel","EDB7D1",-19,decor);Visual("Ventana dorada",new Vector2(x,8),new Vector2(.6f,1.4f),"MintCore","FFE6A0",-18,decor);}
 for(int i=0;i<6;i++)Visual("Restos del puente",new Vector2(i*1.2f,-1.1f-i%2*.4f),new Vector2(.8f,.3f),"Caramel","C5A2C7",-5,decor);
 foreach(var r in decor.GetComponentsInChildren<SpriteRenderer>()){var tint=r.color;tint.a=.45f;r.color=tint;}
 EditorSceneManager.SaveScene(scene,BubblegumLevel.ScenePath);
 var builds=EditorBuildSettings.scenes.ToList();if(!builds.Any(s=>s.path==BubblegumLevel.ScenePath))builds.Add(new EditorBuildSettingsScene(BubblegumLevel.ScenePath,true));EditorBuildSettings.scenes=builds.ToArray();AssetDatabase.SaveAssets();Debug.Log("BUBBLEGUM_BUILD_OK: saved scene, existing prefabs, four puzzles, build registration.");
 }
}

#endif

