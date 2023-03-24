using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using StructureBuilder.Events;
using StructureBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureBuilder.ViewModels
{
    public class AsymmetricMarginViewModel:BindableBase
    {
        private double _anteriorMargin;

        public double AnteriorMargin
        {
            get { return _anteriorMargin; }
            set { SetProperty(ref _anteriorMargin,value); }
        }
        private double _posteriorMargin;

        public double PosteriorMargin
        {
            get { return _posteriorMargin; }
            set { SetProperty(ref _posteriorMargin,value); }
        }
        private double _leftMargin;

        public double LeftMargin
        {
            get { return _leftMargin; }
            set { SetProperty(ref _leftMargin,value); }
        }
        private double _rightMargin;

        public double RightMargin
        {
            get { return _rightMargin; }
            set { SetProperty(ref _rightMargin, value); }
        }
        private double _superiorMargin;

        public double SuperiorMargin
        {
            get { return _superiorMargin; }
            set { SetProperty(ref _superiorMargin,value); }
        }
        private double _inferiorMargin;

        public double InferiorMargin
        {
            get { return _inferiorMargin; }
            set { SetProperty(ref _inferiorMargin,value); }
        }
        public int StructureStepId { get; set; }

        private IEventAggregator _eventAggregator;

        public DelegateCommand SaveAsymmetricMarginCommand { get; private set; }
        public List<string> MarginTypes { get; private set; }
        private string _selectedMarginType;

        public string SelectedMarginType
        {
            get { return _selectedMarginType; }
            set { SetProperty(ref _selectedMarginType,value); }
        }

        public AsymmetricMarginViewModel(int structureStepId, IEventAggregator eventAggregator,
            AsymmetricMarginModel asymmetricMargin)
        {
            StructureStepId = structureStepId;
            _eventAggregator = eventAggregator;
            AnteriorMargin = asymmetricMargin.Ant;
            PosteriorMargin = asymmetricMargin.Post;
            LeftMargin = asymmetricMargin.Left;
            RightMargin = asymmetricMargin.Right;
            SuperiorMargin = asymmetricMargin.Sup;
            InferiorMargin = asymmetricMargin.Inf;
            SaveAsymmetricMarginCommand = new DelegateCommand(OnSaveAsymmetricMargin);
            MarginTypes = new List<string> { "Outer", "Inner" };
            SelectedMarginType = MarginTypes.First();
        }

        private void OnSaveAsymmetricMargin()
        {
            _eventAggregator.GetEvent<SetAsymmetricMarginEvent>().Publish(this);
        }
    }
}
