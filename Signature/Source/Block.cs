namespace Signature
{
    // Класс блока файла
    public class Block
    {
        // Позиция блока в файле
        public long Pos { get; set; }
        // Размер блока в байтах
        public int BlockSize { get; set; }
        // Массив байтов данных из файла
        public byte[] Data { get; set; }
        // Массив байтов хэша данных из файла
        public byte[] Hash { get; set; }
    }
}
