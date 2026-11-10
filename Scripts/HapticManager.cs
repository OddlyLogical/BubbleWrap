using Godot;
namespace BubbleWrap;
public sealed class HapticManager
{
    private readonly GodotObject? bridge;
    public bool Available=>bridge!=null && bridge.Call("available").AsBool();
    public HapticManager(){if(OS.GetName()=="Android" && Engine.HasSingleton("BubbleHaptics"))bridge=Engine.GetSingleton("BubbleHaptics");}
    public void Pop(int mode)=>bridge?.Call("pop",mode);
    public void Cancel()=>bridge?.Call("cancel");
}
