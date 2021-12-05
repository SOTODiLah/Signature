using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Signature.Source
{
    // Класс потока читателя
    class ReaderThread : Reader
    {
        // Поток читателя
        private Thread thread;

        // Возвратная функция сообщения состояния потока
        private event Action<bool> callbackAborder;

        // Возвратная функция для передачи считанного блока
        private event Action<Block> callbackAddUnhandledBlock;

        // Конструктор класс читателя
        public ReaderThread(Stream stream, int blockSize, Action<Block> callbackAddUnhandledBlock, Action<bool> callbackAborder) : base(stream, blockSize)
        {
            this.callbackAborder = callbackAborder;
            this.callbackAddUnhandledBlock = callbackAddUnhandledBlock;
            this.thread = new Thread(ReadBlockFromFile);
            this.thread.Priority = ThreadPriority.AboveNormal;
        }

        // Функция запуска работы читателя
        public void Start()
        {
            // Присутсвует ли возвратная функция передачи блока
            if (this.callbackAddUnhandledBlock == null)
            {
                // Присутсвует ли возвратная функция сообщения состояния потока
                if (this.callbackAborder != null)
                    this.callbackAborder(true);
                return;
            }
            // Запуск потока читателя
            thread.Start();
        }

        // Функция присоединения потока читателя
        public void Join()
        {
            // Присоединение читателя
            this.thread.Join();
            // Остановка работы потока читателя
            Stop();
        }

        // Функция остановки работы читателя
        public void Stop()
        {
            this.thread.Abort();
        }

        // Функция читателя блоков из файла
        private void ReadBlockFromFile()
        {
            // получение размера файла
            long streamLength = this.stream.Length;

            // цикл чтения из файла пока он не закончился
            while (streamLength - 1 > this.stream.Position)
            {
                // отлов исключений при чтении
                try
                {
                    // Чтение одного блока из файла
                    Block nextBlock = this.GetNextBlock();
                    // Добавление необработнного блока в обработчик блок
                    this.callbackAddUnhandledBlock(nextBlock);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    if (this.callbackAborder != null)
                    {
                        this.callbackAborder(true);
                    }
                    Console.WriteLine("Block reading fail. Reading of input file are stoped.{0}", e.Message);
                    return;
                }
                catch (ThreadAbortException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
