using UnityEngine;
namespace CoopBeam {
public class IllustratedWorldLibrary:ScriptableObject {
 public Sprite[] backgrounds,candy,ice,fire,neon;
 static IllustratedWorldLibrary instance;
 public static IllustratedWorldLibrary Current {get{if(!instance)instance=Resources.Load<IllustratedWorldLibrary>("MeltWorldStyle");return instance;}}
 public Sprite Get(int world,int index){var a=world==1?ice:world==2?fire:world==3?neon:candy;return a!=null&&index>=0&&index<a.Length?a[index]:null;}
 public static Sprite Sprite(int world,int index){var a=Current;return a?a.Get(world,index):null;}
}
}
