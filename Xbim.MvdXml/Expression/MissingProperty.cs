namespace Xbim.MvdXml.Expression
{
    public class MissingProperty
    {
        public string PropertyName;
        public int AffectedEntity;

        public MissingProperty(string propertyName, int affectedEntity)
        {

            PropertyName = propertyName;
            AffectedEntity = affectedEntity;
        }
    }
}
