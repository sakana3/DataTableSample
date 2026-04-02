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
    }
}