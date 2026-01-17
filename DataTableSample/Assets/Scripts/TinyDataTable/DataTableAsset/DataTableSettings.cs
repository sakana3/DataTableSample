using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace TinyDataTable
{
    [Serializable]
    public struct DataTableSettings
    {
        [Serializable]
        public enum EditMode
        {
            Edit,
            Layout,
        }

        [SerializeField]
        public EditMode editMode;
        
        [SerializeField]
        public string className;

        [SerializeField]
        public string namespaceName;

        [SerializeField]
        public string classType;

        [SerializeField]
        public MonoScript classScript;

    }
}