using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using StructureBuilder.Events;
using StructureBuilder.Models;
using StructureBuilder.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace StructureBuilder.ViewModels
{
    public class StructureStepViewModel:BindableBase
    {
        private string _resultStructure;

        public string ResultStructure
        {
            get { return _resultStructure; }
            set { SetProperty(ref _resultStructure,value); }
        }
        private string _selectedBaseStructure;

        public string SelectedBaseStructure
        {
            get { return _selectedBaseStructure; }
            set { SetProperty(ref _selectedBaseStructure,value); }
        }

        private string _selectedTargetStructure;

        public string SelectedTargetStructure
        {
            get { return _selectedTargetStructure; }
            set { SetProperty(ref _selectedTargetStructure,value); }
        }
        private string _selectedOperation;

        public string SelectedOperation
        {
            get { return _selectedOperation; }
            set { SetProperty(ref _selectedOperation,value); }
        }
        private bool _bTemp;

        public bool bTemp
        {
            get { return _bTemp; }
            set { SetProperty(ref _bTemp,value); }
        }
        private double _margin;

        public double Margin
        {
            get { return _margin; }
            set { SetProperty(ref _margin,value); }
        }
        private int _stepId;

        public int StepId
        {
            get { return _stepId; }
            set { _stepId = value; }
        }
        public AsymmetricMarginModel AsymmetricMargins { get; set; }
        public ObservableCollection<string> Structures { get; set; }
        public ObservableCollection<string> Operations { get; set; }

        private StructureSet _structureSet;
        private IEventAggregator _eventAggregator;
        private AsymmetricMarginView _assymetricMarginView;

        public DelegateCommand SetAsymmetricMarginCommand { get; private set; }

        public StructureStepViewModel(StructureSet structureSet, int stepId, IEventAggregator eventAggregator)
        {
            Structures = new ObservableCollection<string>();
            Operations = new ObservableCollection<string>();
            StepId = stepId;
            //set initial asymmetric margin model so it is not null. 
            AsymmetricMargins = new AsymmetricMarginModel("Outer",0,0,0,0,0,0);
            _structureSet = structureSet;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<SetAsymmetricMarginEvent>().Subscribe(OnSetAsymmetricMargin);

            SetAsymmetricMarginCommand = new DelegateCommand(OnSetAsymmetricMargin);
            InitializeCollections();
        }

        private void OnSetAsymmetricMargin(AsymmetricMarginViewModel obj)
        {
            if(StepId == obj.StructureStepId)
            {
                AsymmetricMargins = new AsymmetricMarginModel(obj.SelectedMarginType,
                    obj.AnteriorMargin,
                    obj.PosteriorMargin,
                    obj.LeftMargin,
                    obj.RightMargin,
                    obj.SuperiorMargin,
                    obj.InferiorMargin);
                _assymetricMarginView.Close();
                _assymetricMarginView = null;
            }
        }

        private void OnSetAsymmetricMargin()
        {
            _assymetricMarginView = new AsymmetricMarginView();
            _assymetricMarginView.DataContext = new AsymmetricMarginViewModel(StepId, _eventAggregator, AsymmetricMargins);
            _assymetricMarginView.ShowDialog();
        }

        private void InitializeCollections()
        {
            foreach(var structure in _structureSet.Structures.OrderByDescending(st=>st.DicomType.Contains("TV")).ThenBy(st=>st.Id))
            {
                Structures.Add(structure.Id);
            }
            Operations.Add("Margin");
            Operations.Add("Asymmetric Margin");
            Operations.Add("And");
            Operations.Add("Or");
            Operations.Add("Sub");
        }
    }
}
