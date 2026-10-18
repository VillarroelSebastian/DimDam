#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoopBeam;
public static class NeonBuilder {
 const string Root="Assets/ReinoNeon/";static Material mat;static NeonLevel level;static Transform terrain,decor;
 static Color C(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
 static Sprite Art(string s)=>AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/"+s+".png");
 static GameObject Visual(string name,Vector2 p,Vector2 size,string art,int order=0,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent?parent:decor);g.transform.position=p;var r=g.AddComponent<SpriteRenderer>();r.sprite=Art(art);if(!r.sprite)throw new Exception("Missing neon art "+art);r.sharedMaterial=mat;r.sortingOrder=order;r.drawMode=(art=="Spark"||art.StartsWith("Core")||art=="Battery"||art=="Heart"||art=="Portal"||art.StartsWith("Panel")||art.StartsWith("Door"))?SpriteDrawMode.Sliced:SpriteDrawMode.Tiled;r.size=size;return g;}
 static GameObject Solid(string n,Vector2 p,Vector2 size,string art="Hull",bool oneWay=false){var g=Visual(n,p,size,art,0,terrain);g.layer=oneWay?9:8;var c=g.AddComponent<BoxCollider2D>();c.size=size;if(oneWay){c.usedByEffector=true;var e=g.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.useOneWayGrouping=true;}return g;}
 static void Label(string s,float x,float y){var g=new GameObject(s);g.transform.SetParent(decor);g.transform.position=new Vector2(x,y);g.AddComponent<BubblegumWorldLabel>().text=s;}
 static Transform Marker(string n,float x,float y,string art="PanelOff",Vector2? size=null){return Visual(n,new Vector2(x,y),size??Vector2.one,art,3).transform;}
 static NeonDevice Device(string n,float x,float y,bool door=false,int player=-1){var g=door?Solid(n,new Vector2(x,y),new Vector2(.5f,3),"DoorOff"):Visual(n,new Vector2(x,y),new Vector2(.75f,.75f),"PanelOff",3);var d=g.AddComponent<NeonDevice>();d.player=player;d.lamp=g.GetComponent<SpriteRenderer>();d.offSprite=Art(door?"DoorOff":"PanelOff");d.onSprite=Art(door?"DoorOn":"PanelOn");if(door)d.blocker=g.GetComponent<Collider2D>();return d;}
 static void Wire(NeonDevice source,Vector2 end){var start=(Vector2)source.transform.position;var corner=new Vector2(start.x+.6f,end.y);var points=new System.Collections.Generic.List<Transform>();int steps=Mathf.CeilToInt(Vector2.Distance(start,corner)/.3f);for(int i=0;i<=steps;i++)points.Add(Visual("Cable / pulso",Vector2.Lerp(start,corner,(float)i/steps),Vector2.one*.16f,"Spark",-2).transform);steps=Mathf.CeilToInt(Vector2.Distance(corner,end)/.3f);for(int i=0;i<=steps;i++)points.Add(Visual("Cable / pulso",Vector2.Lerp(corner,end,(float)i/Mathf.Max(1,steps)),Vector2.one*.16f,"Spark",-2).transform);source.cable=points.ToArray();}
 [MenuItem("Coop Beam/Neon/Build ReinoNeon")]
 public static void Build(){
 if(EditorApplication.isPlaying)throw new Exception("Stop Play mode before building");
 var previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(previous.isDirty)throw new Exception("Current scene has unsaved changes");
 if(previous.path==NeonLevel.ScenePath)previous=EditorSceneManager.OpenScene(BubblegumLevel.ScenePath);
 MakeArt();mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_CoopBeam/0_3_Art/SpriteUnlit.mat");
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
 terrain=new GameObject("01 / Superficies y rutas").transform;decor=new GameObject("02 / Instalacion de circuitos").transform;
 var root=new GameObject("03 / The Neon Core - cooperative state");var gm=root.AddComponent<GameManager>();gm.explorationOnly=true;gm.particle=Art("Spark");gm.players=new PlayerHealth[2];level=root.AddComponent<NeonLevel>();level.game=gm;
 // Use the established kingdom instances to retain the exact character appearance and ability setup.
 var originals=previous.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PlayerController2D>(true)).OrderBy(p=>p.playerIndex).ToArray();
 for(int i=0;i<2;i++){
 GameObject p;if(originals.Length==2){var source=PrefabUtility.GetCorrespondingObjectFromSource(originals[i].gameObject);if(source){p=(GameObject)PrefabUtility.InstantiatePrefab(source,scene);PrefabUtility.SetPropertyModifications(p,PrefabUtility.GetPropertyModifications(originals[i].gameObject));}else{p=UnityEngine.Object.Instantiate(originals[i].gameObject);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(p,scene);}}else{p=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype_CoopBeam/0_2_Prefabs/"+(i==0?"Coral":"Celeste")+".prefab"),scene);p.transform.localScale=Vector3.one*(i==0?1.35f:.8f);}
 p.name=i==0?"Mimo / original":"Pipo / original";p.transform.position=new Vector2(-8-i*.9f,1);gm.players[i]=p.GetComponent<PlayerHealth>();gm.players[i].spawn=p.transform.position;p.GetComponent<PlayerController2D>().interactionOnlyDown=true;PrefabUtility.RecordPrefabInstancePropertyModifications(p.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(gm.players[i]);PrefabUtility.RecordPrefabInstancePropertyModifications(p.GetComponent<PlayerController2D>());
 }
 var camera=new GameObject("04 / Shared horizontal pixel camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5.5f;cam.backgroundColor=C("0C1026");cam.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(-4,2.8f,-10);var follow=camera.AddComponent<NeonCamera>();follow.large=gm.players[0].transform;follow.small=gm.players[1].transform;camera.AddComponent<PixelViewport>();
 var light=new GameObject("05 / Facility light",typeof(Light));light.GetComponent<Light>().type=LightType.Directional;light.GetComponent<Light>().intensity=.2f;
 Solid("Arrival and crossed route floor",new Vector2(1,-.5f),new Vector2(22,1));Solid("Relay floor",new Vector2(24,-.5f),new Vector2(24,1));Solid("Core west floor",new Vector2(39,-.5f),new Vector2(6,1));Solid("Core east and reward floor",new Vector2(55,-.5f),new Vector2(20,1));
 Solid("Left boundary",new Vector2(-10,3),new Vector2(1,8));Solid("Right boundary",new Vector2(65,3),new Vector2(1,8));
 Solid("Pipo upper route",new Vector2(3,3),new Vector2(18,.4f),"Hull",true);Solid("Pipo approach step",new Vector2(-6.8f,1.4f),new Vector2(1.7f,.3f),"Platform",true);
 Solid("Route ceiling",new Vector2(3,6.4f),new Vector2(18,.4f));
 level.crossed=new NeonDevice[6];for(int i=0;i<3;i++){float x=-3+i*5;for(int p=0;p<2;p++){float y=p==0?.75f:3.8f;var d=Device("Cruce "+(i+1)+" / "+(p==0?"Mimo abre arriba":"Pipo abre abajo"),x,y,false,p);var target=Device("Compuerta cruzada "+i+" destino "+(1-p),x+2,p==0?4.7f:1.5f,true);d.target=target;Wire(d,target.transform.position);level.crossed[i*2+p]=d;}}
 level.entryGate=Device("Puerta de energia / seis circuitos",12.5f,1.5f,true);Label("01 / MI ACCION ABRE TU CAMINO",1,7.2f);Label("PIPO / RUTA SUPERIOR",-3,5.6f);Label("MIMO / RUTA INFERIOR",-3,2.5f);
 // Battery rail and a physically Pipo-sized upper vent, reached only after socket A powers its bridge.
 level.socketA=Marker("Zocalo A",18,.16f,"Socket",new Vector2(1.7f,.32f));level.socketB=Marker("Zocalo B",22,.16f,"Socket",new Vector2(1.7f,.32f));
 var battery=Solid("Bateria grande / fuerza original de Mimo",new Vector2(16,1.1f),new Vector2(1.4f,2.2f),"Battery");var rb=battery.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;level.battery=battery.AddComponent<BubblegumHeavy>();level.battery.minX=16;level.battery.maxX=18;level.battery.pusher=gm.players[0].GetComponent<PlayerController2D>();
 // Heavy's rolling visual is a child, so its collision box remains aligned.
 var br=battery.GetComponent<SpriteRenderer>();var visual=new GameObject("Battery pixel shell");visual.transform.SetParent(battery.transform,false);var vr=visual.AddComponent<SpriteRenderer>();vr.sprite=br.sprite;vr.sharedMaterial=mat;vr.drawMode=SpriteDrawMode.Sliced;vr.size=br.size;br.enabled=false;
 var lift=Solid("Elevador del relevo",new Vector2(20,.45f),new Vector2(2,.3f),"Lift",true);var liftBody=lift.AddComponent<Rigidbody2D>();liftBody.bodyType=RigidbodyType2D.Kinematic;liftBody.interpolation=RigidbodyInterpolation2D.Interpolate;var motion=lift.AddComponent<NeonLift>();motion.level=level;motion.travel=2.4f;
 level.relayBridge=Solid("Puente A / acceso al terminal",new Vector2(21.1f,3),new Vector2(1.5f,.3f),"BridgeOn",true);
 Solid("Ventilacion / suelo",new Vector2(24.5f,3),new Vector2(5.4f,.3f));Solid("Ventilacion / techo solo Pipo",new Vector2(24.5f,4.85f),new Vector2(5.4f,1.2f)); // clearance 1.1: Pipo .816, Mimo 1.377
 level.terminal=Marker("Terminal de desvio Pipo",24.8f,3.65f);level.relayGate=Device("Compuerta del relevo",34.8f,1.5f,true);
 Label("02 / RELEVO DE ENERGIA",24,7);Label("A > ELEVADOR > PIPO > B",21,6);Label("A",18,2.9f);Label("B",22,2.9f);
 var wire=Device("Circuito A / indicador",18,.5f);Wire(wire,new Vector2(24.8f,3.65f)); // visual indicator updated by state presentation
 level.coreA=Marker("Canal Mimo",39,.8f);level.coreB=Marker("Inversion Pipo",47,.8f);level.coreC=Marker("Confirmacion Mimo",49,.8f);level.padA=Marker("Placa Mimo",51,.12f,"Pad",new Vector2(1.5f,.24f));level.padB=Marker("Placa Pipo",53,.12f,"Pad",new Vector2(1.5f,.24f));
 level.coreBridge=Solid("Puente temporal del nucleo",new Vector2(43.5f,-.15f),new Vector2(3,.3f),"BridgeOn");
 level.reactor=Visual("Reactor / cuatro fases",new Vector2(49,4),new Vector2(4,4),"Core0",-1).GetComponent<SpriteRenderer>();level.reactorFrames=Enumerable.Range(0,4).Select(i=>Art("Core"+i)).ToArray();level.finalGate=Device("Puerta de recompensa",56,1.5f,true);
 Label("03 / NUCLEO SINCRONIZADO",46,7.3f);Label("MIMO INICIA",38.5f,2.3f);Label("PIPO INVIERTE",46,2.3f);Label("MIMO CONFIRMA",50,2.3f);
 Solid("Pedestal del fragmento",new Vector2(59,.25f),new Vector2(1.6f,.5f),"Pedestal");level.fragment=Marker("Fragmento del Corazon de la Amistad",59,1.5f,"Heart");level.fragment.gameObject.AddComponent<BubblegumFloat>();level.exit=Marker("Portal cooperativo / volver al mapa",62,1.3f,"Portal",new Vector2(2,2.6f));Label("04 / JUNTOS BRILLAMOS",60,5.4f);
 var fx=root.AddComponent<NeonFeedback>();fx.level=level;fx.relayCable=wire;fx.terminal=level.terminal.GetComponent<SpriteRenderer>();fx.socketA=level.socketA.GetComponent<SpriteRenderer>();fx.socketB=level.socketB.GetComponent<SpriteRenderer>();fx.coreA=level.coreA.GetComponent<SpriteRenderer>();fx.coreB=level.coreB.GetComponent<SpriteRenderer>();fx.coreC=level.coreC.GetComponent<SpriteRenderer>();fx.padA=level.padA.GetComponent<SpriteRenderer>();fx.padB=level.padB.GetComponent<SpriteRenderer>();fx.portal=level.exit.GetComponent<SpriteRenderer>();fx.panelOn=Art("PanelOn");fx.panelOff=Art("PanelOff");
 Visual("Puente temporal inactivo",new Vector2(43.5f,-.2f),new Vector2(3,.3f),"BridgeOff",-1);Visual("Puente de relevo inactivo",new Vector2(21.1f,2.95f),new Vector2(1.5f,.3f),"BridgeOff",-1);var distant=Visual("Nucleo distante / silueta",new Vector2(5,4.8f),new Vector2(4,4),"Core0",-22);distant.GetComponent<SpriteRenderer>().color=C("45476B");
 foreach(float x in new[]{14f,36f,54f})Marker("Estacion de control",x,.7f,"Checkpoint");
 // Background modules share a strict pixel grid. Sparse foreground beams keep the routes readable.
 var layers=new Transform[2];for(int l=0;l<2;l++){layers[l]=new GameObject("Parallax "+l).transform;for(int x=-16;x<80;x+=l==0?8:5){var g=Visual("Fondo / banco tecnologico",new Vector2(x,3+l),new Vector2(l==0?4:2,8),"Background",-30+l,layers[l]);g.GetComponent<SpriteRenderer>().color=l==0?C("646884"):C("78739B");}}follow.parallax=layers;
 for(int x=-8;x<65;x+=4){Visual("Columna remachada",new Vector2(x,5.8f),new Vector2(.5f,2),"Wall",-5);Visual("Lampara de techo",new Vector2(x,6.7f),new Vector2(1,.25f),"BridgeOn",-4);}
 // Reusable, imported pixel-art device prefabs; scene instances keep explicit circuit wiring.
 PrefabUtility.SaveAsPrefabAsset(level.crossed[0].gameObject,Root+"Prefabs/CircuitPanel.prefab");PrefabUtility.SaveAsPrefabAsset(level.battery.gameObject,Root+"Prefabs/HeavyBattery.prefab");PrefabUtility.SaveAsPrefabAsset(level.coreBridge,Root+"Prefabs/EnergyBridge.prefab");
 EditorSceneManager.SaveScene(scene,NeonLevel.ScenePath);
 var builds=EditorBuildSettings.scenes.ToList();if(!builds.Any(s=>s.path==NeonLevel.ScenePath))builds.Add(new EditorBuildSettingsScene(NeonLevel.ScenePath,true));EditorBuildSettings.scenes=builds.ToArray();AssetDatabase.SaveAssets();
 // Keep the user's previous scene loaded and untouched; new scene becomes the visible working scene.
 
 EditorSceneManager.CloseScene(previous,true);
 Debug.Log("ReinoNeon built and saved: "+NeonLevel.ScenePath);
 }
 static void MakeArt(){
 string[] names={"Hull","Wall","Platform","BridgeOn","BridgeOff","Battery","Socket","PanelOff","PanelOn","Pad","DoorOff","DoorOn","Lift","Checkpoint","Pedestal","Heart","Portal","Spark","Background","Core0","Core1","Core2","Core3"};
 foreach(var name in names){int size=name.StartsWith("Core")?128:name=="Background"?64:32;int width=name=="Battery"?45:name.StartsWith("Door")?16:name=="Portal"?64:name.StartsWith("Panel")?24:size;int height=name=="Battery"?70:name.StartsWith("Door")?96:name=="Portal"?84:name.StartsWith("Panel")?24:size;var tex=new Texture2D(width,height,TextureFormat.RGBA32,false);var pixels=new Color[width*height];
 Color ink=C("101429"),navy=C("242B49"),steel=C("414767"),edge=C("7D80A0"),cyan=C("79E4DB"),pink=C("DF80B9"),cream=C("F7E4CE");
 for(int y=0;y<height;y++)for(int x=0;x<width;x++){
 Color c=Color.clear;int u=Mathf.FloorToInt(x*32f/width),v=Mathf.FloorToInt(y*32f/height);
 if(name=="Hull"||name=="Wall"||name=="Platform"){c=navy;if(u<2||v<2)c=ink;else if(v>28)c=steel;else if(u==3||v==3)c=steel;if((u==5||u==26)&&(v==5||v==26))c=edge;if(v==25&&u>7&&u<24)c=cyan;if(v>7&&v<18&&u>8&&u<24)c=(u%5==0)?steel:ink;}
 else if(name.StartsWith("Bridge")){if(v<8||v>23)c=Color.clear;else{c=(v==8||v==23)?ink:(v==9||v==22)?steel:navy;if(u%8<5&&v>13&&v<18)c=name=="BridgeOn"?cyan:steel;}}
 else if(name.StartsWith("Door")){c=(u<3||u>28)?ink:steel;if(u>7&&u<24)c=navy;if(v%8==0)c=ink;if((u==11||u==20)&&v%8<5)c=name=="DoorOn"?cyan:pink;}
 else if(name=="Battery"){if(u>3&&u<28&&v>1&&v<29){c=ink;if(u>5&&u<26&&v>3&&v<27)c=steel;if(u>8&&u<23&&v>6&&v<24)c=navy;if(v>9&&v<21&&u>12&&u<20)c=cyan;if(v==20&&u>7&&u<25)c=cream;}if(v>=29&&u>11&&u<21)c=edge;}
 else if(name=="PanelOff"||name=="PanelOn"||name=="Checkpoint"){if(u>3&&u<28&&v>1&&v<30){c=ink;if(u>5&&u<26&&v>3&&v<28)c=steel;if(u>8&&u<23&&v>12&&v<25)c=navy;if(u>10&&u<21&&v>15&&v<22)c=name=="PanelOff"?pink:cyan;if(v==7&&u%5==0)c=cream;}}
 else if(name=="Socket"||name=="Pad"||name=="Lift"||name=="Pedestal"){if(v<10){c=v<2?ink:v>7?edge:steel;if(v>3&&v<7&&u%8<4)c=cyan;}if(name=="Pedestal"&&u>9&&u<23&&v<24)c=navy;if(name=="Socket"&&u>6&&u<25&&v<18)c=v>14?pink:ink;}
 else if(name=="Heart"){bool h=(v>9&&v<23&&Mathf.Abs(u-15.5f)<(v<16?v-6:11))||((u-10)*(u-10)+(v-23)*(v-23)<30)||((u-21)*(u-21)+(v-23)*(v-23)<30);if(h)c=u<13?cream:u<22?pink:steel;}
 else if(name=="Portal"){float d=Vector2.Distance(new Vector2(u,v),new Vector2(15.5f,15.5f));if(d>11&&d<15)c=d>14?ink:d<12?pink:cyan;if(d<11&&u%7==0&&v%5==0)c=steel;}
 else if(name=="Spark"){if(Mathf.Abs(u-16)<3||Mathf.Abs(v-16)<3)c=cyan;if(Mathf.Abs(u-16)+Mathf.Abs(v-16)>12)c=Color.clear;}
 else if(name=="Background"){c=ink;if(x%16<2)c=steel;if(y%24==0)c=navy;if(x%16>5&&x%16<11&&y%24>5&&y%24<14)c=steel;}
 else if(name.StartsWith("Core")){int phase=name[name.Length-1]-'0';float d=Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f));if(d<60&&d>47)c=(d>57||d<50)?ink:steel;if(d<47&&d>38)c=phase>0?cyan:navy;if(d<38&&d>30)c=ink;if(d<29)c=phase>2?cream:phase>1?pink:navy;if((Mathf.Abs(x-64)<5||Mathf.Abs(y-64)<5)&&d>29&&d<60)c=phase>1?pink:steel;if(d<24&&d>20)c=phase>0?cyan:steel;}
 pixels[y*width+x]=c;
 }
 tex.SetPixels(pixels);tex.Apply();string path=Root+"Art/"+name+".png";File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=32;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.wrapMode=TextureWrapMode.Repeat;var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);imp.SaveAndReimport();
 }
 }
}
#endif
