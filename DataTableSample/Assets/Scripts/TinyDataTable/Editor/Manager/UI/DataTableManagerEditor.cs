using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TinyDataTable.Editor
{
    public class DataTableManagerEditor : VisualElement
    {
        public static Texture FolderIcon = EditorGUIUtility.IconContent("d_Folder Icon").image;
        public static Texture FolderEmptyIcon = EditorGUIUtility.IconContent( "d_FolderEmpty Icon").image;
        public static Texture FolderOpenIcon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;

        
        public static Texture ItemIcon = EditorGUIUtility.IconContent("d_VerticalLayoutGroup Icon").image;
        
        private DataTableManager manager = null;
        
        public DataTableManagerEditor(DataTableManager manager)
        {
            this.manager = manager;
            CreateGUI();
        }

        private TwoPaneSplitView splitView;
        private VisualElement treeViewRoot;
        private VisualElement tableViewRoot;

        private Toolbar toolbar;
        
        private void CreateGUI()
        {
            var so = new SerializedObject(manager);
            
            toolbar = new Toolbar();
            Add(toolbar);
            
            this.style.flexGrow = 1;
            splitView = new TwoPaneSplitView(
                fixedPaneIndex: 0,
                fixedPaneStartDimension: 200,
                TwoPaneSplitViewOrientation.Horizontal
            );
            splitView.style.flexGrow = 1;
            this.Add(splitView);
            treeViewRoot = new VisualElement();
            tableViewRoot = new VisualElement();

            splitView.Add(treeViewRoot);
            splitView.Add(tableViewRoot);

            var treeView = new SerializableTreeView<DataTableAsset>(manager.Tree);
            treeView.hierarchyChanged += tree =>
            {
                Undo.RecordObject(manager, "Update DataTableManager HierarchyChanged");
                manager.Tree.FromTree(tree);
                EditorUtility.SetDirty(manager);
            };
            treeView.style.flexGrow = 1;
            treeView.Bind(so);
            treeView.TrackSerializedObjectValue(so, a => treeView.BuildTree(manager.Tree) );
            treeView.makeItem = (id,node,isFold,hasChildren) =>
            {
                var root = new VisualElement();
                root.style.flexDirection = FlexDirection.Row;

                var icon = new Image();
                icon.style.width = 16;
                icon.style.height = 16;
                root.Add(icon);

                if (node.IsFolder)
                {
                    icon.image = isFold ? (hasChildren ? FolderIcon:FolderEmptyIcon) : FolderOpenIcon;
                    var textField = new TextField();
                    var inputElement = textField.Q("unity-text-input");
                    if (inputElement != null)
                    {
                        inputElement.style.borderTopWidth = 0;
                        inputElement.style.borderBottomWidth = 0;
                        inputElement.style.borderLeftWidth = 0;
                        inputElement.style.borderRightWidth = 0;
    
                        // 背景も透明にしたい場合
                        inputElement.style.backgroundColor = Color.clear;
                    }                    
                    textField.value = node.Name;
                    textField.RegisterCallback<FocusOutEvent>(evt =>
                    {
                        if (node.Name != textField.value)
                        {
                            treeView.TreeNameChange(id, textField.value);
                        }
                    });
                    root.Add(textField);
                }
                else
                {
                    icon.image = ItemIcon;
                    var label = new Label();
                    label.text = node.Name;
                    root.Add(label);
                }

                return root;
            };
            treeView.onCreateItem = (Position, func) =>
            {
                var popup = new DataTableCreateTablePopup(manager.DefaultNamespace)
                {
                    clickCreateButton = className =>
                    {
                        var tableAsset = CreateDataTableAsset(className);
                        func(className,tableAsset);
                    }
                };
                UnityEditor.PopupWindow.Show(Position, popup);                    
            };
            
            treeViewRoot.Add(treeView);
            tableViewRoot.Add( new Label("DataTableManager"));
        }

        DataTableAsset CreateDataTableAsset(string name)
        {
            var dataTableAsset = ScriptableObject.CreateInstance<DataTableAsset>();
            
            if (!System.IO.Directory.Exists(manager.TablesPath))
            {
                System.IO.Directory.CreateDirectory(manager.TablesPath);
            
                // Unity側にフォルダが作成されたことを認識させる
                AssetDatabase.Refresh();
            }
            
            AssetDatabase.CreateAsset(dataTableAsset, $"{manager.TablesPath}\\{name}.asset");
            AssetDatabase.SaveAssets();            
            
            return dataTableAsset;
        }
    }
}