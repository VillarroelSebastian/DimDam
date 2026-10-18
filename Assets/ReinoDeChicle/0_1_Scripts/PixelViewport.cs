using UnityEngine;
namespace CoopBeam {
[RequireComponent(typeof(Camera))]
public class PixelViewport:MonoBehaviour {
 public int width=800,height=450;public bool illustrated=true;public Shader paletteShader;RenderTexture target,resolved;Camera view;Material palette;
 void OnEnable(){view=GetComponent<Camera>();if(illustrated){view.targetTexture=null;FitCamera();return;}int w=width,h=height;target=new RenderTexture(w,h,24){name="Melt Together pixel viewport",filterMode=FilterMode.Point,antiAliasing=1,useMipMap=false};target.Create();resolved=new RenderTexture(w,h,0){name="Melt Together presentation",filterMode=FilterMode.Point};resolved.Create();if(paletteShader)palette=new Material(paletteShader);view.targetTexture=target;view.allowMSAA=true;}
 void FitCamera(){var rect=PixelFont.Viewport;view.rect=new Rect(rect.x/Screen.width,rect.y/Screen.height,rect.width/Screen.width,rect.height/Screen.height);}
 void LateUpdate(){if(illustrated&&view)FitCamera();}
 void OnGUI(){if(!target)return;GUI.depth=100;GUI.matrix=Matrix4x4.identity;if(Event.current.type==EventType.Repaint){var old=RenderTexture.active;if(palette)Graphics.Blit(target,resolved,palette);else Graphics.Blit(target,resolved);RenderTexture.active=old;}GUI.color=Color.black;GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;GUI.DrawTexture(PixelFont.Viewport,resolved,ScaleMode.StretchToFill,false);}
 void OnDisable(){if(view){view.targetTexture=null;view.rect=new Rect(0,0,1,1);}if(target){target.Release();Destroy(target);target=null;}if(resolved){resolved.Release();Destroy(resolved);resolved=null;}if(palette)Destroy(palette);}
}}
