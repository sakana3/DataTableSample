using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TinyDataTable.Editor
{
    public partial class DataSheetField : VisualElement
    {
        private SerializedProperty _property = null;
        private MultiColumnListView _multiColumnListView;
        private static Color _obsoleteColor = new Color( Color.darkViolet.r,Color.darkViolet.g,Color.darkViolet.b , 0.25f );
        private List<TextField> idTextFieldList = new List<TextField>();
        private List<int> rowIDList = new List<int>();
        private List<int> columnIDList = new List<int>();

        private ( List<string> fieldNames, List<string> recordNames ) _names = (null,null);

        
        public DataSheetField(SerializedProperty property)
        {
            _property = property;
            // 拡張子 (.uss) を含めて指定します
            var styleSheet = EditorGUIUtility.Load("TinyDataTableMultiColumListViewStyle.uss") as StyleSheet;
            if (styleSheet != null)
            {
                this.styleSheets.Add(styleSheet);
            }

  
            Add(new Label("DataSheet"));
            _multiColumnListView = CreateListView(property);
            Add(_multiColumnListView);
        }

        public MultiColumnListView CreateListView(SerializedProperty property)
        {
            idTextFieldList.Clear();
            
            var listView = new MultiColumnListView()
            {
                name = property.displayName,
                reorderable = true,
                reorderMode = ListViewReorderMode.Simple, //AnimatedにするとdragAndDropUpdateが来ない

                showAddRemoveFooter = true,
                sortingMode = ColumnSortingMode.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBoundCollectionSize = false,
                showFoldoutHeader = true,
                selectionType = SelectionType.Multiple
            };
            listView.columns.reorderable = false;
            listView.columns.resizePreview = true;
            listView.columns.resizable = true;
            listView.style.overflow = Overflow.Visible; // 通常はHiddenにしてスクロールバーに任せる
            
            listView.itemsAdded += (indexes) =>
            {
                foreach (var index in indexes)
                {
                    DataSheetPropertyUtility.AddRow(property,index);
                }
            };
            listView.itemsRemoved += (indexes) =>
            {
                foreach (var index in indexes.OrderByDescending(i => i))
                {
                    DataSheetPropertyUtility.RemoveRow(property,index);
                }
            };
            listView.itemIndexChanged += (form, to) =>
            {
                DataSheetPropertyUtility.MoveRow(property,form,to);
            };
            listView.canStartDrag += args => args.id is not 0;
            listView.dragAndDropUpdate += (args) => (args.insertAtIndex is 0) ?
                DragVisualMode.Rejected : DragVisualMode.Move;
            listView.columnSortingChanged += () =>
            {
                Debug.Log("columnSortingChanged");
            };
    
            this.TrackSerializedObjectValue(property.serializedObject, (prop) =>
            {
                var columnChange = DataSheetPropertyUtility.CheckColums(property, columnIDList);
                if (columnChange is false)
                {
                    SetupColumns(property, listView);
                }
                var rowChange = DataSheetPropertyUtility.CheckRows(property, rowIDList);
                if ((columnChange && rowChange) is false)
                {
                    SetupRows(property, listView);                    
                    _multiColumnListView.Rebuild();
                }
            });
            
            SetupColumns(property, listView);

            SetupRows(property, listView);
            
            return listView;
        }


        /// <summary>
        /// 行をセットアップする
        /// </summary>
        /// <param name="property"></param>
        /// <param name="listView"></param>
        private void SetupRows(SerializedProperty property, MultiColumnListView listView)
        {
            rowIDList = DataSheetPropertyUtility.MakeRowIDList(property);

            listView.itemsSource = rowIDList;                    
        }        
        
        /// <summary>
        /// 列をセットアップする
        /// </summary>
        /// <param name="property"></param>
        /// <param name="listView"></param>
        private void SetupColumns(SerializedProperty property, MultiColumnListView listView)
        {
            //Make Columns
            columnIDList.Clear();
            //Clearを呼ぶと何故かコールバックが呼ばれるので潰してから呼ぶ。どう考えてもバグ
            foreach (var column in listView.columns)
            {
                column.makeHeader = null;
                column.bindCell = null;
                column.makeCell = null;
            }
            listView.columns.Clear();

            var indexColumn = MakeIndexColumn(property);
            listView.columns.Add(indexColumn);
            
            var recordNameColumn = MakeIDNameColumn(property);
            listView.columns.Add(recordNameColumn);
            
            var columnsCount = DataSheetPropertyUtility.GetColumnCount(property);
            for (int i = 0; i < columnsCount; i++)
            {
                var columProp = MakePropertyColumn(property, i);
                listView.columns.Add(columProp);
            }

            var lastColumn = MakeLastColumn(property);
            listView.columns.Add(lastColumn);
            
        }        
     
     
        private Column MakeIDNameColumn(SerializedProperty property)
        {
            _names = (null,null);
            var colum = new Column()
            {
                name = "ID",                
                makeHeader = () =>
                {
                    var header = MakeColumHeader(property, "ID",  false, "ID");
//                    var manipulator = MakeMenuManipulator(property,header ,-1);
//                    header.AddManipulator( manipulator);
                    return header;
                },
                makeCell = () =>
                {
                    var e = new VisualElement();
                    e.style.flexGrow = 1.0f;
                    var textField = new TextField() { };
                    var inputElement = textField.Q(className: "unity-text-field__input");
                    if (inputElement != null)
                    {
                        inputElement.style.backgroundColor = Color.clear;
                        inputElement.style.borderTopWidth = 0;
                        inputElement.style.borderBottomWidth = 0;
                        inputElement.style.borderLeftWidth = 0;
                        inputElement.style.borderRightWidth = 0;
                    }                    
                    e.Add(textField);
                    return e;
                },
                bindCell = (e,iRow) =>
                {
                    var textField = e.Q<TextField>();
                    if( textField != null )
                    {
                        var nameProperty = DataSheetPropertyUtility.GetRowNameProperty(property,iRow);
                        textField.BindProperty(nameProperty);
                        textField.RegisterValueChangedCallback(evt =>
                        {
                            ReloadIDText();
                        });
                        e.userData = nameProperty;
                        ReloadIDText(textField);
                        textField.SetEnabled(iRow > 0);
                        idTextFieldList.Add(textField);
                    }
                    var isObsolete = DataSheetPropertyUtility.RowObsolete(property, iRow).boolValue;
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
                width = 120,
            };
    
            return colum;                        
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
                bindCell = (e,iRow) =>
                {
                    if (iRow > 0)
                    {
                        var label = new Label();
                        label.text =$"{iRow - 1}";
                        label.style.unityTextAlign = TextAnchor.MiddleCenter;
                        label.AddManipulator( MakeRowIndexManipulator(property,label,iRow) );
                        var isObsolete = DataSheetPropertyUtility.RowObsolete(property, iRow).boolValue;
                        e.style.backgroundColor = isObsolete?_obsoleteColor:new StyleColor();                        
                        e.Clear();
                        e.Add(label);
                    }
                    else
                    {
                        e.AddManipulator( MakeRowIndexManipulator(property,e,iRow) );
                    }
                    e.parent.style.justifyContent = Justify.Center;
                },
                stretchable = false,
                resizable = false,
                width = 40    ,
                maxWidth = 40,
            };
            return colum;            
        }

        private Column MakePropertyColumn(SerializedProperty property, int iColum )
        {
            Column colum = new Column()
            {
                makeHeader = () =>
                {
                    var (title,id,description,isObsolete) = DataSheetPropertyUtility.GetColumn(property,iColum);
                    var header = MakeColumHeader(property, title, isObsolete,description) as Label;
                    var manipulator = MakeColumHeaderManipulator(property,header ,iColum);
                    header.AddManipulator( manipulator);
                    columnIDList.Add( id);
                    return header;
                },
                makeCell = () => new VisualElement() { },
                bindCell = (e,iRow) =>
                {
                    var isObsoleteCol = DataSheetPropertyUtility.ColumObsolete(property,iColum).boolValue;
                    var isObsoleteRow = DataSheetPropertyUtility.RowObsolete(property,iRow).boolValue;
                    e.style.flexGrow = 1.0f;
                    e.style.backgroundColor = (isObsoleteCol|isObsoleteRow)?_obsoleteColor:new StyleColor();

                    e.Clear();
                    var prop = DataSheetPropertyUtility.GetCellProperty(property,iColum,iRow);
                    var propertyField = new PropertyField(prop, string.Empty);
                    propertyField.BindProperty(prop);
                    propertyField.RegisterValueChangeCallback((evt) =>
                    {
                    } );
                    e.Add(propertyField);
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

        private static Texture2D plusTex = (Texture2D)EditorGUIUtility.IconContent("Toolbar Plus").image;        
        private Column MakeLastColumn(SerializedProperty property)
        {
            Column colum = new Column()
            {
                stretchable = false,
                resizable = false,
                width = 32,
                maxWidth = 32,
//                minWidth = 42,
                makeHeader = () =>
                {
                    var button = new Image();
                    button.image = plusTex;
                    button.RegisterCallback<MouseDownEvent>((t) =>
                    {
                        if (t.button == 0)
                        {
                            OpenAddFieldPopup(property, -1, button.worldBound);
                        }
                    });

                    return button;
                },
                makeCell = () => new VisualElement() { },             
                optional = true,
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

        /// <summary>
        /// IDのテキストを更新する
        /// </summary>
        private void ReloadIDText()
        {
            _names = DataSheetPropertyUtility.MakeNameList(_property);
            foreach (var textField in idTextFieldList)
            {
                ReloadIDText(textField);
            }
        }

        /// <summary>
        /// 指定フィールドのテキストを更新する
        /// </summary>
        private void ReloadIDText( TextField textField )
        {
            if (_names.fieldNames == null || _names.recordNames == null)
            {
                _names = DataSheetPropertyUtility.MakeNameList(_property);
            }

            var value = textField.value;
            
            var input = textField.Q(className: "unity-text-field__input");
            if (input != null)
            {
                if ( string.IsNullOrEmpty(textField.value) )
                {
                    input.style.color = StyleKeyword.Null;
                    textField.tooltip = "Input ID name";                    
                }
                else if (DataSheetPropertyUtility.CheckCSharpSafeName(textField.value) is false)
                {
                    input.style.color = Color.red;
                    textField.tooltip = "Invalid C# identifier.";
                }
                else if (_names.recordNames.Count( t => t == value ) >= 2 ||
                         _names.fieldNames.Contains(  value ) )
                {
                    input.style.color = Color.yellow;
                    textField.tooltip = "This name is conflict.";
                }
                else
                {
                    input.style.color = StyleKeyword.Null;
                    textField.tooltip = string.Empty;
                }
            }
        }        
        
    }
}