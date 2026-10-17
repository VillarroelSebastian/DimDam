using UnityEngine;
namespace CoopBeam {
[RequireComponent(typeof(Camera))]
public class ArenaCamera:MonoBehaviour {
 void LateUpdate(){var c=GetComponent<Camera>();c.orthographicSize=Mathf.Max(6.4f,12.6f/c.aspect);}
}}
