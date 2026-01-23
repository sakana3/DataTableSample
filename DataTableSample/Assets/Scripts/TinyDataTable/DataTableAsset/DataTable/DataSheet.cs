using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace TinyDataTable
{
    [Serializable]
    public class DataSheet
    {
        [SerializeReference] public IRecord record;


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
                id = MakeNewID(),
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
                name = fieldName,
                id = MakeNewID(),
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

        public bool RemoveField(int index)
        {
             if (index >= 0)
             {
                 var tmpHeader = record.Header;
                 tmpHeader.fieldInfos = tmpHeader.fieldInfos
                     .Where((t, i) => i != index)
                     .ToArray();
                 record.Header = tmpHeader;
                 var oldTypes = record.GetFieldTypes();
                 var newTypes = oldTypes
                     .Where((_, i) => i != index )
                     .Where((_, i) => i < tmpHeader.fieldInfos.Length)
                     .ToArray();
                 
                 var json = UnityEditor.EditorJsonUtility.ToJson(record,true);
                 //Jsonの中身を新しいフィールドに書き換える
                 //TODO : 単なる置き換えなので後でちゃんとJSONをパースするようにする。
                 for (int i = index; i < oldTypes.Length; i++)
                 {
                     json = json.Replace($"\"Field{i}\":", $"\"___Field{i}\":");
                 }
                 for ( int i = index ; i < oldTypes.Length ; i++)
                 {
                     var t = i -1 >= 0 ? $"Field{i -1}" : "FieldX";
                     json = json.Replace($"\"___Field{i}\":", $"\"{t}\":");
                 }
                 record = MakeRecord(newTypes);
                 UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, record);                 

                 return true;
             }
             return false;
        }

        /// <summary>
        /// ヘッダーと定義に差があった場合いい感じにそろえる
        /// </summary>
        public void FitField()
        {
            var filedCount = record.Header.fieldInfos.Length;
            if (filedCount < record.GetFieldTypes().Length)
            {
                var newTypes = record.GetFieldTypes()
                    .Where((_, i) => i < filedCount)
                    .ToArray();
                record = MakeRecord(newTypes);
            }
        }

        /// <summary>
        /// 新規idを作成
        /// </summary>
        private int MakeNewID()
        {
            var idCandidates = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, int.MaxValue);
            while ( record.Header.fieldInfos.Any(t => t.id == idCandidates) ||
                    record.Records.Any( t => t.Header.id == idCandidates )
                  )
            {
                idCandidates = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, int.MaxValue);
            }

            return idCandidates;
        }
#endif        
    }
}