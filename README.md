# Bubble Wrap 1.0

An offline portrait Godot 4.5.1 .NET / C# application. Open `project.godot` in **Godot 4.5.1 .NET** and press F6 on `Scenes/Main.tscn` or F5. Mouse clicks/drags work on desktop; Android supports independent fingers. Settings are under `user://preferences.cfg`. New Sheet clears the current count and preserves lifetime pops. Lifetime writes are batched every two seconds and flushed on background/exit; abrupt process termination can lose the last two seconds.

## Toolchain

- Godot **4.5.1 .NET**, including its matching **mono export templates** (not the standard editor).
- .NET SDK 10.0.403 for this build. Desktop targets net8.0; Android targets net10.0. Godot 4.5 Android requires .NET 9 or newer. No MAUI UI dependency.
- JDK 17 recommended by Godot; the local build uses installed OpenJDK 21.
- Android SDK platform 35, build-tools 35.0.0, platform-tools, command-line tools, NDK 28.1.13356709, CMake 3.10.2.4988404. Accept the Android SDK licenses.
- Gradle 8.7 for the independent haptic library (AGP 8.6.1). Godot's installed Android template provides its own Gradle wrapper for APK export.

Set Godot Editor Settings → Export → Android → Java SDK Path and Android SDK Path. Install matching export templates with Editor → Manage Export Templates.

## Build and deploy to Pixel 7a

From the project directory in PowerShell:

```powershell
dotnet build .\BubbleWrap.sln
# Set these to your installed tool locations:
$env:JAVA_HOME = 'C:\Program Files\Android\openjdk\jdk-21.0.8'
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
gradle -p .\Android\HapticsPlugin installPlugin
```

The last command compiles `addons/bubble_haptics/bin/BubbleHaptics.aar`. A compiled AAR is supplied. To build without global Gradle, open `Android/HapticsPlugin` in Android Studio or use an extracted Gradle 8.7 distribution's `bin/gradle.bat`. No binary wrapper JAR is required for this source project.

In Godot use Project → Install Android Build Template. The supplied export preset sets the Gradle directory to **res://Android**, so its generated build goes in `Android/build` alongside `HapticsPlugin`. Keep the native source folder. Enable the Bubble Haptics editor plugin in Project Settings → Plugins; it is already enabled. Export preset Android uses Gradle, arm64, portrait, package `com.bubblewrap.simulator`, vibration permission, and no Internet permission. Debug exports use Godot's debug keystore. Release signing requires your own keystore; keep credentials outside version control.

```powershell
# Substitute your Godot .NET console executable:
godot --headless --editor --path . --import --quit
New-Item -ItemType Directory -Force build | Out-Null
godot --headless --path . --export-debug Android build/BubbleWrap.apk
# Enable Developer options / USB debugging on the phone and authorize this PC.
& "$env:ANDROID_HOME\platform-tools\adb.exe" devices
& "$env:ANDROID_HOME\platform-tools\adb.exe" install -r .\build\BubbleWrap.apk
& "$env:ANDROID_HOME\platform-tools\adb.exe" shell am start -n com.bubblewrap.simulator/com.godot.game.GodotApp
```

Choose New Sheet and touch or swipe bubbles. Android Back closes Settings first, then exits the application. Reopen from the launcher. The current sheet is intentionally fresh on each launch; preferences and lifetime persist.

## Design

`BubbleGrid` draws the entire sheet in one custom Control, without physics bodies or per-bubble nodes. Logical positions use a density-independent 432-wide canvas. Mathematical segment/circle intersection over candidate grid cells catches fast swipes, including tangencies. Each finger retains its own prior point. The popped flag is set before feedback to prevent duplicate events. A 150 ms collapse animation leaves collapsed wrinkles. Idle sheets stop animation processing.

Five user-supplied MP3 clips, `Assets/Audio/1.mp3` through `5.mp3`, provide random pop variations. The original synthesis script remains at `Tools/generate-audio.ps1` as an optional asset generator. Sixteen preallocated players vary pitch and volume. Per-clip gain matches measured peaks to roughly -3 dBFS. A dedicated pop audio bus with a hard limiter controls overlapping transients. Reusing the oldest voice bounds rapid-pop resource usage.

The Android v2 plugin uses VibratorManager on API 31+, predefined tick/click/heavy-click effects where supported, and short amplitude-controlled one-shot fallbacks. It honors the system haptic-feedback toggle and gracefully handles absent hardware. No UI-thread dispatch or sleeps are introduced. Multiple effects may coalesce on Android during extremely fast swipes. Audio, haptics, counter, and visual state are triggered in the same pop callback; perceived synchronization and motor latency require hardware evaluation.

Pressure is not used: this is immediate contact popping. A reported touchscreen pressure channel is not a calibrated physical-force measurement.

## Validation

See `TESTING.md` for automated checks, device checks, and actual results. Godot 4.5 documents C# Android export as experimental.

Official references:
- https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_android.html
- https://docs.godotengine.org/en/4.5/tutorials/platform/android/android_plugin.html
- https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html

Application code, procedural drawings, and the optional sound synthesis script are supplied under the MIT license. User-supplied MP3 recordings retain their existing rights and are not relicensed here. Godot, .NET, and Android retain their own licenses.



New Sheet plays the supplied tanweraman-paper-tearing-252620.mp3 once, through the limited sound-effects bus. Repeated presses restart this sound rather than overlapping it. The Sound toggle and background pause stop it along with pop audio.

Background ambience: enternalrainsounds-calm-rain-ambience-421001.mp3 loops at -22 dB behind pop effects. Sound Off stops all audio. Backgrounding pauses the ambience; returning resumes it. A master hard limiter protects the combined mix.
