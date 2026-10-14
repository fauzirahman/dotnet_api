namespace Market.Resources
{
    public class ProductResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int QuantityInPackage { get; set; }
        public string UnitOfMeasurement { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public CategoryResource Category {get;set;}
    }
}