using UnityEngine;
namespace CoopBeam {
public class CandyPixelHUD:MonoBehaviour {
 public GameManager game;public CandyLevelExit exit;
 void OnGUI(){if(!game)return;GUI.depth=0;PixelFont.Begin();PixelFont.Panel(new Rect(2,2,636,35),PixelFont.Ink,new Color(.4f,.26f,.5f));
  Life(game.players[0],12,9,"MORA",new Color(.77f,.48f,1));Life(game.players[1],493,9,"LIMA",new Color(.65f,1,.35f));
  PixelFont.Text(320,9,(game.level.fireKingdom?"REINO DE FUEGO / ":game.level.iceKingdom?"REINO DE HIELO / ":"REINO DE CHICLE / ")+game.level.number,1,PixelFont.Cream,true);PixelFont.Text(320,23,"OLEADA "+game.Wave+"/3  PUNTOS "+game.Score.ToString("000000"),1,PixelFont.Mint,true);
  PixelFont.Panel(new Rect(2,339,636,19),PixelFont.Ink,new Color(.4f,.26f,.5f));PixelFont.Text(12,345,"MORA A/D | W SALTO | S BAJAR | SHIFT DASH",1,PixelFont.Pink);PixelFont.Text(340,345,"LIMA FLECHAS | CTRL DER DASH | R REINICIA",1,new Color(.65f,1,.35f));
  if(game.Running){PixelFont.Panel(new Rect(258,42,124,17),PixelFont.Ink,PixelFont.Mint);PixelFont.Text(320,47,"ENEMIGOS "+game.enemies.Count,1,PixelFont.Cream,true);
   if(game.Combo>=2)PixelFont.Text(320,65,"COMBO X"+game.Combo,2,new Color(1,.85f,.4f),true);
   if(game.Announcement!=""){PixelFont.Panel(new Rect(170,120,300,45),PixelFont.Ink,PixelFont.Pink);PixelFont.Text(320,136,game.Announcement,2,PixelFont.Cream,true);}
   foreach(var e in game.enemies){var boss=e?e.GetComponent<CandyBoss>():null;if(!boss)continue;PixelFont.Panel(new Rect(130,83,380,38),PixelFont.Ink,PixelFont.Pink);PixelFont.Fill(new Rect(140,91,360,5),new Color(.3f,.15f,.35f));PixelFont.Fill(new Rect(140,91,360*Mathf.Clamp01(e.Health/e.maxHealth),5),PixelFont.Pink);PixelFont.Text(320,104,"REY BURBUJA / FASE "+boss.Phase+" / "+(boss.Vulnerable?"NUCLEO ABIERTO":"ESQUIVEN LAS PERLAS"),1,PixelFont.Cream,true);}
  }
  if(game.Victory||exit&&exit.Arriving){PixelFont.Panel(new Rect(145,72,350,45),PixelFont.Ink,PixelFont.Mint);PixelFont.Text(320,83,game.Victory?"VICTORIA! ARRIBA, EQUIPO!":"UN NUEVO RINCON DEL REINO",2,PixelFont.Cream,true);PixelFont.Text(320,102,game.Victory?"R REINICIAR / M MAPA":"PREPARADOS PARA ENLAZAR",1,PixelFont.Mint,true);return;}
  if(game.Started&&game.Running)return;
  PixelFont.Panel(new Rect(65,74,510,235),new Color(.13f,.07f,.23f,.98f),PixelFont.Pink);
  string title=!game.Started?game.level.title:game.Victory?(game.level.number==5?"REINO COMPLETADO!":"VICTORIA!"):"FIN DE LA PARTIDA";PixelFont.Text(320,94,title,2,PixelFont.Mint,true);
  if(!game.Started){Portrait(game.players[0],new Rect(89,117,75,75));Portrait(game.players[1],new Rect(476,117,75,75));PixelFont.Fill(new Rect(163,164,314,1),new Color(.4f,.8f,.7f,.45f));PixelFont.Text(320,128,game.level.lesson,1,PixelFont.Cream,true);PixelFont.Text(320,174,"EL LAZO Y SUS PODERES SON SU UNICA ARMA",1,PixelFont.Pink,true);PixelFont.Text(110,197,"MORA\nA/D MOVER - W SALTAR\nS BAJAR - SHIFT IZQ DASH",1,new Color(.77f,.48f,1));PixelFont.Text(350,197,"LIMA\nFLECHAS MOVER Y SALTAR\nABAJO BAJAR - CTRL DER DASH",1,new Color(.65f,1,.35f));PixelFont.Text(320,242,"3 CORAZONES / ACERQUENSE PARA RESCATAR",1,PixelFont.Cream,true);PixelFont.Text(320,254,"BOTIN: TOQUEN EL ICONO O CRUCENLO CON EL LAZO",1,PixelFont.Mint,true);}
  else {PixelFont.Text(320,146,game.Victory?"UN GRAN LAZO. UN GRAN EQUIPO.":"SI AMBOS CAEN, SE APAGA EL LAZO.",1,PixelFont.Cream,true);PixelFont.Text(320,177,"PUNTUACION "+game.Score.ToString("000000"),2,PixelFont.Pink,true);PixelFont.Text(320,220,game.Victory&&game.level.number<5?"NUEVO NIVEL DESBLOQUEADO":"VUELVAN A INTENTARLO JUNTOS",1,PixelFont.Mint,true);}
  if(PixelFont.Button(new Rect(90,270,165,25),!game.Started?"ENTER / JUGAR":"R / REINICIAR")){if(!game.Started)game.Begin();else game.Restart();}
  if(PixelFont.Button(new Rect(270,270,115,25),"M / MAPA"))CandyCampaign.Map();
  if(!game.Started&&PixelFont.Button(new Rect(400,270,150,25),"P / ENSAYAR ULTIS"))game.BeginPractice();
  if(game.Victory&&game.level.number<5&&PixelFont.Button(new Rect(400,270,150,25),"N / SIGUIENTE"))CandyCampaign.Load(game.level.number+1);
 }
 void Portrait(PlayerHealth h,Rect target){var sprite=h.GetComponent<PlayerController2D>().sprite.sprite;var texture=sprite.texture;var uv=new Rect(sprite.rect.x/texture.width,sprite.rect.y/texture.height,sprite.rect.width/texture.width,sprite.rect.height/texture.height);GUI.color=Color.white;GUI.DrawTextureWithTexCoords(target,texture,uv);}
 void Life(PlayerHealth h,float x,float y,string name,Color color){PixelFont.Text(x,y,name,1,color);for(int i=0;i<3;i++)PixelFont.Heart(x+55+i*14,y-1,i<h.Hearts,color);if(h.Dead)PixelFont.Text(x,y+15,game.Running?"NECESITA RESCATE":"SIN CORAZONES",1,color);}
}}
