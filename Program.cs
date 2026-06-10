using System;

namespace Компилятор
{
    class Program
    {
        static void Main(string[] args)
        {
            InputOutputTests.RunAllTests();

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}