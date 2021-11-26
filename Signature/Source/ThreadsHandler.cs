using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Signature.Source
{
    // Класс потоков обработчиков
    class ThreadsHandler
    {
        // переменная размера блока
        protected int blockSize;

        // поток ввода файла
        protected readonly Stream inputStream;

        // поток вывода файла
        protected readonly Stream outputStream;

        // объект читателя из файла
        protected Reader reader;

        // объект писателя в файл
        protected Writer writer;

        // объект обработчика блоков
        private BlocksHandler blocksHandler;

        // Запусщена ли?
        private bool isRun;

        // Завершена ли?
        private bool isAborder;

        // Дополнительный поток для читателя
        private Thread readerThread;

        // Дополнительный поток для писателя
        private Thread writerThread;

        // Коструктор класса потоков обработчиков
        public ThreadsHandler(Stream inputStream, Stream outputStream, int blockSize)
        {
            this.inputStream = inputStream;
            this.outputStream = outputStream;
            this.blockSize = blockSize;
            reader = new Reader(inputStream, blockSize);
            writer = new Writer(outputStream, Hasher.GetHashSize());
        }

        // Исполнения сигнатуры файла
        public bool Run()
        {
            // Создание обработчка блоков (количество задействованых потоков - 2, читатель и писатель), лямбда возвратной функции
            this.blocksHandler = new BlocksHandler(Environment.ProcessorCount - 2, s =>
            {
                Console.WriteLine(s);
                this.Abort();
            });

            // Запуск процесса создания сигнатуры файла
            this.Process();
            return !isAborder;
        }


        // Функция создания потоков и последующего их завершения
        private void Process()
        {
            this.readerThread = new Thread(ReadBlockFromFile);
            this.readerThread.Priority = ThreadPriority.AboveNormal;

            this.writerThread = new Thread(GetBlockAndWriteToFile);
            this.writerThread.Priority = ThreadPriority.AboveNormal;

            this.readerThread.Start();
            this.blocksHandler.Start();

            this.isRun = true;
            this.writerThread.Start();
            this.readerThread.Join();
            this.blocksHandler.Stop();
            this.isRun = false;
            this.writerThread.Join();
        }

        // Функция читателя блоков из файла
        private void ReadBlockFromFile()
        {
            // получение размера файла
            long streamLength = this.inputStream.Length;

            // цикл чтения из файла пока он не закончился
            while (streamLength - 1 > this.inputStream.Position)
            {
                // отлов исключений при чтении
                try
                {
                    // Чтение одного блока из файла
                    Block nextBlock = this.reader.GetNextBlock();

                    // Добавление необработнного блока в обработчик блок
                    this.blocksHandler.AddUnhandledBlock(nextBlock);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    this.isAborder = true;
                    Console.WriteLine("Block reading fail. Reading of input file are stoped.{0}", e.Message);
                    return;
                }
                catch (ThreadAbortException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        // Функция писателя блоков в файл
        private void GetBlockAndWriteToFile()
        {
            // Лист обработанных блоков
            List<Block> blocks;

            // Цикл получения блоков и запись в файл пока работает читатель
            do
            {
                blocks = this.blocksHandler.GetHandledBlocks();
                for (int i = 0; i < blocks.Count; i++)
                {
                    this.writer.WriteBlock(blocks[i]);
                    this.outputStream.Flush();
                }

            } while (this.isRun);

            // Запись оставшихся обработанных блоков
            blocks = this.blocksHandler.GetHandledBlocks();
            for (int i = 0; i < blocks.Count; i++)
            {
                this.writer.WriteBlock(blocks[i]);
                this.outputStream.Flush();
            }
        }

        // Функция завершения потоков
        public void Abort()
        {
            this.isAborder = true;

            if (this.readerThread != null)
            {
                this.readerThread.Abort();
            }

            if (this.blocksHandler != null)
            {
                this.blocksHandler.Abort();
            }

            if (this.writerThread != null)
            {
                this.writerThread.Abort();
            }
        }
    }
}
