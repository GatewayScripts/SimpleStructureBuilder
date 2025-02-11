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
using System.Security.Permissions;
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
                if (step.SelectedBaseStructure?.StructureId == null)
                {
                    //emptyBaseStructures.Add(step.SelectedBaseStructure);
                    valid = false;
                }
                if (step.SelectedTargetStructure?.StructureId == null && !(step.SelectedOperation.Contains("Margin")|| step.SelectedOperation.Contains("Res")))
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
                var baseStructure = _structureSet.Structures.First(s => s.Id.Equals(step.SelectedBaseStructure.StructureId));

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
                if (baseStructure.IsHighResolution && !step.SelectedOperation.Contains("LoRes"))
                {
                    if (newStructure.CanConvertToHighResolution())
                    {
                        newStructure.ConvertToHighResolution();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Cannot convert {newStructure.Id} to high resolution segment");
                    }
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
                else if(step.SelectedOperation == "HiRes")
                {
                    //if base structure is already hi res, then make hi res and copy.
                    if (baseStructure.IsHighResolution)
                    {
                        newStructure.ConvertToHighResolution();
                        newStructure.SegmentVolume = baseStructure.SegmentVolume;
                    }
                    else
                    {
                        //if base structure is not hi res, copy then make hi res.
                        newStructure.SegmentVolume = baseStructure.SegmentVolume;
                        newStructure.ConvertToHighResolution();
                    }
                }
                else if(step.SelectedOperation == "LoRes")
                {
                    if (baseStructure.IsHighResolution)
                    {
                        //loop through contours and copy contours to new image.
                        for(int slice = 0; slice < _structureSet.Image.ZSize; slice++)
                        {
                            var baseContours = baseStructure.GetContoursOnImagePlane(slice);
                            if (baseContours.Any())
                            {
                                foreach(var contour in baseContours)
                                {
                                    newStructure.AddContourOnImagePlane(contour, slice);
                                }
                            }
                        }
                    }
                    else
                    {
                        //if structure is already low resolution, simply copy.
                        newStructure.SegmentVolume = baseStructure.SegmentVolume;
                    }
                }
                else
                {
                    var targetStructure = _structureSet.Structures.First(s => s.Id.Equals(step.SelectedTargetStructure.StructureId));
                    //check that target structure and base structure are the same resolution.
                    if ((targetStructure.IsHighResolution && !baseStructure.IsHighResolution) || (!targetStructure.IsHighResolution && baseStructure.IsHighResolution))
                    {
                        System.Windows.MessageBox.Show($"Base Structure ({baseStructure.Id} - {(baseStructure.IsHighResolution ? "High Res" : "Standard Res")}) and Target Structure ({targetStructure.Id} - {(targetStructure.IsHighResolution ? "High Res" : "Standard Res")}) are not the same resolution.\nPlease use the HiRes and LoRes Conversions.");
                        return;
                    }
                    //Other steps require the operation and a target structure.
                    //if (targetStructure.IsHighResolution && !newStructure.IsHighResolution)
                    //{
                    //    newStructure.ConvertToHighResolution();
                    //}
                    //Convert target to high resolution if it is not BODY
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
            List<StructureModel> priorSteps = new List<StructureModel>();
            foreach(var step in StructureCreationSteps)
            {
                StructureModel structureModel = new StructureModel();
                structureModel.StructureId = step.ResultStructure;
                if ((step.SelectedBaseStructure.bHiRes || step.SelectedOperation == "HiRes") && step.SelectedOperation != "LoRes")
                {
                    structureModel.bHiRes = true;
                }
                priorSteps.Add(structureModel);
            }
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
            List<StructureCreationModelTransfer> scmList = new List<StructureCreationModelTransfer>();
            foreach (var step in StructureCreationSteps)
            {
                StructureCreationModelTransfer scModel = new StructureCreationModelTransfer();
                scModel.StructureStepId = step.StepId;
                scModel.ResultStructure = step.ResultStructure;
                scModel.BaseStructure = step.SelectedBaseStructure?.StructureId;
                scModel.StructureOperation = step.SelectedOperation;
                scModel.TargetStructure = step.SelectedTargetStructure?.StructureId;
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
                List<StructureCreationModelTransfer> scmList = 
                    JsonConvert.DeserializeObject<List<StructureCreationModelTransfer>>(File.ReadAllText(ofd.FileName));

                foreach (var scm in scmList)
                {
                    OnAddStep();//add the step manually, then fill the data from JSON. 
                    var scStep = StructureCreationSteps.Last();
                    scStep.bTemp = scm.bTemp;
                    scStep.ResultStructure = scm.ResultStructure;
                    scStep.SelectedBaseStructure =
                       scStep.Structures.Any(st => st.StructureId.Equals(scm.BaseStructure, StringComparison.OrdinalIgnoreCase)) ?
                        scStep.Structures.First(st => st.StructureId.Equals(scm.BaseStructure,StringComparison.OrdinalIgnoreCase))
                        : null;
                    scStep.SelectedTargetStructure =
                        scStep.Structures.Any(st => st.StructureId.Equals(scm.TargetStructure, StringComparison.OrdinalIgnoreCase)) ?
                        scStep.Structures.First(st=>st.StructureId.Equals(scm.TargetStructure,StringComparison.OrdinalIgnoreCase))
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
