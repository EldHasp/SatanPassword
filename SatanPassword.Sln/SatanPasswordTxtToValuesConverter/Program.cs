using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

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

        static readonly int length = (int)Math.Pow(10, 8);
        static void TxtToValuesConvert(string fileTxt, string fileBin, string fileNotKey)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var timeBeg = DateTime.Now;

            uint value = 0, key = 0;
            int letter;
            uint[] values = Enumerable.Repeat(uint.MaxValue, length).ToArray();
            uint[] counts = new uint[length];
            uint foundCount = 0;
            uint countsMax = 0;
            uint countsMaxKey = 0;

            Console.WriteLine(
$@"Конвертация текстового ASCII файла ""{fileTxt}"" последовательности цифр
в массив всех значений и его сериализиция в файл ""{fileBin}"".
Ненайденные ключи будут записаны в файл ""{fileNotKey}"".");



            FileStream file;
            long position;
            using (file = File.OpenRead(fileTxt))
            {
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
                while (foundCount < length && (letter = file.ReadByte()) >= 0)
                {
                    // Получение новых значения и ключа.
                    value <<= 4;
                    value |= key >> 28;
                    key <<= 4;
                    key |= TryDigitMessagePosition(letter);
                    uint index = HexDecimalToUInt(key);

                    // Проверка был ли уже такой ключ
                    if (counts[index] == 0)
                    {
                        values[index] = value;
                        foundCount++;
                    }

                    // Инкремент счётчика ключа.
                    counts[index]++;
                    // Получение максимального счётчика
                    if (countsMax < counts[index])
                    {
                        countsMax = counts[index];
                        countsMaxKey = index;
                    }

                    // Вывод прогресса
                    if (stopwatch.ElapsedMilliseconds > 500)
                    {
                        OutputProgress();
                        stopwatch.Restart();
                    }

                }
                OutputProgress();
                position = file.Position;
            }

            Console.WriteLine($"\r\n{new string('*', 80)}\r\nРасчёт закончен. Сохранение результата.");
            using (file = File.Create(fileBin))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(file, values);
                file.Dispose();
            }

            Console.WriteLine("Сохранение списка ненайденных ключей.");
            File.WriteAllLines(fileNotKey,
                values
                .Select((value, key) => (value, key))
                .Where(vk => vk.value == uint.MaxValue)
                .Select(vk => vk.key.ToString("0000-0000")));

            Console.WriteLine(
$@"Обработано до позиции: {position}. Найдено ключей: {foundCount}. Не найденно ключей: {length - foundCount}.
Среднее количество значений на ключ: { position / (decimal)foundCount:F2}
Максимально количество {countsMax} раз встретился ключ {countsMaxKey}.
Время затраченое на ковертацию {DateTime.Now - timeBeg}");

            Console.Write("\r\nДля выхода нажмите Enter:");
            Console.ReadLine();

            uint TryDigitMessagePosition(int digit)
            {
                if (TryDigit(digit, out uint result))
                    return result;
                throw new Exception($"В позиции {file.Position} не цифра.");
            }
            void OutputProgress()
                => Console.Write($" \rОбработано: {file.Position / (decimal)file.Length:P2}. Осталось найти ключей: {length - foundCount}.");

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
