using MahApps.Metro.Controls;

namespace StructureBuilder.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : MetroWindow
    {
        public VMS.TPS.Common.Model.API.Application _app;
        public MainView()
        {
            InitializeComponent();
            Closing += MainView_Closing;
        }

        private void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _app.ClosePatient();

        }
    }
}
