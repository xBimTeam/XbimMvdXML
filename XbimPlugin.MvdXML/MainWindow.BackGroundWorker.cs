using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;

namespace XbimPlugin.MvdXML
{
    public partial class MainWindow
    {
        BackgroundWorker _backgroundTester = new BackgroundWorker();

        private bool _backgroundScheduleRestart;

        private void RequestUpdateReport()
        {
            if (_backgroundTester.IsBusy)
            {
                WorkerRequestCancel();
                _backgroundScheduleRestart = true;
            }
            else
            {
                WorkerStart();
            }
        }

        private void ToggleUpdate(object sender, RoutedEventArgs e)
        {
            if (_backgroundTester.IsBusy)
                WorkerRequestCancel();
            else
                WorkerStart();            
        }

        private void WorkerRequestCancel()
        {
            _backgroundTester.CancelAsync();
            ToggleActivityButton.IsEnabled = false;
        }

        // todo: find a way to update the UI fluidly
        //
/*
        private static void ProcessUiTasks()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(
                delegate
                {
                    frame.Continue = false;
                    return null;
                }), null);
            Dispatcher.PushFrame(frame);
        }
*/

        private void WorkerStart()
        {
            TestResults.Clear();
            _backgroundScheduleRestart = false;
            if (Model?.ModelFactors == null)
                return;
            
            var selectedConcepts = SelectedConcepts();
            var selectedExchReq = SelectedExchangeRequirements();
            var selectedIfcClasses = SelectedIfcClasses();
            if (!selectedConcepts.Any() && !selectedExchReq.Any() && !selectedIfcClasses.Any())
                return;
            // define entitites list
            var entities = _xpWindow.DrawingControl.Selection.ToList();
            if (!entities.Any())
            {
                entities = Model?.Instances?.ToList();
            }

            ToggleActivityButton.Content = "Cancel";
            ToggleActivityButton.Background = Brushes.Orange;
            // ProcessUiTasks();
            
            _backgroundTester = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _backgroundTester.DoWork += (obj, ea) => GetReport(obj, ea, selectedConcepts, selectedExchReq, selectedIfcClasses, entities, Doc);
            _backgroundTester.ProgressChanged += bw_ProgressChanged;
            _backgroundTester.RunWorkerCompleted += bw_RunWorkerCompleted;
            _backgroundTester.RunWorkerAsync();
        }

        private void WorkerEnsureStop()
        {
            if (!_backgroundTester.IsBusy) 
                return;
            WorkerRequestCancel();
            _backgroundScheduleRestart = false;
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ToggleActivityButton.Content = "Update results";
            ToggleActivityButton.Background = Brushes.DarkGray;
            TestProgress.Value = 0;
            ToggleActivityButton.IsEnabled = true;
            if (_backgroundScheduleRestart)
                WorkerStart();
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TestProgress.Value = e.ProgressPercentage;
        }
        
        // todo: this needs to be reviewed, while trying to ensure a responsive UI.
        // private void GetReport(object sender, DoWorkEventArgs e)
        private void GetReport(object sender, 
            // ReSharper disable once UnusedParameter.Local
            DoWorkEventArgs ea, 
            HashSet<Concept> selectedConcepts, 
            HashSet<ModelViewExchangeRequirement> selectedExchReq, 
            HashSet<ExpressType> selectedIfcClasses, 
            List<IPersistEntity> entities, 
            MvdEngine doc)
        {
            var bw = sender as BackgroundWorker;
            if (bw == null)
                return;
            // var report = new List<ReportResult>();
            var report = TestResults;

            var entitiesInQueue = entities.Count;
            double itemsDone = 0;
            var lastReported = 0;
            
            
            foreach (var entity in entities)
            {
                var thisEntityExpressType = entity.ExpressType; 

                if (selectedIfcClasses.Any())
                {
                    // if no one of the selected classes contains the element type in the subtypes skip entity
                    var needTest = selectedIfcClasses.Any(classesToTest => classesToTest == thisEntityExpressType || classesToTest.SubTypes.Contains(thisEntityExpressType));
                    if (!needTest)
                        continue;
                }
                var todo = new List<RequirementsRequirement>();
                var suitableRoots = doc.GetConceptRoots(thisEntityExpressType);
                foreach (var suitableRoot in suitableRoots)
                {
                    if (suitableRoot.Concepts == null)
                        continue;
                    foreach (var concept in suitableRoot.Concepts)
                    {
                        if (bw.CancellationPending)
                            return;
                        if (concept.Requirements == null)
                            continue;
                        if (selectedConcepts.Any())
                        {
                            if (!selectedConcepts.Contains(concept))
                                continue; // out of concept loop
                        }
                        foreach (var requirementsRequirement in concept.Requirements)
                        {
                            if (selectedExchReq.Any())
                            {
                                if (!selectedExchReq.Contains(requirementsRequirement.GetExchangeRequirement()))
                                    continue; // out of requirementsRequirement loop
                            }
                            todo.Add(requirementsRequirement);
                        }
                    }
                }
                var queueEstimate = --entitiesInQueue * 10 + todo.Count; // assumed 10 req per element on average

                foreach (var requirementsRequirement in todo)
                {
                    itemsDone++;
                    var thisProgress = Convert.ToInt32(itemsDone/queueEstimate*100);
                    if (lastReported != thisProgress)
                    {
                        _backgroundTester.ReportProgress(thisProgress);
                        lastReported = thisProgress;
                    }

                    if (bw.CancellationPending)
                        return;
                    var result = ReportRequirementRequirement(requirementsRequirement, entity);
                    if (!ConfigShowResult(result))
                        continue;
                    AddOnUi(report, result);
                }
            }
        }

        private static void AddOnUi<T>(ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }
    }
}
