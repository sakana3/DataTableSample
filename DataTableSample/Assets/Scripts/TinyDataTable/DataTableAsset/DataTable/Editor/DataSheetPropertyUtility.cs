using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace TinyDataTable.Editor
{
    internal static class DataSheetPropertyUtility
    {
        public static (string name, string description, bool obsolete) GetColumn(SerializedProperty property, int index)
        {
            var header = property.FindPropertyRelative("record.header.fieldInfos");
            var info = header.GetArrayElementAtIndex(index);
            var name = info.FindPropertyRelative("name");
            var description = info.FindPropertyRelative("description");
            var obsolete = info.FindPropertyRelative("obsolete");

            return (name.stringValue, description.stringValue, obsolete.boolValue);
        }

        
        public static bool IsColumObsolete(SerializedProperty property , int iColum)
        {
            var infos = property.FindPropertyRelative("record.header.fieldInfos");
            var info = infos.GetArrayElementAtIndex(iColum);
            var obsoleteColum = info.FindPropertyRelative("obsolete");

            return obsoleteColum.boolValue;
        }        
        
        public static bool IsRowObsolete(SerializedProperty property,int iRow)
        {
            var recordHeader = property.FindPropertyRelative("record.recordData");
            var recordInfo = recordHeader.GetArrayElementAtIndex(iRow);
            var obsoleteRow = recordInfo.FindPropertyRelative("header.obsolete");

            return obsoleteRow.boolValue;
        }

        public static SerializedProperty GetRowNameProperty(SerializedProperty property, int iRow)
        {
            var recordHeader = property.FindPropertyRelative("record.recordData");
            var recordInfo = recordHeader.GetArrayElementAtIndex(iRow);
            var nameProp = recordInfo.FindPropertyRelative("header.name");
            return nameProp;
        }
        
        /// <summary>
        /// 行の個数を取得
        /// </summary>
        /// <returns></returns>
        public static int GetColumnCount(SerializedProperty property)
        {
            var infos = property.FindPropertyRelative("record.header.fieldInfos");
            return infos == null ? 0 :infos.arraySize;
        }
        
        /// <summary>
        /// 列の個数を取得
        /// </summary>
        /// <returns></returns>
        public static int GetRowCount(SerializedProperty property)
        {
            var infos = property.FindPropertyRelative("record.recordData");
            return infos == null ? 0 : infos.arraySize;
        }
        
        public static SerializedProperty GetCellProperty(SerializedProperty property, int iColum, int iRow)
        {
            var recordHeader = property.FindPropertyRelative("record.recordData");
            var recordInfo = recordHeader.GetArrayElementAtIndex(iRow);
            var cellProp = recordInfo.FindPropertyRelative($"Field{iColum}");
             
            return cellProp;
        }

        public static (List<string> fieldNames, List<string> recordNames ) MakeNameList(SerializedProperty property)
        {
            List<string> fieldNames = new ();

            var infos = property.FindPropertyRelative("record.header.fieldInfos");
            for (int i = 0; i < infos.arraySize; i++)
            {
                var info = infos.GetArrayElementAtIndex(i);
                var obsoleteColum = info.FindPropertyRelative("name");
                fieldNames.Add(obsoleteColum.stringValue);
            }

            List<string> recordNames = new ();

            return (fieldNames, recordNames);
        }
        


        /// <summary>
        /// SerializedPropertyが参照している実際のインスタンスを取得する
        /// </summary>
        public static object GetValue(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data[", "["); // 配列パスを正規化
            string[] elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    // 配列/リスト要素の場合
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    
                    obj = GetFieldValue(obj, elementName);
                    obj = GetElementAtIndex(obj, index);
                }
                else
                {
                    // 通常フィールドの場合
                    obj = GetFieldValue(obj, element);
                }
            }
            
            return obj;
        }

        private static object GetFieldValue(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return f.GetValue(source);
                
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null) return p.GetValue(source, null);
                
                type = type.BaseType;
            }
            return null;
        }

        private static object GetElementAtIndex(object collection, int index)
        {
            if (collection is IList list)
            {
                return list[index];
            }
            return null;
        }
    
    }
}