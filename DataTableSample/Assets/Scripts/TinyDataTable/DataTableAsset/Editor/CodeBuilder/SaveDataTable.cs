using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Compilation;
    
namespace TinyDataTable.Editor
{
    
    public static class SaveDataTable
    {
        
        private const string KeyIsGenerating = "TinyDataTable_IsGenerating";
        private const string CompilError = "TinyDataTable_CompilError";
        private const string ScriptFilePath = "TinyDataTableScript_FilePath";        
        private const string AssetFilePath = "TinyDataTableAsset_FilePath";        

        public static void SaveScript(
            DataTableAsset dataTableAsset ,
            string tmpClassName ,
            string tmpNamespace,
            string filePath )
        {
            var scriptName = String.Empty;
            var fullPath = String.Empty;
            var namespaceName = string.Empty;
            var assetpath = AssetDatabase.GetAssetPath(dataTableAsset);
            string address = null;
            if (dataTableAsset.Settings.classScript != null)
            {
                var script = dataTableAsset.Settings.classScript;
                scriptName = script.GetClass().Name;
                namespaceName = script.GetClass().Namespace;
                fullPath = AssetDatabase.GetAssetPath(script);
                address = GetAddressFromObject(dataTableAsset);                       
            }
            else
            {
                scriptName = tmpClassName;
                namespaceName = tmpNamespace;
                var fileName = "{className}.cs";
                fullPath = Path.Combine(filePath, fileName);
                address = GetAddressFromObject(dataTableAsset);                
            }

//            Debug.Log($"Exporting {assetpath} -> {fullPath} {scriptName} {namespaceName} {address}");

            var text = TinyDataTable.Editor.ExportDataTableToCSharp.Export(
                dataTableAsset,
                scriptName,
                namespaceName,
                address ?? assetpath
                );

            SaveScript(fullPath, text);
            

            // アセットデータベースを更新してUnityに認識させる
            AssetDatabase.Refresh();
            // セッションにデータを保存
            SessionState.SetBool(KeyIsGenerating, true);
            SessionState.SetString(ScriptFilePath, fullPath);
            SessionState.SetString(AssetFilePath, assetpath);
            //コンパイラーが走ってないなら直接呼び出す
            if (EditorApplication.isCompiling is false)
            {
                OnCompileFinished();
            }
            else
            {
                CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;                
            }
        }

        private static void OnCompilationFinished(string assemblyPath, UnityEditor.Compilation.CompilerMessage[] messages)
        {
            // エラーメッセージが含まれているかチェック
            bool hasErrors = messages.Any(m => m.type == CompilerMessageType.Error);

            SessionState.SetBool(CompilError, hasErrors);                
            CompilationPipeline.assemblyCompilationFinished -= OnCompilationFinished;            
        }
        
        [InitializeOnLoadMethod]
        private static void OnCompileFinished()
        {
            if (!SessionState.GetBool(KeyIsGenerating, false)) return;
            
            string scriptPath = SessionState.GetString(ScriptFilePath, string.Empty);
            string assetPath = SessionState.GetString(AssetFilePath, string.Empty);

            SessionState.EraseBool(KeyIsGenerating);
            SessionState.EraseBool(CompilError);
            SessionState.EraseString(ScriptFilePath);
            SessionState.EraseString(AssetFilePath);            
            
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            DataTableAsset asset = AssetDatabase.LoadAssetAtPath<DataTableAsset>(assetPath);
            if (script != null && asset != null)
            {
                asset.Settings.classScript = script;
                asset.Settings.classType = script.GetClass().FullName;
                var serializedObject =  new SerializedObject(asset);
                
                EditorUtility.SetDirty(asset);
                serializedObject.Update();
            }
            else
            {
                Debug.LogError("Failed to load asset.");
            }
        }        
        
        private static string GetAddressFromObject(UnityEngine.Object obj)
        {
            if (obj == null) return null;

            string path = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);

            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return null;
            
            var entry = settings.FindAssetEntry(guid);
            if (entry != null)
            {
                return entry.address; // アドレスを返す
            }

            return null; // Addressableではない
        }
               
        /// <summary>
        /// Resources以下のパスを取得する。Resources以下にないならnull
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetResourcePath(UnityEngine.Object obj)
        {
            if (obj == null) return null;

            // アセットのパスを取得 (例: "Assets/MyGame/Resources/Characters/Hero.prefab")
            string fullPath = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(fullPath)) return null;

            // "Resources/" が含まれているかチェック
            int index = fullPath.LastIndexOf("/Resources/");
        
            if (index >= 0)
            {
                // "Resources/" の後ろを切り出す
                string path = fullPath.Substring(index + "/Resources/".Length);

                // 拡張子を削除
                int extIndex = path.LastIndexOf('.');
                if (extIndex >= 0)
                {
                    path = path.Substring(0, extIndex);
                }

                return path;
            }

            return null;
        }


        
        public static void SaveScript(string fullPath, string content)
        {
//            var fileName = Path.GetFileName(fullPath);
            var filePath = Path.GetDirectoryName(fullPath);            
            
            if (!AssetDatabase.IsValidFolder(filePath))
            {
                CreateFolderRecursively(filePath);
            }

            // ファイル書き込み
            File.WriteAllText(fullPath, content);

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