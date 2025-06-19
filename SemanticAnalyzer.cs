using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsolePascalCompiler
{
    using System.Collections.Generic;
    using static System.Formats.Asn1.AsnWriter;
    using System.Linq.Expressions;
    using System.Reflection;
    //using static ConsolePascalCompiler.SemanticAnalyzer;
    using static ConsolePascalCompiler.IdentTable.IdentDescription;
    using Compiler;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics.Metrics;

    //public class SymbolTable
    //{        
    //    private Dictionary<string, SymbolInfo> _symbols = new Dictionary<string, SymbolInfo>();
    //    private Stack<Scope> _scopes = new Stack<Scope>();

    //    public void AddSymbol(string name, SymbolInfo info)
    //    {
    //        _symbols[name] = info;
    //    }

    //    public bool IsDeclaredInCurrentScope(string name)
    //    {
    //        return _symbols.ContainsKey(name);
    //    }

    //    // Другие методы...
    //}

    class IdentTable
    {
        public class IdentDescription
        {
            public enum Purpose
            {
                Programm,
                Type,
                Constant,
                Variable,
                Procedure,
                Function
            }

            public Purpose Type { get; }
            public string? DataType { get; }

            public IdentDescription(Purpose type, string? dataType = null)
            {
                Type = type;

                if (dataType != null)
                {
                    // поиск

                    // выводы ошибок
                }

                DataType = dataType;
            }
        }

        private Stack<Dictionary<string, IdentDescription>> _scopes;

        bool IsKeyExists(string key)
        {
            foreach (var dict in _scopes)
            {
                if (dict.ContainsKey(key))
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(string name, Purpose type, string? dataType = null)
        {
            if (IsKeyExists(name))
            {
                InputOutput.Error(1001, InputOutput.positionNow);
            }
            else
            {
                _scopes.Peek().Add(name, new(type, dataType));
            }
        }

        public IdentTable()
        {
            Dictionary<string, IdentDescription> glob = new()
            {
                {"Real", new(Purpose.Type) },
                {"Boolean", new(Purpose.Type) },
                {"Integer", new(Purpose.Type) },
                {"Char", new(Purpose.Type) }
            };

            _scopes.Push(glob);
        }

        public void PushScope()
        {
            _scopes.Push(new());
        }

        public void PopScope()
        {
            _scopes.Pop();
        }
    }

    


    //public class SemanticAnalyzer
    //{
    //    private readonly SymbolTable _symbolTable;


    //    private Stack<Scope> _scopes = new Stack<Scope>();
    //    private SymbolTable _symbols = new SymbolTable();

    //    // При начале блока (program/procedure)
    //    public void EnterScope(string name)
    //    {
    //        _scopes.Push(new Scope(name));
    //    }

    //    // При завершении блока
    //    public void LeaveScope()
    //    {
    //        _scopes.Pop();
    //    }

    //    // Обработка объявления переменной
    //    public void ProcessVarDeclaration(string varName, string typeName)
    //    {
    //        if (_symbols.IsDeclaredInScope(varName, _scopes.Peek()))
    //        {
    //            //ReportError($"Duplicate variable: {varName}");
    //            return;
    //        }

    //        var type = ResolveType(typeName);
    //        _symbols.Add(varName, new Symbol(varName, SymbolKind.Variable, type));
    //    }

    //    //// Обработка присваивания (вызывается при встрече :=)
    //    //public void ProcessAssignment(string leftVar, IExpression rightExpr)
    //    //{
    //    //    if (!_symbols.TryLookup(leftVar, out var leftSym))
    //    //    {
    //    //        //ReportError($"Undeclared variable: {leftVar}");
    //    //        return;
    //    //    }

    //    //    var rightType = AnalyzeExpression(rightExpr);
    //    //    if (!TypesAreCompatible(leftSym.Type, rightType))
    //    //    {
    //    //        ReportError($"Type mismatch: {leftSym.Type} != {rightType}");
    //    //    }
    //    //}

    //    //private TypeInfo AnalyzeExpression(IExpression expr)
    //    //{
    //    //    // Рекурсивный анализ выражения без построения AST
    //    //    if (expr is BinaryExpression binOp)
    //    //    {
    //    //        var leftType = AnalyzeExpression(binOp.Left);
    //    //        var rightType = AnalyzeExpression(binOp.Right);
    //    //        return CheckBinaryOperation(binOp.Operator, leftType, rightType);
    //    //    }
    //    //    // ... другие типы выражений
    //    //}
    //}
}
