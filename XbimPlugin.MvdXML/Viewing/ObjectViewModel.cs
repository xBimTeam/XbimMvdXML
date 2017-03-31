using System.Collections.Generic;
using System.ComponentModel;

namespace XbimPlugin.MvdXML.Viewing
{
    public class ObjectViewModel : INotifyPropertyChanged
    {
        #region Data

        bool? _isChecked = false;
        ObjectViewModel _parent;

        internal ObjectViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        #endregion // Data

        #region CreateFoos

        internal ObjectViewModel()
        {
        }

        internal ObjectViewModel(object @object) 
        {
            Tag = @object;
        }

/*
        void Initialize()
        {
            foreach (var child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }
*/

        #endregion // CreateFoos

        #region Properties

        // todo: create a custom list that sets the parent on add 
        public void AddChild(ObjectViewModel child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        private List<ObjectViewModel> _children;
        public List<ObjectViewModel> Children {
            get
            {
                if (_children == null)
                    ExpandChildren();
                return _children;
            }
            set { _children = value; }   
        }

        private void ExpandChildren()
        {
            var element = Tag as ITreeElement;
            _children = new List<ObjectViewModel>();
            if (element == null) 
                return;
            foreach (var child in element.GetChildren())
            {
                AddChild(child);
            }
        }

        public bool IsInitiallySelected { get; private set; }

        public string Header { get; set; }

        public string Name => string.IsNullOrEmpty(Header) 
            ? Tag.ToString() 
            : Header;

        public object Tag { set; get; }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent)
                _parent?.VerifyCheckState();

            OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (var i = 0; i < Children.Count; ++i)
            {
                var current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}