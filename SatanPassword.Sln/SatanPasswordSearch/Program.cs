using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SatanPasswordSearch
{
    class Program
    {
        private static string fileTxt = "pi.txt";
        static void Main(string[] args)
        {
            if (args.Length > 0)
                fileTxt = args[0];

            SearchValue(fileTxt);

        }

        static void SearchValue(string fileTxt)
        {

            bool isCancel = false;
            uint key;
            Stopwatch stopwatch = new Stopwatch();
            while (!isCancel)
            {
                while (!TryUIntInput("Введите ключ - восемь цифр второй части пароля (Пустая строка -- Cancel): ",
                    out key, out isCancel))
                {
                    Console.WriteLine("Ошибка ввода! Повторите!");
                }
                if (!isCancel)
                {
                    stopwatch.Restart();
                    uint value = SearchInFile(fileTxt, key);
                    stopwatch.Stop();

                    if (value == uint.MaxValue)
                        Console.Write("Нет значения для этого ключа.");
                    else
                        Console.Write($"Первая часть пароля {Convert.ToString(value, 16)}.");
                    Console.WriteLine($" Время поиска {stopwatch.ElapsedMilliseconds} мс.");
                }
            }


        }

        static readonly uint keyMax = (uint)Math.Pow(10, 8);
        static bool TryUIntInput(string message, out uint result, out bool cancel)
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
                result = num;
                return true;
            }
            return false;
        }

        static uint SearchInFile(string fileTxt, uint key)
        {
            IReadOnlyList<byte> asciiDigits = GetAsciiDigits(key);

            byte[] buffer = new byte[1_000_000 + 15];
            byte[] result = new byte[8];
            using (var file = File.OpenRead(fileTxt))
            {
                file.Read(buffer, 0, 15);

                int readLength;
                while ((readLength = file.Read(buffer, 15, 1_000_000)) > 0)
                {
                    for (int i = 8; i < readLength + 8; i++)
                    {
                        if (asciiDigits[0] == buffer[i])
                        {
                            int j = 1;
                            for (; j < 8 && asciiDigits[j] == buffer[i + j]; j++);

                            if (j ==8)
                            {
                                Array.Copy(buffer, i - 8, result, 0, 8);
                                return AsciiToUInt(result);
                            }
                        }
                    }
                    Array.Copy(buffer, readLength - 15, buffer, 0, 15);
                }
            }
            return uint.MaxValue;
        }

        static IReadOnlyList<byte> GetAsciiDigits(uint number)
        {
            byte[] array = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                array[7 - i] = (byte)((number % 10) + 0x30);
                number /= 10;
            }
            return Array.AsReadOnly(array);
        }

        static uint AsciiToUInt(IEnumerable<byte> bytes)
        {
            uint result = 0;
            foreach (var digit in bytes)
            {
                result *= 10;
                result += (uint)digit - 0x30;
            }
            return result;
        }
    }
}
