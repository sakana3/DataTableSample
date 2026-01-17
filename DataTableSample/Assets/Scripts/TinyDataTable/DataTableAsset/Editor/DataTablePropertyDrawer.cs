using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace TinyDataTable.Editor
{
    [CustomPropertyDrawer(typeof(DataTable))]
    public class DataTablePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var dataTable = new DataTableGridField(property);
            dataTable.style.flexGrow = 1.0f;
            
            root.Add( dataTable );

            return root;
        }
    }
}
