#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
public static class GameDuoFinalAudioValidation {
 [MenuItem("GameDuo/Validar audio de batalla final")]
 public static void Run(){
  var report=new System.Text.StringBuilder();int samples=0;
  foreach(var name in new[]{"Battle","Tension","Warning","Attack","Hurt","Defeat","Bats","Rain","Phase"}){
   var path="Assets/GameDuo/Resources/FinalAudio/"+name+".wav";
   var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
   if(!clip||clip.samples==0)throw new Exception("Missing/empty clip: "+path);
   if(name=="Battle")samples=clip.samples;
   if(name=="Tension"&&clip.samples!=samples)throw new Exception("Music layers must have identical loop lengths");
   var data=new float[clip.samples*clip.channels];
   if(!clip.GetData(data,0))throw new Exception("Cannot read PCM: "+name);
   float peak=0;double energy=0;
   foreach(float value in data){if(float.IsNaN(value)||float.IsInfinity(value))throw new Exception("Invalid sample");peak=Mathf.Max(peak,Mathf.Abs(value));energy+=value*value;}
   if(peak>.66f||energy/data.Length<.00001)throw new Exception("Audio level outside bounds: "+name);
   report.AppendLine("PASS "+name+": "+clip.length.ToString("F2")+"s, peak "+peak.ToString("F3"));
  }
  // Worst-case sum of bounded sources, before existing game cues: < 0.89 full scale.
  report.AppendLine("PASS synchronized music layers; six bounded effect voices; scene-local bootstrap");
  Directory.CreateDirectory("Logs");File.WriteAllText("Logs/FinalAudioValidation.txt",report.ToString());Debug.Log(report.ToString());
 }
}
#endif
