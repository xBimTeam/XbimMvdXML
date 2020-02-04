using System;
using System.Windows.Media;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;


namespace XbimPlugin.MvdXML.Viewing
{
    public class ReportResult
    {
        public Concept Concept;
        public ModelViewExchangeRequirement Requirement;
        public IPersistEntity Entity;
        public ConceptTestResult TestResult;

        public ReportResult(Concept cpt, IPersistEntity selectedEntity, ConceptTestResult result, ModelViewExchangeRequirement requirement)
        {
            Concept = cpt;
            Entity = selectedEntity;
            TestResult = result;
            Requirement = requirement;
        }

        public string EntityLabel => $"#{Entity.EntityLabel}";

        public string EntityDesc
        {
            get
            {
                var asRoot = Entity as IIfcRoot;
                if (asRoot == null)
                    return Entity.ToString();
                return !string.IsNullOrEmpty(asRoot.Name) 
                    ? $"{asRoot.Name} [#{asRoot.EntityLabel}]" 
                    : $"{asRoot.ExpressType.Name} [#{asRoot.EntityLabel}]";
            }
        }

        public string InvolvedRequirement => Requirement != null 
            ? Requirement.name 
            : "undefined";

        public string ResultSummary
        {
            get
            {
                switch (TestResult)
                {
                    case ConceptTestResult.Pass:
                        return "Passed";
                    case ConceptTestResult.Fail:
                        return "Not Passed";
                    case ConceptTestResult.DoesNotApply:
                        return "Not applicable";
                    default:
                        return TestResult.ToString();
                }
            }
        }

        public string ConceptName => Concept.name;

        public Brush CircleBrush
        {
            get
            {
                switch (TestResult)
                {
                    case ConceptTestResult.Pass:
                        return Brushes.Green;
                    case ConceptTestResult.DoesNotApply:
                        return Brushes.Blue;
                    case ConceptTestResult.Warning:
                        return Brushes.Orange;
                    case ConceptTestResult.Fail:
                        return Brushes.Red;
                    default:
                        return Brushes.Black;
                }                
            }
        }
    }
}
