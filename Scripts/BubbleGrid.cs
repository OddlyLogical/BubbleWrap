using Godot;
using System;
namespace BubbleWrap;
public struct Bubble { public Vector2 Center; public bool Popped; public float Animation; }
public partial class BubbleGrid : Control
{
    public event Action? Popped;
    public Bubble[] Bubbles { get; private set; }=Array.Empty<Bubble>();
    public int Count { get; private set; }
    public int Columns { get; private set; }
    private float pitch,radius;
    private Vector2 origin;
    private readonly StyleBoxFlat plastic=new() { BgColor=new Color(1,1,1,.15f),BorderColor=new Color(1,1,1,.4f),BorderWidthLeft=1,BorderWidthRight=1,BorderWidthTop=1,BorderWidthBottom=1,CornerRadiusTopLeft=16,CornerRadiusTopRight=16,CornerRadiusBottomLeft=16,CornerRadiusBottomRight=16 };
    public override void _Ready() { MouseFilter=MouseFilterEnum.Ignore; Resized+=Layout;Layout();SetProcess(false); }
    private void Layout() {
        if(Size.X<100 || Size.Y<100)return;
        if(Bubbles.Length==0) { Columns=Math.Max(1,(int)(Size.X/62));Bubbles=new Bubble[Columns*Math.Max(1,(int)(Size.Y/62))]; }
        int rows=Bubbles.Length/Columns;pitch=Math.Min(Size.X/Columns,Size.Y/rows);radius=pitch*.435f;
        origin=(Size-new Vector2(Columns*pitch,rows*pitch))/2+Vector2.One*pitch/2;
        for(int i=0;i<Bubbles.Length;i++)Bubbles[i].Center=origin+new Vector2(i%Columns,i/Columns)*pitch;
        QueueRedraw();
    }
    public void NewSheet() {Count=0;for(int i=0;i<Bubbles.Length;i++){Bubbles[i].Popped=false;Bubbles[i].Animation=0;}SetProcess(false);QueueRedraw();}
    public void Stroke(Vector2 a,Vector2 b) {
        if(pitch<=0)return;
        int x0=Math.Clamp((int)Math.Floor((Math.Min(a.X,b.X)-radius-origin.X)/pitch),0,Columns-1),x1=Math.Clamp((int)Math.Ceiling((Math.Max(a.X,b.X)+radius-origin.X)/pitch),0,Columns-1);
        int rows=Bubbles.Length/Columns;
        int y0=Math.Clamp((int)Math.Floor((Math.Min(a.Y,b.Y)-radius-origin.Y)/pitch),0,rows-1),y1=Math.Clamp((int)Math.Ceiling((Math.Max(a.Y,b.Y)+radius-origin.Y)/pitch),0,rows-1);
        for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++) {int i=y*Columns+x;
            if(Bubbles[i].Popped || !Geometry.Hits(a.X,a.Y,b.X,b.Y,Bubbles[i].Center.X,Bubbles[i].Center.Y,radius))continue;
            Bubbles[i].Popped=true;Bubbles[i].Animation=.15f;Count++;Popped?.Invoke();SetProcess(true);
        }QueueRedraw();
    }
    public override void _Process(double delta) {bool active=false;for(int i=0;i<Bubbles.Length;i++)if(Bubbles[i].Animation>0){Bubbles[i].Animation=Math.Max(0,Bubbles[i].Animation-(float)delta);active|=Bubbles[i].Animation>0;}QueueRedraw();SetProcess(active);}
    public override void _Draw() {
        DrawStyleBox(plastic,new Rect2(Vector2.Zero,Size));
        foreach(var bubble in Bubbles) {
            Vector2 c=bubble.Center;float raised=bubble.Popped?bubble.Animation/.15f:1,r=radius*(.91f+.09f*raised);
            DrawCircle(c+new Vector2(0,2+raised*3),r,new Color(.12f,.26f,.35f,.14f+.25f*raised));
            DrawCircle(c,r,new Color(.43f,.66f,.77f,.85f));
            DrawCircle(c-Vector2.One,r*.89f,new Color(.94f,.98f,1,.12f+.48f*raised));
            DrawArc(c,r,0,Mathf.Tau,40,new Color(.20f,.38f,.49f,.86f),2.1f,true);
            DrawArc(c-Vector2.One,r*.9f,3.45f,5.95f,24,new Color(1,1,1,.35f+.65f*raised),2.4f,true);
            DrawArc(c,r*.78f,.15f,2.5f,20,new Color(.18f,.36f,.48f,.12f+.36f*raised),2,true);
            DrawArc(c+new Vector2(-r*.22f,-r*.25f),r*.3f*raised,3.6f,5.6f,12,new Color(1,1,1,.65f*raised),3,true);
            if(bubble.Popped)for(int k=0;k<5;k++){Vector2 d=Vector2.FromAngle(k*1.256f+.4f);DrawLine(c+d*r*.22f,c+d.Rotated(.13f)*r*.81f,new Color(.26f,.45f,.55f,.50f*(1-raised)),1.5f,true);DrawLine(c+d*r*.24f+Vector2.One,c+d.Rotated(.13f)*r*.8f+Vector2.One,new Color(1,1,1,.55f*(1-raised)),1,true);}
        }
    }
}

