using UnityEngine;
namespace CoopBeam {
// Same shared framing rule as BubblegumCamera, extended to horizontal exploration.
public class NeonCamera:MonoBehaviour {
 public Transform large,small; public Transform[] parallax;
 Camera cam;Vector3[] origins;
 void Awake(){cam=GetComponent<Camera>();origins=new Vector3[parallax.Length];for(int i=0;i<origins.Length;i++)origins[i]=parallax[i].position;}
 void LateUpdate(){if(!large||!small)return;var a=large.position;var b=small.position;float size=Mathf.Max(5.5f,Mathf.Abs(a.y-b.y)*.5f+2.7f,(Mathf.Abs(a.x-b.x)*.5f+3)/cam.aspect);cam.orthographicSize=Mathf.Lerp(cam.orthographicSize,size,1-Mathf.Exp(-5*Time.deltaTime));var target=new Vector3(Mathf.Clamp((a.x+b.x)*.5f,-4,58),2.8f,-10);transform.position=Vector3.Lerp(transform.position,target,1-Mathf.Exp(-5*Time.deltaTime));for(int i=0;i<origins.Length;i++)parallax[i].position=origins[i]+Vector3.right*transform.position.x*(.04f+i*.035f);}
 }
}
