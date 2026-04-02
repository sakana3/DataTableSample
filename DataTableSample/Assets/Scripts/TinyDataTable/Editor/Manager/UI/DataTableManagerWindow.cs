using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;

namespace TinyDataTable.Editor
{
    public class DataTableManagerWindow : EditorWindow
    {
        // メニューアイテムを追加 (Window > My Sample Window)
        [MenuItem("Window/TinyDataTable")]
        public static void ShowWindow()
        {
            // ウィンドウを表示 (既存ならフォーカス、なければ作成)
            GetWindow<DataTableManagerWindow>("TinyDataTable");
        }

        private DataTableManager dataTableManager;
        private SerializedObject serializedObject;
        
        /// <summary>
        /// Unityエディタでエディターウィンドウが有効化されたときに呼び出されるメソッド。
        /// 必要なリソースをロードし、シリアライズされたオブジェクトの初期化を行う。
        /// </summary>
        private void OnEnable()
        {
            dataTableManager = null;
            if (dataTableManager == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:DataTableManager");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids.First());
                    dataTableManager = AssetDatabase.LoadAssetAtPath<DataTableManager>(path);
                }
            }
            
            if (dataTableManager != null)
            {
                serializedObject = new SerializedObject(dataTableManager);
            }
        }
        
        public void CreateGUI()
        {
            rootVisualElement.Clear();
            // ルート要素
            var root = new VisualElement();
            root.style.flexGrow = 1;
            rootVisualElement.Add(root);
            
            if (dataTableManager == null)
            {
                var welcome = new DataTableManagerWelcome(dataTableManager);
                welcome.OnClickStart = CreateManager;
                root.Add(welcome);
            }
            else
            {
                var editor = new DataTableManagerEditor(dataTableManager);
                root.Add(editor);
            }
        }

        public void CreateManager(DataTableManager.DataType dataType,string rootPath, string nameSpace)
        {
            dataTableManager = ScriptableObject.CreateInstance<DataTableManager>();
            dataTableManager.dataType = dataType;
            dataTableManager.RootPath = rootPath;
            dataTableManager.DefaultNamespace = nameSpace;

            MakeDirectory(rootPath, "Editor");
            MakeDirectory(rootPath, "Resources");
            MakeDirectory(rootPath, "Scripts\\ID");
            
            UnityEditor.AssetDatabase.CreateAsset(dataTableManager, $"Assets/{rootPath}\\Editor\\TinyDataTableManager.asset");
            UnityEditor.AssetDatabase.SaveAssets();            
            
            CreateGUI();
        }


        private void MakeDirectory(string rootPath, string subPath)
        {
            var directory = $"Assets/{rootPath}\\{subPath}";
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            
                // Unity側にフォルダが作成されたことを認識させる
                AssetDatabase.Refresh();
            }
        }
    }
}