using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler
{
    struct TextPosition
    {
        public uint lineNumber; // номер строки
        public byte charNumber; // номер позиции в строке

        public TextPosition(uint ln = 0, byte c = 0)
        {
            lineNumber = ln;
            charNumber = c;
        }
    }

    struct Err
    {
        public TextPosition errorPosition;
        public byte errorCode;

        public Err(TextPosition errorPosition, byte errorCode)
        {
            this.errorPosition = errorPosition;
            this.errorCode = errorCode;
        }
    }

    class InputOutput
    {
        const byte ERRMAX = 9;
        public static char Ch { get; set; }
        public static TextPosition positionNow = new();
        static string line = string.Empty;
        static int lastInLine;
        public static List<Err> err = new();
        static StreamReader File { get; set; }
        static uint errCount = 0;

        /// <summary>
        /// Установка файла для компиляции
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        public static void SetFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("Файл не найден: " + filePath);
                    return;
                }

                File = new StreamReader(filePath);
                line = File.ReadLine() ?? string.Empty;
                lastInLine = line.Length;
                err = new List<Err>();
                positionNow = new TextPosition(1, 0); // Reset position to first line
                Ch = line.Length > 0 ? line[0] : '\0';
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Нет доступа к файлу: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        /// <summary>
        /// Получение следующего символа
        /// </summary>
        static public void NextCh()
        {
            if (positionNow.charNumber >= lastInLine)
            {
                ListThisLine();
                if (err.Count > 0)
                    ListErrors();
                ReadNextLine();
                positionNow.lineNumber++;
                positionNow.charNumber = 0;
                Ch = line.Length > 0 ? line[0] : '\0';
            }
            else
            {
                positionNow.charNumber++;
                Ch = positionNow.charNumber < line.Length ? line[positionNow.charNumber] : '\0';
            }
        }

        /// <summary>
        /// Вывод строки текущей строки в консоль
        /// </summary>
        private static void ListThisLine()
        {
            Console.WriteLine($"{positionNow.lineNumber,4}: {line}");
        }

        /// <summary>
        /// Получение следующей строки
        /// </summary>
        private static void ReadNextLine()
        {
            try
            {
                if (!File.EndOfStream)
                {
                    line = File.ReadLine() ?? string.Empty;
                    lastInLine = line.Length;
                    err = new List<Err>();
                }
                else
                {
                    line = string.Empty;
                    lastInLine = 0;
                    Ch = '\0';
                    End();
                }
            }
            catch (ObjectDisposedException)
            {
                // Если файл уже закрыт, просто выходим
                return;
            }
        }

        /// <summary>
        /// Вывод сообщения о завершении в консоль
        /// </summary>
        static void End()
        {
            Console.WriteLine($"Компиляция завершена: ошибок — {errCount}!");
            if (File != null)
            {
                File.Dispose();
                File = null;
            }
        }

        /// <summary>
        /// Вывод ошибок в консоль
        /// </summary>
        static void ListErrors()
        {
            foreach (Err item in err)
            {
                ++errCount;
                string marker = errCount < 10 ? $"**0{errCount}**" : $"**{errCount}**";
                int spaces = item.errorPosition.charNumber + 6;
                Console.WriteLine($"{marker.PadRight(spaces)}^ ошибка код {item.errorCode}");
            }
        }

        /// <summary>
        /// Формирование таблицы ошибок
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="position"></param>
        static public void Error(byte errorCode, TextPosition position)
        {
            if (err.Count < ERRMAX)
            {
                err.Add(new Err(position, errorCode));
            }
        }
    }
}