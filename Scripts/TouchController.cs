using Godot;
using System.Collections.Generic;
namespace BubbleWrap;
public partial class TouchController : Node
{
    public BubbleGrid Grid=null!;public bool Blocked;
    private readonly Dictionary<int,Vector2> fingers=new(10);
    public void Clear()=>fingers.Clear();
    private Vector2 Local(Vector2 p)=>Grid.GetGlobalTransformWithCanvas().AffineInverse()*p;
    public override void _Input(InputEvent e){if(Blocked)return;
        // GUI controls need touch-to-mouse emulation; gameplay already handles real touches.
        if(e is InputEventMouse && e.Device==-1)return;
        if(e is InputEventScreenTouch t){if(t.Pressed && !t.Canceled)Start(t.Index,t.Position);else fingers.Remove(t.Index);}
        else if(e is InputEventScreenDrag d && fingers.TryGetValue(d.Index,out var previous)){var p=Local(d.Position);Grid.Stroke(previous,p);fingers[d.Index]=p;}
        else if(e is InputEventMouseButton m && m.ButtonIndex==MouseButton.Left){if(m.Pressed)Start(-1,m.Position);else fingers.Remove(-1);}
        else if(e is InputEventMouseMotion motion && fingers.TryGetValue(-1,out var old)){var p=Local(motion.Position);Grid.Stroke(old,p);fingers[-1]=p;}
    }
    private void Start(int id,Vector2 screen){var p=Local(screen);if(!new Rect2(Vector2.Zero,Grid.Size).HasPoint(p))return;fingers[id]=p;Grid.Stroke(p,p);}
}
