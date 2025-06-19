using ConsolePascalCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Compiler
{
    class SyntaxAnalyzer
    {
        private LexicalAnalyzer lexer;
        private IdentTable semantic;
        private byte currentSymbol;

        /// <summary>
        /// Обязательный символ
        /// </summary>
        /// <param name="symbol"></param>
        private void Accept(byte expected)
        {
            if (currentSymbol == expected)
            {
                currentSymbol = lexer.NextSym();
            }
            else
            {
                InputOutput.Error(expected, lexer.tokenPos);

                if (currentSymbol == LexicalAnalyzer.eof)
                {
                    InputOutput.ListErrors();
                    InputOutput.End();
                }
            }
        }

        public SyntaxAnalyzer(LexicalAnalyzer lexicalAnalyzer)
        {
            lexer = lexicalAnalyzer;
        }

        public void Analyze()
        {
            currentSymbol = lexer.NextSym();
            Program();       
            Accept(LexicalAnalyzer.eof);

            while (currentSymbol != LexicalAnalyzer.eof)
            {
                currentSymbol = lexer.NextSym();
            }

            InputOutput.End();
        } 

        
        // 'program' <ident> ['(' <ident> {, <ident>} ')' ];' <Block> '.'
        void Program()
        {
            Accept(LexicalAnalyzer.programsy);
            Accept(LexicalAnalyzer.ident);

            // ['(' <ident> {, <ident>} ')' ]
            if (currentSymbol == LexicalAnalyzer.leftpar)
            {
                currentSymbol = lexer.NextSym();
                IdentList();
                Accept(LexicalAnalyzer.rightpar);
            }

            Accept(LexicalAnalyzer.semicolon);
            Block();
            Accept(LexicalAnalyzer.point);
        }

        // <какой-то символ> <ident> {, <ident>}
        void IdentList()
        {
            Accept(LexicalAnalyzer.ident);
            while (currentSymbol == LexicalAnalyzer.comma)
            {
                currentSymbol = lexer.NextSym();
                Accept(LexicalAnalyzer.ident);
            }
        }

        // <блок> ::= <раздел меток> <раздел констант> <раздел типов> <раздел переменных> <раздел
        // процедур и функций> <раздел операторов>
        void Block()
        {
            // <раздел меток> ::= label <метка> {, <метка>};
            // НЕ АНАЛИЗИРУЕТСЯ
            if (currentSymbol == LexicalAnalyzer.labelsy)
            {
                while (currentSymbol != LexicalAnalyzer.semicolon)
                {
                    currentSymbol = lexer.NextSym();
                }
                currentSymbol = lexer.NextSym();
            }

            // <раздел констант> ::= <пусто> | const <определение константы>; { <определение константы>;}
            // <раздел типов> ::= <пусто> | type <определение типа> ;{ <определение типа>;}
            // НЕ АНАЛИЗИРУЕТСЯ
            if (currentSymbol == LexicalAnalyzer.constsy || currentSymbol == LexicalAnalyzer.typesy)
            {
                while (currentSymbol != LexicalAnalyzer.varsy && 
                       currentSymbol != LexicalAnalyzer.beginsy)
                {
                    currentSymbol = lexer.NextSym();
                }
            }

            // <раздел переменных> ::= var <описание однотипных переменных> ;
            // {<описание однотипных переменных>;} | <пусто>
            VarDeclarations();

            // <раздел процедур и функций> ::= {<описание процедуры или функции> ;}
            // НЕ АНАЛИЗИРУЕТСЯ
            while (currentSymbol == LexicalAnalyzer.procedurensy || 
                   currentSymbol == LexicalAnalyzer.functionsy)
            {
                while (currentSymbol != LexicalAnalyzer.semicolon)
                {
                    currentSymbol = lexer.NextSym();
                }
                currentSymbol = lexer.NextSym();
            }

            // <раздел операторов> ::= <составной оператор>
            // <составной оператор> ::= begin <оператор> {; <оператор>} end
            CompoundStatement();
        }

        void VarDeclarations()
        {
            if (currentSymbol == LexicalAnalyzer.varsy)
            {
                currentSymbol = lexer.NextSym();
                do
                {
                    VarDeclaration();   
                    Accept(LexicalAnalyzer.semicolon);
                } while (currentSymbol == LexicalAnalyzer.ident);
            }
        }

        void VarDeclaration()
        {
            IdentList();
            Accept(LexicalAnalyzer.colon);
            Type();
        }

        void Type()
        {
            // ^
            if (currentSymbol == LexicalAnalyzer.arrow)
            {
                currentSymbol = lexer.NextSym();
                Accept(LexicalAnalyzer.ident);
            }

            else if (currentSymbol == LexicalAnalyzer.ident ||
                     currentSymbol == LexicalAnalyzer.leftpar ||
                     currentSymbol == LexicalAnalyzer.intc ||
                     currentSymbol == LexicalAnalyzer.floatc ||
                     currentSymbol == LexicalAnalyzer.charc)
            {
                OrdinalType();
            }

            else
            {
                if (currentSymbol == LexicalAnalyzer.packedsy)
                {
                    currentSymbol = lexer.NextSym();
                }

                switch (currentSymbol)
                {
                    // 'array' '[' <ordinal_type> {, <ordinal_type>} ']' 'of' <type>
                    case LexicalAnalyzer.arraysy:
                        currentSymbol = lexer.NextSym();

                        if (currentSymbol != LexicalAnalyzer.lbracket)
                        {
                            InputOutput.Error(12, lexer.tokenPos);
                        }
                        else
                        {
                            do
                            {
                                currentSymbol = lexer.NextSym();
                                OrdinalType();  
                            } while (currentSymbol == LexicalAnalyzer.comma);

                            Accept(LexicalAnalyzer.rbracket);
                        }
                        
                        if (currentSymbol != LexicalAnalyzer.ofsy)
                        {
                            InputOutput.Error(14, lexer.tokenPos);
                        }
                        else
                        {
                            currentSymbol = lexer.NextSym();
                        }
                        
                        Type();

                        break;
                        
                    // 'file' 'of' <type>
                    case LexicalAnalyzer.filesy:
                        currentSymbol = lexer.NextSym();

                        Accept(LexicalAnalyzer.ofsy);

                        Type();

                        break;

                    // 'set' 'of' <ordinal_type>
                    case LexicalAnalyzer.setsy:
                        currentSymbol = lexer.NextSym();

                        Accept(LexicalAnalyzer.ofsy);

                        OrdinalType();

                        break;

                    case LexicalAnalyzer.recordsy:
                        currentSymbol = lexer.NextSym();
                        FieldList();
                        Accept(LexicalAnalyzer.endsy);
                        break;

                    default:
                        InputOutput.Error(2, lexer.tokenPos);
                        break;
                }
            }
        }

        void FieldList()
        {
            while (currentSymbol == LexicalAnalyzer.ident ||
                   currentSymbol == LexicalAnalyzer.casesy)
            {
                if (currentSymbol == LexicalAnalyzer.casesy)
                {
                    currentSymbol = lexer.NextSym();

                    if (currentSymbol == LexicalAnalyzer.ident)
                    {
                        currentSymbol = lexer.NextSym();
                        Accept(LexicalAnalyzer.colon);
                        Accept(LexicalAnalyzer.ident);
                        Accept(LexicalAnalyzer.ofsy);
                        ConstantAndFields();
                    }
                }
                else // ident
                {
                    IdentList();
                    Accept(LexicalAnalyzer.colon);
                    Type();

                    if (currentSymbol == LexicalAnalyzer.semicolon)
                    {
                        currentSymbol = lexer.NextSym();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        // <constant> {, <constant>} ':' '(' <field_list> ')'
        void ConstantAndFields()
        {
            do
            {
                Constant();
            } while (currentSymbol == LexicalAnalyzer.comma);
            
            Accept(LexicalAnalyzer.colon);
            Accept(LexicalAnalyzer.leftpar);
            FieldList();
            Accept(LexicalAnalyzer.rightpar);
        }

        void OrdinalType()
        {
            switch(currentSymbol)
            {
                case LexicalAnalyzer.leftpar:
                    currentSymbol = lexer.NextSym();
                    IdentList();
                    Accept(LexicalAnalyzer.rightpar);
                    break;

                case LexicalAnalyzer.intc:
                case LexicalAnalyzer.floatc:
                case LexicalAnalyzer.charc:
                    byte tmp = currentSymbol;
                    currentSymbol = lexer.NextSym();
                    Accept(LexicalAnalyzer.twopoints);
                    Accept(tmp);
                    break;

                default:
                    Accept(LexicalAnalyzer.ident);
                    break;
            }
        }

        void CompoundStatement()
        {
            Accept(LexicalAnalyzer.beginsy);
            // <оператор> {; <оператор>}
            Statement();
            while (currentSymbol == LexicalAnalyzer.semicolon)
            {
                currentSymbol = lexer.NextSym();
                Statement();
            }

            Accept(LexicalAnalyzer.endsy);
        }

        // все, далее БНФ абсолютно не читаемы и бесполезны
        void Statement()
        {
            // <метка> :
            // НЕ АНАЛИЗИРУЕТСЯ
            if (currentSymbol == LexicalAnalyzer.intc)
            {
                currentSymbol = lexer.NextSym();
                currentSymbol = lexer.NextSym();
            }

            switch (currentSymbol)
            {
                // 'goto' <метка>
                case LexicalAnalyzer.gotosy:
                    currentSymbol = lexer.NextSym();
                    Accept(LexicalAnalyzer.intc);
                    break;

                // 'while' <Expression> 'do' <Statement>
                case LexicalAnalyzer.whilesy:
                    currentSymbol = lexer.NextSym();
                    Expression();
                    Accept(LexicalAnalyzer.dosy);
                    Statement();
                    break;

                // 'begin' <Statement> {';' <Statement>} 'end'
                case LexicalAnalyzer.beginsy:
                    do
                    {
                        currentSymbol = lexer.NextSym();
                        Statement();
                    } while (currentSymbol == LexicalAnalyzer.semicolon);

                    Accept(LexicalAnalyzer.endsy);
                    break;

                // 'if' <Expression> 'then' <Statement>
                case LexicalAnalyzer.ifsy:
                    currentSymbol = lexer.NextSym();
                    Expression();
                    Accept(LexicalAnalyzer.endsy);
                    Statement();

                    if (currentSymbol == LexicalAnalyzer.elsesy)
                    {
                        currentSymbol = lexer.NextSym();
                        Statement();
                    }

                    break;

                // 'with' <variable> {, <variable>} 'do' <Statement>
                case LexicalAnalyzer.withsy:
                    // <variable> {, <variable>}
                    do
                    {
                        currentSymbol = lexer.NextSym();
                        Variable();
                    } while (currentSymbol == LexicalAnalyzer.comma);

                    Accept(LexicalAnalyzer.dosy);
                    Statement();
                    break;

                // 'repeat' <Statement> {';' <Statement>} 'until' <Expression>
                case LexicalAnalyzer.repeatsy:
                    do
                    {
                        currentSymbol = lexer.NextSym();
                        Statement();
                    } while (currentSymbol == LexicalAnalyzer.semicolon);

                    Accept(LexicalAnalyzer.untilsy);
                    Expression();
                    break;
                
                case LexicalAnalyzer.ident:
                    Variable();
                    if (currentSymbol == LexicalAnalyzer.leftpar)
                    {
                        currentSymbol = lexer.NextSym();
                        ParameterList();
                        Accept(LexicalAnalyzer.rightpar);
                    }
                    else
                    {
                        // ':=' <Expression>
                        if (currentSymbol == LexicalAnalyzer.assign)
                        {       
                            currentSymbol = lexer.NextSym();
                            Expression();
                        }
                    }
                    break;

                // 'case' <Expression> 'of' <case_list> 'end'
                case LexicalAnalyzer.casesy:
                    currentSymbol = lexer.NextSym();
                    Expression();
                    Accept(LexicalAnalyzer.ofsy);
                    CaseList();
                    Accept(LexicalAnalyzer.endsy);
                    break;

                // 'for' <variableIdentifier> ':=' <Expression> 'to' <Expression> 'do' <Statement>
                case LexicalAnalyzer.forsy:
                    currentSymbol = lexer.NextSym();
                    Accept(LexicalAnalyzer.ident);
                    Accept(LexicalAnalyzer.assign);
                    Expression();

                    if (currentSymbol != LexicalAnalyzer.tosy &&
                        currentSymbol != LexicalAnalyzer.downtosy)
                    {
                        //InputOutput.Error(15, lexer.tokenPos); // Ожидается ':'
                        //return;
                    }
                    else
                    {
                        currentSymbol = lexer.NextSym();
                    }

                    Expression();
                    Accept(LexicalAnalyzer.dosy);                   
                    Statement();

                    break;

                default:
                    break;
            }
        }

        void CaseList()
        {
            Constant();
                while (currentSymbol == LexicalAnalyzer.comma)
                {
                    currentSymbol = lexer.NextSym();
                    Constant();
                }

                Accept(LexicalAnalyzer.colon);
                currentSymbol = lexer.NextSym();
                Statement();

            while (currentSymbol == LexicalAnalyzer.semicolon)
            {
                currentSymbol = lexer.NextSym();
                
                Constant();
                while (currentSymbol == LexicalAnalyzer.comma)
                {
                    currentSymbol = lexer.NextSym();
                    Constant();
                }

                Accept(LexicalAnalyzer.colon);
                currentSymbol = lexer.NextSym();
                Statement();
            }
        }

        void ParameterList()
        {
            Expression();
            while (currentSymbol == LexicalAnalyzer.comma ||
                   currentSymbol == LexicalAnalyzer.colon)
            {
                currentSymbol = lexer.NextSym();
                Expression();
            }
        }

        void Variable()
        {
            Accept(LexicalAnalyzer.ident);

            while (currentSymbol == LexicalAnalyzer.lbracket ||
                    currentSymbol == LexicalAnalyzer.point ||
                    currentSymbol == LexicalAnalyzer.arrow)
            {
                switch (currentSymbol)
                {
                    // '[' <Expression> {',' <Expression>} ']'
                    case LexicalAnalyzer.lbracket:
                        currentSymbol = lexer.NextSym();

                        do
                        {
                            Expression();
                        } while (currentSymbol == LexicalAnalyzer.comma);

                        Accept(LexicalAnalyzer.rbracket);

                        break;
                    
                    // '.' <FieldIdentifier>
                    case LexicalAnalyzer.point:
                        currentSymbol = lexer.NextSym();
                        Accept(LexicalAnalyzer.ident);
                        break;
                    
                    // '^'
                    case LexicalAnalyzer.arrow:
                        currentSymbol = lexer.NextSym();
                        break;
                }
            }
        }


        // <Expression> {, <Expression>}
        void Expression()
        {
            // <Expression> ::= <Simple Expression> [(= | < | > | <> | <= | >= | in) <SimpleExprssion>] ===
            SimpleExpression();

            // [(= | < | > | <> | <= | >= | in) < SimpleExprssion >]
            if (currentSymbol == LexicalAnalyzer.equal ||
                   currentSymbol == LexicalAnalyzer.later ||
                   currentSymbol == LexicalAnalyzer.greater ||
                   currentSymbol == LexicalAnalyzer.laterequal ||
                   currentSymbol == LexicalAnalyzer.greaterequal ||
                   currentSymbol == LexicalAnalyzer.latergreater ||
                   currentSymbol == LexicalAnalyzer.insy)
            {
                currentSymbol = lexer.NextSym();
                SimpleExpression();
            }
            // ============================================================================================
            //while (currentSymbol == LexicalAnalyzer.equal ||
            //       currentSymbol == LexicalAnalyzer.later ||
            //       currentSymbol == LexicalAnalyzer.greater ||
            //       currentSymbol == LexicalAnalyzer.laterequal ||
            //       currentSymbol == LexicalAnalyzer.greaterequal ||
            //       currentSymbol == LexicalAnalyzer.latergreater)
            //{
            //    currentSymbol = lexer.NextSym();
            //    SimpleExpression();
            //}

            // {, <Expression>}
            if (currentSymbol == LexicalAnalyzer.comma)
            {
                currentSymbol = lexer.NextSym();
                Expression();
            }
        }

        // <SimpleExpression> ::= [+ | -] <Term> {(+ | - | or) <Term>}
        void SimpleExpression()
        {
            if (currentSymbol == LexicalAnalyzer.plus ||
                currentSymbol == LexicalAnalyzer.minus)
            {
                currentSymbol = lexer.NextSym();
            }

            // <Term> {(+ | - | or) <Term>}
            Term();
            while (currentSymbol == LexicalAnalyzer.plus ||
                   currentSymbol == LexicalAnalyzer.minus ||
                   currentSymbol == LexicalAnalyzer.orsy)
            {
                currentSymbol = lexer.NextSym();
                Term();
            }
        }

        // <Term> ::= <Factor> {(* | / | div | mod | and) <Factor>}
        void Term()
        {
            Factor();
            while (currentSymbol == LexicalAnalyzer.star ||
                   currentSymbol == LexicalAnalyzer.slash ||
                   currentSymbol == LexicalAnalyzer.divsy ||
                   currentSymbol == LexicalAnalyzer.modsy ||
                   currentSymbol == LexicalAnalyzer.andsy)
            {
                currentSymbol = lexer.NextSym();
                Factor();
            }
        }


        void Factor()
        {
            switch (currentSymbol)
            {
                // '(' <Expression> ')'
                case LexicalAnalyzer.leftpar:
                    currentSymbol = lexer.NextSym();
                    Expression();
                    Accept(LexicalAnalyzer.rightpar);
                    break;

                // '[' [<Expression> {'..' <Expression>} {,<Expression> {'..' <Expression>}}] ']'
                case LexicalAnalyzer.lbracket:
                    currentSymbol = lexer.NextSym();

                    do
                    {
                        Expression();

                        if (currentSymbol == LexicalAnalyzer.twopoints)
                        {
                            currentSymbol = lexer.NextSym();
                            Expression();
                        }
                    } while (currentSymbol == LexicalAnalyzer.comma);

                    Accept(LexicalAnalyzer.rbracket);

                    break;


                // 'not' <Factor>
                case LexicalAnalyzer.notsy:
                    currentSymbol = lexer.NextSym();
                    Factor();
                    break;

                case LexicalAnalyzer.intc:
                case LexicalAnalyzer.floatc:
                case LexicalAnalyzer.charc:
                case LexicalAnalyzer.stringc:
                    currentSymbol = lexer.NextSym();
                    break;
                
                case LexicalAnalyzer.ident:
                    Variable();
                    if (currentSymbol == LexicalAnalyzer.leftpar)
                    {
                        currentSymbol = lexer.NextSym();
                        ParameterList();
                        Accept(LexicalAnalyzer.rightpar);
                    }
                    break;

            }
            // ???

            // if (currentSymbol == LexicalAnalyzer.ident)
            // {
            //     ParseVariable();
            // }
            // else if (IsUnsignedConstant(currentSymbol))
            // {
            //     ParseUnsignedConstant();
            // }
            // else if (currentSymbol == LexicalAnalyzer.leftpar)
            // {
            //     ParseParenthesizedExpression();
            // }
        }

        void ParseArrayIndex()
        {
            currentSymbol = lexer.NextSym();
            Expression();
            Accept(LexicalAnalyzer.rbracket);
            currentSymbol = lexer.NextSym();
        }

        void ParseRecordField()
        {
            currentSymbol = lexer.NextSym();
            Accept(LexicalAnalyzer.ident);
            currentSymbol = lexer.NextSym();
        }

        void ParseUnsignedConstant()
        {
            currentSymbol = lexer.NextSym();
        }

        void Constant()
        {
            if (currentSymbol == LexicalAnalyzer.stringc)
            {
                currentSymbol = lexer.NextSym();
            }
            else
            {
                if (currentSymbol == LexicalAnalyzer.plus ||
                    currentSymbol == LexicalAnalyzer.minus)
                {
                    currentSymbol = lexer.NextSym();
                }

                if (currentSymbol == LexicalAnalyzer.ident ||
                    currentSymbol == LexicalAnalyzer.intc ||
                    currentSymbol == LexicalAnalyzer.floatc ||
                    currentSymbol == LexicalAnalyzer.charc)
                {
                    currentSymbol = lexer.NextSym();
                }
                else
                {
                    InputOutput.Error(18, lexer.tokenPos);
                }
            }
        }

        private bool IsUnsignedConstant(byte symbol)
        {
            return symbol == LexicalAnalyzer.intc ||
                   symbol == LexicalAnalyzer.floatc ||
                   symbol == LexicalAnalyzer.charc;
        }

        void UnsignedConstant()
        {
            switch (currentSymbol)
            {
                case LexicalAnalyzer.intc:
                case LexicalAnalyzer.floatc:
                case LexicalAnalyzer.charc:
                case LexicalAnalyzer.stringc:
                    currentSymbol = lexer.NextSym();
                    break;
                default:
                    InputOutput.Error(18, lexer.tokenPos); // Ожидается константа
                    return;
            }
        }

        static bool Belong(byte element, HashSet<byte> set)
        {
            return set.Contains(element);
        }

        void SkipTo(HashSet<byte> where)
        {
            while (!Belong(lexer.symbol, where))
            {
                currentSymbol = lexer.NextSym();
            }
        }

        void SkipTo2(HashSet<byte> start, HashSet<byte> follow)
        {
            while (!(Belong(lexer.symbol, start) ||
                Belong(lexer.symbol, follow)))
            {
                currentSymbol = lexer.NextSym();
            }
        }
    }
}