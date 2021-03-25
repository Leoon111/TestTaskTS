using System;
using System.IO;
using System.Runtime.InteropServices;
using Test.Models;


namespace Test
{
    class Program
    {
        // todo отрефакторить файл Main
        static void Main(string[] args)
        {
            TableStructure tableStructure;

            // Пути к файлам указаны тестово, прикрутить открытие файла
            string tableHeaderFileFdt = @"..\..\..\Data\Katalog.fdt";
            string fileDateBin = @"..\..\..\Data\KAT003.BIN";

            using (FileStream tableHeaderFileFdtFileStream = new FileStream(tableHeaderFileFdt, FileMode.Open))
            {
                var buffHeaderFilesTypesSize = new byte[Marshal.SizeOf(typeof(ModelStructs.Header_files_types))]; // Создаем буффер необходимого размера
                tableHeaderFileFdtFileStream.Read(buffHeaderFilesTypesSize, 0, buffHeaderFilesTypesSize.Length); // Читаем необходимый объем из файла
                ModelStructs.Header_files_types headerFilesTypes = BuffToStruct<ModelStructs.Header_files_types>(buffHeaderFilesTypesSize); // Преобразуем byte[] в структуру

                // тестовый вывод проверки правильности кодировки символов в файле
#if DEBUG
                Console.WriteLine(headerFilesTypes.name_f_bin);
                Console.WriteLine(headerFilesTypes.name_f_pnt);
#endif
                var buffFieldTypeSize = new byte[Marshal.SizeOf(typeof(ModelStructs.Field_type))];
                ModelStructs.Field_type[] fieldTypes = new ModelStructs.Field_type[headerFilesTypes.count_field_all];
                for (int i = 0; i < headerFilesTypes.count_field_all; i++)
                {
                    tableHeaderFileFdtFileStream.Read(buffFieldTypeSize, 0, buffFieldTypeSize.Length);
                    fieldTypes[i] = BuffToStruct<ModelStructs.Field_type>(buffFieldTypeSize);
                }

                // длина байт дополнительных атрибутов полей
                short moreInfoSize = TwoBytesToShort(tableHeaderFileFdtFileStream);

                var buffExtendetFieldTipeSize = new byte[Marshal.SizeOf(typeof(ModelStructs.Extendet_field_tipe))];
                ModelStructs.Extendet_field_tipe[] еxtendetFieldTipes = new ModelStructs.Extendet_field_tipe[headerFilesTypes.count_field_all];
                for (int i = 0; i < headerFilesTypes.count_field_all; i++)
                {
                    tableHeaderFileFdtFileStream.Read(buffExtendetFieldTipeSize, 0, buffExtendetFieldTipeSize.Length);
                    еxtendetFieldTipes[i] = BuffToStruct<ModelStructs.Extendet_field_tipe>(buffExtendetFieldTipeSize);
                }

                short fieldNameLenght;
                string[] fieldNames = new string[headerFilesTypes.count_field_all];
                for (int i = 0; i < headerFilesTypes.count_field_all; i++)
                {
                    fieldNameLenght = TwoBytesToShort(tableHeaderFileFdtFileStream);
                    fieldNames[i] = StreamOfBytesToString(tableHeaderFileFdtFileStream, fieldNameLenght);
                }
                // вносим полученные данные из файла в модель данных
                tableStructure = new TableStructure(headerFilesTypes, fieldTypes, еxtendetFieldTipes, fieldNames);
            }

            using (FileStream fileDateBinFileStream = new FileStream(fileDateBin, FileMode.Open))
            {
                // Запись таблицы состоит из заголовка и полей.
                // Заголовок начинается с двух байт - это длина записи таблицы.
                short tableRecordLength = TwoBytesToShort(fileDateBinFileStream);
                //string headingField = StreamOfBytesToString(fileDateBinFileStream, tableRecordLength);

                // Если необходимо использовать маску столбцов, то далее идет маска
                if ((0x08 & tableStructure.Flag) == 0x08)
                {
                    // Перед маской находится байт, определяющий ее длину. Каждый бит маски определяет присутствие в данной записи соответствующего ему поля.
                    byte maskLength = ReadOneByte(fileDateBinFileStream);
                    int bitsMask = 0;
                    // 4 байта
                    if (maskLength == 4)
                        bitsMask = FourBytesToInt(fileDateBinFileStream);
                    else
                    {
                        // проверка
                        new ArgumentException(message: "Другая длина маски");
                    }

                    ReadSelectDataFileSream(fileDateBinFileStream, bitsMask, tableStructure);


                }


                // Сразу после заголовка расположены данные полей, способы чтения которых, определяется типом данных, хранящихся в них

            }
        }

        /// <summary>
        /// Чтение полей по маске
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="bitsMask"></param>
        /// <param name="tableStructure"></param>
        public static void ReadSelectDataFileSream(FileStream fileStream, long bitsMask, TableStructure tableStructure) 
        {
            short field = 0;
            // перебираем биты маски, чтоб определить что нам читать в файле
            foreach (FieldNumberEnum fieldNumber in Enum.GetValues(typeof(FieldNumberEnum)))
            {
                // если поле в маске включено
                if ((bitsMask | (long)fieldNumber) == bitsMask)
                {
                    // определяем тип поля, читаем его

                }

                field++;
                if(tableStructure.TableFieldAttributes.Length < field) return;
            }

        }

        private static string StreamOfBytesToString(FileStream fs, int fieldNameLenght)
        {
            byte[] moreInfoSizeBytes = new byte[fieldNameLenght];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            return System.Text.Encoding.Default.GetString(moreInfoSizeBytes);
        }

        //private static byte[] StreamOfBytes(FileStream fileStream, )

        /// <summary>
        /// Преобразует байты в число
        /// </summary>
        /// <param name="fs">поток байтов типа FileStream</param>
        /// <returns>число</returns>
        private static short TwoBytesToShort(FileStream fs)
        {
            byte[] moreInfoSizeBytes = new byte[2];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            return BitConverter.ToInt16(moreInfoSizeBytes, 0);
        }

        private static int FourBytesToInt(FileStream fs)
        {
            byte[] moreInfoSizeBytes = new byte[4];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            return BitConverter.ToInt32(moreInfoSizeBytes, 0);
        }

        private static byte ReadOneByte(FileStream fileStream)
        {
            byte[] moreInfoSizeBytes = new byte[1];
            fileStream.Read(moreInfoSizeBytes, 0, 1);
            byte i = moreInfoSizeBytes[0];
            return i; //BitConverter.to ToInt16(moreInfoSizeBytes);
        }

        public static T BuffToStruct<T>(byte[] arr) where T : struct
        {
            GCHandle gch = GCHandle.Alloc(arr, GCHandleType.Pinned); // зафиксировать в памяти
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0); // и взять его адрес
            T ret = (T)Marshal.PtrToStructure(ptr, typeof(T)); // создать структуру
            gch.Free(); // снять фиксацию
            return ret;
        }
    }



    enum FieldNumberEnum : long
    {
        //None = 0x0,
        //бит по счету с начала    40   36   32   28   24   20   16   12   8    4
        Field1 = 0x01,          // 0000 0000 0000 0000 0000 0000 0000 0000 0000 0001
        Field2 = 0x02,          // 0000 0000 0000 0000 0000 0000 0000 0000 0000 0010
        Field3 = 0x04,          // 0000 0000 0000 0000 0000 0000 0000 0000 0000 0100
        Field4 = 0x08,          // 0000 0000 0000 0000 0000 0000 0000 0000 0000 1000
        Field5 = 0x10,          // 0000 0000 0000 0000 0000 0000 0000 0000 0001 0000
        Field6 = 0x20,          // 0000 0000 0000 0000 0000 0000 0000 0000 0010 0000
        Field7 = 0x40,          // 0000 0000 0000 0000 0000 0000 0000 0000 0100 0000
        Field8 = 0x80,          // 0000 0000 0000 0000 0000 0000 0000 0000 1000 0000
        Field9 = 0x100,         // 0000 0000 0000 0000 0000 0000 0000 0001 0000 0000
        Field10 = 0x200,        // 0000 0000 0000 0000 0000 0000 0000 0010 0000 0000
        Field11 = 0x400,        // 0000 0000 0000 0000 0000 0000 0000 0100 0000 0000
        Field12 = 0x800,        // 0000 0000 0000 0000 0000 0000 0000 1000 0000 0000
        Field13 = 0x1000,       // 0000 0000 0000 0000 0000 0000 0001 0000 0000 0000
        Field14 = 0x2000,       // 0000 0000 0000 0000 0000 0000 0010 0000 0000 0000
        Field15 = 0x4000,       // 0000 0000 0000 0000 0000 0000 0100 0000 0000 0000
        Field16 = 0x8000,       // 0000 0000 0000 0000 0000 0000 1000 0000 0000 0000
        Field17 = 0x10000,      // 0000 0000 0000 0000 0000 0001 0000 0000 0000 0000
        Field18 = 0x20000,      // 0000 0000 0000 0000 0000 0010 0000 0000 0000 0000
        Field19 = 0x40000,      // 0000 0000 0000 0000 0000 0100 0000 0000 0000 0000
        Field20 = 0x80000,      // 0000 0000 0000 0000 0000 1000 0000 0000 0000 0000
        Field21 = 0x100000,     // 0000 0000 0000 0000 0001 0000 0000 0000 0000 0000
        Field22 = 0x200000,     // 0000 0000 0000 0000 0010 0000 0000 0000 0000 0000
        Field23 = 0x400000,     // 0000 0000 0000 0000 0100 0000 0000 0000 0000 0000
        Field24 = 0x800000,     // 0000 0000 0000 0000 1000 0000 0000 0000 0000 0000
        Field25 = 0x1000000,    // 0000 0000 0000 0001 0000 0000 0000 0000 0000 0000
        Field26 = 0x2000000,    // 0000 0000 0000 0010 0000 0000 0000 0000 0000 0000
        Field27 = 0x4000000,    // 0000 0000 0000 0100 0000 0000 0000 0000 0000 0000
        Field28 = 0x8000000,    // 0000 0000 0000 1000 0000 0000 0000 0000 0000 0000
        Field29 = 0x10000000,   // 0000 0000 0001 0000 0000 0000 0000 0000 0000 0000
        Field30 = 0x20000000,   // 0000 0000 0010 0000 0000 0000 0000 0000 0000 0000
        Field31 = 0x40000000,   // 0000 0000 0100 0000 0000 0000 0000 0000 0000 0000
        Field32 = 0x80000000,   // 0000 0000 1000 0000 0000 0000 0000 0000 0000 0000
        Field33 = 0x100000000,  // 0000 0001 0000 0000 0000 0000 0000 0000 0000 0000
        Field34 = 0x200000000,  // 0000 0010 0000 0000 0000 0000 0000 0000 0000 0000
        Field35 = 0x400000000,  // 0000 0100 0000 0000 0000 0000 0000 0000 0000 0000
        Field36 = 0x800000000,  // 0000 1000 0000 0000 0000 0000 0000 0000 0000 0000
        Field37 = 0x1000000000, // 0001 0000 0000 0000 0000 0000 0000 0000 0000 0000
        Field38 = 0x2000000000, // 0010 0000 0000 0000 0000 0000 0000 0000 0000 0000
        Field39 = 0x4000000000, // 0100 0000 0000 0000 0000 0000 0000 0000 0000 0000
        Field40 = 0x8000000000  // 1000 0000 0000 0000 0000 0000 0000 0000 0000 0000

    }
}
