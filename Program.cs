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
            InputOutput.SetFile("example.pas");
                
            while (true) // Читаем до конца файл
            {
                try
                {
                    InputOutput.NextCh();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
                    break;
                }
            }
        }
    }
}
