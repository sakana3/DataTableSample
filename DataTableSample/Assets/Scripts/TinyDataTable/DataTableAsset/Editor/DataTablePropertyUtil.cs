using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TinyDataTable;

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
    
    public static void InsertRow(SerializedProperty property ,int index = -1)
    {
        var columns = property.FindPropertyRelative("columns");
        for (int col = 0; col < columns.arraySize; col++)
        {
            var columProp = columns.GetArrayElementAtIndex(col);
            var rowProp = columProp.FindPropertyRelative("rowData");     
            rowProp.InsertArrayElementAtIndex( index >= 0 ? index : rowProp.arraySize);
            if (columns.displayName == DataTable.HeaderUniqeName)
            {
                var newHeader = rowProp.GetArrayElementAtIndex(rowProp.arraySize - 1);
            }
        }
        
        property.serializedObject.ApplyModifiedProperties();
 //       property.serializedObject.Update();
    }
    
    public static void RemoveRow(SerializedProperty property ,int index = -1)
    {
        var columns = property.FindPropertyRelative("columns");
        for (int col = 0; col < columns.arraySize; col++)
        {
            var columProp = columns.GetArrayElementAtIndex(col);
            var rowProp = columProp.FindPropertyRelative("rowData"); 
            rowProp.DeleteArrayElementAtIndex( index >= 0 ? index : rowProp.arraySize);
        }
        
        property.serializedObject.ApplyModifiedProperties();
//        property.serializedObject.Update();
    }

    public static void ResizeRow(SerializedProperty property , uint newSize )
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
                    rowProp.DeleteArrayElementAtIndex( rowProp.arraySize-1);
                }
                else
                {
                    rowProp.InsertArrayElementAtIndex( rowProp.arraySize);
                }
            }
        }
        
        property.serializedObject.ApplyModifiedProperties();
//        property.serializedObject.Update();
    }    
    
    public static void MoveRow(SerializedProperty property ,int from , int to)
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
    
    
    public static bool CheckTableSizeChanged(SerializedProperty property,List<int> rows )
    {
        var header = GetHeaderRow(property);
        if(header.arraySize != rows.Count)
        {
            return true;
        }
        return false;
    }
}
