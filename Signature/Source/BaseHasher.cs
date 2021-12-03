using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace Signature.Source
{
    // Базовый класс хэшера
    abstract class BaseHasher
    {
        // Объект хэш-алгоритма
        protected readonly HashAlgorithm hashAlgorithm;

        // Абстрактное свойство размер хэша
        protected abstract int HashSize { get; }

        // Конструктор базого класса с инициализацией хэш-алгоритма
        protected BaseHasher(HashAlgorithm hashAlgorithm)
        {
            this.hashAlgorithm = hashAlgorithm;
        }

        // Абстрактный метод получения блока
        public abstract void GetHashBlock(Block block);

        // Метод получения массива байтов хэша из массива байтов данных
        protected byte[] GetHash(byte[] input)
        {
            if (hashAlgorithm == null)
                return null;
            return this.hashAlgorithm.ComputeHash(input);
        }
    }
}
