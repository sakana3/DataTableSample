using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace TinyDataTable.Editor
{
    [CustomEditor(typeof(DataTableAsset))]
    public class DataTableAssetInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var scriptProp = serializedObject.FindProperty("m_Script");
            var dataTableAsset = serializedObject.targetObject as DataTableAsset;

            //クラス情報
            if (dataTableAsset.ClassScript == null)
            {
                string path = AssetDatabase.GetAssetPath(serializedObject.targetObject);
                var classNameField = new TextField()
                {
                    value = FileNameToClassName(path),
                };
                var className = UIToolkitEditorUtility.CreateLabeledVisualElement("Class Name", classNameField);

                var namespaceField = new TextField()
                {
                    value = TinyDataTableSettings.Instance.DefaultNamespace,
                };
                var namespaceName = UIToolkitEditorUtility.CreateLabeledVisualElement("NameSpace", namespaceField);
                
                //Exportボタン
                var exportButton = new Button()
                {
                    text = CheckNeedEnsureAddressable(dataTableAsset)? "Ensure Addressable" :"Create ID",
                };
                exportButton.clicked += () =>
                {
                    serializedObject.Update();
                    if (CheckNeedEnsureAddressable(dataTableAsset))
                    {
                        EnsureAddressable(dataTableAsset);
                        exportButton.text = "Create ID";
                    }
                    else
                    {
                        if (dataTableAsset != null)
                        {
                            SaveDataTable.SaveScript(
                                dataTableAsset,
                                classNameField.value,
                                namespaceField.value,
                                TinyDataTableSettings.Instance.DefaultScriptPath);
                        }
                    }
                };
                root.Add(exportButton);

                root.Add(namespaceName.container);
                root.Add(className.container);
            }
            else
            {
                //Exportボタン
                var exportButton = new Button()
                {
                    text = "Export",
                };
                exportButton.clicked += () =>
                {
                    serializedObject.Update();

                    if (dataTableAsset != null)
                    {
                        SaveDataTable.SaveScript(
                            dataTableAsset,
                            string.Empty,
                            string.Empty,
                            "Assets/TinyDataTable/Script");
                    }
                };
                root.Add(exportButton);
                
                var typeNameField = new PropertyField(serializedObject.FindProperty("classType"));
                typeNameField.SetEnabled(false);
                root.Add(typeNameField);
                
                var classField = new PropertyField(serializedObject.FindProperty("classScript"));
                classField.SetEnabled(false);
                root.Add(classField);
            }

            /*
            var exportSettingProp = serializedObject.FindProperty("settings");
            if (exportSettingProp != null)
            {
                // カスタムグリッドフィールドを表示
                var propertyField = new PropertyField(exportSettingProp);
                propertyField.style.flexGrow = 1;
                propertyField.style.marginTop = 10;
                root.Add(propertyField);
            }
*/
            // Tagフィールド
            root.Add(new Label("Tags"));            
            var tagField = MakeTagField();
            root.Add(tagField);
            
            // DataTableフィールドの取得
            var dataTableProp = serializedObject.FindProperty("dataTable");
            if (dataTableProp != null)
            {
                // カスタムグリッドフィールドを表示
                var propertyField = new PropertyField(dataTableProp,"DataTable");
                root.Add(propertyField);

                var button = new Button(){text = "Export"};
                button.clicked += () =>
                {
                    GUIUtility.systemCopyBuffer = ExportDataTableToCSharp.MakeRecordScript(32);
                };
                root.Add(button);
            }


            // DataTableフィールドの取得
            var dataProp = serializedObject.FindProperty("data");
            if (dataProp != null)
            {
                // カスタムグリッドフィールドを表示
                var gridField = new DataTableGridField(dataProp,"record");
                gridField.style.flexGrow = 1;
                gridField.style.marginTop = 10;
                root.Add(gridField);
            }

            
            return root;
        }

        VisualElement MakeTagField()
        {
            void setStyle( VisualElement element)
            {
                var toggle = new ToolbarToggle()
                {
                    text = string.IsNullOrEmpty(name) ? "String.Empty" : name,
                };
                element.style.borderLeftWidth = 1.0f;
                element.style.borderRightWidth = 1.0f;
                element.style.borderBottomWidth = 1.0f;
                element.style.borderTopWidth = 1.0f;
                element.style.borderTopLeftRadius = 10;
                element.style.borderTopRightRadius = 10;
                element.style.borderBottomLeftRadius = 10;
                element.style.borderBottomRightRadius = 10;
                element.style.paddingLeft = 2;
                element.style.paddingRight = 2;

            }

            void refreshToggle( Toggle toggle , string tag )
            {
                var tagProp = this.serializedObject.FindProperty("tags");
                for (int i = 0; i < tagProp.arraySize; i++)
                {
                    if (tagProp.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        toggle.style.backgroundColor = Color.darkCyan; // オン色
                        if(toggle.value == false) toggle.value = true;
                        return;
                    }
                } 
                if(toggle.value == true) toggle.value = false;
                toggle.style.backgroundColor = StyleKeyword.Null; // デフォルトに戻す
            }


            void MakeTages(VisualElement baseField)
            {
                serializedObject.Update();
                var target = serializedObject.targetObject as DataTableAsset;
                var tags = TinyDataTableSettings.Instance.Tags
                    .Where(t => string.IsNullOrEmpty(t) is false &&
                                t.Contains("\"") is false &&
                                t.Contains("\\") is false &&
                                t.Contains("'") is false )
                    .Concat(target.Tags)
                    .Distinct()
                    .ToArray();


                foreach (var tag in tags)
                {
                    var toggle = new ToolbarToggle()
                    {
                        text = string.IsNullOrEmpty(tag) ? "String.Empty" : tag,
                    };
                    setStyle(toggle);
                    refreshToggle(toggle, tag);
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        this.serializedObject.Update();
                        var tagProp = this.serializedObject.FindProperty("tags");
                        if (evt.newValue)
                        {
                            var hasTag = Enumerable.Range(0, tagProp.arraySize)
                                .Any(i => tagProp.GetArrayElementAtIndex(i).stringValue == tag);
                            if (hasTag is false)
                            {
                                var i = tagProp.arraySize;
                                tagProp.InsertArrayElementAtIndex(i);
                                tagProp.GetArrayElementAtIndex(i).stringValue = tag;
                                this.serializedObject.ApplyModifiedProperties();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < tagProp.arraySize; i++)
                            {
                                if (tagProp.GetArrayElementAtIndex(i).stringValue == tag)
                                {
                                    tagProp.DeleteArrayElementAtIndex(i);
                                    this.serializedObject.ApplyModifiedProperties();
                                    break;
                                }
                            }
                        }

                    });
                    
                    // カーソルが離れた時 (MouseLeave)
                    toggle.RegisterCallback<PointerLeaveEvent>(evt =>
                    {
//                        Debug.Log("カーソルが離れました (Left)");
                    });

                    // 押して離した時 (MouseUp)
                    toggle.RegisterCallback<PointerUpEvent>(evt =>
                    {
//                        Debug.Log("離されました (Released)");
                    });                    

                    toggle.TrackSerializedObjectValue(this.serializedObject, (prop) => { refreshToggle(toggle, tag); });


                    baseField.Add(toggle);
                }


                var button = new Button() { text = "+" };
                button.style.width = 18;
                button.style.height = 18;

                setStyle(button);
                button.clicked += () =>
                {
                    baseField.Remove(button);
                    var textField = new TextField();
                    textField.isDelayed = true;
                    setStyle(textField);
                    textField.style.minWidth = 80;
                    textField.RegisterValueChangedCallback(evt =>
                    {
                        if (tags.Contains(evt.newValue) is false &&
                            evt.newValue.Contains("\"") is false &&
                            evt.newValue.Contains("\\") is false &&
                            evt.newValue.Contains("'") is false )
                        {
                            // 設定オブジェクトを取得
                            var serializedSettings = TinyDataTableSettings.GetSerializedSettings();
                            var settingTagProp = serializedSettings.FindProperty("tags");
                            settingTagProp.InsertArrayElementAtIndex(settingTagProp.arraySize);
                            settingTagProp.GetArrayElementAtIndex(settingTagProp.arraySize - 1).stringValue =
                                evt.newValue;
                            serializedSettings.ApplyModifiedProperties();
                            var setting = serializedSettings.targetObject as TinyDataTableSettings;
                            if (setting != null)
                            {
                                setting.Save();
                            }

                            var tagProp = this.serializedObject.FindProperty("tags");
                            tagProp.InsertArrayElementAtIndex(tagProp.arraySize);
                            tagProp.GetArrayElementAtIndex(tagProp.arraySize - 1).stringValue = evt.newValue;
                            serializedObject.ApplyModifiedProperties();

                            baseField.Clear();
                            MakeTages(baseField);
                        }
                    });

                    baseField.Add(textField);
                };
                baseField.Add(button);
            }

            var tagField = new VisualElement(){ name = "Tags" };
            tagField.style.flexDirection = FlexDirection.Row;
            tagField.style.flexWrap = Wrap.Wrap;
            MakeTages(tagField);            
            
            return tagField;
        }
        
        /// <summary>
        /// ファイル名をクラス名に使える文字列に変換する
        /// 1._以外の記号を_に置き換える
        /// 2.先頭が数字なら_をつける
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string FileNameToClassName(string path)
        {
            string fileNameNoExt = Path.GetFileNameWithoutExtension(path);

            if (string.IsNullOrEmpty(fileNameNoExt)) return "_";            

            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < fileNameNoExt.Length; i++)
            {
                char c = fileNameNoExt[i];
        
                // 文字、数字、アンダースコアならOK
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
                else
                {
                    // それ以外 (記号、スペースなど) は _ に
                    sb.Append('_');
                }
            }

            string result = sb.ToString();

            // 先頭が数字なら _ を付与
            if (result.Length > 0 && char.IsDigit(result[0]))
            {
                result = "_" + result;
            }            
            
            return result;
        }

        public static bool CheckNeedEnsureAddressable(UnityEngine.Object asset)
        {
            if (asset == null) return false;

            //Resources以下にあるならは登録しない
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (assetPath.Contains("/Resources/"))
            {
                return false;
            }
            
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return false;

            string path = AssetDatabase.GetAssetPath(asset);
            string guid = AssetDatabase.AssetPathToGUID(path);

            // エントリを検索
            var entry = settings.FindAssetEntry(guid);            
            
            return entry == null;
        }


        public static void EnsureAddressable(UnityEngine.Object asset)
        {
           if (asset == null) return;

            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            string path = AssetDatabase.GetAssetPath(asset);
            string guid = AssetDatabase.AssetPathToGUID(path);

            // エントリを検索
            var entry = settings.FindAssetEntry(guid);

            // まだ登録されていなければ登録
            if (entry == null)
            {
                // デフォルトグループに登録
                entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            
                // アドレスを設定
                entry.SetAddress(path);
            
                EditorUtility.SetDirty(settings);
            }
        }        
    }
}
