using System;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    internal class ReferenceConstraint
    {
        public IUnique Referencing;
        public MvdItemReference Referenced;

        public ReferenceConstraint(IUnique referencing, string @ref, Type type)
        {
            Referencing = referencing;
            Referenced = new MvdItemReference(@ref, type);
        }
    }

    internal class MvdItemReference
    {
        public string ReferencedUuid;
        public Type ReferencedType;

        public MvdItemReference(string @ref, Type type)
        {
            ReferencedUuid = @ref;
            ReferencedType = type;
        }

        public MvdItemReference(IUnique item)
        {
            ReferencedUuid = item.GetUuid();
            ReferencedType = item.GetType();
        }

        public override bool Equals(object obj)
        {
            var item = obj as MvdItemReference;

            if (item == null)
            {
                return false;
            }

            return ReferencedUuid.Equals(item.ReferencedUuid)
                   && ReferencedType.Name.Equals(item.ReferencedType.Name);
        }

        public override int GetHashCode()
        {
            var tot = ReferencedUuid + ReferencedType.Name;
            return tot.GetHashCode();
        }
    }
}
