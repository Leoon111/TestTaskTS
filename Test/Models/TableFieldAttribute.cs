using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Models
{
    /// <summary>
    /// Описание атрибутов одного столбца таблицы
    /// </summary>
    public class TableFieldAttribute
    {
        public TableFieldAttribute(ModelStructs.Field_type fieldType)
        {
            TableColumnCode = new string(fieldType.cod);
            DataLength = fieldType.length;
            DataType = fieldType.type;
        }
        /// <summary>
        /// Кодовое обозначение столбца таблицы
        /// </summary>
        public string TableColumnCode { get; }
        /// <summary>
        /// Длина данных
        /// </summary>
        public ushort DataLength { get; }
        /// <summary>
        /// Тип данных
        /// </summary>
        public ushort DataType { get; }

    }
}
