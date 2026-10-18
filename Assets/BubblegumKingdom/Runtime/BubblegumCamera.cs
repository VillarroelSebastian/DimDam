using UnityEngine;
namespace CoopBeam {
public class BubblegumCamera : MonoBehaviour {
 public Transform large,small;
 Camera cam;
 void Awake(){cam=GetComponent<Camera>();}
 void LateUpdate(){if(!large||!small)return;var a=large.position;var b=small.position;var mid=(a+b)*.5f;
 float size=Mathf.Max(6,Mathf.Abs(a.y-b.y)*.5f+3,(Mathf.Abs(a.x-b.x)*.5f+3)/cam.aspect);
 cam.orthographicSize=Mathf.Lerp(cam.orthographicSize,size,Time.deltaTime*5);
 var target=new Vector3(mid.x,Mathf.Max(3,mid.y+1.5f),-10);transform.position=Vector3.Lerp(transform.position,target,Time.deltaTime*5);
 }
}}
