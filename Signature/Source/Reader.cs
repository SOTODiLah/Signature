using System;
using System.IO;

namespace Signature.Source
{
    // Класс читателя 
    class Reader
    {
        // Поток ввода из файла
        private readonly Stream stream;

        // Размер блока
        private readonly int blockSize;

        // Количество считанных блоков
        private uint counter;

        // Коструктор класса читателя
        public Reader(Stream stream, int blockSize)
        {
            this.stream = stream;
            this.blockSize = blockSize;
        }

        // Получение байтов следующего блока в файле
        public byte[] GetNextBlockBytes()
        {
            byte[] buffer = new byte[this.blockSize];
            int bytesReaded = this.stream.Read(buffer, 0, this.blockSize);

            if (bytesReaded == 0)
                return null;

            // Равен ли размер считанных байтов размеру неоходимого блока?
            if (bytesReaded == this.blockSize)
                return buffer;

            // Возвращение из функции обрезанного блока
            byte[] result = new byte[bytesReaded];
            Array.Copy(buffer, result, result.Length);

            return result;
        }

        // Функция получения блока
        public Block GetNextBlock()
        {
            var buffer = this.GetNextBlockBytes();
            Block block = new Block
            {
                Pos = this.counter++,
                BlockSize = buffer.Length,
                Data = buffer
            };
            return block;
        }
    }
}
