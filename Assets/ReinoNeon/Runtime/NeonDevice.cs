using UnityEngine;
namespace CoopBeam {
// Serialized circuit endpoints: the source belongs to one player and powers the other route.
public class NeonDevice : MonoBehaviour {
 public int player; public NeonDevice target; public Collider2D blocker; public SpriteRenderer lamp;
 public Sprite offSprite,onSprite; public bool powered; public Transform[] cable;
 Vector3 home; bool initialized;
 void Awake(){home=transform.position;initialized=true;}
 public void Set(bool on){if(!initialized){home=transform.position;initialized=true;}bool changed=powered!=on;powered=on;if(blocker)blocker.enabled=!on;if(lamp){lamp.sprite=on?onSprite:offSprite;lamp.color=Color.white;}if(changed&&Application.isPlaying){BeamFX.Burst(transform.position,Color.cyan,8);if(GameManager.Instance)GameManager.Instance.Tone(on?740:330,.08f);}}
 void Update(){if(blocker)transform.position=Vector3.MoveTowards(transform.position,home+Vector3.up*(powered?3.1f:0),Time.deltaTime*4);if(cable!=null)for(int i=0;i<cable.Length;i++){var r=cable[i].GetComponent<SpriteRenderer>();if(r)r.color=powered?Color.Lerp(new Color(.18f,.65f,.7f),Color.white,Mathf.Pow(Mathf.Max(0,Mathf.Sin(Time.time*5-i*.6f)),8)):new Color(.18f,.24f,.37f);}}
 }
}
