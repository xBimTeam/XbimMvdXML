using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace XbimPlugin.MvdXML.Viewing
{
    public class MyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StateTemplate
        { get; set; }
        public DataTemplate CountyTemplate
        { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var cp = container as ContentPresenter;
            if (cp == null) 
                return base.SelectTemplate(item, container);
            var cvg = cp.Content as CollectionViewGroup;
            if (cvg == null)
                return null;

            if (cvg.Items.Count <= 0) 
                return base.SelectTemplate(item, container);
            var stinfo = cvg.Items[0] as ReportResult;

            return stinfo != null 
                ? CountyTemplate 
                : StateTemplate;
        }
    }
}
