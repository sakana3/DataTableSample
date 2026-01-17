using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TinyDataTable.Editor
{
    public class DataTableGridField : VisualElement
    {
        private MultiColumnListView multiColumnListView;
        private SerializedProperty _targetProperty;
        
        private List<int> itemList = new List<int>();
        private List<int> columnList = new List<int>();
        private List<TextField> idTextFieldList = new List<TextField>();

        public string Name { private set; get; }
        
        private Color _obsoleteColor = new Color( Color.darkViolet.r,Color.darkViolet.g,Color.darkViolet.b , 0.25f );

        
        public DataTableGridField( string name , SerializedProperty property)
        {
            Name = name;
            _targetProperty = property;
            multiColumnListView = MakeMultiColumnListView(property);
            Add( multiColumnListView );
        }

        public DataTableGridField( SerializedProperty property) : this(property.name,property)
        {
        }
        
        private MultiColumnListView MakeMultiColumnListView(SerializedProperty property)
        {
            var listView = new MultiColumnListView()
            {
                name = property.displayName,
                reorderable = true,
                reorderMode = ListViewReorderMode.Simple,   //AnimatedにするとdragAndDropUpdateが来ない
                
                showAddRemoveFooter = true,
                sortingMode = ColumnSortingMode.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBoundCollectionSize = false,
                showFoldoutHeader = true,
            };
            listView.columns.reorderable = false;
            listView.columns.resizePreview = true;
            listView.columns.resizable = true;
   
            listView.style.overflow = Overflow.Visible; // 通常はHiddenにしてスクロールバーに任せる

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
                    DataTablePropertyUtil.InsertRow(property,Name, index);
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
            listView.canStartDrag += args => args.id is not 0;
            listView.dragAndDropUpdate += (args) => (args.insertAtIndex == 0) ? DragVisualMode.Rejected : DragVisualMode.Move;
            
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
                    DataTablePropertyUtil.ResizeRow(property,Name, TableSizeField.value);
                });
            }
            
            SetupColumns(property, listView);

            SetupRows(property, listView);
            
            return listView;
        }

        private void SetupRows(SerializedProperty property, MultiColumnListView listView)
        {
            var rowProp = DataTablePropertyUtil.GetHeaderRow(property);

            itemList = Enumerable.Range(0, rowProp.arraySize).Select(i => i).ToList();

            listView.itemsSource = itemList;                    
        }

        private void SetupColumns(SerializedProperty property, MultiColumnListView listView)
        {
            //Make Columns
            listView.columns.Clear();
            idTextFieldList.Clear();
            var columns = DataTablePropertyUtil.GetColumns(property);
            columnList = new List<int>();

            var columIndex = MakeIndexColumn(property);
            listView.columns.Add(columIndex);
            var columName = MakeIDNameColumn(property);
            listView.columns.Add(columName);
            
            for (int i = 0; i < columns.arraySize; i++)
            {
                var columProp = columns.GetArrayElementAtIndex(i);
                columnList.Add( columProp.FindPropertyRelative("id").intValue);
                var colum = MakePropertyColumn(property, i);
                listView.columns.Add(colum);
            }
        }
        
        
        private Column MakeIndexColumn(SerializedProperty property)
        {
            var colum = new Column()
            {
                name = "Index",
                makeHeader = () => MakeColumHeader(property,"Index",false,"Index"),
                makeCell = () =>
                {
                    var e= new VisualElement() { };
                    e.style.flexGrow = 1.0f;
                    return e;
                },
                bindCell = (e,i) =>
                {
                    if (i > 0)
                    {
                        var label = new Label();
                        label.text =$"{i - 1}";
                        label.style.unityTextAlign = TextAnchor.MiddleCenter;
                        label.AddManipulator( MakeMenuIndexManipulator(property,label,i) );
                        var isObsolete = DataTablePropertyUtil.GetHeaderObsolete(property, i).boolValue;
                        e.style.backgroundColor = isObsolete?_obsoleteColor:new StyleColor();                        
                        e.Clear();
                        e.Add(label);
                    }
                    e.parent.style.justifyContent = Justify.Center;
                },
                stretchable = false,
                resizable = false,
                width = 40    ,
                sortable = false
            };
            return colum;            
        }
        
        private Column MakeIDNameColumn(SerializedProperty property)
        {
            var colum = new Column()
            {
                name = "ID",                
                makeHeader = () =>
                {
                    var header = MakeColumHeader(property, "ID",  false, "ID");
                    var manipulator = MakeMenuManipulator(property,header ,-1);
                    header.AddManipulator( manipulator);
                    return header;
                },
                makeCell = () =>
                {
                    var e = new VisualElement();
                    e.style.flexGrow = 1.0f;
                    var t = new TextField() { };
                    idTextFieldList.Add(t);
                    e.Add(t);
                    return e;
                },
                bindCell = (e,i) =>
                {
                    var textField = e.Q<TextField>();
                    if ( textField != null)
                    {
                        var headerProp = DataTablePropertyUtil.GetHeader(property,i);
                        var nameProp = headerProp.FindPropertyRelative("Name");
                        textField.BindProperty(nameProp);
                        textField.RegisterValueChangedCallback(evt => { ReloadIDText(); });
                        e.userData = nameProp;
                        ReloadIDText(textField);
                        textField.SetEnabled(headerProp.FindPropertyRelative("ID").intValue > 0);
                    }
                    var isObsolete = DataTablePropertyUtil.GetHeaderObsolete(property, i).boolValue;
                    e.style.backgroundColor = isObsolete?_obsoleteColor:new StyleColor();                        
                },
                unbindCell = (e,i) =>
                {
                    if( e is TextField textField )
                    {
                        idTextFieldList.Remove(textField);
                    }
                },
                stretchable = false,
                width = 120
            };

    
            return colum;                        
        }
        
        private Column MakePropertyColumn(SerializedProperty property, int index )
        {
            var columProp = DataTablePropertyUtil.GetColumn(property,index);

            var name = columProp.displayName;

            Column colum = new Column()
            {
                name = name,                     
                makeHeader = () =>
                {
                    var columProp = DataTablePropertyUtil.GetColumn(property,index);
                    var isObsolete = columProp.FindPropertyRelative("obsolete").boolValue;
                    var description = columProp.FindPropertyRelative("description").stringValue;
                    var header = MakeColumHeader(property, name, isObsolete,description) as Label;
                    var manipulator = MakeMenuManipulator(property,header ,index);
                    header.AddManipulator( manipulator);
                    return header;
                },
                makeCell = () => new VisualElement() { },
                bindCell = (e,i) =>
                {
                    var columProp = DataTablePropertyUtil.GetColumn(property,index);
                    var isObsoleteCol = columProp.FindPropertyRelative("obsolete").boolValue;
                    var isObsoleteRow = DataTablePropertyUtil.GetHeaderObsolete(property, i).boolValue;

                    e.style.flexGrow = 1.0f;
                    e.style.backgroundColor = (isObsoleteCol|isObsoleteRow)?_obsoleteColor:new StyleColor();

                    var rows = DataTablePropertyUtil.GetRows(columProp);
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
                width = 80,
            };
            return colum;
        }
        
        private VisualElement MakeColumHeader(
            SerializedProperty property ,
            string name ,
            bool isObsolete,
            string description)
        {
            var label = new Label(){ text = name };
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.paddingTop = 2.0f;
            label.style.paddingBottom = 2.0f;
            label.style.backgroundColor = isObsolete?_obsoleteColor:new StyleColor();
            label.tooltip = description;

            return label;
        }

        private ContextualMenuManipulator MakeMenuManipulator(SerializedProperty property,VisualElement element,int index)
        {
            var manipulator = new ContextualMenuManipulator((evt) =>
            {
                // メニュー項目を追加
                evt.menu.AppendAction("Add Field", (action) => OpenProp(index,action.eventInfo.mousePosition));
                if (index > 0)
                {
                    var columProp = DataTablePropertyUtil.GetColumn(property,index);
                    var obsoleteProp = columProp.FindPropertyRelative("obsolete");
                    evt.menu.AppendAction(
                        "Obsolete Field",
                        (action) =>
                        {
                            obsoleteProp.boolValue = !obsoleteProp.boolValue;
                            columProp.serializedObject.ApplyModifiedProperties();
                            element.style.backgroundColor = obsoleteProp.boolValue?_obsoleteColor:new StyleColor();
                            property.serializedObject.ApplyModifiedProperties();
                            property.serializedObject.Update();
                            multiColumnListView.RefreshItems();
                        },
                        (action) =>
                        {
                            var obsolete = columProp.FindPropertyRelative("obsolete").boolValue;
                            return obsolete ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
                        });
                    evt.menu.AppendAction(
                        obsoleteProp.boolValue?"Remove Field":"Remove Field(Obsolete first)",
                        (action) =>
                        {
                            bool result = EditorUtility.DisplayDialog(
                                "Confirmation", // Title
                                $"Deleting this field<{columProp.displayName}> could result in breaking changes. Do you want to continue?", // Message
                                "Yes", // OK button text
                                "No" // Cancel button text
                            );
                            if (result)
                            {
                                DataTablePropertyUtil.RemoveColumn(_targetProperty, index);
                            }
                        },
                        (action) =>
                        {
                            var obsolete = columProp.FindPropertyRelative("obsolete").boolValue;
                            return obsolete ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                        });
                    evt.menu.AppendSeparator();
                }
            });  
            return manipulator;
        }

        private ContextualMenuManipulator MakeMenuIndexManipulator(
            SerializedProperty property,
            VisualElement element,
            int index)
        {
            var manipulator = new ContextualMenuManipulator((evt) =>
            {
                evt.menu.AppendAction("Obsolete Record",
                    (action) =>
                    {
                        var obsoleteProp = DataTablePropertyUtil
                            .GetHeader(property, index)
                            .FindPropertyRelative("Obsolete");
                        obsoleteProp.boolValue = !obsoleteProp.boolValue;
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();    
                        multiColumnListView.RefreshItems();
                    },
                    (action) =>
                    {
                        var obsolete = DataTablePropertyUtil
                            .GetHeader(property,index)
                            .FindPropertyRelative("Obsolete").boolValue;
                        
                        return obsolete ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
                    }
                );
            }
            );
            return manipulator;
        }

        private void ReloadIDText( TextField textField )
        {
            var input = textField.Q(className: "unity-text-field__input");
            if (input != null)
            {
                if ( string.IsNullOrEmpty(textField.value) )
                {
                    input.style.color = Color.white;
                    textField.tooltip = "Input ID name";                    
                }
                else if (UIToolkitEditorUtility.CheckCanUseFieldName(textField.value) is false)
                {
                    input.style.color = Color.red;
                    textField.tooltip = "Invalid C# identifier.";
                }
                else if (DataTablePropertyUtil.ChakeCanUseName(_targetProperty, (SerializedProperty)textField.userData ) is false)
                {
                    input.style.color = Color.yellow;
                    textField.tooltip = "This name is already in use.";
                }
                else
                {
                    input.style.color = Color.white;
                    textField.tooltip = string.Empty;
                }
            }
        }
        
        private void ReloadIDText()
        {
            foreach (var textField in idTextFieldList)
            {
                ReloadIDText(textField);
            }
        }        
        
        private void OpenProp( int index ,Vector2 mousePos)
        {
            Rect activatorRect = new Rect(mousePos.x, mousePos.y, 0, 0);
   
            var names = DataTablePropertyUtil.MakeNameList(_targetProperty);
            DataTableAddPropertyPopup.Show(
                activatorRect,
                names.propNames,
                names.idNames, 
                DataTablePropertyUtil.ReservWords,
                (type, name, isArray,description) => 
                {
                    if (string.IsNullOrEmpty(name) is false)
                    {
                        var newColumnProp = DataTablePropertyUtil.InsertColumn(_targetProperty,index+1, name, type, isArray,description);

                        var newColumn = MakePropertyColumn(_targetProperty, index + 1);
                        multiColumnListView.columns.Insert( index + 3 , newColumn );
                    }
                });
        }
    }
}

