using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TinyDataTable.Editor
{
    public class TypeSelectorDropdown : AdvancedDropdown
    {
        private readonly Action<Type> _onTypeSelected;
        private readonly IEnumerable<Type> _types;

        //Unityの代表的な型
        private static Type[] types = new []
        {
            typeof(int),
            typeof(float),
            typeof(bool),
            typeof(string),
            typeof(long),
            typeof(double),
        };
        private static Type[] builtinTypes = new []
        {
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(Color),
            typeof(Color32),
            typeof(Rect),
            typeof(Bounds),
            typeof(LayerMask),
            typeof(AnimationCurve),
            typeof(Gradient),
        };

        private static Type[] _enumTypes = null;
        private static Type[] _classTypes = null;
        private static Type[] _unityObjectTypes = null;

        public TypeSelectorDropdown(AdvancedDropdownState state, IEnumerable<Type> types, Action<Type> onTypeSelected) : base(state)
        {
            _types = types;
            _onTypeSelected = onTypeSelected;

            if (_enumTypes == null || _classTypes == null || _unityObjectTypes == null)
            {
                (_enumTypes,_classTypes,_unityObjectTypes) = CollectTEmumTypes();
            }
            
            // ウィンドウサイズの最小値を設定
            minimumSize = new Vector2(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Types");

            foreach (var type in types)
            {
                AddTypeItem(root, type,false);
            }            
            
            var typeRoot = new AdvancedDropdownItem("Unity Type");
            foreach (var type in builtinTypes)
            {
                AddTypeItem(typeRoot, type,false);
            }
            root.AddChild(typeRoot);

            var objectRoot = new AdvancedDropdownItem("UnityObject");
            foreach (var type in _unityObjectTypes)
            {
                AddTypeItem(objectRoot, type,true);
            }
            root.AddChild(objectRoot);
      
            var enumRoot = new AdvancedDropdownItem("Enum");
            foreach (var type in _enumTypes)
            {
                AddTypeItem(enumRoot, type,true);
            }
            root.AddChild(enumRoot);

            var classRoot = new AdvancedDropdownItem("Class");
            foreach (var type in _classTypes)
            {
                AddTypeItem(classRoot, type,true);
            }
            root.AddChild(classRoot);
            
            return root;
        }

        private void AddTypeItem( AdvancedDropdownItem root, Type type ,bool isNest)
        {
            // シンプルに型名だけで追加する場合
            // var item = new TypeDropdownItem(type.Name, type);
            // root.AddChild(item);
            
            // 名前空間で階層化する場合
            var parent = root;
            if (isNest)
            {
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    var parts = type.Namespace.Split('.');
                    foreach (var part in parts)
                    {
                        var child = parent.children.FirstOrDefault(c => c.name == part);
                        if (child == null)
                        {
                            child = new AdvancedDropdownItem(part)
                            {
                                icon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D
                            };
                            parent.AddChild(child);
                        }

                        parent = child;
                    }
                }
            }

            var item = new TypeDropdownItem(type.Name, type)
            {
                icon = EditorGUIUtility.ObjectContent(null, type).image as Texture2D // 型のアイコンがあれば設定
            };
            parent.AddChild(item);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeDropdownItem typeItem)
            {
                _onTypeSelected?.Invoke(typeItem.Type);
            }
        }

        // 型情報を保持するためのカスタムアイテムクラス
        private class TypeDropdownItem : AdvancedDropdownItem
        {
            public Type Type { get; }

            public TypeDropdownItem(string name, Type type) : base(name)
            {
                Type = type;
            }
        }

        private (Type[] enumTypes,Type[] classTypes,Type[] unityObjectTypes)  CollectTEmumTypes()
        {
            var types  = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.Contains("Editor") is false )
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsPublic)
                .ToArray();
            
            var enumTypes = types
                .Where(t => t.IsEnum )
                .Where(t => t.IsDefined(typeof(SerializableAttribute), inherit: true))
                .ToArray();

            var classTypes = types
                .Where(t => t.IsClass || t.IsValueType || t.IsPrimitive )
                .Where(t => t.IsDefined(typeof(SerializableAttribute), inherit: true))
                .ToArray();

            var unityObjectTypes = types
                .Where(t => t.IsClass )
                .Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t))
                .ToArray();
            
            return (enumTypes,classTypes,unityObjectTypes);
        }
    }
}

[Serializable]
public enum testenum
{
    A,
    B,
    C,
}