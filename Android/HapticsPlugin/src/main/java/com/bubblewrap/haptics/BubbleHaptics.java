package com.bubblewrap.haptics;
import android.app.Activity;
import android.content.Context;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.os.VibratorManager;
import android.provider.Settings;
import org.godotengine.godot.Godot;
import org.godotengine.godot.plugin.GodotPlugin;
import org.godotengine.godot.plugin.UsedByGodot;

public final class BubbleHaptics extends GodotPlugin {
    private Vibrator vibrator;
    public BubbleHaptics(Godot godot) { super(godot); }
    @Override public String getPluginName() { return "BubbleHaptics"; }
    private Vibrator device() {
        Activity a=getActivity(); if(a==null)return null;
        if(vibrator==null) {
            if(Build.VERSION.SDK_INT>=31) { VibratorManager m=(VibratorManager)a.getSystemService(Context.VIBRATOR_MANAGER_SERVICE); if(m!=null)vibrator=m.getDefaultVibrator(); }
            else vibrator=(Vibrator)a.getSystemService(Context.VIBRATOR_SERVICE);
        }
        return vibrator;
    }
    @UsedByGodot public boolean available() { Vibrator v=device();return v!=null && v.hasVibrator(); }
    @UsedByGodot public void pop(int mode) {
        Activity a=getActivity(); Vibrator v=device(); if(a==null || v==null || !v.hasVibrator())return;
        if(Settings.System.getInt(a.getContentResolver(),Settings.System.HAPTIC_FEEDBACK_ENABLED,1)==0)return;
        int m=Math.max(0,Math.min(2,mode));
        try {
            if(Build.VERSION.SDK_INT>=29) {
                int effect=m==0?VibrationEffect.EFFECT_TICK:m==1?VibrationEffect.EFFECT_CLICK:VibrationEffect.EFFECT_HEAVY_CLICK;
                if(Build.VERSION.SDK_INT<30 || v.areEffectsSupported(effect)[0]==Vibrator.VIBRATION_EFFECT_SUPPORT_YES) {v.vibrate(VibrationEffect.createPredefined(effect));return;}
            }
            int duration=m==0?8:m==1?12:20;
            if(Build.VERSION.SDK_INT>=26)v.vibrate(VibrationEffect.createOneShot(duration,v.hasAmplitudeControl()?new int[]{45,130,230}[m]:VibrationEffect.DEFAULT_AMPLITUDE));
            else v.vibrate(duration);
        } catch(SecurityException ignored) { /* Missing permission: remain playable. */ }
    }
    @UsedByGodot public void cancel() {Vibrator v=device();if(v!=null)try{v.cancel();}catch(SecurityException ignored){} }
    @Override public void onMainPause() {cancel();super.onMainPause();}
}
