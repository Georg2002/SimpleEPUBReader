using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer
{
    public static class Util
    {
        public static bool IsOver(this Task task) => task is null || task.IsCompleted;
        public static async void CatchAll(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Shutdown();
            }
        }
    }
}
