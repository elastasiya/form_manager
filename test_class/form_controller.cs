using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace test_class
{
    public static class form_manager
    {
        public static int curr_form_num = 1;
        private static Form curr_form_obj = null;
        static List<Type> forms = new List<Type>();

        public static void fill_forms ()
        {
            List<Type> forms_ = new List<Type>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                forms_.AddRange(from t in asm.GetTypes() where t.IsSubclassOf(typeof(Form)) select t);
            }
            string curr_namespace = (typeof(form_manager)).Namespace;
            foreach (Type form_ in forms_)
            {
                if (form_.Namespace == curr_namespace)
                {
                    forms.Add(form_);
                }
                
            }

            string form_names = "";
            foreach (Type form in forms)
            {
                form_names += form.Name + "; ";
            }
            MessageBox.Show(form_names + (typeof(form_manager)).Namespace);

        }
    }
}
