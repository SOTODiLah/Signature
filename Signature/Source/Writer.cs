using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Signature.Source
{
    // Класс писателя
    class Writer
    {
        // Поток вывода в файл
        private readonly Stream stream;

        // Размер блока
        private readonly int blockSize;

        // Конструктор класса писателя
        public Writer(Stream stream, int blockSize)
        {
            this.stream = stream;
            this.blockSize = blockSize;
            this.stream.Flush();
        }

        // Функция записи блока в файл
        public void WriteBlock(Block block)
        {
            WriteBlock(block.Hash, block.Pos);
        }

        // Фукция записи байтов в файл
        public void WriteBlock(byte[] hash, long pos)
        {
            this.stream.Position = (long)this.blockSize * pos;
            this.stream.Write(hash, 0, hash.Length);
        }
    }
}
