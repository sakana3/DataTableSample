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

        [ContextMenu("Add")]
        public void AddRow()
        {
            Data.AddRow( typeof(int) , "A");
            Data.AddRow( typeof(int) , "B");
            Data.AddRow( typeof(string) , "C" );
            Data.AddRow( typeof(Color)  , "D");
            Data.AddRow( typeof(float) , "E", true );
            
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
