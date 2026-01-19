using System;
using UnityEngine;

namespace TinyDataTable
{
    [CreateAssetMenu(fileName = "NewDataTable", menuName = "TinyDataTable/NewDataTable")]
    public class DataTableAsset : ScriptableObject
    {
        [SerializeField]
        private string classType;

        public string ClassType
        {
            get => classType;
            set => classType = value;
        }
        
#if UNITY_EDITOR        
        [SerializeField]
        private UnityEditor.MonoScript classScript;
        
        public UnityEditor.MonoScript ClassScript
        {
            get => classScript;
            set => classScript = value;
        }
#endif
        //Tags
        [SerializeField] private string[] tags = Array.Empty<string>();

        //Tags
        public string[] Tags => tags;
        
        //Data
        [SerializeField] private DataTableRow data;

        /// GetData
        public DataTableRow Data => data;
        
        /// GetColum       
        public DataTableColumnData<T> GetColum<T>(int index) => data.GetColum<T>(index);

        /// GetColum
        public DataTableColumnData<T> GetColum<T>(string fieldName) => data.GetColum<T>(fieldName);
        
        /// GetCell
        public T GetCell<T>(int iColumn, int iRow) => data.GetCell<T>(iColumn,iRow);

        /// GetCell
        public T GetCell<T>(string fieldName, int iRow ) => data.GetCell<T>(fieldName,iRow);

        /// GetCell
        public T GetCell<T>(string fieldName, string recordName ) => data.GetCell<T>(fieldName,recordName);
        
        /// Returns the number of columns in the data table.
        public int ColumnSize => data.ColumnSize;

        /// Returns the number of rows in the data table.
        public int RowSize => data.RowSize;        
    }
}

