using UnityEngine;
namespace CoopBeam {
public class PlayerHealth:MonoBehaviour {
 public int maxHearts=3; public int Hearts{get;private set;}=3;public bool Dead=>Hearts<=0;public bool Invulnerable=>Time.time<immuneUntil;public bool Stunned=>Time.time<stunUntil;
 public Vector2 spawn;float immuneUntil,stunUntil;public float respawnDelay=3;
 public void Heal(){if(!Dead)Hearts=Mathf.Min(maxHearts,Hearts+1);}
 public void Protect(float seconds){immuneUntil=Mathf.Max(immuneUntil,Time.time+seconds);}
 public void Hit(Vector2 direction){if(Dead||Invulnerable||!GameManager.Instance||!GameManager.Instance.Running)return;var powers=GameManager.Instance.GetComponent<LinkPowers>();if(powers&&powers.Absorb(this)){Protect(.75f);return;}Hearts--;immuneUntil=Time.time+1.4f;stunUntil=Time.time+.18f;
 GetComponent<Rigidbody2D>().linearVelocity=new Vector2(Mathf.Sign(direction.x)*4,5);BeamFX.Burst(transform.position,new Color(1,.5f,.5f),12);GameManager.Instance.Tone(Hearts==0?140:280,.15f);
 if(Dead){GetComponent<Collider2D>().enabled=false;var down=GetComponent<DownedBody>();if(!down)down=gameObject.AddComponent<DownedBody>();down.Begin();GameManager.Instance.CheckDefeat();}}
 public void ReviveAt(Vector3 position){Revive();transform.position=position;}
 public void Revive(){var down=GetComponent<DownedBody>();if(down)down.Release();Hearts=maxHearts;transform.position=spawn;immuneUntil=Time.time+2;GetComponent<Collider2D>().enabled=true;GetComponent<PlayerController2D>().ResetMotion();}
}}
