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
        //Member
        [SerializeField] private DataTable data;

        /// GetData
        public DataTable Data => data;
        
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

