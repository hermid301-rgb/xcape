#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XCAPE.Editor
{
    [InitializeOnLoad]
    public static class UGSLinkOnLoad
    {
        private const string LinkFilePath = "ProjectSettings/UGS.link.json";

        [Serializable]
        private class UGSLinkData
        {
            public string organizationId;
            public string projectId;
            public string projectName;
        }

        static UGSLinkOnLoad()
        {
            // Delay a bit to allow editor to settle
            EditorApplication.update += TryLink;
        }

        private static void TryLink()
        {
            EditorApplication.update -= TryLink;
            try
            {
                if (!string.IsNullOrEmpty(Application.cloudProjectId))
                    return; // Already linked

                var data = LoadData();
                if (data == null || string.IsNullOrWhiteSpace(data.organizationId) || string.IsNullOrWhiteSpace(data.projectId))
                    return; // Nothing to link

                if (ApplyLink(data.organizationId.Trim(), data.projectId.Trim(), string.IsNullOrWhiteSpace(data.projectName) ? null : data.projectName.Trim()))
                {
                    Debug.Log("[XCAPE] UGS linked from UGS.link.json / environment.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XCAPE] UGS auto-link failed: {e.Message}");
            }
        }

        private static UGSLinkData LoadData()
        {
            // 1) JSON file in ProjectSettings
            if (File.Exists(LinkFilePath))
            {
                try
                {
                    var json = File.ReadAllText(LinkFilePath);
                    var data = JsonUtility.FromJson<UGSLinkData>(json);
                    if (data != null) return data;
                }
                catch { }
            }
            // 2) Environment variables
            var org = Environment.GetEnvironmentVariable("UGS_ORG_ID");
            var proj = Environment.GetEnvironmentVariable("UGS_PROJECT_ID");
            var name = Environment.GetEnvironmentVariable("UGS_PROJECT_NAME");
            if (!string.IsNullOrWhiteSpace(org) && !string.IsNullOrWhiteSpace(proj))
            {
                return new UGSLinkData { organizationId = org, projectId = proj, projectName = name };
            }
            return null;
        }

        private static bool ApplyLink(string orgId, string projectId, string projectName)
        {
            var ucType = Type.GetType("UnityEditor.Connect.UnityConnect, UnityEditor");
            if (ucType != null)
            {
                var instProp = ucType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
                var instance = instProp?.GetValue(null);
                var loggedInProp = ucType.GetProperty("loggedIn", BindingFlags.Public | BindingFlags.Instance);
                if (loggedInProp != null && instance != null)
                {
                    bool loggedIn = (bool)loggedInProp.GetValue(instance);
                    if (!loggedIn) { Debug.LogWarning("[XCAPE] Editor not logged in; cannot link UGS."); return false; }
                }
            }

            var cpsType = Type.GetType("UnityEditor.Connect.CloudProjectSettings, UnityEditor");
            if (cpsType == null) return false;

            try
            {
                var orgProp = cpsType.GetProperty("organizationId", BindingFlags.Public | BindingFlags.Static);
                if (orgProp != null && orgProp.CanWrite) orgProp.SetValue(null, orgId);
                else cpsType.GetMethod("SetOrganizationId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, new object[] { orgId });

                var projProp = cpsType.GetProperty("projectId", BindingFlags.Public | BindingFlags.Static);
                if (projProp != null && projProp.CanWrite) projProp.SetValue(null, projectId);
                else cpsType.GetMethod("SetProjectId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, new object[] { projectId });

                if (!string.IsNullOrEmpty(projectName))
                {
                    var nameProp = cpsType.GetProperty("projectName", BindingFlags.Public | BindingFlags.Static);
                    if (nameProp != null && nameProp.CanWrite) nameProp.SetValue(null, projectName);
                    else cpsType.GetMethod("SetProjectName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, new object[] { projectName });
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XCAPE] ApplyLink failed: {e.Message}");
                return false;
            }
        }
    }
}
#endif