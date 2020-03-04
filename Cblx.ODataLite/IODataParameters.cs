namespace Cblx.ODataLite
{
    public interface IODataParameters
    {
        int? Skip { get; set; }

        int? Top { get; set; }

        string OrderBy { get; set; }

        string Select { get; set; }

        bool? Count { get; set; }
    }
}
