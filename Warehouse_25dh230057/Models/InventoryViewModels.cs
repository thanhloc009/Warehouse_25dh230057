using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Warehouse_25dh230057.Models
{
    public class UpdateQuantityViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int OldQuantity { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng thực tế.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là số nguyên không âm.")]
        public int? ActualQuantity { get; set; }

        public string DifferenceMessage { get; set; }
        public int Difference { get; set; }
    }

    public class UpdateTwoQuantitiesViewModel
    {
        [Required]
        public int? ProductID1 { get; set; }

        [Required]
        public int? ProductID2 { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng A.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng A phải không âm.")]
        public int? ActualQuantity1 { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng B.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng B phải không âm.")]
        public int? ActualQuantity2 { get; set; }

        public string Message { get; set; }
    }

    public class ProductSearchViewModel
    {
        public List<Product> Products { get; set; }
        public string Keyword { get; set; }
        public string StockStatus { get; set; }
        public string SortOrder { get; set; }
    }

    public class InventoryStatisticsViewModel
    {
        public int ProductCount { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }
}
