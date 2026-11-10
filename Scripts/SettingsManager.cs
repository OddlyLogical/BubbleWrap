using Godot;
namespace BubbleWrap;
public sealed class SettingsManager
{
    public bool Sound=true,ShowCounter=true;
    public int Haptic=1;
    public long Lifetime;
    public void Load(){var c=new ConfigFile();if(c.Load("user://preferences.cfg")!=Error.Ok)return;Sound=c.GetValue("settings","sound",true).AsBool();ShowCounter=c.GetValue("settings","counter",true).AsBool();Haptic=Mathf.Clamp(c.GetValue("settings","haptic",1).AsInt32(),0,2);Lifetime=System.Math.Max(0,c.GetValue("stats","lifetime",0L).AsInt64());}
    public void Save(){var c=new ConfigFile();c.SetValue("settings","sound",Sound);c.SetValue("settings","counter",ShowCounter);c.SetValue("settings","haptic",Haptic);c.SetValue("stats","lifetime",Lifetime);var e=c.Save("user://preferences.cfg");if(e!=Error.Ok)GD.PushWarning($"Could not save preferences: {e}");}
}
