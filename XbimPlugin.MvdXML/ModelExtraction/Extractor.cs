using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace XbimPlugin.MvdXML.ModelExtraction
{
    internal static class Extractor
    {
        internal static void Extract(IfcStore model, ICollection<IPersistEntity> keys)
        {
            if (model == null)
                return;
            var newName = model.FileName + ".mvdxmlsubset.ifc";

            PropertyTranformDelegate semanticFilter = (property, parentObject) =>
            {
                var retProp = property.PropertyInfo.GetValue(parentObject, null);
                // todo: need to filter only items that are in the keys passed

                var asIpersistEntity = retProp as IPersistEntity;
                if (asIpersistEntity != null)
                {
                    // if it's in the list then return
                    //
                    return keys.Contains(asIpersistEntity)
                        ? asIpersistEntity
                        : null;
                }
                var asIenumerablePersist = retProp as IEnumerable<IPersistEntity>;
                if (asIenumerablePersist != null)
                {
                    return asIenumerablePersist.Where(keys.Contains).ToArray();
                }

                return retProp;
            };


            using (var iModel = IfcStore.Create(model.SchemaVersion, Xbim.IO.XbimStoreType.InMemoryModel))
            {
                using (var txn = iModel.BeginTransaction("Insert copy"))
                {
                    //single map should be used for all insertions between two models
                    var map = new XbimInstanceHandleMap(model, iModel);

                    foreach (var wall in keys)
                    {
                        iModel.InsertCopy(wall, map, semanticFilter, true, false);
                    }

                    txn.Commit();
                }

                iModel.SaveAs(newName);
            }


        }
    }
}
