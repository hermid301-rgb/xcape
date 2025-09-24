using UnityEngine;
using UnityEditor;
using System.IO;

namespace XCAPE.Editor
{
    [InitializeOnLoad]
    public class ProjectSetup
    {
        static ProjectSetup()
        {
            // Configurar automáticamente cuando Unity se abre
            EditorApplication.delayCall += SetupProject;
        }
        
        [MenuItem("XCAPE/Setup Project for Android")]
        public static void SetupProject()
        {
            Debug.Log("Setting up XCAPE project for Android...");
            
            // 1. Configurar Player Settings
            SetupPlayerSettings();
            
            // 2. Configurar Build Settings
            SetupBuildSettings();
            
            // 3. Configurar Android SDK
            SetupAndroidSDK();
            
            Debug.Log("Project setup completed!");
        }
        
        private static void SetupPlayerSettings()
        {
            // Configuración básica
            PlayerSettings.companyName = "XCAPE Studios";
            PlayerSettings.productName = "XCAPE Bowling";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.xcape.bowling");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            
            // Configuración Android específica
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            
            // Graphics
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2
            });
            
            // Configuración de URP
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            
            Debug.Log("Player Settings configured for Android");
        }
        
        private static void SetupBuildSettings()
        {
            // Buscar escenas en el proyecto
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[sceneGuids.Length];
            
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                buildScenes[i] = new EditorBuildSettingsScene(scenePath, true);
                Debug.Log($"Added scene to build: {scenePath}");
            }
            
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log($"Build Settings configured with {buildScenes.Length} scenes");
        }
        
        private static void SetupAndroidSDK()
        {
            // Configurar Android SDK desde variable de entorno
            string androidHome = System.Environment.GetEnvironmentVariable("ANDROID_HOME");
            if (!string.IsNullOrEmpty(androidHome) && Directory.Exists(androidHome))
            {
                EditorPrefs.SetString("AndroidSdkRoot", androidHome);
                Debug.Log($"Android SDK configured: {androidHome}");
                
                // Configurar JDK si está disponible
                string[] jdkPaths = {
                    System.Environment.GetEnvironmentVariable("JAVA_HOME"),
                    Path.Combine(androidHome, "jre"),
                    @"C:\Program Files\Eclipse Adoptium\jdk-11.0.19.7-hotspot",
                    @"C:\Program Files\Java\jdk-11.0.19"
                };
                
                foreach (string jdkPath in jdkPaths)
                {
                    if (!string.IsNullOrEmpty(jdkPath) && Directory.Exists(jdkPath))
                    {
                        EditorPrefs.SetString("JdkPath", jdkPath);
                        Debug.Log($"JDK configured: {jdkPath}");
                        break;
                    }
                }
            }
            else
            {
                Debug.LogWarning("ANDROID_HOME not found. Please configure Android SDK manually.");
            }
        }
        
        [MenuItem("XCAPE/Quick Build and Deploy")]
        public static void QuickBuildAndDeploy()
        {
            Debug.Log("Starting quick build and deploy...");
            
            // Asegurar que estamos en Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("Switching to Android platform...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                return; // Unity necesita reiniciar el proceso después del switch
            }
            
            // Configurar build
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();
            buildOptions.scenes = GetEnabledScenePaths();
            buildOptions.locationPathName = "builds/android/XCAPE.apk";
            buildOptions.target = BuildTarget.Android;
            buildOptions.options = BuildOptions.Development | BuildOptions.ConnectWithProfiler;
            
            // Crear directorio si no existe
            Directory.CreateDirectory("builds/android");
            
            // Build
            var report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded! Deploying to emulator...");
                DeployToEmulator();
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.totalErrors} errors");
            }
        }
        
        private static void DeployToEmulator()
        {
            string androidHome = EditorPrefs.GetString("AndroidSdkRoot");
            string adbPath = Path.Combine(androidHome, "platform-tools", "adb.exe");
            string apkPath = Path.Combine(Application.dataPath, "..", "builds", "android", "XCAPE.apk");
            
            if (File.Exists(adbPath) && File.Exists(apkPath))
            {
                // Instalar APK
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = adbPath,
                    Arguments = $"install -r \"{apkPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                var process = System.Diagnostics.Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                Debug.Log($"Deploy result: {output}");
                
                // Iniciar la app
                if (output.Contains("Success"))
                {
                    string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
                    startInfo.Arguments = $"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1";
                    System.Diagnostics.Process.Start(startInfo);
                    Debug.Log($"XCAPE Bowling launched on emulator!");
                }
            }
            else
            {
                Debug.LogError($"ADB not found at: {adbPath}");
            }
        }
        
        private static string[] GetEnabledScenePaths()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            System.Collections.Generic.List<string> enabledScenes = new System.Collections.Generic.List<string>();
            
            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes.Add(scene.path);
                }
            }
            
            return enabledScenes.ToArray();
        }
    }
}
