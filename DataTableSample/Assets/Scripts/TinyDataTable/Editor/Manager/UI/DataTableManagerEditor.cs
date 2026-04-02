using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TinyDataTable.Editor
{
    public class DataTableManagerEditor : VisualElement
    {
        private DataTableManager manager = null;
        
        public DataTableManagerEditor(DataTableManager manager)
        {
            this.manager = manager;
            CreateGUI();
        }

        private TwoPaneSplitView splitView;
        private VisualElement treeView;
        private VisualElement tableView;

        private Toolbar toolbar;
        
        private void CreateGUI()
        {
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
            treeView = new VisualElement();
            tableView = new VisualElement();

            splitView.Add(treeView);
            splitView.Add(tableView);            
            
            treeView.Add( new Label("DataTableManager"));
            tableView.Add( new Label("DataTableManager"));
        }
    }
}