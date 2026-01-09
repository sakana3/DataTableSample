using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        }
        
        /// Rows     
        [SerializeReference] private IDataTableRow[] rows = default;
        
        /// Rows List  
        private IReadOnlyList<IDataTableRow> Rows => rows;
        
        /// RowsSpan       
        public ReadOnlySpan<IDataTableRow> RowsSpan => rows.AsSpan();

        /// Returns the number of columns in the data table.
        public int columnSize => rows[0].Size;
        
        /// Returns the number of rows in the data table.
        public int rowSize => rows[0] == null ? 0 : rows[0].Size;
        
        /// <summary> get header </summary>
        public ref Header GetHeader( int column ) => ref ((DataTableRowData<Header>)rows[0]).Data[column];

        /// Represents a table structure
        public DataTable()
        {
            rows = new IDataTableRow[0];
            AddRow(typeof(Header), "Header");
            var header = AddColumnInline();
            header.Name = "Invalid";
            header.ID = -1;
        }        
        
        /// <summary> Add row </summary>
        public IDataTableRow AddRow(Type typeRaw , string rawName , bool isArray = false)
        {
            Array.Resize(ref rows, rows.Length + 1);
            var newRow = DataTableRow.MakeRawData(typeRaw,isArray);
            newRow.Prepare();
            newRow.Name = rawName;
            rows[^1] = newRow;
            newRow.Resize( rows[0] == null ? 0 : rows[0].Size );
            
            return newRow;
        }
        
        /// <summary> Add column </summary>
        public void AddColumn( string columnName )
        {
            var header = AddColumnInline();
            header.Name = columnName;
            header.ID = MakeUID();
            RecalculateColumnIndex();
        }
        
        /// <summary> Add column </summary>
        public ref Header AddColumnInline()
        {
            var colSize = rows == null ? 0 : rows[0].Size;
            foreach (var row in rows)
            {   
                row.Resize( colSize + 1 );
            }
            return ref GetHeader(columnSize - 1);
        }        

        public string MakeTmpColumnName( string header )
        {
            var tmp = $"{header}_{0}";
            return tmp;
        }

        private void RecalculateColumnIndex()
        {
            var index = 0;
            foreach (ref var header in ((DataTableRowData<Header>)RowsSpan[0]).Data.AsSpan())
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
            var headers = ((DataTableRowData<Header>)RowsSpan[0]).Data;
            while (random <= 0 && headers.Any(t => t.ID == random))
            {
                random = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0,int.MaxValue);
            }
            return random;
        }
    }
}

