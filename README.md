### Unity Android extension for Visual Studio AGDE

## Requirements

* Windows
* Unity 2021.3 or higher
* Visual Studio 2022 or higher
* [Android Game Development Extension for Visual Studio](https://developer.android.com/games/agde) 23.2.87 or higher
* [JDK 17](https://www.oracle.com/java/technologies/javase/jdk17-archive-downloads.html) or higher (required only for device querying and application launching)

## Settings

The settings can be found under Preferences->Android->Rubix AGDE Visual Studio Extension.


| **General Settings**|  |
| ----------------- |------------|
| Enabled|Enable/Disable Visual Studio AGDE solution/project generation. |
| **Launch Settings** |  |
| Use Unity Jdk | Use Unity's JDK both for building and launching the application.<br>Note: JDK 17 or higher is required. |
| Custom Jdk Path | Use a custom JDK for launching the application. |


## Quick Steps

* Add AGDE Extension package to your project:
    * In Unity Editor, go to Window->Package Manager
    * Click **+** button, and **Add Package from git URL**
    * Copy paste https://github.com/Rubix1991/com.rubix.unity.android.visualstudio.agde.git
    * Note: If you already have the package added, you might want to click Update to get latest changes
* Export gradle project:
    * Switch to Android platform from Build Settings window
    * Enable **Export Project**
    * Export
* In exported folder, double click on **OpenVisualStudioAGDE.cmd**, this does few things:
    * Sets env variables:
        * ANDROID_SDK_ROOT and AGDE_JAVA_HOME - required by Visual Studio to query android devices, and launch the application.
        * JAVA_HOME - used by gradle build.
    * Opens the solution
* That's it, you can now build and run the android application from Visual Studio

## Notes

* The actual solution/project is generated in <exported_folder>/VisualStudioAGDE
* If you pick multiple target architectures for scripting backend il2cpp, it doesn't matter what Visual Studio solution platform you'll pick (Android-armeabi-v7a or Android-arm64-v8a):
    * gradle will still build all for all architectures
    * when running the app, the most compatible ABI will picked, for ex., if you have Android-armeabi-v7a, but had ARMv7 and ARM64 selected in Unity's player settings, when the app is ran arm64-v8a might be picked for your phone, since it was most compatible one.
    * thus if you want to explicitly target one architecture, you need to pick one architecture in Unity's Player Settings.