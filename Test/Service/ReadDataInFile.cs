using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Test.Models;

namespace Test.Service
{
    public static class ReadDataInFile
    {
        internal static void ReadBinFile(string fileDateBin, TableStructure tableStructure, ref EtkaDataReady etkaDataReady)
        {
            using (FileStream fileDateBinFileStream = new FileStream(fileDateBin, FileMode.Open))
            {
                for (var i = 0; i < tableStructure.StructureSize; i++)
                {
                    // Запись таблицы состоит из заголовка и полей.
                    // Заголовок начинается с двух байт - это длина записи таблицы.
                    short tableRecordLength = ReadBytes.TwoBytesToShort(fileDateBinFileStream);
                    //string headingField = StreamOfBytesToString(fileDateBinFileStream, tableRecordLength);

                    // Если необходимо использовать маску столбцов, то далее идет маска
                    if ((0x08 & tableStructure.Flag) == 0x08)
                    {
                        // Перед маской находится байт, определяющий ее длину. Каждый бит маски определяет присутствие в данной записи соответствующего ему поля.
                        byte maskLength = ReadBytes.ReadOneByte(fileDateBinFileStream);
                        tableRecordLength -= 1;
                        int bitsMask = 0;
                        // 4 байта
                        if (maskLength == 4)
                        {
                            bitsMask = ReadBytes.FourBytesToInt(fileDateBinFileStream);
                            tableRecordLength -= 4;
                        }

                        if (maskLength == 2)
                        {
                            bitsMask = ReadBytes.TwoBytesToShort(fileDateBinFileStream);
                            tableRecordLength -= 2;
                        }
                        if (maskLength != 2 && maskLength != 4)
                        {
                            // проверка
                            throw new ArgumentException(message: "Другая длина маски");
                        }

                        etkaDataReady.ColumDataTamble.Add(new string[tableStructure.TableFieldAttributes.Length]);
                        etkaDataReady.ColumDataTamble[i] = ReadDataInFile.ReadSelecToMasktDataFileSream(fileDateBinFileStream, bitsMask, tableStructure, tableRecordLength);
                        //var a = StreamOfBytesToString(fileDateBinFileStream, tableRecordLength);

                    }
                    else throw new ArgumentException("Флаг использования маски отсутствует");
                }


                // Сразу после заголовка расположены данные полей, способы чтения которых, определяется типом данных, хранящихся в них

            }
        }


        // todo нужно решить с переменными внутри метода, сейчас они тестово

        /// <summary>
        /// Чтение полей по маске
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="bitsMask"></param>
        /// <param name="tableStructure"></param>
        internal static string[] ReadSelecToMasktDataFileSream(FileStream fileStream, long bitsMask, TableStructure tableStructure, int tableRecordLength)
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
                            f = ReadBytes.TwoBytesToShort(fileStream);
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
                        var f = ReadBytes.StreamOfBytesToString(fileStream, b);

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
                        var f = ReadBytes.StreamOfBytesToString(fileStream, b);

                        fieldEtkaDataReady[field] = f;
                        remainingBytes -= b;

                        continue;
                    }
                    else throw new ArgumentException("Неизвестный тип данных в поле");
                }

            }
            // Читаем биты, которые остались как строку, возможно понадобится
            string aa;
            if (remainingBytes > 0)
                aa = ReadBytes.StreamOfBytesToString(fileStream, remainingBytes);

            return fieldEtkaDataReady;
        }
       
    }
}
