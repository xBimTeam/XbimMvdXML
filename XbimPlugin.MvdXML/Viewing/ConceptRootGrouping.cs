using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.MvdXml;
using Xbim.Presentation;

namespace XbimPlugin.MvdXML.Viewing
{
    public class ConceptRootGrouping : ITreeElement
    {
        public int Level = 1;

        public override string ToString()
        {
            return $"ConceptRootGroup: {Name}";
        }

        public string[] Separators = {":"};

        private List<object> _children;

        public IEnumerable<object> GetChildren()
        {
            if (_children == null)
                PrepareChildren();
            return _children;
        }

        IEnumerable<ObjectViewModel> ITreeElement.GetChildren()
        {
            if (_children == null)
                PrepareChildren();
            foreach (var child in _children)
            {
                if (child is ConceptRootGrouping)
                {
                    var childAsGroup = child as ConceptRootGrouping;
                    yield return new ObjectViewModel() { Header = childAsGroup.Name, Tag = childAsGroup };
                }
                else if (child is ConceptRoot)
                {
                    var childAsConceptRoot = child as ConceptRoot;
                    yield return new ObjectViewModel() { Header = childAsConceptRoot.name, Tag = new ConceptRootExpander(childAsConceptRoot) };
                }
            }
        }

        private void PrepareChildren()
        {
            using (new WaitCursor())
            {
                _children = new List<object>();
                var dic = new Dictionary<string, List<ConceptRoot>>();
                foreach (var conceptRoot in _roots)
                {
                    var splitNames = conceptRoot.name.Split(Separators, StringSplitOptions.None);
                    var thisLen = splitNames.Length;
                    if (Level > thisLen)
                    { 
                        // add child to group
                        _children.Add(conceptRoot);
                        continue;
                    }

                    var thisIndex = Level - 1;
                    if (GroupOrder == GroupOrderMode.TopAtBack)
                    {
                        thisIndex = thisLen - Level;
                    }

                    var name = splitNames[thisIndex].Trim();
                    if (!dic.ContainsKey(name))
                        dic.Add(name, new List<ConceptRoot>() {conceptRoot});
                    else
                        dic[name].Add(conceptRoot);
                }
                var sorted = dic.Keys.ToList();
                sorted.Sort();
                foreach (var key in sorted)
                {
                    var value = dic[key];
                    var firstVal = value.FirstOrDefault();
                    if (firstVal == null)
                        continue;
                    var len = firstVal.name.Split(Separators, StringSplitOptions.None).Length;

                    if (value.Count == 1 && len == Level)
                        _children.Add(value.FirstOrDefault());
                    else
                        _children.Add(new ConceptRootGrouping(value) { Name = key, Level = Level + 1 });
                }
            }
        }

        public string Name { get; set; }

        private readonly IEnumerable<ConceptRoot> _roots;

        public IEnumerable<ConceptRoot> Roots => _roots;

        private GroupOrderMode _groupOrder = GroupOrderMode.TopAtBack;
        public GroupOrderMode GroupOrder
        {
            get { return _groupOrder; }
            set
            {
                _groupOrder = value;
                _children = null;
            }
        }

        public enum GroupOrderMode
        {
            TopAtFront,
            TopAtBack
        }

        public ConceptRootGrouping(IEnumerable<ConceptRoot> roots)
        {
            _roots = roots;
        }   
    }
}
