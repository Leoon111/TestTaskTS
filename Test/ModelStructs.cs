using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Test
{
    public class ModelStructs
    {
        /// <summary>
        /// структура таблицы
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public unsafe struct Header_files_types
        {
            // размер структуры;
            public ushort size;
            // количество используемых столбцов в таблице
            public ushort count_field_r;
            // количество всех столбцов в таблице
            public ushort count_field_all;
            // неопределённые данные
            public ushort s6;
            //char name_f_bin[13]; // имя файла самой таблицы
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public char[] name_f_bin;
            //char name_f_pnt[13]; // имя файла индекса
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public char[] name_f_pnt;
            //char S22[4]; // неопределённые данные
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char[] S22;
            //unsigned char flag; // если (0x08 & flag) == 0x08, нужно использовать маску столбцов таблицы. Иначе используются все столбцы
            public char flag;
            //char S27[17];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public char[] S27;
        }

        /// <summary>
        /// Описание атрибутов столбца таблицы
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public unsafe struct Field_type
        {
            // кодовое обозначение столбца таблицы
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public char[] cod;
            // длина данных
            public ushort length;
            // неопределенные данные
            public ushort s5;
            // неопределенные данные
            public ushort s7;
            // неопределенные данные
            public ushort s9;
            // тип данных
            public ushort type;
        }

        /// <summary>
        /// дополнительные атрибуты столбцов таблицы
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public unsafe struct Extendet_field_tipe
        {
            // неопределенные данные
            public ushort s0;
            // неопределенные данные
            public ushort s2;
            // код, определяющий принадлежность его к субтаблице. Если значение данного поля равно 0x0B или 0x05,
            // то данное поле представляет собой субтаблицу.
            public ushort type_gruppen;
            // порядковый номыер
            public ushort npp;
        }
    }
}
