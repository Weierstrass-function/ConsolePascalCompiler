//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ConsolePascalCompiler
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    using System.Collections.Generic;
//    using static System.Formats.Asn1.AsnWriter;
//    using System.Linq.Expressions;
//    using System.Reflection;
//    using static ConsolePascalCompiler.SemanticAnalyzer;

//    public class SymbolTable
//    {
//        private Dictionary<string, SymbolInfo> _symbols = new Dictionary<string, SymbolInfo>();
//        private Stack<Scope> _scopes = new Stack<Scope>();

//        public void AddSymbol(string name, SymbolInfo info)
//        {
//            _symbols[name] = info;
//        }

//        public bool IsDeclaredInCurrentScope(string name)
//        {
//            return _symbols.ContainsKey(name);
//        }

//        // Другие методы...
//    }

//    public class SymbolInfo
//    {
//        public SymbolType Type { get; }
//        public string DataType { get; }

//        public SymbolInfo(SymbolType type, string dataType = null)
//        {
//            Type = type;
//            DataType = dataType;
//        }
//    }


//    public class SemanticAnalyzer
//    {
//        private readonly SymbolTable _symbolTable;

//        public enum SymbolType
//        {
//            Variable,
//            Procedure,
//            BaseType,
//            // Другие типы...
//        }

//        private Stack<Scope> _scopes = new Stack<Scope>();
//        private SymbolTable _symbols = new SymbolTable();

//        // При начале блока (program/procedure)
//        public void EnterScope(string name)
//        {
//            _scopes.Push(new Scope(name));
//        }

//        // При завершении блока
//        public void LeaveScope()
//        {
//            _scopes.Pop();
//        }

//        // Обработка объявления переменной
//        public void ProcessVarDeclaration(string varName, string typeName)
//        {
//            if (_symbols.IsDeclaredInScope(varName, _scopes.Peek()))
//            {
//                //ReportError($"Duplicate variable: {varName}");
//                return;
//            }

//            var type = ResolveType(typeName);
//            _symbols.Add(varName, new Symbol(varName, SymbolKind.Variable, type));
//        }

//        // Обработка присваивания (вызывается при встрече :=)
//        public void ProcessAssignment(string leftVar, IExpression rightExpr)
//        {
//            if (!_symbols.TryLookup(leftVar, out var leftSym))
//            {
//                //ReportError($"Undeclared variable: {leftVar}");
//                return;
//            }

//            var rightType = AnalyzeExpression(rightExpr);
//            if (!TypesAreCompatible(leftSym.Type, rightType))
//            {
//                ReportError($"Type mismatch: {leftSym.Type} != {rightType}");
//            }
//        }

//        private TypeInfo AnalyzeExpression(IExpression expr)
//        {
//            // Рекурсивный анализ выражения без построения AST
//            if (expr is BinaryExpression binOp)
//            {
//                var leftType = AnalyzeExpression(binOp.Left);
//                var rightType = AnalyzeExpression(binOp.Right);
//                return CheckBinaryOperation(binOp.Operator, leftType, rightType);
//            }
//            // ... другие типы выражений
//        }
//    }
//}
