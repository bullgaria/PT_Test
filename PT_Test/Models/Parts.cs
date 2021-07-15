namespace PT_Test.Models
{
    public class PartItem
    {
        public string PartNumber { get; set; }
        public string Description { get; set; }

        public string PartCode { get; set; }
        public string PartId { get; set; }

        public int Availability { get; set; }
        public decimal Price { get; set; }
    }
}
