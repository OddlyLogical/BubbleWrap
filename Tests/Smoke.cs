using Godot;
using System;
namespace BubbleWrap.Tests;
public partial class Smoke : Node
{
    private int checks;
    private void Check(bool value,string message){if(!value)throw new Exception(message);checks++;GD.Print("PASS: "+message);}
    public override async void _Ready(){try{
        var main=GD.Load<PackedScene>("res://Scenes/Main.tscn").Instantiate<Control>();main.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft);main.Size=GetViewport().GetVisibleRect().Size;AddChild(main);
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        if(DisplayServer.GetName()!="headless"){
            foreach(var child in main.GetChildren())if(child is AudioManager manager){
                AudioStreamPlayer? rain=null;
                foreach(var voice in manager.GetChildren())if(voice is AudioStreamPlayer player && player.Stream?.ResourcePath.Contains("enternalrainsounds")==true)rain=player;
                Check(rain?.Stream is AudioStreamMP3 mp3 && mp3.Loop,"rain ambience loads with looping enabled");
                manager.SetSoundEnabled(true);Check(rain!.Playing,"sound on starts rain ambience");
                manager.SetSoundEnabled(false);Check(!rain.Playing,"sound off stops rain ambience");
                manager.SetSoundEnabled(true);manager.PauseAll();Check(rain.StreamPaused,"background pauses rain ambience");
                manager.SetSoundEnabled(true);Check(!rain.StreamPaused && rain.Playing,"returning resumes rain ambience");
            }
            int bus=AudioServer.GetBusIndex("BubblePops");
            Check(bus>=0 && AudioServer.GetBusEffect(bus,0) is AudioEffectHardLimiter,"pop audio bus has peak limiter");
            var capture=new AudioEffectCapture();AudioServer.AddBusEffect(bus,capture);
            foreach(var child in main.GetChildren())if(child is AudioManager manager)manager.Pop();
            await ToSignal(GetTree().CreateTimer(.25),Timer.SignalName.Timeout);
            var samples=capture.GetBuffer(capture.GetFramesAvailable());float peak=0;
            foreach(var sample in samples)peak=Math.Max(peak,Math.Max(Math.Abs(sample.X),Math.Abs(sample.Y)));
            Check(peak>.1f,$"decoded MP3 reaches audible mixer level (peak {peak:F3})");
            AudioServer.RemoveBusEffect(bus,1);
        }
        BubbleGrid? grid=null;foreach(var child in main.FindChildren("*","BubbleGrid",true,false))if(child is BubbleGrid g)grid=g;
        // C# custom types are not registered as native class names.
        grid ??= FindGrid(main);
        Check(grid!=null && grid.Bubbles.Length>0,"main scene generates bubbles");
        var g2=grid!;var first=g2.Bubbles[0].Center;g2.Stroke(first,first);Check(g2.Count==1,"single tap pops exactly one");g2.Stroke(first,first);Check(g2.Count==1,"duplicate tap ignored");
        g2.NewSheet();Check(g2.Count==0 && !g2.Bubbles[0].Popped,"new sheet clears state");
        g2.Stroke(g2.Bubbles[0].Center,g2.Bubbles[g2.Columns-1].Center);Check(g2.Count==g2.Columns,"fast swipe crosses entire row");
        g2.NewSheet();GetTree().Paused=false;
        for(int finger=0;finger<2;finger++)Dispatch(main,new InputEventScreenTouch {Index=finger,Pressed=true,Position=g2.GetGlobalTransformWithCanvas()*g2.Bubbles[finger].Center});
        Input.FlushBufferedEvents();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        Check(g2.Count==2,$"independent simultaneous touches (received {g2.Count})");
        for(int finger=0;finger<2;finger++)Dispatch(main,new InputEventScreenTouch {Index=finger,Pressed=false});
        for(int i=0;i<g2.Bubbles.Length;i++)g2.Stroke(g2.Bubbles[i].Center,g2.Bubbles[i].Center);
        Check(g2.Count==g2.Bubbles.Length,"rapid interaction pops each bubble once");
        Check(Geometry.Hits(-100,0,100,0,0,0,2),"segment crosses circle");Check(!Geometry.Hits(-100,3,100,3,0,0,2),"near miss rejected");Check(Geometry.Hits(-10,2,10,2,0,0,2),"tangent included");
        var config=new SettingsManager();config.Load();long original=config.Lifetime;config.Sound=false;config.ShowCounter=false;config.Haptic=2;config.Lifetime=123456;config.Save();var restored=new SettingsManager();restored.Load();Check(!restored.Sound && !restored.ShowCounter && restored.Haptic==2 && restored.Lifetime==123456,"settings and lifetime persist");config.Sound=true;config.ShowCounter=true;config.Haptic=1;config.Lifetime=original;config.Save();
        if(DisplayServer.GetName()!="headless"){
            var reset=main.FindChild("NewSheet",true,false) as Button;
            Check(reset!=null,"New Sheet button exists");
            var point=reset!.GetGlobalRect().GetCenter();
            Input.ParseInputEvent(new InputEventScreenTouch {Index=0,Pressed=true,Position=point});Input.FlushBufferedEvents();
            await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            Input.ParseInputEvent(new InputEventScreenTouch {Index=0,Pressed=false,Position=point});Input.FlushBufferedEvents();
            await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            Check(g2.Count==0,"touching New Sheet button resets popped bubbles");
        }
        g2.NewSheet();g2.Stroke(first,first);
        await ToSignal(GetTree().CreateTimer(.2),Timer.SignalName.Timeout);Check(g2.Bubbles[0].Popped && g2.Bubbles[0].Animation==0,"collapse completes and state remains");
        if(!DisplayServer.GetName().Equals("headless",StringComparison.OrdinalIgnoreCase)) {await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);GetViewport().GetTexture().GetImage().SavePng("res://../preview.png");}
        GD.Print($"SMOKE PASSED: {checks} checks");GetTree().Quit();
    }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
    private static BubbleGrid? FindGrid(Node n){if(n is BubbleGrid g)return g;foreach(var c in n.GetChildren()){var found=FindGrid(c);if(found!=null)return found;}return null;}
    private void Dispatch(Node main,InputEvent e){
        // Headless windows do not route OS input; exercise the same controller directly.
        if(DisplayServer.GetName()=="headless"){foreach(var c in main.GetChildren())if(c is TouchController t)t._Input(e);}
        else GetViewport().PushInput(e);
    }
}
