using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
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
        public static TextPosition positionNow = new TextPosition();
        static string line;
        static byte lastInLine = 0;
        public static List<Err> err;
        static StreamReader File { get;  set; }
        static uint errCount = 0;

        static public void NextCh()
        {
            if (positionNow.charNumber == lastInLine)
            {
                ListThisLine();
                if (err.Count > 0)
                        ListErrors();
                ReadNextLine();
                positionNow.lineNumber = positionNow.lineNumber + 1;
                positionNow.charNumber = 0;
            }
            else ++positionNow.charNumber;
            Ch = line[positionNow.charNumber];
        }

        private static void ListThisLine()
        {
            Console.WriteLine(line);
        }

        private static void ReadNextLine()
        {
            if (!File.EndOfStream)
            {
                line = File.ReadLine();
                err = new List<Err>();
            }
            else
            {
                End();
            }
        }

        static void End()
        {
            Console.WriteLine($"Компиляция завершена: : ошибок — {errCount}!");
        }

        static void ListErrors()
        {
            int pos = 6 - $"{positionNow.lineNumber} ".Length;
            string s;
            foreach (Err item in err)
            {
                ++errCount;
                s = "**";
                if (errCount < 10) s += "0";
                s += $"{errCount}**";
                while (s.Length - 1 < pos + item.errorPosition.charNumber) s += " ";
                s += $"^ ошибка код {item.errorCode}";
                Console.WriteLine(s);
            }
        }

        static public void Error(byte errorCode, TextPosition position)
        {
            Err e;
            if (err.Count <= ERRMAX)
            {
                e = new Err(position, errorCode);
                err.Add(e);
            }
        }
    }
}