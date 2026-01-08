using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyDataTable
{
    [CreateAssetMenu(fileName = "DataTableAsset", menuName = "Scriptable Objects/DataTableAsset")]
    public class DataTableAsset : ScriptableObject
    {
        [SerializeField] private DataTable _Data;
        public DataTable Data => _Data;

        private void Awake()
        {
            _Data = new DataTable();
        }

        [ContextMenu("Add")]
        public void AddRaw()
        {
            Data.AddRaw( typeof(int) , "A");
            Data.AddRaw( typeof(int) , "B");
            Data.AddRaw( typeof(string) , "C" );
            Data.AddRaw( typeof(Color)  , "D");
            Data.AddRaw( typeof(float) , "E", true );
            
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Add Raw");
#endif
        }
        
        [ContextMenu("AddColumns")]
        public void AddColumns()
        {
            _Data.AddColumn("Hoge");
        }
    }
}
