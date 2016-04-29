using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Invest.ViewModel.Models
{
    public class SecurityModel : BaseViewModel
    {
        public List<string> SortList { get; set; }
        public string TypeName { get; set; }
        public int Id { get; set; }

        [Required(ErrorMessage = @"Security Code is Required")]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = @"Security Name is Required")]
        [Display(Name = @"Security Name")]
        public string Name { get; set; }

        [Display(Name = @"Security Description")]
        public string Description { get; set; }

        [Display(Name = @"APIR Code")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = @"APIR Code must be 9 character in length.")]
        public string APIRCode { get; set; }

        [MaxLength(12)]
        [StringLength(12, MinimumLength = 12, ErrorMessage = @"ISIN Code must be 12 character in length.")]
        [Display(Name = @"ISIN Code")]
        public string ISINCode { get; set; }

        [Display(Name = @"Security Category")]
        [Required(ErrorMessage = "Security Category is Required")]
        public int? SecurityCategoryId { get; set; }

        [Display(Name = @"Category")]
        public string SecurityCategory { get; set; }

        [Display(Name = @"Security Type")]
        public int? SecurityTypeId { get; set; }

        [Display(Name = @"Security Type")]
        public string SecurityType { get; set; }

        [Display(Name = @"Unitised")]
        public int? UnitisedId { get; set; }

        [Display(Name = @"Unitised")]
        public string Unitised { get; set; }

        [Display(Name = @"Market")]
        public int? MarketId { get; set; }

        [Display(Name = @"Market")]
        public string Market { get; set; }

        [Display(Name = @"Currency")]
        public int? CurrencyId { get; set; }

        [Display(Name = @"Currency")]
        public string Currency { get; set; }

        [Display(Name = @"Sub Asset Class")]
        public int? SubAssetClassId { get; set; }

        [Display(Name = @"Sub Asset Class")]
        public string SubAssetClass { get; set; }

        [Display(Name = @"Region")]
        public int? RegionId { get; set; }

        [Display(Name = @"Region")]
        public string Region { get; set; }

        [Display(Name = @"GICS - Sector")]
        public int? GICSId { get; set; }

        [Display(Name = @"GICS - Sector")]
        public string GICS { get; set; }

        [Display(Name = @"GICS - Industry Group")]
        public int? GICSTypeId { get; set; }

        [Display(Name = @"GICS - Industry Group")]
        public string GICSType { get; set; }

        [Display(Name = @"Rating")]
        public int? RatingId { get; set; }

        [Display(Name = @"Rating")]
        public string Rating { get; set; }

        [Display(Name = @"Security Status")]
        public int? SecurityStatusId { get; set; }

        [Display(Name = @"Security Status")]
        public string SecurityStatus { get; set; }

        [Display(Name = @"Units Held")]
        public decimal? UnitsHeld { get; set; }

        [Display(Name = @"Pricing Source")]
        public int? PricingSourceId { get; set; }

        [Display(Name = @"Pricing Source")]
        public string PricingSource { get; set; }

        [Display(Name = @"Distribution Type")]
        public int? DistributionTypeId { get; set; }

        [Display(Name = @"Distribution Type")]
        public string DistributionType { get; set; }

        [Display(Name = @"Distribution Frequency")]
        public int? DistributionFrequencyId { get; set; }

        [Display(Name = @"Distribution Frequency")]
        public string DistributionFrequency { get; set; }

        [Display(Name = @"Expense Ratio")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? ExpenseRatio { get; set; }

        [Display(Name = @"Liquidity(days)")]
        [Range(1, 9999, ErrorMessage = @"Liquidity(days) must be between 1 to 9999")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = @"Liquidity(days) must be between 1 to 9999")]
        public int? Liquidity { get; set; }

        [Display(Name = @"Property Details")]
        public string PropertyDetails { get; set; }

        public bool IsDeleted { get; set; }

        [Display(Name = @"Asset Class")]
        [Required(ErrorMessage = "Asset Class is Required")]
        public List<int?> AssetClassIds { get; set; }

        [Display(Name = @"Asset Class")]
        public List<string> AssetClass { get; set; }

        public string SearchString { get; set; }
        public string SearchUnderlyingType { get; set; }
        //TODO Need to change
        //[Display(Name = @"MDA Holding Limit")]
        //[DisplayFormat(DataFormatString = "{0:P2}")]
        //public decimal? MDAHoldingLimit { get; set; }

        //[Display(Name = @"Approval")]
        //public bool MDAIsApproved { get; set; }

        //[Display(Name = @"Super Holding Limit")]
        //[DisplayFormat(DataFormatString = "{0:P2}")]
        //public decimal? SuperHoldingLimit { get; set; }

        //[Display(Name = @"Approval")]
        //public bool SuperIsApproved { get; set; }

        public List<int?> SecurityIds { get; set; }

        [Display(Name = @"Primary Benchmark")]
        public int? PrimaryBenchmarkProductID { get; set; }

        [Display(Name = @"Secondary Benchmark")]
        public int? SecondaryBenchmarkProductID { get; set; }

        public string PrimaryBenchmark { get; set; }
        public string SecondaryBenchmark { get; set; }

        [Required(ErrorMessage = @"Primary Price Type is Required")]
        [Display(Name = @"Primary Price Type")]
        public int? PriceTypeId { get; set; }

        [Required(ErrorMessage = @"Secondary Price Type is Required")]
        [Display(Name = @"Secondary Price Type")]
        public int? SecondaryPriceTypeId { get; set; }

        [Display(Name = @"Total Return Year")]
        public decimal? TotalReturnYear { get; set; }
        
        public OptionsDetailModel OptionsDetail { get; set; }

        public SecurityReturnModel SecurityReturn { get; set; }

        public List<OptionsTypeModel> OptionsType { get; set; }

        public List<OptionsStyleModel> OptionsStyle { get; set; }

        public List<OptionsProductTypeModel> OptionsProductType { get; set; }

        public List<UnderlyingModel> UnderlyingList { get; set; }

        public List<string> DashboardFilter { get; set; }

        public List<AssetClassModel> AssetClassModelList { get; set; }

        public TermDeposite TermDeposite { get; set; }

        public List<string> Term { get; set; }

        public List<SelectListItem> FilteredTermList { get; set; }

        public List<SelectListItem> FilteredIntTermList { get; set; }

        public List<TermSortModel> TermSortList { get; set; }
        public List<int?> Institution { get; set; }
        //Nav Security Propert

        public int? AddressId { get; set; }
        public int NavSecurityPropertyDetailId { get; set; }

        [Display(Name = @"Line 1")]
        [Required(ErrorMessage = "Line One is Required")]
        public string LineOne { get; set; }

        [Display(Name = @"Line 2")]
        public string LineTwo { get; set; }

        [Display(Name = @"City")]
        public string City { get; set; }

        [Display(Name = @"Country")]
        public string Country { get; set; }

        [Display(Name = @"State")]
        public string State { get; set; }

        [Display(Name = @"Post Code")]
        public int? PostCode { get; set; }

        [Display(Name = @"Volume Number")]
        public string VolumeNumber { get; set; }

        [Display(Name = @"Lot On Plan")]
        public string LotOnPlan { get; set; }

        [Display(Name = @"Council Property Number")]
        public string CouncilPropertyNumber { get; set; }

        [Display(Name = @"Standard Parcel Identifier")]
        public string StandardParcelIdentifier { get; set; }

        [Display(Name = @"Crown Allotment")]
        public string CrownAllotment { get; set; }
        [Display(Name = @"Allocation")]
        [Required(ErrorMessage = @"Allocation is Required")]
        public decimal? Allocation { get; set; }


        public List<SelectListItem> CountryAll { get; set; }

        public List<SelectListItem> StateAll { get; set; }

        [Display(Name = @"Inception Date")]
        public DateTime? InceptionDate { get; set; }

        [Display(Name = @"Termination Date")]
        public DateTime? TerminationDate { get; set; }

        public decimal? AmountInvest { get; set; }
        public decimal? IntrestRate { get; set; }
        public string RequestType { get; set; }

        public bool DisplayStatus { get; set; }

        [Display(Name = "Client Type")]
        public int? ClientAccountTypeId { get; set; }

    }

    public class TermSortModel
    {
        public string Term { get; set; }
        public int? index { get; set; }
    }

    public class OptionsDetailModel
    {
        public int Id { get; set; }

        [Display(Name = @"Option Market")]
        [Required(ErrorMessage = @"Option Market is Required")]
        public int MarketId { get; set; }

        [Display(Name = @"ASX Code")]
        public string ASXCode { get; set; }

        [Display(Name = @"Underlying")]
        [Required(ErrorMessage = @"Underlying is Required")]
        public int Underlying { get; set; }

        [Display(Name = @"Underlying Type")]
        public string UnderlyingType { get; set; }

        public string UnderlyingValue { get; set; }

        [Display(Name = @"Options Type")]
        public int? OptionsTypeId { get; set; }

        [Display(Name = @"Exp Date")]
        [Required(ErrorMessage = @"Exp Date is Required")]
        public DateTime ExpDate { get; set; }

        [Display(Name = @"Strike Price")]
        public decimal? StrikePrice { get; set; }

        [Display(Name = @"Options Style")]
        public int? OptionsStyleId { get; set; }

        [Display(Name = @"Contract Size")]
        public decimal? ContractSize { get; set; }

        [Display(Name = @"Derivative Product")]
        public string DerivativeProduct { get; set; }

        [Display(Name = @"Options Product Type")]
        [Required(ErrorMessage = @"Product Type is Required")]
        public int OptionsProductTypeId { get; set; }

        [Display(Name = @"Category")]
        public string Category { get; set; }

        [Display(Name = @"SecurityId")]
        public int? SecurityId { get; set; }

    }

    public class OptionsTypeModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class OptionsStyleModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class OptionsProductTypeModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class UnderlyingModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }

    public class KeyValPair
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class TermDeposite
    {
        public int Id { get; set; }

        [Display(Name = @"Institution")]
        [Required(ErrorMessage = @"Institution is Required")]
        public int InstitutionId { get; set; }

        [Display(Name = @"Min Deposit")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Min Deposit is Required")]
        public decimal? MinDeposite { get; set; }

        [Display(Name = @"Max Deposit")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Max Deposit is Required")]
        public decimal? MaxDeposite { get; set; }

        [Display(Name = @"Term")]
        [Required(ErrorMessage = "Term is Required")]
        public string Term { get; set; }

        [Display(Name = @"Provider Rate")]
        [Required(ErrorMessage = @"Provider rate is Required")]
        public int ProviderRateId { get; set; }

        [Display(Name = @"Provider Type")]
        public string ProviderType { get; set; }

        [Display(Name = @"Provider Call Rate")]
        public string ProviderCallRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Display(Name = @"Provider Rate Brokerage")]
        public decimal? ProviderRateBrokerage { get; set; }

        [Display(Name = @"Provider Settlement Type")]
        public string ProviderSettlementType { get; set; }

        [Display(Name = @"Broker")]
        [Required(ErrorMessage = "Broker is Required")]
        public int? BrokerId { get; set; }


        public string Broker { get; set; }
        public string Institution { get; set; }

        public string InstituionPDSLink { get; set; }

        public string InstituionFSGLink { get; set; }
        public byte[] InstitutionImage { get; set; }

        public int? TermSortIndex { get; set; }

        public string ClientType { get; set; }

    }
}
