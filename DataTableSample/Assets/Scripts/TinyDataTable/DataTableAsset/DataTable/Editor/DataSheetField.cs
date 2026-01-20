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
        private MultiColumnListView multiColumnListView;
        private static Color _obsoleteColor = new Color( Color.darkViolet.r,Color.darkViolet.g,Color.darkViolet.b , 0.25f );
        private List<TextField> idTextFieldList = new List<TextField>();
        private List<int> itemList = null;
        
        public DataSheetField(SerializedProperty property)
        {
            _property = property;
            
            Add(new Label("DataSheet"));
            multiColumnListView = CreateListView(property);
            Add(multiColumnListView);
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
            };
            listView.columns.reorderable = false;
            listView.columns.resizePreview = true;
            listView.columns.resizable = true;            
            
            listView.style.overflow = Overflow.Visible; // 通常はHiddenにしてスクロールバーに任せる
            
            SetupColumns(property, listView);

            SetupRows(property, listView);
            
            return listView;
        }

         
        private void SetupRows(SerializedProperty property, MultiColumnListView listView)
        {
            var rowCount = DataSheetPropertyUtility.GetRowCount(property);

            itemList = Enumerable.Range(0, rowCount).Select(i => i).ToList();

            listView.itemsSource = itemList;                    
        }        
        
        private void SetupColumns(SerializedProperty property, MultiColumnListView listView)
        {
            //Make Columns
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
        }        
     
     
        private Column MakeIDNameColumn(SerializedProperty property)
        {
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
                    var t = new TextField() { };
                    idTextFieldList.Add(t);
                    e.Add(t);
                    return e;
                },
                bindCell = (e,iRow) =>
                {
                    var textField = e.Q<TextField>();
                    if ( textField != null)
                    {
                        var nameProperty = DataSheetPropertyUtility.GetRowNameProperty(property,iRow);
                        textField.BindProperty(nameProperty);
//                        textField.RegisterValueChangedCallback(evt => { ReloadIDText(); });
                        e.userData = nameProperty;
//                        ReloadIDText(textField);
                        textField.SetEnabled(iRow > 0);
                    }
                    var isObsolete = DataSheetPropertyUtility.IsRowObsolete(property, iRow);
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
//                        label.AddManipulator( MakeMenuIndexManipulator(property,label,i) );
                        var isObsolete = DataSheetPropertyUtility.IsRowObsolete(property, iRow);
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


        private Column MakePropertyColumn(SerializedProperty property, int iColum )
        {
            Column colum = new Column()
            {
                makeHeader = () =>
                {
                    var (title,description,isObsolete) = DataSheetPropertyUtility.GetColumn(property,iColum);
                    var header = MakeColumHeader(property, title, isObsolete,description) as Label;
                    var manipulator = MakeColumHeaderManipulator(property,header ,iColum);
                    header.AddManipulator( manipulator);
                    return header;
                },
                makeCell = () => new VisualElement() { },
                bindCell = (e,iRow) =>
                {
                    var isObsoleteCol = DataSheetPropertyUtility.IsColumObsolete(property,iColum);
                    var isObsoleteRow = DataSheetPropertyUtility.IsRowObsolete(property,iRow);
                    e.style.flexGrow = 1.0f;
                    e.style.backgroundColor = (isObsoleteCol|isObsoleteRow)?_obsoleteColor:new StyleColor();

                    e.Clear();
                    var prop = DataSheetPropertyUtility.GetCellProperty(property,iColum,iRow);
                    var propertyField = new PropertyField(prop, string.Empty);
                    propertyField.BindProperty(prop);
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
    }
}