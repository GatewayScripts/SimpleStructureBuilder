using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using StructureBuilder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace StructureBuilder.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private Application _application;
        private StructureSet _structureSet;
        private IEventAggregator _eventAggregator;
        public bool bValidForClinic { get; set; }
        public ObservableCollection<StructureStepViewModel> StructureCreationSteps { get; set; }
        private StructureStepViewModel _selectedStructureCreationStep;

        public StructureStepViewModel SelectedStructureCreationStep
        {
            get { return _selectedStructureCreationStep; }
            set { 
                SetProperty(ref _selectedStructureCreationStep, value);
                RemoveStepCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand ImportTemplateCommand { get; set; }
        public DelegateCommand ExportTemplateCommand { get; set; }
        public DelegateCommand AddStepCommand { get; set; }
        public DelegateCommand RunStepsCommand { get; set; }
        public DelegateCommand RemoveStepCommand { get; set; }
        public MainViewModel(Application application, StructureSet structureSet, IEventAggregator eventAggregator)
        {
            _application = application;
            _structureSet = structureSet;
            _eventAggregator = eventAggregator;
            bValidForClinic = ConfigurationManager.AppSettings["ValidForClinicalUse"] == "true";
            StructureCreationSteps = new ObservableCollection<StructureStepViewModel>();
            ImportTemplateCommand = new DelegateCommand(OnImport);
            ExportTemplateCommand = new DelegateCommand(OnExport);
            AddStepCommand = new DelegateCommand(OnAddStep);
            RemoveStepCommand = new DelegateCommand(OnRemoveStep, CanRemoveStep);
            RunStepsCommand = new DelegateCommand(OnRunSteps);

        }

        private bool CanRemoveStep()
        {
            return SelectedStructureCreationStep != null;
        }

        private void OnRemoveStep()
        {
            if (SelectedStructureCreationStep != null)
            {
                StructureCreationSteps.Remove(SelectedStructureCreationStep);
            }
        }

        private void OnRunSteps()
        {
            //validate structures.
            bool valid = true;
            List<string> invalidStructures = new List<string>();
            var exclusions = ConfigurationManager.AppSettings["ExclusionTypes"].Split(';');
            foreach (var step in StructureCreationSteps)
            {
                if (_structureSet.Structures.Any(st => st.Id.Equals(step.ResultStructure, StringComparison.OrdinalIgnoreCase)))
                {
                    var localStructure = _structureSet.Structures.First(st => st.Id.Equals(step.ResultStructure, StringComparison.OrdinalIgnoreCase));
                    if (exclusions.Any(e => e.Equals(localStructure.DicomType)))
                    {
                        valid = false;
                        invalidStructures.Add(localStructure.Id);
                    }
                }
            }
            if (!valid)
            {
                System.Windows.MessageBox.Show($"The following structures cannot be overriden:\n\t{String.Join("\n\t", invalidStructures)}");
                return;
            }
            //check for missing inputs
            //List<string> emptyBaseStructures = new List<string>();
            //List<string> emptyTargetStructures = new List<string>();

            foreach (var step in StructureCreationSteps)
            {
                if (String.IsNullOrEmpty(step.SelectedBaseStructure))
                {
                    //emptyBaseStructures.Add(step.SelectedBaseStructure);
                    valid = false;
                }
                if (String.IsNullOrEmpty(step.SelectedTargetStructure) && !step.SelectedOperation.Contains("Margin"))
                {
                    //emptyTargetStructures.Add(step.SelectedTargetStructure);
                    valid = false;
                }
            }
            if (!valid)
            {
                System.Windows.MessageBox.Show($"Some base structures or target structures are missing.");
                return;
            }


            //build structure with ESAPI
            foreach (var step in StructureCreationSteps)
            {
                var baseStructure = _structureSet.Structures.First(s => s.Id.Equals(step.SelectedBaseStructure));

                Structure newStructure = null;
                //first check if structure exists, if so modify, if no, create.
                if (_structureSet.Structures.Any(s => s.Id.Equals(step.ResultStructure, StringComparison.OrdinalIgnoreCase)))
                {
                    //check that the existing structure isn't an exclusion structure type.
                    var currentStructure = _structureSet.Structures.First(st => st.Id.Equals(step.ResultStructure, StringComparison.OrdinalIgnoreCase));

                    newStructure = _structureSet.Structures.First(s => s.Id.Equals(step.ResultStructure, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    if (step.StructureCode != null)
                    {
                        newStructure = _structureSet.AddStructure(new StructureCodeInfo(step.StructureCode.Scheme, step.StructureCode.Code));
                        newStructure.Id = step.ResultStructure;
                    }
                    else
                    {
                        newStructure = _structureSet.AddStructure("CONTROL", step.ResultStructure);
                    }
                    if (!String.IsNullOrEmpty(step.StructureColor))
                    {
                        newStructure.Color = (Color)ColorConverter.ConvertFromString(step.StructureColor);
                    }
                }
                //comment about auto generated structure.
                newStructure.Comment = $"Auto Generated Structure {Assembly.GetExecutingAssembly().GetName()}";
                //if base structure is high resolution make the new structure high resolution.
                if (baseStructure.IsHighResolution)
                {
                    newStructure.ConvertToHighResolution();
                }
                if (step.SelectedOperation == "Margin")
                {
                    //symmetric margins only supported.
                    newStructure.SegmentVolume = baseStructure.SegmentVolume.Margin(step.Margin);
                }
                else if (step.SelectedOperation == "Asymmetric Margin")
                {
                    newStructure.SegmentVolume = baseStructure.SegmentVolume.AsymmetricMargin(
                        new VMS.TPS.Common.Model.Types.AxisAlignedMargins(
                            step.AsymmetricMargins.MarginDirection == "Outer" ? VMS.TPS.Common.Model.Types.StructureMarginGeometry.Outer : VMS.TPS.Common.Model.Types.StructureMarginGeometry.Inner,
                            step.AsymmetricMargins.Right,
                            step.AsymmetricMargins.Ant,
                            step.AsymmetricMargins.Inf,
                            step.AsymmetricMargins.Left,
                            step.AsymmetricMargins.Post,
                            step.AsymmetricMargins.Sup));
                }
                else
                {
                    var targetStructure = _structureSet.Structures.First(s => s.Id.Equals(step.SelectedTargetStructure));
                    //check that target structure and base structure are the same resolution.
                    if ((targetStructure.IsHighResolution && !baseStructure.IsHighResolution) || (!targetStructure.IsHighResolution && baseStructure.IsHighResolution))
                    {
                        System.Windows.MessageBox.Show($"Base Structure ({baseStructure.Id} - {(baseStructure.IsHighResolution ? "High Res" : "Standard Res")}) and Target Structure ({targetStructure.Id} - {(targetStructure.IsHighResolution ? "High Res" : "Standard Res")}) are not the same resolution.\nPlease close the app and update the structures.");
                        return;
                    }
                    //Other steps require the operation and a target structure.
                    if (targetStructure.IsHighResolution && !newStructure.IsHighResolution)
                    {
                        newStructure.ConvertToHighResolution();
                    }
                    if (newStructure.IsHighResolution && !targetStructure.IsHighResolution && targetStructure.DicomType != "EXTERNAL")
                    {
                        targetStructure.ConvertToHighResolution();
                    }
                    if (step.SelectedOperation == "And")
                    {
                        newStructure.SegmentVolume = baseStructure.SegmentVolume.And(targetStructure);
                    }
                    else if (step.SelectedOperation == "Or")
                    {
                        newStructure.SegmentVolume = baseStructure.SegmentVolume.Or(targetStructure);
                    }
                    else if (step.SelectedOperation == "Sub")
                    {
                        newStructure.SegmentVolume = baseStructure.SegmentVolume.Sub(targetStructure);
                    }
                }
            }

            //remove all structures that were only temporary.
            //TODO:Add some validation that makes sure the resultStructure wasn't an already existing structure. 
            //We CANNOT accidentally delete a manually generated contour. 
            foreach (var tempStep in StructureCreationSteps.Where(scs => scs.bTemp))
            {
                _structureSet.RemoveStructure(_structureSet.Structures.FirstOrDefault(s => s.Id == tempStep.ResultStructure));
            }
            if (System.Windows.MessageBox.Show("Save Modifications?", "Save", System.Windows.MessageBoxButton.YesNo)
                == System.Windows.MessageBoxResult.Yes)
            {
                _application.SaveModifications();
            }
        }

        private void OnAddStep()
        {
            List<string> priorSteps = StructureCreationSteps.Select(scs => scs.ResultStructure).ToList();
            var creationStep = new StructureStepViewModel(_structureSet, StructureCreationSteps.Count(), _eventAggregator);
            if (priorSteps.Any())
            {
                //add all structures to the structures collections. 
                foreach (var step in priorSteps)
                {
                    creationStep.Structures.Add(step);
                }

            }
            StructureCreationSteps.Add(creationStep);
        }

        private void OnExport()
        {
            List<StructureCreationModel> scmList = new List<StructureCreationModel>();
            foreach (var step in StructureCreationSteps)
            {
                StructureCreationModel scModel = new StructureCreationModel();
                scModel.StructureStepId = step.StepId;
                scModel.ResultStructure = step.ResultStructure;
                scModel.BaseStructure = step.SelectedBaseStructure;
                scModel.StructureOperation = step.SelectedOperation;
                scModel.TargetStructure = step.SelectedTargetStructure;
                scModel.Margin = step.Margin;
                scModel.bTemp = step.bTemp;
                scModel.AsymmetricMargin = step.AsymmetricMargins;
                scModel.ResultStructureColor = step.StructureColor;
                scModel.ResultStructureCode = step.StructureCode;
                scmList.Add(scModel);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JSON (*json)|*.json";
            sfd.Title = "Save Structure Template";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(scmList));
            }
        }

        private void OnImport()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON (*json)|*.json";
            ofd.Title = "Open Structure Template";
            if (ofd.ShowDialog() == true)
            {
                StructureCreationSteps.Clear();
                //get list from template
                //TODO error checking onJson conversion.
                List<StructureCreationModel> scmList = JsonConvert.DeserializeObject<List<StructureCreationModel>>(File.ReadAllText(ofd.FileName));
                foreach (var scm in scmList)
                {
                    OnAddStep();//add the step manually, then fill the data from JSON. 
                    var scStep = StructureCreationSteps.Last();
                    scStep.bTemp = scm.bTemp;
                    scStep.ResultStructure = scm.ResultStructure;
                    scStep.SelectedBaseStructure =
                       scStep.Structures.Any(st => st.Equals(scm.BaseStructure, StringComparison.OrdinalIgnoreCase)) ?
                        scStep.Structures.First(st=>st.Equals(scm.BaseStructure,StringComparison.OrdinalIgnoreCase))
                        : null;
                    scStep.SelectedTargetStructure =
                        scStep.Structures.Any(st => st.Equals(scm.TargetStructure, StringComparison.OrdinalIgnoreCase)) ?
                        scStep.Structures.First(st=>st.Equals(scm.TargetStructure,StringComparison.OrdinalIgnoreCase))
                        : null;
                    scStep.Margin = scm.Margin;
                    scStep.SelectedOperation = scm.StructureOperation;
                    scStep.AsymmetricMargins = scm.AsymmetricMargin;
                    scStep.StructureCode = scm.ResultStructureCode;
                    scStep.StructureColor = scm.ResultStructureColor;
                }
            }
        }
    }
}
