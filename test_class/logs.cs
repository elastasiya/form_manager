using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_class
{
    public static class logs
    {
        static StringBuilder log_buffer = new StringBuilder();      //для вывода в лог
        static bool first_time = true;
        static string current_data = "";
        static string time_format = "HH:mm:ss ";
        static string date_format = "yyyy.MM.dd";
        static string date_time_format = "yyyy.MM.dd-HH:mm:ss ";
        static string date_time_file_format = "yyyy.MM.dd-HH.mm.ss";
        static string first_mes = 
            "-@@- Лог запущен " + DateTime.Now.ToString(date_time_format) + "-@@-";
        static string another_day_mes = 
            "-@@- Лог продолжен " + DateTime.Now.ToString(date_time_format) + "-@@-";
        static string error = "ОШИБКА";
        static string information = "Информация";
        static string dir_error = "Не удалось создать папку с логами";
        static string file_name = ""; 


        public static void mes_manage (string message, bool is_error = false)
        {
            string current_mes = 
                DateTime.Now.ToString(time_format) + (is_error ? (error + ": ") : "") + message; //собираем сообщение
            if (settings.PARAMETERS["LOG"].value_bool) //нужен ли лог
            {
                if (first_time) //программа запущена первый раз 
                {
                    first_time = false;
                    current_data = DateTime.Now.ToString(date_format); //текущая дата
                    log_buffer.AppendLine(first_mes);  //уведомление о запуске лога
                    file_name = settings.PARAMETERS["LOG_USE_NEW_FILE"].value_bool ? 
                                DateTime.Now.ToString(date_time_file_format) + "_" +
                                settings.PARAMETERS["LOG_FILE_NAME"].value_string :
                                settings.PARAMETERS["LOG_FILE_NAME"].value_string; //имя файла лога
                }
                if (DateTime.Now.ToString(date_format) != current_data) //произошла смена даты (полночь)
                {
                    current_data = DateTime.Now.ToString(date_format);
                    log_buffer.AppendLine(another_day_mes); 
                    file_name = settings.PARAMETERS["LOG_USE_NEW_FILE"].value_bool ?
                                DateTime.Now.ToString(date_time_file_format) + "_" +
                                settings.PARAMETERS["LOG_FILE_NAME"].value_string :
                                settings.PARAMETERS["LOG_FILE_NAME"].value_string;
                }

                log_buffer.AppendLine (current_mes); //записали в лог сообщение
                 
                if (settings.setting_ready) //запись в файл
                {
                    bool dir_success = false; //получили ли доступ к папке логов
                    string dir_name = settings.PARAMETERS["LOG_DIR_NAME"].value_string; //имя папки логов
                    try
                    {
                        Directory.CreateDirectory(dir_name); //создаём папку, если нет
                        dir_success = true;
                    }
                    catch (Exception)
                    {
                        log_buffer.AppendLine(DateTime.Now.ToString(time_format) + error + ": " + dir_error);
                    }
                    if (dir_success)
                    {
                        try //create log file
                        {
                            using (StreamWriter stream = new StreamWriter(
                                new FileStream(dir_name + @"\" + file_name, FileMode.Append, FileAccess.Write, FileShare.None)))
                            {
                                stream.Write(log_buffer);
                                stream.Close(); //закрыли поток
                                log_buffer.Clear(); //чистим буфер
                            }
                        }
                        catch (Exception exc)
                        {
                            log_buffer.AppendLine(DateTime.Now.ToString(time_format) + error + ": " + exc.ToString());
                        }
                    }
                }
            }

            if (settings.PARAMETERS["DEBUG"].value_bool)
            {
                if (is_error)
                {
                    MessageBox.Show(message, error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (settings.PARAMETERS["DEBUG_SHOW_NOT_ERROR_MES"].value_bool)
                {
                    MessageBox.Show(message, information, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            } 
        }
    }
}
