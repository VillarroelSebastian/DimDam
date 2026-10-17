using UnityEngine;
using UnityEngine.InputSystem;
namespace CoopBeam {
public class CandyWorldMap:MonoBehaviour {
 public Texture2D backdrop,logo;string[] names={"Praderas de Caramelo","Bosque de Gomitas","Fábrica de Dulces","Fortaleza de Nube y Cacao","Trono del Rey Burbuja"};
 void Update(){var k=Keyboard.current;if(k==null)return;if(k.enterKey.wasPressedThisFrame)CandyCampaign.Load(CandyCampaign.Unlocked);for(int i=0;i<5;i++)if(k[(Key)((int)Key.Digit1+i)].wasPressedThisFrame)CandyCampaign.Load(i+1);}
 void OnGUI(){GUI.depth=0;PixelFont.Begin();PixelFont.Panel(new Rect(55,12,530,336),new Color(.13f,.07f,.23f,.97f),PixelFont.Pink);
 if(logo)GUI.DrawTexture(new Rect(170,18,300,88),logo,ScaleMode.ScaleToFit);else PixelFont.Text(320,42,"REINO DE CHICLE",3,PixelFont.Mint,true);PixelFont.Text(320,114,CandyCampaign.WorldCleared?"REINO COMPLETADO / MORA Y LIMA":"MUNDO 1 / DOS CRIATURAS, UN SOLO LAZO",1,PixelFont.Cream,true);
 for(int i=0;i<5;i++){bool unlocked=i<CandyCampaign.Unlocked;if(PixelFont.Button(new Rect(95,135+i*31,450,26),(i+1)+" / "+(unlocked?names[i]:"BLOQUEADO"),unlocked))CandyCampaign.Load(i+1);}
 PixelFont.Text(320,302,"COMPLETA CADA NIVEL PARA ABRIR EL SIGUIENTE",1,PixelFont.Cream,true);PixelFont.Text(320,322,"ENTER: CONTINUAR / 1-5: ELEGIR DESBLOQUEADO",1,PixelFont.Mint,true);
 }
}}
