using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace TinyDataTable.Editor
{
    public class DataTableManagerTableView : VisualElement
    {
        private DataTableManager Manager = null;
        private DataTableAsset asset;
        private bool IsEditMode = false;
        
        public DataTableManagerTableView(DataTableManager manager,DataTableAsset asset,bool isEditMode)
        {
            this.Manager = manager;
            this.IsEditMode = isEditMode;
            this.asset = asset;
            CreateGUI();
        }

        private void CreateGUI()
        {
            var property = new SerializedObject(asset)
                .FindProperty("dataSheet");
            
            var sheet = new DataSheetField(property);            
            Add( sheet);            
        }
    }
}