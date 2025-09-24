using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

namespace XCAPE.Editor
{
    public class AndroidBuildHelper : EditorWindow
    {
        private static string androidSdkPath = "";
        private static string jdkPath = "";
        private static bool autoConnectProfiler = true;
        private static bool developmentBuild = true;
        
        [MenuItem("XCAPE/Android Build Helper")]
        public static void ShowWindow()
        {
            GetWindow<AndroidBuildHelper>("Android Build Helper");
        }
        
        void OnGUI()
        {
            GUILayout.Label("XCAPE Android Build & Deploy", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // SDK Configuration
            GUILayout.Label("SDK Configuration", EditorStyles.boldLabel);
            androidSdkPath = EditorGUILayout.TextField("Android SDK Path:", androidSdkPath);
            jdkPath = EditorGUILayout.TextField("JDK Path:", jdkPath);
            
            if (GUILayout.Button("Auto-Detect SDK Paths"))
            {
                AutoDetectSDKPaths();
            }
            
            GUILayout.Space(10);
            
            // Build Options
            GUILayout.Label("Build Options", EditorStyles.boldLabel);
            developmentBuild = EditorGUILayout.Toggle("Development Build", developmentBuild);
            autoConnectProfiler = EditorGUILayout.Toggle("Auto Connect Profiler", autoConnectProfiler);
            
            GUILayout.Space(10);
            
            // Build Actions
            GUILayout.Label("Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("1. Configure Unity SDK Paths"))
            {
                ConfigureSDKPaths();
            }
            
            if (GUILayout.Button("2. Build and Deploy to Emulator"))
            {
                BuildAndDeploy();
            }
            
            if (GUILayout.Button("3. View Emulator Logs"))
            {
                ViewEmulatorLogs();
            }
            
            GUILayout.Space(10);
            
            // Status
            GUILayout.Label("Status", EditorStyles.boldLabel);
            if (GUILayout.Button("Check Connected Devices"))
            {
                CheckConnectedDevices();
            }
        }
        
        private void AutoDetectSDKPaths()
        {
            // Try to detect Android SDK from environment
            string envAndroidHome = System.Environment.GetEnvironmentVariable("ANDROID_HOME");
            if (!string.IsNullOrEmpty(envAndroidHome) && Directory.Exists(envAndroidHome))
            {
                androidSdkPath = envAndroidHome;
                UnityEngine.Debug.Log($"Found Android SDK: {androidSdkPath}");
            }
            
            // Try to detect JDK
            string[] jdkPaths = {
                System.Environment.GetEnvironmentVariable("JAVA_HOME"),
                @"C:\Program Files\Eclipse Adoptium\jdk-11.0.19.7-hotspot",
                @"C:\Program Files\Java\jdk-11.0.19",
                @"C:\Program Files\Android\Android Studio\jre"
            };
            
            foreach (string path in jdkPaths)
            {
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    jdkPath = path;
                    UnityEngine.Debug.Log($"Found JDK: {jdkPath}");
                    break;
                }
            }
        }
        
        private void ConfigureSDKPaths()
        {
            if (string.IsNullOrEmpty(androidSdkPath))
            {
                AutoDetectSDKPaths();
            }
            
            if (!string.IsNullOrEmpty(androidSdkPath))
            {
                EditorPrefs.SetString("AndroidSdkRoot", androidSdkPath);
                UnityEngine.Debug.Log($"Android SDK configured: {androidSdkPath}");
            }
            
            if (!string.IsNullOrEmpty(jdkPath))
            {
                EditorPrefs.SetString("JdkPath", jdkPath);
                UnityEngine.Debug.Log($"JDK configured: {jdkPath}");
            }
            
            // Configure build settings for Android
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
            EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.Android;
            
            UnityEngine.Debug.Log("Unity configured for Android builds!");
        }
        
        private void BuildAndDeploy()
        {
            // Ensure Android is the active platform
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                UnityEngine.Debug.Log("Switching to Android platform...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            
            // Configure build settings
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetScenePaths();
            buildPlayerOptions.locationPathName = "builds/android/XCAPE.apk";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            if (developmentBuild)
            {
                buildPlayerOptions.options |= BuildOptions.Development;
            }
            
            if (autoConnectProfiler)
            {
                buildPlayerOptions.options |= BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
            }
            
            // Ensure build directory exists
            Directory.CreateDirectory("builds/android");
            
            UnityEngine.Debug.Log("Starting Android build...");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log("Build succeeded! Installing to emulator...");
                InstallToEmulator();
            }
            else
            {
                UnityEngine.Debug.LogError("Build failed!");
            }
        }
        
        private void InstallToEmulator()
        {
            string adbPath = Path.Combine(androidSdkPath, "platform-tools", "adb.exe");
            string apkPath = Path.Combine(Application.dataPath, "..", "builds", "android", "XCAPE.apk");
            
            if (File.Exists(adbPath) && File.Exists(apkPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = adbPath,
                    Arguments = $"install -r \"{apkPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                Process process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                UnityEngine.Debug.Log($"ADB Install result: {output}");
                
                if (output.Contains("Success"))
                {
                    UnityEngine.Debug.Log("APK installed successfully! Starting app...");
                    StartAppOnEmulator();
                }
            }
        }
        
        private void StartAppOnEmulator()
        {
            string adbPath = Path.Combine(androidSdkPath, "platform-tools", "adb.exe");
            string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = adbPath,
                Arguments = $"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            Process.Start(startInfo);
            UnityEngine.Debug.Log($"Starting {packageName} on emulator...");
        }
        
        private void ViewEmulatorLogs()
        {
            string adbPath = Path.Combine(androidSdkPath, "platform-tools", "adb.exe");
            
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = adbPath,
                Arguments = "logcat -s Unity",
                UseShellExecute = true
            };
            
            Process.Start(startInfo);
        }
        
        private void CheckConnectedDevices()
        {
            string adbPath = Path.Combine(androidSdkPath, "platform-tools", "adb.exe");
            
            if (File.Exists(adbPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = adbPath,
                    Arguments = "devices",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                Process process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                UnityEngine.Debug.Log($"Connected devices:\n{output}");
            }
        }
        
        private string[] GetScenePaths()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            string[] scenePaths = new string[scenes.Length];
            
            for (int i = 0; i < scenes.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }
            
            return scenePaths;
        }
    }
}
