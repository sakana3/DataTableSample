using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = System.Object;


namespace TinyDataTable
{
    public interface IDataTableColumn
    {
        Type Type { get; }
        int RowSize { get; }

        string Name { set; get; }
        public bool Obsolete { set; get; }
        public int ID { set; get; }
        
        void Prepare();
        
        void Resize(int size);

        IEnumerable<E> GetRowObjects<E>();
        
        bool IsArray => typeof(IDataTableRowArray).IsAssignableFrom(Type);            
    }

    public interface IDataTableColumnT<T> : IDataTableColumn
    {
        T[] RowData { get; }
    }
    
    [Serializable]
    public struct DataTableColumnData<T> : IDataTableColumnT<T>
    {
        public Type Type => typeof(T);

        [SerializeField]
        private string name;
        public string Name { set => name = value; get => name; }

        [SerializeField]
        private bool obsolete;
        public bool Obsolete { set => obsolete = value; get => obsolete; }

        [SerializeField]
        private int id;
        public int ID { set => id = value; get => id; }
        
        [SerializeField]
        private T[] rowData;
        public T[] RowData => rowData;

        public int RowSize => rowData == null ? 0 : rowData.Length;

        public void Prepare()
        {
            rowData = rowData ?? Array.Empty<T>();
        }
        
        public void Resize(int size)
        {
            Array.Resize(ref rowData, size);
        }

        public IEnumerable<E> GetRowObjects<E>() => rowData.OfType<E>();
    }

    public interface IDataTableRowArray
    {
        
    }
    
    [Serializable]
    public struct DataTableRowArray<T> : IDataTableRowArray
    {
        public T[] array;
    }
    
    internal static class DataTableColumn
    {
        internal static IDataTableColumn MakeColumnData(Type typeArgument , bool isArray = false)
        {
            if (isArray)
            {
                return MakeColumnArray(typeArgument);
            }
            else
            {
                return MakeGenericClass(typeof(DataTableColumnData<>), typeArgument);
            }
        }

        internal static IDataTableColumn MakeColumnArray(Type typeArgument)
        {
            Type arrayType = typeof(DataTableRowArray<>).MakeGenericType(typeArgument);
            return MakeGenericClass(typeof(DataTableColumnData<>), arrayType);
        }

        internal static IDataTableColumn MakeGenericClass(Type genericDefinition, Type typeArgument)
        {
            // MakeGenericType で 型を生成
            Type constructedType = genericDefinition.MakeGenericType(typeArgument);

            // インスタンス化 (Activatorを使用)
            object instance = Activator.CreateInstance(constructedType);
            return instance as IDataTableColumn;
        }

    }    
}