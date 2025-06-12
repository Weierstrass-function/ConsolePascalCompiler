using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Compiler
{
    class Program
    {
        static void Main()
        {
            InputOutput.ReadFile("example.pas");

            // SyntaxAnalyzer s = new SyntaxAnalyzer(new LexicalAnalyzer());
            // s.Analyze();


            //LexicalAnalyzer l = new LexicalAnalyzer();
            //while (true) // Читаем до конца файл
            //{
            //    try
            //    {
            //        l.NextSym();
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            //        break;
            //    }
            //}
        }
    }
}
