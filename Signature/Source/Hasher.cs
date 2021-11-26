using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Signature.Source
{
    // Класс для хэширования блока
    class Hasher
    {
        // Объект для создания хэша MD5
        private readonly MD5 md5;

        // Коструктор класс хэшера
        public Hasher()
        {
            this.md5 = new MD5CryptoServiceProvider();
        }

        // Функция хэширования блока
        public void Action(Block block)
        {
            block.Hash = this.GetHash(block.Data);
        }

        // Функция получения массива байтов хэша из массива байтов данных
        public byte[] GetHash(byte[] input)
        {
            return this.md5.ComputeHash(input);
        }

        // Функция получения размера хэша
        public static int GetHashSize()
        {
            return 16;
        }
    }
}
