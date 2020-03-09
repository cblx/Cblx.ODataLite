using System;
using System.Collections.Generic;
using System.Text;

namespace Cblx.ODataLite
{
    internal class ODataParametersInternal : IODataParameters
    {
        public int? Skip { get; set; }
        public int? Top { get; set; }
        public string OrderBy { get; set; }
        public string Select { get; set; }
        public bool? Count { get; set; }
    }
}
