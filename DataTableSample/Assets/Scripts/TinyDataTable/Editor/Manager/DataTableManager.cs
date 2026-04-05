using UnityEngine;
using UnityEditor;
using System;

namespace TinyDataTable.Editor
{
    public class DataTableManager : ScriptableObject
    {
        public enum DataType
        {
            Resources,
            Addresable
        }
        
        [SerializeField]
        public DataType dataType;
        [SerializeField]
        public string RootPath;
        [SerializeField]
        public string DefaultNamespace;

        public string TablesPath => $"Assets\\{RootPath}\\Tables";
        public string ScriptsPath => $"Assets\\{RootPath}\\Scripts";
        
        [SerializeField] public SerializableTree<DataTableAsset> Tree;
        
        public void MakeDirectory( string subPath )
        {
            var directory = $"Assets/{RootPath}\\{subPath}";
            
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            
                // Unity側にフォルダが作成されたことを認識させる
                AssetDatabase.Refresh();
            }
        }
    }
}