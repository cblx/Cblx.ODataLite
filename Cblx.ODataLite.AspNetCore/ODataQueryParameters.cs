using Microsoft.AspNetCore.Mvc;
using System;

namespace Cblx.ODataLite.AspNetCore
{
    public class ODataQueryParameters : IODataParameters
    {
        [FromQuery(Name = "$skip")]
        public int? Skip { get; set; }

        [FromQuery(Name = "$top")]
        public int? Top { get; set; }

        [FromQuery(Name = "$orderby")]
        public string OrderBy { get; set; }

        [FromQuery(Name = "$select")]
        public string Select { get; set; }

        [FromQuery(Name = "$count")]
        public bool? Count { get; set; }
    }
}
