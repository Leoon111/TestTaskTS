using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Models
{
    /// <summary>Данные подготовленные для таблицы</summary>
    public class EtkaDataReady
    {
        private readonly string[] _fieldName;
        private List<string[]> _columDataTamble;
        private readonly string[] _tableColumnCode;
        private readonly List<string> _dataType;
        private readonly int[] _columNumberFromData;

        /// <summary>
        /// Конструктор формирует описание таблицы
        /// </summary>
        /// <param name="tableStructure">Данные из файла заголовка таблицы .fdt</param>
        public EtkaDataReady(TableStructure tableStructure)
        {
            // заполняем имена полей
            _fieldName = new string[tableStructure.FieldNames.Length];
            for (int i = 0; i < tableStructure.FieldNames.Length; i++)
                _fieldName[i] = tableStructure.FieldNames[i];

            _tableColumnCode = new string[tableStructure.TableFieldAttributes.Length];
            for (var i = 0; i < tableStructure.TableFieldAttributes.Length; i++)
                _tableColumnCode[i] = tableStructure.TableFieldAttributes[i].TableColumnCode;

            // todo вставить преобразование, когда будет отдельный класс в парсинге
            //for (var i = 0; i < tableStructure.TableFieldAttributes.Length; i++)
            //    _dataType[i] = tableStructure.TableFieldAttributes[i].DataType;

            _columNumberFromData = new int[tableStructure.AdditionalTableFieldAttributes.Length];
            for (var i = 0; i < tableStructure.AdditionalTableFieldAttributes.Length; i++)
                _columNumberFromData[i] = tableStructure.AdditionalTableFieldAttributes[i].ColumnNumber;

            _columDataTamble = new List<string[]>();
        }


        // todo заменить все на массивы, коллекции здесь для тестирования
        /// <summary>Имена полей в таблице</summary>
        public string[] FieldName => _fieldName;

        /// <summary>Данные таблицы в двумерной коллекции(коллекция временно)</summary>
        public List<string[]> ColumDataTamble
        {
            get => _columDataTamble;
            set => _columDataTamble = value;
        }

        /// <summary>(вспомогательное) Кодовое обозначение столбца таблицы</summary>
        public string[] TableColumnCode => _tableColumnCode;

        /// <summary>(вспомогательное, не реализовано) Тип данных столбца</summary>
        public List<string> DataType => _dataType;

        /// <summary>(вспомогательное) Порядковый номер столбца из данных описания таблицы</summary>
        public int[] ColumNumberFromData => _columNumberFromData;
    }
}
