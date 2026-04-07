using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TinyDataTable.Editor
{
    public class DataTableManagerTableOperator : VisualElement
    {
        private DataTableManager manager = null;
        private DataTableAsset asset = null;
        
        private static Texture2D BuildIcon = EditorGUIUtility.IconContent("KnobCShape").image as Texture2D;

        public DataTableManagerTableOperator(DataTableManager manager, DataTableAsset asset)
        {
            this.manager = manager;
            this.asset = asset;
            CreateGUI();
        }

        private void CreateGUI()
        {
            var so = new SerializedObject(asset);

            var propGroup = new VisualElement();
            propGroup.style.flexDirection = FlexDirection.Row;
            Add(propGroup);
            
            var scriptProp = so.FindProperty("classScript");
            if (scriptProp.objectReferenceValue != null)
            {
                var root = new VisualElement();
                root.style.flexGrow = 1;
                root.Bind(so);
                propGroup.Add(root);                
                
                var assetField = new ObjectField("Target");
                assetField.objectType = typeof(DataTableAsset);
                assetField.value = asset;
                assetField.SetEnabled(false);
                root.Add(assetField);                
                
                var typeNameField = new PropertyField(so.FindProperty("classType"));
                typeNameField.SetEnabled(false);
                root.Add(typeNameField);            
                
                var classField = new PropertyField(scriptProp);
                classField.SetEnabled(false);
                root.Add(classField);
            }
            
            var exportButton = new Button()
            {
                text = asset.classScript == null ? "Prepare the script" : "Reload the script",
            };
            exportButton.iconImage = new Background(BuildIcon);
            exportButton.clicked += () =>
            {
                SaveDataTable.CheckNeedEnsureAddressable(asset,true);
                
                SaveDataTable.SaveScript(
                    asset,
                    asset.name,
                    manager.DefaultNamespace,
                    manager.ScriptsPath);
            };
            propGroup.Add(exportButton);
        }

    }
}