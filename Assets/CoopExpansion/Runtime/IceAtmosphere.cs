using UnityEngine;
namespace CoopBeam {
public class IceAtmosphere:MonoBehaviour {
 public int level;Transform[] flakes;float clock;
 void Start(){flakes=new Transform[38];for(int i=0;i<flakes.Length;i++){var g=new GameObject("Pixel snow");g.transform.SetParent(transform);var r=g.AddComponent<SpriteRenderer>();r.sprite=IllustratedWorldLibrary.Sprite(1,27)??ExpansionSprites.Square;r.color=new Color(.75f,.9f,1,i%3==0?.65f:.25f);r.sortingOrder=-8;g.transform.localScale=Vector3.one*(i%3==0?.045f:.025f);flakes[i]=g.transform;}}
 void Update(){clock+=Time.deltaTime;for(int i=0;i<flakes.Length;i++){float x=Mathf.Repeat(i*3.73f+clock*(.2f+(i%4)*.1f),24)-12;float y=Mathf.Repeat(i*1.13f-clock*(.3f+(i%3)*.15f)+5,12)-5;flakes[i].position=new Vector3(x,y,0);}}
}
}
