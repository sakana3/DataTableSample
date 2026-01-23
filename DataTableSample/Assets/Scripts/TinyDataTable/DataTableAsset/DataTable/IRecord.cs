using System;
using System.Collections;
using System.Collections.Generic;

namespace TinyDataTable
{
    public interface IRecord
    {
        public RecordHeader Header { get; set; }
        public Type[] GetFieldTypes();
        public IRecordData GetRecord(int rowIndex);
        public IEnumerable<IRecordData> Records { get; }
        public void Iniaialize(RecordDataHeader newHeader) { }
    }
    
    public interface IRecordData
    {
        public RecordDataHeader Header {set; get;}        
    }
}