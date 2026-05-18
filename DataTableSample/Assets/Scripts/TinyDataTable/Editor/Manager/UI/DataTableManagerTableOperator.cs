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

        private Button exportButton;

        private bool isDirty = false;
        
        public DataTableManagerTableOperator(DataTableManager manager, DataTableAsset asset)
        {
            this.manager = manager;
            this.asset = asset;
            
            isDirty = manager.CheckDirty(asset);            
            
            var so = new SerializedObject(asset);
            CreateGUI(so);

            this.TrackSerializedObjectValue(so, (s) =>
            {
                isDirty = manager.CheckDirty(asset);
                exportButton.style.backgroundColor =isDirty ? new StyleColor(Color.cornflowerBlue) : StyleKeyword.Null;
            });
        }

        private void CreateGUI(SerializedObject so)
        {
            var assetField = new ObjectField();
            assetField.objectType = typeof(DataTableAsset);
            assetField.value = asset;
            assetField.SetEnabled(false);
            MakeMargine(assetField);            
            Add(assetField);          
            
            var addressableElement = new AddressableElement(asset);
            MakeMargine(addressableElement);
            Add(addressableElement);
            
            var propGroup = new VisualElement();
            propGroup.style.flexDirection = FlexDirection.Row;
            MakeMargine(propGroup);            
            Add(propGroup);
            
            var root = new VisualElement();
            root.style.flexGrow = 1;
            root.Bind(so);
            propGroup.Add(root);                

            var obsoleteField = new PropertyField(so.FindProperty("obsolete"));
            root.Add(obsoleteField);
            
            var scriptProp = so.FindProperty("classScript");
            if (scriptProp.objectReferenceValue != null)
            {
                var typeNameField = new PropertyField(so.FindProperty("classType"));
                typeNameField.SetEnabled(false);
                root.Add(typeNameField);            
                
                var classField = new PropertyField(scriptProp);
                classField.SetEnabled(false);
                root.Add(classField);
            }
            
            exportButton = new Button()
            {
                text = asset.classScript == null ? "Repair" : "Rebuild",
            };
            exportButton.iconImage = Background.FromTexture2D(BuildIcon);
            exportButton.clicked += () =>
            {
                SaveDataTable.CheckNeedEnsureAddressable(asset,true);

                                
                var scriptPath = AssetDatabase.GetAssetPath(asset.classScript);
                var scriptDir = System.IO.Path.GetDirectoryName(scriptPath);

                SaveDataTable.SaveScript(
                    asset,
                    asset.classScript.GetClass().Name,
                    manager.DefaultNamespace,
                    scriptDir);
            };

            exportButton.style.backgroundColor = isDirty ? new StyleColor(Color.cornflowerBlue) : StyleKeyword.Null;
            
            propGroup.Add(exportButton);
        }

        public static void MakeMargine(VisualElement ve)
        {
            ve.style.borderBottomColor = Color.gray;
            ve.style.borderBottomWidth = 1;
            ve.style.paddingBottom = 4;
            ve.style.marginBottom = 4;            
        }
    }
}