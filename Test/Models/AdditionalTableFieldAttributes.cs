using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Models
{
    /// <summary>
    /// Дополнительные атрибуты одного поля таблицы
    /// </summary>
    public class AdditionalTableFieldAttributes
    {
        public AdditionalTableFieldAttributes(
            ModelStructs.Extendet_field_tipe extendetFieldTipe)
        {
            BelongingToSubtable = extendetFieldTipe.type_gruppen;
            ColumnOrdinal = extendetFieldTipe.npp;
        }
        /// <summary>
        /// Код, определяющий принадлежность поля к субтаблице.
        /// </summary>
        public ushort BelongingToSubtable { get; }
        /// <summary>
        /// Порядковый номер столбца
        /// </summary>
        public ushort ColumnOrdinal { get; }
    }
}
