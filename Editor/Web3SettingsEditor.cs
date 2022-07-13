
#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using TheOneUnity.Sdk.Constants;

namespace TheOneUnity.Editor
{
    public class Web3SettingsEditor : EditorWindow
    {
        private static string[] pages = { "Page_1", "Page_2" };
        
        private VisualElement rootElement;
        private bool windowDrawn = false;

        public bool isSetupWizard = true;

        protected static Type WindowType = typeof(Web3SettingsEditor);



        /// <summary>
        /// Menu show event - displays the setup window when menu selection made.
        /// </summary>
        [MenuItem( TheOneConstants.PathTheOneWindowMenu + "/" + TheOneConstants.Open + " " + "Web3 Request Settings", false, 15 )]
        public static void ShowWindow()
        {
            var window = GetWindow<Web3SettingsEditor>();

            window.isSetupWizard = false;
            window.titleContent = new GUIContent("Web3 Request Settings");
            window.minSize = new Vector2(750, 510);
            window.maxSize = new Vector2(750, 510);

            window.Show();
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
                string moralisEditorwindowPath = UnityFileHelper.FindTheOneEditorFolder();

                if (TheOneSettings.TheOneData == null)
                {
                    // Just in case moralisData has not been loaded, handle it here.
                    TheOneSettings.LoadOrCreateSettings();
                }

                rootElement = rootVisualElement;
                
                bool mdLoaded = TheOneSettings.TheOneData != null;

                // Loads the page definition.
                VisualTreeAsset original = AssetDatabase
                    .LoadAssetAtPath<VisualTreeAsset>(moralisEditorwindowPath + "Web3SettingsEditorWindow.uxml");

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
                    .LoadAssetAtPath<StyleSheet>(moralisEditorwindowPath + "TheOneWeb3SdkEditorStyles.uss");
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
                #endregion

                #region TextField Values Setup
                var DappNameField = rootVisualElement.Q<TextField>("DappNameField");
                DappNameField.SetValueWithoutNotify(TheOneSettings.TheOneData.DappName);
                DappNameField.RegisterValueChangedCallback(evt =>
                {
                    TheOneSettings.TheOneData.DappName = evt.newValue;
                    SaveSettings();
                });

                var DappDescriptionField = rootVisualElement.Q<TextField>("DappDescriptionField");
                DappDescriptionField.SetValueWithoutNotify(TheOneSettings.TheOneData.DappDescription);
                DappDescriptionField.RegisterValueChangedCallback(evt =>
                {
                    TheOneSettings.TheOneData.DappDescription = evt.newValue;
                    SaveSettings();
                });


                var DappVersionField = rootVisualElement.Q<TextField>("DappVersionField");
                DappVersionField.SetValueWithoutNotify(TheOneSettings.TheOneData.DappVersion);
                DappVersionField.RegisterValueChangedCallback(evt =>
                {
                    TheOneSettings.TheOneData.DappVersion = evt.newValue;
                    SaveSettings();
                });

                var DappWebsiteUrlField = rootVisualElement.Q<TextField>("DappWebsiteUrlField");
                DappWebsiteUrlField.SetValueWithoutNotify(TheOneSettings.TheOneData.DappWebsiteUrl);
                DappWebsiteUrlField.RegisterValueChangedCallback(evt =>
                {
                    TheOneSettings.TheOneData.DappWebsiteUrl = evt.newValue;
                    SaveSettings();
                });

                var DappIconUrlField = rootVisualElement.Q<TextField>("DappIconUrlField");
                DappIconUrlField.SetValueWithoutNotify(TheOneSettings.TheOneData.DappIconUrl);
                DappIconUrlField.RegisterValueChangedCallback(evt =>
                {
                    TheOneSettings.TheOneData.DappIconUrl = evt.newValue;
                    SaveSettings();
                });
                #endregion

            }
        }
    }
}
#endif