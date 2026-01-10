using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TinyDataTable;
using UnityEditor.UIElements;


namespace TinyDataTable.Editor
{
    [CustomPropertyDrawer(typeof(DataTable))]
    public class DataTablePropertieDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
                
            root.Add( new DataTableGridField(property) );

            return root;
        }
    }
}
