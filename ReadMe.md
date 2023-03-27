<h1>Simple Structure Builder</h1>
<h2>Disclaimer</h2>
<p>Simple Structure Builder &copy; 2023. The SimpleStructureBuilder, herein software tool, is a training tool meant to teach script automation for the Clinical Automation with Varian APIs training course. <a href="https://gatewayscripts.com">See course description here.</a> The code is delivered through an available compiled download and open-source code for learning purposes. The tool may be updated as needs arise in the course. The software tool is not intended for clinical use. Clinicians may choose to validate the code for clinical use as it may have some clinical utility. The completelness and accuracy of the software is to be determined by the qualified clinician. If users find errors in the development of the Simple Structure Builder, please submit an issue in github or email the develop directly at mschmidt@gatewayscripts.com. Gateway Scripts or Matthew Schmidt or any affiliated organizations is not liable for structures created with this software or any derivatives from this software.</p>
<h2>Instructions for Use</h2>
<p>Launching the structure builder brings the user to the following UI:</p>
<img src="/StructureBuilder/Resources/Images/Image1.JPG" alt="Structure Builder UI"/>
<p>The UI for the structure builder only has 4 buttons 
  <ol type="A">
    <li>Import JSON formatted structure builder template</li>
    <li>Export current structure builder steps to JSON template</li>
    <li>Add a new step toward building a new structure </li>
    <li>Execute the structure builder by running current steps</li>
   </ol>
  </p>
<p>Steps can be added to the structure builder template by clicking the icon [C]</p>
<img src="/StructureBuilder/Resources/Images/Image2.JPG" alt="Structure Builder Step"/>
<p>Each step has a number of different components
  <ol type="A">
    <li>Result: Id of the structure to be created.</li>
    <li>Base Structure: Id of the structure on which to perform the operation</li>
    <li>Operation: Boolean operation of margin operation to be performed.</li>
    <li>Margin or Target Structure: Allows the user to set the margin, or the structure on which the boolean operation should be performed</li>
   </ol>
  </p>
 <p>Some structure steps can be labelled as temporary.</p>
 <img src="/StructureBuilder/Resources/Images/Image3.JPG" alt="Temporary Structures"/>
 <p><b>Temporary Structures</b> are those that should not be saved, and should not be visible in the Treatment Planning System, but they are necessary for generating the expected structure. </p>
 <p>Once the <b>Run all steps</b> button is pressed, the user is prompted to save changes <img src="/StructureBuilder/Resources/Images/Image4.JPG" alt="Save Structures"/> The user can review their contours. Remember to review all contours for completeness.</p>
 <img src="/StructureBuilder/Resources/Images/Image5.JPG" alt="New contours"/>
 <p><b>Note: By default, the Simple Structure Builder will not overwrite structures that have DICOM types of PTV, GTV, CTV, or ORGAN. This can be changed in the configuration, but the tool is not meant to overwrite hand-drawn OARs. Please take care to make sure not to overwrite anyone else's work.</b></p>
 <h2>Running Structure Builder from Source Code</h2>
 <p>As a stand-alone executable, the source code will run best against a research system, where Eclipse and Visual Studio can co-exist. Clone the code from github. For instructions on how to clone the code please see <a href="https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository">instructions for cloning a repository</a>. The code requires an injected structureset to run. The developer can choose the structure set to be loaded into the application</p>
 <p>First, go to the StructureBuilder properties</p>
 <img src="/StructureBuilder/Resources/Images/Image6.JPG" alt="Application properties"/>
 <p>In the <b>Debug</b> section of the properties, the patient and structure set information can be injected into the <b>Command line arguments</b>. The order for this argument is "PatientId;ImageId;StructureSetId"</p>
 <img src="/StructureBuilder/Resources/Images/Image7.JPG" alt="Command line arguments"/>
 <h2>Running Structure Builder from Download.</h2>
