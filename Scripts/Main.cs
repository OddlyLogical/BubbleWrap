using Godot;
namespace BubbleWrap;
public partial class Main : Control
{
    private readonly SettingsManager settings=new();
    private BubbleGrid grid=null!;private Label counter=null!,lifetime=null!;
    private AudioManager audio=null!;private HapticManager haptics=null!;private TouchController touch=null!;
    private Control shade=null!;private MarginContainer margin=null!;private bool dirty;
    public override void _Ready(){
        Engine.MaxFps=60;GetTree().AutoAcceptQuit=false;settings.Load();haptics=new();
        var theme=new Theme {DefaultFontSize=16};theme.SetColor("font_color","Label",new Color("294854"));theme.SetColor("font_color","Button",new Color("294854"));Theme=theme;
        foreach(string type in new[]{"Button","OptionButton","CheckButton"})
            foreach(string state in new[]{"font_color","font_hover_color","font_pressed_color","font_hover_pressed_color","font_focus_color","font_disabled_color"})
                theme.SetColor(state,type,new Color("294854"));
        var buttonStyle=new StyleBoxFlat {BgColor=new Color("f0f8fb"),CornerRadiusTopLeft=14,CornerRadiusTopRight=14,CornerRadiusBottomLeft=14,CornerRadiusBottomRight=14};theme.SetStylebox("normal","Button",buttonStyle);
        var pressed=(StyleBoxFlat)buttonStyle.Duplicate();pressed.BgColor=new Color("b9d6e2");theme.SetStylebox("pressed","Button",pressed);theme.SetStylebox("hover","Button",buttonStyle);
        theme.SetColor("font_color","CheckButton",new Color("294854"));theme.SetColor("font_color","OptionButton",new Color("294854"));theme.SetStylebox("normal","OptionButton",buttonStyle);
        var panelStyle=(StyleBoxFlat)buttonStyle.Duplicate();panelStyle.BgColor=new Color("e6f1f6");theme.SetStylebox("panel","PanelContainer",panelStyle);
        margin=new MarginContainer();AddChild(margin);margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);SafeMargins();Resized+=SafeMargins;
        var layout=new VBoxContainer();layout.AddThemeConstantOverride("separation",10);margin.AddChild(layout);
        var title=new Label {Text="Bubble Wrap",HorizontalAlignment=HorizontalAlignment.Center};title.AddThemeFontSizeOverride("font_size",30);layout.AddChild(title);
        counter=new Label {HorizontalAlignment=HorizontalAlignment.Center};layout.AddChild(counter);
        grid=new BubbleGrid {SizeFlagsVertical=SizeFlags.ExpandFill,CustomMinimumSize=new Vector2(0,100)};layout.AddChild(grid);
        var hint=new Label {Text="A little pressure. A little peace.",HorizontalAlignment=HorizontalAlignment.Center};hint.AddThemeFontSizeOverride("font_size",13);layout.AddChild(hint);
        var buttons=new HBoxContainer();layout.AddChild(buttons);
        Button("New Sheet",buttons).Pressed+=()=>{touch.Clear();grid.NewSheet();if(settings.Sound)audio.NewSheet();UpdateCounter();settings.Save();dirty=false;};
        Button("Settings",buttons).Pressed+=()=>ShowSettings(true);
        audio=new AudioManager();AddChild(audio);audio.SetSoundEnabled(settings.Sound);touch=new TouchController {Grid=grid};AddChild(touch);
        grid.Popped+=()=>{settings.Lifetime++;dirty=true;if(settings.Sound)audio.Pop();haptics.Pop(settings.Haptic);UpdateCounter();};
        BuildSettings();UpdateCounter();grid.Resized+=()=>CallDeferred(MethodName.UpdateCounter);
        var timer=new Timer {WaitTime=2,Autostart=true};AddChild(timer);timer.Timeout+=()=>{if(dirty){settings.Save();dirty=false;}};
    }
    private static Button Button(string text,Node parent){var b=new Button {Name=text.Replace(" ",""),Text=text,CustomMinimumSize=new Vector2(0,50),SizeFlagsHorizontal=SizeFlags.ExpandFill};parent.AddChild(b);return b;}
    private void SafeMargins(){int left=18,right=18,top=24,bottom=18;
        if(OS.GetName()=="Android"){var safe=DisplayServer.GetDisplaySafeArea();var screen=DisplayServer.ScreenGetSize();if(screen.X>0 && screen.Y>0){left+=(int)(safe.Position.X*Size.X/screen.X);right+=(int)((screen.X-safe.End.X)*Size.X/screen.X);top+=(int)(safe.Position.Y*Size.Y/screen.Y);bottom+=(int)((screen.Y-safe.End.Y)*Size.Y/screen.Y);}}
        margin.AddThemeConstantOverride("margin_left",left);margin.AddThemeConstantOverride("margin_right",right);margin.AddThemeConstantOverride("margin_top",top);margin.AddThemeConstantOverride("margin_bottom",bottom);
    }
    private void BuildSettings(){
        shade=new ColorRect {Color=new Color(.08f,.18f,.23f,.4f),Visible=false};AddChild(shade);shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var panel=new PanelContainer();shade.AddChild(panel);panel.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);panel.OffsetLeft=-175;panel.OffsetRight=175;panel.OffsetTop=-220;panel.OffsetBottom=220;
        var pad=new MarginContainer();panel.AddChild(pad);foreach(string s in new[]{"left","right","top","bottom"})pad.AddThemeConstantOverride("margin_"+s,22);
        var box=new VBoxContainer();box.AddThemeConstantOverride("separation",16);pad.AddChild(box);box.AddChild(new Label {Text="Make it yours"});box.AddChild(new Label {Text="Haptic feel"});
        var modes=new OptionButton();foreach(string s in new[]{"Soft","Crisp","Strong"})modes.AddItem(s);modes.Selected=settings.Haptic;box.AddChild(modes);modes.ItemSelected+=i=>{settings.Haptic=(int)i;settings.Save();haptics.Pop(settings.Haptic);};
        var sound=new CheckButton {Text="Sound",ButtonPressed=settings.Sound};box.AddChild(sound);sound.Toggled+=v=>{settings.Sound=v;audio.SetSoundEnabled(v);settings.Save();};
        var show=new CheckButton {Text="Show pop counter",ButtonPressed=settings.ShowCounter};box.AddChild(show);show.Toggled+=v=>{settings.ShowCounter=v;UpdateCounter();settings.Save();};
        lifetime=new Label();box.AddChild(lifetime);
        box.AddChild(new Label {Text=haptics.Available?"Device haptics ready":"Haptics require a supported Android device",AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new Vector2(280,0)});
        Button("Done",box).Pressed+=()=>ShowSettings(false);
    }
    private void ShowSettings(bool show){shade.Visible=show;touch.Blocked=show;touch.Clear();lifetime.Text=$"Lifetime: {settings.Lifetime:N0} pops";}
    private void UpdateCounter(){counter.Visible=settings.ShowCounter;counter.Text=$"{grid.Count} popped  ·  {grid.Bubbles.Length} bubbles";}
    public override void _Notification(int what){
        if(what==NotificationApplicationFocusOut || what==NotificationApplicationPaused){settings.Save();dirty=false;touch?.Clear();audio?.PauseAll();haptics?.Cancel();GetTree().Paused=true;}
        if(what==NotificationApplicationResumed || what==NotificationApplicationFocusIn){GetTree().Paused=false;audio?.SetSoundEnabled(settings.Sound);}
        if(what==NotificationWMCloseRequest || what==NotificationWMGoBackRequest){if(shade!=null && shade.Visible)ShowSettings(false);else{settings.Save();GetTree().Quit();}}
    }
}
