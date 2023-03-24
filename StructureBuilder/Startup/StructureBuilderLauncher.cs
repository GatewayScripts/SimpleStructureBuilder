﻿using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Diagnostics;
using System.IO;
using System;

//[assembly: AssemblyVersion("1.0.0.1")]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            try
            {
                if (context.Patient != null)
                {
                    Process.Start(AppExePath(), $"\"{context.Patient.Id};{context.StructureSet.Image.Id};{context.StructureSet.Id}\"");
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Failed to start application.");
            }
        }
        private string AppExePath()
        {
            return @"put exe path here";

        }
    }
}