using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace TestOnvif
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += new ThreadExceptionEventHandler(ThreadUnhandledExceptionHandler);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(DomainUnhandledExceptionHandler);

            Logger.LoggingEnabled = true;
            

            try
            {
                MainForm form = MediaService.Instance.CreateMainForm();
                Logger.Verbose += form.Verbose;

                Application.Run(form);
            }
            catch (Exception e)
            {
                ProcessException(e);
            }

            Application.ThreadException -= new ThreadExceptionEventHandler(ThreadUnhandledExceptionHandler);
            AppDomain.CurrentDomain.UnhandledException -= new System.UnhandledExceptionEventHandler(DomainUnhandledExceptionHandler);
        }


        static void DomainUnhandledExceptionHandler(object sender, System.UnhandledExceptionEventArgs args)
        {
            ProcessException((Exception)args.ExceptionObject);
        }

        static void ThreadUnhandledExceptionHandler(object sender, ThreadExceptionEventArgs args)
        {
            ProcessException((Exception)args.Exception);
        }

        public static void ProcessException(Exception exception)
        {
            try
            {
                MessageBox.Show(exception.Message);
                Logger.Write(exception);
            }
            finally
            {
                if (exception != null)
                {
                    Application.Exit();
                }
            }
        }
    }
}
