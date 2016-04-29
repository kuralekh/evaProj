using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Invest.ViewModel.Models
{
    public class ProductModel : BaseViewModel
    {
        public bool MakeItActive { get; set; }
        public bool MakeItInActive { get; set; }
        public bool MakeItArchived { get; set; }
        public bool IsBaseProduct { get; set; }

        [Required(ErrorMessage = @"Template Product is Required")]
        public int BaseProductVersionID { get; set; }
        public int ProductID { get; set; }

        [Required(ErrorMessage = @"Code is Required")]
        public string Code { get; set; }

        public string ProductVersion { get; set; }
        public DateTime? InceptionDate { get; set; }
        public string Name { get; set; }

        [Display(Name = @"Product Type")]
        [Required(ErrorMessage = @"Product Type is Required")]
        public int? ProductTypeId { get; set; }

        [Display(Name = @"Index Type")]
        public int? IndexTypeId { get; set; }

        [Display(Name = @"APIR Code")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = @"APIR Code must be 9 character in length.")]
        public string ProductAPIR { get; set; }

        [MaxLength(12)]
        [StringLength(12, MinimumLength = 12, ErrorMessage = @"ISIN Code must be 12 character in length.")]
        [Display(Name = @"ISIN Code")]
        public string ProductISIN { get; set; }

        [Display(Name = @"Institution")]
        public int? InstitutionId { get; set; }

        [Display(Name = @"Market")]
        public int? MarketId { get; set; }

        [Display(Name = @"Currency")]
        public int? CurrencyId { get; set; }

        [Display(Name = @"Sub Asset Class")]
        public int? SubAssetClassId { get; set; }

        [Display(Name = @"Region")]
        public int? RegionId { get; set; }

        [Display(Name = @"Status")]
        public int? StatusId { get; set; }

        public bool IsDeleted { get; set; }

        [Required(ErrorMessage = @"Asset Class is Required")]
        [Display(Name = @"Asset Class")]
        public int? AssetClassId { get; set; }

        [Display(Name = @"Asset Class")]
        public string AssetClass { get; set; }

        [Display(Name = @"Product Broker")]
        public List<int?> ProductBrokerIds { get; set; }

        [Display(Name = @"Product Description")]
        public string Description { get; set; }

        public decimal? Index { get; set; }
        public string ProductType { get; set; }
        public string IndexType { get; set; }
        public string Institution { get; set; }
        public string Market { get; set; }
        public string Currency { get; set; }
        public string SubAssetClass { get; set; }
        public string Region { get; set; }
        public string Status { get; set; }


        [Display(Name = @"Associate Security")]
        public List<ProductSecurityAssociationModel> SecurityAssociation { get; set; }

        [Display(Name = @"Associate Security List")]
        public List<int?> SecurityListIds { get; set; }
        public List<int?> SecurityIds { get; set; }
        public string SecName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StatusDate { get; set; }

        [Display(Name = @"Security Allocation")]
        public decimal? Allocation { get; set; }

        public decimal? TotalAllocation { get; set; }
        public bool IsWarning { get; set; }
        public bool IsAll { get; set; }
        public int? SecurityId { get; set; }
        public int? SecurityListId { get; set; }
        public string SecurityName { get; set; }
        public decimal? YearReturn { get; set; }
        public decimal? MonthReturn { get; set; }
        public List<ProductVersionModel> ProductVersionList { get; set; }
        public ProductVersionModel VersionDetail { get; set; }
        public ProductStatisticModel Statistic { get; set; }

        public int DisplayStatus { get; set; }
    }

    public class ProductSecurityAssociationModel
    {
        public int? SecurityId { get; set; }
        public string SecurityCode { get; set; }
        public string SecurityName { get; set; }
        public decimal? Allocation { get; set; }
        public int? SecurityListId { get; set; }
        public List<SecurityModel> SecurityList { get; set; }
    }

    public class ProductVersionModel
    {
        public int ProductVersionID { get; set; }
        public int? ProductID { get; set; }
        [Display(Name = @"Is Priced")]
        public bool IsPriced { get; set; }
        public string PricingSource { get; set; }
        [Display(Name = @"Pricing Source")]
        public int? PricingSourceId { get; set; }
        [Display(Name = @"Product ICR")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? ProductICR { get; set; }
        [Display(Name = @"Security MER")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? SecurityMER { get; set; }
        [Display(Name = @"Product MER")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? ProductMER { get; set; }
        public int? MajorVersion { get; set; }
        public int? MinorVersion { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Display(Name = @"Product Reimbursable")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? ProductReimbursable { get; set; }
        public int? StatusID { get; set; }
        public string ProductVersionStatus { get; set; }
        public string CombineVersion { get; set; }
        public bool IsOnlyVersion { get; set; }
        [Display(Name = @"Primary Benchmark")]
        public int? PrimaryBenchmarkProductID { get; set; }
        [Display(Name = @"Secondary Benchmark")]
        public int? SecondaryBenchmarkProductID { get; set; }

        [Required(ErrorMessage = @"Primary Price Type is Required")]
        [Display(Name = @"Primary Price Type")]
        public int? PrimaryPriceTypeId { get; set; }

        [Required(ErrorMessage = @"Secondary Price Type is Required")]
        [Display(Name = @"Secondary Price Type")]
        public int? SecondaryPriceTypeId { get; set; }

        [Display(Name = @"Target Return Rate")]
        public decimal? TargetReturnRate { get; set; }

        public List<ProductAssociationModel> ProductAssociationList { get; set; }
    }

    public class ProductAssociationModel
    {
        public int Id { get; set; }
        public int? ProductVersionId { get; set; }
        public int? SecurityId { get; set; }
        public SecurityModel Security { get; set; }
        public int? SecurityListId { get; set; }
        public decimal Allocation { get; set; }
        public bool IsDeleted { get; set; }
        public List<SecurityModel> SecurityList { get; set; }
        public List<SecurityListDetailModel> SecurityListDetail { get; set; }
    }

    public class ProductDayChange
    {
        public decimal Capital { get; set; }
        public decimal Total { get; set; }

        public ProductDayChange(decimal capital, decimal total)
        {
            Capital = capital;
            Total = total;
        }
    }
}
