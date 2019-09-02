using Xbim.Common;

// todo: we need to decide if the namespace Xbim.MvdXml.DataManagement makes sense
// todo: and how it does relate to Xbim.MvdXml.Validation
namespace Xbim.MvdXml.DataManagement
{
    internal delegate void ClearCacheHandler();

    // Todo: Before release document removing warnings

    /// <summary>
    /// Use with caution
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="args"></param>
    public delegate void EntityProcessingHandler(MvdEngine engine, EntityProcessingEventArgs args);

    public class EntityProcessingEventArgs
    {
        public enum ProcessingEvent
        {
            ExtractRulesValues,
            ProcessRuleTree
        }

        public ProcessingEvent EventType;
        public IPersistEntity Entity;
 
        public EntityProcessingEventArgs(IPersistEntity entity, ProcessingEvent eventType)
        {
            Entity = entity;
            EventType = eventType;
        }
    }


    /// <summary>
    /// MvdEngine manages the execution of MvdXml logic on models.
    /// </summary>
    public partial class MvdEngine
    {
        /// <summary>
        /// Elements in the the mvd tree will subscribe to this in order to be notified when a ClearCache event is requested.
        /// </summary>
        internal event ClearCacheHandler RequestClearCache;

        public event EntityProcessingHandler OnProcessing;
    }
}
