using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_class
{
    public static class settings
    {
        public static bool setting_ready = false;

        public class parameter //параметр - bool или string
        {
            public parameter (bool value_b)  //конструктор для bool
            {
                is_bool = true;
                value_bool = value_b;
            }
            public parameter(string value_s)  //конструктор для string
            {
                is_bool = false;
                value_string = value_s;
            }
            public bool is_bool = false; //bool = true, string = false
            public bool value_bool = true;
            public string value_string = "";
        }

        static string SETTINGS_FILE_NAME = "settings.ini"; //файл с пользовательскими настройками

        public static Dictionary<string, parameter> PARAMETERS = new Dictionary<string, parameter>() //словарь параметров
        {
            { "LOG", new parameter(false)}, //используем ли лог
            { "DEBUG", new parameter(false)}, //используем ли режим отладки (вывод всех ошибок в messageBox)
            { "DEBUG_SHOW_NOT_ERROR_MES", new parameter(false)}, //выводим не только ошибки
            { "LOG_FILE_NAME", new parameter("log.txt")}, //имя файла лога
            { "LOG_USE_NEW_FILE",new parameter(true)}, //если да - создаёт новый файл лога при каждом запуске программы
            { "LOG_DIR_NAME", new parameter("logs")}, //имя (путь) папки с логами
        };

        public static void read_settings () //считываем настройки их файла
        {
            bool success_reading_setting = false; //удалось ли считать настройки
            Dictionary<string, parameter> new_parameters = new Dictionary<string, parameter>(); //новые настройки
            try //open settings file
            {
                using (StreamReader stream = new StreamReader( //используем конструкцию used: ->
                    new FileStream( //потокобезопасность и автоматическая очистка памяти
                        SETTINGS_FILE_NAME, //имя файла настроек
                        FileMode.Open, FileAccess.Read, FileShare.None))) //параметры доступа к файлу
                {
                    int num_of_rows = 0; //количество строк
                    int max_count_of_rows = 500; //максимальное количество строк в файле, после котрого будет ошибка
                    foreach (string param in PARAMETERS.Keys) //перебираем параметры
                    {
                        if (!stream.EndOfStream) //если файл не закончился
                        {
                            if (num_of_rows < max_count_of_rows) //если не превышено количество строк
                            {
                                string line = stream.ReadLine(); //считываем троку
                                num_of_rows++;
                                while (line == Environment.NewLine) //если пустая строка
                                {
                                    line = stream.ReadLine(); //считываем новую и повтрояем
                                    num_of_rows++;
                                    if (num_of_rows >= max_count_of_rows) //если не превышено количество строк
                                    {
                                        throw new Exception("Too big file!");
                                    }
                                }
                                line = line.ToLower(); //переводим строку в нижний регистр
                                line = line.Replace("\t", " "); //заменяем табуляцию на пробел
                                line = System.Text.RegularExpressions.Regex.Replace(line, @"\s+", " "); //заменяем повторяющиеся пробелы на 1
                                line = line.Trim(); //удаляем пробелы в ачале и конце, TODO: исправить, чтобы можно было использовать пробелы в значении параметра
                                string[] words = line.Split(' '); // разделяем строку в массив по пробелам
                                if (words[0] == param.ToLower()) //если первое слово - имя параметра
                                {
                                    if (PARAMETERS[param].is_bool) //если второе должно быть true/false
                                    {
                                        if (words[1] == "true")
                                        {
                                            new_parameters.Add(param, new parameter(true));
                                        }
                                        else if (words[1] == "false")
                                        {
                                            new_parameters.Add(param, new parameter(false));
                                        }
                                        else
                                        {
                                            throw new Exception("Incorrect parameter! (expect true or false)");
                                        }
                                    }
                                    else //string
                                    {
                                        new_parameters.Add(param, new parameter(words[1]));
                                    }
                                }
                            }
                            else //слишком много строк
                            {
                                throw new Exception("Too big file!");
                            }
                        }
                        else //файл закончен слишком рано
                        {
                            throw new Exception("Unexpected end of file!");
                        }
                    }
                    stream.Close(); //закрыли поток
                }
                success_reading_setting = true; //удачно считали настройки
            }
            catch (Exception exc) 
            {
                logs.mes_manage(exc.ToString(), true);
                success_reading_setting = false;
            }

            if (success_reading_setting) //считывание удалось 
            {
                PARAMETERS = new_parameters; //параметры по умолчанию заменены на новые 

                string report = "";
                foreach (string param in PARAMETERS.Keys)
                {
                    report +=
                        param + " " +
                        ((PARAMETERS[param].is_bool) ?
                        (PARAMETERS[param].value_bool.ToString()) :
                        (PARAMETERS[param].value_string)) + Environment.NewLine;
                }
                logs.mes_manage("Настройки считаны из файла");
            }
            else //считывание не удалось - пытаемся записать файл с параметрами по умолчанию
            {
                try //create settings file
                {
                    using (StreamWriter stream = new StreamWriter(
                        new FileStream(
                            SETTINGS_FILE_NAME,
                            FileMode.Create, FileAccess.Write, FileShare.None)))
                    {
                        foreach (string param in PARAMETERS.Keys)
                        {
                            stream.WriteLine(//записываем параметры в файл
                                param+ " "+ 
                                ((PARAMETERS[param].is_bool) ? 
                                (PARAMETERS[param].value_bool.ToString()): 
                                (PARAMETERS[param].value_string)));
                        }
                        stream.Close(); //закрыли поток
                        logs.mes_manage("Настройки по умолчанию записаны в файл");
                    }
                }
                catch (Exception exc)
                {
                    logs.mes_manage(exc.ToString(), true);
                }
            }
            setting_ready = true;
        }
    }
}
