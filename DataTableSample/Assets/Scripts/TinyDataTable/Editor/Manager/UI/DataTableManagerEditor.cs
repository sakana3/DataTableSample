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

        public enum Mode
        {
            Edit ,
            Structure ,
            Preferences,
            Addressable,
        }

        private string[] ModeStr = new[]
        {
            "Edit Mode","Structure Mode","Preferences","Addressable"
        };
        
        public static Texture ItemIcon = EditorGUIUtility.IconContent("d_VerticalLayoutGroup Icon").image;
        
        private DataTableManager manager = null;

        public Mode mode
        {
            private set => EditorPrefs.SetInt("DataTableManagerEditorMode", (int)value);
            get => (Mode)EditorPrefs.GetInt("DataTableManagerEditorMode");
        }

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
        private bool isStructureMode => mode == Mode.Structure;

        private void CreateGUI()
        {
            var so = new SerializedObject(manager);

            toolbar = new Toolbar();
            Add(toolbar);

            var modeMenu = new ToolbarMenu()
            {
                text = ModeStr[(int)mode],
                tooltip = "Change Mode",
            };
            modeMenu.style.width = 120;
            modeMenu.menu.AppendAction("Edit Mode",
                action =>
                {
                    modeMenu.text = action.name;
                    mode = Mode.Edit;
                    CreateTreeView();
                },
                a => mode == Mode.Edit ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal
            );            
            modeMenu.menu.AppendAction("Structure Mode",
                action =>
                {
                    modeMenu.text = action.name;
                    mode = Mode.Structure;
                    CreateTreeView();
                },
                a => mode == Mode.Structure ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal
            );
            toolbar.Add(modeMenu);

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
            treeView = new DataTableManagerTreeView(manager, isStructureMode)
            {
                OnSelectDataTableAsset = OnSelectDataTableAsset,
            };
            treeView.style.flexGrow = 1;
            treeViewRoot.Add(treeView);            
        }
        
        private void OnSelectDataTableAsset(DataTableAsset asset)
        {
            tableViewRoot.Clear();
            if (asset != null)
            {
                if (isStructureMode)
                {
                    var tableOperator = new DataTableManagerTableOperator(manager, asset);
                    tableViewRoot.Add(tableOperator);
                }

                var tableView = new DataTableManagerTableView(manager, asset, isStructureMode);
                tableView.style.flexGrow = 1;
                tableViewRoot.Add(tableView);
            }
        }
    }
}