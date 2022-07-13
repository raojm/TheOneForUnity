using UnityEngine;
using UnityEditor;
using System.IO;

namespace TheOneUnity
{
    public class TheOneSettings
    {
 		private static TheOneServerSettings moralisData;
        
        public static TheOneServerSettings TheOneData
        {
            get
            {
                if (moralisData == null)
                {
                    LoadOrCreateSettings();
                }

                return moralisData;
            }
            private set { moralisData = value; }
        }

        private static string TheOneDataFilename = "TheOneServerSettings";

        /// <summary>
        /// Loads the TheOne Data Setting sasset. If it does not exist, create it.
        /// </summary>
        /// <param name="reload"></param>
        public static void LoadOrCreateSettings(bool reload = false)
        {
            if (reload)
            {
                // Force reload of the moralisData Settings.
                moralisData = null;    
            }
            else if (moralisData != null)
            {
                // TheOne Data setting have already been loaded.
                return;
            }

            // Try to load the resource / asset (TheOneServerSettings
            // a.k.a. TheOne Data Settings)
            moralisData = (TheOneServerSettings)Resources.Load(TheOneDataFilename, typeof(TheOneServerSettings));
            
            // If TheOne Data Setting were loaded successfully, all is well,
            // exit the method.
            if (moralisData != null)
            {
                return;
            }

#if UNITY_EDITOR
            // The TheOneServerSettings a.k.a TheOne Data Settings does not exist so create it.
            if (moralisData == null)
            {
                // Create a fresh instance of the TheOne Datat Setting sasset.
                moralisData = (TheOneServerSettings)TheOneServerSettings.CreateInstance("TheOneServerSettings");
                
                if (moralisData == null)
                {
                    Debug.LogError("Failed to create TheOneServerSettings. TheOne is unable to run this way. If you deleted it from the project, reload the Editor.");
                    return;
                }
            }

            string moralisResourcesDirectory = UnityFileHelper.FindTheOneAssetFolder() ;
            string serverSettingsAssetPath = moralisResourcesDirectory + TheOneDataFilename + ".asset";
            string serverSettingsDirectory = Path.GetDirectoryName(serverSettingsAssetPath);

            if (!Directory.Exists(serverSettingsDirectory))
            {
                Directory.CreateDirectory(serverSettingsDirectory);
                AssetDatabase.ImportAsset(serverSettingsDirectory);
            }

            if (!File.Exists(serverSettingsAssetPath))
            {
                AssetDatabase.CreateAsset(moralisData, serverSettingsAssetPath);
            }

            AssetDatabase.SaveAssets();
#endif
        }
    }
}