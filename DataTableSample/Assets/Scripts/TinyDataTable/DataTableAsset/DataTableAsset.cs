using System;
using System.Reflection;
using UnityEngine;

namespace TinyDataTable
{
    [CreateAssetMenu(fileName = "NewDataTable", menuName = "TinyDataTable/NewDataTable")]
    public class DataTableAsset : ScriptableObject
    {
        [SerializeField]
        private string classType;

        public Type ClassType
        {
            get => string.IsNullOrEmpty(classType) ? null : Type.GetType( classType);
            set => classType = value.FullName;
        }
        
#if UNITY_EDITOR        
        [SerializeField]
        public UnityEditor.MonoScript classScript;
        
        [SerializeField,]
        private bool obsolete;
        public bool Obsolete
        {
            get { return obsolete; }
            set { obsolete = value; }
        }

#endif
        //Tags
        [SerializeField] private string[] tags = Array.Empty<string>();

        //Tags
        public string[] Tags => tags;

        //DataTable
        [SerializeField] private DataSheet dataSheet;

        /// GetData
        public DataSheet DataSheet => dataSheet;       
        
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

        private void Reset()
        {
            dataSheet = new DataSheet();
            dataSheet.Initialize();
        }

        private void OnEnable()
        {
            if (ClassType != null)
            {
                MethodInfo method = ClassType.GetMethod("BindAsset", BindingFlags.NonPublic | BindingFlags.Static);
                method?.Invoke(null, new object[] { this });
            }
        }

        private void OnDisable()
        {
            if (ClassType != null)
            {
                MethodInfo method = ClassType.GetMethod("BindAsset", BindingFlags.NonPublic | BindingFlags.Static);
                method?.Invoke(null, new object[] { null });
            }
        }
    }
}

