using System;

namespace TinyDataTable
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumOrderAttribute : Attribute
    {
        private int order;

        public int Order => order;
        
        // コンストラクタ
        public EnumOrderAttribute( int order ) => this.order = order;
    }
}