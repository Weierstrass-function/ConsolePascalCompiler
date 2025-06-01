using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler
{
    class SyntaxAnalyzer
    {
        private LexicalAnalyzer lexer;
        private byte currentSymbol;

        public SyntaxAnalyzer(LexicalAnalyzer lexicalAnalyzer)
        {
            lexer = lexicalAnalyzer;
        }

        public void Analyze()
        {
            currentSymbol = lexer.NextSym();
            Program();
        }

        // <программа> ::= program <имя> (<имя файла> {, <имя файла>}); <блок>.
        void Program()
        {
            if (currentSymbol != LexicalAnalyzer.programsy)
            {
                InputOutput.Error(1, lexer.token); // Expected 'program'
                return;
            }
            
            currentSymbol = lexer.NextSym();
            
            if (currentSymbol != LexicalAnalyzer.ident)
            {
                InputOutput.Error(2, lexer.token); // Expected program name
                return;
            }

            currentSymbol = lexer.NextSym();

            if (currentSymbol != LexicalAnalyzer.leftpar)
            {
                InputOutput.Error(3, lexer.token); // Expected '('
                return;
            }

            currentSymbol = lexer.NextSym();

            // Parse program file names
            do
            {
                if (currentSymbol != LexicalAnalyzer.ident)
                {
                    InputOutput.Error(4, lexer.token); // Expected file name
                    return;
                }

                currentSymbol = lexer.NextSym();

                if (currentSymbol == LexicalAnalyzer.comma)
                {
                    currentSymbol = lexer.NextSym();
                }

            } while (currentSymbol == LexicalAnalyzer.ident);

            if (currentSymbol != LexicalAnalyzer.rightpar)
            {
                InputOutput.Error(5, lexer.token); // Expected ')'
                return;
            }

            currentSymbol = lexer.NextSym();

            if (currentSymbol != LexicalAnalyzer.semicolon)
            {
                InputOutput.Error(6, lexer.token); // Expected ';'
                return;
            }

            currentSymbol = lexer.NextSym();

            Block();

            if (currentSymbol != LexicalAnalyzer.point)
            {
                InputOutput.Error(7, lexer.token); // Expected '.'
                return;
            }
        }

        // <блок> ::= <раздел меток> <раздел констант> <раздел типов> <раздел переменных> 
        // <раздел процедур и функций> <раздел операторов>
        void Block()
        {
            LabelSection();
            ConstSection();
            TypeSection();
            VarSection();
            ProcFuncSection();
            StatementSection();
        }

        // <раздел меток> ::= <пусто> | label <метка> {, <метка>};
        void LabelSection()
        {
            if (currentSymbol != LexicalAnalyzer.labelsy)
                return;

            currentSymbol = lexer.NextSym();

            do
            {
                if (currentSymbol != LexicalAnalyzer.intc)
                {
                    InputOutput.Error(8, lexer.token); // Expected label number
                    return;
                }

                currentSymbol = lexer.NextSym();

                if (currentSymbol == LexicalAnalyzer.comma)
                    currentSymbol = lexer.NextSym();

            } while (currentSymbol == LexicalAnalyzer.intc);

            if (currentSymbol != LexicalAnalyzer.semicolon)
            {
                InputOutput.Error(6, lexer.token); // Expected ';'
                return;
            }

            currentSymbol = lexer.NextSym();
        }

        // <раздел констант> ::= <пусто> | const <описание константы> {;<описание константы>};
        void ConstSection()
        {
            if (currentSymbol != LexicalAnalyzer.constsy)
                return;

            currentSymbol = lexer.NextSym();

            do
            {
                if (currentSymbol != LexicalAnalyzer.ident)
                {
                    InputOutput.Error(9, lexer.token); // Expected identifier
                    return;
                }

                currentSymbol = lexer.NextSym();

                if (currentSymbol != LexicalAnalyzer.equal)
                {
                    InputOutput.Error(10, lexer.token); // Expected '='
                    return;
                }

                currentSymbol = lexer.NextSym();

                // Константное выражение
                if (currentSymbol != LexicalAnalyzer.intc && 
                    currentSymbol != LexicalAnalyzer.floatc &&
                    currentSymbol != LexicalAnalyzer.charc)
                {
                    InputOutput.Error(11, lexer.token); // Expected constant
                    return;
                }

                currentSymbol = lexer.NextSym();

                if (currentSymbol == LexicalAnalyzer.semicolon)
                    currentSymbol = lexer.NextSym();

            } while (currentSymbol == LexicalAnalyzer.ident);
        }

        // <раздел типов> ::= <пусто> | type <описание типа> {;<описание типа>};
        void TypeSection()
        {
            if (currentSymbol != LexicalAnalyzer.typesy)
                return;

            currentSymbol = lexer.NextSym();

            do
            {
                if (currentSymbol != LexicalAnalyzer.ident)
                {
                    InputOutput.Error(9, lexer.token); // Expected identifier
                    return;
                }

                currentSymbol = lexer.NextSym();

                if (currentSymbol != LexicalAnalyzer.equal)
                {
                    InputOutput.Error(10, lexer.token); // Expected '='
                    return;
                }

                currentSymbol = lexer.NextSym();

                // Разбор описания типа (массив или запись)
                if (currentSymbol == LexicalAnalyzer.arraysy)
                {
                    currentSymbol = lexer.NextSym();
                    if (currentSymbol != LexicalAnalyzer.lbracket)
                    {
                        InputOutput.Error(12, lexer.token); // Expected '['
                        return;
                    }
                    
                    currentSymbol = lexer.NextSym();
                    // Разбор размерности массива
                    while (currentSymbol == LexicalAnalyzer.intc)
                    {
                        currentSymbol = lexer.NextSym();
                        if (currentSymbol == LexicalAnalyzer.comma)
                            currentSymbol = lexer.NextSym();
                    }

                    if (currentSymbol != LexicalAnalyzer.rbracket)
                    {
                        InputOutput.Error(13, lexer.token); // Expected ']'
                        return;
                    }

                    currentSymbol = lexer.NextSym();
                    if (currentSymbol != LexicalAnalyzer.ofsy)
                    {
                        InputOutput.Error(14, lexer.token); // Expected 'of'
                        return;
                    }

                    currentSymbol = lexer.NextSym();
                    if (currentSymbol != LexicalAnalyzer.ident)
                    {
                        InputOutput.Error(9, lexer.token); // Expected identifier
                        return;
                    }
                    currentSymbol = lexer.NextSym();
                }
                else if (currentSymbol == LexicalAnalyzer.recordsy)
                {
                    currentSymbol = lexer.NextSym();
                    // Разбор полей записи
                    while (currentSymbol == LexicalAnalyzer.ident)
                    {
                        do
                        {
                            currentSymbol = lexer.NextSym();
                            if (currentSymbol == LexicalAnalyzer.comma)
                                currentSymbol = lexer.NextSym();
                        } while (currentSymbol == LexicalAnalyzer.ident);

                        if (currentSymbol != LexicalAnalyzer.colon)
                        {
                            InputOutput.Error(15, lexer.token); // Expected ':'
                            return;
                        }

                        currentSymbol = lexer.NextSym();
                        if (currentSymbol != LexicalAnalyzer.ident)
                        {
                            InputOutput.Error(9, lexer.token); // Expected identifier
                            return;
                        }

                        currentSymbol = lexer.NextSym();
                        if (currentSymbol == LexicalAnalyzer.semicolon)
                            currentSymbol = lexer.NextSym();
                    }

                    if (currentSymbol != LexicalAnalyzer.endsy)
                    {
                        InputOutput.Error(16, lexer.token); // Expected 'end'
                        return;
                    }
                    currentSymbol = lexer.NextSym();
                }

                if (currentSymbol == LexicalAnalyzer.semicolon)
                    currentSymbol = lexer.NextSym();

            } while (currentSymbol == LexicalAnalyzer.ident);
        }

        // <раздел переменных> ::= <пусто> | var <описание переменных> {;<описание переменных>};
        void VarSection()
        {
            if (currentSymbol != LexicalAnalyzer.varsy)
                return;

            currentSymbol = lexer.NextSym();

            do
            {
                if (currentSymbol != LexicalAnalyzer.ident)
                {
                    InputOutput.Error(9, lexer.token); // Expected identifier
                    return;
                }

                // Разбор списка идентификаторов через запятую (например "a, b, c: integer")
                do
                {
                    currentSymbol = lexer.NextSym();
                    if (currentSymbol == LexicalAnalyzer.comma)
                        currentSymbol = lexer.NextSym();
                } while (currentSymbol == LexicalAnalyzer.ident);

                if (currentSymbol != LexicalAnalyzer.colon)
                {
                    InputOutput.Error(15, lexer.token); // Expected ':'
                    return;
                }

                currentSymbol = lexer.NextSym();
                if (currentSymbol != LexicalAnalyzer.ident)
                {
                    InputOutput.Error(9, lexer.token); // Expected identifier
                    return;
                }

                currentSymbol = lexer.NextSym();
                if (currentSymbol == LexicalAnalyzer.semicolon)
                    currentSymbol = lexer.NextSym();

            } while (currentSymbol == LexicalAnalyzer.ident);
        }

        void ProcFuncSection() { }

        // <раздел операторов> ::= begin <оператор> {;<оператор>} end
        void StatementSection()
        {
            if (currentSymbol != LexicalAnalyzer.beginsy)
            {
                InputOutput.Error(17, lexer.token); // Expected 'begin'
                return;
            }

            currentSymbol = lexer.NextSym();

            do
            {
                // Оператор присваивания
                if (currentSymbol == LexicalAnalyzer.ident)
                {
                    currentSymbol = lexer.NextSym();
                    
                    // Индексированная переменная или поле записи
                    while (currentSymbol == LexicalAnalyzer.lbracket || currentSymbol == LexicalAnalyzer.point)
                    {
                        if (currentSymbol == LexicalAnalyzer.lbracket)
                        {
                            currentSymbol = lexer.NextSym();
                            // Разбор индекса
                            if (currentSymbol != LexicalAnalyzer.intc)
                            {
                                InputOutput.Error(18, lexer.token); // Expected array index
                                return;
                            }
                            currentSymbol = lexer.NextSym();
                            if (currentSymbol != LexicalAnalyzer.rbracket)
                            {
                                InputOutput.Error(13, lexer.token); // Expected ']'
                                return;
                            }
                            currentSymbol = lexer.NextSym();
                        }
                        else // period
                        {
                            currentSymbol = lexer.NextSym();
                            if (currentSymbol != LexicalAnalyzer.ident)
                            {
                                InputOutput.Error(9, lexer.token); // Expected identifier
                                return;
                            }
                            currentSymbol = lexer.NextSym();
                        }
                    }

                    if (currentSymbol != LexicalAnalyzer.assign)
                    {
                        InputOutput.Error(19, lexer.token); // Expected ':='
                        return;
                    }

                    currentSymbol = lexer.NextSym();
                    // Разбор выражения
                    while (currentSymbol == LexicalAnalyzer.ident || 
                           currentSymbol == LexicalAnalyzer.intc ||
                           currentSymbol == LexicalAnalyzer.floatc ||
                           currentSymbol == LexicalAnalyzer.charc)
                    {
                        currentSymbol = lexer.NextSym();
                    }
                }

                if (currentSymbol == LexicalAnalyzer.semicolon)
                    currentSymbol = lexer.NextSym();

            } while (currentSymbol == LexicalAnalyzer.ident);

            if (currentSymbol != LexicalAnalyzer.endsy)
            {
                InputOutput.Error(16, lexer.token); // Expected 'end'
                return;
            }

            currentSymbol = lexer.NextSym();
        }
    }
}