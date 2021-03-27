using System;
using System.IO;
using System.Runtime.InteropServices;
using Test.Models;
using System.Linq;

namespace Test
{
    class Program
    {
        // todo отрефакторить файл Main
        private static void Main(string[] args)
        {
            TableStructure tableStructure;
            EtkaDataReady etkaDataReady;
            // Пути к файлам указаны тестово, прикрутить открытие файла
            var tableHeaderFileFdt = @"..\..\..\Data\Katalog.fdt";
            var fileDateBin = @"..\..\..\Data\KAT005.BIN";

            #region Чтение файла заголовка таблицы .fdt

            using (FileStream tableHeaderFileFdtFileStream = new FileStream(tableHeaderFileFdt, FileMode.Open))
            {
                var buffHeaderFilesTypesSize = new byte[Marshal.SizeOf(typeof(ModelStructs.Header_files_types))]; // Создаем буффер необходимого размера
                tableHeaderFileFdtFileStream.Read(buffHeaderFilesTypesSize, 0, buffHeaderFilesTypesSize.Length); // Читаем необходимый объем из файла
                var headerFilesTypes = BuffToStruct<ModelStructs.Header_files_types>(buffHeaderFilesTypesSize); // Преобразуем byte[] в структуру

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

                etkaDataReady = new EtkaDataReady(tableStructure);
            }

            #endregion

            using (FileStream fileDateBinFileStream = new FileStream(fileDateBin, FileMode.Open))
            {

                for (var i = 0; i < tableStructure.StructureSize; i++)
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
                        tableRecordLength -= 1;
                        int bitsMask = 0;
                        // 4 байта
                        if (maskLength == 4)
                        {
                            bitsMask = FourBytesToInt(fileDateBinFileStream);
                            tableRecordLength -= 4;
                        }

                        if (maskLength == 2)
                        {
                            bitsMask = TwoBytesToShort(fileDateBinFileStream);
                            tableRecordLength -= 2;
                        }
                        if (maskLength != 2 && maskLength != 4)
                        {
                            // проверка
                            throw new ArgumentException(message: "Другая длина маски");
                        }
                        
                        etkaDataReady.ColumDataTamble.Add(new string[tableStructure.TableFieldAttributes.Length]);
                        etkaDataReady.ColumDataTamble[i] = ReadSelecToMasktDataFileSream(fileDateBinFileStream, bitsMask, tableStructure, tableRecordLength);
                        //var a = StreamOfBytesToString(fileDateBinFileStream, tableRecordLength);

                    }
                    else throw new ArgumentException("Флаг использования маски отсутствует");
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
        private static string[] ReadSelecToMasktDataFileSream(FileStream fileStream, long bitsMask, TableStructure tableStructure, int tableRecordLength)
        {
            // строка полей для возвращаемого значения
            var fieldEtkaDataReady = new string[tableStructure.TableFieldAttributes.Length];
            var remainingBytes = tableRecordLength;
            //short field = 0;
            var s = Convert.ToString(bitsMask, 2);
            var maskArray = s.Select(x => byte.Parse(x.ToString())).ToArray();

            // перебираем биты маски, чтоб определить что нам читать в файле
            //foreach (FieldNumberEnum fieldNumber in Enum.GetValues(typeof(FieldNumberEnum)))
            for (int field = 0; field < maskArray.Length; field++)
            {
                if (tableStructure.TableFieldAttributes.Length < field) break;
                // если поле в маске включено
                if (maskArray[field] == 1)
                {
                    // определяем тип поля, читаем его
                    // число
                    if (tableStructure.TableFieldAttributes[field].DataType == 0x14
                        || tableStructure.TableFieldAttributes[field].DataType == 0x16
                        || tableStructure.TableFieldAttributes[field].DataType == 0x18)
                    {

                        var a = tableStructure.TableFieldAttributes[field].TableColumnCode;
                        var b = tableStructure.TableFieldAttributes[field].DataLength;
                        var d = tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable;
                        if (tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable == 0x0B
                            || tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable == 0x05)
                        {
                            throw new ArgumentException("Субтаблица здесь");
                        }
                        var e = tableStructure.AdditionalTableFieldAttributes[field].ColumnNumber;

                        var f = 0;
                        if (b == 2)
                            f = TwoBytesToShort(fileStream);
                        fieldEtkaDataReady[field] = f.ToString();
                        remainingBytes -= b;

                        continue;
                    }
                    // строка
                    if (tableStructure.TableFieldAttributes[field].DataType == 0x22
                        || tableStructure.TableFieldAttributes[field].DataType == 0x2C
                        || tableStructure.TableFieldAttributes[field].DataType == 0x12)
                    {
                        var a = tableStructure.TableFieldAttributes[field].TableColumnCode;
                        var b = tableStructure.TableFieldAttributes[field].DataLength;
                        var d = tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable;
                        if (tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable == 0x0B
                            || tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable == 0x05)
                        {
                            throw new ArgumentException("Субтаблица здесь");
                        }
                        var e = tableStructure.AdditionalTableFieldAttributes[field].ColumnNumber;
                        // длина данных в таблице важнее, чем длина данных в описании таблицы.
                        if (b > remainingBytes) b = (ushort)remainingBytes;
                        var f = StreamOfBytesToString(fileStream, b);

                        fieldEtkaDataReady[field] = f;
                        remainingBytes -= b;

                        continue;
                    }
                    // За данным полем перечисляются поля, которые используются, как субтаблица
                    if (tableStructure.TableFieldAttributes[field].DataType == 0x0A
                        || tableStructure.TableFieldAttributes[field].DataType == 0x1A)
                    {


                        continue;
                    }
                    // Содержит множество чисел, разделенных квадратными скобками
                    if (tableStructure.TableFieldAttributes[field].DataType == 0x26)
                    {
                        var a = tableStructure.TableFieldAttributes[field].TableColumnCode;
                        var b = tableStructure.TableFieldAttributes[field].DataLength;
                        var d = tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable;
                        if (tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable == 0x0B
                            || tableStructure.AdditionalTableFieldAttributes[field].BelongingToSubtable == 0x05)
                        {
                            throw new ArgumentException("Субтаблица здесь");
                        }
                        var e = tableStructure.AdditionalTableFieldAttributes[field].ColumnNumber;
                        // длина данных в таблице важнее, чем длина данных в описании таблицы.
                        if (b > remainingBytes) b = (ushort)remainingBytes;
                        var f = StreamOfBytesToString(fileStream, b);

                        fieldEtkaDataReady[field] = f;
                        remainingBytes -= b;

                        continue;
                    }
                    else throw new ArgumentException("Неизвестный тип данных в поле");
                }

            }

            string aa;
            if (remainingBytes > 0)
                aa = StreamOfBytesToString(fileStream, remainingBytes);

            return fieldEtkaDataReady;
        }

        private static string StreamOfBytesToString(FileStream fs, int fieldNameLenght)
        {
            byte[] moreInfoSizeBytes = new byte[fieldNameLenght];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            var m = Convert.ToBase64String(moreInfoSizeBytes);
            var st = System.Text.Encoding.ASCII.GetString(moreInfoSizeBytes);
            return st;
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
