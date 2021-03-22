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
                if (tableStructure.Flag == 0x08)
                {
#if DEBUG
                    Console.WriteLine("Флаг установлен");
#endif
                    // Перед маской находится байт, определяющий ее длину. Каждый бит маски определяет присутствие в данной записи соответствующего ему поля.
                    byte maskLength = OneByteToShort(fileDateBinFileStream);
                    bool[] maskBools = new bool[maskLength * 8];



                }


                // Сразу после заголовка расположены данные полей, способы чтения которых, определяется типом данных, хранящихся в них

            }
        }

        private static string StreamOfBytesToString(FileStream fs, short fieldNameLenght)
        {
            byte[] moreInfoSizeBytes = new byte[fieldNameLenght];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            return System.Text.Encoding.Default.GetString(moreInfoSizeBytes);
        }

        //private static byte[] StreamOfBytes(FileStream fileStream, )

        /// <summary>
        /// Преобразует два байта в число типа short
        /// </summary>
        /// <param name="fs">поток байтов типа FileStream</param>
        /// <returns>число типа short</returns>
        private static short TwoBytesToShort(FileStream fs)
        {
            byte[] moreInfoSizeBytes = new byte[2];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            return BitConverter.ToInt16(moreInfoSizeBytes, 0);
        }

        private static byte OneByteToShort(FileStream fileStream)
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
}
