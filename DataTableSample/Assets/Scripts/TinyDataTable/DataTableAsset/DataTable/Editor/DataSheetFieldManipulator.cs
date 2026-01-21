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
        private ContextualMenuManipulator MakeColumHeaderManipulator(
            SerializedProperty property,
            VisualElement element,
            int index)
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
                    evt.menu.AppendAction(
                        "Obsolete Field",
                        (action) =>
                        {
                            var obsolete = DataSheetPropertyUtility.ColumObsolete(property, index);
                            obsolete.boolValue = !obsolete.boolValue;
                            property.serializedObject.ApplyModifiedProperties();
                            element.style.backgroundColor =  obsolete.boolValue?_obsoleteColor:new StyleColor();
                            multiColumnListView.RefreshItems();
                        },
                        (action) =>
                        {
                            var obsolete = DataSheetPropertyUtility.ColumObsolete(property, index);
                            return obsolete.boolValue ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
                        });
                    evt.menu.AppendAction(
                        "Remove Field",
                        (action) =>
                        {
                        },
                        (action) =>
                        {
                            var obsolete = DataSheetPropertyUtility.ColumObsolete(property, index);
                            return obsolete.boolValue ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                        });
                    evt.menu.AppendSeparator();
            });  
            return manipulator;
        }

        private ContextualMenuManipulator MakeRowIndexManipulator(
            SerializedProperty property,
            VisualElement element,
            int index)
        {
            var manipulator = new ContextualMenuManipulator((evt) =>
            {
                // メニュー項目を追加
                evt.menu.AppendAction(
                    "Add Record",
                    (action) =>
                    {
                    });
                evt.menu.AppendAction(
                    "Obsolete Field",
                    (action) =>
                    {
                        if (multiColumnListView.selectedIndices.Contains(index))
                        {
                            var isObsolete = !DataSheetPropertyUtility.RowObsolete(property, index).boolValue;
                            foreach (var idx in multiColumnListView.selectedIndices)
                            {
                                var obsolete = DataSheetPropertyUtility.RowObsolete(property, idx);
                                obsolete.boolValue = isObsolete;
                            }
                        }
                        property.serializedObject.ApplyModifiedProperties();
                        multiColumnListView.RefreshItems();
                    },
                    (action) =>
                    {
                        var obsolete = DataSheetPropertyUtility.RowObsolete(property, index);
                        return obsolete.boolValue
                            ? DropdownMenuAction.Status.Checked
                            : DropdownMenuAction.Status.Normal;
                    });
                evt.menu.AppendAction(
                    "Remove Record",
                    (action) =>
                    {
                        DataSheetPropertyUtility.RemoveRow(property, index);
                        multiColumnListView.ClearSelection();
                        itemList.RemoveAt(index);
                        multiColumnListView.Rebuild();
                    },
                    (action) =>
                    {
                        var obsolete = DataSheetPropertyUtility.RowObsolete(property, index);
                        return obsolete.boolValue ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    });
                
                evt.menu.AppendSeparator();                
            });
            return manipulator;
        }

        private void OpenAddFieldPopup(SerializedProperty property, int index, Vector2 mousePos)
        {
            OpenAddFieldPopup(property, index, new Rect(mousePos.x, mousePos.y, 0, 0));
        }
        
        private void OpenAddFieldPopup( SerializedProperty property, int index ,Rect activatorRect)
        {
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
                        var sheet = DataSheetPropertyUtility.GetValue(property) as DataSheet;
                        if (sheet != null)
                        {
                            sheet.AddField(type, fieldName, isArray);
                            property.serializedObject.Update();
                            property.serializedObject.ApplyModifiedProperties();

                            var newIndex = sheet.record.Header.fieldInfos.Length;
                            var newColumn = MakePropertyColumn(property, sheet.record.Header.fieldInfos.Length-1);
                            multiColumnListView.columns.Insert( multiColumnListView.columns.Count - 1, newColumn );
                        }
                    }
                });
        }
    }
}