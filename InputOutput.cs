using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler
{
    struct TextPosition
    {
        public uint lineNumber; // номер строки
        public byte charNumber; // номер позиции в строке
    }

    struct Err
    {
        public TextPosition errorPosition;
        public ushort errorCode;

        public Err(TextPosition errorPosition, ushort errorCode)
        {
            this.errorPosition = errorPosition;
            this.errorCode = errorCode;
        }
    }

    class InputOutput
    {
        const byte ERRMAX = 9;
        public static char Ch = ' ';
        public static TextPosition positionNow = new();
        static string? line = string.Empty;
        static int lastInLine = 0;
        public static List<Err> err = new();
        static StreamReader? File = null;
        static uint errCount = 0;

        /// <summary>
        /// Точка начала работы компилятора
        /// </summary>
        /// <param name="filePath"></param>
        public static void ReadFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("Файл не найден: " + filePath);
                    return;
                }

                File = new StreamReader(filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Нет доступа к файлу: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }

            if (File != null)
            {
                SyntaxAnalyzer s = new SyntaxAnalyzer(new LexicalAnalyzer());
                s.Analyze();
            }
        }

        /// <summary>
        /// Точка завершения работы компилятора
        /// </summary>
        static public void End()
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Компиляция завершена: ошибок — ");
            if (errCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{errCount}");
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.WriteLine("нет");
            }
            Console.ForegroundColor = defaultColor;

            Environment.Exit(0);
        }

        /// <summary>
        /// Получение следующего символа
        /// </summary>
        static public void NextCh()
        {
            if (Ch == '\n')
            {
                ListLine();

                if (File == null)
                {
                    return;
                }

                line = File.ReadLine();

                if (line == null)
                {
                    File.Dispose();
                    File = null;

                    Ch = '\0'; // служебный символ конца файла
                    lastInLine = 0;
                    positionNow.charNumber++;
                }
                else
                {
                    positionNow.charNumber = 0;
                    lastInLine = line.Length;
                }

                positionNow.lineNumber++;
                err = new List<Err>();
            }

            if (positionNow.charNumber < lastInLine)
            {
                Ch = line[positionNow.charNumber];
                ++positionNow.charNumber;
            }
            else if (Ch != '\0')
            {
                Ch = '\n';
            }
        }

        /// <summary>
        /// Вывод строки и ее ошибок в консоль
        /// </summary>
        private static void ListLine()
        {
            if (positionNow.lineNumber > 0)
            {
                Console.WriteLine($"{positionNow.lineNumber,4}: {line}");
            }

            if (err.Count > 0)
            {
                ListErrors();
            }
        }

        /// <summary>
        /// Вывод ошибок в консоль
        /// </summary>
        static public void ListErrors()
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (Err item in err)
            {
                ++errCount;

                string marker = string.Format("**{0:D2}**", errCount);

                int spaces = item.errorPosition.charNumber + 5;
                Console.WriteLine($"{marker.PadRight(spaces)}^ ошибка код {item.errorCode}");

                string errorMessage = GetErrorMessage(item.errorCode);
                Console.WriteLine($"****** {errorMessage}");
            }
            Console.ForegroundColor = defaultColor;
        }

        /// <summary>
        /// Добавление новой ошибки
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="position"></param>
        static public void AddError(ushort errorCode, TextPosition position)
        {
            if (err.Count < ERRMAX)
            {
                err.Add(new Err(position, errorCode));
            }
        }

        /// <summary>
        /// Получение сообщения об ошибке по коду
        /// </summary>
        private static string GetErrorMessage(ushort code)
        {
            switch (code)
            {
                // Ошибки, связанные с символами (коды совпадают с кодами символов)
                case 0: return "запрещенный символ";
                case 2: return "должно идти имя";                          // ident
                case 4: return "должен идти символ ')'";                   // rightpar
                case 5: return "должен идти символ ':'";                   // colon
                case 9: return "должен идти символ '('";                   // leftpar
                case 11: return "должен идти символ '['";                  // lbracket
                case 12: return "должен идти символ ']'";                 // rbracket
                case 14: return "должен идти символ ';'";                 // semicolon
                case 15: return "должно идти целое";                       // intc
                case 16: return "должен идти символ '='";                  // equal
                case 20: return "должен идти символ ','";                 // comma
                case 51: return "должен идти символ ':='";                // assign
                case 52: return "должно идти слово THEN";                 // thensy
                case 53: return "должно идти слово UNTIL";                // untilsy
                case 54: return "должно идти слово DO";                   // dosy
                case 56: return "должно идти слово IF";                   // ifsy
                case 61: return "должен идти символ '.'";                 // point
                case 74: return "должен идти символ '..'";                // twopoints
                case 82: return "ошибка в вещественной константе";        // floatc
                case 83: return "ошибка в символьной константе";          // charc
                case 84: return "незакрытый строковый литерал";           // stringc
                case 100: return "использование имени не соответствует описанию"; // insy
                
                case 104: return "должно идти слово END";                 // endsy
                case 113: return "должно идти слово BEGIN";              // beginsy
                case 122: return "должно идти слово PROGRAM";            // programsy

                // Остальные ошибки (перенумерованы, начиная с 1000, чтобы не пересекаться с кодами символов)
                case 1000: return "ошибка в простом типе";
                case 1001: return "имя описано повторно";                  // ofsy
                case 1002: return "ошибка в списке параметров";
                case 1003: return "ошибка в типе";
                case 1004: return "ошибка в разделе описаний";
                case 1005: return "ошибка в списке полей";
                case 1006: return "ошибка в константе";
                case 1007: return "нижняя граница превосходит верхнюю";
                case 1008: return "недопустимое рекурсивное определение";
                case 1009: return "файл здесь использовать нельзя";
                case 1010: return "тип не должен быть REAL";
                case 1011: return "несовместимость с типом дискриминанта";
                case 1012: return "недопустимый ограниченный тип";
                case 1013: return "тип основания не должен быть REAL или INTEGER";
                case 1014: return "файл должен быть текстовым";
                case 1015: return "ошибка в типе параметра стандартной процедуры";
                case 1016: return "неподходящее опережающее описание";
                case 1017: return "недопустимый тип признака вариантной части записи";
                case 1018: return "опережающее описание: повторение списка параметров не допускается";
                case 1019: return "тип результата функции должен быть скалярным, ссылочным или ограниченным";
                case 1020: return "параметр-значение не может быть файлом";
                case 1021: return "опережающее описание функции: повторять тип результата нельзя";
                case 1022: return "в описании функции пропущен тип результата";
                case 1023: return "F-формат только для REAL";
                case 1024: return "ошибка в типе параметра стандартной функции";
                case 1025: return "число параметров не согласуется с описанием";
                case 1026: return "недопустимая подстановка параметров";
                case 1027: return "тип результата функции не соответствует описанию";
                case 1028: return "выражение не относится к множественному типу";
                case 1029: return "элементы множества не должны выходить из диапазона 0 .. 255";
                case 1030: return "тип операнда должен быть BOOLEAN";
                case 1031: return "недопустимые типы элементов множества";
                case 1032: return "переменная не есть массив";
                case 1033: return "тип индекса не соответствует описанию";
                case 1034: return "переменная не есть запись";
                case 1035: return "переменная должна быть файлом или ссылкой";
                case 1036: return "недопустимая подстановка параметров";
                case 1037: return "недопустимый тип параметра цикла";
                case 1038: return "недопустимый тип выражения";
                case 1039: return "конфликт типов";
                case 1040: return "тип метки не совпадает с типом выбирающего выражения";
                case 1041: return "тип индекса не может быть REAL или INTEGER";
                case 1042: return "в этой записи нет такого поля";
                case 1043: return "метка варианта определяется несколько раз";
                case 1044: return "метка определяется несколько раз";
                case 1045: return "метка описывается несколько раз";
                case 1046: return "неописанная метка";
                case 1047: return "неопределенная метка";
                case 1048: return "ошибка в основании множества (в базовом типе)";
                case 1049: return "тип не может быть упакован";
                case 1050: return "здесь не допускается присваивание имени функции";
                case 1051: return "типы не совместны";
                case 1052: return "запрещенная в данном контексте операция";
                case 1053: return "элемент этого типа не может иметь знак";
                case 1054: return "несоответствие типов для операции отношения";
                case 1055: return "конфликт типов параметров";
                case 1056: return "повторное опережающее описание";
                case 1057: return "ошибка в конструкторе множества";
                case 1058: return "лишний индекс для доступа к элементу массива";
                case 1059: return "указано слишком мало индексов для доступа к элементу массива";
                case 1060: return "выбирающая константа вне границ описанного диапазона";
                case 1061: return "недопустимый тип выбирающей константы";
                case 1062: return "параметры процедуры (функции) должны быть параметрами-значениями";
                case 1063: return "несоответствие количества параметров параметра-процедуры (функции)";
                case 1064: return "несоответствие типов параметров параметра-процедуры (функции)";
                case 1065: return "тип параметра-функции не соответствует описанию";
                case 1066: return "ошибка в вещественной константе: должна идти цифра";
                case 1067: return "целая константа превышает предел";
                case 1068: return "деление на нуль";
                case 1069: return "слишком маленькая вещественная константа";
                case 1070: return "слишком большая вещественная константа";
                case 1071: return "недопустимые типы операндов операции IN";
                case 1072: return "вторым операндом IN должно быть множество";
                case 1073: return "операнды AND, NOT, OR должны быть булевыми";
                case 1074: return "недопустимые типы операндов операции + или -";
                case 1075: return "операнды DIV и MOD должны быть целыми";
                case 1076: return "недопустимые типы операндов операции *";
                case 1077: return "недопустимые типы операндов операции /";
                case 1078: return "первым операндом IN должно быть выражение базового типа множества";
                case 1079: return "опережающее описание есть, полного нет";
                case 1080: return "индексное значение выходит за границы";
                case 1081: return "присваиваемое значение выходит за границы";
                case 1082: return "выражение для элемента множества выходит за пределы";
                case 1083: return "выражение выходит за допустимые пределы";
                case 1084: return "ожидается конец файла";
                case 1085: return "пустой строковый литерал";
                case 1086: return "комментарий не закрыт";

                default: return "неизвестная ошибка";
            }
        }
    }
}