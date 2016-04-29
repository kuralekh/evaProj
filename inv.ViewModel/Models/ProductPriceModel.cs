using System;
using System.ComponentModel.DataAnnotations;

namespace Invest.ViewModel.Models
{
    public class ProductPriceModel : BaseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = @"Product is Required")]
        [Display(Name = @"Product")]
        public int? ProductId { get; set; }

        [Required(ErrorMessage = @"Date is Required")]
        [Display(Name = @"Date")]
        public DateTime? Date { get; set; }

        [Display(Name = @"Capital Price")]
        public decimal? CapitalPrice { get; set; }

        [Display(Name = @"Income Price")]
        public decimal? IncomePrice { get; set; }

        [Display(Name = @"TR Price")]
        public decimal? TRPrice { get; set; }

        public bool IsDeleted { get; set; }
        
        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FromDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ToDate { get; set; }
        public string ImportSource { get; set; }

        public int PriceCalculationStatus { get; set; }
    }
}
