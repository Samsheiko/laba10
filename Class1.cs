using System;
using System.IO;

namespace Компилятор
{
    static class InputOutputTests
    {
        private static string CreateTestFile(string name, string data)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory + name;
            File.WriteAllText(dir, data);
            return dir;
        }

        private static void TestRealPascalCode()
        {
            Console.WriteLine("+------------------------------------+");
            Console.WriteLine("|   ТЕСТИРОВАНИЕ МОДУЛЯ ВВОДА-ВЫВОДА |");
            Console.WriteLine("+------------------------------------+");
            Console.WriteLine();

            string code =
                "program example ( input, output );\n" +
                "const c = 3;\n" +
                "b = 56;\n" +
                "var a : 'a' .. 'c';\n" +
                "k, i : integer;\n" +
                "begin\n" +
                "read ( k, i );\n" +
                "for a := 'a' to 'c' do\n" +
                "case a of\n" +
                "k : i := i * k;\n" +
                "'b' : i := i + 1;\n" +
                "i : k := k + 2;\n" +
                "b : i := i - k;\n" +
                "c : i := ( i + k ) * 2\n" +
                "end;\n" +
                "writeln( i, k )\n" +
                "end.";

            string path = CreateTestFile("test_real.pas", code);
            InputOutput.OpenFile(path);

            InputOutput.Error(100, new TextPosition(10, 0));
            InputOutput.Error(100, new TextPosition(12, 0));
            InputOutput.Error(147, new TextPosition(13, 0));
            InputOutput.Error(147, new TextPosition(14, 0));

            while (true)
            {
                if (InputOutput.Ch == '\0')
                {
                    break;
                }
                InputOutput.NextCh();
            }

            InputOutput.CloseFile();
        }

        public static void RunAllTests()
        {
            InputOutput.PrintErrorTable();
            TestRealPascalCode();
            Console.WriteLine("\n=== Тест завершён ===");
        }
    }
}
