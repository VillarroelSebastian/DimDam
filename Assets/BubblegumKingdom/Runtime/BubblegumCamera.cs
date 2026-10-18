using UnityEngine;
namespace CoopBeam {
public class BubblegumCamera : MonoBehaviour {
 public Transform large,small;
 Camera cam;
 void Awake(){cam=GetComponent<Camera>();}
 void LateUpdate(){if(!large||!small)return;
 float low=Mathf.Min(large.position.y,small.position.y),high=Mathf.Max(large.position.y,small.position.y);
 float size=Mathf.Max(6.2f,10.2f/cam.aspect,(high-low)*.5f+3.2f);
 cam.orthographicSize=Mathf.Lerp(cam.orthographicSize,size,1-Mathf.Exp(-5*Time.deltaTime));
 var target=new Vector3(0,Mathf.Max(3,(low+high)*.5f+1.4f),-10);
 transform.position=Vector3.Lerp(transform.position,target,1-Mathf.Exp(-5*Time.deltaTime));
 }
}}
