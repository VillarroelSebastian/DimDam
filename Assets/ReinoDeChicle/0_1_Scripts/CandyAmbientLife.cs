using System.Collections.Generic;
using UnityEngine;
namespace CoopBeam {
public class CandyAmbientLife:MonoBehaviour {
 public int levelNumber=1;public Sprite spark,cloud,bubble;public Sprite[] gears;
 readonly List<SpriteRenderer> motes=new List<SpriteRenderer>();readonly List<Vector3> positions=new List<Vector3>();readonly List<Vector2> velocities=new List<Vector2>();readonly List<Transform> sweets=new List<Transform>();readonly List<Vector3> sweetBases=new List<Vector3>();readonly List<SpriteRenderer> gearArt=new List<SpriteRenderer>();float clock;
 void Start(){var random=new System.Random(100+levelNumber);for(int i=0;i<18;i++){var g=new GameObject(levelNumber==5?"Floating gum bubble":"Drifting sugar mote");g.transform.parent=transform;var r=g.AddComponent<SpriteRenderer>();r.sprite=levelNumber==5?bubble:spark;r.sharedMaterial=GetMaterial();r.sortingOrder=-6;r.color=i%3==0?new Color(1,.8f,.9f,.3f):new Color(.65f,1,.85f,.28f);float size=levelNumber==5?.13f+(float)random.NextDouble()*.18f:.035f+(float)random.NextDouble()*.07f;g.transform.localScale=Vector3.one*size;var p=new Vector3((float)random.NextDouble()*23-11.5f,(float)random.NextDouble()*10-4,0);g.transform.position=p;motes.Add(r);positions.Add(p);velocities.Add(new Vector2(.08f+(float)random.NextDouble()*.15f,.08f+(float)random.NextDouble()*.15f));}
  if(levelNumber<=2&&cloud)for(int i=0;i<3;i++){var g=new GameObject("Slow sugar cloud");g.transform.parent=transform;var r=g.AddComponent<SpriteRenderer>();r.sprite=cloud;r.sharedMaterial=GetMaterial();r.sortingOrder=-15;r.color=new Color(1,.9f,.95f,.16f);g.transform.position=new Vector3(i*8-8,5+i*.3f,0);g.transform.localScale=new Vector3(2.7f,1.1f,1);motes.Add(r);positions.Add(g.transform.position);velocities.Add(new Vector2(.1f,0));}
  if(levelNumber==3&&gears!=null&&gears.Length>0)for(int side=-1;side<=1;side+=2){var g=new GameObject("Turning sugar-machine gear");g.transform.parent=transform;g.transform.position=new Vector3(side*9.1f,2,0);g.transform.localScale=Vector3.one*1.7f;var r=g.AddComponent<SpriteRenderer>();r.sprite=gears[0];r.sharedMaterial=GetMaterial();r.sortingOrder=-5;r.color=new Color(.9f,.65f,.4f,.7f);gearArt.Add(r);}
  foreach(var r in FindObjectsByType<SpriteRenderer>())if(r.name=="Lollipop"){sweets.Add(r.transform);sweetBases.Add(r.transform.position);}
 }
 Material GetMaterial(){var game=GameManager.Instance;return game&&game.players.Length>0?game.players[0].GetComponent<PlayerController2D>().sprite.sharedMaterial:null;}
 void Update(){clock+=Time.deltaTime;for(int i=0;i<motes.Count;i++){if(!motes[i])continue;var p=positions[i]+(Vector3)(velocities[i]*clock);p.x=Mathf.Repeat(p.x+12,24)-12;p.y=Mathf.Repeat(p.y+4.5f,11)-4.5f;p.x+=Mathf.Sin(clock*.8f+i)*.12f;motes[i].transform.position=p;var c=motes[i].color;c.a=.13f+Mathf.Sin(clock*1.6f+i)*.07f;motes[i].color=c;}
  for(int i=0;i<sweets.Count;i++)if(sweets[i]){var p=sweetBases[i];p.x+=Mathf.Sin(clock*1.7f+i)*.06f;sweets[i].position=p;}
  foreach(var gear in gearArt)if(gear)gear.sprite=gears[(int)(clock*7)%gears.Length];
 }
}}
