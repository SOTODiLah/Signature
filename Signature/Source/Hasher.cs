using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Signature.Source
{
    // Класс для хэширования блока хэшем MD5 
    class Hasher : BaseHasher
    {
        // Конструктор класса для хэширования блока хэшем MD5. Передаём конструктору базового класса наследника хэш-алгоритма
        public Hasher() : base(new MD5CryptoServiceProvider())
        {
        }
        
        // Переопределение функции хэширования блока
        public override void GetHashBlock(Block block)
        {
            block.Hash = this.GetHash(block.Data);
            block.HashSize = HashSize;
        }

        // Переопределение функции получения размера хэша
        protected override int HashSize { get { return 16; } }

    }
}
