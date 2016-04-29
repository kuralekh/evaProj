using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.ServiceModel.Web;
using Invest.Common;
using Invest.Common.Enumerators;
using Invest.Common.Extensions;
using Invest.Database;
using Invest.ViewModel;
using Invest.ViewModel.Models;

namespace Invest.Service.Components
{
    public class SecurityComponent : BaseComponent
    {
        private readonly IRepository<Security> _securityRepository;
        private readonly IRepository<SecurityAssetClass> _securityAssetClassRepository;
        private readonly IRepository<OptionsDetail> _optionDetailRepository;
        private readonly IRepository<SecurityType> _securityTypeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<AssetClass> _assetClassRepositry;
        private readonly IRepository<SecurityReturn> _securityReturnRepositry;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<NAVSecurityPropertyDetail> _navsecuritypropertydetailRepository;
        private readonly IRepository<LinkNAVSecurity> _linknavsecurityRepository;
        private readonly IRepository<TermDeposit> _termDepositeRepository;
        private readonly IRepository<Institution> _institutionRepository;
        private readonly IRepository<SecurityCategory> _securityCategoryRepository;
        public SecurityComponent()
        {
            _securityRepository = uow.Repository<Security>();
            _securityAssetClassRepository = uow.Repository<SecurityAssetClass>();
            _optionDetailRepository = uow.Repository<OptionsDetail>();
            _securityTypeRepository = uow.Repository<SecurityType>();
            _productRepository = uow.Repository<Product>();
            _assetClassRepositry = uow.Repository<AssetClass>();
            _securityReturnRepositry = uow.Repository<SecurityReturn>();
            _addressRepository = uow.Repository<Address>();
            _navsecuritypropertydetailRepository = uow.Repository<NAVSecurityPropertyDetail>();
            _linknavsecurityRepository = uow.Repository<LinkNAVSecurity>();
            _termDepositeRepository = uow.Repository<TermDeposit>();
            _institutionRepository = uow.Repository<Institution>();
            _securityCategoryRepository = uow.Repository<SecurityCategory>();

        }

        private IViewModel GetSecurityDetail(IViewModel baseModel, FilterOption filterOption)
        {
            var model = (SecurityModel)baseModel;
            var responseModel = new ResponseModel<SecurityModel>
            {
                Status = ResponseStatus.Success
            };
            try
            {
                switch (filterOption)
                {
                    case FilterOption.GetAll:
                        responseModel.ModelList = GetAllModelList();
                        break;
                    case FilterOption.ModelListByFields:
                        responseModel.ModelList = GetModelListByModelFields(model);
                        break;
                    case FilterOption.ModelByFields:
                        responseModel.Model = GetModelByModelFields(model);
                        break;
                    case FilterOption.GetAllPagination:
                        int totalRecords;
                        responseModel.ModelList = GetPaginatedData(model, out totalRecords);
                        responseModel.RecordCount = totalRecords;
                        break;
                    case FilterOption.GetAllForDropdown:
                        responseModel.ModelList = GetAllModelDropDownList();
                        break;
                    case FilterOption.GetFilteredDropdown:
                        responseModel.ModelList = GetFilteredModelDropDownList(model);
                        break;
                    case FilterOption.GetSearchedDropdown:
                        responseModel.Model = GetFilteredUnderlyingList(model);
                        break;
                    case FilterOption.GetSecurityAssetClass:
                        responseModel.Model = GetSecurityAssetClass(model);
                        break;
                    case FilterOption.GetTermDepositPaginatedData:
                        int totalTermDepositRecords;
                        responseModel.ModelList = GetTermDepositPaginatedData(model, out totalTermDepositRecords);
                        responseModel.RecordCount = totalTermDepositRecords;
                        break;
                }
                responseModel.Message = string.Format("{0}: {1}", filterOption,
                    MessageManager.GetReponseMessage(this, Option.Get, responseModel.Status));
            }
            catch (Exception exception)
            {
                LogUtility.LogException(exception, string.Format("{0}: {1}", filterOption, MessageManager.GetReponseMessage(this, Option.Get, responseModel.Status)));
                responseModel.Status = ResponseStatus.Error;
                responseModel.Error =
                    new ErrorModel
                    {
                        Description = filterOption.ToString(),
                        ErrorMessage = exception.Message,
                        Exception = exception
                    };
            }
            //var json = ModelUtilities.SerializeModel(responseModel);
            return responseModel;
        }

        public override IViewModel Get(IViewModel model, FilterOption filterOption)
        {
            return GetSecurityDetail(model, filterOption);
        }

        public override IViewModel Add(IViewModel model)
        {
            return AddSecurity(model);
        }

        public override IViewModel Update(IViewModel model)
        {
            return UpdateSecurity(model);
        }

        public override IViewModel Delete(IViewModel model)
        {
            return DeleteSecurity(model);
        }

        protected List<SecurityModel> GetAllModelList()
        {
            return _securityRepository.GetAll(w => w.IsDeleted == false)
                .Select(s => new SecurityModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Description = s.Description,
                    Name = s.Name,
                    APIRCode = s.APIRCode,
                    ISINCode = s.ISINCode,
                    SecurityCategoryId = s.SecurityCategoryId,
                    SecurityTypeId = s.SecurityTypeId,
                    UnitisedId = s.UnitisedId,
                    MarketId = s.MarketId,
                    CurrencyId = s.CurrencyId,
                    SubAssetClassId = s.SubAssetClassId,
                    RegionId = s.RegionId,
                    GICSId = s.GICSId,
                    GICSType = s.GICSType.Type,
                    Region = s.Region.RegionName,
                    GICSTypeId = s.GICSTypeId,
                    RatingId = s.RatingId,
                    SecurityStatusId = s.SecurityStatusId,
                    UnitsHeld = s.UnitsHeld,
                    PricingSourceId = s.PricingSourceId,
                    DistributionTypeId = s.DistributionTypeId,
                    DistributionFrequencyId = s.DistributionFrequencyId,
                    ExpenseRatio = s.ExpenseRatio * 100,
                    Liquidity = s.Liquidity,
                    PropertyDetails = s.PropertyDetails,
                    PrimaryBenchmarkProductID = s.PrimaryBenchmarkProductID,
                    SecondaryBenchmarkProductID = s.SecondaryBenchmarkProductID,
                    SecondaryPriceTypeId = s.SecondaryPriceTypeId,
                    SecurityType = s.SecurityType.Type,
                    AssetClass = s.SecurityAssetClasses.Where(w => w.IsDeleted == false).Select(r => r.AssetClass.Class).ToList(),
                    IsDeleted = s.IsDeleted,
                    SecurityCategory = s.SecurityCategory.Category,
                    Market = s.Market.Code,
                    PriceTypeId = s.PrimaryPriceTypeId
                }).ToList();
        }

        protected List<SecurityModel> GetModelListByModelFields(SecurityModel modelWithValue)
        {
            return _securityRepository.GetAll(w => w.IsDeleted == false).Where(Createfilter<Security>(modelWithValue))
                .Select(s =>
                    new SecurityModel
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Description = s.Description,
                        Name = s.Name,
                        APIRCode = s.APIRCode,
                        ISINCode = s.ISINCode,
                        SecurityCategoryId = s.SecurityCategoryId,
                        SecurityTypeId = s.SecurityTypeId,
                        UnitisedId = s.UnitisedId,
                        MarketId = s.MarketId,
                        CurrencyId = s.CurrencyId,
                        SubAssetClassId = s.SubAssetClassId,
                        RegionId = s.RegionId,
                        GICSId = s.GICSId,
                        GICSTypeId = s.GICSTypeId,
                        RatingId = s.RatingId,
                        SecurityStatusId = s.SecurityStatusId,
                        UnitsHeld = s.UnitsHeld,
                        PricingSourceId = s.PricingSourceId,
                        DistributionTypeId = s.DistributionTypeId,
                        DistributionFrequencyId = s.DistributionFrequencyId,
                        ExpenseRatio = s.ExpenseRatio * 100,
                        Liquidity = s.Liquidity,
                        PropertyDetails = s.PropertyDetails,
                        PrimaryBenchmarkProductID = s.PrimaryBenchmarkProductID,
                        SecondaryBenchmarkProductID = s.SecondaryBenchmarkProductID,
                        PriceTypeId = s.PrimaryPriceTypeId,
                        IsDeleted = s.IsDeleted,
                        AssetClassIds = s.SecurityAssetClasses.Where(w => w.IsDeleted == false).Select(r => r.AssetClassId).ToList(),
                        ////TODO Need to change
                        //MDAHoldingLimit = s.SecurityHoldingDetails.FirstOrDefault(f => f.SecurityHoldingTypeId == 1).HoldingLimit,
                        //SuperHoldingLimit = s.SecurityHoldingDetails.FirstOrDefault(f => f.SecurityHoldingTypeId == 2).HoldingLimit,
                        //MDAIsApproved = s.SecurityHoldingDetails.Any() && s.SecurityHoldingDetails.FirstOrDefault(f => f.SecurityHoldingTypeId == 1).IsApproved,
                        //SuperIsApproved = s.SecurityHoldingDetails.Any() && s.SecurityHoldingDetails.FirstOrDefault(f => f.SecurityHoldingTypeId == 2).IsApproved,
                    }).ToList();
        }

        protected SecurityModel GetModelByModelFields(SecurityModel modelWithValue)
        {
            var result = _securityRepository.GetAll(w => w.IsDeleted == false).Where(Createfilter<Security>(modelWithValue))
                .Select(s =>
                    new SecurityModel
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Description = s.Description,
                        Name = s.Name,
                        APIRCode = s.APIRCode,
                        ISINCode = s.ISINCode,
                        SecurityCategoryId = s.SecurityCategoryId,
                        SecurityTypeId = s.SecurityTypeId,
                        SecurityType = s.SecurityType.Type,
                        UnitisedId = s.UnitisedId,
                        MarketId = s.MarketId,
                        CurrencyId = s.CurrencyId,
                        SubAssetClassId = s.SubAssetClassId,
                        RegionId = s.RegionId,
                        GICSId = s.GICSId,
                        GICSTypeId = s.GICSTypeId,
                        RatingId = s.RatingId,
                        SecurityStatusId = s.SecurityStatusId,
                        UnitsHeld = s.UnitsHeld,
                        PricingSourceId = s.PricingSourceId,
                        DistributionTypeId = s.DistributionTypeId,
                        DistributionFrequencyId = s.DistributionFrequencyId,
                        ExpenseRatio = s.ExpenseRatio * 100,
                        Liquidity = s.Liquidity,
                        PropertyDetails = s.PropertyDetails,
                        PrimaryBenchmarkProductID = s.PrimaryBenchmarkProductID,
                        SecondaryBenchmarkProductID = s.SecondaryBenchmarkProductID,
                        PriceTypeId = s.PrimaryPriceTypeId,
                        SecondaryPriceTypeId = s.SecondaryPriceTypeId,
                        IsDeleted = s.IsDeleted,
                        AssetClassIds = s.SecurityAssetClasses.Where(w => w.IsDeleted == false).Select(r => r.AssetClassId).ToList(),
                        LineOne = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).Address.AddressLine1 : null),
                        LineTwo = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).Address.AddressLine2 : null),
                        Country = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).Address.Country : null),
                        PostCode = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).Address.PostCode : null),
                        City = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).Address.SuburbCity : null),
                        State = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).Address.State : null),
                        VolumeNumber = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).VolumeNumber : null),
                        CrownAllotment = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).CrownAllotment : null),
                        LotOnPlan = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).LotOnPlan : null),
                        CouncilPropertyNumber = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).CouncilPropertyNumber : null),
                        StandardParcelIdentifier = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).StandaradParcelIdentifier : null),
                        AddressId = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).AddressId : null),
                        NavSecurityPropertyDetailId = (s.SecurityCategoryId == 20 && s.UnitisedId == 1 ? s.NAVSecurityPropertyDetails.FirstOrDefault(a => a.SecurityId == s.Id).Id : 0),
                        TerminationDate = s.TerminationDate,
                        InceptionDate = s.InceptionDate,
                        OptionsDetail = s.OptionsDetails.Where(w => w.SecurityId == s.Id && w.IsDeleted == false).AsEnumerable().Select(o => new OptionsDetailModel
                        {
                            MarketId = o.MarketId,
                            ASXCode = o.ASXCode,
                            Underlying = o.Underlying,
                            UnderlyingType = o.UnderlyingType,
                            OptionsTypeId = o.OptionsTypeId,
                            ExpDate = o.ExpDate,
                            StrikePrice = o.StrikePrice,
                            OptionsStyleId = o.OptionsStyleId,
                            ContractSize = o.ContractSize,
                            DerivativeProduct = o.DerivativeProduct,
                            OptionsProductTypeId = o.OptionsProductTypeId,
                            Category = o.Category,
                            SecurityId = o.SecurityId

                        }).FirstOrDefault(),
                        TermDeposite = s.TermDeposits.Where(w => w.SecurityId == s.Id).AsEnumerable().Select(a => new TermDeposite
                        {
                            Id = a.Id,
                            InstitutionId = a.InstitutionId,
                            MinDeposite = a.MinDeposit,
                            MaxDeposite = a.MaxDeposit,
                            Term = a.Term,
                            ProviderRateBrokerage = a.ProviderRateBrokerage,
                            ProviderSettlementType = a.ProviderSettlementType,
                            BrokerId = a.BrokerId,
                        }).FirstOrDefault(),
                        ClientAccountTypeId = (s.TermDeposits.FirstOrDefault(w => w.SecurityId == s.Id) != null ? (s.SecurityCategoryId == 35 ? s.TermDeposits.FirstOrDefault(w => w.SecurityId == s.Id).ClientTypeId : null) : null)
                        ////TODO Need to change
                        //MDAHoldingLimit = s.SecurityHoldingDetails.FirstOrDefault(f => f.SecurityHoldingTypeId == 1).HoldingLimit,
                        //SuperHoldingLimit = s.SecurityHoldingDetails.FirstOrDefault(f => f.SecurityHoldingTypeId == 2).HoldingLimit,
                        //MDAIsApproved = s.SecurityHoldingDetails.Any() && s.SecurityHoldingDetails.FirstOrDefault(f => f.SecurityHoldingTypeId == 1).IsApproved,
                        //SuperIsApproved = s.SecurityHoldingDetails.Any() && s.SecurityHoldingDetails.FirstOrDefault(f => f.SecurityHoldingTypeId == 2).IsApproved,
                    }).FirstOrDefault();


            if (result != null && result.OptionsDetail != null)
            {
                int Id = result.OptionsDetail.Underlying;
                string underlyingtype = result.OptionsDetail.UnderlyingType;
                string Value = GetUnderlyingValue(Id, underlyingtype);
                result.OptionsDetail.UnderlyingValue = Value;
            }

            return result;

        }

        private string GetUnderlyingValue(int id, string underlyingtype)
        {
            string res = string.Empty;
            if (!string.IsNullOrEmpty(underlyingtype))
            {
                int _Id = int.Parse(id.ToString());
                if (underlyingtype.ToLower() == UnderlyingType.Index.ToString().ToLower())
                {
                    var data = _productRepository.Where(w => w.ProductID == _Id).FirstOrDefault();
                    res = data.Code + "-" + data.Name + "- Product";
                }
                if (underlyingtype.ToLower() == UnderlyingType.Security.ToString().ToLower())
                {
                    var data = _securityRepository.Where(w => w.Id == _Id).FirstOrDefault();
                    res = data.Code + "-" + data.Name + "- Security";
                }
            }
            return res;
        }

        public List<SecurityModel> GetPaginatedData(SecurityModel model, out int totalRecords)
        {
            IQueryable<Security> query;
            List<SecurityModel> result;
            if (model.RequestType == "OptionSecurities")
            {
                query = _securityRepository.GetAll(w => w.IsDeleted == false && w.SecurityTypeId == 31);
                if (model.Id > 0)
                {
                    query = query.Where(w => w.OptionsDetails.Any(a => a.Underlying == model.Id));
                }
                if (model.DisplayStatus)
                {
                    query = query.Where(w => w.SecurityStatusId != 3 && w.SecurityStatusId != 5 && w.SecurityStatusId != 7);
                }
            }
            else if (model.RequestType == "NonOptionSecurities")
            {
                query = model.DisplayStatus ? _securityRepository.Where(w => w.IsDeleted == false && w.SecurityTypeId != 31 && w.SecurityStatusId != 3 && w.SecurityStatusId != 5 && w.SecurityStatusId != 7) : _securityRepository.Where(w => w.IsDeleted == false && w.SecurityTypeId != 31);
            }
            else
            {
                query = _securityRepository.GetAll(w => w.IsDeleted == false);
            }

            if (string.IsNullOrWhiteSpace(model.TableParam.sSearch))
            {
                if (model.IsDownload)
                {
                    result = query.Select(s => new SecurityModel
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Description = s.Description,
                        Name = s.Name,
                        SecurityCategory = s.SecurityCategory.Category,
                        Market = s.Market.Code,
                        SecurityType = s.SecurityType.Type,
                        Unitised = s.Unitised.Unitised1,
                        Currency = s.Currency.Code,
                        AssetClass = s.SecurityAssetClasses.Where(w => w.IsDeleted == false).Select(r => r.AssetClass.Class).ToList(),
                        SubAssetClass = s.SubAssetClass.Class,
                        Region = s.Region.RegionName,
                        GICS = s.GIC.GICS,
                        GICSType = s.GICSType.Type,
                        Rating = s.Rating.Rating1,
                        SecurityStatus = s.SecurityStatu.Status,
                        APIRCode = s.APIRCode,
                        ISINCode = s.ISINCode,
                        UnitsHeld = s.UnitsHeld,
                        PricingSource = s.PricingSource.Source,
                        DistributionType = s.DistributionType.Type,
                        PrimaryBenchmarkProductID = s.PrimaryBenchmarkProductID,
                        SecondaryBenchmarkProductID = s.SecondaryBenchmarkProductID,
                        PrimaryBenchmark = s.PrimaryBenchmarkProductID == null ? "" : s.Product.Code + "-" + s.Product.Name,
                        SecondaryBenchmark = s.SecondaryBenchmarkProductID == null ? "" : s.Product1.Code + "-" + s.Product1.Name,
                        DistributionFrequency = s.DistibutionFrequency.Frequency,
                        ExpenseRatio = s.ExpenseRatio,
                        PriceTypeId = s.PrimaryPriceTypeId,
                        Liquidity = s.Liquidity,
                        TotalReturnYear = s.SecurityReturn.YearReturn
                    }).OrderBy(od => od.Code).ToList();
                }
                else
                {
                    result = query.Select(s => new SecurityModel
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Name = s.Name,
                        SecurityCategory = s.SecurityCategory.Category,
                        Market = s.Market.Code,

                        //Need Add Asset Class
                        RegionId = s.RegionId,
                        GICSId = s.GICSId,
                        GICSTypeId = s.GICSTypeId,
                        AssetClass = s.SecurityAssetClasses.Where(w => w.IsDeleted == false).Select(r => r.AssetClass.Class).ToList(),
                        AssetClassIds = s.SecurityAssetClasses.Where(w => w.IsDeleted == false).Select(r => r.AssetClassId).ToList(),
                        SubAssetClass = s.SubAssetClass.Class,
                        SubAssetClassId = s.SubAssetClass.Id,
                        TotalReturnYear = s.SecurityReturn.YearReturn
                    }).ToList();
                }
            }
            else
            {
                if (model.IsDownload)
                {
                    result = query.Select(s => new SecurityModel
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Description = s.Description,
                        Name = s.Name,
                        SecurityCategory = s.SecurityCategory.Category,
                        Market = s.Market.Code,
                        SecurityType = s.SecurityType.Type,
                        Unitised = s.Unitised.Unitised1,
                        Currency = s.Currency.Code,
                        AssetClass = s.SecurityAssetClasses.Where(w => w.IsDeleted == false).Select(r => r.AssetClass.Class).ToList(),
                        SubAssetClass = s.SubAssetClass.Class,
                        Region = s.Region.RegionName,
                        GICS = s.GIC.GICS,
                        GICSType = s.GICSType.Type,
                        Rating = s.Rating.Rating1,
                        SecurityStatus = s.SecurityStatu.Status,
                        APIRCode = s.APIRCode,
                        ISINCode = s.ISINCode,
                        UnitsHeld = s.UnitsHeld,
                        PricingSource = s.PricingSource.Source,
                        DistributionType = s.DistributionType.Type,
                        PrimaryBenchmarkProductID = s.PrimaryBenchmarkProductID,
                        SecondaryBenchmarkProductID = s.SecondaryBenchmarkProductID,
                        PrimaryBenchmark = s.PrimaryBenchmarkProductID == null ? "" : s.Product.Code + "-" + s.Product.Name,
                        SecondaryBenchmark = s.SecondaryBenchmarkProductID == null ? "" : s.Product1.Code + "-" + s.Product1.Name,
                        PriceTypeId = s.PrimaryPriceTypeId,
                        DistributionFrequency = s.DistibutionFrequency.Frequency,
                        ExpenseRatio = s.ExpenseRatio,
                        Liquidity = s.Liquidity
                    }).Where(s => s.Code != null && s.Code.ToLower().Contains(model.TableParam.sSearch.ToLower())
                                  || s.Name != null && s.Name.ToLower().Contains(model.TableParam.sSearch.ToLower())
                                  ||
                                  s.SecurityCategory != null &&
                                  s.SecurityCategory.ToLower().Contains(model.TableParam.sSearch.ToLower())
                                  || s.Market != null && s.Market.ToLower().Contains(model.TableParam.sSearch.ToLower())).OrderBy(od => od.Code)
                        .ToList();
                }
                else
                {
                    result = query.Select(s => new SecurityModel
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Name = s.Name,
                        SecurityCategory = s.SecurityCategory.Category,
                        Market = s.Market.Code
                    }).Where(s => s.Code != null && s.Code.ToLower().Contains(model.TableParam.sSearch.ToLower())
                                  || s.Name != null && s.Name.ToLower().Contains(model.TableParam.sSearch.ToLower())
                                  ||
                                  s.SecurityCategory != null &&
                                  s.SecurityCategory.ToLower().Contains(model.TableParam.sSearch.ToLower())
                                  || s.Market != null && s.Market.ToLower().Contains(model.TableParam.sSearch.ToLower()))
                        .ToList();
                }
            }

            List<SecurityModel> filtered = CustomFilter(model, result);
            totalRecords = filtered.Count;

            return model.IsDownload ? filtered : SortingTableData(model, filtered);

        }

        protected List<SecurityModel> GetAllModelDropDownList()
        {
            var query = _securityRepository.GetAll(w => w.IsDeleted == false);
            return query
                .Select(s => new SecurityModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name + (s.Market == null ? string.Empty : " - " + s.Market.Code)
                }).OrderBy(o => o.Code).ToList();
        }

        protected List<SecurityModel> GetFilteredModelDropDownList(SecurityModel model)
        {
            IQueryable<Security> query;
            if (model.SecurityType == "Option")
            {
                query = (from s in _securityRepository.GetAll()
                         join o in _optionDetailRepository.GetAll() on s.Id equals o.Underlying
                         where s.IsDeleted == false && o.IsDeleted == false
                         select s).Distinct();
            }
            else
            {
                query = _securityRepository.GetAll(w => w.IsDeleted == false && (model.SecurityType == "All" || (model.SecurityType == "Rate" ? (w.Unitised.Unitised1 == "Rate") : (w.Unitised.Unitised1 != "Rate"))));
            }

            if (!string.IsNullOrEmpty(model.Code))
            {
                query = query.Where(w =>  w.Code.ToLower().StartsWith(model.Code.ToLower()) || w.Name.ToLower().Contains(model.Code.ToLower()) || w.Market.Code.ToLower().StartsWith(model.Code.ToLower()));
            }
            if (model.SecurityIds != null && model.SecurityIds.Any())
                query = query.Where(w => model.SecurityIds.Contains(w.Id));

            var result = query.Select(s => new SecurityModel
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name + (s.Market == null ? string.Empty : " - " + s.Market.Code)
            }).OrderByDescending(o => o.Code == model.Code).ThenBy(o => o.Code).Take(150).ToList();

            if (model.Id != 0)
            {
                var security = _securityRepository.GetById(model.Id);
                if (!result.Exists(f => f.Id == security.Id))
                {
                    result.Add(new SecurityModel
                    {
                        Id = security.Id,
                        Code = security.Code,
                        Name = security.Name
                    });
                }
            }
            return result;
        }

        protected SecurityModel GetFilteredUnderlyingList(SecurityModel model)
        {
            var securityQuery = _securityRepository.GetAll(s => s.IsDeleted == false);
            var productQuery = _productRepository.GetAll(p => p.IsDeleted == false && p.ProductTypeId == 3);
            if (!string.IsNullOrEmpty(model.SearchString))
            {
                securityQuery = securityQuery.Where(s => s.Code.ToLower().StartsWith(model.SearchString.ToLower()) || s.Name.ToLower().Contains(model.SearchString.ToLower()) || s.Market.Code.ToLower().StartsWith(model.SearchString.ToLower()));
                productQuery = productQuery.Where(p => p.Code.ToLower().Contains(model.SearchString.ToLower()) || p.Name.ToLower().Contains(model.SearchString.ToLower()));
            }

            var result = new List<UnderlyingModel>();
            //if (model.SearchUnderlyingType.ToLower() == "security")
            //{
            result = securityQuery.Select(s => new UnderlyingModel
            {
                Id = s.Id,
                Name = s.Code + "-" + s.Name + "- Security"
            }).OrderBy(o => o.Name).Take(100).ToList();
            //}
            //if (model.SearchUnderlyingType.ToLower() == "index")
            //{
            result.AddRange(productQuery.Select(s => new UnderlyingModel
            {
                Id = s.ProductID,
                Name = s.Code + "-" + s.Name + "- Product"
            }).OrderBy(o => o.Name).Take(100).ToList());
            //}
            SecurityModel securityModel = new SecurityModel();
            securityModel.UnderlyingList = new List<UnderlyingModel>();
            securityModel.UnderlyingList = result.OrderBy(o => o.Name).ToList();

            return securityModel;
        }

        protected SecurityModel GetSecurityAssetClass(SecurityModel model)
        {
            var securityAssetClassList = _securityAssetClassRepository.Where(w => w.IsDeleted == false && w.SecurityId == model.Id).Select(s => s.AssetClassId).ToList();

            //var result = _assetClassRepositry.Where(w => w.IsDeleted == false && w.SecurityAssetClasses.FirstOrDefault().SecurityId == model.Id && w.SecurityAssetClasses.FirstOrDefault().IsDeleted == false).Select(s =>
            var assetClassList = _assetClassRepositry.Where(w => securityAssetClassList.Contains(w.Id)).Select(s =>
                    new AssetClassModel()
                    {
                        Id = s.Id,
                        Class = s.Class,

                    }).ToList();

            var response = new SecurityModel { AssetClassModelList = assetClassList };
            return response;
        }

        private static List<SecurityModel> SortingTableData(SecurityModel model, List<SecurityModel> filtered)
        {
            if (model.TableParam.iSortingCols == 1)
            {
                if (model.TableParam.iSortCol_0 == 0 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(od => od.Code).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 0 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(od => od.Code).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 1 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.Name).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 1 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.Name).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.SecurityCategory).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "desc")
                {
                    return
                        filtered.OrderByDescending(p => p.SecurityCategory)
                            .Skip(model.TableParam.iDisplayStart)
                            .Take(model.TableParam.iDisplayLength)
                            .ToList();
                }

                if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.Market).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.Market).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                return filtered.OrderBy(x => x.Code).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
            }
            return filtered.OrderBy(od => od.Code).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
        }

        private static List<SecurityModel> CustomFilter(SecurityModel model, List<SecurityModel> query)
        {
            List<SecurityModel> filtered = query;

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_0))
            {
                filtered = filtered.Where(s => s.Code != null && s.Code.ToLower().Contains(model.TableParam.sSearch_0.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_1))
            {
                filtered = filtered.Where(s => s.Name != null && s.Name.ToLower().Contains(model.TableParam.sSearch_1.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_2))
            {
                filtered = filtered.Where(s => s.SecurityCategory != null && s.SecurityCategory.ToLower().Contains(model.TableParam.sSearch_2.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_3))
            {
                filtered = filtered.Where(s => s.Market != null && s.Market.ToLower().Contains(model.TableParam.sSearch_3.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_4))
            {
                //get all values between searched and searched+1
                filtered = filtered.Where(s => s.TotalReturnYear != null &&
                    s.TotalReturnYear.ToString().StartsWith(model.TableParam.sSearch_4.Replace(',', '.'))).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_6))
            {
                filtered = filtered.Where(s => s.Region != null && s.Region.ToLower().Contains(model.TableParam.sSearch_6.ToLower())).ToList();
            }

            if (model.TableParam.sAssetClassSearch != null)
            {
                filtered = filtered.Where(s => s.AssetClassIds != null && s.AssetClassIds.Intersect(model.TableParam.sAssetClassSearch).Any()).ToList();
            }

            if (model.TableParam.sSubAssetClassSearch != null)
            {
                filtered = filtered.Where(s => s.SubAssetClassId != null && model.TableParam.sSubAssetClassSearch.Contains(s.SubAssetClassId)).ToList();
            }

            if (model.TableParam.sRegionIdSearch != null)
            {
                filtered = filtered.Where(s => s.RegionId != null && s.RegionId == model.TableParam.sRegionIdSearch).ToList();
            }

            if (model.TableParam.sGicsSectorSearch != null)
            {
                filtered = filtered.Where(s => s.GICSId != null && s.GICSId == model.TableParam.sGicsSectorSearch).ToList();
            }

            if (model.TableParam.sGicsIndustryGrowthSearch != null)
            {
                filtered = filtered.Where(s => s.GICSTypeId != null && s.GICSTypeId == model.TableParam.sGicsIndustryGrowthSearch).ToList();
            }

            return filtered;
        }

        public IViewModel AddSecurity(IViewModel baseModel)
        {
            var model = (SecurityModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success,

            };
            try
            {
                //Check For Existing Record 
                var isExists = _securityRepository.Any(a => a.Code == model.Code && a.MarketId ==  model.MarketId && !a.IsDeleted);
             
                if (!isExists)
                {
                    var security = new Security
                    {
                        Code = model.Code.Trim(),
                        Name = model.Name,
                        Description = (model.Description == string.Empty ? model.LineOne + " " + model.LineTwo + " " + model.PostCode : model.Description),
                        APIRCode = model.APIRCode,
                        ISINCode = model.ISINCode,
                        SecurityCategoryId = model.SecurityCategoryId,
                        SecurityTypeId = model.SecurityTypeId,
                        UnitisedId = model.UnitisedId,
                        MarketId = model.MarketId,
                        CurrencyId = model.CurrencyId,
                        SubAssetClassId = model.SubAssetClassId,
                        RegionId = model.RegionId,
                        GICSId = model.GICSId,
                        GICSTypeId = model.GICSTypeId,
                        RatingId = model.RatingId,
                        SecurityStatusId = model.SecurityStatusId,
                        UnitsHeld = model.UnitsHeld,
                        PricingSourceId = model.PricingSourceId,
                        DistributionTypeId = model.DistributionTypeId,
                        DistributionFrequencyId = model.DistributionFrequencyId,
                        ExpenseRatio = model.ExpenseRatio / 100,
                        Liquidity = model.Liquidity,
                        PropertyDetails = model.PropertyDetails,
                        PrimaryBenchmarkProductID = model.PrimaryBenchmarkProductID,
                        SecondaryBenchmarkProductID = model.SecondaryBenchmarkProductID,
                        PrimaryPriceTypeId = model.PriceTypeId,
                        SecondaryPriceTypeId = model.SecondaryPriceTypeId,
                        TerminationDate = model.TerminationDate,
                        InceptionDate = model.InceptionDate

                    };
                    _securityRepository.Add(security);
                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Security", ObjectID = security.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                    if (model.UnitisedId == 1 && model.SecurityCategoryId == 20)
                    {
                        //var isAddressExists = _addressRepository.Where(a => a.AddressLine1.ToLower() == model.LineOne.ToLower() && a.AddressLine2.ToLower() == model.LineTwo.ToLower() && a.Country.ToLower() == model.Country.ToLower() && a.State.ToLower() == model.State.ToLower() && a.PostCode == model.PostCode && a.SuburbCity.ToLower() == model.City.ToLower()).FirstOrDefault();
                        var isAddressExists = _addressRepository.Where(a => a.AddressLine1.ToLower() == (model.LineOne != null ? model.LineOne.ToLower() : null) && a.AddressLine2.ToLower() == (model.LineTwo == null ? null : model.LineTwo.ToLower()) && a.Country.ToLower() == (model.Country != null ? model.Country.ToLower() : null) && a.State.ToLower() == (model.State != null ? model.State.ToLower() : null) && a.PostCode == (model.PostCode) && a.SuburbCity.ToLower() == (model.City != null ? model.City.ToLower() : null)).FirstOrDefault();
                        int? AddressId;
                        if (isAddressExists == null)
                        {
                            var _Address = new Address()
                            {
                                AddressLine1 = (model.SecurityCategoryId == 20 ? model.LineOne : null),
                                AddressLine2 = (model.SecurityCategoryId == 20 ? model.LineTwo : null),
                                Country = (model.SecurityCategoryId == 20 ? model.Country : null),
                                PostCode = (model.SecurityCategoryId == 20 ? model.PostCode : null),
                                SuburbCity = (model.SecurityCategoryId == 20 ? model.City : null),
                                State = (model.SecurityCategoryId == 20 ? model.State : null),
                            };
                            _addressRepository.Add(_Address);
                            uow.Commit();
                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Addresses", ObjectID = _Address.AddressId, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                            AddressId = _Address.AddressId;
                        }
                        else
                        {
                            AddressId = isAddressExists.AddressId;
                        }

                        var navSecurityPropertyDetail = new NAVSecurityPropertyDetail
                        {
                            //If security Category is property then insert else null
                            VolumeNumber = (model.SecurityCategoryId == 20 ? model.VolumeNumber : null),
                            CrownAllotment = (model.SecurityCategoryId == 20 ? model.CrownAllotment : null),
                            LotOnPlan = (model.SecurityCategoryId == 20 ? model.LotOnPlan : null),
                            CouncilPropertyNumber = (model.SecurityCategoryId == 20 ? model.CouncilPropertyNumber : null),
                            SecurityId = security.Id,
                            StandaradParcelIdentifier = (model.SecurityCategoryId == 20 ? model.StandardParcelIdentifier : null),
                            AddressId = AddressId,
                        };
                        _navsecuritypropertydetailRepository.Add(navSecurityPropertyDetail);
                        uow.Commit();
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "NavSecurityPropertyDetail", ObjectID = navSecurityPropertyDetail.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });


                    }

                    if (model.SecurityTypeId != null && model.SecurityTypeId.Value != 0 && _securityTypeRepository.GetById(model.SecurityTypeId.Value).Type.ToLower() == "option")
                    {
                        var optionsDetail = new OptionsDetail
                        {
                            MarketId = model.OptionsDetail.MarketId,
                            ASXCode = model.OptionsDetail.ASXCode,
                            UnderlyingType = model.OptionsDetail.UnderlyingType,
                            Underlying = model.OptionsDetail.Underlying,
                            OptionsTypeId = model.OptionsDetail.OptionsTypeId,
                            ExpDate = model.OptionsDetail.ExpDate,
                            StrikePrice = model.OptionsDetail.StrikePrice,
                            OptionsStyleId = model.OptionsDetail.OptionsStyleId,
                            ContractSize = model.OptionsDetail.ContractSize,
                            DerivativeProduct = model.OptionsDetail.DerivativeProduct,
                            OptionsProductTypeId = model.OptionsDetail.OptionsProductTypeId,
                            Category = model.OptionsDetail.Category,
                            SecurityId = security.Id
                        };
                        _optionDetailRepository.Add(optionsDetail);
                        uow.Commit();
                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "OptionsDetail", ObjectID = optionsDetail.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                    }
                    //For Term Deposite
                    if (security.Id > 0 && security.UnitisedId == 3)
                    {
                        var termDeposite = new TermDeposit()
                            {
                                InstitutionId = model.TermDeposite.InstitutionId,
                                MaxDeposit = model.TermDeposite.MaxDeposite,
                                SecurityId = security.Id,
                                MinDeposit =  model.TermDeposite.MinDeposite ,
                                Term = model.TermDeposite.Term,
                                ProviderRateBrokerage = model.TermDeposite.ProviderRateBrokerage,
                                ProviderSettlementType = model.TermDeposite.ProviderSettlementType,
                                BrokerId = model.TermDeposite.BrokerId,
                                ClientTypeId = model.SecurityCategoryId==35?model.ClientAccountTypeId:null
                            };
                        _termDepositeRepository.Add(termDeposite);
                        uow.Commit();
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "TermDeposit", ObjectID = termDeposite.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                    }



                    if (model.AssetClassIds != null)
                    {
                        foreach (var assetClassId in model.AssetClassIds)
                        {
                            var secAssetClass = new SecurityAssetClass
                            {
                                SecurityId = security.Id,
                                AssetClassId = assetClassId,
                            };
                            _securityAssetClassRepository.Add(secAssetClass);
                            uow.Commit();

                            // Maintaining Log
                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "SecurityAssetClass", ObjectID = secAssetClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                        }
                    }

                    #region HoldingDetail_ToDo

                    //var securityHoldingMDA = new SecurityHoldingDetail()
                    //    {
                    //        HoldingLimit = model.MDAHoldingLimit,
                    //        IsApproved = model.MDAIsApproved,
                    //        SecurityId = security.Id,
                    //        SecurityHoldingTypeId = 1,
                    //        CreatedOn = model.CreatedOn ?? DateTime.Now,
                    //        CreatedBy = model.CreatedBy ?? SessionObject.GetInstance.UserID,
                    //    };
                    //_securityHoldingDetail.Add(securityHoldingMDA);
                    //var securityHoldingSuper = new SecurityHoldingDetail()
                    //{
                    //    HoldingLimit = model.SuperHoldingLimit,
                    //    IsApproved = model.SuperIsApproved,
                    //    SecurityId = security.Id,
                    //    SecurityHoldingTypeId = 2,
                    //    CreatedOn = model.CreatedOn ?? DateTime.Now,
                    //    CreatedBy = model.CreatedBy ?? SessionObject.GetInstance.UserID,
                    //};
                    //_securityHoldingDetail.Add(securityHoldingSuper);

                    #endregion


                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordAddedSuccessfully, "Security");
                }
                else
                {
                    responseModel.Status = ResponseStatus.Failure;
                    responseModel.Message = "Security already exists.";
                }
            }
            catch (Exception exception)
            {

                responseModel.Status = ResponseStatus.Error;
                responseModel.Error =
                    new ErrorModel
                    {
                        Description = MessageManager.GetReponseMessage(this, Option.Add, responseModel.Status),
                        ErrorMessage = exception.Message,
                        Exception = exception
                    };
                //Adding log.
                LogUtility.LogException(exception, string.Format(MDA_Resource.Error_UnableToAdd));
            }
            return responseModel;
        }

        public IViewModel UpdateSecurity(IViewModel baseModel)
        {
            var model = (SecurityModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success
            };
            try
            {

                var security = _securityRepository.GetById(model.Id);
                var deletedAssetClassList = new Dictionary<int, bool>();
                //var secAssetClass = _securityAssetClassRepository.GetAll(w => w.SecurityId == model.Id);
                if (security != null)
                {
                    //Check For Existing Record 
                    if (model.UnitisedId == 3)
                    {
                        if (model.SecurityCategoryId!=36)
                        {
                            var institution = _institutionRepository.Where(a => a.Id == model.TermDeposite.InstitutionId && !a.IsDeleted).FirstOrDefault();
                            var broker = _institutionRepository.Where(a => a.Id == model.TermDeposite.BrokerId && a.IsBroker == true && !a.IsDeleted).FirstOrDefault();
                            if (model.SecurityCategoryId == 35 && model.TermDeposite.Term == "AtCall")
                            {
                                model.Code = (institution.Description.Replace(" ", "") + "_" + (model.SecurityCategoryId == 22 ? "TD_" : "AtCall_") + broker.Code + "_" + Math.Floor(model.TermDeposite.MaxDeposite.Value));
                            }
                            else
                            {
                                model.Code = (institution.Description.Replace(" ", "") + "_" + (model.SecurityCategoryId == 22 ? "TD_" : "AtCall_") + model.TermDeposite.Term.Replace(" ", "") + "_" + broker.Code + "_" + Math.Floor(model.TermDeposite.MaxDeposite.Value));
                            }
                        }
                       
                    }
                    var isExists = _securityRepository.Any(a => a.Id != model.Id && a.Code == model.Code && a.MarketId == model.MarketId && !a.IsDeleted);

                    if (!isExists)
                    {
                        security.Code = model.Code.Trim();
                        security.Description = model.Description;
                        security.Name = model.Name;
                        security.APIRCode = model.APIRCode;
                        security.ISINCode = model.ISINCode;
                        security.SecurityCategoryId = model.SecurityCategoryId;
                        security.SecurityTypeId = model.SecurityTypeId;
                        security.UnitisedId = model.UnitisedId;
                        security.MarketId = model.MarketId;
                        security.CurrencyId = model.CurrencyId;
                        security.SubAssetClassId = model.SubAssetClassId;
                        security.RegionId = model.RegionId;
                        security.GICSId = model.GICSId;
                        security.GICSTypeId = model.GICSTypeId;
                        security.RatingId = model.RatingId;
                        security.SecurityStatusId = model.SecurityStatusId;
                        security.UnitsHeld = model.UnitsHeld;
                        security.PricingSourceId = model.PricingSourceId;
                        security.DistributionTypeId = model.DistributionTypeId;
                        security.DistributionFrequencyId = model.DistributionFrequencyId;
                        security.ExpenseRatio = model.ExpenseRatio / 100;
                        security.Liquidity = model.Liquidity;
                        security.PropertyDetails = model.PropertyDetails;
                        security.PrimaryBenchmarkProductID = model.PrimaryBenchmarkProductID;
                        security.SecondaryBenchmarkProductID = model.SecondaryBenchmarkProductID;
                        security.PrimaryPriceTypeId = model.PriceTypeId;
                        security.SecondaryPriceTypeId = model.SecondaryPriceTypeId;
                        security.TerminationDate = model.TerminationDate;
                        security.InceptionDate = model.InceptionDate;
                        uow.Commit();
                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Security", ObjectID = security.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                        int? AddressId = 0;
                        if (model.UnitisedId == 1 && model.SecurityCategoryId == 20)
                        {
                            //var isAddressExists = _addressRepository.Where(a => a.AddressLine1.ToLower() == model.LineOne.ToLower() && a.AddressLine2.ToLower() == model.LineTwo.ToLower() && a.Country.ToLower() == model.Country.ToLower() && a.State.ToLower() == model.State.ToLower() && a.PostCode == model.PostCode && a.SuburbCity.ToLower() == model.City.ToLower()).FirstOrDefault();
                            var isAddressExists = _addressRepository.Where(a => a.AddressLine1.ToLower() == (model.LineOne != null ? model.LineOne.ToLower() : null) && a.AddressLine2.ToLower() == (model.LineTwo == null ? null : model.LineTwo.ToLower()) && a.Country.ToLower() == (model.Country != null ? model.Country.ToLower() : null) && a.State.ToLower() == (model.State != null ? model.State.ToLower() : null) && a.PostCode == (model.PostCode) && a.SuburbCity.ToLower() == (model.City != null ? model.City.ToLower() : null)).FirstOrDefault();
                            if (isAddressExists == null)
                            {
                                isAddressExists = _addressRepository.Where(a => a.AddressId == model.AddressId).FirstOrDefault();
                                if (isAddressExists != null)
                                {
                                    isAddressExists.AddressLine1 = model.LineOne;
                                    isAddressExists.AddressLine2 = model.LineTwo;
                                    isAddressExists.Country = model.Country;
                                    isAddressExists.PostCode = model.PostCode;
                                    isAddressExists.SuburbCity = model.City;
                                    isAddressExists.State = model.State;

                                    uow.Commit();
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Addresses", ObjectID = isAddressExists.AddressId, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                                    AddressId = isAddressExists.AddressId;
                                }
                                else
                                {
                                    var _Address = new Address()
                                    {
                                        AddressLine1 = (model.SecurityCategoryId == 20 ? model.LineOne : null),
                                        AddressLine2 = (model.SecurityCategoryId == 20 ? model.LineTwo : null),
                                        Country = (model.SecurityCategoryId == 20 ? model.Country : null),
                                        PostCode = (model.SecurityCategoryId == 20 ? model.PostCode : null),
                                        SuburbCity = (model.SecurityCategoryId == 20 ? model.City : null),
                                        State = (model.SecurityCategoryId == 20 ? model.State : null),
                                    };
                                    _addressRepository.Add(_Address);
                                    uow.Commit();
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Addresses", ObjectID = _Address.AddressId, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                    AddressId = _Address.AddressId;
                                }
                            }
                            else
                            {
                                AddressId = isAddressExists.AddressId;
                            }

                            var IsnavSecurityPropertyDetailExist = _navsecuritypropertydetailRepository.Where(a => a.Id == model.NavSecurityPropertyDetailId).FirstOrDefault();
                            if (IsnavSecurityPropertyDetailExist != null)
                            {
                                IsnavSecurityPropertyDetailExist.VolumeNumber = model.VolumeNumber;
                                IsnavSecurityPropertyDetailExist.CrownAllotment = model.CrownAllotment;
                                IsnavSecurityPropertyDetailExist.LotOnPlan = model.LotOnPlan;
                                IsnavSecurityPropertyDetailExist.CouncilPropertyNumber = model.CouncilPropertyNumber;
                                IsnavSecurityPropertyDetailExist.SecurityId = security.Id;
                                IsnavSecurityPropertyDetailExist.StandaradParcelIdentifier = model.StandardParcelIdentifier;
                                IsnavSecurityPropertyDetailExist.AddressId = AddressId;
                                uow.Commit();
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "NavSecurityPropertyDetail", ObjectID = IsnavSecurityPropertyDetailExist.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });

                            }
                            else
                            {
                                var navSecurityPropertyDetail = new NAVSecurityPropertyDetail
                                {
                                    //If security Category is property then insert else null
                                    VolumeNumber = (model.SecurityCategoryId == 20 ? model.VolumeNumber : null),
                                    CrownAllotment = (model.SecurityCategoryId == 20 ? model.CrownAllotment : null),
                                    LotOnPlan = (model.SecurityCategoryId == 20 ? model.LotOnPlan : null),
                                    CouncilPropertyNumber = (model.SecurityCategoryId == 20 ? model.CouncilPropertyNumber : null),
                                    SecurityId = security.Id,
                                    StandaradParcelIdentifier = (model.SecurityCategoryId == 20 ? model.StandardParcelIdentifier : null),
                                    AddressId = AddressId,
                                };
                                _navsecuritypropertydetailRepository.Add(navSecurityPropertyDetail);
                                uow.Commit();
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "NavSecurityPropertyDetail", ObjectID = navSecurityPropertyDetail.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                            }

                        }
                        foreach (var securityAssetClass in security.SecurityAssetClasses)
                        {
                            if (securityAssetClass.IsDeleted == false)
                            {
                                deletedAssetClassList.Add(securityAssetClass.Id, true);
                            }
                            securityAssetClass.IsDeleted = true;
                        }


                        // UPDATE OPTION DETAIL 
                        var optionsDetail = _optionDetailRepository.Where(w => w.SecurityId == security.Id).FirstOrDefault();
                        if (optionsDetail != null)
                        {
                            optionsDetail.IsDeleted = true;
                            uow.Commit();
                        }
                        if (model.SecurityTypeId != null && model.SecurityTypeId.Value != 0 && _securityTypeRepository.GetById(model.SecurityTypeId.Value).Type.ToLower() == "option")
                        {
                            // UPDATE 
                            if (optionsDetail != null)
                            {
                                optionsDetail.MarketId = model.OptionsDetail.MarketId;
                                optionsDetail.ASXCode = model.OptionsDetail.ASXCode;
                                optionsDetail.UnderlyingType = model.OptionsDetail.UnderlyingType;
                                optionsDetail.Underlying = model.OptionsDetail.Underlying;
                                optionsDetail.OptionsTypeId = model.OptionsDetail.OptionsTypeId;
                                optionsDetail.ExpDate = model.OptionsDetail.ExpDate;
                                optionsDetail.StrikePrice = model.OptionsDetail.StrikePrice;
                                optionsDetail.OptionsStyleId = model.OptionsDetail.OptionsStyleId;
                                optionsDetail.ContractSize = model.OptionsDetail.ContractSize;
                                optionsDetail.DerivativeProduct = model.OptionsDetail.DerivativeProduct;
                                optionsDetail.OptionsProductTypeId = model.OptionsDetail.OptionsProductTypeId;
                                optionsDetail.Category = model.OptionsDetail.Category;
                                optionsDetail.IsDeleted = false;
                                uow.Commit();
                                // Maintaining Log
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "OptionsDetail", ObjectID = optionsDetail.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                            }
                            else
                            {
                                var _optionsDetail = new OptionsDetail
                                {
                                    MarketId = model.OptionsDetail.MarketId,
                                    ASXCode = model.OptionsDetail.ASXCode,
                                    UnderlyingType = model.OptionsDetail.UnderlyingType,
                                    Underlying = model.OptionsDetail.Underlying,
                                    OptionsTypeId = model.OptionsDetail.OptionsTypeId,
                                    ExpDate = model.OptionsDetail.ExpDate,
                                    StrikePrice = model.OptionsDetail.StrikePrice,
                                    OptionsStyleId = model.OptionsDetail.OptionsStyleId,
                                    ContractSize = model.OptionsDetail.ContractSize,
                                    DerivativeProduct = model.OptionsDetail.DerivativeProduct,
                                    OptionsProductTypeId = model.OptionsDetail.OptionsProductTypeId,
                                    Category = model.OptionsDetail.Category,
                                    SecurityId = security.Id
                                };
                                _optionDetailRepository.Add(_optionsDetail);
                                uow.Commit();
                                // Maintaining Log
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "OptionsDetail", ObjectID = _optionsDetail.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                            }

                        }

                        if (security.Id > 0 && model.UnitisedId == 3)//For Term Deposite
                        {
                            var isTermDepositeExist = _termDepositeRepository.Where(w => w.Id == model.TermDeposite.Id).FirstOrDefault();
                            if (isTermDepositeExist != null)
                            {
                                isTermDepositeExist.InstitutionId = model.TermDeposite.InstitutionId;
                                isTermDepositeExist.MaxDeposit = model.TermDeposite.MaxDeposite;
                                isTermDepositeExist.SecurityId = security.Id;
                                isTermDepositeExist.MinDeposit = model.TermDeposite.MinDeposite;
                                isTermDepositeExist.Term = security.SecurityCategoryId != 35 ? model.TermDeposite.Term : "AtCall";
                                isTermDepositeExist.ProviderRateBrokerage = model.TermDeposite.ProviderRateBrokerage;
                                isTermDepositeExist.ProviderSettlementType = model.TermDeposite.ProviderSettlementType;
                                isTermDepositeExist.BrokerId = model.TermDeposite.BrokerId;
                                isTermDepositeExist.ClientTypeId = (model.SecurityCategoryId==35?model.ClientAccountTypeId:null);
                                uow.Commit();
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "TermDeposit", ObjectID = isTermDepositeExist.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                            }
                            else
                            {
                                var termDeposite = new TermDeposit()
                                {
                                    InstitutionId = model.TermDeposite.InstitutionId,
                                    MaxDeposit = model.TermDeposite.MaxDeposite,
                                    SecurityId = security.Id,
                                    MinDeposit = model.TermDeposite.MinDeposite,
                                    Term = model.TermDeposite.Term,
                                    ProviderRateBrokerage = model.TermDeposite.ProviderRateBrokerage,
                                    ProviderSettlementType = model.TermDeposite.ProviderSettlementType,
                                    BrokerId = model.TermDeposite.BrokerId,
                                    ClientTypeId = model.SecurityCategoryId==35?model.ClientAccountTypeId:null
                                };
                                _termDepositeRepository.Add(termDeposite);
                                uow.Commit();
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "TermDeposit", ObjectID = termDeposite.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                            }
                        }

                        if (model.AssetClassIds != null)
                        {
                            foreach (var assetClassId in model.AssetClassIds)
                            {
                                if (assetClassId != null)
                                {
                                    var assetClass =
                                        security.SecurityAssetClasses.FirstOrDefault(a => a.AssetClassId == assetClassId);
                                    if (assetClass != null)
                                    {
                                        //update
                                        assetClass.SecurityId = model.Id;
                                        assetClass.AssetClassId = assetClassId;
                                        assetClass.IsDeleted = false;
                                        uow.Commit();
                                        // Maintaining Log
                                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "SecurityAssetClass", ObjectID = assetClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                                        if (deletedAssetClassList.ContainsKey(assetClass.Id))
                                        {
                                            deletedAssetClassList[assetClass.Id] = assetClass.IsDeleted;
                                        }
                                    }
                                    else
                                    {
                                        //add
                                        var secAssetClass = new SecurityAssetClass
                                        {
                                            SecurityId = model.Id,
                                            AssetClassId = assetClassId,
                                        };
                                        _securityAssetClassRepository.Add(secAssetClass);
                                        uow.Commit();
                                        // Maintaining Log
                                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "SecurityAssetClass", ObjectID = secAssetClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                    }

                                }

                            }

                            foreach (var item in deletedAssetClassList.Where(w => w.Value == true))
                            {
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "SecurityAssetClass", ObjectID = item.Key, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });
                            }
                        }

                        #region HoldingDetail_ToDo

                        //TODO to be change
                        //var securityHoldingMDA = _securityHoldingDetail.GetAll(w => w.SecurityHoldingTypeId == 1 && w.SecurityId == model.Id).FirstOrDefault();
                        //if (securityHoldingMDA != null)
                        //{
                        //    securityHoldingMDA.HoldingLimit = model.MDAHoldingLimit;
                        //    securityHoldingMDA.IsApproved = model.MDAIsApproved;
                        //    securityHoldingMDA.SecurityId = security.Id;
                        //    securityHoldingMDA.SecurityHoldingTypeId = 1;
                        //    securityHoldingMDA.UpdatedOn = model.UpdatedOn ?? DateTime.Now;
                        //    securityHoldingMDA.UpdatedBy = model.UpdatedBy ?? SessionObject.GetInstance.UserID;
                        //}
                        //else
                        //{
                        //    securityHoldingMDA = new SecurityHoldingDetail()
                        //    {
                        //        HoldingLimit = model.MDAHoldingLimit,
                        //        IsApproved = model.MDAIsApproved,
                        //        SecurityId = security.Id,
                        //        SecurityHoldingTypeId = 1,
                        //        CreatedOn = model.CreatedOn ?? DateTime.Now,
                        //        CreatedBy = model.CreatedBy ?? SessionObject.GetInstance.UserID,
                        //    };
                        //    _securityHoldingDetail.Add(securityHoldingMDA);
                        //}


                        //var securityHoldingSuper = _securityHoldingDetail.GetAll(w => w.SecurityHoldingTypeId == 2 && w.SecurityId == model.Id).FirstOrDefault();
                        //if (securityHoldingSuper != null)
                        //{
                        //    securityHoldingSuper.HoldingLimit = model.SuperHoldingLimit;
                        //    securityHoldingSuper.IsApproved = model.SuperIsApproved;
                        //    securityHoldingSuper.SecurityId = security.Id;
                        //    securityHoldingSuper.SecurityHoldingTypeId = 2;
                        //    securityHoldingSuper.UpdatedOn = model.UpdatedOn ?? DateTime.Now;
                        //    securityHoldingSuper.UpdatedBy = model.UpdatedBy ?? SessionObject.GetInstance.UserID;
                        //}
                        //else
                        //{
                        //    securityHoldingSuper = new SecurityHoldingDetail()
                        //    {
                        //        HoldingLimit = model.SuperHoldingLimit,
                        //        IsApproved = model.SuperIsApproved,
                        //        SecurityId = security.Id,
                        //        SecurityHoldingTypeId = 2,
                        //        CreatedOn = model.CreatedOn ?? DateTime.Now,
                        //        CreatedBy = model.CreatedBy ?? SessionObject.GetInstance.UserID,
                        //    };
                        //    _securityHoldingDetail.Add(securityHoldingSuper);
                        //}

                        #endregion

                        //uow.Commit();
                        uow.Dispose();
                        responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Security");
                    }
                    else
                    {
                        responseModel.Status = ResponseStatus.Failure;
                        responseModel.Message = "Security already exists.";
                    }
                }
                else
                {
                    responseModel.Status = ResponseStatus.Failure;
                    responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "Security");
                }

            }
            catch (Exception exception)
            {
                responseModel.Status = ResponseStatus.Error;
                responseModel.Error =
                    new ErrorModel
                    {
                        Description = MessageManager.GetReponseMessage(this, Option.Update, responseModel.Status),
                        ErrorMessage = exception.Message,
                        Exception = exception
                    };
                //Adding log.
                LogUtility.LogException(exception, string.Format(MDA_Resource.Error_UnableToUpdate));
            }
            // var json = ModelUtilities.SerializeModel(responseModel);
            return responseModel;
        }

        private IViewModel DeleteSecurity(IViewModel baseModel)
        {
            var model = (SecurityModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success
            };
            try
            {
                var security = _securityRepository.GetById(model.Id);
                if (security != null)
                {
                    security.IsDeleted = true;

                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Security", ObjectID = security.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });
                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Security");
                }
                else
                {
                    responseModel.Status = ResponseStatus.Failure;
                    responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "Security");
                }

            }
            catch (Exception exception)
            {
                responseModel.Status = ResponseStatus.Error;
                responseModel.Error =
                    new ErrorModel
                    {
                        Description = MessageManager.GetReponseMessage(this, Option.Delete, responseModel.Status),
                        ErrorMessage = exception.Message,
                        Exception = exception
                    };
                //Adding log.
                LogUtility.LogException(exception, string.Format(MDA_Resource.Error_UnableToUpdate));
            }
            // var json = ModelUtilities.SerializeModel(responseModel);
            return responseModel;
        }

        public List<SecurityModel> GetTermDepositPaginatedData(SecurityModel model, out int totalTermDepositRecords)
        {
            List<SecurityModel> query;
            if (model.Term.Count>0)
            {
                var GetFilteredTerm = (from filteredlist in model.FilteredTermList
                                join termlist in model.Term on filteredlist.Text equals termlist
                                select new
                                {
                                    Text=filteredlist.Text,
                                    Value = System.Convert.ToInt32(filteredlist.Value)
                                }).ToList();
                var LowRange = System.Convert.ToInt32(GetFilteredTerm[0].Value);
                var Highrange = System.Convert.ToInt32(GetFilteredTerm[1].Value);

                model.Term = model.FilteredIntTermList.Where(a => System.Convert.ToInt32(a.Value) >= LowRange && System.Convert.ToInt32(a.Value) <= Highrange).Select(a => a.Text).ToList();
                if (!model.SecurityCategoryId.HasValue || (model.SecurityCategoryId == 35))
                {
                    model.Term.Add("AtCall");
                }
              
            }
            model.TermSortList = model.FilteredIntTermList.Select(a => new TermSortModel()
            {
                Term = a.Text,
                index = System.Convert.ToInt32(a.Value)
            }).ToList();
            model.TermSortList.Add(new TermSortModel(){Term = "AtCall",index=19});
            query = _securityRepository.GetAll(w => w.IsDeleted == false && w.UnitisedId == 3).Select(s => new SecurityModel
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                SecurityCategory = s.SecurityCategory != null ? s.SecurityCategory.Category : "",
                SecurityCategoryId = s.SecurityCategoryId,
                IntrestRate = s.SecurityPrices.Where(a=>a.IsDeleted==false).OrderByDescending(w => w.Date).FirstOrDefault().InterestRate,
                TermDeposite = s.TermDeposits.Where(w => w.SecurityId == s.Id).AsEnumerable().Select(a => new TermDeposite
                {
                    Id = a.Id,
                    InstitutionId = a.Institution.Id,
                    Institution = a.Institution.Description ?? string.Empty,
                    MinDeposite = a.MinDeposit ?? 0,
                    MaxDeposite = a.MaxDeposit ?? 0,
                    Term = a.Term ?? string.Empty,
                    Broker = a.BrokerId.HasValue?a.Institution1.Description:"",
                    InstituionFSGLink = a.Institution.ProviderFSGLink ?? "",
                    InstituionPDSLink = a.Institution.ProviderPDSLink ?? "",
                    InstitutionImage = a.Institution.InstitutionImage ?? new byte[] { },
                    ClientType = a.ClientTypeId!=null?a.ClientAccountType.Description:""
                }).FirstOrDefault(),
          
            }).Where(s => ((!model.SecurityCategoryId.HasValue || s.SecurityCategoryId == model.SecurityCategoryId)
                             && (!model.AmountInvest.HasValue || (s.TermDeposite.MinDeposite <= model.AmountInvest && s.TermDeposite.MaxDeposite >= model.AmountInvest))
                            && (model.Term.Count <= 0 || (model.Term.Any(f => f == s.TermDeposite.Term)))
                             && (model.Institution.Count <= 0 || model.Institution.Any(f => f == s.TermDeposite.InstitutionId)))).OrderBy(od => od.Code)
                   .ToList();
            foreach (var item in query)
            {
                item.TermDeposite.TermSortIndex =model.TermSortList.FirstOrDefault(w => w.Term == item.TermDeposite.Term).index;
            }
            if (string.IsNullOrWhiteSpace(model.TableParam.sSearch))
            {
                query = query.Select(s => new SecurityModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    SecurityCategory = s.SecurityCategory,
                    SecurityCategoryId = s.SecurityCategoryId,
                    IntrestRate = s.IntrestRate,
                    TermDeposite = s.TermDeposite,
                    
                }).ToList();
            }
            else
            {
                query = query.Select(s => new SecurityModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    SecurityCategory = s.SecurityCategory,
                    SecurityCategoryId = s.SecurityCategoryId,
                    IntrestRate = s.IntrestRate,
                    TermDeposite = s.TermDeposite
                }).Where(s => (s.TermDeposite != null && s.TermDeposite.Institution.ToLower().Contains(model.TableParam.sSearch.ToLower()))
                              || (s.TermDeposite != null && s.TermDeposite.MinDeposite.ToString().ToLower().Contains(model.TableParam.sSearch.ToLower()))
                              || (s.TermDeposite != null && s.TermDeposite.MaxDeposite.ToString().ToLower().Contains(model.TableParam.sSearch.ToLower()))
                              || (s.TermDeposite != null && s.TermDeposite.Term.ToLower().Contains(model.TableParam.sSearch.ToLower()))
                              || (s.SecurityCategory != null && s.SecurityCategory.ToLower().Contains(model.TableParam.sSearch.ToLower()))
                              || (s.IntrestRate != null && s.IntrestRate.ToString().ToLower().Contains(model.TableParam.sSearch.ToLower()))
                              ).OrderBy(od => od.Code)
                    .ToList();
            }

            List<SecurityModel> filtered = CustomTermDepositFilter(model, query);
            totalTermDepositRecords = filtered.Count;

            return model.IsDownload ? filtered : SortingTermDepositTableData(model, filtered);

        }
        private static List<SecurityModel> SortingTermDepositTableData(SecurityModel model, List<SecurityModel> filtered)
        {
            if (model.TableParam.iSortingCols == 1)
            {

                if (model.TableParam.iSortCol_0 == 1 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.Code).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 1 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.Code).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(od => od.TermDeposite.Institution).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(od => od.TermDeposite.Institution).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }


                if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.SecurityCategory).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.SecurityCategory).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 4 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.TermDeposite.MinDeposite).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 4 && model.TableParam.sSortDir_0 == "desc")
                {
                    return
                        filtered.OrderByDescending(p => p.TermDeposite.MinDeposite)
                            .Skip(model.TableParam.iDisplayStart)
                            .Take(model.TableParam.iDisplayLength)
                            .ToList();
                }

                if (model.TableParam.iSortCol_0 == 5 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.TermDeposite.MaxDeposite).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 5 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.TermDeposite.MaxDeposite).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 6 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => (p.TermDeposite.TermSortIndex)).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 6 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => (p.TermDeposite.TermSortIndex)).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 7 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.IntrestRate).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 7 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.IntrestRate).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 8 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.TermDeposite.Broker).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 8 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.TermDeposite.Broker).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 9 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.TermDeposite.ClientType).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 9 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.TermDeposite.ClientType).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }



                if (model.TableParam.iSortCol_0 == 10 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.TermDeposite.InstituionFSGLink).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 10 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.TermDeposite.InstituionFSGLink).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 11 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.TermDeposite.InstituionPDSLink).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 11 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.TermDeposite.InstituionPDSLink).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

               
                return filtered.OrderByDescending(x => x.SecurityCategory).ThenBy(x=>x.TermDeposite.Term).ThenBy(x=>x.TermDeposite.Institution).ThenBy(x=>x.TermDeposite.MaxDeposite).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
            }
            return filtered.OrderByDescending(x => x.SecurityCategory).ThenBy(x => x.TermDeposite.Term).ThenBy(x => x.TermDeposite.Institution).ThenBy(x => x.TermDeposite.MaxDeposite).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
        }

        private static List<SecurityModel> CustomTermDepositFilter(SecurityModel model, List<SecurityModel> query)
        {
            List<SecurityModel> filtered = query;

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_1))
            {
                filtered = filtered.Where(s => s.Code.ToLower().Contains(model.TableParam.sSearch_1.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_2))
            {
                filtered = filtered.Where(s => s.TermDeposite != null && s.TermDeposite.Institution.ToLower().Contains(model.TableParam.sSearch_2.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_3))
            {
                filtered = filtered.Where(s => s.SecurityCategory != null && s.SecurityCategory.ToString().ToLower().Contains(model.TableParam.sSearch_3.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_4))
            {
                filtered = filtered.Where(s => s.TermDeposite != null && s.TermDeposite.MinDeposite.ToString().ToLower().Contains(model.TableParam.sSearch_4.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_5))
            {
                filtered = filtered.Where(s => s.TermDeposite != null && s.TermDeposite.MaxDeposite.ToString().ToLower().Contains(model.TableParam.sSearch_5.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_6))
            {
                filtered = filtered.Where(s => s.TermDeposite != null && s.TermDeposite.Term.ToLower().Contains(model.TableParam.sSearch_6.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_7))
            {
                filtered = filtered.Where(s => s.IntrestRate != null && s.IntrestRate.ToString().ToLower().Contains(model.TableParam.sSearch_7.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_8))
            {
                filtered = filtered.Where(s => s.TermDeposite != null && s.TermDeposite.Broker.ToLower().Contains(model.TableParam.sSearch_8.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_9))
            {
                filtered = filtered.Where(s => s.TermDeposite != null && s.TermDeposite.ClientType.ToLower().Contains(model.TableParam.sSearch_9.ToLower())).ToList();
            }
            return filtered;
        }
    }
}