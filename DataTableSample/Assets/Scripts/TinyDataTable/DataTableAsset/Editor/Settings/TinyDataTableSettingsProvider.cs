using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TinyDataTable.Editor
{
    public class TinyDataTableSettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedSettings;

        public TinyDataTableSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _serializedSettings = TinyDataTableSettings.GetSerializedSettings();
            CreateGUI(searchContext,rootElement);
        }

        public void CreateGUI(string searchContext, VisualElement rootElement)
        {
            // 設定オブジェクトを取得
            _serializedSettings = TinyDataTableSettings.GetSerializedSettings();
            
            // タイトル
            var title = new Label("TinyDataTable Settings");
            title.style.fontSize = 19;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 10;
            title.style.marginLeft = 5;
            title.style.marginTop = 5;
            rootElement.Add(title);


            if (_serializedSettings != null && _serializedSettings.targetObject != null)
            {
                var container = new VisualElement();
                container.style.marginLeft = 10;                
                
                // プロパティフィールドの作成
                
                var defaultAssetPathField = new PropertyField(_serializedSettings.FindProperty("defaultAssetPath"));
                defaultAssetPathField.RegisterValueChangeCallback(evt => SaveSettings());
                defaultAssetPathField.Bind(_serializedSettings);
                container.Add(defaultAssetPathField);

                var defaultResourcePathField = new PropertyField(_serializedSettings.FindProperty("defaultResourcePath"));
                defaultResourcePathField.RegisterValueChangeCallback(evt => SaveSettings());
                defaultResourcePathField.Bind(_serializedSettings);
                container.Add(defaultResourcePathField);

                var defaultScriptPathField = new PropertyField(_serializedSettings.FindProperty("defaultScriptPath"));
                defaultScriptPathField.RegisterValueChangeCallback(evt => SaveSettings());
                defaultScriptPathField.Bind(_serializedSettings);
                container.Add(defaultScriptPathField);
                
                var defaultNamespaceField = new PropertyField(_serializedSettings.FindProperty("defaultNamespace"));
                defaultNamespaceField.RegisterValueChangeCallback(evt => SaveSettings());
                defaultNamespaceField.Bind(_serializedSettings);
                container.Add(defaultNamespaceField);
                
                var assembliesProp = _serializedSettings.FindProperty("assemblies");
                var allTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                    .Select(e => e.GetName().Name)
                    .ToList();
                var assembliesList = new ListView()
                {
                    makeItem = () => new PopupField<string>(allTypes,0),
                    bindItem = (e, i) =>
                    {
                        if (e is PopupField<string> popup)
                        {
                            popup.index = allTypes.IndexOf(assembliesProp.GetArrayElementAtIndex(i).stringValue);
                            popup.RegisterValueChangedCallback((evt) =>
                                {
                                    assembliesProp.GetArrayElementAtIndex(i).stringValue = evt.newValue;
                                    SaveSettings();
                                }
                            );
                            popup.Bind(_serializedSettings);
                        }
                        
                    },
                    showFoldoutHeader = true,
                    headerTitle = "Assemblies",
                    reorderable = true,
                    showAddRemoveFooter = true,
                };
                assembliesList.bindingPath = "assemblies";
                assembliesList.itemsAdded += _ => SaveSettings();
                assembliesList.itemsChosen += _ => SaveSettings();
                assembliesList.itemIndexChanged += (_,_) => SaveSettings();
                assembliesList.Bind(_serializedSettings);
                container.Add(assembliesList);
                
                var tagsProp = _serializedSettings.FindProperty("tags");
                var tagField = new PropertyField(tagsProp);
                tagField.RegisterValueChangeCallback(evt => SaveSettings());
                tagField.Bind(_serializedSettings);
                container.Add(tagField);

                container.TrackSerializedObjectValue(_serializedSettings, (prop) => { SaveSettings(); });
                
                rootElement.Add(container);
            }
        }
        

        private void SaveSettings()
        {
            if (_serializedSettings != null && _serializedSettings.targetObject != null)
            {
                _serializedSettings.ApplyModifiedProperties();
                var settings = _serializedSettings.targetObject as TinyDataTableSettings;
                settings?.Save();
            }
        }        


        [SettingsProvider]
        public static SettingsProvider CreateTinyDataTableSettingsProvider()
        {
            var provider = new TinyDataTableSettingsProvider("Project/TinyDataTable", SettingsScope.Project)
            {
                label = "Tiny DataTable",
                keywords = new HashSet<string>(new[] { "DataTable", "TinyDataTable" })
            };

            return provider;
        }
    }
}
