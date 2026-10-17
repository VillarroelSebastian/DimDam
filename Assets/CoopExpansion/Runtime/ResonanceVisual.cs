using UnityEngine;
namespace CoopBeam {
// Procedural pixel lightning follows the actual damaging segment. Visuals never apply damage.
public class ResonanceVisual:MonoBehaviour {
 CoopBeam beam;LineRenderer[] arcs;SpriteRenderer[,] shields;float tick;
 void Start(){beam=GetComponent<CoopBeam>();arcs=new LineRenderer[2];for(int i=0;i<2;i++){var g=new GameObject("Resonance lightning "+i);g.transform.SetParent(transform,false);var r=g.AddComponent<LineRenderer>();r.sharedMaterial=beam.core.sharedMaterial;r.positionCount=19;r.widthMultiplier=.035f;r.sortingOrder=9;r.enabled=false;arcs[i]=r;}shields=new SpriteRenderer[2,6];for(int p=0;p<2;p++)for(int i=0;i<6;i++){var g=new GameObject("Protective crystal");g.transform.SetParent(transform,false);g.transform.localScale=new Vector3(.07f,.12f,1);var r=g.AddComponent<SpriteRenderer>();r.sprite=ExpansionSprites.Square;r.color=p==0?new Color(.8f,.6f,1):new Color(.5f,1,.65f);r.sortingOrder=10;shields[p,i]=r;}}
 void LateUpdate(){var game=GameManager.Instance;if(!game||!beam||arcs==null)return;var powers=game.GetComponent<LinkPowers>();bool alive=game.Running&&!beam.red.Dead&&!beam.blue.Dead;Vector2 a=beam.red.transform.position,b=beam.blue.transform.position;bool active=alive&&powers&&powers.Empowered&&(b-a).sqrMagnitude>.0025f;
  for(int arc=0;arc<2;arc++){arcs[arc].enabled=active;if(!active)continue;var color=arc==0?new Color(.65f,.4f,1,.9f):new Color(.4f,1,1,.9f);arcs[arc].startColor=arcs[arc].endColor=color;Vector2 normal=Vector2.Perpendicular((b-a).normalized);for(int i=0;i<19;i++){float t=i/18f;float wobble=Mathf.Sin(t*36+Mathf.Floor(Time.time*20)*1.7f+arc*Mathf.PI)*Mathf.Sin(t*Mathf.PI)*(powers.Ultimate?.22f:.12f);Vector2 p=Vector2.Lerp(a,b,t)+normal*wobble;arcs[arc].SetPosition(i,new Vector3(Mathf.Round(p.x*32)/32,Mathf.Round(p.y*32)/32,-.25f));}}
  for(int p=0;p<2;p++)for(int i=0;i<6;i++){var r=shields[p,i];r.enabled=alive&&powers&&powers.HasShield(p);if(!r.enabled)continue;float angle=Time.time*1.8f+i*Mathf.PI/3;Vector2 center=game.players[p].transform.position;r.transform.position=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.78f;r.transform.rotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg);}
 }
}
}
