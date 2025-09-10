#if UNITY_IOS && UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace XCAPE.Editor
{
    public class iOSPostProcess
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                ConfigureXcodeProject(buildPath);
            }
        }

        private static void ConfigureXcodeProject(string buildPath)
        {
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            string targetGuid = project.GetUnityMainTargetGuid();

            // Add AdMob frameworks (these are usually auto-added by AdMob SDK)
            project.AddFrameworkToProject(targetGuid, "AdSupport.framework", false);
            project.AddFrameworkToProject(targetGuid, "AppTrackingTransparency.framework", false);
            project.AddFrameworkToProject(targetGuid, "StoreKit.framework", false);

            // Configure Info.plist for AdMob
            string plistPath = Path.Combine(buildPath, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // Add AdMob App ID (Replace with your actual AdMob App ID)
            rootDict.SetString("GADApplicationIdentifier", "ca-app-pub-3940256099942544~1458002511");

            // Add App Transport Security settings for ads
            var atsDict = rootDict.CreateDict("NSAppTransportSecurity");
            atsDict.SetBoolean("NSAllowsArbitraryLoads", true);

            // Add SKAdNetwork IDs for iOS 14+ attribution
            var skAdNetworkArray = rootDict.CreateArray("SKAdNetworkItems");
            
            // Google's SKAdNetwork IDs (subset - full list available from AdMob documentation)
            string[] skAdNetworkIds = {
                "cstr6suwn9.skadnetwork",
                "4fzdc2evr5.skadnetwork",
                "4pfyvq9l8r.skadnetwork",
                "2fnua5tdw4.skadnetwork",
                "ydx93a7ass.skadnetwork",
                "5a6flpkh64.skadnetwork",
                "p78axxw29g.skadnetwork",
                "v72qych5uu.skadnetwork",
                "ludvb6z3bs.skadnetwork",
                "cp8zw746q7.skadnetwork",
                "3sh42y64q3.skadnetwork",
                "c6k4g5qg8m.skadnetwork",
                "s39g8k73mm.skadnetwork",
                "3qy4746246.skadnetwork",
                "f38h382jlk.skadnetwork",
                "hs6bdukanm.skadnetwork",
                "v4nxqhlyqp.skadnetwork",
                "wzmmz9fp6w.skadnetwork",
                "yclnxrl5pm.skadnetwork",
                "t38b2kh725.skadnetwork"
            };

            foreach (string skAdNetworkId in skAdNetworkIds)
            {
                var skAdNetworkDict = skAdNetworkArray.AddDict();
                skAdNetworkDict.SetString("SKAdNetworkIdentifier", skAdNetworkId);
            }

            plist.WriteToFile(plistPath);
            project.WriteToFile(projectPath);

            UnityEngine.Debug.Log("[iOSPostProcess] Configured Xcode project for AdMob");
        }
    }
}
#endif
