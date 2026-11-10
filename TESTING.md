# Verification

## Automated checks

Run from this directory with Godot 4.5.1 .NET on PATH:

```powershell
dotnet build BubbleWrap.sln
godot --headless --editor --path . --import --quit
godot --path . res://Tests/Smoke.tscn
```

The smoke scene exits 0 on success, 1 on failure. It checks scene instantiation, tap, duplicate suppression, reset, a full-row fast swipe, two independent fingers, rapid full-sheet popping, geometric crossing/near-miss/tangency, preference/lifetime round-trip, and animation completion. It also saves `../preview.png` when run with a rendering window. `--headless` exercises the touch controller directly because headless windows do not dispatch OS input; a rendered run exercises viewport input dispatch.

Actual desktop results: C# compilation passed with zero warnings and zero errors. Twelve rendered Godot smoke checks passed. The Android Java haptics library compiled successfully with Gradle 8.7 (only Android API deprecation notices). Godot asset import and scene load succeeded. Headless editor shutdown emits a Godot editor-settings teardown message; runtime scene checks do not emit this message.

## Physical Pixel 7a checklist

Actual device deployment: the final ~97 MB debug APK built, installed successfully over ADB, and launched on the connected Pixel 7a. Godot logged successful BubbleHaptics plugin initialization and main-loop startup. An on-device screenshot showed the sheet, six collapsed bubbles, and the updated counter. No Godot runtime exception was observed in the inspected process logs. Haptic feel, sound quality, sustained frame rate, and the full checklist below still need manual evaluation.

- Tap bubble centers and gaps: exactly one pop at a center and no pop in a gap.
- Retap a flattened bubble: no sound, haptic, or counter increment.
- Tap rapidly and swipe diagonally/vertically across rows: each intersected bubble collapses once.
- Use 2–5 fingers, release one, and keep dragging others.
- Compare Soft, Crisp, and Strong with system touch haptics enabled. Check that disabling system touch haptics suppresses vibration. Restore preferred system setting afterward.
- Confirm perceived sound/haptic/visual synchronization. Motor latency, subjective realism, and effect strength need a person holding the device.
- Turn Sound off, pop bubbles, exit, reopen: Sound remains off. Restore Sound if desired.
- Toggle Show pop counter, restart, and confirm preference persistence.
- New Sheet restores every bubble, resets current count, and keeps lifetime count in Settings.
- Android Back closes Settings; a second Back exits. Relaunch works.
- Background during a drag; return: no stuck finger or continuing vibration/audio.
- Test without network connectivity; gameplay uses no remote resources.
- Play continuously for five minutes; check smoothness, temperature, audio quality, and battery behavior. Sustained 60 FPS has not been instrumented yet.

Do not interpret a successful APK build as proof of tactile quality. Multi-finger hardware behavior and perceived audio/haptic timing require manual testing.

Update: New Sheet touch regression fixed by enabling GUI touch-to-mouse emulation and ignoring duplicate emulated mouse events in gameplay. All 14 rendered smoke checks passed, including tapping the actual New Sheet button through synthesized touchscreen events. All five supplied MP3s imported and loaded successfully. The updated APK was installed and launched on the Pixel 7a. An ADB-injected bubble tap followed by a New Sheet button tap left all bubbles restored and the on-device counter at zero, verified in a screenshot.


Audio/contrast update: source MP3 peaks measured with FFmpeg. Replaced -25 to -27 dB voice attenuation with per-clip gain targeting approximately -3 dBFS and a dedicated hard limiter. Rendered mixer-capture test measured a decoded pop peak of 0.684 full scale; all 16 checks passed. Raised-bubble outlines, shadows, reflections, and collapsed wrinkles now have stronger contrast.

New Sheet audio update: supplied paper-tearing MP3 imported, C# built with zero warnings/errors, and all 16 rendered smoke checks passed. Physical-device listening remains pending installation.

Visibility update: all standard button text states explicitly use dark ink, including hover, focus, pressed, and hover-pressed. Increased bubble body saturation, outline opacity/width, and shadow depth. Rendered preview verified the New Sheet label stays dark after touchscreen activation. All 16 checks and C# build passed.

Rain ambience update: C# compilation passed with zero warnings/errors; all 21 rendered checks passed, including rain asset loading/looping, sound toggle, background pause, and resume. On-device subjective audio balance still requires listening.
