using System.ComponentModel.DataAnnotations;
using Market.Models;

namespace Market.Resources
{
    public class SaveProductResource
    {
        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Range(1, short.MaxValue)]
        public short QuantityInPackage { get; set; }

        [EnumDataType(typeof(EUnitOfMeasurement))]
        public EUnitOfMeasurement UnitOfMeasurement { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999.99")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}