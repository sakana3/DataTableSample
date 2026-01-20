using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace TinyDataTable
{
    [Serializable]
    public class DataSheet
    {
        [SerializeReference] public IRecord record;

        public void Test()
        {
            Debug.Log(this.ToString());
        }
        
#if UNITY_EDITOR
        public void Initialize( params Type[] typeArgument )
        {
            record = MakeRecord(typeArgument);
            record.Header = new RecordHeader()
            {
                fieldInfos = Array.Empty<RecordFieldInfo>()
            };
            record.Iniaialize(new RecordDataHeader()
            {
                name = "Invalid",
                id = 0,
                index = 0,
                description = string.Empty
            });
        }
        
        public static IRecord MakeRecord( params Type[] typeArgument )
        {
            Type constructedType;
            if (typeArgument.Length > 0)
            {
                //型引数に応じたGeneric型を取得
                var genericDefinition = RecordGenericType.GetType(typeArgument.Length);
                //MakeGenericType で 型を生成
                constructedType = genericDefinition.MakeGenericType(typeArgument);
            }
            else
            {
                constructedType = typeof(RecordEmpty);
            }

            // インスタンス化
            IRecord instance = Activator.CreateInstance(constructedType) as IRecord;
            return instance;
        }

        public static IRecord ChangeRecordFieldType( IRecord record , params Type[] typeArgument )
        {
            var json = UnityEditor.EditorJsonUtility.ToJson(record);
            record = MakeRecord(typeArgument);
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, record);
            return record;
        }
        
        public void AddField( Type type , string fieldName ,bool isArray = false )
        {
            var header = record.Header;
            var newField = new RecordFieldInfo()
            {
                name = fieldName
            };
            header.fieldInfos = header.fieldInfos.Append(newField).ToArray();
            record.Header = header;

            if (isArray)
            {
                type = type.MakeArrayType();
            }
            
            var newFiledTypes = record.GetFieldTypes().Append(type).ToArray();
            record = ChangeRecordFieldType(record, newFiledTypes);
        }

        public void RemoveFieldField(string fieldName)
        {
            
        }
#endif        
    }
}