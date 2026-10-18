using System.Collections.Generic;
using UnityEngine;
namespace CoopBeam {
public class CoopBeam:MonoBehaviour {
 public PlayerHealth red,blue;public float damagePerSecond=38;public LineRenderer core,halo,outline;public bool IsDamaging{get;private set;}
 readonly RaycastHit2D[] hits=new RaycastHit2D[64];readonly HashSet<EnemyBase> visited=new HashSet<EnemyBase>();float spark;
 public int DamageSegment(Vector2 a,Vector2 b,float dt){visited.Clear();if((b-a).sqrMagnitude<.0025f)return 0;
 var filter=new ContactFilter2D();filter.SetLayerMask(1<<10);filter.useTriggers=true;
 var powers=GameManager.Instance?GameManager.Instance.GetComponent<LinkPowers>():null;int casts=powers&&powers.Empowered?3:1;Vector2 normal=Vector2.Perpendicular((b-a).normalized);
 for(int cast=0;cast<casts;cast++){Vector2 offset=normal*(cast==0?0:cast==1?.16f:-.16f);int n=Physics2D.Linecast(a+offset,b+offset,filter,hits);for(int i=0;i<n;i++){var e=hits[i].collider.GetComponentInParent<EnemyBase>();if(e&&visited.Add(e)){float before=e.Health;e.Damage(damagePerSecond*dt*(powers?powers.DamageMultiplier:1));if(powers){float accepted=Mathf.Min(before,Mathf.Max(0,before-e.Health));powers.AddCharge(accepted);powers.OnBeamHit(e,accepted);}}}}
 return visited.Count;}
 void FixedUpdate(){IsDamaging=false;if(CoopInteractionDirector.Active&&CoopInteractionDirector.Active.Dancing)return;if(!red||!blue||red.Dead||blue.Dead||!GameManager.Instance||!GameManager.Instance.Running)return;IsDamaging=DamageSegment(red.transform.position,blue.transform.position,Time.fixedDeltaTime)>0;}
 void LateUpdate(){if(!core||!halo)return;bool on=red&&blue&&!red.Dead&&!blue.Dead&&GameManager.Instance&&(GameManager.Instance.Running||GameManager.Instance.Victory);core.enabled=halo.enabled=on;if(outline)outline.enabled=on;if(!on)return;
 Vector3 a=red.transform.position,b=blue.transform.position;a.z=b.z=-.2f;core.SetPosition(0,a);core.SetPosition(1,b);halo.SetPosition(0,a);halo.SetPosition(1,b);
 if(outline){outline.SetPosition(0,a);outline.SetPosition(1,b);}
 var powers=GameManager.Instance.GetComponent<LinkPowers>();bool super=powers&&powers.Empowered;Color c=super?Color.Lerp(new Color(.7f,.4f,1),new Color(.3f,1,1),Mathf.PingPong(Time.time*3,1)):IsDamaging?new Color(1,.9f,.35f):new Color(.4f,1,1);core.startColor=core.endColor=Color.Lerp(c,Color.white,.65f);core.widthMultiplier=super?.19f:.055f;halo.startColor=halo.endColor=new Color(c.r,c.g,c.b,super?.5f:.17f);halo.widthMultiplier=super?.5f:IsDamaging?.32f:.22f;
 spark-=Time.deltaTime;if(IsDamaging&&spark<=0){spark=.09f;foreach(var e in visited)if(e)BeamFX.Burst(e.transform.position,c,2);}}
}}
