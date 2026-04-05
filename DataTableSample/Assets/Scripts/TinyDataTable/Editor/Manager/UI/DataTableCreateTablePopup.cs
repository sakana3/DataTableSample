using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace TinyDataTable.Editor
{
    public class DataTableCreateTablePopup : PopupWindowContent
    {
        //Set the window size
        public override Vector2 GetWindowSize() => new Vector2(256, 48);

        private TextField textField;
        private Button confirmButton;
        private string namespaceName;

        public Action<string> clickCreateButton;
        
        public DataTableCreateTablePopup(string name)
        {
            namespaceName = name;
        }
        
        public override void OnOpen()
        {
            var root = editorWindow.rootVisualElement;

            textField = new TextField("Table Name");
         
            textField.RegisterValueChangedCallback(evt => OnClassNameChangeCallback(textField,evt));            
            // 少し遅延させてフォーカス
            textField.schedule.Execute(() => 
            {
                textField.Focus();
            }).StartingIn(50); // 50ms後くらい            
            root.Add( textField);

            confirmButton = new Button()
            {
                text = "Create",
            };
            confirmButton.tooltip = "Input table name.";          
            confirmButton.clicked += () =>
            {
                clickCreateButton?.Invoke(textField.value);
                editorWindow.Close();
            };
            root.Add(confirmButton);
            
            confirmButton.SetEnabled( false);
        }
        
        public override void OnClose()
        {
        
        }

        private void OnClassNameChangeCallback(TextField textField, ChangeEvent<string> evt)
        {
            var className = textField.value;
            Type type = Type.GetType($"{namespaceName}.{className}");

            if (string.IsNullOrEmpty(className))
            {
                confirmButton.tooltip = "Input table name.";
                confirmButton.SetEnabled( false);                
            }
            else if (UIToolkitEditorUtility.CheckCanUseFieldName(className) is false )
            {
                confirmButton.tooltip = "Invalid table name.";
                confirmButton.SetEnabled( false);
            }
            else if (type != null)
            {
                confirmButton.tooltip = "This name is already used.";
                confirmButton.SetEnabled( false);              
            }
            else
            {
                confirmButton.tooltip = "Create new table.";                
                confirmButton.SetEnabled( true);
            }
        }

    }
}