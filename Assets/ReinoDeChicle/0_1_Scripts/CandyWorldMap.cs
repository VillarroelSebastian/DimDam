using UnityEngine;
using UnityEngine.InputSystem;
namespace CoopBeam {
public class CandyWorldMap:MonoBehaviour {
 public Texture2D backdrop,logo;
 readonly string[][] names={new[]{"PRADERAS DE CARAMELO","BOSQUE DE GOMITAS","FABRICA DE DULCES","FORTALEZA DE NUBE Y CACAO","TRONO DEL REY BURBUJA"},new[]{"VALLE DE LOS COPOS","PUERTO A LA DERIVA","GRUTAS DE AURORA","BASTION DEL VENTISQUERO","CORONA DEL REY CENCELLON"},new[]{"HUERTO DE BRASAS","PUERTO DE FUNDICION","GRUTAS DEL MAGMA CANTOR","BASTION DE LA CENIZA","CORAZON DE BRONTOFORNO"}};
 void Update(){var k=Keyboard.current;if(k==null)return;if(k.tabKey.wasPressedThisFrame){int next=(CandyCampaign.SelectedRealm+1)%3;for(int i=0;i<3&&!CandyCampaign.CanEnter(next);i++)next=(next+1)%3;CandyCampaign.SelectRealm(next);}if(k.enterKey.wasPressedThisFrame)CandyCampaign.Load(CandyCampaign.UnlockedFor(CandyCampaign.SelectedRealm));for(int i=0;i<5;i++)if(k[(Key)((int)Key.Digit1+i)].wasPressedThisFrame)CandyCampaign.Load(i+1);}
 void OnGUI(){GUI.depth=0;PixelFont.Begin();int realm=CandyCampaign.SelectedRealm;Color accent=realm==2?new Color(1,.65f,.18f):realm==1?Color.cyan:PixelFont.Pink;PixelFont.Fill(new Rect(0,0,640,360),new Color(.055f,.065f,.15f));for(int i=0;i<45;i++){float x=Mathf.Repeat(i*71+Time.unscaledTime*(i%3+1)*3,640),y=Mathf.Repeat(i*43+Time.unscaledTime*(realm==2?9:-7),360);PixelFont.Fill(new Rect(x,y,2,2),new Color(accent.r,accent.g,accent.b,.3f));}
  PixelFont.Panel(new Rect(25,12,590,336),new Color(.09f,.08f,.19f,.95f),accent);PixelFont.Text(320,30,"DOS GOTAS / UN SOLO LAZO",2,PixelFont.Cream,true);
  string[] tabs={"1 / CHICLE","2 / HIELO","3 / FUEGO"};for(int r=0;r<3;r++)if(PixelFont.Button(new Rect(45+r*185,58,178,28),tabs[r]+(CandyCampaign.CanEnter(r)?"":" / CERRADO"),CandyCampaign.CanEnter(r)))CandyCampaign.SelectRealm(r);
  PixelFont.Text(320,101,realm==2?"MUNDO 3 / FORJEN SU CAMINO JUNTOS":realm==1?"MUNDO 2 / EL DESHIELO EMPIEZA CON USTEDES":"MUNDO 1 / APRENDAN A COMBATIR EN EQUIPO",1,accent,true);
  int unlocked=CandyCampaign.UnlockedFor(realm);for(int i=0;i<5;i++)if(PixelFont.Button(new Rect(80,124+i*32,480,27),(i+1)+" / "+(i<unlocked?names[realm][i]:"BLOQUEADO"),i<unlocked&&CandyCampaign.CanEnter(realm)))CandyCampaign.Load(i+1);
  bool cleared=realm==2?CandyCampaign.FireCleared:realm==1?CandyCampaign.IceCleared:CandyCampaign.WorldCleared;PixelFont.Text(320,294,cleared?"REINO LIBERADO / GRAN TRABAJO, EQUIPO":"CADA JEFE ABRE EL CAMINO AL SIGUIENTE REINO",1,PixelFont.Cream,true);PixelFont.Text(320,315,"ENTER CONTINUAR / 1-5 ELEGIR / TAB CAMBIAR REINO",1,accent,true);
 }
}}
