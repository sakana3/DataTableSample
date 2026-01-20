using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TinyDataTable.Editor
{
    public partial class DataSheetField
    {

        private ContextualMenuManipulator MakeColumHeaderManipulator(SerializedProperty property,VisualElement element,int index)
        {
            var manipulator = new ContextualMenuManipulator((evt) =>
            {
                // メニュー項目を追加
                evt.menu.AppendAction(
                    "Add Field",
                    (action) =>
                    {
                        OpenAddFieldPopup(property,index, action.eventInfo.mousePosition);
                    });
                if (index > 0)
                {
                    evt.menu.AppendAction(
                        "Obsolete Field",
                        (action) =>
                        {

                        },
                        (action) =>
                        {
                            return true ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
                        });
                    evt.menu.AppendAction(
                        "Remove Field",
                        (action) =>
                        {
                        },
                        (action) =>
                        {
                            return true ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                        });
                    evt.menu.AppendSeparator();
                }
            });  
            return manipulator;
        }
        
        
        private void OpenAddFieldPopup( SerializedProperty property, int index ,Vector2 mousePos)
        {
            Rect activatorRect = new Rect(mousePos.x, mousePos.y, 0, 0);
   
            var names = DataSheetPropertyUtility.MakeNameList(property);
            DataTableAddPropertyPopup.Show(
                activatorRect,
                names.fieldNames,
                names.recordNames, 
                DataTablePropertyUtil.ReservWords,
                (type, fieldName, isArray,description) =>
                {
                    if (string.IsNullOrEmpty(fieldName) is false)
                    {
                        //エディター専用なのでdynamicで呼んでしまう
                        var sheet = DataSheetPropertyUtility.GetValue(property) as DataSheet;
                        if (sheet != null)
                        {
                            sheet.AddField(type, fieldName, isArray);
                            property.serializedObject.Update();
                            property.serializedObject.ApplyModifiedProperties();

                            var newIndex = sheet.record.Header.fieldInfos.Length;
                            var newColumn = MakePropertyColumn(property, sheet.record.Header.fieldInfos.Length-1);
                            multiColumnListView.columns.Add( newColumn );
                        }
                    }
                });
        }
    }
}