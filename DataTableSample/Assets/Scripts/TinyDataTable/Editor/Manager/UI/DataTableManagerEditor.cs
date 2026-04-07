using System;
using UnityEngine;
using UnityEditor;
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
        private DataTableManagerTreeView treeView;
        
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

            CreateTreeView();

        }

        private void CreateTreeView()
        {
            treeViewRoot.Clear();
            treeView = new DataTableManagerTreeView(manager, true)
            {
                OnSelectDataTableAsset = OnSelectDataTableAsset,
            };
            treeViewRoot.Add(treeView);            
        }
        
        private void OnSelectDataTableAsset(DataTableAsset asset)
        {
            tableViewRoot.Clear();
            if (asset != null)
            {
                var tableOperator = new DataTableManagerTableOperator(manager, asset);
                tableViewRoot.Add(tableOperator);
                
                var tableView = new DataTableManagerTableView(manager, asset, true);
                tableViewRoot.Add(tableView);
            }
        }
    }
}