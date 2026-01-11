using System;
using System.Text;
using System.Linq;

namespace TinyDataTable.Editor
{
    public class CSharpCodeBuilder
    {
        private StringBuilder _sb = new StringBuilder();
        private int _indentLevel = 0;
        private const string IndentString = "    "; // 4スペース

        /// <summary>
        /// 現在のインデント文字列を取得
        /// </summary>
        private string CurrentIndent => string.Concat(Enumerable.Repeat(IndentString, _indentLevel));

        /// <summary>
        /// 行を追加する
        /// </summary>
        public CSharpCodeBuilder AppendLine(string line = "")
        {
            if (!string.IsNullOrEmpty(line))
            {
                _sb.AppendLine($"{CurrentIndent}{line}");
            }
            else
            {
                _sb.AppendLine();
            }
            return this;
        }

        /// <summary>
        /// インデントなしで行を追加する（#if ディレクティブなど用）
        /// </summary>
        public CSharpCodeBuilder AppendLineNoIndent(string line)
        {
            _sb.AppendLine(line);
            return this;
        }

        /// <summary>
        /// ブロックを開始する {
        /// </summary>
        public CSharpCodeBuilder BeginBlock(string header = "")
        {
            if (!string.IsNullOrEmpty(header))
            {
                AppendLine(header);
            }
            AppendLine("{");
            _indentLevel++;
            return this;
        }

        /// <summary>
        /// ブロックを終了する }
        /// </summary>
        public CSharpCodeBuilder EndBlock(string footer = "")
        {
            if (_indentLevel > 0) _indentLevel--;
            AppendLine("}" + footer);
            return this;
        }

        /// <summary>
        /// using ステートメントを追加
        /// </summary>
        public CSharpCodeBuilder AddUsing(string namespaceName)
        {
            AppendLine($"using {namespaceName};");
            return this;
        }

        /// <summary>
        /// using アトリビュートを追加
        /// </summary>
        public CSharpCodeBuilder AddAttribute( params string[] attributres )
        {
            AppendLine($"[{string.Join("," ,attributres)}]");
            return this;
        }        
        
        /// <summary>
        /// 名前空間を開始
        /// </summary>
        public CSharpCodeBuilder BeginNamespace(string namespaceName)
        {
            return BeginBlock($"namespace {namespaceName}");
        }

        /// <summary>
        /// クラス定義を開始
        /// </summary>
        public CSharpCodeBuilder BeginClass(string className, string accessModifier = "public", string inherit = null, bool isPartial = false)
        {
            var partialStr = isPartial ? "partial " : "";
            var inheritStr = string.IsNullOrEmpty(inherit) ? "" : $" : {inherit}";
            return BeginBlock($"{accessModifier} {partialStr}class {className}{inheritStr}");
        }

        /// <summary>
        /// クラス定義を開始
        /// </summary>
        public CSharpCodeBuilder BeginStruct(string className, string accessModifier = "public", string inherit = null, bool isPartial = false)
        {
            var partialStr = isPartial ? "partial " : "";
            var inheritStr = string.IsNullOrEmpty(inherit) ? "" : $" : {inherit}";
            return BeginBlock($"{accessModifier} {partialStr}struct {className}{inheritStr}");
        }
        
        /// <summary>
        /// 列挙定義を開始
        /// </summary>
        public CSharpCodeBuilder BeginEnum(string enumName, string accessModifier = "public")
        {
            return BeginBlock($"{accessModifier} enum {enumName}");
        }        

        /// <summary>
        /// メソッド定義を開始
        /// </summary>
        public CSharpCodeBuilder BeginMethod(string returnType, string methodName, string args = "", string accessModifier = "public", bool isStatic = false)
        {
            var staticStr = isStatic ? "static " : "";
            return BeginBlock($"{accessModifier} {staticStr}{returnType} {methodName}({args})");
        }

        /// <summary>
        /// プロパティを追加
        /// </summary>
        public CSharpCodeBuilder AddProperty(string type, string name, string accessModifier = "public", bool hasSet = true)
        {
            var setStr = hasSet ? " set;" : "";
            AppendLine($"{accessModifier} {type} {name} {{ get;{setStr} }}");
            return this;
        }

        /// <summary>
        /// フィールドを追加
        /// </summary>
        public CSharpCodeBuilder AddField(string type, string name, string accessModifier = "public", string initialValue = null)
        {
            var initStr = initialValue != null ? $" = {initialValue}" : "";
            AppendLine($"{accessModifier} {type} {name}{initStr};");
            return this;
        }

        /// <summary>
        /// Enumを追加
        /// </summary>
        public CSharpCodeBuilder AddEnums( params (string Name , int Value , string Comment )[] members )
        {
            var maxLength = members.Max( m => m.Name.Length );
            
            foreach (var member in members)
            {
                if (string.IsNullOrEmpty(member.Comment))
                {
                    AppendLine($"{member.Name.PadRight(maxLength)} = {member.Value.ToString()},");
                }
                else
                {
                    AppendLine($"{member.Name.PadRight(maxLength)} = {(member.Value.ToString()+",").PadRight(10)} // {member.Comment}");
                }
            }
            return this;
        }
        
        /// <summary>
        /// コメントを追加
        /// </summary>
        public CSharpCodeBuilder AddComment(string comment)
        {
            AppendLine($"// {comment}");
            return this;
        }

        /// <summary>
        /// XMLドキュメントコメントを追加
        /// </summary>
        public CSharpCodeBuilder AddSummary( params string[] summarys)
        {
            AppendLine("/// <summary>");
            foreach (var summary in summarys)
            {
                var lines = summary.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    AppendLine($"/// {line}");
                }
            }
            AppendLine("/// </summary>");
            return this;
        }

        public CSharpCodeBuilder AddSingleLineSummary( string summary)
        {
            AppendLine($"/// <summary> {summary} </summary>");
            return this;
        }

        /// <summary>
        /// 生成されたコードを取得
        /// </summary>
        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
