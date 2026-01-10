using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TinyDataTable;
using UnityEditor.UIElements;

namespace TinyDataTable.Editor
{
    public class DataTableGridField : VisualElement
    {
        private MultiColumnListView multiColumnListView;
        
        private List<int> itemList = new List<int>();
        private List<uint> columnList = new List<uint>();
        
        public DataTableGridField( SerializedProperty property)
        {
            multiColumnListView = MakeMultiColumnListView(property);
            Add( multiColumnListView );
        }

        private MultiColumnListView MakeMultiColumnListView(SerializedProperty property)
        {
            var listView = new MultiColumnListView()
            {
                name = property.displayName,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                
                showAddRemoveFooter = true,
                sortingMode = ColumnSortingMode.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBoundCollectionSize = false,
                showFoldoutHeader = true
            };
            listView.style.overflow = Overflow.Visible; // 通常はHiddenにしてスクロールバーに任せる


            listView.columns.resizePreview = true;
            this.TrackSerializedObjectValue(property.serializedObject, (prop) =>
            {
                if (DataTablePropertyUtil.CheckTableSizeChanged(property,itemList,columnList))
                {
                    this.Clear();
                    multiColumnListView = MakeMultiColumnListView(property);
                    Add( multiColumnListView );
                }
            });
            
            listView.itemsAdded += (indexes) =>
            {
                foreach (var index in indexes)
                {
                    DataTablePropertyUtil.InsertRow(property, index);
                }
            };
            listView.itemsRemoved += (indexes) =>
            {
                foreach (var index in indexes)
                {
                    DataTablePropertyUtil.RemoveRow(property, index);
                }
            };
            listView.itemIndexChanged += (form, to) =>
            {
                DataTablePropertyUtil.MoveRow(property, form,to);
            };
            listView.headerContextMenuPopulateEvent += (evt,clm) =>
            {
                // メニュー項目を追加
                evt.menu.InsertAction(0,"Add Property", (action) =>
                {
                    Vector2 mousePos = action.eventInfo.mousePosition;
    
                    Rect activatorRect = new Rect(mousePos.x, mousePos.y, 0, 0);
       
                    var names = DataTablePropertyUtil.MakeNameList(property);
                    DataTableAddPropertyPopup.Show(
                        activatorRect,
                        names.propNames,
                        names.idNames,
                        new List<string>(){"ID","Invalid","ToString","GetHashCode","GetType","Enum"},
                        (type, name, isArray) => 
                    {
                        if (string.IsNullOrEmpty(name) is false)
                        {
                            DataTablePropertyUtil.InsertColumn(property, name, type, isArray);
                            SetupRows(property, listView);
                            listView.RefreshItems();
                        }
                    });
                });
            };
            
            //フッターにサイズ変更フィールドを追加            
            var footer = listView.Q<VisualElement>("unity-list-view__footer");
            if (footer != null)
            {
                var TableSizeField = new UnsignedIntegerField()
                {
                    value = (uint)DataTablePropertyUtil.GetHeaderRow(property).arraySize,
                };
                footer.Add( TableSizeField );
                TableSizeField.SendToBack();
                TableSizeField.style.marginRight = 4.0f;
                TableSizeField.TrackPropertyValue(DataTablePropertyUtil.GetHeaderRow(property) , (t) =>
                {
                    TableSizeField.SetValueWithoutNotify( (uint)DataTablePropertyUtil.GetHeaderRow(property).arraySize );
                });
                // 編集終了（Enterキー or フォーカス外れ）
                TableSizeField.RegisterCallback<FocusOutEvent>(evt =>
                {
                    DataTablePropertyUtil.ResizeRow(property, TableSizeField.value);
                });
            }
            
            SetupColumns(property, listView);

            SetupRows(property, listView);
            
            return listView;
        }

        private void SetupRows(SerializedProperty property, MultiColumnListView listView)
        {
            var columns = DataTablePropertyUtil.GetColumns(property);  
            var headerProp = columns.GetArrayElementAtIndex(0);
            var rowProp = DataTablePropertyUtil.GetRows(headerProp);

            itemList = Enumerable.Range(0, rowProp.arraySize).Select(i => i).ToList();

            listView.itemsSource = itemList;                    
        }

        private void SetupColumns(SerializedProperty property, MultiColumnListView listView)
        {
            //Make Columns
            var columns = DataTablePropertyUtil.GetColumns(property);
            columnList = new List<uint>();
            for (int i = 0; i < columns.arraySize; i++)
            {
                var columProp = columns.GetArrayElementAtIndex(i);
                
                columnList.Add( columProp.contentHash);
                
                if (columProp.displayName == DataTable.HeaderUniqeName)
                {
                    var columIndex = MakeIndexColumn(columProp, i);
                    listView.columns.Add(columIndex);
                    var columName = MakeNameColumn(columProp, i);
                    listView.columns.Add(columName);
                }
                else
                {
                    var colum = MakePropertyColumn(columProp, i);
                    listView.columns.Add(colum);
                }
            }
        }
        
        
        private Column MakeIndexColumn(SerializedProperty property, int index)
        {
            var colum = new Column()
            {
                name = "Index",
                makeHeader = () => MakeColumHeader("Index"),
                makeCell = () => new Label() { },
                bindCell = (e,i) =>
                {
                    var label = e as Label;
                    label.text = $"{i}";
                },
                stretchable = false,
                width = 48    ,
                sortable = false
            };
            return colum;            
        }
        
        private Column MakeNameColumn(SerializedProperty property, int index)
        {
            var rows = DataTablePropertyUtil.GetRows(property);       
            var colum = new Column()
            {
                name = "ID",                
                makeHeader = () => MakeColumHeader("ID"),
                makeCell = () => new VisualElement() { },
                bindCell = (e,i) =>
                {
                    var prop = rows.GetArrayElementAtIndex(i);
                    var nameProp = prop.FindPropertyRelative("Name");
                    var propertyField = new PropertyField(nameProp,"");
                    propertyField.BindProperty(nameProp);
                    e.Clear();
                    e.Add(propertyField);
                },
                unbindCell = (e,i) =>
                {
                    e.Clear();                        
                },
                stretchable = false,
                width = 120
            };
    
            return colum;                        
        }

        private Column MakePropertyColumn(SerializedProperty property, int index)
        {
            var rows = DataTablePropertyUtil.GetRows(property);
            
            var colum = new Column()
            {
                name = property.displayName,                     
                makeHeader = () => MakeColumHeader(property.displayName),
                makeCell = () => new VisualElement() { },
                bindCell = (e,i) =>
                {
                    if (i < rows.arraySize)
                    {
                        var prop = rows.GetArrayElementAtIndex(i);
                        var propertyField = new PropertyField(prop, string.Empty);
                        propertyField.BindProperty(prop);
                        e.Clear();
                        e.Add(propertyField);
                    }
                },
                unbindCell = (e,i) =>
                {
                    e.Clear();                        
                },                
//                stretchable = true,
                resizable = true,
            };
            return colum;
        }
        
        private VisualElement MakeColumHeader(string name)
        {
            var label = new Label(){ text = name };
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.paddingTop = 2.0f;
            label.style.paddingBottom = 2.0f;
            return label;
        }        
    }
}

