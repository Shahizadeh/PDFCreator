﻿using System;
using System.IO;
using System.Windows;

namespace pdfforge.PDFCreator.ErrorReport
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 1)
                return;

            var errorFile = e.Args[0];

            if (!File.Exists(errorFile))
                return;

            ShowReportWindow(errorFile);
        }

        private bool ShowReportWindow(string errorFile)
        {
            try
            {
                var errorAssistant = new ErrorAssistant("pdfcreator", new Version());
                var report = errorAssistant.Tartaros.LoadReport(errorFile);

                var err = new ErrorReportWindow(report, errorAssistant.Tartaros);
                err.ShowDialog();

                File.Delete(errorFile);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
