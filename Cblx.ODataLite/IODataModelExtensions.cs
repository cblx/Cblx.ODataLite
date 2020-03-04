using System;
using System.Collections.Generic;

namespace Cblx.ODataLite
{
    public static class Extensions {
        public static IEnumerable<string> GetSelected(this IODataParameters oDataParameters)
        {
            if(oDataParameters == null) { return new string[0]; }
            if (string.IsNullOrWhiteSpace(oDataParameters.Select)) { return new string[0]; }
            return new HashSet<string>(oDataParameters.Select.Split(','), StringComparer.OrdinalIgnoreCase);
        }

        public static TODataModel SetSelected<TODataModel>(this TODataModel oDataModel, params string[] selected) where TODataModel: IODataParameters
        {
            if(oDataModel != null)
            {
                oDataModel.Select = string.Join(",", selected);
            }
            return oDataModel;
        }
    }
}
