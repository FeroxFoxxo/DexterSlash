namespace DexterSlash.Databases.Models
{
    public class StatusDetail
    {
        public bool Online { get; set; }
        public double? ResponseTime { get; set; }

        public StatusDetail()
        {
            Online = true;
            ResponseTime = null;
        }
    }
}
