using UnityEngine;
namespace CoopBeam {
// Quantized ambience affects visuals only; the gameplay colliders remain stable.
public class BubblegumFloat : MonoBehaviour {
 Vector3 home;float phase;
 void Start(){home=transform.localPosition;phase=home.x*1.7f;}
 void Update(){float dy=Mathf.Round(Mathf.Sin(Time.time*2+phase)*3)/16;transform.localPosition=home+Vector3.up*dy;}
}
}
