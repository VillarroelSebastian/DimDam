using UnityEngine;
namespace CoopBeam {
public class BubblegumWorldLabel:MonoBehaviour {
 public string text;
 void OnGUI(){var camera=Camera.main;if(!camera)return;var v=camera.WorldToViewportPoint(transform.position);float y=(1-v.y)*360;
 if(v.z<0||y<62||y>290||v.x<0||v.x>1)return;
 GUI.depth=1;PixelFont.Begin();float width=text.Length*6+12;PixelFont.Panel(new Rect(v.x*640-width/2,y-5,width,18),PixelFont.Ink,new Color(.5f,.35f,.55f));PixelFont.Text(v.x*640,y,text,1,PixelFont.Cream,true);
 }
}
}
