using UnityEngine;
using UnityEditor;
using System.IO;

namespace TheOneUnity
{
    public class TheOneSettings
    {
 		private static TheOneServerSettings theoneData;
        
        public static TheOneServerSettings TheOneData
        {
            get
            {
                if (theoneData == null)
                {
                    LoadOrCreateSettings();
                }

                return theoneData;
            }
            private set { theoneData = value; }
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
                // Force reload of the theoneData Settings.
                theoneData = null;    
            }
            else if (theoneData != null)
            {
                // TheOne Data setting have already been loaded.
                return;
            }

            // Try to load the resource / asset (TheOneServerSettings
            // a.k.a. TheOne Data Settings)
            theoneData = (TheOneServerSettings)Resources.Load(TheOneDataFilename, typeof(TheOneServerSettings));
            
            // If TheOne Data Setting were loaded successfully, all is well,
            // exit the method.
            if (theoneData != null)
            {
                return;
            }

#if UNITY_EDITOR
            // The TheOneServerSettings a.k.a TheOne Data Settings does not exist so create it.
            if (theoneData == null)
            {
                // Create a fresh instance of the TheOne Datat Setting sasset.
                theoneData = (TheOneServerSettings)TheOneServerSettings.CreateInstance("TheOneServerSettings");
                
                if (theoneData == null)
                {
                    Debug.LogError("Failed to create TheOneServerSettings. TheOne is unable to run this way. If you deleted it from the project, reload the Editor.");
                    return;
                }
            }

            string theoneResourcesDirectory = UnityFileHelper.FindTheOneAssetFolder() ;
            string serverSettingsAssetPath = theoneResourcesDirectory + TheOneDataFilename + ".asset";
            string serverSettingsDirectory = Path.GetDirectoryName(serverSettingsAssetPath);

            if (!Directory.Exists(serverSettingsDirectory))
            {
                Directory.CreateDirectory(serverSettingsDirectory);
                AssetDatabase.ImportAsset(serverSettingsDirectory);
            }

            if (!File.Exists(serverSettingsAssetPath))
            {
                AssetDatabase.CreateAsset(theoneData, serverSettingsAssetPath);
            }

            AssetDatabase.SaveAssets();
#endif
        }
    }
}