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
        [SerializeField] private DataTable data;
        public DataTable Data => data;

        
        [ContextMenu("Add")]
        public void AddRow()
        {
            Data.AddColumn( typeof(int) , "A");
            Data.AddColumn( typeof(int) , "B");
            Data.AddColumn( typeof(string) , "C" );
            Data.AddColumn( typeof(Color)  , "D");
            Data.AddColumn( typeof(float) , "E", true );
            
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Add Raw");
#endif
        }
        
        [ContextMenu("AddColumns")]
        public void AddColumns()
        {
            data.AddRow("Hoge");
        }
    }
}
