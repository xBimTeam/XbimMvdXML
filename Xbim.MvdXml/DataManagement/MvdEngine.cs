using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Xbim.Common;
using Xbim.Common.Metadata;

[assembly: InternalsVisibleTo("Tests")]

// todo: we need to decide if the namespace Xbim.MvdXml.DataManagement makes sense
// todo: and how it does relate to Xbim.MvdXml.Validation
namespace Xbim.MvdXml.DataManagement
{
/// <summary>
    /// MvdEngine manages the execution of MvdXml logic on models.
    /// </summary>
    public partial class MvdEngine
    {
        private static readonly ILogger Log = Xbim.Common.XbimLogging.CreateLogger<MvdEngine>();

        // todo: there's probably scope for removing the model from the mvdEngine
        // we could have a manager to resolve references and UUIDs within the mvdxml schema
        // and another class to deal with the caching of results attached with a model

        private readonly IModel _model;

        /// <summary>
        /// Removes all cacher results from the validation
        /// </summary>
        public void ClearCache()
        {
            _failedLookupMessages = new HashSet<string>();
            RequestClearCache?.Invoke();
        }
        
        /// <summary>
        /// The underlying validation structure.
        /// </summary>
        public mvdXML Mvd { get; }

        private bool _forceModelSchema;

        /// <summary>
        /// Determine if the engine should treat the MvdXML type specifications strictly or use the model schema instead.
        /// </summary>
        public bool ForceModelSchema
        {
            get { return _forceModelSchema; }
            set
            {
                if (_forceModelSchema == value)
                    return;
                _forceModelSchema = value;
                ClearCache();
                FixReferences();
            }
        }

        private Dictionary<ExpressType, List<ConceptRoot>> _expressTypeConceptRootLookup;

        internal void AddConceptRootLookup(ConceptRoot conceptRoot)
        {
            if (_expressTypeConceptRootLookup == null)
                _expressTypeConceptRootLookup = new Dictionary<ExpressType, List<ConceptRoot>>();
            if (string.IsNullOrEmpty(conceptRoot.applicableRootEntity))
            {
                Log.LogError($"Null or empty ExpressType for ConceptRoot '{conceptRoot.name}' (uuid: {conceptRoot.uuid})");
                return;
            }
            try
            {
                var tp = GetExpressType(conceptRoot.ParentModelView.applicableSchema, conceptRoot.applicableRootEntity);
                if (tp == null)
                {
                    return;
                }
                // needs to add conceptRoot to all non abstract sub types
                AddWithSubtypes(tp, conceptRoot);
            }
            catch (Exception)
            {
                Log.LogError($"ExpressType {conceptRoot.applicableRootEntity} not recognised for ConceptRoot '{conceptRoot.name}' (uuid: {conceptRoot.uuid})");
            }
        }

        /// <summary>
        /// Initialises the engine given instances of an xbim model and an mvdxml
        /// </summary>
        /// <param name="modelViewDefinition">modelviewdefinition instance that defines the validation logic</param>
        /// <param name="model">xbim model to be tested</param>
        /// <param name="forceModelSchema">sets the value for the <see cref="ForceModelSchema" /> property of the engine</param>
        public MvdEngine(mvdXML modelViewDefinition, IModel model, bool forceModelSchema = false)
        {
            _model = model;
            Mvd = modelViewDefinition;
            _forceModelSchema = forceModelSchema;

            FixReferences();
            ClearCache();
        }
        
        /// <summary>
        /// navigates the mvdXml tree to set all references and lookup dictionaries
        /// </summary>
        public void FixReferences()
        {
            _expressTypeConceptRootLookup = null;
            Mvd.Engine = this;
            if (Mvd.Views != null)
            {
                foreach (var modelView in Mvd.Views)
                {
                    modelView.SetParent(Mvd);
                }
            }
            // ReSharper disable once InvertIf
            if (Mvd.Templates != null)
            {
                foreach (var template in Mvd.Templates)
                {
                    template.SetParent(Mvd);
                }
            }
        }

        /// <summary>
        /// Accessof of nested ConceptRoots
        /// </summary>
        public IEnumerable<ConceptRoot> ConceptRoots
        {
            get {
                return Mvd.Views.SelectMany(modelView => modelView.Roots);
            }
        }

        /// <summary>
        /// Accessof of nested ExchangeRequirement
        /// </summary>
        public IEnumerable<ModelViewExchangeRequirement> ExchangeRequirement
        {
            get {
                return Mvd.Views.SelectMany(modelView => modelView.ExchangeRequirements);
            }
        }

        /// <summary>
        /// Allows to use a concept for extracting datatables of values from an entity.
        /// This function returns columns from all the indicators found in the TemplateRules
        /// </summary>
        /// <param name="entity">the entity of interest</param>
        /// <param name="concept">the concept to apply</param>
        /// <returns></returns>
        public DataTable GetData(IPersistEntity entity, Concept concept)
        {
            var template = concept.ConceptTemplate;
            if (concept.TemplateRules == null)
                return null;
            var dataIndicators = concept.TemplateRules.GetIndicators();
            return GetData(entity, template, dataIndicators);
        }

        /// <summary>
        /// Allows to use a ConceptTemplate for extracting datatables of values from an entity.
        /// This function returns columns from all the indicators found in all applicable rules
        /// </summary>
        /// <param name="entity">the entity of interest</param>
        /// <param name="template">the ConceptTemplate to apply</param>
        /// <returns></returns>
        public DataTable GetData(IPersistEntity entity, ConceptTemplate template)
        {
            // todo: there's no filter of the applicability of the template to the entity in the GetData function
            var allRules = template.GetRecursiveRuleIds().Distinct();
            var allIndicators = allRules.Select(rule => new DataIndicator(rule)).ToList();

            return GetData(entity, template, allIndicators);
        }

        /// <summary>
        /// Allows to use a ConceptTemplate for extracting datatables of values from an entity.
        /// Columns returned depend on specified dataIndicators
        /// </summary>
        /// <param name="entity">the entity of interest</param>
        /// <param name="template">the ConceptTemplate to apply</param>
        /// <param name="dataIndicators">the indicators of interest</param>
        /// <returns></returns>
        public DataTable GetData(IPersistEntity entity, ConceptTemplate template, IEnumerable<DataIndicator> dataIndicators)
        {
            // prepare the datatable
            var dt = new DataTable();
            var indicators = dataIndicators as DataIndicator[] ?? dataIndicators.ToArray();
            foreach (var dataIndicator in indicators)
            {
                var c = new DataColumn(dataIndicator.ColumnName);
                dt.Columns.Add(c);
            }
            
            // get the data from the model
            var fastIndicators = new DataIndicatorLookup(indicators);
            var vls = GetAttributes(template, entity, fastIndicators, "");

            // Now populate the datatable
            vls?.Populate(dt);
            
            return dt;
        }

        internal DataFragment GetAttributes(ConceptTemplate template, IPersistEntity entity, DataIndicatorLookup dataIndicators, string prefix)
        {
            var useT = template.GetSpecificConceptTemplateFor(entity);
            return GetAttributes(useT.Rules, entity, dataIndicators, prefix);
        }

        private DataFragment GetAttributes(AttributeRule[] attributeRules, IPersistEntity entity, DataIndicatorLookup dataIndicators, string prefix)
        {
            // 1.  get all values

            // add values at this level
            //
            var fragments = new List<DataFragment>();
            var t1 = ExtractRulesValues(entity, dataIndicators, attributeRules, prefix);
            if (t1 != null)
                fragments.Add(t1);

            // process the rest of the tree 
            //            
            var t = ProcessRuleTree(entity, dataIndicators, attributeRules, prefix);
            if (t != null)
                fragments.Add(t);
            

            // 2. combine them and return
            return  DataFragment.Combine(fragments);
        }

     

        // this is only internal for testing purposes; it should be private otherwise...
        // is there a way to do that?
        internal DataFragment ProcessRuleTree(IPersistEntity entity, DataIndicatorLookup dataIndicators, AttributeRule[] rules, string prefix)
        {
            OnProcessing?.Invoke(this, new EntityProcessingEventArgs(entity, EntityProcessingEventArgs.ProcessingEvent.ProcessRuleTree));

            // todo: xxx review return value logic
            var f = new List<DataFragment>();

            if (rules == null)
                return null;          
            foreach (var attributeRule in rules)
            {
                var attributeFragment = new DataFragment();

                // todo: if no entity is specified then the attribute is not set; is this correct?
                // see at deeper tree levels
                if (attributeRule.EntityRules == null || attributeRule.EntityRules.EntityRule.Length <= 0)
                    continue;
                // here we have to see if the attribute gets values that we can use
                ExpressMetaProperty retProp;
                var entityRuleValue = GetFieldValue(entity, attributeRule.AttributeName, out retProp);
                if (retProp == null) // we are in the case where the property does not exist in the schema
                {
                    Log.LogWarning($"{attributeRule.AttributeName} property is not available for type {entity.GetType().Name} (as expected on attributeRule '{attributeRule.RuleID}' in {attributeRule.ParentConceptTemplate.uuid})");
                    continue;
                }

                if (retProp.EntityAttribute.IsEnumerable)
                {
                    var propCollection = entityRuleValue as IEnumerable<object>;
                    if (propCollection == null)
                        continue;
                    var children = propCollection.OfType<IPersistEntity>().ToArray();
                    foreach (var child in children)
                    {
                        // todo: this is likely to return the same variable names for the datatable
                        var t = FillEntities(attributeRule.EntityRules.EntityRule, child, dataIndicators, prefix);
                        attributeFragment.Merge(t);
                    }
                }
                else
                {
                    var t = FillEntities(attributeRule.EntityRules.EntityRule, entityRuleValue as IPersistEntity, dataIndicators, prefix);
                    attributeFragment.Merge(t);
                }
                if (!attributeFragment.IsEmpty)
                    f.Add(attributeFragment);
            }
            return DataFragment.Combine(f);
        }
        
        private DataFragment ExtractRulesValues(IPersistEntity entity, DataIndicatorLookup dataIndicators, IEnumerable<IRule> rules, string prefix)
        {
            OnProcessing?.Invoke(this, 
                new EntityProcessingEventArgs(entity, EntityProcessingEventArgs.ProcessingEvent.ExtractRulesValues));
            
            var tmp = new List<DataFragment>();            
            if (rules == null)
                return null;
            foreach (var attributeRule in rules)
            {
                // sort out values at current level
                // we have an id and indicators require it
                if (string.IsNullOrEmpty(attributeRule.RuleID))
                    continue;
                var storageName = prefix + attributeRule.RuleID;
                if (!dataIndicators.Contains(storageName))
                    continue;

                // entityRules are already on the right element, attribute rules have to select it
                object value = entity;
                if (attributeRule is AttributeRule)
                {
                    ExpressMetaProperty retProp;
                    value = GetFieldValue(entity, attributeRule.Name, out retProp);
                    if (retProp == null)
                    {
                        // propery not found in class... ignore
                        // 
                        continue;
                    }
                }
                
                // set the value
                if (dataIndicators.Requires(storageName, DataIndicator.ValueSelectorEnum.Value))
                {
                    DataFragment tF;
                    if (value is IEnumerable<object>)
                    {
                        var asEnum = value as IEnumerable<object>;
                        tF = new DataFragment(storageName, asEnum);
                    }
                    else
                        tF = new DataFragment(storageName, value);
                    tmp.Add(tF);
                }

                // set the type
                //
                if (dataIndicators.Requires(storageName, DataIndicator.ValueSelectorEnum.Type))
                {
                    var storName = DataIndicator.GetColumnName(storageName, DataIndicator.ValueSelectorEnum.Type);            
                    tmp.Add( new DataFragment(storName, value.GetType().Name));
                }

                // set the Size
                //
                if (dataIndicators.Requires(storageName, DataIndicator.ValueSelectorEnum.Size))
                {
                    var storName = DataIndicator.GetColumnName(storageName, DataIndicator.ValueSelectorEnum.Size);
                    
                    if (value is IEnumerable<object>)
                    {
                        var asEnum = value as IEnumerable<object>;
                        tmp.Add(new DataFragment(storName, asEnum.Count()));
                    }
                    else if (value == null)
                        tmp.Add(new DataFragment(storName, 0));
                    else
                        tmp.Add(new DataFragment(storName, 1)); // there's one entity   
                }

                // set Existence
                // ReSharper disable once InvertIf // for symmetry in code
                //
                if (dataIndicators.Requires(storageName, DataIndicator.ValueSelectorEnum.Exists))
                {
                    var storName = DataIndicator.GetColumnName(storageName, DataIndicator.ValueSelectorEnum.Exists);
                    var storValue = value != null;
                    if (value != null)
                    {
                        storValue = value.ToString().Trim() != "";
                    }
                    tmp.Add(new DataFragment(storName, storValue)); // true if not null
                }
            }
            return DataFragment.Combine(tmp);
        }

        /// <summary>
        /// Used to identify the list of express classes that are specified in the mvdXML.
        /// The function uses the forceModelSchema variable to determine schema behaviour.
        /// </summary>
        /// <returns>distinct list of classes.</returns>
        public List<ExpressType> GetExpressTypes()
        {
            var classesInFilePrep = ConceptRoots.Select(cr => cr.ParentModelView.applicableSchema + "/" + cr.applicableRootEntity).Distinct().ToList();
            classesInFilePrep.Sort();

            var split = new[] { "/" };
            var previouslyAdded = new HashSet<ExpressType>();
            foreach (var classSchemaPair in classesInFilePrep)
            {
                var classSchemaArray = classSchemaPair.Split(split, StringSplitOptions.None);
                if (classSchemaArray.Length != 2)
                {
                    Log.LogError($"{classSchemaPair} is not a recognised class identification.");
                    continue;
                }
                var tp = GetExpressType(classSchemaArray[0], classSchemaArray[1]);
                if (tp == null) 
                    continue;
                if (!previouslyAdded.Contains(tp))
                    previouslyAdded.Add(tp);
            }
            return previouslyAdded.ToList();
        }
        
        internal DataFragment FillEntities(EntityRule[] rules, IPersistEntity entity, DataIndicatorLookup dataIndicators, string prefix)
        {
            if (entity == null)
                return null;
            var retFragments = new List<DataFragment>();

            foreach (var entityRule in rules)
            {
                // it's probably safe to assume that if we are here then the whole schema is the same of the element we are using
                // regardless from _forceModelSchema settings; it would otherwise be complicated to consider which of the schemas to use of the
                // array of schemes of the parent ConceptTemplate.
                var filterType = GetExpressType(entity.Model.Metadata, entityRule.EntityName.ToUpper());
                if (filterType == null)
                    continue;
                if (!filterType.NonAbstractSubTypes.Contains(entity.ExpressType))
                    continue;

                var thisRuleFragments = new List<DataFragment>();

                // get data at this level
                var t1 = ExtractRulesValues(entity, dataIndicators, entityRule.Yield(), prefix);
                if (t1 != null)
                    thisRuleFragments.Add(t1);
                
                if (entityRule.References != null)
                {
                    // need to resolve reference
                    if (string.IsNullOrEmpty(entityRule.References.Template?.@ref))
                        continue;
                    var tPrefix = prefix;
                    // Debug.Print(@"Ref: {0} on {1} ({2})", entityRule.References.Template.@ref, entity.EntityLabel, entity.GetType().Name);
                    var refTemplate = Mvd.GetConceptTemplate(entityRule.References.Template.@ref);
                    if (!string.IsNullOrEmpty(entityRule.References.IdPrefix))
                        tPrefix += entityRule.References.IdPrefix;
                    
                    var t = GetAttributes(refTemplate, entity, dataIndicators, tPrefix);
                    thisRuleFragments.Add(t);
                }
                else if (entityRule.AttributeRules != null)
                {
                    // rules nested directly
                    var t = GetAttributes(entityRule.AttributeRules.AttributeRule, entity, dataIndicators, prefix);
                    thisRuleFragments.Add(t);
                }

                if (thisRuleFragments.Any())
                {
                    retFragments.Add(DataFragment.Combine(thisRuleFragments));
                }
            }
            if (retFragments.Any())
            {
                return DataFragment.Combine(retFragments);
            }
            return null;
        }

        private Dictionary<string, ExpressMetaData> _metadatas;

        internal Dictionary<string, ExpressMetaData> SchemaMetadatas => _metadatas ?? (_metadatas = new Dictionary<string, ExpressMetaData>
        {
            {"ifc2x3", ExpressMetaData.GetMetadata(typeof(Ifc2x3.SharedBldgElements.IfcWall).Module)},
            {"ifc4", ExpressMetaData.GetMetadata(typeof(Ifc4.SharedBldgElements.IfcWall).Module)}
        });

        private static object GetFieldValue(IPersistEntity entity, string attributeName, out ExpressMetaProperty prop)
        {
            var entType = entity.ExpressType;
            prop = entType.Properties.FirstOrDefault(x => x.Value.PropertyInfo.Name == attributeName).Value  // test direct 
                ?? entType.Inverses.FirstOrDefault(x => x.PropertyInfo.Name == attributeName);                   // otherwise inverses
            var propVal = prop?.PropertyInfo.GetValue(entity, null);
            return propVal;
        }

        /// <summary>
        /// get Concept roots compatible with given ExpressType
        /// </summary>
        /// <param name="applicableClass">the ExpressType of interest</param>
        /// <returns></returns>
        public List<ConceptRoot> GetConceptRoots(ExpressType applicableClass)
        {
            List<ConceptRoot> ret;
            return _expressTypeConceptRootLookup.TryGetValue(applicableClass, out ret) 
                ? ret 
                : new List<ConceptRoot>();
        }

        /// <summary>
        /// Gets the list of ConceptRoots that match a string, interpreted against the schema of the specified model.
        /// </summary>
        /// <param name="applicableClass">The class name</param>
        /// <returns>A list of conceptRoots that match the requirement.</returns>
        public IEnumerable<ConceptRoot> GetConceptRoots(string applicableClass = "")
        {
            if (string.IsNullOrEmpty(applicableClass))
            {
                Log.LogError(@"GetConceptRoots() null or empty class name.");
                return Enumerable.Empty<ConceptRoot>();
            }
            try
            {
                var tp = GetExpressType(_model.Metadata, applicableClass);
                return GetConceptRoots(tp);
            }
            catch 
            {
                Log.LogWarning($"GetConceptRoots() invalid class name '{applicableClass}'");
            }
            return Enumerable.Empty<ConceptRoot>();
        }
        
        /// <summary>
        /// this function is only valuable to support development, it shows the content of a datatable in the debug window.
        /// </summary>
        /// <param name="dt">the datatable of interest</param>
        /// <returns></returns>
        public static IEnumerable<string> DebugDataTable(DataTable dt)
        {
            yield return ("___Start table\r\n");
            foreach (DataColumn dataCol in dt.Columns)
            {
                yield return ($"{dataCol.ColumnName}\t");
            }
            yield return ("\r\n");
            foreach (DataRow dataRow in dt.Rows)
            {
                foreach (DataColumn dataCol in dt.Columns)
                {
                    yield return ($"{dataRow[dataCol.ColumnName]}\t");
                }
                yield return ("\r\n");
            }
            yield return ("___End table\r\n");
        }

        /// <summary>
        /// Accessof to nested ConceptTemplates
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ConceptTemplate> GetAllConceptTemplates()
        {
            return Mvd.GetAllConceptTemplates();
        }

        /// <summary>
        /// Accessof to nested Concepts
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Concept> GetAllConcepts()
        {
            return Mvd.GetAllConcepts();
        }
        
        private void AddWithSubtypes(ExpressType tp, ConceptRoot conceptRoot)
        {
            AddType(conceptRoot, tp);
            // todo: should this be tp.SubTypes or allSubTypes (used to be ifcsubtypes before conversion)
            foreach (var nonAbstractSubType in tp.SubTypes)
            {
                AddWithSubtypes(nonAbstractSubType, conceptRoot);
            }
        }

        private void AddType(ConceptRoot conceptRoot, ExpressType st)
        {
            List<ConceptRoot> dicItem;
            if (_expressTypeConceptRootLookup.TryGetValue(st, out dicItem))
            {
                dicItem.Add(conceptRoot);
            }
            else
            {
                _expressTypeConceptRootLookup.Add(st, new List<ConceptRoot>() {conceptRoot});
            }
        }

        Dictionary<string, Concept> _dicConcepts;

        /// <summary>
        /// Returns a Concept by uuid lookup
        /// </summary>
        /// <param name="conceptUuid">the uuid that identifies the required Concept</param>
        /// <returns>null if not found</returns>
        public Concept GetConcept(string conceptUuid)
        {
            if (_dicConcepts == null)
            {
                _dicConcepts = new Dictionary<string, Concept>();
                foreach (var concept in GetAllConcepts())
                {
                    _dicConcepts.Add(concept.uuid, concept);
                }
            }
            Concept cRet;
            _dicConcepts.TryGetValue(conceptUuid, out cRet);
            return cRet;
        }

        private HashSet<string> _failedLookupMessages = new HashSet<string>();

        internal ExpressMetaData GetSchema(string schemaIdentifier)
        {
            if (_model != null && _forceModelSchema)
            {
                return _model.Metadata;
            }
            ExpressMetaData schemaToUse;
            if (SchemaMetadatas.TryGetValue(schemaIdentifier.ToLower(), out schemaToUse))
                return schemaToUse;
            if (_failedLookupMessages.Contains(schemaIdentifier.ToLower()))
                return null;
            Log.LogError($"Schema version {schemaIdentifier} is not a supported.");
            _failedLookupMessages.Add(schemaIdentifier.ToLower());
            return null;
        }
        
        internal ExpressType GetExpressType(string schemaString, string classString)
        {
            var schema =  GetSchema(schemaString);
            if (schema == null)
            {
                return null;
            }
            return GetExpressType(schema, classString);
        }

        private ExpressType GetExpressType(ExpressMetaData schema, string classString)
        {
            var v = schema.ExpressType(classString.ToUpper());
            if (v != null)
                return v;
            // warn once only;
            if (_failedLookupMessages.Contains(classString.ToUpper()))
                return null;
            Log.LogError($"{classString} is not a recognised class in {schema.Module.Name}.");
            _failedLookupMessages.Add(classString.ToUpper());
            return null;
        }
    }
}
