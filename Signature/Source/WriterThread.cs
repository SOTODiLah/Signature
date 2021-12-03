using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Signature.Source
{
    // Класс потока писателя
    class WriterThread : Writer
    {
        // Поступают ли блоки? Идёт работа?
        bool isRun;

        // Поток писателя
        private Thread thread;

        // Возвратная функция сообщения об остановки
        private event Action<string> callbackAborder;

        // Делегат функции на получение блоков писателем
        private event Func<List<Block>> delegateGetHandledBlocks;

        // Конструктор писателя, а также запуск конструктора базового класса
        public WriterThread(Stream stream, Action<string> callbackAborder, Func<List<Block>> delegateGetHandledBlocks) : base(stream)
        {
            this.callbackAborder = callbackAborder;
            this.delegateGetHandledBlocks = delegateGetHandledBlocks;
            this.isRun = false;
            this.thread = new Thread(GetBlockAndWriteToFile);
            this.thread.Priority = ThreadPriority.AboveNormal;
        }

        // Функция запуска работы писателя
        public void Start()
        {
            // Присуствует ли функция получения блоков?
            if (this.delegateGetHandledBlocks == null)
            {
                // Присутсвуется ли возвратная функция?
                if (this.callbackAborder != null)
                    this.callbackAborder("Writer can't get blocks");
            }
            // Работаем..
            this.isRun = true;
            // Запуск потока писателя
            thread.Start();
        }

        // Функция присоединения потока писателя
        public void Join()
        {
            // Остановка получения блоков, поскольку происходит присоединение
            isRun = false;
            // Присоедение потока писателя к потоку вывовшему функцию
            this.thread.Join();
            // Остановка работы писателя после завершения исполнения основной функции
            Stop();
        }

        // Функция остановки потока писателя
        public void Stop()
        {
            this.thread.Abort();
        }

        // Функция писателя блоков в файл
        private void GetBlockAndWriteToFile()
        {
            // Лист обработанных блоков
            List<Block> blocks;

            // Цикл получения блоков и запись в файл пока работает писатель
            do
            {
                // Получение листа блоков писателем
                blocks = this.delegateGetHandledBlocks();
                for (int i = 0; i < blocks.Count; i++)
                {
                    // Запись блока в файл
                    this.WriteBlock(blocks[i]);
                    this.stream.Flush();
                }

            } while (this.isRun);

            // Запись оставшихся обработанных блоков
            blocks = this.delegateGetHandledBlocks();
            for (int i = 0; i < blocks.Count; i++)
            {
                this.WriteBlock(blocks[i]);
                this.stream.Flush();
            }
        }
    }
}
