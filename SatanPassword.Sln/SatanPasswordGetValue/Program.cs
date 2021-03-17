using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SatanPasswordGetValue
{
    class Program
    {
        private static string fileBin = "pi.bin";

        static void Main(string[] args)
        {
            if (args.Length > 0)
                fileBin = args[0];

            KeyToValue(fileBin);

        }

        static void KeyToValue(string fileBin)
        {
            uint[] values;
            using (var file = File.OpenRead(fileBin))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                values = (uint[])formatter.Deserialize(file);
            }

            bool isCancel = false;
            int key;
            while (!isCancel)
            {
                while (!TryIntInput("Введите ключ - восемь цифр второй части пароля (Пустая строка -- Cancel): ",
                    out key, out isCancel))
                {
                    Console.WriteLine("Ошибка ввода! Повторите!");
                }
                if (!isCancel)
                {
                    uint value = values[key];
                    if (value == 0xFF_FF_FF_FF)
                        Console.WriteLine("Нет значения для этого кляюча.");
                    else
                        Console.WriteLine($"Первая часть пароля {Convert.ToString(value, 16)}.");
                }
            }


        }

        static readonly uint keyMax = (uint)Math.Pow(10, 8);
        static bool TryIntInput(string message, out int result, out bool cancel)
        {
            result = default;
            cancel = default;
            Console.Write(message);
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                cancel = true;
                return true;
            }
            if (uint.TryParse(input, out uint num) && num < keyMax)
            {
                result = (int) num;
                return true;
            }
            return false;
        }
    }
}
