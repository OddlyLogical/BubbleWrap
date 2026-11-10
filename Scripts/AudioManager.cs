using Godot;
namespace BubbleWrap;
public partial class AudioManager : Node
{
    private readonly AudioStreamPlayer[] voices=new AudioStreamPlayer[16];
    private readonly AudioStream[] clips=new AudioStream[5];
    private AudioStreamPlayer newSheet=null!;
    private AudioStreamPlayer ambience=null!;
    private readonly RandomNumberGenerator random=new();private int cursor;
    private const string PopBus="BubblePops";
    // Measured source peaks: -5.8, -6.1, -0.9, -2.8, -1.8 dBFS.
    private readonly float[] gains={2.8f,3.1f,-2.1f,-.2f,-1.2f};
    public override void _Ready(){
        random.Randomize();
        if(AudioServer.GetBusIndex(PopBus)<0){AudioServer.AddBus();int index=AudioServer.BusCount-1;AudioServer.SetBusName(index,PopBus);AudioServer.SetBusSend(index,"Master");AudioServer.AddBusEffect(index,new AudioEffectHardLimiter());}
        for(int i=0;i<clips.Length;i++){var clip=GD.Load<AudioStreamMP3>($"res://Assets/Audio/{i+1}.mp3");clip.Loop=false;clips[i]=clip;}
        for(int i=0;i<voices.Length;i++){voices[i]=new AudioStreamPlayer {Bus=PopBus,MaxPolyphony=1};AddChild(voices[i]);}
        var tear=GD.Load<AudioStreamMP3>("res://Assets/Audio/tanweraman-paper-tearing-252620.mp3");tear.Loop=false;
        newSheet=new AudioStreamPlayer {Stream=tear,Bus=PopBus,VolumeDb=-3,MaxPolyphony=1};AddChild(newSheet);
        var rain=GD.Load<AudioStreamMP3>("res://Assets/Audio/enternalrainsounds-calm-rain-ambience-421001.mp3");rain.Loop=true;
        ambience=new AudioStreamPlayer {Stream=rain,VolumeDb=-22,MaxPolyphony=1};AddChild(ambience);
        // Limit the final mix as well, since ambience and pop effects can overlap.
        if(AudioServer.GetBusEffectCount(0)==0)AudioServer.AddBusEffect(0,new AudioEffectHardLimiter());
    }
    public void Pop(){var p=voices[cursor++%voices.Length];int clip=random.RandiRange(0,clips.Length-1);p.Stream=clips[clip];p.PitchScale=random.RandfRange(.94f,1.07f);p.VolumeDb=gains[clip]+random.RandfRange(-.5f,.5f);p.Play();}
    public void NewSheet(){newSheet.Stop();newSheet.Play();}
    public void SetSoundEnabled(bool enabled){if(!enabled){StopAll();return;}ambience.StreamPaused=false;if(!ambience.Playing)ambience.Play();}
    public void PauseAll(){foreach(var p in voices)p.Stop();newSheet.Stop();ambience.StreamPaused=true;}
    public void StopAll(){foreach(var p in voices)p.Stop();newSheet.Stop();ambience.Stop();}
}


