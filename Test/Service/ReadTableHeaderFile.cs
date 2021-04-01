using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Test.Models;

namespace Test.Service
{
    /// <summary>
    /// Класс чтения заголовочного файла таблицы
    /// </summary>
    public static class ReadTableHeaderFile
    {
        /// <summary>
        /// Чтение заголовочного файла
        /// </summary>
        /// <param name="tableHeaderFileFdt">Адрес файла заголовка таблиц</param>
        /// <returns>Структура таблицы данных</returns>
        public static TableStructure Read(string tableHeaderFileFdt)
        {
            #region Чтение файла заголовка таблицы .fdt

            using (FileStream tableHeaderFileFdtFileStream = new FileStream(tableHeaderFileFdt, FileMode.Open))
            {
                var buffHeaderFilesTypesSize = new byte[Marshal.SizeOf(typeof(ModelStructs.Header_files_types))]; // Создаем буфер необходимого размера
                tableHeaderFileFdtFileStream.Read(buffHeaderFilesTypesSize, 0, buffHeaderFilesTypesSize.Length); // Читаем необходимый объем из файла
                var headerFilesTypes = ReadBytes.BuffToStruct<ModelStructs.Header_files_types>(buffHeaderFilesTypesSize); // Преобразуем byte[] в структуру

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
                    fieldTypes[i] = ReadBytes.BuffToStruct<ModelStructs.Field_type>(buffFieldTypeSize);
                }

                // длина байт дополнительных атрибутов полей
                short moreInfoSize = ReadBytes.TwoBytesToShort(tableHeaderFileFdtFileStream);

                var buffExtendetFieldTipeSize = new byte[Marshal.SizeOf(typeof(ModelStructs.Extendet_field_tipe))];
                ModelStructs.Extendet_field_tipe[] еxtendetFieldTipes = new ModelStructs.Extendet_field_tipe[headerFilesTypes.count_field_all];
                for (int i = 0; i < headerFilesTypes.count_field_all; i++)
                {
                    tableHeaderFileFdtFileStream.Read(buffExtendetFieldTipeSize, 0, buffExtendetFieldTipeSize.Length);
                    еxtendetFieldTipes[i] = ReadBytes.BuffToStruct<ModelStructs.Extendet_field_tipe>(buffExtendetFieldTipeSize);
                }

                short fieldNameLenght;
                string[] fieldNames = new string[headerFilesTypes.count_field_all];
                for (int i = 0; i < headerFilesTypes.count_field_all; i++)
                {
                    fieldNameLenght = ReadBytes.TwoBytesToShort(tableHeaderFileFdtFileStream);
                    fieldNames[i] = ReadBytes.StreamOfBytesToString(tableHeaderFileFdtFileStream, fieldNameLenght);
                }
                // вносим полученные данные из файла в модель данных
                var tableStructure = new TableStructure(headerFilesTypes, fieldTypes, еxtendetFieldTipes, fieldNames);

                return tableStructure;
            }

            #endregion
        }
    }
}
