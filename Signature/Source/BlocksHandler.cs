using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Signature.Source
{
    // Обработчик блоков
    class BlocksHandler
    {
        // Поступают ли блоки? Идёт работа?
        bool isRun;
        // Объект для блокировки доступа к очереди введённых блоков
        private readonly object inputLockObject;

        // Объект для блокировки допступа к очереди обработанных блоков
        private readonly object outputLockObject;

        // Очередь введённых блоков
        private readonly Queue<Block> input;

        // Очередь обработанных блоков
        private readonly Queue<Block> output;

        // Потоки обработки блоков
        private readonly Thread[] workers;

        // Максимальный размер очередей
        private const int MAX_QUEUE_SIZE = 30;

        // Возвратная функция в случае исключения
        private event Action<string> callback;

        // Дескрипор ожидания события на добавления необработанного блока
        private readonly EventWaitHandle addMutexUnhandledBlock = new EventWaitHandle(false, EventResetMode.AutoReset);

        // Дескриптор ожидания события на получения необработанного блока
        private readonly EventWaitHandle getMutexUnhandledBlock = new EventWaitHandle(false, EventResetMode.AutoReset);

        // Дескриптор ожидания события на добавление обработанного блока
        private readonly EventWaitHandle addMutexHandledBlock = new EventWaitHandle(false, EventResetMode.AutoReset);

        // Конструктор класса обработчика блоков
        public BlocksHandler(int threadsCount, Action<string> callback)
        {
            this.inputLockObject = new object();
            this.outputLockObject = new object();

            this.input = new Queue<Block>();
            this.output = new Queue<Block>();

            this.workers = new Thread[threadsCount];

            isRun = false;
            
            this.callback = callback;
        }

        // Создание дополнительных потоков для обработки блоков
        public void Init<T>() where T : BaseHasher, new()
        {
            for (int i = 0; i < this.workers.Length; i++)
            {
                this.workers[i] = new Thread(() => Run<T>());
                this.workers[i].Priority = ThreadPriority.Lowest;
            }
        }

        // Старт обработки блоков
        public void Start() 
        {
            isRun = true;
            for (int i = 0; i < this.workers.Length; i++)
            {
                this.workers[i].IsBackground = true;
                this.workers[i].Start();
            }
        }

        // Стоп обработки блоков
        public void Stop()
        {
            isRun = false;
            foreach (var w in this.workers)
            {
                w.Join();
            }
        }

        // Функция испольнитель для потоков обработки блоков
        private void Run<T>() where T : BaseHasher, new()
        {
            // Объкта хэширования блоков
            BaseHasher hasher = new T();
            // бесконечный цикл пока блоки поступают в обработчик
            while (true)
            {
                Block block = null;
                // Отлов исключений при обработки блоков
                try
                {
                    // Блокировка доступа к очереди блоков
                    lock (this.inputLockObject)
                    {
                        // Проверка наличия блоков
                        if (this.input.Count > 0)
                        {
                            block = this.input.Dequeue();
                            // Завершение работы потока
                            if (block.Data == null)
                                return;
                        }
                        // Завершение работы потока при условии что блоки больше не поступают и очередь пустая
                        else if (!isRun)
                        {
                            return;
                        }
                    }
                    // Сообщение о добавлении необработнного блока
                    this.addMutexUnhandledBlock.Set();

                    // Проверка наличия блока
                    if (block != null)
                    {
                        // Хэширования блока и добавление в очередь
                        hasher.GetHashBlock(block);
                        this.AddHandledBlock(block);
                    }
                    else
                    {
                        // Ожидания необработанных блоков 1 миллисекунд
                        this.getMutexUnhandledBlock.WaitOne(1);
                    }
                }
                // Исключение о завершении потока
                catch (ThreadAbortException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
                // Иные исключения
                catch (Exception e)
                {
                    if (callback != null)
                    {
                        callback(e.Message);
                    }
                    return;
                }
            }
        }

        // Завершение потоков потоков обработки
        public void Abort()
        {
            for (int i = 0; i < this.workers.Length; i++)
            {
                this.workers[i].Abort();
            }
        }

        // Функция добавление необработанного блока
        public void AddUnhandledBlock(Block block)
        {
            // Проверка заполнена ли очередь
            bool isFullQueue;
            lock (this.inputLockObject)
            {
                isFullQueue = this.input.Count >= MAX_QUEUE_SIZE;
            }

            // Ожидание события в случае заполнености очереди
            if (isFullQueue)
                this.addMutexUnhandledBlock.WaitOne();

            // Блокировка доступка к очереди введённых блоков
            lock (this.inputLockObject)
            {
                this.input.Enqueue(block);
            }

            // Сообщение о доступности необработных блов
            this.getMutexUnhandledBlock.Set();
        }

        // Функция получения обработнных блоков
        public List<Block> GetHandledBlocks()
        {
            List<Block> result;
            lock (this.outputLockObject)
            {
                result = this.output.ToList();
                this.output.Clear();
            }

            // Сообщение о возможности добавление обработнных блоков
            this.addMutexHandledBlock.Set();
            return result;
        }

        // Функция добавление обработнных блоков
        private void AddHandledBlock(Block block)
        {
            // Проверка заполнена ли очередь
            bool isFullQueue;
            lock (this.outputLockObject)
            {
                isFullQueue = this.output.Count >= MAX_QUEUE_SIZE;
            }

            // Ожидание в случае заполнености очереди
            if (isFullQueue)
                this.addMutexHandledBlock.WaitOne();

            // Блокировка доступа к очереди обработанных блоков
            lock (this.outputLockObject)
            {
                this.output.Enqueue(block);
            }
        }
    }
}
