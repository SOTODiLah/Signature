using System;
using System.IO;
using Signature.Source;

namespace Signature
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Создание объекта класса настроек
            Options options = new Options();

            // Обработка агрументов командной строки
            CommandLine.Parser.Default.ParseArguments(args, options);

            // Проверка на пустые поля названия файлов
            if (string.IsNullOrEmpty(options.InputFileName) || string.IsNullOrEmpty(options.OutputFileName))
            {
                Console.WriteLine(0);
                return;
            }

            // Создание объекта класса программы
            Program program = new Program();
            // Завершение дополнительных потоков в случае закрытия программы
            Console.CancelKeyPress += program.Handler;

            // Запуск исполнения сигнатуры файла
            program.RunConsole(options);
        }

        // класс мультипоточного обрабочика
        private ThreadsHandler threadsHandler;

        // поток файла ввода
        private Stream inputStream;

        // поток файла вывода
        private Stream outputStream;

        // функция исполнения сигнатуры файла
        private void RunConsole(Options options)
        {
            // блоки отлова исключений
            try
            {
                // попытка получения файлов ввода по названия пользователя
                inputStream = GetInputStream(options.InputFileName);
                outputStream = GetOutputStream(options.OutputFileName);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(0);
                return;
            }
            catch (FileLoadException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(0);
                return;
            }

            this.threadsHandler = null;
            this.threadsHandler = new ThreadsHandler(inputStream, outputStream, options.sizeBlock);

            // вывод результата работы программы
            var result = this.threadsHandler.Run();
            Console.WriteLine(result ? "Completed signature file." : "Didn't complete signature file.");

            // закрытие потоков файлов
            this.DisposeStreams();
        }

        // функция уничтожения потоков в преждевременного закрытия программы
        private void Handler(object sender, ConsoleCancelEventArgs args)
        {
            if (this.threadsHandler != null)
            {
                this.threadsHandler.Abort();
            }

            args.Cancel = true;
        }

        // функция попытки открытия файла для потока ввода
        private Stream GetInputStream(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException("File not found.", inputFile);
            }

            return File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        // функция попытки создания/открытия файла для потока вывода
        private Stream GetOutputStream(string outputFile)
        {
            if (File.Exists(outputFile))
            {
                // спрашиваем пользователя о необходимости продолжить работу в случае уже существуещего файла для потока вывода
                Console.WriteLine("File {0} already exist. Override? (y/n)", outputFile);
                bool isOverride = GetConsoleResult();
                if (isOverride)
                {
                    return File.Open(outputFile, FileMode.Truncate, FileAccess.Write, FileShare.Read);
                }

                throw new FileLoadException("Can't load file.", outputFile);
            }

            return File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        // функция закрытия потоков и дискрипторов файлов
        private void DisposeStreams()
        {
            if (inputStream != null)
            {
                inputStream.Close();
                inputStream.Dispose();
            }

            if (outputStream != null)
            {
                outputStream.Close();
                outputStream.Dispose();
            }
        }

        // функция обработки ответа пользователя на вопрос о продолжении программы
        private static bool GetConsoleResult()
        {
            string str;
            // цикл ожидания корректного ответа
            do
            {
                str = Console.ReadLine();
                if (str != null)
                    str = str.ToLower();
            }
            while (str != "yes" && str != "y" && str != "no" && str != "n");
            return str == "yes" || str == "y";
        }
    }
}
