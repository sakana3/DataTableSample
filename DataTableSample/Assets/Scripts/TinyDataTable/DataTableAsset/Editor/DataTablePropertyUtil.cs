using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TinyDataTable;

namespace TinyDataTable.Editor
{

    public static class DataTablePropertyUtil
    {
        public static SerializedProperty GetColumns(SerializedProperty property)
        {
            return property.FindPropertyRelative("columns");
        }

        public static SerializedProperty GetRows(SerializedProperty property)
        {
            return property.FindPropertyRelative("rowData");
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

        public static void InsertRow(SerializedProperty property, int index = -1)
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
                    InitializeHeader(property, newIndex);
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

        public static void ResizeRow(SerializedProperty property, uint newSize)
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
                            InitializeHeader(property, rowProp.arraySize - 1);
                        }
                    }
                }
            }


            property.serializedObject.ApplyModifiedProperties();
//        property.serializedObject.Update();
        }

        private static void InitializeHeader(SerializedProperty property, int rowIndex)
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
            string nameCandidates = $"{property.displayName}_{rowIndex:0000}";
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
            var idCandidates = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue);
            var idEnumrator = headerProps
                .Select(prop => prop.FindPropertyRelative("ID").intValue);
            while (idEnumrator.Any(i => i == idCandidates) && idCandidates > 0)
            {
                idCandidates = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue);
            }

            idProp.intValue = idCandidates;
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

        public static void InsertColumn(
            SerializedProperty property ,
            string name ,
            Type type,
            bool isArray)
        {
            var columns = property.FindPropertyRelative("columns");

            columns.InsertArrayElementAtIndex(columns.arraySize);

            var newColum = columns.GetArrayElementAtIndex(columns.arraySize - 1);

            var dt = MakeColumnData(type , isArray );
            dt.Name = name;
            dt.Resize(GetHeaderRow(property).arraySize);

            newColum.managedReferenceValue = dt;

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
        
        
        public static bool CheckTableSizeChanged(SerializedProperty property, List<int> rows,List<uint> columnList)
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
    }
}