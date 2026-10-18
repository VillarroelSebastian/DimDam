using UnityEngine;
namespace CoopBeam {
public class FireAtmosphere:MonoBehaviour {
 public int level;Transform[] embers;SpriteRenderer[] art;float clock;
 void Start(){embers=new Transform[32];art=new SpriteRenderer[32];for(int i=0;i<embers.Length;i++){var g=new GameObject("Rising ember");g.transform.SetParent(transform,false);g.transform.localScale=Vector3.one*(i%3==0?.1f:.055f);art[i]=g.AddComponent<SpriteRenderer>();art[i].sprite=FireSprites.Frame(1,i%4);art[i].sortingOrder=-8;art[i].color=new Color(1,.5f,.15f,.4f);embers[i]=g.transform;}}
 void Update(){clock+=Time.deltaTime;for(int i=0;i<embers.Length;i++){float x=Mathf.Repeat(i*3.73f+Mathf.Sin(clock+i)*.3f+12,24)-12,y=Mathf.Repeat(i*1.13f+clock*(.25f+i%3*.12f)+5,12)-5;embers[i].position=new Vector3(x,y,0);art[i].sprite=FireSprites.Frame(1,((int)(clock*7)+i)%4);}}
}
}
