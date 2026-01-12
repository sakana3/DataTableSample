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
                // プロパティフィールドの作成
                var headerColorField = new PropertyField(_serializedSettings.FindProperty("headerColor"));
                var autoSaveField = new PropertyField(_serializedSettings.FindProperty("enableAutoSave"));
                var defaultNamespaceField = new PropertyField(_serializedSettings.FindProperty("defaultNamespace"));
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

                // 変更検知と保存
                // PropertyFieldはバインドされているので値は自動更新されるが、
                // ファイルへの書き出し(Save)は明示的に行う必要がある
                headerColorField.RegisterValueChangeCallback(evt => SaveSettings());
                autoSaveField.RegisterValueChangeCallback(evt => SaveSettings());
                defaultNamespaceField.RegisterValueChangeCallback(evt => SaveSettings());

                // バインド
                headerColorField.Bind(_serializedSettings);
                autoSaveField.Bind(_serializedSettings);
                defaultNamespaceField.Bind(_serializedSettings);
                assembliesList.Bind(_serializedSettings);

                // UIに追加
                var container = new VisualElement();
                container.style.marginLeft = 10;
//                container.Add(headerColorField);
//                container.Add(autoSaveField);
                container.Add(defaultNamespaceField);
                container.Add(assembliesList);
                
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
