using System;
using Unity.VisualScripting;
using UnityEngine;

namespace TinyDataTable
{
    public interface IDataTableRaw
    {
        Type type { get; }
        int Size { get; }

        string Name { set; get; }

        void Prepare();
        
        void Resize(int size);
    }

    public interface IDataTableRawT<T> : IDataTableRaw
    {
        T[] Data { get; }
    }
    
    [Serializable]
    public struct DataTableRawData<T> : IDataTableRawT<T>
    {
        public Type type => typeof(T);

        [SerializeField]
        private string _name;
        public string Name { set => _name = value; get => _name; }

        [SerializeField]
        public T[] data;
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
    }

    [Serializable]
    public struct DataTableArray<T>
    {
        public T[] array;
    }

}