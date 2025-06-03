using System;
namespace Compiler
{
    class LexicalAnalyzer
    {
        public const byte
            star = 21, // *
            slash = 60, // /
            equal = 16, // =
            comma = 20, // ,
            semicolon = 14, // ;
            colon = 5, // :
            point = 61,	// .
            arrow = 62,	// ^
            leftpar = 9,	// (
            rightpar = 4,	// )
            lbracket = 11,	// [
            rbracket = 12,	// ]
            flpar = 63,	// {
            frpar = 64,	// }
            later = 65,	// <
            greater = 66,	// >
            laterequal = 67,	//  <=
            greaterequal = 68,	//  >=
            latergreater = 69,	//  <>
            plus = 70,	// +
            minus = 71,	// –
            lcomment = 72,	//  (*
            rcomment = 73,	//  *)
            assign = 51,	//  :=
            twopoints = 74,	//  ..

            // Constantldentifier, Variableldentifier, Fieldldentifier. Boundldentifier.
            // Typeldentifier. Procedureldentifier and Functionldentifier
            ident = 2,	// идентификатор 
            
            floatc = 82,	// вещественная константа
            intc = 15,	// целая константа
            charc = 83,	// символьная константа
            casesy = 31,
            elsesy = 32,
            filesy = 57,
            gotosy = 33,
            thensy = 52,
            typesy = 34,
            untilsy = 53,
            dosy = 54,
            withsy = 37,
            ifsy = 56,
            insy = 100,
            ofsy = 101,
            orsy = 102,
            tosy = 103,
            endsy = 104,
            varsy = 105,
            divsy = 106,
            andsy = 107,
            notsy = 108,
            forsy = 109,
            modsy = 110,
            nilsy = 111,
            setsy = 112,
            beginsy = 113,
            whilesy = 114,
            arraysy = 115,
            constsy = 116,
            labelsy = 117,
            downtosy = 118,
            packedsy = 119,
            recordsy = 120,
            repeatsy = 121,
            programsy = 122,
            functionsy = 123,
            procedurensy = 124,
            stringc = 84;
        //public const byte stringc = 84;    // строковая константа

        private Keywords keywords;

        public byte symbol; // код символа
        public TextPosition token; // позиция символа
        string addrName; // адрес идентификатора в таблице имен
        int nmb_int; // значение целой константы
        float nmb_float; // значение вещественной константы
        char one_symbol; // значение символьной константы

        public LexicalAnalyzer()
        {
            keywords = new Keywords();
        }

        /// <summary>
        /// Получение следующего символа
        /// </summary>
        /// <returns>Код символа</returns>
        public byte NextSym()
        {
            while (char.IsWhiteSpace(InputOutput.Ch) || InputOutput.Ch == '\0') InputOutput.NextCh();
            token.lineNumber = InputOutput.positionNow.lineNumber;
            token.charNumber = InputOutput.positionNow.charNumber;

           //сканировать символ
            switch (InputOutput.Ch)
            {
                // case <идентификатор или ключевое слово> :
                case char c when ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')):
                    string name = "";
                    while ((InputOutput.Ch >= 'a' && InputOutput.Ch <= 'z') ||
                            (InputOutput.Ch >= 'A' && InputOutput.Ch <= 'Z') ||
                            (InputOutput.Ch >= '0' && InputOutput.Ch <= '9'))
                    {
                        name += InputOutput.Ch;
                        InputOutput.NextCh();
                    }
                    
                    // Проверяем является ли слово ключевым
                    bool isKeyword = false;
                    foreach(var kvp in keywords.Kw)
                    {
                        if(kvp.Key == name.Length && kvp.Value.ContainsKey(name.ToLower()))
                        {
                            symbol = kvp.Value[name.ToLower()];
                            isKeyword = true;
                            break;
                        }
                    }
                    
                    if(!isKeyword)
                    {
                        symbol = ident;
                        addrName = name;
                    }
                    break;

                // case <целая константа> | <вещественная константа> :
                case char c when (c >= '0' && c <= '9'):
                    byte digit;
                    Int16 maxint = Int16.MaxValue;
                    nmb_int = 0;
                    while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
                    {
                        digit = (byte)(InputOutput.Ch - '0');
                        if (nmb_int < maxint / 10 ||
                        (nmb_int == maxint / 10 &&
                        digit <= maxint % 10))
                            nmb_int = 10 * nmb_int + digit;
                        else
                        {
                            // константа превышает предел
                            InputOutput.Error(203, InputOutput.positionNow);
                            nmb_int = 0;
                            while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9') InputOutput.NextCh();
                        }
                        InputOutput.NextCh();
                    }
                    
                    // проверка на вещественное число
                    if (InputOutput.Ch == '.')
                    {
                        InputOutput.NextCh();
                        if (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
                        {
                            nmb_float = nmb_int;
                            float scale = 0.1f;
                            while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
                            {
                                digit = (byte)(InputOutput.Ch - '0');
                                nmb_float += digit * scale;
                                scale *= 0.1f;
                                InputOutput.NextCh();
                            }
                            
                            // проверка на экспоненциальную форму
                            if (InputOutput.Ch == 'E' || InputOutput.Ch == 'e')
                            {
                                InputOutput.NextCh();
                                bool negative = false;
                                if (InputOutput.Ch == '+') InputOutput.NextCh();
                                else if (InputOutput.Ch == '-')
                                {
                                    negative = true;
                                    InputOutput.NextCh();
                                }
                                
                                if (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
                                {
                                    int exponent = 0;
                                    while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
                                    {
                                        exponent = exponent * 10 + (InputOutput.Ch - '0');
                                        InputOutput.NextCh();
                                    }
                                    if (negative) exponent = -exponent;
                                    nmb_float *= (float)Math.Pow(10, exponent);
                                }
                                else
                                {
                                    InputOutput.Error(206, InputOutput.positionNow); // Ожидались цифры после E
                                }
                            }
                            symbol = floatc;
                        }
                        else
                        {
                            symbol = intc;
                            InputOutput.NextCh(); // пропуск точки
                        }
                    }
                    else
                    {
                        symbol = intc;
                    }
                    break;
                   
                // // case <символьная константа> :
                // case '\'':
                //     InputOutput.NextCh();
                //     if (InputOutput.Ch == '\'')
                //     {
                //         InputOutput.Error(205, InputOutput.positionNow); // Empty character constant
                //         symbol = charc;
                //         one_symbol = ' ';
                //     }
                //     else
                //     {
                //         one_symbol = InputOutput.Ch;
                //         InputOutput.NextCh();
                //         if (InputOutput.Ch == '\'')
                //         {
                //             symbol = charc;
                //             InputOutput.NextCh();
                //         }
                //         else
                //         {
                //             InputOutput.Error(204, InputOutput.positionNow);
                //             symbol = charc;
                //         }
                //     }
                //     break;

                // case CharacterString
                case '\'':
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '\'')
                    {
                        InputOutput.Error(205, InputOutput.positionNow); // Empty character constant
                        symbol = charc;
                        one_symbol = ' ';
                        InputOutput.NextCh();
                    }
                    else
                    {
                        char firstChar = InputOutput.Ch;
                        InputOutput.NextCh();
                        
                        if (InputOutput.Ch == '\'') 
                        {
                            // Single character - treat as char constant
                            symbol = charc;
                            one_symbol = firstChar;
                            InputOutput.NextCh();
                        }
                        else
                        {
                            // Multi-character string
                            while (true)
                            {
                                if (InputOutput.Ch == '\'')
                                {
                                    InputOutput.NextCh();
                                    if (InputOutput.Ch == '\'') // Escaped quote
                                    {
                                        InputOutput.NextCh();
                                    }
                                    else
                                    {
                                        break; // End of string
                                    }
                                }
                                else if (InputOutput.Ch == '\n' || InputOutput.Ch == '\0')
                                {
                                    InputOutput.Error(204, InputOutput.positionNow); // Unclosed string
                                    break;
                                }
                                else
                                {
                                    InputOutput.NextCh();
                                }
                            }
                            symbol = stringc;
                        }
                    }
                    break;

                // < | <= | <>
                case '<':
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '=')
                    {
                        symbol = laterequal; InputOutput.NextCh();
                    }
                    else if (InputOutput.Ch == '>')
                    {
                        symbol = latergreater; InputOutput.NextCh(); // <> неравенство в Pascal
                    }
                    else
                        symbol = later;
                    break;
            
                // > | >=
                case '>':
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '=') 
                    {
                        symbol = greaterequal; InputOutput.NextCh();
                    }
                    else
                        symbol = greater;
                    break;

                // : | :=
                case ':':
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '=')
                    {
                        symbol = assign; InputOutput.NextCh();
                    }
                    else
                        symbol = colon;
                    break;

                case '+':
                    symbol = plus;
                    InputOutput.NextCh();
                    break;
                case '-':
                    symbol = minus;
                    InputOutput.NextCh();
                    break;

                    // * | *)
                case '*':
                    InputOutput.NextCh();
                    if (InputOutput.Ch == ')')
                    {
                        symbol = rcomment;
                        InputOutput.NextCh();
                    }
                    else
                        symbol = star;
                    break;

                case '/':
                    symbol = slash;
                    InputOutput.NextCh();
                    break;
                case '=':
                    symbol = equal;
                    InputOutput.NextCh();
                    break;

                // ( | (*    
                case '(':
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '*')
                    {
                        symbol = lcomment;
                        InputOutput.NextCh();
                    }
                    else
                        symbol = leftpar;
                    break;

                case ')':
                    symbol = rightpar;
                    InputOutput.NextCh();
                    break;

                case '{':
                    symbol = flpar;
                    InputOutput.NextCh();
                    break;
                case '}':
                    symbol = frpar;
                    InputOutput.NextCh();
                    break;
                case '[':
                    symbol = lbracket;
                    InputOutput.NextCh();
                    break;
                case ']':
                    symbol = rbracket;
                    InputOutput.NextCh();
                    break;
                case ',':
                    symbol = comma;
                    InputOutput.NextCh();
                    break;
                case '^':
                    symbol = arrow;
                    InputOutput.NextCh();
                    break;

                case ';':
                    symbol = semicolon;
                    InputOutput.NextCh();
                    break;

                // . | ..
                case '.':
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '.')
                    {
                        symbol = twopoints; InputOutput.NextCh();
                    }
                    else symbol = point;
                    break;

                default:
                    InputOutput.Error(6, InputOutput.positionNow);
                    InputOutput.NextCh();
                    symbol = 0;
                    break;
            }


            return symbol;
        }


    }
}
