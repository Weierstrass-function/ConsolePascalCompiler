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
                string errorMessage = GetErrorMessage(item.errorCode);
                Console.WriteLine($"****** {errorMessage}");
            }
        }

        /// <summary>
        /// Получение сообщения об ошибке по коду
        /// </summary>
        private static string GetErrorMessage(ushort code)
        {
            switch (code)
            {
                case 1: return "ошибка в простом типе";
                case 2: return "должно идти имя";
                case 3: return "должно быть служебное слово PROGRAM";
                case 4: return "должен идти символ ')'";
                case 5: return "должен идти символ ':'";
                case 6: return "запрещенный символ";
                case 7: return "ошибка в списке параметров";
                case 8: return "должно идти OF";
                case 9: return "должен идти символ '('";
                case 10: return "ошибка в типе";
                case 11: return "должен идти символ '['";
                case 12: return "должен идти символ ']'";
                case 13: return "должно идти слово END";
                case 14: return "должен идти символ ';'";
                case 15: return "должно идти целое";
                case 16: return "должен идти символ '='";
                case 17: return "должно идти слово BEGIN";
                case 18: return "ошибка в разделе описаний";
                case 19: return "ошибка в списке полей";
                case 20: return "должен идти символ ','";
                case 50: return "ошибка в константе";
                case 51: return "должен идти символ ':='";
                case 52: return "должно идти слово THEN";
                case 53: return "должно идти слово UNTIL";
                case 54: return "должно идти слово DO";
                case 55: return "должно идти слово TO или DOWNTO";
                case 56: return "должно идти слово IF";
                case 58: return "должно идти слово TO или DOWNTO";
                case 61: return "должен идти символ '.'";
                case 74: return "должен идти символ '..'";
                case 75: return "ошибка в символьной константе";
                case 76: return "слишком длинная строковая константа";
                case 86: return "комментарий не закрыт";
                case 100: return "использование имени не соответствует описанию";
                case 101: return "имя описано повторно";
                case 102: return "нижняя граница превосходит верхнюю";
                case 104: return "имя не описано";
                case 105: return "недопустимое рекурсивное определение";
                case 108: return "файл здесь использовать нельзя";
                case 109: return "тип не должен быть REAL";
                case 111: return "несовместимость с типом дискриминанта";
                case 112: return "недопустимый ограниченный тип";
                case 114: return "тип основания не должен быть REAL или INTEGER";
                case 115: return "файл должен быть текстовым";
                case 116: return "ошибка в типе параметра стандартной процедуры";
                case 117: return "неподходящее опережающее описание";
                case 118: return "недопустимый тип пpизнака ваpиантной части записи";
                case 119: return "опережающее описание: повторение списка параметров не допускается";
                case 120: return "тип результата функции должен быть скалярным, ссылочным или ограниченным";
                case 121: return "параметр-значение не может быть файлом";
                case 122: return "опережающее описание функции: повторять тип результата нельзя";
                case 123: return "в описании функции пропущен тип результата";
                case 124: return "F-формат только для REAL";
                case 125: return "ошибка в типе параметра стандартной функции";
                case 126: return "число параметров не согласуется с описанием";
                case 127: return "недопустимая подстановка параметров";
                case 128: return "тип результата функции не соответствует описанию";
                case 130: return "выражение не относится к множественному типу";
                case 131: return "элементы множества не должны выходить из диапазона 0 .. 255";
                case 135: return "тип операнда должен быть BOOLEAN";
                case 137: return "недопустимые типы элементов множества";
                case 138: return "переменная не есть массив";
                case 139: return "тип индекса не соответствует описанию";
                case 140: return "переменная не есть запись";
                case 141: return "переменная должна быть файлом или ссылкой";
                case 142: return "недопустимая подстановка параметров";
                case 143: return "недопустимый тип параметра цикла";
                case 144: return "недопустимый тип выражения";
                case 145: return "конфликт типов";
                case 147: return "тип метки не совпадает с типом выбирающего выражения";
                case 149: return "тип индекса не может быть REAL или INTEGER";
                case 152: return "в этой записи нет такого поля";
                case 156: return "метка варианта определяется несколько раз";
                case 165: return "метка определяется несколько раз";
                case 166: return "метка описывается несколько раз";
                case 167: return "неописанная метка";
                case 168: return "неопределенная метка";
                case 169: return "ошибка в основании множества (в базовом типе)";
                case 170: return "тип не может быть упакован";
                case 177: return "здесь не допускается присваивание имени функции";
                case 182: return "типы не совместны";
                case 183: return "запрещенная в данном контексте операция";
                case 184: return "элемент этого типа не может иметь знак";
                case 186: return "несоответствие типов для операции отношения";
                case 189: return "конфликт типов параметров";
                case 190: return "повторное опережающее описание";
                case 191: return "ошибка в конструкторе множества";
                case 193: return "лишний индекс для доступа к элементу массива";
                case 194: return "указано слишком мало индексов для доступа к элементу массива";
                case 195: return "выбирающая константа вне границ описанного диапазона";
                case 196: return "недопустимый тип выбирающей константы";
                case 197: return "параметры процедуры (функции) должны быть параметрами-значениями";
                case 198: return "несоответствие количества параметров параметра-процедуры (функции)";
                case 199: return "несоответствие типов параметров параметра-процедуры (функции)";
                case 200: return "тип парамера-функции не соответствует описанию";
                case 201: return "ошибка в вещественной константе: должна идти цифра";
                case 203: return "целая константа превышает предел";
                case 204: return "деление на нуль";
                case 206: return "слишком маленькая вещественная константа";
                case 207: return "слишком большая вещественная константа";
                case 208: return "недопустимые типы операндов операции IN";
                case 209: return "вторым операндом IN должно быть множество";
                case 210: return "операнды AND, NOT, OR должны быть булевыми";
                case 211: return "недопустимые типы операндов операции + или -";
                case 212: return "операнды DIV и MOD должны быть целыми";
                case 213: return "недопустимые типы операндов операции *";
                case 214: return "недопустимые типы операндов операции /";
                case 215: return "первым операндом IN должно быть выражение базового типа множества";
                case 216: return "опережающее описание есть, полного нет";
                case 305: return "индексное значение выходит за границы";
                case 306: return "присваиваемое значение выходит за границы";
                case 307: return "выражение для элемента множества выходит за пределы";
                case 308: return "выражение выходит за допустимые пределы";
                default: return "неизвестная ошибка";
            }
        }

        /// <summary>
        /// Формирование таблицы ошибок
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="position"></param>
        static public void Error(ushort errorCode, TextPosition position)
        {
            if (err.Count < ERRMAX)
            {
                err.Add(new Err(position, errorCode));
            }
        }
    }
}