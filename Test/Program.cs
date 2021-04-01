using System;
using System.IO;
using System.Runtime.InteropServices;
using Test.Models;
using System.Linq;
using MySql.Data.MySqlClient;
using Test.DataBase;
using Test.Service;

namespace Test
{
    class Program
    {
        private static void Main(string[] args)
        {
            TableStructure tableStructure;
            EtkaDataReady etkaDataReady;

            // Пути к файлам указаны тестово, прикрутить открытие файла
            var tableHeaderFileFdt = @"..\..\..\Data\Katalog.fdt";
            var fileDateBin = @"..\..\..\Data\KAT005.BIN";

            // читаем заголовочный файл, получаем структуру таблицы
            tableStructure = ReadTableHeaderFile.Read(tableHeaderFileFdt);
            // подготавливаем данные для таблицы (возможно позднее объединю с tableStructure, пока так)
            etkaDataReady = new EtkaDataReady(tableStructure);

            // создаем метод в котором читается бинарник и асинхронно сохраняется в базу

            // параллельно запускаем этот метод сколько раз, сколько бинарников нашли



            using (var mySqlConnection = new MySqlCon().GetConntction())
            {
                var queryIsATable = @"select count(*) 
                            from information_schema.tables
                            where table_type = 'BASE TABLE' and 
                              table_name = 'test'";

                var commandToMySql = new MySqlCommand(queryIsATable, mySqlConnection);

                if (Convert.ToInt32(commandToMySql.ExecuteScalar()) > 0)
                {

                    commandToMySql = new MySqlCommand("DROP TABLE test", mySqlConnection);
                    var a = commandToMySql.ExecuteNonQuery();
                }
#if DEBUG
                Console.WriteLine("Соединение успешно");
#endif
                // создаем столбцы с типами данных из данных из описания таблицы
                // создаем строку sql запроса из данный описания таблицы
                string sqlCreateColumns = "CREATE TABLE test \n ( \n";
                sqlCreateColumns += " Id int";
                for (var i = 0; i < etkaDataReady.TableColumnCode.Length; i++)
                {
                    //if (i > 0)
                    sqlCreateColumns += ",\n "; // вставляем запятую у предыдущей команды и переход на новую строку, если есть дальше данные

                    sqlCreateColumns += /*"ADD " +*/ etkaDataReady.TableColumnCode[i].Substring(0, 2) + " VARCHAR(200) NULL";
                }


                sqlCreateColumns += " \n) ";
                // Вносим данные в таблицу
                commandToMySql = new MySqlCommand(sqlCreateColumns, mySqlConnection);
                commandToMySql.ExecuteNonQuery();
                var answer = commandToMySql.UpdatedRowSource;
                var an = commandToMySql.IsPrepared;
                var ans = commandToMySql.EnableCaching;

                mySqlConnection.Close();
            }
        }

        

        
    }

}
