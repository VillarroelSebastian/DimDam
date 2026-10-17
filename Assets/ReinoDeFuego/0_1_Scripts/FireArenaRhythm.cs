using UnityEngine;
namespace CoopBeam {
public class FireArenaRhythm:MonoBehaviour {
 public int level;float timer=5;int cycle;public int Eruptions{get;private set;}
 void Update(){if(!GameManager.Instance||!GameManager.Instance.Running||level<2||level==5)return;timer-=Time.deltaTime;if(timer>0)return;timer=level==4?7.5f:8.5f;cycle++;Eruptions++;float[] xs=level==2?new[]{-8f,8f}:new[]{-6f,0,6};for(int i=0;i<xs.Length;i++)if((i+cycle)%2==0)FireHazard.Spawn(new Vector2(xs[i],-3.35f),new Vector2(1.1f,1.8f),1.5f,1.1f,Vector2.zero,this,1,false);}
 void OnDisable(){EncounterHazard.ClearOwned(this);}
}
}
