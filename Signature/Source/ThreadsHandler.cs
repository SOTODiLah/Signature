using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Signature.Source
{
    // Класс потоков обработчиков
    class ThreadsHandler
    {
        // объект читателя из файла
        protected ReaderThread readerThread;

        // объект писателя в файл
        protected WriterThread writerThread;

        // объект обработчика блоков
        private BlocksHandler blocksHandler;

        // Завершена ли?
        private bool isAborder;

        // Коструктор класса потоков обработчиков
        public ThreadsHandler(Stream inputStream, Stream outputStream, int blockSize)
        {
            // Инициализация обработчика блоков. Тернарная операция вычисления потоков для обработки блоков. Лямбда-функция сообщения об ошибки и завершения работы потоков.
            this.blocksHandler = new BlocksHandler(Environment.ProcessorCount > 2 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount, str =>
            {
                Console.WriteLine(str);
                this.Abort();
            });
            
            /**
            **  Инициализация класса потока читателя. 
            **  Первая лямбда-функция содержит в себе метод объекта класса обработчика блоков. 
            **  Вторая лямбда-функция изменяет состояние логической переменной "Завершена ли?".
            **/
            this.readerThread = new ReaderThread(inputStream, blockSize, block =>
            {
                this.blocksHandler.AddUnhandledBlock(block);
            }, aborder =>
            {
                isAborder = aborder;
            });

            // Инициализация класса потока писателя. Лямбда-функция сообщения об ошибке при инициализации читателя.
            this.writerThread = new WriterThread(outputStream, str =>
            {
                Console.WriteLine(str);
                this.Abort();
            }, this.blocksHandler.GetHandledBlocks);
        }

        // Исполнения сигнатуры файла
        public bool Run()
        {
            // Инициализация потоков обработчиков в классе обработчик блоков
            this.blocksHandler.Init<Hasher>();
            // Запуск процесса создания сигнатуры файла
            this.Process();
            return !isAborder;
        }


        // Функция создания потоков и последующего их завершения
        private void Process()
        {
            // Первым запускается поток читателя
            this.readerThread.Start();

            // После запускается поток обработчика блоков
            this.blocksHandler.Start();

            // Затем запускается поток писателя
            this.writerThread.Start();

            // Присоеднияем поток читателя к текущему потоку и ожидаем завершения работы читателя
            this.readerThread.Join();

            // После завершения работы читателя, присоединяем обработчик блоков и останавлияем работу в случае отсутсвия блоков в очереди.
            this.blocksHandler.Stop();

            // Присоединяем поток писателя к текущему потоку и ожидаем завершения работы читателя
            this.writerThread.Join();
        }

        // Функция завершения потоков
        public void Abort()
        {
            this.isAborder = true;

            if (this.readerThread != null)
            {
                this.readerThread.Stop();
            }

            if (this.blocksHandler != null)
            {
                this.blocksHandler.Abort();
            }

            if (this.writerThread != null)
            {
                this.writerThread.Stop();
            }
        }
    }
}
