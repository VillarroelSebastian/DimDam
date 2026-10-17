using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
namespace CoopBeam {
// Original 5 x 7 bitmap alphabet. No external font or license dependency.
public static class PixelFont {
 static readonly Dictionary<char,string> patterns=new Dictionary<char,string>{
 {'A',"01110/10001/10001/11111/10001/10001/10001"},{'B',"11110/10001/10001/11110/10001/10001/11110"},{'C',"01111/10000/10000/10000/10000/10000/01111"},{'D',"11110/10001/10001/10001/10001/10001/11110"},{'E',"11111/10000/10000/11110/10000/10000/11111"},{'F',"11111/10000/10000/11110/10000/10000/10000"},
 {'G',"01111/10000/10000/10111/10001/10001/01111"},{'H',"10001/10001/10001/11111/10001/10001/10001"},{'I',"11111/00100/00100/00100/00100/00100/11111"},{'J',"00111/00010/00010/00010/10010/10010/01100"},{'K',"10001/10010/10100/11000/10100/10010/10001"},{'L',"10000/10000/10000/10000/10000/10000/11111"},
 {'M',"10001/11011/10101/10101/10001/10001/10001"},{'N',"10001/11001/10101/10011/10001/10001/10001"},{'O',"01110/10001/10001/10001/10001/10001/01110"},{'P',"11110/10001/10001/11110/10000/10000/10000"},{'Q',"01110/10001/10001/10001/10101/10010/01101"},{'R',"11110/10001/10001/11110/10100/10010/10001"},
 {'S',"01111/10000/10000/01110/00001/00001/11110"},{'T',"11111/00100/00100/00100/00100/00100/00100"},{'U',"10001/10001/10001/10001/10001/10001/01110"},{'V',"10001/10001/10001/10001/10001/01010/00100"},{'W',"10001/10001/10001/10101/10101/11011/10001"},{'X',"10001/10001/01010/00100/01010/10001/10001"},{'Y',"10001/10001/01010/00100/00100/00100/00100"},{'Z',"11111/00001/00010/00100/01000/10000/11111"},
 {'0',"01110/10001/10011/10101/11001/10001/01110"},{'1',"00100/01100/00100/00100/00100/00100/01110"},{'2',"01110/10001/00001/00010/00100/01000/11111"},{'3',"11110/00001/00001/01110/00001/00001/11110"},{'4',"00010/00110/01010/10010/11111/00010/00010"},{'5',"11111/10000/10000/11110/00001/00001/11110"},{'6',"01110/10000/10000/11110/10001/10001/01110"},{'7',"11111/00001/00010/00100/01000/01000/01000"},{'8',"01110/10001/10001/01110/10001/10001/01110"},{'9',"01110/10001/10001/01111/00001/00001/01110"},
 {'-',"00000/00000/00000/11111/00000/00000/00000"},{'.',"00000/00000/00000/00000/00000/00110/00110"},{':',"00000/00110/00110/00000/00110/00110/00000"},{'/',"00001/00001/00010/00100/01000/10000/10000"},{'!',"00100/00100/00100/00100/00100/00000/00100"},{'?',"01110/10001/00001/00010/00100/00000/00100"},{'+',"00000/00100/00100/11111/00100/00100/00000"},{'<',"00010/00100/01000/10000/01000/00100/00010"},{'>',"01000/00100/00010/00001/00010/00100/01000"},{'=',"00000/11111/00000/11111/00000/00000/00000"},{'|',"00100/00100/00100/00100/00100/00100/00100"}
 };
 static readonly Dictionary<char,Texture2D> textures=new Dictionary<char,Texture2D>();
 public static Color Ink=>new Color(.12f,.065f,.21f);public static Color Cream=>new Color(1,.94f,.79f);public static Color Mint=>new Color(.51f,1,.79f);public static Color Pink=>new Color(1,.5f,.7f);
 static string Clean(string s){var sb=new StringBuilder();foreach(char c in s.ToUpperInvariant().Normalize(NormalizationForm.FormD))if(CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)sb.Append(c);return sb.ToString();}
 public static Rect Viewport {get{float scale=Mathf.Min(Screen.width/640f,Screen.height/360f);return new Rect((Screen.width-640*scale)*.5f,(Screen.height-360*scale)*.5f,640*scale,360*scale);}}
 public static void Begin(){var v=Viewport;GUI.matrix=Matrix4x4.TRS(new Vector3(v.x,v.y,0),Quaternion.identity,new Vector3(v.width/640,v.height/360,1));}
 public static void Fill(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
 public static void Panel(Rect r,Color fill,Color edge){Fill(new Rect(r.x+3,r.y,r.width-6,r.height),edge);Fill(new Rect(r.x,r.y+3,r.width,r.height-6),edge);Fill(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),fill);Fill(new Rect(r.x+6,r.y+6,r.width-12,1),new Color(edge.r,edge.g,edge.b,.3f));}
 static Texture2D Glyph(char c){if(textures.TryGetValue(c,out var t)&&t)return t;if(!patterns.TryGetValue(c,out var data))return null;t=new Texture2D(5,7,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,hideFlags=HideFlags.HideAndDontSave};var rows=data.Split('/');var pixels=new Color[35];for(int y=0;y<7;y++)for(int x=0;x<5;x++)pixels[(6-y)*5+x]=rows[y][x]=='1'?Color.white:Color.clear;t.SetPixels(pixels);t.Apply();textures[c]=t;return t;}
 public static void Text(float x,float y,string message,int scale,Color color,bool centered=false){var clean=Clean(message);foreach(string line in clean.Split('\n')){float cursor=centered?x-(line.Length*6-1)*scale*.5f:x;GUI.color=color;foreach(char c in line){var g=Glyph(c);if(g)GUI.DrawTexture(new Rect(cursor,y,5*scale,7*scale),g);cursor+=6*scale;}y+=10*scale;}GUI.color=Color.white;}
 public static bool Button(Rect rect,string message,bool enabled=true){bool hover=rect.Contains(Event.current.mousePosition)&&enabled;Panel(rect,enabled?(hover?new Color(.34f,.2f,.43f):Ink):new Color(.14f,.12f,.19f),enabled?(hover?Mint:Pink):new Color(.3f,.25f,.36f));Text(rect.center.x,rect.y+8,message,1,enabled?Cream:new Color(.5f,.44f,.55f),true);bool old=GUI.enabled;GUI.enabled=enabled;bool hit=GUI.Button(rect,GUIContent.none,GUIStyle.none);GUI.enabled=old;return hit;}
 public static void Heart(float x,float y,bool full,Color color){string[] rows={"01010","11111","11111","01110","00100"};for(int yy=0;yy<5;yy++)for(int xx=0;xx<5;xx++)if(rows[yy][xx]=='1')Fill(new Rect(x+xx*2,y+yy*2,2,2),full?color:new Color(.3f,.22f,.4f));}
}
}
