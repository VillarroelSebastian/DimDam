from pathlib import Path
import struct,re,wave,math,array
root=Path('Assets/GameDuo'); report=[]
def check(ok,msg):
    report.append(('PASS ' if ok else 'FAIL ')+msg)
    if not ok: raise AssertionError(msg)
def boxes(f,start,end):
    pos=start
    while pos+8<=end:
        f.seek(pos); size,kind=struct.unpack('>I4s',f.read(8)); head=8
        if size==1: size=struct.unpack('>Q',f.read(8))[0];head=16
        if size==0: size=end-pos
        if size<head: break
        yield kind,pos+head,pos+size
        pos+=size
def audio_tracks(path):
    tracks=[]
    with path.open('rb') as f:
        moov=next(b for b in boxes(f,0,path.stat().st_size) if b[0]==b'moov')
        for kind,start,end in boxes(f,moov[1],moov[2]):
            if kind!=b'trak': continue
            mdia=next((b for b in boxes(f,start,end) if b[0]==b'mdia'),None)
            if not mdia: continue
            entries=list(boxes(f,mdia[1],mdia[2]));hdlr=next(b for b in entries if b[0]==b'hdlr')
            f.seek(hdlr[1]+8)
            if f.read(4)!=b'soun': continue
            minf=next(b for b in entries if b[0]==b'minf');stbl=next(b for b in boxes(f,minf[1],minf[2]) if b[0]==b'stbl')
            entries=list(boxes(f,stbl[1],stbl[2]));stsd=next(b for b in entries if b[0]==b'stsd')
            f.seek(stsd[1]+8);size,codec=struct.unpack('>I4s',f.read(8));f.seek(stsd[1]+8+24);channels,bits=struct.unpack('>HH',f.read(4))
            stsz=next(b for b in entries if b[0]==b'stsz');f.seek(stsz[1]+4);sample_size,samples=struct.unpack('>II',f.read(8))
            tracks.append((codec.decode(),channels,samples))
    return tracks
build=Path('ProjectSettings/EditorBuildSettings.asset').read_text()
scenes=re.findall(r'path: (Assets/GameDuo/Scenes/[^\n]+)',build)
check(len(scenes)==6 and scenes[0].endswith('00_Menu.unity'),'Menu first, followed by all five campaign scenes in build')
for scene in scenes: check(Path(scene).exists(),'Scene exists: '+Path(scene).name)
refs=re.findall(r'guid: ([a-f0-9]{32})', (root/'Resources/GameDuoMovies.asset').read_text())[1:]
lookup={re.search(r'guid: (\w+)',p.read_text()).group(1):p.with_suffix('') for p in (root/'video').glob('*.mp4.meta')}
check(len(refs)==9 and all(g in lookup for g in refs),'All nine cinematic references resolve')
check([lookup[g].stem for g in refs]==['1','2','3','4','5','6','7','8','the final'],'Cinematic order 1-8, the final')
for guid in refs:
    path=lookup[guid]; tracks=audio_tracks(path)
    check(bool(tracks) and all(t[2]>0 for t in tracks),f'{path.name}: nonempty encoded audio tracks {tracks}')
    check('importAudio: 1' in path.with_suffix('.mp4.meta').read_text(),path.name+': Unity imports audio')
with wave.open(str(root/'Resources/MenuTheme.wav')) as f:
    pcm=array.array('h',f.readframes(f.getnframes()));rms=math.sqrt(sum(x*x for x in pcm)/len(pcm))/32768
    check(f.getnframes()/f.getframerate()>30 and rms>.02,'Menu loop has non-silent audio, over 30 seconds')
Path('Logs/MenuFlowStaticValidation.txt').write_text('\n'.join(report)+'\n')
print('\n'.join(report))
