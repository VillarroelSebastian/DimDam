using UnityEngine;
namespace CoopBeam {
public class CandyHazard:MonoBehaviour {
 public float period=4,warningDuration=1,activeDuration=.7f,phaseOffset;public SpriteRenderer warning,crusher;public Vector2 area=new Vector2(2,2);public bool Active{get;private set;}
 float lastBurst;readonly Collider2D[] contacts=new Collider2D[8];
 void FixedUpdate(){if(!GameManager.Instance||!GameManager.Instance.Running){Active=false;return;}float phase=(Time.time+phaseOffset)%period;Active=phase>=warningDuration&&phase<warningDuration+activeDuration;
 var filter=new ContactFilter2D();filter.SetLayerMask(1<<11);filter.useTriggers=false;
 if(Active){int count=Physics2D.OverlapBox(transform.position,area,0,filter,contacts);for(int i=0;i<count;i++){var h=contacts[i].GetComponent<PlayerHealth>();if(h)h.Hit((Vector2)contacts[i].transform.position-(Vector2)transform.position);}}
 if(warning)warning.color=Active?new Color(1,.2f,.3f,.55f):phase<warningDuration?new Color(1,.8f,.2f,.2f+Mathf.PingPong(Time.time*2,.3f)):new Color(.9f,.6f,.8f,.08f);
 if(crusher)crusher.transform.localPosition=new Vector3(0,Active?0:2,0);
 }
}
}

