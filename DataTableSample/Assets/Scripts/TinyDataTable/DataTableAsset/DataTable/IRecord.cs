using System;

namespace TinyDataTable
{
    public interface IRecord
    {
        public RecordHeader Header { get; set; }
        public Type[] GetFieldTypes();
        public IRecordData GetRecord(int rowIndex);
        public void Iniaialize(RecordDataHeader newHeader) { }
    }
    
    public interface IRecordData
    {
        public RecordDataHeader Header {set; get;}        
    }
}