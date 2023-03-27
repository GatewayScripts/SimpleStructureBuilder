using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Diagnostics;
using System.IO;

namespace VMS.TPS
{

    public class Script
    {

        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            // TODO : Add here your code that is called when the script is launched from Portal Dosimetry
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(AppExePath());
                startInfo.Arguments = String.Format("\"{0};{1};{2}\"", context.Patient.Id, context.StructureSet.Image.Id, context.StructureSet.Id);
                Process.Start(startInfo);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to start application.");
            }
        }

        private string AppExePath()
        {
            return @"EXE Path goes here.";
        }
    }

}
