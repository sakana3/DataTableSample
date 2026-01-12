using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyDataTable
{
    [Serializable]
    public class DataTable
    {
        [Serializable]
        public struct Header
        {
            public string Name;
            public int Index;
            public int ID;
            public bool Obsolete;
            public string Description;
        }

        public static string HeaderUniqeName = "Header";
        
        /// Rows     
        [SerializeReference] private IDataTableColumn[] columns = default;
        
        /// Rows List  
        public IReadOnlyList<IDataTableColumn> Columns => columns;
        
        /// RowsSpan       
        public ReadOnlySpan<IDataTableColumn> ColumnsSpan => columns.AsSpan();

        /// GetColum       
        public DataTableColumnData<T> GetColum<T>(int index) => columns[index+1] as DataTableColumnData<T>;

        /// GetColum       
        public DataTableColumnData<T> GetColum<T>(string fieldName) => columns.FirstOrDefault(t=>t.Name==fieldName) as DataTableColumnData<T>;
        
        /// GetCell
        public T GetCell<T>(int iColumn, int iRow) => GetColum<T>(iColumn).RowData[iRow+1];

        /// GetCell
        public T GetCell<T>(string fieldName, int iRow ) =>
            GetColum<T>(fieldName).RowData[iRow];

        /// GetCell
        public T GetCell<T>(string fieldName, string recordName ) =>
            GetColum<T>(fieldName).RowData[GetRowIndex(recordName)];
        
        /// GetRowIndex
        public int GetRowIndex(string recordName) =>
            Array.FindIndex((columns[0] as DataTableColumnData<Header>).RowData,t=>t.Name == recordName);
        
        /// Returns the number of columns in the data table.
        public int columnSize => columns[0].RowSize;

        /// Returns the number of rows in the data table.
        public int rowSize => columns[0] == null ? 0 : columns[0].RowSize;

        /// <summary> get header </summary>
        public ref Header GetHeader( int row ) => ref ((DataTableColumnData<Header>)columns[0]).RowData[row];

        /// Represents a table structure
        public DataTable()
        {
            columns = new IDataTableColumn[0];
            AddColumn(typeof(Header), HeaderUniqeName);
            ref var header = ref AddRowInline();
            header.Name = "Invalid";
            header.ID = -1;
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
        public ref Header AddRowInline()
        {
            var rowSize = columns == null ? 0 : columns[0].RowSize;
            foreach (var column in columns)
            {   
                column.Resize( rowSize + 1 );
            }
            return ref GetHeader(columnSize - 1);
        }        

        public string MakeTmpRowName( string header )
        {
            var tmp = $"{header}_{0}";
            return tmp;
        }

        private void RecalculateRowIndex()
        {
            var index = 0;
            foreach (ref var header in ((DataTableColumnData<Header>)ColumnsSpan[0]).RowData.AsSpan())
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
            var headers = ((DataTableColumnData<Header>)ColumnsSpan[0]).RowData;
            while (random <= 0 && headers.Any(t => t.ID == random))
            {
                random = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0,int.MaxValue);
            }
            return random;
        }
    }
}

