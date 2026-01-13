using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System;

namespace TinyDataTable.Editor
{
    [CustomEditor(typeof(DataTableAsset))]
    public class DataTableAssetInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            var scriptProp = serializedObject.FindProperty("m_Script");
            
/*            
            var scriptField = new PropertyField(scriptProp);
            scriptField.SetEnabled(false);
            root.Add(scriptField);
*/
            var exportButton = new Button()
            {
                text = "Export",
            };
            exportButton.clicked += () =>
            {
                serializedObject.Update();
                var dataTableAsset = serializedObject.targetObject as DataTableAsset;
                if (dataTableAsset != null)
                {
                    Export(dataTableAsset);
                }
            };
            root.Add(exportButton);

            var exportSettingProp = serializedObject.FindProperty("settings");
            if (exportSettingProp != null)
            {
                // カスタムグリッドフィールドを表示
                var propertyField = new PropertyField(exportSettingProp);
                propertyField.style.flexGrow = 1;
                propertyField.style.marginTop = 10;
                root.Add(propertyField);
            }
            
            // DataTableフィールドの取得
            var dataProp = serializedObject.FindProperty("data");
            if (dataProp != null)
            {
                // カスタムグリッドフィールドを表示
                var gridField = new DataTableGridField(dataProp);
                gridField.style.flexGrow = 1;
                gridField.style.marginTop = 10;
                root.Add(gridField);
            }

            return root;
        }
        
        private const string KeyIsGenerating = "TinyDataTable_IsGenerating";
        private const string ScriptFilePath = "TinyDataTableScript_FilePath";        
        private const string AssetFilePath = "TinyDataTableAsset_FilePath";        

        private void Export(DataTableAsset dataTableAsset)
        {
            var text = TinyDataTable.Editor.ExportDataTableToCSharp.Export(dataTableAsset.Data,dataTableAsset.Settings,"TinyDataTable/DataTableAsset.asset");

            var path = SaveScript("Assets/TinyDataTable/Script", $"{dataTableAsset.Settings.className}.cs", text);
            
            SessionState.SetBool(KeyIsGenerating, true);
            SessionState.SetString(ScriptFilePath, path);
            SessionState.SetString(AssetFilePath, AssetDatabase.GetAssetPath(dataTableAsset));

            // アセットデータベースを更新してUnityに認識させる
            AssetDatabase.Refresh();
        }

        [InitializeOnLoadMethod]
        private static void OnCompileFinished()
        {
            // 4. 続きの処理が必要かチェック
            if (!SessionState.GetBool(KeyIsGenerating, false)) return;
            
            string scriptPath = SessionState.GetString(ScriptFilePath, string.Empty);
            string assetPath = SessionState.GetString(AssetFilePath, string.Empty);

            SessionState.EraseBool(KeyIsGenerating);
            SessionState.EraseString(ScriptFilePath);
            SessionState.EraseString(AssetFilePath);            
            
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            DataTableAsset asset = AssetDatabase.LoadAssetAtPath<DataTableAsset>(assetPath);
            if (script != null && asset != null)
            {
                asset.Settings.classScript = script;
                var serializedObject =  new SerializedObject(asset);
                serializedObject.Update();
                EditorUtility.SetDirty(asset);
            }
            else
            {
                Debug.LogError("Failed to load asset.");
            }
        }


        public static string SaveScript(string folderPath,string fileName, string content)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                CreateFolderRecursively(folderPath);
            }

            // ファイルパスの結合
            string filePath = Path.Combine(folderPath, fileName);

            // ファイル書き込み
            File.WriteAllText(filePath, content);
            
            return filePath;
        }

        // フォルダを再帰的に作成するヘルパー
        private static void CreateFolderRecursively(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                CreateFolderRecursively(parent);
            }

            string parentFolder = Path.GetDirectoryName(path);
            string newFolder = Path.GetFileName(path);
    
            AssetDatabase.CreateFolder(parentFolder, newFolder);
        }
    }
}
