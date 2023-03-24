using Autofac;
using StructureBuilder.Startup;
using StructureBuilder.ViewModels;
using StructureBuilder.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

[assembly:ESAPIScript(IsWriteable =true)]
namespace StructureBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string _patientId = e.Args.FirstOrDefault().Split(';').FirstOrDefault();
            string _imageId = e.Args.FirstOrDefault().Split(';').ElementAt(1);
            string _structureSetId = e.Args.FirstOrDefault().Split(';').ElementAt(2);
            try
            {
                using (VMS.TPS.Common.Model.API.Application app = VMS.TPS.Common.Model.API.Application.CreateApplication())
                {
                    Patient patient = app.OpenPatientById(_patientId);
                    patient.BeginModifications();
                    StructureSet structureSet = patient.StructureSets.FirstOrDefault(ss => ss.Id == _structureSetId && ss.Image.Id == _imageId);
                    var bootstrapper = new Bootstrapper();
                    var container = bootstrapper.Boostrap(structureSet,app);
                    var MV = container.Resolve<MainView>();
                    MV.DataContext = container.Resolve<MainViewModel>();
                    MV.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
