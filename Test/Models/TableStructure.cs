using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Models
{
    /// <summary>
    /// Модель структуры таблицы (слово СТРУКТУРА/Structure здесь имеет смысл, как расположение полей, а не как конструкция языка программирования)
    /// </summary>
    public class TableStructure
    {
        /// <summary>
        /// Запись данных из структуры
        /// </summary>
        /// <param name="dateHeaderFilesTypes">Данные заголовка структуры</param>
        /// <param name="fieldTypes">Описание атрибутов столбца таблицы</param>
        /// <param name="extendetFieldTipes">Дополнительные атрибуты столбцов таблицы</param>
        /// <param name="fNames">Имена полей</param>
        public TableStructure(ModelStructs.Header_files_types dateHeaderFilesTypes,
                              ModelStructs.Field_type[] fieldTypes,
                              ModelStructs.Extendet_field_tipe[] extendetFieldTipes,
                              string[] fNames)
        {
            // инициализация данных заголовка структуры
            StructureSize = dateHeaderFilesTypes.size;
            NumberUsedColumnsInTable = dateHeaderFilesTypes.count_field_r;
            NumberAllColumnsInTable = dateHeaderFilesTypes.count_field_all;
            // убираем ненужные символы в конце строки
            FileNameTableItself = new string(dateHeaderFilesTypes.name_f_bin).Remove(dateHeaderFilesTypes.name_f_bin.Length - 5);
            IndexFileName = new string(dateHeaderFilesTypes.name_f_pnt).Remove(dateHeaderFilesTypes.name_f_pnt.Length - 5);
            Flag = dateHeaderFilesTypes.flag;

            SetFieldAttributes(fieldTypes);
            SetAdditionalTableFieldAttributes(extendetFieldTipes);
            SetFieldNames(fNames);
        }

        /// <summary>
        /// Размер структуры
        /// </summary>
        public ushort StructureSize { get; }
        /// <summary>
        /// Количество используемых столбцов в таблице
        /// </summary>
        public ushort NumberUsedColumnsInTable { get; }
        /// <summary>
        /// Количество всех столбцов в таблице
        /// </summary>
        public ushort NumberAllColumnsInTable { get; }
        /// <summary>
        /// Имя файла самой таблицы
        /// </summary>
        public string FileNameTableItself { get; }
        /// <summary>
        /// Имя файла индекса
        /// </summary>
        public string IndexFileName { get; }
        /// <summary>
        /// Флаг, использовать ли маску таблицы
        /// </summary>
        public char Flag { get; }

        public TableFieldAttribute[] TableFieldAttributes;

        public AdditionalTableFieldAttributes[] AdditionalTableFieldAttributes;

        public string[] FieldNames { get; set; }

        private void SetFieldNames(string[] fNames)
        {
            FieldNames = new string[NumberAllColumnsInTable];
            //Array.Copy(fNames, FieldNames, NumberAllColumnsInTable);
            for (int c = 0; c < fNames.Length; c++)
            {
                FieldNames[c] = fNames[c].TrimEnd('\0');
            }
        }

        private void SetAdditionalTableFieldAttributes(ModelStructs.Extendet_field_tipe[] extendetFieldTipes)
        {
            AdditionalTableFieldAttributes = new AdditionalTableFieldAttributes[NumberAllColumnsInTable];
            for (int b = 0; b < extendetFieldTipes.Length; b++)
            {
                AdditionalTableFieldAttributes[b] = new AdditionalTableFieldAttributes(extendetFieldTipes[b]);
            }
        }

        private void SetFieldAttributes(ModelStructs.Field_type[] fieldTypes)
        {
            TableFieldAttributes = new TableFieldAttribute[NumberAllColumnsInTable];
            for (int a = 0; a < fieldTypes.Length; a++)
            {
                TableFieldAttributes[a] = new TableFieldAttribute(fieldTypes[a]);
            }
        }
    }
}
