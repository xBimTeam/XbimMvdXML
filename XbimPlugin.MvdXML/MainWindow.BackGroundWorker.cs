using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;
using XbimPlugin.MvdXML.Viewing;
using Microsoft.Extensions.Logging;

// todo: there's scope for moving/refactoring this class to the mvdxml dll, to establish some model validation 
// and reporting functions

namespace XbimPlugin.MvdXML
{
    public partial class MainWindow: IDisposable
    {
        private BackgroundWorker _backgroundTester = new BackgroundWorker();

        private bool _backgroundScheduleRestart;

        private void RequestUpdateReport()
        {
            if (_backgroundTester != null && _backgroundTester.IsBusy)
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
            if (_backgroundTester == null || !_backgroundTester.IsBusy)
                WorkerStart();
            else
                WorkerRequestCancel();
        }

        private void WorkerRequestCancel()
        {
            _backgroundTester?.CancelAsync();
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
            
            _backgroundTester.DoWork +=
                (obj, ea) => PerformReport(selectedConcepts, selectedExchReq, selectedIfcClasses, entities, Doc, obj, ea, TestResults, _backgroundTester, _validShowResults);
            _backgroundTester.ProgressChanged += bw_ProgressChanged;
            _backgroundTester.RunWorkerCompleted += bw_RunWorkerCompleted;
            _backgroundTester.RunWorkerAsync();
        }

      

        private void WorkerEnsureStop()
        {
            _backgroundScheduleRestart = false;
            if (_backgroundTester != null)
            {
                if (!_backgroundTester.IsBusy)
                    return;
                WorkerRequestCancel();
            }
            _backgroundScheduleRestart = false;
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ToggleActivityButton.Content = "Update results";
            ToggleActivityButton.Background = Brushes.DarkGray;
            TestProgress.Value = 0;
            ToggleActivityButton.IsEnabled = true;

            _backgroundTester?.Dispose();
            _backgroundTester = null;
            if (_backgroundScheduleRestart)
                WorkerStart();
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TestProgress.Value = e.ProgressPercentage;
        }

        // todo: this needs to be reviewed, while trying to ensure a responsive UI.
        // private void GetReport(object sender, DoWorkEventArgs e)
        private void PerformReport(HashSet<Concept> selectedConcepts,
            HashSet<ModelViewExchangeRequirement> selectedExchReq,
            HashSet<ExpressType> selectedIfcClasses,
            List<IPersistEntity> entities,
            MvdEngine doc,
            object sender = null,
            // ReSharper disable once UnusedParameter.Local
            DoWorkEventArgs ea = null,
            ObservableCollection<ReportResult> destinationResultCollection = null,
            BackgroundWorker reportingWorker = null,
            HashSet<ConceptTestResult> requiredReportingSet = null
            )
        {
            var bw = sender as BackgroundWorker;
            // var report = new List<ReportResult>();
            

            var entitiesInQueue = entities.Count;
            double itemsDone = 0;
            var lastReported = 0;


            foreach (var entity in entities)
            {
                var thisEntityExpressType = entity.ExpressType;

                if (selectedIfcClasses.Any())
                {
                    // if no one of the selected classes contains the element type in the subtypes skip entity
                    var needTest =
                        selectedIfcClasses.Any(
                            classesToTest =>
                                classesToTest == thisEntityExpressType ||
                                classesToTest.NonAbstractSubTypes.Contains(thisEntityExpressType));
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
                        if (bw != null && bw.CancellationPending)
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
                var queueEstimate = --entitiesInQueue*10 + todo.Count; // assumed 10 req per element on average
                try
                {
                    foreach (var requirementsRequirement in todo)
                    {
                        itemsDone++;
                        var thisProgress = Convert.ToInt32(itemsDone/queueEstimate*100);
                        if (lastReported != thisProgress)
                        {
                            reportingWorker?.ReportProgress(thisProgress);
                            lastReported = thisProgress;
                        }

                        if (bw != null && bw.CancellationPending)
                            return;
                        var result = ReportRequirementRequirement(requirementsRequirement, entity);

                        if ( destinationResultCollection == null)
                            continue;
                        if (!ConfigShowResult(requiredReportingSet, result))
                            continue;
                        AddOnUi(destinationResultCollection, result);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error processing entity #{entity.EntityLabel}.", ex);
                }
            }
        }

        private static void AddOnUi<T>(ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _backgroundTester.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MainWindow()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
