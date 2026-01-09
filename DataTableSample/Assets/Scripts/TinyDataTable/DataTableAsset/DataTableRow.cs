using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Object = System.Object;


namespace TinyDataTable
{
    public interface IDataTableRow
    {
        Type Type { get; }
        int Size { get; }

        string Name { set; get; }

        void Prepare();
        
        void Resize(int size);

        IEnumerable<E> GetObjects<E>();
        
        bool IsArray => typeof(IDataTableRowArray).IsAssignableFrom(Type);            
    }

    public interface IDataTableRowT<T> : IDataTableRow
    {
        T[] Data { get; }
    }
    
    [Serializable]
    public struct DataTableRowData<T> : IDataTableRowT<T>
    {
        public Type Type => typeof(T);

        [SerializeField]
        private string _name;
        public string Name { set => _name = value; get => _name; }

        [SerializeField]
        private T[] data;
        public T[] Data => data;

        public int Size => data == null ? 0 : data.Length;

        public void Prepare()
        {
            data = data ?? Array.Empty<T>();
        }
        
        public void Resize(int size)
        {
            Array.Resize(ref data, size);
        }

        public IEnumerable<E> GetObjects<E>() => data.OfType<E>();
    }

    public interface IDataTableRowArray
    {
        
    }
    
    [Serializable]
    public struct DataTableRowArray<T> : IDataTableRowArray
    {
        public T[] array;
    }
    
    internal static class DataTableRow
    {
        internal static IDataTableRow MakeRawData(Type typeArgument , bool isArray = false)
        {
            if (isArray)
            {
                return MakeGenericClass(typeof(DataTableRowData<>), typeArgument);
            }
            else
            {
                return MakeRawArray(typeArgument);
            }
        }

        internal static IDataTableRow MakeRawArray(Type typeArgument)
        {
            Type arrayType = typeof(DataTableRowArray<>).MakeGenericType(typeArgument);
            return MakeGenericClass(typeof(DataTableRowData<>), arrayType);
        }

        internal static IDataTableRow MakeGenericClass(Type genericDefinition, Type typeArgument)
        {
            // MakeGenericType で 型を生成
            Type constructedType = genericDefinition.MakeGenericType(typeArgument);

            // インスタンス化 (Activatorを使用)
            object instance = Activator.CreateInstance(constructedType);
            return instance as IDataTableRow;
        }

    }    
}