using UnityEngine;
namespace CoopBeam {
[DefaultExecutionOrder(1000)]
public class IllustratedBackdrop:MonoBehaviour {
 void LateUpdate(){var camera=Camera.main;if(!camera)return;transform.position=new Vector3(camera.transform.position.x*.98f,camera.transform.position.y*.98f,0);}
}
}
