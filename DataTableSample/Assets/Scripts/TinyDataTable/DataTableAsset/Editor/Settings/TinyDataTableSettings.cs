using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TinyDataTable.Editor
{
    // ProjectSettingsフォルダに保存される設定クラス
    public class TinyDataTableSettings : ScriptableObject
    {
        // 保存先のパス (ProjectSettingsフォルダ内)
        private const string SettingsPath = "ProjectSettings/TinyDataTableSettings.asset";

        [SerializeField] private Color headerColor = Color.gray;
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private string defaultNamespace = "ID";
        [SerializeField] private string[] assemblies = new string[]
        {
            "Assembly-CSharp", "UnityEngine", "UnityEngine.CoreModule"
        };

        public Color HeaderColor => headerColor;
        public bool EnableAutoSave => enableAutoSave;
        public string DefaultNamespace => defaultNamespace;
        public string[] Assemblies => assemblies;

        private static TinyDataTableSettings _instance;

        public static TinyDataTableSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance<TinyDataTableSettings>();
                    
                    // ファイルが存在すれば読み込む
                    if (File.Exists(SettingsPath))
                    {
                        // InternalEditorUtilityを使って読み込む
                        // これにより、Assetsフォルダ外のYAML/Binary形式のScriptableObjectをロードできる
                        var objects = InternalEditorUtility.LoadSerializedFileAndForget(SettingsPath);
                        if (objects != null && objects.Length > 0)
                        {
                            // 読み込んだデータでインスタンスを上書きするのではなく、値をコピーするか
                            // ロードされたインスタンスを使う
                            _instance = objects[0] as TinyDataTableSettings;
                        }
                    }
                }
                return _instance;
            }
        }

        public void Save()
        {
            if (_instance == null) return;

            // ProjectSettingsフォルダに保存
            // SaveToSerializedFileAndForget は Assetsフォルダ外に保存するためのAPI
            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { _instance }, SettingsPath, true);
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(Instance);
        }
    }
}
