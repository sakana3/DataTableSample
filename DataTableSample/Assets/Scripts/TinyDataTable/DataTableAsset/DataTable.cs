using System;
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
            public string name;
            public int index;
            public int id;
        }
        
        [SerializeReference] private IDataTableRaw[] columns = default;
        
        public ReadOnlySpan<IDataTableRaw> ColumnsSpan => columns;

        public Header GetHeader(int index)
        {
            return ((DataTableRawData<Header>)columns[index]).data[0];
        }

        /// Represents a table structure capable of holding multiple columns of different types.
        /// Includes functionality for adding raw data, adding columns, and managing headers.
        public DataTable()
        {
            columns = new IDataTableRaw[0];
            AddRaw(typeof(Header), "Header");
            AddColumn("Invalid");            
        }

        
        public IDataTableRaw AddRaw(Type typeRaw , string rawName , bool isArray = false)
        {
            Array.Resize(ref columns, columns.Length + 1);
            var newRaw = isArray ? MakeRawArray(typeRaw) : MakeRawData(typeRaw);
            newRaw.Prepare();
            newRaw.Name = rawName;
            columns[^1] = newRaw;
            newRaw.Resize( columns[0] == null ? 0 : columns[0].Size );
            
            return newRaw;
        }


        /// Adds a new column to the data table, resizing all existing columns to accommodate the new column.
        /// <param name="columnName">The name of the new column to be added.</param>
        public void AddColumn( string columnName )
        {
            var rawSize = columns == null ? 0 : columns[^1].Size;
            foreach (var d in columns)
            {   
                d.Resize( rawSize + 1 );
            }

            if (columns[0] is DataTableRawData<Header>)
            {
                var header = (DataTableRawData<Header>)columns[0];
                header.Name = columnName;
            }
        }
        
        private static IDataTableRaw MakeRawData(Type typeArgument)
        {
            return MakeGenericClass(typeof(DataTableRawData<>), typeArgument);
        }

        private static IDataTableRaw MakeRawArray(Type typeArgument)
        {
            Type arrayType = typeof(DataTableArray<>).MakeGenericType(typeArgument);
            return MakeGenericClass(typeof(DataTableRawData<>), arrayType);
        }

        private static IDataTableRaw MakeGenericClass(Type genericDefinition, Type typeArgument)
        {
            // MakeGenericType で 型を生成
            Type constructedType = genericDefinition.MakeGenericType(typeArgument);

            // インスタンス化 (Activatorを使用)
            object instance = Activator.CreateInstance(constructedType);
            return instance as IDataTableRaw;
        }
    }
}