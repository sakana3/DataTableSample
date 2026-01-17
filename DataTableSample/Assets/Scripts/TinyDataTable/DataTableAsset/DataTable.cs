using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TinyDataTable
{
    [Serializable]
    public class DataTable
    {
        [Serializable]
        public struct HeaderData
        {
            public string Name;
            public int Index;
            public int ID;
            public bool Obsolete;
            public string Description;
        }

        [Serializable]
        public class DataTableColumnHeader : DataTableColumnData<HeaderData>
        {
            
        }

//        public static string HeaderUniqeName = "Header";

        /// Rows     
        [SerializeField] private DataTableColumnHeader header;

        public DataTableColumnHeader Header => header;
        
        /// Rows     
        [SerializeReference] private IDataTableColumn[] columns = default;
        
        /// Rows List  
        public IReadOnlyList<IDataTableColumn> Columns => columns;
        
        /// RowsSpan       
        public ReadOnlySpan<IDataTableColumn> ColumnsSpan => columns.AsSpan();

        /// GetColum       
        public DataTableColumnData<T> GetColum<T>(int index) => columns[index] as DataTableColumnData<T>;

        /// GetColum       
        public DataTableColumnData<T> GetColum<T>(string fieldName) => columns.FirstOrDefault(t=>t.Name==fieldName) as DataTableColumnData<T>;
        
        /// GetCell
        public T GetCell<T>(int iColumn, int iRow) => (columns[iColumn] as DataTableColumnData<T>).RowData[iRow];
        
        /// GetCell
        public T GetCell<T>(string fieldName, int iRow ) => GetColum<T>(fieldName).RowData[iRow];

        /// GetCell
        public T GetCell<T>(string fieldName, string recordName ) =>
            GetColum<T>(fieldName).RowData[GetRowIndex(recordName)];

        /// GetRowIndex
        public int GetRowIndex(string recordName) =>
            Array.FindIndex(Header.RowData,t=>t.Name == recordName);

        /// GetRowIndex
        public int GetRowIndexByID(int ID) =>
            Array.FindIndex(Header.RowData,t=>t.ID == ID);
        
        /// Returns the number of columns in the data table.
        public int ColumnSize => columns.Length;

        /// Returns the number of rows in the data table.
        public int RowSize => Header.RowSize;

        /// Represents a table structure
        public DataTable()
        {
            this.header = new DataTableColumnHeader();
            this.header.Prepare();
            this.header.Name = "Header";
            this.header.ID = 0;
            this.header.Resize(1);
            this.header.RowData[0].Index = 0;
            this.header.RowData[0].Name = "Invalid";
            this.header.RowData[0].ID = 0;
            
            columns = Array.Empty<IDataTableColumn>();
        }

        /// <summary> Add column </summary>
        public IDataTableColumn AddColumn(Type typeRaw , string rawName , bool isArray = false)
        {
            Array.Resize(ref columns, columns.Length + 1);
            var newColumn = DataTableColumn.MakeColumnData(typeRaw,isArray);
            newColumn.Prepare();
            newColumn.Name = rawName;
            columns[^1] = newColumn;
            newColumn.Resize( columns[0] == null ? 0 : columns[0].RowSize );
            
            return newColumn;
        }
        
        /// <summary> Add row </summary>
        public void AddRow( string columnName )
        {
            ref var header = ref AddRowInline();
            header.Name = columnName;
            header.ID = MakeUID();
            RecalculateRowIndex();
        }
        
        /// <summary> Add column </summary>
        private ref HeaderData AddRowInline()
        {
            var rowSize = columns == null ? 0 : header.RowSize;
            foreach (var column in columns)
            {   
                column.Resize( rowSize + 1 );
            }
            header.Resize( rowSize + 1);
            return ref header.RowData[rowSize];
        }

        public string MakeTmpRowName( string header )
        {
            var tmp = $"{header}_{0}";
            return tmp;
        }

        private void RecalculateRowIndex()
        {
            var index = 0;
            foreach (ref var header in ((DataTableColumnData<HeaderData>)ColumnsSpan[0]).RowData.AsSpan())
            {
                if (header.ID == 0)
                {
                    header.ID = MakeUID();
                }
                header.Index = index++;
            }            
        }
        
        public int MakeUID()
        {
            var random = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0,int.MaxValue);
            var headers = ((DataTableColumnData<HeaderData>)ColumnsSpan[0]).RowData;
            while (random <= 0 && headers.Any(t => t.ID == random))
            {
                random = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0,int.MaxValue);
            }
            return random;
        }
    }
}

