using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TinyDataTable.Editor
{
    public class TestWindow : EditorWindow
    {
        // メニューアイテムを追加 (Window > My Sample Window)
        [MenuItem("Window/Test")]
        public static void ShowWindow()
        {
            // ウィンドウを表示 (既存ならフォーカス、なければ作成)
            GetWindow<TestWindow>("Test");
        }

        private DataTableManager dataTableManager;
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
        }

        public void OnGUI()
        {
            var so = new SerializedObject(dataTableManager);
            
            EditorGUILayout.PropertyField(so.FindProperty("RootPath"));
        }

        public void _CreateGUI()
        {
            rootVisualElement.Clear();
            
            rootVisualElement.Add( new Label( "TestWindow"));

            var so = new SerializedObject(dataTableManager);

            rootVisualElement.Add(new PropertyField(so.FindProperty("RootPath")));
        }
    }
}