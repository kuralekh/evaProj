using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Invest.ViewModel.Models
{
    public class SecurityPriceModel : BaseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = @"Security is Required")]
        [Display(Name = @"Code")]
        public int SecurityId { get; set; }

        [Display(Name = @"Code")]
        public string SecurityCode { get; set; }

        [Display(Name = @"Security Name")]
        public string SecurityName { get; set; }

        [Required(ErrorMessage = @"Date is Required")]
        [Display(Name = @"Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }

        [Display(Name = @"Currency")]
        public int? CurrencyId { get; set; }

        [Display(Name = @"Currency")]
        public string Currency { get; set; }

        [Required(ErrorMessage = @"Unit Price is Required")]
        [Display(Name = @"Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        [Display(Name = @"Price NAV")]
        public decimal? PriceNAV { get; set; }

        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        [Display(Name = @"Price PUR")]
        public decimal? PricePUR { get; set; }

        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        [Display(Name = @"Interest Rate")]
        public decimal? InterestRate { get; set; }

        [Display(Name = @"Valuation Type")]
        public int? ValuationTypeId { get; set; }

        [Display(Name = @"Valuation Type")]
        public string ValuationType { get; set; }

        [Display(Name = @"Valuer")]
        public string Valuer { get; set; }

        [Display(Name = @"Attachment")]
        public string Attachment { get; set; }
        public bool IsDeleted { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FromDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ToDate { get; set; }


        [Display(Name = @"File")]
        //[RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Invalid Allocation")]
        //[Required(ErrorMessage = @"File is Required")]
        public HttpPostedFileBase File { get; set; }

        public bool IsRateIndex { get; set; }
        public string FileName { get; set; }
        public int Extension { get; set; }
        public string FName { get; set; }
        public int AttachmentTypeId { get; set; }
    }
}
