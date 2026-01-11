using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyDataTable
{
    [CreateAssetMenu(fileName = "DataTableAsset", menuName = "Scriptable Objects/DataTableAsset")]
    public class DataTableAsset : ScriptableObject
    {
        [SerializeField] private DataTable data;
        public DataTable Data => data;
    }
}

