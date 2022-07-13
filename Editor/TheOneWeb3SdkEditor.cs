#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using TheOneUnity.Sdk.Constants;

namespace TheOneUnity.Editor
{
    public class TheOneWeb3SdkEditor : EditorWindow
    {
        private VisualElement rootElement;
        private bool windowDrawn = false;

        public bool isSetupWizard = true;

        protected static Type WindowType = typeof(TheOneWeb3SdkEditor);

        /// <summary>
        /// Evnt handled when the package is installed or the project re-opened.
        /// </summary>
        [UnityEditor.InitializeOnLoadMethod]
        public static void InitializeOnLoadMethod()
        {
            EditorApplication.delayCall += OnDelayCall;
        }

        /// <summary>
        /// Menu show event - displays the setup window when menu selection made.
        /// </summary>
        [MenuItem( TheOneConstants.PathTheOneWindowMenu + "/" + TheOneConstants.Open + " " + "Web3 Setup", false, 15 )]
        public static void ShowWindow()
        {
            var window = GetWindow<TheOneWeb3SdkEditor>();

            window.isSetupWizard = false;
            window.titleContent = new GUIContent("Unity Web3 SDK");
            window.minSize = new Vector2(750, 500);
            window.maxSize = new Vector2(750, 500);
        }

        /// <summary>
        /// Launches the Setupwizard
        /// </summary>
        protected static void ShowSetupWizard()
        {
            TheOneWeb3SdkEditor win = GetWindow(WindowType, false, "TheOne Unity Web3 SDK", true) as TheOneWeb3SdkEditor;
            if (win == null)
            {
                return;
            }

            win.isSetupWizard = true;
            win.minSize = new Vector2(750, 500);
            win.maxSize = new Vector2(750, 500);
            win.Show();
        }

        // Called after OnLoad and displays the setup wizard.
        private static void OnDelayCall()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            
            if (TheOneSettings.TheOneData == null)
            {
                // Load or (when first run) create the settings scriptable object.
                TheOneSettings.LoadOrCreateSettings(true);
            }

            // If something horrible happened and theone data settings were not
            // loaded, do not show the wizard.
            if (TheOneSettings.TheOneData == null)
            {
                return;
            }

            // Only show the wizard if it is not disabled and the information has
            // not already been filled in.
            if (!TheOneSettings.TheOneData.DisableAutoOpenWizard && 
                (TheOneSettings.TheOneData.DappId.Equals(String.Empty) ||
                 TheOneSettings.TheOneData.DappUrl.Equals(String.Empty)))
            {
                ShowSetupWizard();
            }
        }

        // Marks settings object as dirty, so it gets saved.
        // unity 5.3 changes the usecase for SetDirty(). but here we don't modify a scene object! so it's ok to use
        private static void SaveSettings()
        {
            EditorUtility.SetDirty(TheOneSettings.TheOneData);
        }

        /// <summary>
        /// Handles the on draw event for the editor window.
        /// </summary>
        protected virtual void OnGUI()
        {
            if (!windowDrawn)
            {
                // Only draw once
                windowDrawn = true;
                string theoneEditorwindowPath = UnityFileHelper.FindTheOneEditorFolder();

                if (TheOneSettings.TheOneData == null)
                {
                    // Just in case theoneData has not been loaded, handle it here.
                    TheOneSettings.LoadOrCreateSettings();
                }

                rootElement = rootVisualElement;

                bool mdLoaded = TheOneSettings.TheOneData != null;

                // Loads the page definition.
                VisualTreeAsset original = AssetDatabase
                    .LoadAssetAtPath<VisualTreeAsset>(theoneEditorwindowPath + "TheOneWeb3SdkEditorWindow.uxml");

                // If page not found, close and exit window
                if (original == null)
                {
                    this.Close();
                    return;
                }

                TemplateContainer treeAsset = original.CloneTree();
                // Add page definition to the root element.
                rootVisualElement.Add(treeAsset);

                // Load stylsheet
                StyleSheet styleSheet = AssetDatabase
                    .LoadAssetAtPath<StyleSheet>(theoneEditorwindowPath + "TheOneWeb3SdkEditorStyles.uss");
                // Apply stylesheet root element.
                rootVisualElement.styleSheets.Add(styleSheet);

                #region Page Button Setup
                // Add action to Save button
                var doneButton = rootVisualElement.Q<Button>("DoneButton");
                doneButton.RegisterCallback<MouseUpEvent>((evt) =>
                {
                    windowDrawn = false;
                    this.Close();
                });
                // Since we start on the first page, back button should start hidden.
                doneButton.style.display = DisplayStyle.None;
                rootElement.Q<Button>("DoneButton").style.display = DisplayStyle.Flex;
                #endregion

                #region TextField Values Setup
                var DappUrlField = rootVisualElement.Q<TextField>("DappUrlField");
                DappUrlField.SetValueWithoutNotify(TheOneSettings.TheOneData.DappUrl);
                DappUrlField.RegisterValueChangedCallback(evt =>
                {
                    TheOneSettings.TheOneData.DappUrl = evt.newValue;
                    SaveSettings();
                });

                var DappIdField = rootVisualElement.Q<TextField>("DappIdField");
                DappIdField.SetValueWithoutNotify(TheOneSettings.TheOneData.DappId);
                DappIdField.RegisterValueChangedCallback(evt =>
                {
                    TheOneSettings.TheOneData.DappId = evt.newValue;
                    SaveSettings();
                });
                #endregion
            }
        }
        
        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (TheOneSettings.TheOneData.DappUrl.Equals(String.Empty) || TheOneSettings.TheOneData.DappId.Equals(String.Empty))
            {
                EditorUtility.DisplayDialog("Warning", "You have not yet completed the TheOne setup wizard. Your game won't be able to connect. Click Okay to open the wizard.", "Okay");
            }
        }
    }
}
#endif
