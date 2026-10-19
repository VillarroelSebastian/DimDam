from pathlib import Path
import math, wave, array, uuid
rate=22050; beat=60/108; bars=16; duration=bars*4*beat; count=round(duration*rate)
buffer=array.array('f',[0])*count
# Original, seamless 16-bar celesta waltz-like menu theme in C major (4/4).
def note(midi,start,length,gain,kind):
    freq=440*2**((midi-69)/12); n=int(length*rate); offset=int(start*rate)
    for i in range(n):
        t=i/rate; attack=min(1,t/.018); release=min(1,(length-t)/.10)
        if kind=='bell': value=(math.sin(2*math.pi*freq*t)+.28*math.sin(2*math.pi*freq*2*t)+.08*math.sin(2*math.pi*freq*3*t))*math.exp(-t*4)
        elif kind=='pad': value=(math.sin(2*math.pi*freq*t)+.2*math.sin(2*math.pi*freq*2*t))*.6
        else: value=math.sin(2*math.pi*freq*t)*math.exp(-t*2)
        buffer[(offset+i)%count]+=value*gain*attack*release
chords=[(48,52,55),(45,48,52),(41,45,48),(43,47,50)]
melodies=[[76,79,84,79,76,74,72,79],[76,81,84,83,81,76,72,76],[77,81,84,81,79,77,76,74],[74,79,83,86,83,79,74,71]]
for bar in range(bars):
    chord=chords[bar%4]; start=bar*4*beat
    for pitch in chord: note(pitch+12,start,4*beat,.037,'pad')
    for step in range(4): note(chord[0]-12,start+step*beat,.75*beat,.11,'bass')
    melody=melodies[bar%4]
    for step,pitch in enumerate(melody):
        if bar>=8 and step in (2,6): pitch+=12
        note(pitch,start+step*.5*beat,.85*beat,.15,'bell')
        note(pitch,start+step*.5*beat+beat*.75,.65*beat,.026,'bell')
peak=max(abs(x) for x in buffer); pcm=array.array('h',(round(x/peak*.70*32767) for x in buffer))
path=Path('Assets/GameDuo/Resources/MenuTheme.wav')
with wave.open(str(path),'wb') as out: out.setnchannels(1);out.setsampwidth(2);out.setframerate(rate);out.writeframes(pcm.tobytes())
path.with_suffix('.wav.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nAudioImporter:\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 1\n    quality: 0.8\n    conversionMode: 0\n  forceToMono: 1\n  normalize: 0\n  preloadAudioData: 1\n  loadInBackground: 0\n  ambisonic: 0\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
print(f'Menu music: {duration:.2f}s, peak {max(abs(x) for x in pcm)/32768:.3f}, {path.stat().st_size} bytes')
