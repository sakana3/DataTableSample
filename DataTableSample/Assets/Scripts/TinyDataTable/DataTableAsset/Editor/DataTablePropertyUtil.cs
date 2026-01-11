using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace TinyDataTable.Editor
{

    public static class DataTablePropertyUtil
    {
        public static SerializedProperty GetColumns(SerializedProperty property)
        {
            return property.FindPropertyRelative("columns");
        }
        public static SerializedProperty GetColumn(SerializedProperty property , int index)
        {
            return property
                .FindPropertyRelative("columns")
                .GetArrayElementAtIndex( index);
        }

        public static SerializedProperty GetRows(SerializedProperty property)
        {
            return property.FindPropertyRelative("rowData");
        }
        
        public static SerializedProperty GetCell(SerializedProperty property , int colum , int row)
        {
            var columProp = property
                .FindPropertyRelative("columns")
                .GetArrayElementAtIndex( colum);            
            var cell = columProp.FindPropertyRelative("rowData")
                .GetArrayElementAtIndex( row);            
            
            return cell;
        }
        

        public static SerializedProperty GetHeader(SerializedProperty property)
        {
            var columns = GetColumns(property);
            var header = columns.GetArrayElementAtIndex(0);
            return header;
        }

        public static SerializedProperty GetHeaderRow(SerializedProperty property)
        {
            var header = GetHeader(property);
            var rows = GetRows(header);
            return rows;
        }

        public static void InsertRow(SerializedProperty property,string recordName, int index = -1)
        {
            var columns = property.FindPropertyRelative("columns");
            for (int col = 0; col < columns.arraySize; col++)
            {
                var columProp = columns.GetArrayElementAtIndex(col);
                var rowProp = columProp.FindPropertyRelative("rowData");
                var newIndex = index >= 0 ? index : rowProp.arraySize;
                rowProp.InsertArrayElementAtIndex(newIndex);
                if (col == 0)
                {
                    InitializeHeader(property,recordName, newIndex);
                }
            }

            property.serializedObject.ApplyModifiedProperties();
            //       property.serializedObject.Update();
        }

        public static void RemoveRow(SerializedProperty property, int index = -1)
        {
            var columns = property.FindPropertyRelative("columns");
            for (int col = 0; col < columns.arraySize; col++)
            {
                var columProp = columns.GetArrayElementAtIndex(col);
                var rowProp = columProp.FindPropertyRelative("rowData");
                rowProp.DeleteArrayElementAtIndex(index >= 0 ? index : rowProp.arraySize);
            }

            property.serializedObject.ApplyModifiedProperties();
//        property.serializedObject.Update();
        }

        public static void ResizeRow(SerializedProperty property,string recordName, uint newSize)
        {
            var columns = property.FindPropertyRelative("columns");
            for (int col = 0; col < columns.arraySize; col++)
            {
                var columProp = columns.GetArrayElementAtIndex(col);
                var rowProp = columProp.FindPropertyRelative("rowData");
                while (rowProp.arraySize != newSize)
                {
                    if (rowProp.arraySize > newSize)
                    {
                        rowProp.DeleteArrayElementAtIndex(rowProp.arraySize - 1);
                    }
                    else
                    {
                        rowProp.InsertArrayElementAtIndex(rowProp.arraySize);
                        if (col == 0)
                        {
                            InitializeHeader(property,recordName, rowProp.arraySize - 1);
                        }
                    }
                }
            }


            property.serializedObject.ApplyModifiedProperties();
//        property.serializedObject.Update();
        }

        private static void InitializeHeader(SerializedProperty property,string recordName, int rowIndex)
        {
            var columns = property.FindPropertyRelative("columns");
            var columProp = columns.GetArrayElementAtIndex(0);
            var rowProp = columProp.FindPropertyRelative("rowData");
            var headerProp = rowProp.GetArrayElementAtIndex(rowIndex);
            var headerProps = Enumerable.Range(0, rowProp.arraySize)
                .Select(i => rowProp.GetArrayElementAtIndex(i));

            //仮の名前を付ける
            var nameProp = headerProp.FindPropertyRelative("Name");
            nameProp.stringValue = string.Empty;
            string nameCandidates = $"{recordName.Replace(" ","_")}_{rowIndex:0000}";
            var nameEnumrator = headerProps
                .Select(prop => prop.FindPropertyRelative("Name").stringValue);
            while (nameEnumrator.Any(i => i == nameCandidates))
            {
                nameCandidates += "_";
            }

            nameProp.stringValue = nameCandidates;

            //IDをつける
            var idProp = headerProp.FindPropertyRelative("ID");
            idProp.intValue = 0;
            idProp.intValue = MakeNewID(property);
        }

        public static void MoveRow(SerializedProperty property, int from, int to)
        {
            var columns = property.FindPropertyRelative("columns");
            for (int col = 0; col < columns.arraySize; col++)
            {
                var columProp = columns.GetArrayElementAtIndex(col);
                var rowProp = columProp.FindPropertyRelative("rowData");
                rowProp.MoveArrayElement(from, to);
            }

            property.serializedObject.ApplyModifiedProperties();
//        property.serializedObject.Update();
        }

        internal static SerializedProperty InsertColumn(
            SerializedProperty property ,
            int index,
            string name ,
            Type type,
            bool isArray,
            string description
            )
        {
            var columns = property.FindPropertyRelative("columns");

            columns.InsertArrayElementAtIndex(index);

            var newColum = columns.GetArrayElementAtIndex(index);

            
            var dt = MakeColumnData(type , isArray );
            dt.Name = name;
            dt.Obsolete = false;
            dt.Description = description;
            dt.Resize(GetHeaderRow(property).arraySize);

            newColum.managedReferenceValue = dt;

            newColum.FindPropertyRelative("id").intValue = MakeNewID(property);

            property.serializedObject.ApplyModifiedProperties();

            return newColum;
        }

        internal static void RemoveColumn( SerializedProperty property , int index )
        {
            var columns = property.FindPropertyRelative("columns");
            columns.DeleteArrayElementAtIndex(index);
            property.serializedObject.ApplyModifiedProperties();
        }

        internal static IDataTableColumn MakeColumnData(Type typeArgument, bool isArray = false)
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

        
        
        public static ( List<string> propNames, List<string> idNames ) MakeNameList(SerializedProperty property)
        {
            var columns = property.FindPropertyRelative("columns");
            var propNames = Enumerable.Range(0, columns.arraySize)
                .Select(i => columns.GetArrayElementAtIndex(i))
                .Select(col => col.FindPropertyRelative("name"))
                .Select(prop => prop.stringValue)
                .ToList();

            var rows = columns
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative("rowData");
            var idNames = Enumerable.Range(0, rows.arraySize)
                .Select(i => rows.GetArrayElementAtIndex(i))
                .Select(row => row.FindPropertyRelative("Name"))
                .Select(prop => prop.stringValue)
                .ToList();
            
            return (propNames, idNames);
        }

        public static bool ChakeCanUseName(SerializedProperty property, SerializedProperty nameProp)
        {
            var columns = property.FindPropertyRelative("columns");
            var propNames = Enumerable.Range(0, columns.arraySize)
                .Select(i => columns.GetArrayElementAtIndex(i))
                .Select(col => col.FindPropertyRelative("name"))
                .Where( t => !SerializedProperty.EqualContents(t, nameProp) )
                .Select(prop => prop.stringValue);

            if (propNames.Any(t => t == nameProp.stringValue ))
            {
                return false;
            }

            var rows = columns
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative("rowData");
            var idNames = Enumerable.Range(0, rows.arraySize)
                .Select(i => rows.GetArrayElementAtIndex(i))
                .Select(row => row.FindPropertyRelative("Name"))
                .Where( t => !SerializedProperty.EqualContents(t, nameProp) )
                .Select(prop => prop.stringValue);

            if (idNames.Any(t => t == nameProp.stringValue ))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 新規IDを作る
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static int MakeNewID(SerializedProperty property)
        {
            var columns = property.FindPropertyRelative("columns");
            var columProp = columns.GetArrayElementAtIndex(0);
            var rowProp = columProp.FindPropertyRelative("rowData");
            var headerProps = Enumerable.Range(0, rowProp.arraySize)
                .Select(i => rowProp.GetArrayElementAtIndex(i));

            var idCandidates = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, int.MaxValue);
            var idEnumrator = headerProps
                .Select(prop => prop.FindPropertyRelative("ID").intValue);
            while (idEnumrator.Any(i => i == idCandidates) && idCandidates > 0)
            {
                idCandidates = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, int.MaxValue);
            }

            var colEnumtaror = Enumerable
                .Range(0, columns.arraySize)
                .Select(i => columns.GetArrayElementAtIndex(i).FindPropertyRelative("id"));
            while (colEnumtaror.Any(t => t.intValue == idCandidates))
            {
                idCandidates = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, int.MaxValue);
            }    

            return idCandidates;
        }

        public static bool CheckTableSizeChanged(SerializedProperty property, List<int> rows,List<int> columnList)
        {
            var columnProp = GetColumns(property);
            if (columnProp.arraySize != columnList.Count)
            {
                return true;
            }
            var header = GetHeaderRow(property);
            if (header.arraySize != rows.Count)
            {
                return true;
            }

            return false;
        }

        public static List<string> ReservWords = new List<string>()
            { "ID", "Invalid", "ToString", "GetHashCode", "GetType", "Enum" };        
    }
}