using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Win32;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;
using Xbim.Presentation;
using Xbim.Presentation.XplorerPluginSystem;
using XbimPlugin.MvdXML.Properties;
using XbimPlugin.MvdXML.Viewing;
using Xbim.Common;
using Xbim.Presentation.LayerStyling;
using Xbim.Ifc;
using Xbim.Common.Metadata;
using Xbim.MvdXml.Integrity;
using XbimPlugin.MvdXML.ModelExtraction;
using Microsoft.Extensions.Logging;

namespace XbimPlugin.MvdXML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [XplorerUiElement(PluginWindowUiContainerEnum.LayoutAnchorable, PluginWindowActivation.OnMenu, "MvdXML")]
    public partial class MainWindow : IXbimXplorerPluginWindow
    {
        private static readonly ILogger Log = Xbim.Common.XbimLogging.CreateLogger<MvdEngine>();

        public MainWindow()
        {
            InitializeComponent();
            IsFileOpen = false;
            ListResults.ItemsSource = TestResults;


            ChangeGrouping("Element");

            if (string.IsNullOrEmpty(Settings.Default.ColorFail)
                || string.IsNullOrEmpty(Settings.Default.ColorWarning)
                || string.IsNullOrEmpty(Settings.Default.ColorPass)
                || string.IsNullOrEmpty(Settings.Default.ColorNonApplicable)
            )
            {
                DefaultColors(null, null);
            }

            CmbColorGroup.SelectionChanged += ColorGroupChanged;

#if DEBUG
            // loads the last commands stored
            var fname = Path.Combine(Path.GetTempPath(), "mvdxmlcommands.txt");
            if (!System.IO.File.Exists(fname))
                return;
            using (var reader = System.IO.File.OpenText(fname))
            {
                var read = reader.ReadToEnd();
                TxtCommand.Text = read;
            }
#endif

        }

        internal MvdEngine Doc;

        public string SelectedPath { get; set; }

        private bool AdaptSchema => ChkAdaptSchema.IsChecked.HasValue && ChkAdaptSchema.IsChecked.Value;

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog { Filter = @"mvdXML|*.mvdXML;*.xml" };
            var res = openFile.ShowDialog();

            if (!res.HasValue || res.Value != true) 
                return;

            using (new WaitCursor())
            {
                mvdXML mvd = null;
                try
                {
                    var comp = mvdXML.TestCompatibility(openFile.FileName);
                    if (comp == mvdXML.CompatibilityResult.InvalidNameSpace)
                    {
                        var newName = Path.GetTempFileName();
                        if (mvdXML.FixNamespace(openFile.FileName, newName))
                        {
                            mvd = mvdXML.LoadFromFile(newName);
                        }
                        else
                        {
                            var msg = $"Attempt to fix namespace in invalid xml file [{openFile.FileName}] failed.";
                            NotifyError(msg, null);
                        }
                    }
                    else
                    {
                        mvd = mvdXML.LoadFromFile(openFile.FileName);
                    }

                }
                catch (Exception ex)
                {
                    var msg = $"Invalid xml file [{openFile.FileName}].";
                    NotifyError(msg, ex);
                }
                if (mvd == null)
                    return;

                try
                {
                    Doc = new MvdEngine(mvd, Model, AdaptSchema);
                }
                catch (Exception ex)
                {
                    var msg = $"Error creating engine from valid mvdXML [{openFile.FileName}].";
                    NotifyError(msg, ex);
                }
                if (Doc == null)
                    return;
                UpdateUiLists();

                IsFileOpen = true;

                // the following are going to be inverted by the call to UnMatchedToggle and WarnToggle straight after
                _useAmber = false;
                _useBlue = false;
                UnMatchedToggle(null, null);
                WarnToggle(null, null);

                ColorGroupChanged(null, null);
            }
        }

        private void UpdateUiLists()
        {
            UpdateRootsTree();
            UpdateIfcClassesTree();
            UpdateErTree();
            SelectedConcept.ItemsSource = Doc.GetAllConcepts();
            UpdateDataTableSource();
        }

        private static void NotifyError(string msg, Exception ex)
        {
            // log directly; ex is changed by the loop below.
            Log.LogError(msg, ex);

            // attempt to produce a richer UI feedback.
            var sb = new StringBuilder();
            sb.AppendLine(msg);
            while (ex != null)
            {
                sb.AppendLine(ex.Message);
                ex = ex.InnerException;
            }
            MessageBox.Show(sb.ToString(), "Problem opening xml file", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool IsFileOpen
        {
            get
            {
                return (Doc != null);
            }
            set
            {
                if (value)
                {
                    Browse.Visibility = Visibility.Visible;
                    Visuals.Visibility = Visibility.Visible;
                    Commands.Visibility = Visibility.Visible;
                    File.Visibility = Visibility.Collapsed;

                    Browse.Focus();
                }
                else
                {
                    Browse.Visibility = Visibility.Collapsed;
                    Visuals.Visibility = Visibility.Collapsed;
                    Commands.Visibility = Visibility.Collapsed;

                    File.Visibility = Visibility.Visible;
                    File.Focus();
                }
                // PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OpenButtonVisibility"));
                // PropertyChanged.Invoke(this, new PropertyChangedEventArgs("UIVisibility"));
            }
        }

        public Visibility OpenButtonVisibility => (IsFileOpen) ? Visibility.Hidden : Visibility.Visible;
        public Visibility UiVisibility => (!IsFileOpen) ? Visibility.Hidden : Visibility.Visible;

        private IXbimXplorerPluginMasterWindow _xpWindow;


        public void BindUi(IXbimXplorerPluginMasterWindow mainWindow)
        {
            _xpWindow = mainWindow;
            SetBinding(SelectedItemProperty, new Binding("SelectedItem") { Source = mainWindow, Mode = BindingMode.TwoWay });
            SetBinding(SelectionProperty, new Binding("Selection") { Source = mainWindow.DrawingControl, Mode = BindingMode.TwoWay });
            SetBinding(ModelProperty, new Binding()); // whole datacontext binding, see http://stackoverflow.com/questions/8343928/how-can-i-create-a-binding-in-code-behind-that-doesnt-specify-a-path
            
            // versioning information
            //
            var assembly = Assembly.GetAssembly(typeof(MvdEngine));
            PluginVersion.Text = $"Assembly Version: {assembly.GetName().Version}";
            if (_xpWindow == null)
                return;
            var location = _xpWindow.GetAssemblyLocation(assembly);
            var fvi = FileVersionInfo.GetVersionInfo(location);
            PluginVersion.Text += $"\r\nFile Version: {fvi.FileVersion}";
        }

        // Selection
        public EntitySelection Selection
        {
            get { return (EntitySelection)GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        public static DependencyProperty SelectionProperty =
            DependencyProperty.Register("Selection", typeof(EntitySelection), typeof(MainWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnPropertyChanged));

        // SelectedEntity
        public IPersistEntity SelectedEntity
        {
            get { return (IPersistEntity)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedEntity", typeof(IPersistEntity), typeof(MainWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                                                                      OnPropertyChanged));

        // Model
        public IModel Model
        {
            get { return (IModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(IModel), typeof(MainWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                                                                      OnPropertyChanged));


        private bool _suspendReportUpdate;

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // if any UI event should happen it needs to be specified here
            var window = d as MainWindow;
            if (window == null) 
                return;
            
            switch (e.Property.Name)
            {
                case "Selection":
                    // Debug.WriteLine(e.Property.Name + @" changed");
                    window.RefreshReport();
                    break;
                case "Model":
                    // Debug.WriteLine(e.Property.Name + @" changed");
                    window.WorkerEnsureStop();
                    if (window.Doc != null)
                    {
                        window.Doc.ClearCache();
                        if (window.AdaptSchema)
                        {
                            window.Doc.FixReferences();
                            window.UpdateUiLists();
                        }
                    }
                    break;
                case "SelectedEntity":
                    window.RefreshReport();
                    break;
            }
        }

        private void RefreshReport()
        {
            if (Doc == null)
                return;
            UpdateReport();
            UpdateDataTable();
        }

        private void UpdateIfcClassesTree()
        {
            IfcClassesTree.Items.Clear();
            var allClasses = new ObjectViewModel() {Header = "All classes", IsChecked = true };
            var types = Doc.GetExpressTypes();
            foreach (var tp in types)
            {
                allClasses.AddChild(new ObjectViewModel() { Header = tp.Type.FullName, Tag = tp, IsChecked = true });
            }           
            IfcClassesTree.Items.Add(allClasses);
        }

        private void UpdateErTree()
        {
            ErTree.Items.Clear();           
            if (Doc == null)
                return;
            foreach (var view in Doc.Mvd.Views)
            {
                var v = new ObjectViewModel
                {
                    Header = view.name,
                    Tag = new ExchangeRequirementViewExpander(view),
                    IsChecked = true
                };
                ErTree.Items.Add(v);
                
            }
        }

        private void UpdateRootsTree()
        {
            ConceptRootsTree.Items.Clear();
            if (Doc == null)
                return;
            foreach (var view in Doc.Mvd.Views)
            {
                ConceptRootsTree.Items.Add(new ObjectViewModel() { Header = view.name, Tag = new ConceptRootsViewExpander(view), IsChecked = true});
            }
        }
        
        public string WindowTitle => "mvdXML 1.1";

        private ILayerStyler _prevStyler;

        private void TrafficLight(object sender, RoutedEventArgs e)
        {
            if (_prevStyler == null)
                _prevStyler = _xpWindow.DrawingControl.DefaultLayerStyler;
            var ls2 = new TrafficLightStyler((IfcStore)Model, this)
            {
                UseAmber = _useAmber,
                UseBlue =  _useBlue
            };
            ls2.SetColors(
                XbimColour.FromString(Settings.Default.ColorPass),
                XbimColour.FromString(Settings.Default.ColorFail),
                XbimColour.FromString(Settings.Default.ColorWarning),
                XbimColour.FromString(Settings.Default.ColorNonApplicable)
                );
            
            ls2.SetFilters(
                SelectedConcepts(),
                SelectedExchangeRequirements(),
                SelectedIfcClasses()
                );

            _xpWindow.DrawingControl.DefaultLayerStyler = ls2;
            
            // then reload
            _xpWindow.DrawingControl.ReloadModel(DrawingControl3D.ModelRefreshOptions.ViewPreserveAll);
        }

        private void CloseFile(object sender, RoutedEventArgs e)
        {
            Doc = null;
            IfcClassesTree.ItemsSource = null;
            UpdateDataTable();
            ErTree.ItemsSource = null;
            ConceptRootsTree.ItemsSource = null;
            // ListResults.ItemsSource = null;            
            TestResults.Clear();
            IsFileOpen = false;
        }

        private void AddComment(object sender, RoutedEventArgs e)
        {
            var verbalStatus = "Information";

            var lst = new List<ReportResult>();
            var comment = "";
            foreach (var item in ListResults.SelectedItems)
            {
                var res = item as ReportResult;
                if (res == null) 
                    continue;
                lst.Add(res);
                comment +=
                    $"Entity {res.EntityDesc} ({res.Entity.EntityLabel}) {res.TestResult} request {res.ConceptName}\r\n\r\n";
                if (res.TestResult == ConceptTestResult.Fail)
                    verbalStatus = "Error";
            }
            
            if (!lst.Any()) 
                return;
            
            var messageData = new Dictionary<string, object>
            {
                {"InstanceTitle", "mvdXML validation message"},
                {"CommentVerbalStatus", verbalStatus},
                {"CommentAuthor", "CB"},
                {"CommentText", comment},
                {"DestinationEmail", "claudio.benghi@northumbria.ac.uk"}
            };
            _xpWindow.BroadCastMessage(this, "BcfAddInstance", messageData);
        }

        private bool _useAmber = true;

        private void WarnToggle(object sender, MouseButtonEventArgs e)
        {
            Warnings.Fill = _useAmber ? Brushes.Transparent : Brushes.Orange;
            _useAmber = !_useAmber;
        }

        private bool _useBlue = true;

        private void UnMatchedToggle(object sender, MouseButtonEventArgs e)
        {
            UnMatched.Fill = _useBlue ? Brushes.Transparent : Brushes.LightSkyBlue;
            _useBlue = !_useBlue;
        }
        
        private void UpdateDataTable()
        {
            var getD = SelectedEntity != null;

            var dataTableSourceConcept = SelectedConcept.SelectedItem as Concept;
            var dataTableSourceConceptTemplate = SelectedConcept.SelectedItem as ConceptTemplate;
            
            if (Doc == null)
                getD = false;
            if (dataTableSourceConcept == null && dataTableSourceConceptTemplate == null)
                getD = false;

            if (getD)
            {
                DataTable data = null;
                // set new table
                if (dataTableSourceConcept != null)
                {
                    data = Doc.GetData(SelectedEntity, dataTableSourceConcept);
                }
                if (dataTableSourceConceptTemplate != null)
                {
                    data = Doc.GetData(SelectedEntity, dataTableSourceConceptTemplate);
                }
                SelectedConceptData.AutoGenerateColumns = true;

                // prepare view to block add/delete
                var v = data.DefaultView;
                v.AllowNew = false;
                v.AllowDelete = false;

                SelectedConceptData.ItemsSource = v;
            }
            else
            {
                // empty table display
                SelectedConceptData.AutoGenerateColumns = true;
                SelectedConceptData.ItemsSource = null;
            }
        }

        private void ShowUnderscores(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = e.Column.Header.ToString();

            // this allows to bind correctly to properties with dots in the name
            // more at: http://stackoverflow.com/questions/25017542/how-to-prevent-columns-from-not-displaying-in-wpf-datagrids-when-using-in-th
            // 
            if (e.PropertyName.Contains('.') && e.Column is DataGridBoundColumn)
            {
                var col = e.Column as DataGridBoundColumn;
                col.Binding = new Binding(string.Format("[{0}]", e.PropertyName));
            }

            // Replace all underscores with two underscores, to prevent AccessKey handling
            e.Column.Header = header.Replace("_", "__");
        }

#region "Checked items"

        private static IEnumerable<Concept> SelectedConcepts(ObjectViewModel item)
        {
            if (item.IsChecked.HasValue && item.IsChecked.Value == false)
                yield break;
            
            var v = item.Tag as Concept;
            if (v != null)
                yield return v;
            foreach (var childItem in item.Children.SelectMany(SelectedConcepts))
                yield return childItem;
        }

        private HashSet<Concept> SelectedConcepts()
        {
            var ret = new HashSet<Concept>();
            var startnode = ConceptRootsTree.Items[0] as ObjectViewModel;
            if (startnode == null)
                return ret;
            foreach (var val in SelectedConcepts(startnode).Where(val => !ret.Contains(val)))
                ret.Add(val);
            
            return ret;
        }

        private static IEnumerable<ModelViewExchangeRequirement> SelectedExchangeRequirements(ObjectViewModel item)
        {
            if (item.IsChecked.HasValue && item.IsChecked.Value == false)
                yield break;
            
            var v = item.Tag as ModelViewExchangeRequirement;
            if (v != null)
                yield return v;
            foreach (var childItem in item.Children.SelectMany(SelectedExchangeRequirements))
                yield return childItem;
        }

        private HashSet<ModelViewExchangeRequirement> SelectedExchangeRequirements()
        {
            var ret = new HashSet<ModelViewExchangeRequirement>();
            var startnode = ErTree.Items[0] as ObjectViewModel;
            if (startnode == null)
                return ret;
            foreach (var val in SelectedExchangeRequirements(startnode).Where(val => !ret.Contains(val)))
                ret.Add(val);

            return ret;
        }

        private static IEnumerable<ExpressType> SelectedIfcClasses(ObjectViewModel item)
        {
            if (item.IsChecked.HasValue && item.IsChecked.Value == false)
                yield break;

            var v = item.Tag as ExpressType;
            if (v != null)
                yield return v;
            foreach (var childItem in item.Children.SelectMany(SelectedIfcClasses))
                yield return childItem;
        }

        private HashSet<ExpressType> SelectedIfcClasses()
        {
            var ret = new HashSet<ExpressType>();
            var startnode = IfcClassesTree.Items[0] as ObjectViewModel;
            if (startnode == null)
                return ret;
            foreach (var val in SelectedIfcClasses(startnode).Where(val => !ret.Contains(val)))
                ret.Add(val);

            return ret;
        }

#endregion
        
        
        private void SetReportRequest(object sender, RoutedEventArgs e)
        {
            UpdateReport();
        }

        internal ObservableCollection<ReportResult> TestResults = new ObservableCollection<ReportResult>();

        private void UpdateReport()
        {
            if (_suspendReportUpdate)
                return;
            ReportTextBox.Text = "";
            RequestUpdateReport();
        }

        
        // ReSharper disable once UnusedMember.Local
        private IEnumerable<ReportResult> ReportType(ExpressType iType)
        {
            if (Model == null)
                yield break;
            var suitableRoots = Doc.GetConceptRoots(iType);
            {
                var entititesOfType = Model.Instances.OfType(iType.Name, false);
                foreach (var ent in entititesOfType)
                {
                    foreach (var validRoot in suitableRoots)
                    {
                        foreach (var cpt in validRoot.Concepts)
                        {
                            var rep = ReportConcept(cpt, ent);
                            if (!ConfigShowResult(_validShowResults, rep))
                                continue;
                            yield return rep;
                        }
                    }
                }
            }
        }
        
        private static ReportResult ReportRequirementRequirement(RequirementsRequirement requirementsRequirement, IPersistEntity entity)
        {
            var testResult = requirementsRequirement.Test(entity);
            
            // todo: restore text report
            //ReportTextBox.Text += string.Format("RequirementRequirement test on #{2}; result: {3}\r\n" +
            //                                    "  Concept: {0} ({1})\r\n" +
            //                                    "  ER:      {4} ({5})\r\n" +
            //                                    "\r\n",
            //                                    requirementsRequirement.ParentConcept.name, 
            //                                    requirementsRequirement.ParentConcept.uuid, 
            //                                    entity.EntityLabel, 
            //                                    testResult, 
            //                                    requirementsRequirement.GetExchangeRequirement().name, 
            //                                    requirementsRequirement.exchangeRequirement);

            return new ReportResult(requirementsRequirement.ParentConcept, entity, testResult, requirementsRequirement.GetExchangeRequirement() );
        }
        
        // ReSharper disable once UnusedMember.Local
        private IEnumerable<ReportResult> ReportConceptRoot(ConceptRoot croot, IPersistEntity entity)
        {
            ReportTextBox.Text += $"ConceptRoot {croot.name} ({croot.uuid}) on #{entity.EntityLabel}\r\n";
            foreach (var cpt2 in croot.Concepts)
            {
                var rep = ReportConcept(cpt2, entity);
                if (!ConfigShowResult(_validShowResults, rep))
                    continue;
                yield return rep;
            }
        }

        private ReportResult ReportConcept(Concept cpt, IPersistEntity entity)
        {
            var testResult = cpt.Test(entity, Concept.ConceptTestMode.ThroughRequirementRequirements);
            // todo: this is really tested on several requirements, they should probably be all reported individually
            var ret = new ReportResult(cpt, entity, testResult, null);
            ReportTextBox.Text += $"Concept {cpt.name} ({cpt.uuid}) on #{entity.EntityLabel}: {testResult}\r\n";
            return ret;
        }

        private void ShowElement(object sender, MouseButtonEventArgs e)
        {
            var snd = sender as ListViewItem;
            var repRes = snd?.DataContext as ReportResult;
            if (repRes?.Entity == null)
                return;
            
            ConceptsFilter.Text = repRes.Concept.uuid;
            DataSourceIsConcepts.IsChecked = true;

            _suspendReportUpdate = true;
            _xpWindow.SelectedItem = repRes.Entity;
            _xpWindow.DrawingControl.ZoomSelected();
            _suspendReportUpdate = false;
            
        }

        private void ClearCache(object sender, RoutedEventArgs e)
        {
            Doc.ClearCache();
        }

        private void UpdateDataTableSource(object sender, RoutedEventArgs e)
        {
            UpdateDataTableSource();
        }

        private void FilterConcepts(object sender, TextChangedEventArgs e)
        {
            UpdateDataTableSource();
        }

        private void UpdateDataTableSource()
        {
            if (Doc == null)
                return;
            if (DataSourceIsConcepts.IsChecked.HasValue && DataSourceIsConcepts.IsChecked.Value)
            {
                SelectedConcept.ItemsSource = Doc.GetAllConcepts().Where(
                    x =>
                        CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.name, ConceptsFilter.Text,
                            CompareOptions.IgnoreCase) >= 0
                        ||
                        CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.uuid, ConceptsFilter.Text,
                            CompareOptions.IgnoreCase) >= 0
                    );
            }
            else
            {
                SelectedConcept.ItemsSource = Doc.GetAllConceptTemplates().Where(
                    x =>
                        CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.name, ConceptsFilter.Text,
                            CompareOptions.IgnoreCase) >= 0
                        ||
                        CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.uuid, ConceptsFilter.Text,
                            CompareOptions.IgnoreCase) >= 0
                    );
            }
        }

        private HashSet<ConceptTestResult> _validShowResults = new HashSet<ConceptTestResult>();

        private static bool ConfigShowResult(HashSet<ConceptTestResult> requiredSet, ReportResult result)
        {
            return requiredSet.Contains(result.TestResult);
        }

        private void ChangeShowFilter(object sender, SelectionChangedEventArgs e)
        {
            var snd = sender as ComboBox;
            var se = snd?.SelectedItem as ComboBoxItem;
            if (se == null)
                return;
            switch (se.Content.ToString())
            {
                case "Pass" :
                    _validShowResults = new HashSet<ConceptTestResult>() {
                        ConceptTestResult.Pass
                    };
                    break;
                case "Fail":
                    _validShowResults = new HashSet<ConceptTestResult>() {
                        ConceptTestResult.Fail
                    };
                    break;
                case "Warning":
                    _validShowResults = new HashSet<ConceptTestResult>() {
                        ConceptTestResult.Warning
                    };
                    break;
                case "Applicable":
                    _validShowResults = new HashSet<ConceptTestResult>() {
                        ConceptTestResult.Fail,
                        ConceptTestResult.Pass,
                        ConceptTestResult.Warning
                    };
                    break;
                // ReSharper disable once RedundantCaseLabel
                case "All":
                default:
                    _validShowResults = new HashSet<ConceptTestResult>() {
                        ConceptTestResult.Fail, 
                        ConceptTestResult.DoesNotApply,
                        ConceptTestResult.Pass,
                        ConceptTestResult.Warning
                    };
                    break;
            }
            if (Doc != null)
                UpdateReport();
        }

        private void ResetStyler(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void UpdateColor(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var c = new Color
            {
                R = (byte) SliderR.Value,
                G = (byte) SliderG.Value,
                B = (byte) SliderB.Value,
                A = (byte) SliderA.Value
            };
            var b = new SolidColorBrush(c);
            ColorDisplay.Fill = b;
        }

        private void ColorGroupChanged(object sender, SelectionChangedEventArgs e)
        {
            XbimColour col = null;
            var sel = CmbColorGroup.SelectedItem as ComboBoxItem;
            if (sel?.Tag == null)
                return;
            var tag = sel.Tag.ToString();
            switch ( tag )
            {
                case "F":
                    col = XbimColour.FromString(Settings.Default.ColorFail);
                    break;
                case "P":
                    col = XbimColour.FromString(Settings.Default.ColorPass);
                    break;
                case "W":
                    col = XbimColour.FromString(Settings.Default.ColorWarning);
                    break;
                case "N/A":
                    col = XbimColour.FromString(Settings.Default.ColorNonApplicable);
                    break;
            }
            if (col==null)
                return;
            try
            {
                SliderR.Value = col.Red * 255;
                SliderG.Value = col.Green * 255;
                SliderB.Value = col.Blue * 255;
                SliderA.Value = col.Alpha * 255;
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message, ex);
            }
        }
        
        private void SaveColors(object sender, RoutedEventArgs e)
        {
            var tmpCol = new XbimColour(
                "",
                SliderR.Value / 255,
                SliderG.Value / 255,
                SliderB.Value / 255,
                SliderA.Value / 255
                );

            var tag = ((ComboBoxItem)CmbColorGroup.SelectedItem).Tag.ToString();
            switch (tag)
            {
                case "F":
                    Settings.Default.ColorFail = tmpCol.ToString();
                    break;
                case "P":
                    Settings.Default.ColorPass = tmpCol.ToString();
                    break;
                case "W":
                    Settings.Default.ColorWarning = tmpCol.ToString();
                    break;
                case "N/A":
                    Settings.Default.ColorNonApplicable = tmpCol.ToString();
                    break;
            }
            Settings.Default.Save();
        }

        private void DefaultColors(object sender, RoutedEventArgs e)
        {
            Settings.Default.ColorFail = new XbimColour(@"Fail", 1, 0, 0, 0.5).ToString();
            Settings.Default.ColorWarning = new XbimColour(@"Warning", 1, 0.64, 0, 0.5).ToString();
            Settings.Default.ColorPass = new XbimColour(@"Pass", 0, 1, 0, 0.5).ToString();
            Settings.Default.ColorNonApplicable = new XbimColour(@"Non applicable", 0, 0, 1, 0.3).ToString();
            ColorGroupChanged(null, null);
        }
  
        private MemoryStream StreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        private void ChangeGrouping(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as ComboBox;
            var si = s?.SelectedItem as ComboBoxItem;
            if (si == null)
                return;
            ChangeGrouping(si.Content.ToString());
        }

        private void ChangeGrouping(string groupMode)
        {
            if (ListResults?.GroupStyle == null)
                return;

            // currently a single grouping is enabled, but more can be done looking at:
            // https://zamjad.wordpress.com/2011/05/15/applying-multiple-group-style-in-listview/

            string xamlItemView;
            PropertyGroupDescription groupDesc;

            //ReportResult t;
            //t.ConceptName;


            if (groupMode == "Element")
            {
                xamlItemView =
                    "<DataTemplate><WrapPanel><Ellipse Width=\"10\" Height=\"10\" Fill=\"{Binding CircleBrush}\" />" +
                    "<TextBlock Text=\"{Binding ConceptName}\" Margin=\"3,0\" />" +
                    "<TextBlock Text=\"{Binding ResultSummary}\" FontStyle=\"Italic\" Margin=\"3,0\" />" +
                    "<TextBlock Text=\"{Binding InvolvedRequirement}\" Margin=\"3,0\" />" +
                    "</WrapPanel></DataTemplate>";
                groupDesc = new PropertyGroupDescription("EntityDesc");
            }
            else if (groupMode == "Concept")
            {
                xamlItemView =
                    "<DataTemplate><WrapPanel><Ellipse Width=\"10\" Height=\"10\" Fill=\"{Binding CircleBrush}\" />" +
                    "<TextBlock Text=\"{Binding ResultSummary}\" FontStyle=\"Italic\" Margin=\"3,0\" />" +
                    "<TextBlock Text=\"on\" />" +
                    "<TextBlock Text=\"{Binding EntityDesc}\" Margin=\"3,0\" />" +
                     "<TextBlock Text=\"for\" />" +
                    "<TextBlock Text=\"{Binding InvolvedRequirement}\" Margin=\"3,0\" />" +
                    "</WrapPanel></DataTemplate>";
                groupDesc = new PropertyGroupDescription("ConceptName");
            }
            else if (groupMode == "Requirement")
            {
                xamlItemView =
                    "<DataTemplate><WrapPanel><Ellipse Width=\"10\" Height=\"10\" Fill=\"{Binding CircleBrush}\" />" +
                    "<TextBlock Text=\"{Binding ConceptName}\" Margin=\"3,0\" />" +
                    "<TextBlock Text=\"{Binding ResultSummary}\" FontStyle=\"Italic\" Margin=\"3,0\" />" +
                    "<TextBlock Text=\"on\" />" +
                    "<TextBlock Text=\"{Binding EntityDesc}\" Margin=\"3,0\" />" +
                    "</WrapPanel></DataTemplate>";
                groupDesc = new PropertyGroupDescription("InvolvedRequirement");
            }
            else
            {
                return;
            }
            // xaml side
            var context = new ParserContext();
            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            
            //var groupHeaderStyle = (GroupStyle) XamlReader.Load(StreamFromString(xamlGroupHeader), context);
            //ListResults.GroupStyle.Clear();
            //ListResults.GroupStyle.Add(groupHeaderStyle);

            var itemStyle = (DataTemplate)XamlReader.Load(StreamFromString(xamlItemView), context);
            ListResults.ItemTemplate = itemStyle;

            // grouping settings
            var collectionView = (CollectionView)CollectionViewSource.GetDefaultView(ListResults.ItemsSource);
            if (collectionView.GroupDescriptions == null)
                return;
            collectionView.GroupDescriptions.Clear();
            collectionView.GroupDescriptions.Add(groupDesc);

        }

        private void AdaptSchemaChanged(object sender, RoutedEventArgs e)
        {
            if (Doc == null)
                return;
            Doc.ForceModelSchema = AdaptSchema;
            UpdateUiLists();
        }

        private void SelectedRawDataConceptChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDataTable();
        }

        private void txtCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)))
                return;
            CommandExecute();
            e.Handled = true;
        }

        private void CommandExecute()
        {
#if DEBUG
            // stores the commands being launched
            var fname = Path.Combine(Path.GetTempPath(), "mvdxmlcommands.txt");
            using (var writer = System.IO.File.CreateText(fname))
            {
                writer.Write(TxtCommand.Text);
                writer.Flush();
                writer.Close();
            }
#endif

            TxtOut.Document = new FlowDocument();

            var commandArray = TxtCommand.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (TxtCommand.SelectedText != string.Empty)
                commandArray = TxtCommand.SelectedText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var cmdF in commandArray)
            {
                var th = new TextHighliter();

                th.Append("> " + cmdF, Brushes.ForestGreen);
                var cmd = cmdF;
                var i = cmd.IndexOf("//", StringComparison.Ordinal);
                if (i > 0)
                {
                    cmd = cmd.Substring(0, i);
                }
                if (cmd.TrimStart().StartsWith("//"))
                    continue;

                var commandMatch = Regex.Match(cmd, @"^Check *(?<mode>(all|uuid|variables))?$", RegexOptions.IgnoreCase);
                if (commandMatch.Success)
                {
                    var mode = commandMatch.Groups["mode"].Value.ToLowerInvariant();
                    var some = false;
                    if (mode == "all" || mode == "uuid")
                    {
                        th.Append("== Reporting uuid-issues", Brushes.DarkGreen);
                        int iCnt = 0;
                        foreach (var issue in Doc.Mvd.ReportUuidIssues())
                        {
                            th.Append(issue, Brushes.OrangeRed);
                            iCnt++;
                        }
                        if (iCnt == 0)
                            th.Append($"== No uuid-issues found.", Brushes.DarkGreen);
                        else if (iCnt == 1)
                            th.Append($"== 1 uuid-issue found.", Brushes.Red);
                        else
                            th.Append($"== {iCnt} uuid-issues found.", Brushes.Red);

                        some = true;
                    }
                    if (mode == "all" || mode == "variables")
                    {
                        th.Append("== Reporting variable-issues", Brushes.DarkGreen);
                        int iCnt = 0;
                        foreach (var issue in Doc.Mvd.ReportVariableNameIssues())
                        {
                            th.Append(issue, Brushes.OrangeRed);
                            iCnt++;
                        }
                        if (iCnt == 0)
                            th.Append($"== No variable-issues found.", Brushes.DarkGreen);
                        else if (iCnt == 1)
                            th.Append($"== 1 variable-issues found.", Brushes.Red);
                        else
                            th.Append($"== {iCnt} variable-issues found.", Brushes.Red);

                        some = true;
                    }
                    if (!some)
                    {
                        th.Append($"== Checkm command. Unknown mode \"{mode}\". Nothing done.", Brushes.Red);
                    }

                    th.DropInto(TxtOut.Document);
                    continue;
                }

                commandMatch = Regex.Match(cmd, @"^clear *(?<what>(cache))*$", RegexOptions.IgnoreCase);
                if (commandMatch.Success)
                {
                    var what = commandMatch.Groups["what"].Value.ToLowerInvariant();
                    if (what == "cache")
                    {
                        Doc.ClearCache();
                        th.Append($"== Cache cleared.", Brushes.DarkGreen);
                        th.DropInto(TxtOut.Document);
                        continue;
                    }
                    th.Append($"== Clear command. Unknown item \"{what}\". Nothing done.", Brushes.Red);
                    th.DropInto(TxtOut.Document);
                    continue;
                }

                commandMatch = Regex.Match(cmd, @"^strip *(?<what>(model))*$", RegexOptions.IgnoreCase);
                if (commandMatch.Success)
                {
                    var what = commandMatch.Groups["what"].Value.ToLowerInvariant();
                    if (what == "model")
                    {
                        th.Append("Attempting to extract model, this might take long.", Brushes.Blue);
                        th.DropInto(TxtOut.Document);
                        th = new TextHighliter();
                        // setup the processing id capture delegate
                        _identifiedItems = new ConcurrentDictionary<IPersistEntity, int>();
                        Doc.OnProcessing += DocOnOnProcessing;

                        // now execute the call
                        var selectedConcepts = SelectedConcepts();
                        var selectedExchReq = SelectedExchangeRequirements();
                        var selectedIfcClasses = SelectedIfcClasses();
                        if (!selectedConcepts.Any() && !selectedExchReq.Any() && !selectedIfcClasses.Any())
                            return;
                        var entities = _xpWindow.DrawingControl.Selection.ToList();
                        if (!entities.Any())
                        {
                            entities = Model?.Instances?.ToList();
                        }

                        // the function is launched with no backgroundworker
                        PerformReport(selectedConcepts, selectedExchReq, selectedIfcClasses, entities, Doc);

                        // remove the hook
                        Doc.OnProcessing -= DocOnOnProcessing;

                        XbimPlugin.MvdXML.ModelExtraction.Extractor.Extract(Model as IfcStore, new HashSet<IPersistEntity>(_identifiedItems.Keys));
                        th.Append($"Strip command completed, attempting to extract {_identifiedItems.Keys.Count} entities.", Brushes.Black);
                        th.DropInto(TxtOut.Document);
                        continue;
                    }
                    th.Append($"== Strip command. Unknown parameter \"{what}\". Nothing done.", Brushes.Red);
                    th.DropInto(TxtOut.Document);
                    continue;
                }

                commandMatch = Regex.Match(cmd, @"^help$", RegexOptions.IgnoreCase);
                if (commandMatch.Success)
                {
                    th.AppendFormat("Commands:");
                    th.AppendFormat("- Check <UUID|Variables|All>");
                    th.AppendFormat("- Strip <Model>");
                    th.AppendFormat("- Clear <Cache>");
                    th.DropInto(TxtOut.Document);
                    continue;
                }

                th.Append($"Commands not understood: \"{cmd}\".", Brushes.Red);
                th.DropInto(TxtOut.Document);
            }
        }

        private ConcurrentDictionary<IPersistEntity, int> _identifiedItems;

        private void DocOnOnProcessing(MvdEngine engine, EntityProcessingEventArgs args)
        {
            if (args.EventType != EntityProcessingEventArgs.ProcessingEvent.ProcessRuleTree)
                return;

            _identifiedItems.AddOrUpdate(
                args.Entity,
                entity => 1,
                (entity, i) => i + 1);
        }

        private void cmdRun(object sender, RoutedEventArgs e)
        {
            CommandExecute();
        }
    }
}
