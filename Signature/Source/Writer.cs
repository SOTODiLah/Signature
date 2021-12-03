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
        protected readonly Stream stream;

        // Конструктор класса писателя
        public Writer(Stream stream)
        {
            this.stream = stream;
            this.stream.Flush();
        }

        // Функция записи блока в файл
        public void WriteBlock(Block block)
        {
            WriteBlock(block.Hash, block.Pos, block.HashSize);
        }

        // Фукция записи байтов в файл
        public void WriteBlock(byte[] hash, long pos, int hashSize)
        {
            this.stream.Position = (long)hashSize * pos;
            this.stream.Write(hash, 0, hash.Length);
        }
    }
}
