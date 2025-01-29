
using System;
using JahroConsole.Core.Data;
using JahroConsole.Core.Network;
using UnityEditor;
using UnityEngine;

namespace JahroConsole.Editor
{
    public class JahroSettingsProvider : EditorWindow
    {
        private Color _backgroundColor = new Color(0.055f, 0.055f, 0.063f);

        private JahroProjectSettings _projectSettings;

        private string[] _assembliesNames;

        private int _assembliesFlag;

        private Vector2 _scrollPosition;

        private bool _isLoading;

        private KeyValidator.ValidateKeyResponse _validation;

        private bool _keyInputDirty = true;

        [MenuItem("Tools/Jahro Settings")]
        public static void ShowWindow()
        {
            JahroSettingsProvider window = EditorWindow.GetWindow(typeof(JahroSettingsProvider), false, "Jahro Settings", true) as JahroSettingsProvider;
        }

        public void OnFocus()
        {
            _projectSettings = JahroProjectSettings.LoadOrCreate();

            if (_assembliesNames == null || _assembliesNames.Length == 0)
            {
                var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player);
                _assembliesNames = new string[assemblies.Length];
                for (int i = 0; i < assemblies.Length; i++)
                {
                    _assembliesNames[i] = assemblies[i].name;
                }
            }
            _assembliesFlag = 0;
            for (int i = 0; i < _assembliesNames.Length; i++)
            {
                if (_projectSettings.ActiveAssemblies.Contains(_assembliesNames[i]))
                {
                    _assembliesFlag |= 1 << i;
                }
            }
        }

        void GeneralSettingsGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.BeginVertical();

            GUILayout.Label("<color=#F24B16>General</color>", EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.BeginVertical();

            _projectSettings.JahroEnabled = EditorGUILayout.Toggle("Jahro Enabled", _projectSettings.JahroEnabled);
            EditorGUILayout.HelpBox("It is highly recommended to disable Jahro in release builds.", MessageType.Warning, false);
            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            _projectSettings.APIKey = EditorGUILayout.PasswordField("Project API Key:", _projectSettings.APIKey);
            if (EditorGUI.EndChangeCheck())
            {
                _validation = null;
                _keyInputDirty = true;
            }
            KeyValidationGUI();

            GUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.EndHorizontal();
        }

        void LauchSettingsGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.BeginVertical();

            GUILayout.Label("<color=#F24B16>Launch Options</color>", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.BeginVertical();

            _projectSettings.LaunchKey = (KeyCode)EditorGUILayout.EnumPopup("Launch key", _projectSettings.LaunchKey);
            EditorGUILayout.HelpBox("The launch key is used to open or hide the Jahro Console. Default: BackQuote", MessageType.None, false);
            EditorGUILayout.Separator();

            _projectSettings.UseLaunchKeyboardShortcut = EditorGUILayout.Toggle("Keyboard shortcuts", _projectSettings.UseLaunchKeyboardShortcut);
            EditorGUILayout.HelpBox("Shortkeys for Jahro Console Window:\n\n"
            + "Switch to Console Mode - press 'Alt+1' on keyboard\n"
            + "Switch to Visual Mode - press 'Alt+2' on keyboard\n"
            + "Switch to Watcher Mode - press 'Alt+3' on keyboard\n", MessageType.None, false);
            EditorGUILayout.Separator();

            _projectSettings.UseLaunchTapArea = EditorGUILayout.Toggle("Tap Area", _projectSettings.UseLaunchTapArea);
            EditorGUILayout.HelpBox("You can always use API method to show/hide console", MessageType.None, false);
            EditorGUILayout.Separator();

            _projectSettings.DuplicateToUnityConsole = EditorGUILayout.Toggle("Jahro Logs to Unity Console", _projectSettings.DuplicateToUnityConsole);
            EditorGUILayout.HelpBox("Duplicate Jahro Logs to Unity Console logs", MessageType.None, false);
            EditorGUILayout.Separator();

            GUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.EndHorizontal();
        }

        void AdvancedSettingsGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.BeginVertical();

            GUILayout.Label("<color=#F24B16>Advanced</color>", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.BeginVertical();

            if (_assembliesNames != null)
            {
                _assembliesFlag = EditorGUILayout.MaskField("Included assemblies", _assembliesFlag, _assembliesNames);
                EditorGUILayout.HelpBox("Please select assemblies which contain Jahro items like commands or watch variables, others can be disabled.", MessageType.None, false);
                for (int i = 0; i < _assembliesNames.Length; i++)
                {
                    string name = _assembliesNames[i];
                    bool active = _projectSettings.ActiveAssemblies.Contains(name);
                    int layer = 1 << i;
                    bool selected = (_assembliesFlag & layer) != 0;
                    if (!active && selected)
                    {
                        _projectSettings.ActiveAssemblies.Add(name);
                    }
                    else if (active && !selected)
                    {
                        _projectSettings.ActiveAssemblies.Remove(name);
                    }
                }
            }


            GUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.EndHorizontal();
        }


        void OnGUI()
        {
            // EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), _backgroundColor);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorStyles.boldLabel.richText = true;
            EditorStyles.boldLabel.fontSize = 16;
            EditorStyles.helpBox.contentOffset = new Vector2(0, 0);
            EditorStyles.label.fontSize = 13;
            EditorStyles.helpBox.padding = new RectOffset(8, 8, 8, 8);
            EditorStyles.label.richText = true;

            EditorGUILayout.Space(16);

            GeneralSettingsGUI();
            EditorGUILayout.Space(32);

            LauchSettingsGUI();
            EditorGUILayout.Space(32);

            AdvancedSettingsGUI();
            EditorGUILayout.Space(16);

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                SaveSettingsChanges();
            }

            if (_validation == null && _keyInputDirty)
            {
                ValidateAPIKey(_projectSettings.APIKey);
                _keyInputDirty = false;
            }
        }

        private void KeyValidationGUI()
        {
            if (string.IsNullOrEmpty(_projectSettings.APIKey))
            {
                EditorGUILayout.HelpBox("Project API Key is missing. Please enter a valid API Key.", MessageType.Error, false);

                EditorGUILayout.Separator();
                if (EditorGUILayout.LinkButton("Get Your API Key Here"))
                    Application.OpenURL("https://jahro.io");
            }
            else
            {
                if (_isLoading)
                {
                    EditorGUILayout.LabelField(" ", "<color=#FFA500><b>Loading...</b></color>");
                }
                else if (_validation != null)
                {
                    if (_validation.success)
                    {
                        EditorGUILayout.LabelField(" ", "<color=#1CB700><b>API Key is valid</b></color>");
                        EditorGUILayout.LabelField("Project Name", _validation.projectName);
                        EditorGUILayout.Separator();
                        if (EditorGUILayout.LinkButton("Open Project Page"))
                            Application.OpenURL(_validation.projectUrl);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(_validation.message, MessageType.Error, false);
                        if (GUILayout.Button("Retry", GUILayout.Width(140)))
                        {
                            _validation = null;
                            _keyInputDirty = true;
                            // ValidateAPIKey(_projectSettings.APIKey);
                        }
                        EditorGUILayout.Separator();
                        if (EditorGUILayout.LinkButton("Get Your API Key Here"))
                            Application.OpenURL("https://jahro.io");

                    }
                }
            }
        }

        private async void ValidateAPIKey(string key)
        {
            _isLoading = true;
            _validation = null;

            try
            {
                await KeyValidator.Send(key, (result) =>
                {
                    _validation = result;
                }, (error) =>
                {
                    _validation = error;
                });
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                _validation = new KeyValidator.ValidateKeyResponse() { success = false, message = ex.Message };
            }
            finally
            {
                _isLoading = false;
                Repaint();
            }
        }

        private void SaveSettingsChanges()
        {

            EditorUtility.SetDirty(_projectSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
