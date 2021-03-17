using System;
using System.IO;
using System.Linq;

namespace SatanPasswordTxtToValuesConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileTxt = "pi.txt";
            string fileBin = "pi.bin";
            string fileNotKey = "not-key.txt";

            if (args.Length > 0)
                fileTxt = args[0];
            if (args.Length > 1)
                fileBin = args[1];
            if (args.Length > 2)
                fileNotKey = args[2];

            TxtToValuesConvert(fileTxt, fileBin, fileNotKey);

        }

      static  readonly int length = (int)Math.Pow(10, 8);
        static void TxtToValuesConvert(string fileTxt, string fileBin, string fileNotKey)
        {
            uint value = 0, key = 0;
            int letter;
            uint[] values = Enumerable.Repeat(0xFF_FF_FF_FF, length).ToArray();
            uint[] counts = new uint[length];
            uint countsMax = 0;
            int countsMaxKey = 0;

            var file = File.OpenRead(fileBin);

            // Заполнение начального значения
            for (int i = 0; i < 7; i++)
            {
                letter = file.ReadByte();
                value |= TryDigitMessagePosition(letter) << (7 - i) * 4;
            }

            // Заполнение начального ключа
            for (int i = 0; i < 8; i++)
            {
                letter = file.ReadByte();
                key |= TryDigitMessagePosition(letter) << (8 - i) * 4;
            }

            //Посимвольное считывание
            while ((letter = file.ReadByte()) >= 0)
            {
                // Получение новых значения и ключа.
                value <<= 4;
                value |= key >> 28;
                key <<= 4;
                key |= TryDigitMessagePosition(letter);
                uint index = HexDecimalToUInt(key);

                // Проверка был ли уже такой ключ
                if (counts[index]==0)
                    values[index] = value;

                counts[index]++;

            }

            uint TryDigitMessagePosition(int digit)
            {
                if (TryDigit(digit, out uint result))
                    return result;
                throw new Exception($"В позиции {file.Position} не цифра.");
            }
        }

        static bool TryDigit(int digit, out uint result)
        {
            digit -= 0x30;
            if (digit < 0 || digit > 9)
            {
                result = default;
                return false;
            }
            result = (uint)digit;
            return true;
        }

        static uint HexDecimalToUInt(uint hexDecimal)
        {
            uint result = 0;
            for (int i = 0; i < 8; i++)
            {
                result *= 10;
                result += hexDecimal >> 28;
                hexDecimal <<= 4;
            }
            return result;
        }
    }
}
