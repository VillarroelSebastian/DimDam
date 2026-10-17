using UnityEngine;
namespace CoopBeam {
public class UIManager:MonoBehaviour {
 public GameManager game;GUIStyle label,title,small,center;Texture2D panel;
 void Init(){label=new GUIStyle(GUI.skin.label){fontSize=23,fontStyle=FontStyle.Bold};label.normal.textColor=new Color(.9f,.96f,1);title=new GUIStyle(label){fontSize=48,alignment=TextAnchor.MiddleCenter};center=new GUIStyle(label){alignment=TextAnchor.MiddleCenter};small=new GUIStyle(label){fontSize=16,alignment=TextAnchor.MiddleCenter};panel=Texture2D.whiteTexture;}
 void Box(Rect rect,Color color){if(!panel)panel=Texture2D.whiteTexture;if(!panel)return;GUI.color=color;GUI.DrawTexture(rect,panel);GUI.color=Color.white;}
 void OnGUI(){if(label==null)Init();float sx=Screen.width/1280f,sy=Screen.height/720f;GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(sx,sy,1));
 Box(new Rect(0,0,1280,78),new Color(.025f,.045f,.12f,.95f));GUI.color=new Color(1,.48f,.45f);GUI.Label(new Rect(28,14,280,32),"CORAL  "+Hearts(game.players[0]),label);GUI.color=new Color(.35f,.85f,1);GUI.Label(new Rect(990,14,280,32),"CELESTE  "+Hearts(game.players[1]),label);GUI.color=Color.white;
 GUI.Label(new Rect(330,9,620,32),(game.level?"REINO DE CHICLE · "+game.level.number:"LAZO DE LUZ")+"   /   OLEADA "+game.Wave+" DE 3",center);GUI.Label(new Rect(330,43,620,28),"PUNTOS "+game.Score.ToString("000000")+"     •     ENEMIGOS "+game.enemies.Count,small);
 Box(new Rect(0,678,1280,42),new Color(.025f,.045f,.12f,.9f));GUI.Label(new Rect(10,685,1260,30),"CORAL: A/D · W salto · S bajar · Shift izq dash       |       CELESTE: ←/→ · ↑ salto · ↓ bajar · Ctrl der dash",small);
 if(game.Combo>=2){GUI.color=new Color(1,.85f,.3f);GUI.Label(new Rect(500,92,280,55),"COMBO ×"+game.Combo,title);GUI.color=Color.white;}
 if(game.Running&&game.Announcement!="")GUI.Label(new Rect(240,235,800,80),game.Announcement,title);
 if(game.level&&game.level.boss&&game.Running)foreach(var enemy in game.enemies){var boss=enemy?enemy.GetComponent<CandyBoss>():null;if(!boss)continue;Box(new Rect(255,90,770,68),new Color(.05f,.04f,.15f,.94f));Box(new Rect(380,100,520,14),new Color(.15f,.08f,.25f));Box(new Rect(380,100,520*Mathf.Clamp01(enemy.Health/enemy.maxHealth),14),new Color(1,.4f,.65f));GUI.Label(new Rect(270,117,740,30),"REY BURBUJA · FASE "+boss.Phase+" · "+boss.Hint,small);}
 if(!game.Started||!game.Running){Box(new Rect(210,155,860,430),new Color(.035f,.055f,.15f,.97f));GUI.color=new Color(.6f,1,.92f);var heading=new GUIStyle(title){fontSize=game.level&&!game.Started?34:48};GUI.Label(new Rect(230,180,820,80),!game.Started?(game.level?game.level.title:"LAZO DE LUZ"):game.Victory?"¡VICTORIA!":"FIN DE LA PARTIDA",heading);GUI.color=Color.white;
 string text=!game.Started?"Dos criaturas. Una conexión.\n\nRodeen a los enemigos: solo el rayo hace daño.\nSalten para cambiar su ángulo y atraviesen varios a la vez.\n\n3 corazones • Reaparición en 3 segundos\nSi ambos caen a la vez, termina la partida.":game.Victory?"¡La arboleda vuelve a brillar!\n\nPuntuación compartida: "+game.Score:"La conexión se ha apagado.\n\nPuntuación compartida: "+game.Score;
 if(game.level&&!game.Started)text=game.level.lesson+"\n\nSolo el lazo entre ambos daña a los enemigos.\nSalto: W / ↑ · Dash: Shift izq / Ctrl der\n3 corazones · Reaparición en 3 segundos.";
 if(game.level&&game.Victory&&game.level.number==5)text="¡REINO DE CHICLE COMPLETADO!\n\nEl Rey Burbuja ha liberado el camino.\nPuntuación: "+game.Score;
 GUI.Label(new Rect(245,278,790,210),text,center);if(GUI.Button(new Rect(game.level?260:460,510,game.level?240:360,48),!game.Started?"ENTRAR / COMENZAR":"R / VOLVER A JUGAR")){if(!game.Started)game.Begin();else game.Restart();}
 if(game.level){if(GUI.Button(new Rect(520,510,220,48),"M / MAPA"))CandyCampaign.Map();if(game.Victory&&game.level.number<5&&GUI.Button(new Rect(760,510,260,48),"N / SIGUIENTE"))CandyCampaign.Load(game.level.number+1);}}
 }
 string Hearts(PlayerHealth h){return h.Dead?"REAPARECIENDO":new string('♥',h.Hearts)+new string('♡',h.maxHearts-h.Hearts);}
}}
