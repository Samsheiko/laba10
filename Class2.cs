using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    struct TextPosition
    {
        public uint Line { get; set; }
        public int Column { get; set; }

        public TextPosition(uint line = 0, int col = 0)
        {
            Line = line;
            Column = col;
        }
    }

    struct Err
    {
        public TextPosition Pos { get; set; }
        public byte Code { get; set; }

        public Err(TextPosition pos, byte code)
        {
            Pos = pos;
            Code = code;
        }
    }

    class InputOutput
    {
        private const byte MaxErrorsInLine = 9;

        private static char ch;
        private static TextPosition currPos;
        private static string currentLine;
        private static int lastIdx;
        private static List<Err> allErrors;
        private static StreamReader srcFile;
        private static uint totalErrors;
        private static bool isEof;

        private static Dictionary<byte, string> errMsgs;
        private static Dictionary<uint, List<Err>> lineErrors;

        static InputOutput()
        {
            ch = '\0';
            currPos = new TextPosition(0, 0);
            currentLine = "";
            lastIdx = 0;
            allErrors = new List<Err>();
            srcFile = null;
            totalErrors = 0;
            isEof = false;
            lineErrors = new Dictionary<uint, List<Err>>();

            errMsgs = new Dictionary<byte, string>()
            {
                { 1, "ошибка ввода-вывода" },
                { 2, "слишком много ошибок в строке" },
                { 50, "неверный символ в программе" },
                { 51, "пропущен идентификатор" },
                { 52, "пропущена точка с запятой" },
                { 53, "пропущена точка" },
                { 54, "пропущено двоеточие" },
                { 55, "пропущена запятая" },
                { 56, "пропущена левая скобка" },
                { 57, "пропущена правая скобка" },
                { 58, "пропущен оператор присваивания :=" },
                { 100, "использование имени не соответствует описанию" },
                { 101, "ожидалось ключевое слово begin" },
                { 102, "ожидалось ключевое слово end" },
                { 103, "пропущено ключевое слово program" },
                { 147, "тип метки не совпадает с типом выбирающего выражения" },
                { 200, "целочисленная константа вне диапазона" },
                { 201, "вещественная константа вне диапазона" },
                { 202, "недопустимый символ в строке" },
                { 203, "константа превышает допустимый предел" },
                { 250, "неожиданный конец файла" }
            };
        }

        public static char Ch
        {
            get { return ch; }
            set { ch = value; }
        }

        public static TextPosition PositionNow
        {
            get { return currPos; }
            set { currPos = value; }
        }

        public static List<Err> Err
        {
            get { return allErrors; }
        }

        public static Dictionary<byte, string> ErrorTable
        {
            get { return errMsgs; }
        }

        public static void OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Ошибка: файл " + path + " не найден!");
                return;
            }

            srcFile = new StreamReader(path);
            totalErrors = 0;
            isEof = false;
            currPos = new TextPosition(1, 0);
            lineErrors = new Dictionary<uint, List<Err>>();
            allErrors.Clear();

            FetchLine();

            if (currentLine != null && currentLine.Length > 0)
            {
                ch = currentLine[0];
                lastIdx = currentLine.Length - 1;
            }
            else
            {
                ch = '\0';
                lastIdx = 0;
                isEof = true;
                FinishCompilation();
            }
        }

        public static void CloseFile()
        {
            if (srcFile != null)
            {
                srcFile.Close();
                srcFile = null;
            }
        }

        public static void NextCh()
        {
            if (isEof) return;

            if (currPos.Column >= lastIdx)
            {
                PrintCurrentLine();
                PrintErrorsForLine(currPos.Line);
                FetchLine();

                if (isEof)
                {
                    ch = '\0';
                    FinishCompilation();
                    return;
                }

                currPos.Line++;
                currPos.Column = 0;
            }
            else
            {
                currPos.Column++;
            }

            if (!isEof && currentLine != null && currPos.Column < currentLine.Length)
            {
                ch = currentLine[currPos.Column];
            }
            else
            {
                ch = '\0';
            }
        }

        public static void Error(byte code, TextPosition pos)
        {
            Err errorObj = new Err(pos, code);
            allErrors.Add(errorObj);

            if (!lineErrors.ContainsKey(pos.Line))
            {
                lineErrors[pos.Line] = new List<Err>();
            }

            if (lineErrors[pos.Line].Count < MaxErrorsInLine)
            {
                lineErrors[pos.Line].Add(errorObj);
            }
            else if (lineErrors[pos.Line].Count == MaxErrorsInLine)
            {
                lineErrors[pos.Line].Add(new Err(pos, 2));
            }
        }

        public static void PrintErrorTable()
        {
            Console.WriteLine("+------------------------------------+");
            Console.WriteLine("|           ТАБЛИЦА ОШИБОК           |");
            Console.WriteLine("+------------------------------------+");
            foreach (KeyValuePair<byte, string> entry in errMsgs)
            {
                Console.WriteLine("Код " + entry.Key.ToString().PadLeft(3) + ": " + entry.Value);
            }
            Console.WriteLine();
        }

        private static void PrintCurrentLine()
        {
            if (currentLine != null)
            {
                Console.WriteLine(currPos.Line.ToString().PadLeft(4) + " " + currentLine);
            }
        }

        private static void FetchLine()
        {
            if (srcFile != null && !srcFile.EndOfStream)
            {
                currentLine = srcFile.ReadLine();
                if (currentLine == null)
                {
                    currentLine = "";
                    isEof = true;
                }
                else
                {
                    lastIdx = currentLine.Length > 0 ? currentLine.Length - 1 : 0;
                }
            }
            else
            {
                currentLine = "";
                lastIdx = 0;
                isEof = true;
            }
        }

        private static void FinishCompilation()
        {
            Console.WriteLine();
            Console.WriteLine("Компиляция окончена: ошибок - " + totalErrors + " !");
            isEof = true;
            ch = '\0';
            CloseFile();
        }

        private static void PrintErrorsForLine(uint lineNum)
        {
            if (!lineErrors.ContainsKey(lineNum)) return;

            List<Err> currentErrors = lineErrors[lineNum];

            foreach (Err errorItem in currentErrors)
            {
                totalErrors++;

                string prefix = "**";
                if (totalErrors < 10)
                {
                    prefix += "0";
                }
                prefix += totalErrors.ToString() + " ";

                if (errMsgs.ContainsKey(errorItem.Code))
                {
                    prefix += errMsgs[errorItem.Code];
                }
                else
                {
                    prefix += "Неизвестная ошибка";
                }

                string pointers = "";
                for (int i = 0; i < 5 + errorItem.Pos.Column; i++)
                {
                    pointers += " ";
                }
                pointers += "^";

                Console.WriteLine(pointers);
                Console.WriteLine(prefix);
            }
        }
    }
}
