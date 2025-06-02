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

        void Program()
        {
            // Skip program header
            while (currentSymbol != LexicalAnalyzer.semicolon)
            {
                currentSymbol = lexer.NextSym();
            }
            currentSymbol = lexer.NextSym();

            Block();

            // Skip final dot
            if (currentSymbol == LexicalAnalyzer.point)
            {
                currentSymbol = lexer.NextSym();
            }
        }

        void Block()
        {
            // Skip labels
            if (currentSymbol == LexicalAnalyzer.labelsy)
            {
                while (currentSymbol != LexicalAnalyzer.semicolon)
                {
                    currentSymbol = lexer.NextSym();
                }
                currentSymbol = lexer.NextSym();
            }

            // Skip constants
            if (currentSymbol == LexicalAnalyzer.constsy)
            {
                while (currentSymbol != LexicalAnalyzer.varsy && 
                       currentSymbol != LexicalAnalyzer.beginsy)
                {
                    currentSymbol = lexer.NextSym();
                }
            }

            // Parse type declarations
            if (currentSymbol == LexicalAnalyzer.typesy)
            {
                currentSymbol = lexer.NextSym();
                ParseTypeDeclarations();
            }

            // Parse variable declarations
            if (currentSymbol == LexicalAnalyzer.varsy)
            {
                currentSymbol = lexer.NextSym();
                ParseVarDeclarations();
            }

            // Skip procedures/functions
            while (currentSymbol == LexicalAnalyzer.procedurensy || 
                   currentSymbol == LexicalAnalyzer.functionsy)
            {
                while (currentSymbol != LexicalAnalyzer.semicolon)
                {
                    currentSymbol = lexer.NextSym();
                }
                currentSymbol = lexer.NextSym();
            }

            // Parse compound statement
            if (currentSymbol == LexicalAnalyzer.beginsy)
            {
                currentSymbol = lexer.NextSym();
                ParseCompoundStatement();
            }
        }

        void ParseTypeDeclarations()
        {
            while (currentSymbol == LexicalAnalyzer.ident)
            {
                // Type name
                currentSymbol = lexer.NextSym();

                if (currentSymbol != LexicalAnalyzer.equal)
                {
                    InputOutput.Error(10, lexer.token); // Expected '='
                    return;
                }
                currentSymbol = lexer.NextSym();

                // Parse type definition
                if (currentSymbol == LexicalAnalyzer.arraysy)
                {
                    // Array type
                    currentSymbol = lexer.NextSym();

                    if (currentSymbol != LexicalAnalyzer.lbracket)
                    {
                        InputOutput.Error(12, lexer.token); // Expected '['
                        return;
                    }
                    currentSymbol = lexer.NextSym();

                    // Parse array dimensions
                    do
                    {
                        // Lower bound
                        if (currentSymbol != LexicalAnalyzer.intc)
                        {
                            InputOutput.Error(18, lexer.token); // Expected array index
                            return;
                        }
                        currentSymbol = lexer.NextSym();

                        if (currentSymbol == LexicalAnalyzer.twopoints)
                        {
                            currentSymbol = lexer.NextSym();
                            // Upper bound
                            if (currentSymbol != LexicalAnalyzer.intc)
                            {
                                InputOutput.Error(18, lexer.token); // Expected array index
                                return;
                            }
                            currentSymbol = lexer.NextSym();
                        }

                        if (currentSymbol == LexicalAnalyzer.comma)
                            currentSymbol = lexer.NextSym();

                    } while (currentSymbol == LexicalAnalyzer.intc);

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

                    // Array base type
                    if (currentSymbol != LexicalAnalyzer.ident)
                    {
                        InputOutput.Error(9, lexer.token); // Expected identifier
                        return;
                    }
                    currentSymbol = lexer.NextSym();
                }
                else if (currentSymbol == LexicalAnalyzer.recordsy)
                {
                    // Record type
                    currentSymbol = lexer.NextSym();

                    // Parse record fields
                    while (currentSymbol == LexicalAnalyzer.ident)
                    {
                        // Field names
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

                        // Field type
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
                else if (currentSymbol == LexicalAnalyzer.ident)
                {
                    // Type alias
                    currentSymbol = lexer.NextSym();
                }

                if (currentSymbol == LexicalAnalyzer.semicolon)
                    currentSymbol = lexer.NextSym();
            }
        }

        void ParseVarDeclarations()
        {
            while (currentSymbol == LexicalAnalyzer.ident)
            {
                // Variable names
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

                // Variable type
                if (currentSymbol != LexicalAnalyzer.ident)
                {
                    InputOutput.Error(9, lexer.token); // Expected identifier
                    return;
                }
                currentSymbol = lexer.NextSym();

                if (currentSymbol == LexicalAnalyzer.semicolon)
                    currentSymbol = lexer.NextSym();
            }
        }

        void ParseCompoundStatement()
        {
            while (currentSymbol != LexicalAnalyzer.endsy)
            {
                if (currentSymbol == LexicalAnalyzer.ident)
                {
                    // Assignment or procedure call
                    currentSymbol = lexer.NextSym();

                    // Array index or record field access
                    while (currentSymbol == LexicalAnalyzer.lbracket || 
                           currentSymbol == LexicalAnalyzer.point)
                    {
                        if (currentSymbol == LexicalAnalyzer.lbracket)
                        {
                            // Array indexing
                            currentSymbol = lexer.NextSym();
                            Expression();
                            if (currentSymbol != LexicalAnalyzer.rbracket)
                            {
                                InputOutput.Error(13, lexer.token); // Expected ']'
                                return;
                            }
                            currentSymbol = lexer.NextSym();
                        }
                        else // point
                        {
                            // Record field access
                            currentSymbol = lexer.NextSym();
                            if (currentSymbol != LexicalAnalyzer.ident)
                            {
                                InputOutput.Error(9, lexer.token); // Expected identifier
                                return;
                            }
                            currentSymbol = lexer.NextSym();
                        }
                    }

                    if (currentSymbol == LexicalAnalyzer.assign)
                    {
                        // Assignment
                        currentSymbol = lexer.NextSym();
                        Expression();
                    }
                }
                else if (currentSymbol == LexicalAnalyzer.withsy)
                {
                    // With statement
                    currentSymbol = lexer.NextSym();

                    do
                    {
                        if (currentSymbol != LexicalAnalyzer.ident)
                        {
                            InputOutput.Error(9, lexer.token); // Expected identifier
                            return;
                        }
                        currentSymbol = lexer.NextSym();

                        if (currentSymbol == LexicalAnalyzer.comma)
                            currentSymbol = lexer.NextSym();

                    } while (currentSymbol == LexicalAnalyzer.comma);

                    if (currentSymbol != LexicalAnalyzer.dosy)
                    {
                        InputOutput.Error(21, lexer.token); // Expected 'do'
                        return;
                    }
                    currentSymbol = lexer.NextSym();

                    if (currentSymbol == LexicalAnalyzer.beginsy)
                    {
                        currentSymbol = lexer.NextSym();
                        ParseCompoundStatement();
                    }
                }
                else if (currentSymbol == LexicalAnalyzer.beginsy)
                {
                    // Nested compound statement
                    currentSymbol = lexer.NextSym();
                    ParseCompoundStatement();
                }
                else
                {
                    // Skip other statements
                    while (currentSymbol != LexicalAnalyzer.semicolon && 
                           currentSymbol != LexicalAnalyzer.endsy)
                    {
                        currentSymbol = lexer.NextSym();
                    }
                }

                if (currentSymbol == LexicalAnalyzer.semicolon)
                    currentSymbol = lexer.NextSym();
            }

            currentSymbol = lexer.NextSym();
        }

        void Expression()
        {
            SimpleExpression();

            while (currentSymbol == LexicalAnalyzer.equal ||
                   currentSymbol == LexicalAnalyzer.later ||
                   currentSymbol == LexicalAnalyzer.greater ||
                   currentSymbol == LexicalAnalyzer.laterequal ||
                   currentSymbol == LexicalAnalyzer.greaterequal ||
                   currentSymbol == LexicalAnalyzer.latergreater)
            {
                currentSymbol = lexer.NextSym();
                SimpleExpression();
            }
        }

        void SimpleExpression()
        {
            if (currentSymbol == LexicalAnalyzer.plus ||
                currentSymbol == LexicalAnalyzer.minus)
            {
                currentSymbol = lexer.NextSym();
            }

            Term();
            
            while (currentSymbol == LexicalAnalyzer.plus ||
                   currentSymbol == LexicalAnalyzer.minus ||
                   currentSymbol == LexicalAnalyzer.orsy)
            {
                currentSymbol = lexer.NextSym();
                Term();
            }
        }

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
            if (currentSymbol == LexicalAnalyzer.ident)
            {
                currentSymbol = lexer.NextSym();

                // Array index or record field access
                while (currentSymbol == LexicalAnalyzer.lbracket ||
                       currentSymbol == LexicalAnalyzer.point)
                {
                    if (currentSymbol == LexicalAnalyzer.lbracket)
                    {
                        currentSymbol = lexer.NextSym();
                        Expression();
                        if (currentSymbol != LexicalAnalyzer.rbracket)
                        {
                            InputOutput.Error(13, lexer.token); // Expected ']'
                            return;
                        }
                        currentSymbol = lexer.NextSym();
                    }
                    else // point
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
            }
            else if (currentSymbol == LexicalAnalyzer.intc || 
                     currentSymbol == LexicalAnalyzer.floatc ||
                     currentSymbol == LexicalAnalyzer.charc)
            {
                currentSymbol = lexer.NextSym();
            }
            else if (currentSymbol == LexicalAnalyzer.leftpar)
            {
                currentSymbol = lexer.NextSym();
                Expression();
                if (currentSymbol != LexicalAnalyzer.rightpar)
                {
                    InputOutput.Error(5, lexer.token); // Expected ')'
                    return;
                }
                currentSymbol = lexer.NextSym();
            }
        }
    }
}