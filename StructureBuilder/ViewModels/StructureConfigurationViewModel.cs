using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using StructureBuilder.Events;
using StructureBuilder.Models;
using StructureBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StructureBuilder.ViewModels
{
    internal class StructureConfigurationViewModel : BindableBase
    {
        private StructureCreationModel _localStructure;
        private IEventAggregator _eventAggregator;
        private string _structureId;

        public string StructureId
        {
            get { return _structureId; }
            set
            {
                SetProperty(ref _structureId, value);
                _localStructure.ResultStructure = StructureId;
            }
        }

        private string _selectedStructureColor;

        public string SelectedStructureColor
        {
            get { return _selectedStructureColor; }
            set
            {
                SetProperty(ref _selectedStructureColor, value);
                if (SelectedStructureColor != null)
                {
                    _localStructure.ResultStructureColor = SelectedStructureColor;
                }
            }
        }
        private StructureCodeModel _selectedStructureCode;

        public StructureCodeModel SelectedStructureCode
        {
            get { return _selectedStructureCode; }
            set
            {
                _selectedStructureCode = value;
                if (SelectedStructureCode != null)
                {
                    _localStructure.ResultStructureCode = SelectedStructureCode;
                }
            }
        }

        //collections
        public List<string> StructureColors { get; set; }
        public List<StructureCodeModel> StructureCodes { get; set; }

        //commands
        public DelegateCommand SaveCommand { get; private set; }
        public StructureConfigurationViewModel(StructureCreationModel structure, IEventAggregator eventAggregator)
        {
            _localStructure = new StructureCreationModel();
            _eventAggregator = eventAggregator;
            StructureCodes = new List<StructureCodeModel>();
            StructureColors = new List<string>();
            FillColors();
            FillStructures();
            if (structure.ResultStructureCode!=null)
            {
                SelectedStructureCode = structure.ResultStructureCode;
            }
            if (!String.IsNullOrEmpty(structure.ResultStructureColor))
            {
                SelectedStructureColor = structure.ResultStructureColor;
            }
            StructureId = structure.ResultStructure;
            _localStructure.StructureStepId = structure.StructureStepId;
            SaveCommand = new DelegateCommand(OnSave);
        }

        private void FillStructures()
        {
            foreach(var code in StructureCodeService.StructureCodes)
            {
                StructureCodes.Add(code);
            }
        }

        private void FillColors()
        {
            Type brushType = typeof(System.Windows.Media.Brushes);
            var properties = brushType.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            //KnownColor[] colors = Enum.GetValues(typeof(KnownColor));
            //var colorValues = new ColorConverter().GetStandardValues();
            foreach(var color in properties)
            {
                var colorString = ((SolidColorBrush)color.GetValue(null, null)).Color.ToString();
                StructureColors.Add(colorString);
                //StructureColors.Add(color.ToString());
            }
        }

        private void OnSave()
        {
            _eventAggregator.GetEvent<UpdateStructureDetailEvent>().Publish(_localStructure);
        }
    }
}
