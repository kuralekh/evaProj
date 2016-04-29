using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Invest.Common;
using Invest.Common.Enumerators;
using Invest.Common.Extensions;
using Invest.Database;
using Invest.ViewModel;
using Invest.ViewModel.Models;

namespace Invest.Service.Components
{
    public class ProductComponent : BaseComponent
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductBroker> _productBrokeRepository;
        private readonly IRepository<ProductAssociation> _productAssociationRepository;
        private readonly IRepository<ProductVersion> _productVersionRepository;
        private readonly IRepository<ProductReturn> _productReturnRepository;
        private readonly IRepository<ProductStatistic> _productStatisticRepository;
        private readonly IRepository<Model> _modelRepository;
        private readonly IRepository<ModelDetail> _modelDetailRepository;
        private readonly IRepository<ModelVersion> _modelVersionRepository;
        private readonly IRepository<ModelAFSLLicenseeAssociation> _modelAFSLLicenseeAssociationRepositry;

        private readonly IRepository<InvestmentProgram> _investmentProgramRepository;
        private readonly IRepository<ProductVersionInvestmentProgram> _productVersionInvestmentProgramRepository;

        private readonly IRepository<InvestmentProgramTemplate> _investmentProgramTemplateRepository;
        private readonly IRepository<ModelVersionInvestmentProgramTemplate> _modelVersionInvestmentProgramTemplateRepository;
        private readonly IRepository<InvestmentProgramStrategicAllocationTemplate> _investmentProgramStrategicAllocationTemplateRepository;
        private readonly IRepository<InvestmentProgramScreenTemplate> _investmentProgramScreenTemplateRepository;
        private readonly IRepository<InvestmentProgramConstraintTemplate> _investmentProgramConstraintTemplateRepository;
        private readonly IRepository<SecuritiesInvestmentProgramTemplate> _securityInvestmentProgramTemplateRepository;
        private readonly IRepository<ProductVersionInvestmentProgramTemplate> _productVersionInvestmentProgramTemplateRepository;
        private readonly IRepository<InvestmentProgramDefaultSecuritiesTemplate> _investmentProgramDefaultSecuritiesTemplateRepository;

        private static List<ProductAssociationModel> GetAssociationModel(ICollection<ProductAssociation> collection)
        {
            List<ProductAssociationModel> result = new List<ProductAssociationModel>();

            foreach (var item in collection)
            {
                ProductAssociationModel version = new ProductAssociationModel();
                version.Allocation = item.Allocation ?? 0;
                version.SecurityId = item.SecurityId;
                version.SecurityListId = item.SecurityListId;
                version.IsDeleted = item.IsDeleted;
                if (item.Security != null)
                    version.Security = new SecurityModel { Code = item.Security.Code };
                result.Add(version); 
            }
            return result;
        }

        private static List<ProductVersionModel> GetVersions(ICollection<ProductVersion> collection)
        {
            List<ProductVersionModel> result = new List<ProductVersionModel>();

            foreach (var item in collection)
            {
                ProductVersionModel version = new ProductVersionModel
                {
                    ProductVersionID = item.ProductVersionID,
                    ProductVersionStatus = item.SecurityStatu.Status,
                    PrimaryBenchmarkProductID = item.PrimaryBenchmarkProductID,
                    SecondaryBenchmarkProductID = item.SecondaryBenchmarkProductID,
                    PrimaryPriceTypeId = item.PrimaryPriceTypeId,
                    SecondaryPriceTypeId = item.SecondaryPriceTypeId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    MajorVersion = item.MajorVersion,
                    MinorVersion = item.MinorVersion,
                    ProductAssociationList = GetAssociationModel(item.ProductAssociations)
                };

                result.Add(version); 
            }

            return result;
        }

        public static ProductModel FromProduct(Product source)
        {
            ProductModel result = new ProductModel
            {
                ProductID = source.ProductID,
                CurrencyId = source.CurrencyId,
                Code = source.Code,
                Name = source.Name,
                InceptionDate = source.InceptionDate,
                ProductType = source.ProductType.Type,
                ProductVersionList = GetVersions(source.ProductVersions)
            };

            return result;
        }

        public ProductComponent()
        {
            _productRepository = uow.Repository<Product>();
            _productBrokeRepository = uow.Repository<ProductBroker>();
            _productAssociationRepository = uow.Repository<ProductAssociation>();
            _productVersionRepository = uow.Repository<ProductVersion>();
            _productReturnRepository = uow.Repository<ProductReturn>();
            _productStatisticRepository = uow.Repository<ProductStatistic>();
            _modelRepository = uow.Repository<Model>();
            _modelDetailRepository = uow.Repository<ModelDetail>();
            _modelVersionRepository = uow.Repository<ModelVersion>();
            _modelAFSLLicenseeAssociationRepositry = uow.Repository<ModelAFSLLicenseeAssociation>();

            _investmentProgramRepository = uow.Repository<InvestmentProgram>();
            _productVersionInvestmentProgramRepository = uow.Repository<ProductVersionInvestmentProgram>();

            _investmentProgramTemplateRepository = uow.Repository<InvestmentProgramTemplate>();
            _modelVersionInvestmentProgramTemplateRepository = uow.Repository<ModelVersionInvestmentProgramTemplate>();
            _investmentProgramStrategicAllocationTemplateRepository = uow.Repository<InvestmentProgramStrategicAllocationTemplate>();
            _investmentProgramScreenTemplateRepository = uow.Repository<InvestmentProgramScreenTemplate>();
            _investmentProgramConstraintTemplateRepository = uow.Repository<InvestmentProgramConstraintTemplate>();
            _securityInvestmentProgramTemplateRepository = uow.Repository<SecuritiesInvestmentProgramTemplate>();
            _productVersionInvestmentProgramTemplateRepository = uow.Repository<ProductVersionInvestmentProgramTemplate>();
            _investmentProgramDefaultSecuritiesTemplateRepository = uow.Repository<InvestmentProgramDefaultSecuritiesTemplate>();
        }

        private IViewModel GetProductTypeDetail(IViewModel baseModel, FilterOption filterOption)
        {
            var model = (ProductModel)baseModel;
            var responseModel = new ResponseModel<ProductModel>
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
                    case FilterOption.GetAssociatedData:
                        responseModel.ModelList = GetAssociatedSecData(model);
                        break;
                    case FilterOption.GetAllPagination:
                        int totalRecords;
                        responseModel.ModelList = GetPaginatedData(model, out totalRecords);
                        responseModel.RecordCount = totalRecords;
                        break;
                    case FilterOption.GetAllForDropdown:
                        responseModel.ModelList = GetAllModelDropDownList(model);
                        break;
                    case FilterOption.GetFilteredSecuritiesByAssetClass:
                        responseModel.ModelList = GetSecurityListByAssetClass(model);
                        break;
                    case FilterOption.GetFilteredDropdown:
                        responseModel.ModelList = GetFilteredDropDownList(model);
                        break;
                    case FilterOption.GetProductByProductVersionFieldsDropdown:
                        responseModel.ModelList = GetAllProductVersionDropDownList(true, model);
                        break;
                    case FilterOption.GetProductByProductVersionFields:
                        responseModel.Model = GetProductByProductVersionFields(model);
                        break;
                    case FilterOption.GetAllVersions:
                        responseModel.ModelList = GetAllVersions(model);
                        break;
                    case FilterOption.GetDashboard:
                        responseModel.ModelList = GetProductDashboard(model);
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
            return responseModel;
        }

        public override IViewModel Get(IViewModel model, FilterOption filterOption)
        {
            return GetProductTypeDetail(model, filterOption);
        }

        public override IViewModel Add(IViewModel model)
        {
            return AddProduct(model);
        }

        public override IViewModel Update(IViewModel model)
        {
            return UpdateProduct(model);
        }

        public override IViewModel Delete(IViewModel model)
        {
            return DeleteProduct(model);
        }

        protected List<ProductModel> GetModelListByModelFields(ProductModel modelWithValue)
        {
            var productVersion = _productVersionRepository.Where(w => (w.StatusID == 1 || w.StatusID == 4) && (!w.IsDeleted) && w.Product.IsDeleted == false).ToList();
            var productIds = productVersion.GroupBy(m => m.ProductID).Select(group => @group.First()).Select(item => item.ProductID).ToList();

            var result = _productRepository.GetAll(w => w.IsDeleted == false).Where(w => productIds.Contains(w.ProductID)).Where(Createfilter<Product>(modelWithValue))
                .Select(p =>
                    new ProductModel
                    {
                        ProductID = p.ProductID,
                        Code = p.Code,
                        Name = p.Name,
                        ProductTypeId = p.ProductTypeId,
                        ProductType = p.ProductType.Type,
                        IndexTypeId = p.IndexTypeId,
                        IndexType = p.IndexType.Type,
                        ProductAPIR = p.ProductAPIR,
                        ProductISIN = p.ProductISIN,
                        InstitutionId = p.InstitutionId,
                        Institution = p.Institution.Code,
                        MarketId = p.MarketId,
                        Market = p.Market.Code,
                        CurrencyId = p.CurrencyId,
                        Currency = p.Currency.Code,
                        SubAssetClassId = p.SubAssetClassId,
                        SubAssetClass = p.SubAssetClass.Class,
                        RegionId = p.RegionId,
                        Region = p.Region.RegionName,
                        StatusId = p.StatusId,
                        Status = p.SecurityStatu.Status,
                        AssetClassId = p.AssetClassId,
                        Description = p.Description,
                        ProductBrokerIds = p.ProductBrokers.Where(w => w.IsDeleted == false).Select(r => r.InstitutionId).ToList(),
                        SecurityAssociation = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductAssociations.Where(w => w.IsDeleted == false && (w.SecurityId != null || w.SecurityListId != null)).Select(r => new ProductSecurityAssociationModel
                        {
                            SecurityId = r.SecurityId,
                            SecurityListId = r.SecurityListId,
                            SecurityCode = r.Security.Code,
                            SecurityName = r.Security.Name ?? r.SecurityList.Name,
                            Allocation = r.Allocation

                        }).ToList() : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductAssociations.Where(w => w.IsDeleted == false && (w.SecurityId != null || w.SecurityListId != null)).Select(r => new ProductSecurityAssociationModel
                        {
                            SecurityId = r.SecurityId,
                            SecurityListId = r.SecurityListId,
                            SecurityCode = r.Security.Code,
                            SecurityName = r.Security.Name ?? r.SecurityList.Name,
                            Allocation = r.Allocation
                        }).ToList()) : new List<ProductSecurityAssociationModel>(),

                        SecurityListIds = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductAssociations.Where(w => w.IsDeleted == false && w.SecurityListId != null).Select(r => r.SecurityListId).ToList()
                        : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductAssociations.Where(w => w.IsDeleted == false && w.SecurityListId != null).Select(r => r.SecurityListId).ToList()) : new List<int?>(),


                        VersionDetail = new ProductVersionModel
                        {
                            ProductICR = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? (p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityMER + p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductMER) * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityMER + p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductMER) * 100 : 0,
                            ProductReimbursable = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? (p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductReimbursable) * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductReimbursable) * 100 : 0,
                            StartDate = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).StartDate : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().StartDate : null,
                            IsPriced = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).IsPriced : p.ProductVersions.Any(ww => ww.StatusID == 4) && p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().IsPriced,
                            PricingSourceId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PricingSourceID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PricingSourceID : null,
                            PricingSource = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PricingSource.Source : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PricingSource.Source : string.Empty,
                            SecurityMER = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityMER * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityMER * 100 : null,
                            ProductMER = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductMER * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductMER * 100 : null,
                            IsOnlyVersion = p.ProductVersions.Count(ww => ww.ProductID == p.ProductID && ww.IsDeleted == false) <= 1,
                            StatusID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).StatusID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().StatusID : null,
                            ProductVersionStatus = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityStatu.Status : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityStatu.Status : null,
                            PrimaryBenchmarkProductID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PrimaryBenchmarkProductID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PrimaryBenchmarkProductID : null,
                            SecondaryBenchmarkProductID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecondaryBenchmarkProductID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecondaryBenchmarkProductID : null,
                            PrimaryPriceTypeId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PrimaryPriceTypeId : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PrimaryPriceTypeId : null,
                            SecondaryPriceTypeId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecondaryPriceTypeId : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecondaryPriceTypeId : null,
                            TargetReturnRate = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).TargetReturnRate : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().TargetReturnRate : null,

                        }
                    }).ToList();
            return result;
        }

        protected List<ProductModel> GetAllModelList()
        {
            var productVersion = _productVersionRepository.Where(w => (w.StatusID == 1 || w.StatusID == 4) && (!w.IsDeleted) && w.Product.IsDeleted == false).ToList();
            var productIds = productVersion.GroupBy(m => m.ProductID).Select(group => @group.First()).Select(item => item.ProductID).ToList();

            var result = _productRepository.GetAll(w => w.IsDeleted == false && productIds.Contains(w.ProductID))
                .Select(p => new ProductModel
                {
                    ProductID = p.ProductID,
                    Code = p.Code,
                    Name = p.Name,
                    ProductTypeId = p.ProductTypeId,
                    ProductType = p.ProductType.Type,
                    IndexTypeId = p.IndexTypeId,
                    IndexType = p.IndexType.Type,
                    ProductAPIR = p.ProductAPIR,
                    ProductISIN = p.ProductISIN,
                    InstitutionId = p.InstitutionId,
                    Institution = p.Institution.Code,
                    MarketId = p.MarketId,
                    Market = p.Market.Code,
                    CurrencyId = p.CurrencyId,
                    Currency = p.Currency.Code,
                    SubAssetClassId = p.SubAssetClassId,
                    SubAssetClass = p.SubAssetClass.Class,
                    RegionId = p.RegionId,
                    Region = p.Region.RegionName,
                    StatusId = p.StatusId,
                    Status = p.SecurityStatu.Status,
                    Description = p.Description,
                    ProductBrokerIds = p.ProductBrokers.Where(w => w.IsDeleted == false).Select(r => r.InstitutionId).ToList(),
                    SecurityAssociation = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductAssociations.Where(w => w.IsDeleted == false && (w.SecurityId != null || w.SecurityListId != null)).Select(r => new ProductSecurityAssociationModel
                    {
                        SecurityId = r.SecurityId,
                        SecurityListId = r.SecurityListId,
                        SecurityCode = r.Security.Code,
                        SecurityName = r.Security.Name ?? r.SecurityList.Name,
                        Allocation = r.Allocation

                    }).ToList() : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductAssociations.Where(w => w.IsDeleted == false && (w.SecurityId != null || w.SecurityListId != null)).Select(r => new ProductSecurityAssociationModel
                    {
                        SecurityId = r.SecurityId,
                        SecurityListId = r.SecurityListId,
                        SecurityCode = r.Security.Code,
                        SecurityName = r.Security.Name ?? r.SecurityList.Name,
                        Allocation = r.Allocation
                    }).ToList()) : new List<ProductSecurityAssociationModel>(),

                    SecurityListIds = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductAssociations.Where(w => w.IsDeleted == false && w.SecurityListId != null).Select(r => r.SecurityListId).ToList()
                    : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductAssociations.Where(w => w.IsDeleted == false && w.SecurityListId != null).Select(r => r.SecurityListId).ToList()) : new List<int?>(),

                    VersionDetail = new ProductVersionModel
                    {
                        ProductICR = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? (p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityMER + p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductMER) * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityMER + p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductMER) * 100 : 0,
                        ProductReimbursable = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? (p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductReimbursable) * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductReimbursable) * 100 : 0,
                        StartDate = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).StartDate : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().StartDate : null,
                        IsPriced = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).IsPriced : p.ProductVersions.Any(ww => ww.StatusID == 4) && p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().IsPriced,
                        PricingSourceId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PricingSourceID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PricingSourceID : null,
                        PricingSource = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PricingSource.Source : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PricingSource.Source : string.Empty,
                        SecurityMER = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityMER * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityMER * 100 : null,
                        ProductMER = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductMER * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductMER * 100 : null,
                        IsOnlyVersion = p.ProductVersions.Count(ww => ww.ProductID == p.ProductID && ww.IsDeleted == false) <= 1,
                        StatusID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).StatusID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().StatusID : null,
                        ProductVersionStatus = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityStatu.Status : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityStatu.Status : null,
                        PrimaryBenchmarkProductID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PrimaryBenchmarkProductID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PrimaryBenchmarkProductID : null,
                        SecondaryBenchmarkProductID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecondaryBenchmarkProductID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecondaryBenchmarkProductID : null,
                        PrimaryPriceTypeId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PrimaryPriceTypeId : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PrimaryPriceTypeId : null,
                        SecondaryPriceTypeId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecondaryPriceTypeId : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecondaryPriceTypeId : null,
                        TargetReturnRate = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).TargetReturnRate : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().TargetReturnRate : null,
                    }
                }).ToList();
            return result;
        }

        protected List<ProductModel> GetAssociatedSecData(ProductModel modelWithValue)
        {
            IQueryable<Product> product = _productRepository.GetAll(w => w.IsDeleted == false && w.ProductID == modelWithValue.ProductID);
            var result = product
                .Select(p => new ProductModel
                    {
                        SecurityAssociation = p.ProductVersions.FirstOrDefault(pv => pv.ProductID == modelWithValue.ProductID && pv.ProductVersionID == modelWithValue.VersionDetail.ProductVersionID).ProductAssociations.Where(w => w.IsDeleted == false && (w.SecurityId != null || w.SecurityListId != null)).Select(r => new ProductSecurityAssociationModel
                        {
                            SecurityId = r.SecurityId,
                            SecurityListId = r.SecurityListId,
                            SecurityCode = r.Security.Code,
                            SecurityName = r.Security.Name ?? r.SecurityList.Name,
                            Allocation = r.Allocation
                        }).ToList(),

                    }).ToList();
            return result;
        }

        protected ProductModel GetModelByModelFields(ProductModel modelWithValue)
        {
            var productVersion = _productVersionRepository.Where(w => (w.StatusID == 1 || w.StatusID == 4) && (!w.IsDeleted) && w.Product.IsDeleted == false).ToList();
            var productIds = productVersion.GroupBy(m => m.ProductID).Select(group => @group.First()).Select(item => item.ProductID).ToList();
            var returns = _productReturnRepository.GetAll();
            var statistics = _productStatisticRepository.GetById(modelWithValue.ProductID);

            var resultFull = _productRepository.GetAll(w => w.IsDeleted == false && productIds.Contains(w.ProductID)).Where(Createfilter<Product>(modelWithValue)).FirstOrDefault();

            var result = _productRepository.GetAll(w => w.IsDeleted == false && productIds.Contains(w.ProductID)).Where(Createfilter<Product>(modelWithValue))
                .Select(p =>
                    new ProductModel
                    {
                        ProductID = p.ProductID,
                        Code = p.Code,
                        Name = p.Name,
                        ProductTypeId = p.ProductTypeId,
                        ProductType = p.ProductType.Type,
                        IndexTypeId = p.IndexTypeId,
                        IndexType = p.IndexType.Type,
                        ProductAPIR = p.ProductAPIR,
                        ProductISIN = p.ProductISIN,
                        InstitutionId = p.InstitutionId,
                        Institution = p.Institution.Code,
                        MarketId = p.MarketId,
                        Market = p.Market.Code,
                        CurrencyId = p.CurrencyId,
                        Currency = p.Currency.Code,
                        SubAssetClassId = p.SubAssetClassId,
                        SubAssetClass = p.SubAssetClass.Class,
                        RegionId = p.RegionId,
                        Region = p.Region.RegionName,
                        StatusId = p.StatusId,
                        Status = p.SecurityStatu.Status,
                        Description= p.Description,
                        AssetClassId = p.AssetClassId,
                        ProductBrokerIds = p.ProductBrokers.Where(w => w.IsDeleted == false).Select(r => r.InstitutionId).ToList(),
                        YearReturn = returns.Where(item => item.ProductId == p.ProductID).Select(i => i.YearReturn == null ? i.YearReturn : Math.Round(i.YearReturn.Value * 100, 4)).FirstOrDefault(),
                        MonthReturn = returns.Where(item => item.ProductId == p.ProductID).Select(i => i.MonthReturn == null ? i.MonthReturn : Math.Round(i.MonthReturn.Value * 100, 4)).FirstOrDefault(),
                        VersionDetail = new ProductVersionModel
                        {
                            ProductVersionID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductVersionID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductVersionID : 0,
                            ProductICR = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? (p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityMER + p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductMER) * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityMER + p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductMER) * 100 : 0,
                            ProductReimbursable = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? (p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductReimbursable) * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductReimbursable) * 100 : 0,
                            StartDate = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).StartDate : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().StartDate : null,
                            IsPriced = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).IsPriced : p.ProductVersions.Any(ww => ww.StatusID == 4) && p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().IsPriced,
                            PricingSourceId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PricingSourceID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PricingSourceID : null,
                            PricingSource = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PricingSource.Source : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PricingSource.Source : string.Empty,
                            SecurityMER = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityMER * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityMER * 100 : null,
                            ProductMER = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductMER * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductMER * 100 : null,
                            IsOnlyVersion = p.ProductVersions.Count(ww => ww.ProductID == p.ProductID && ww.IsDeleted == false) <= 1,
                            StatusID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).StatusID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().StatusID : null,
                            ProductVersionStatus = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityStatu.Status : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityStatu.Status : null,
                            PrimaryBenchmarkProductID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PrimaryBenchmarkProductID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PrimaryBenchmarkProductID : null,
                            SecondaryBenchmarkProductID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecondaryBenchmarkProductID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecondaryBenchmarkProductID : null,
                            PrimaryPriceTypeId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PrimaryPriceTypeId : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PrimaryPriceTypeId : null,
                            SecondaryPriceTypeId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecondaryPriceTypeId : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecondaryPriceTypeId : null,
                            TargetReturnRate = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).TargetReturnRate : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().TargetReturnRate : null,
                        },
                    }).FirstOrDefault();
            if (result != null)
            {
                if (resultFull.ProductStatistic != null)
                {
                    ProductStatisticModel statistic = new ProductStatisticModel
                        {
                            ProductId = resultFull.ProductStatistic.ProductId,
                            Volatility = resultFull.ProductStatistic.Volatility * 100,
                            DividendYield = resultFull.ProductStatistic.DividendYield
                        };

                    result.Index = resultFull.ProductPrices.LastOrDefault().TRPrice;
                    result.Statistic = statistic;
                }

                result.SecurityListIds = _productAssociationRepository.Where(w => w.IsDeleted == false && w.ProductVersionID == result.VersionDetail.ProductVersionID)
                    .Select(r => r.SecurityListId).ToList();

                result.SecurityAssociation = _productAssociationRepository.Where(w => w.IsDeleted == false && w.ProductVersionID == result.VersionDetail.ProductVersionID && (w.SecurityId != null || w.SecurityListId != null))
                        .Select(r => new ProductSecurityAssociationModel
                        {
                            SecurityId = r.SecurityId,
                            SecurityListId = r.SecurityListId,
                            SecurityCode = r.Security.Code,
                            SecurityName = r.Security.Name ?? r.SecurityList.Name,
                            Allocation = r.Allocation

                        }).ToList();
            }
            return result;
        }

        protected Product GetProductById(ProductModel model)
        {
            Product productModel = _productRepository.Where(w => w.ProductID == 1163).FirstOrDefault();

            var product = _productRepository.Where(w => w.ProductID == model.ProductID).ToList();
            //ProductModel product = _productVersionRepository.Where(w => w.ProductID == model.ProductID).ToList();
            return productModel;
        }

        protected ProductModel GetProductByProductVersionFields(ProductModel modelWithValue)
        {
            var result = _productVersionRepository.Where(w => w.ProductVersionID == modelWithValue.BaseProductVersionID && w.Product.IsDeleted == false)
                .Select(p =>
                    new ProductModel
                    {
                        ProductID = p.Product.ProductID,
                        Code = p.Product.Code,
                        Name = p.Product.Name,
                        InceptionDate = p.Product.InceptionDate,
                        ProductTypeId = p.Product.ProductTypeId,
                        ProductType = p.Product.ProductType.Type,
                        IndexTypeId = p.Product.IndexTypeId,
                        IndexType = p.Product.IndexType.Type,
                        ProductAPIR = p.Product.ProductAPIR,
                        ProductISIN = p.Product.ProductISIN,
                        InstitutionId = p.Product.InstitutionId,
                        Institution = p.Product.Institution.Code,
                        MarketId = p.Product.MarketId,
                        Market = p.Product.Market.Code,
                        CurrencyId = p.Product.CurrencyId,
                        Currency = p.Product.Currency.Code,
                        SubAssetClassId = p.Product.SubAssetClassId,
                        SubAssetClass = p.Product.SubAssetClass.Class,
                        RegionId = p.Product.RegionId,
                        Region = p.Product.Region.RegionName,
                        StatusId = p.Product.StatusId,
                        Status = p.Product.SecurityStatu.Status,
                        Description = p.Product.Description,
                        AssetClassId = p.Product.AssetClassId,
                        ProductBrokerIds = p.Product.ProductBrokers.Where(w => w.IsDeleted == false).Select(r => r.InstitutionId).ToList(),
                        SecurityAssociation = p.ProductAssociations.Where(w => w.IsDeleted == false && (w.SecurityId != null || w.SecurityListId != null)).Select(r => new ProductSecurityAssociationModel
                        {
                            SecurityId = r.SecurityId,
                            SecurityListId = r.SecurityListId,
                            SecurityCode = r.Security.Code,
                            SecurityName = r.Security.Name ?? r.SecurityList.Name,
                            Allocation = r.Allocation
                        }).ToList(),
                        SecurityListIds = p.ProductAssociations.Where(w => w.IsDeleted == false && w.SecurityListId != null).Select(r => r.SecurityListId).ToList(),

                        VersionDetail = new ProductVersionModel
                        {
                            ProductICR = (p.SecurityMER + p.ProductMER) * 100,
                            ProductReimbursable = p.ProductReimbursable * 100,
                            StartDate = p.StartDate,
                            IsPriced = p.IsPriced,
                            PricingSourceId = p.PricingSourceID,
                            PricingSource = p.PricingSource.Source,
                            SecurityMER = p.SecurityMER * 100,
                            ProductMER = p.ProductMER * 100,
                            IsOnlyVersion = p.Product.ProductVersions.Count(ww => ww.ProductID == p.ProductID && ww.IsDeleted == false) <= 1,
                            StatusID = p.StatusID,
                            ProductVersionStatus = p.SecurityStatu.Status,
                            PrimaryBenchmarkProductID = p.PrimaryBenchmarkProductID,
                            SecondaryBenchmarkProductID = p.SecondaryBenchmarkProductID,
                            PrimaryPriceTypeId = p.PrimaryPriceTypeId,
                            SecondaryPriceTypeId = p.SecondaryPriceTypeId,
                            TargetReturnRate = p.TargetReturnRate
                        },


                    }).FirstOrDefault();
            
            return result;
        }

        public List<ProductModel> GetPaginatedData(ProductModel model, out int totalRecords)
        {
            List<ProductModel> query;
            if (string.IsNullOrWhiteSpace(model.TableParam.sSearch))
            {
                query = _productVersionRepository.GetAll(w => w.IsDeleted == false && w.Product.IsDeleted == false && (model.DisplayStatus == 0 || (model.DisplayStatus == 1 && (w.StatusID == 1 || w.StatusID == 4)) || (model.DisplayStatus == 2 && w.StatusID == 11))).Select(s => new ProductModel
                {

                    ProductID = s.Product.ProductID,
                    Code = s.Product.Code ?? "",
                    Name = s.Product.Name ?? "",
                    ProductType = s.Product.ProductType.Type,
                    AssetClassId = s.Product.AssetClassId,
                    IsWarning = false,
                    SecurityAssociation = s.ProductAssociations.Where(w => w.IsDeleted == false && (w.SecurityId != null || w.SecurityListId != null)).Select(r => new ProductSecurityAssociationModel
                    {
                        SecurityId = r.SecurityId,
                        SecurityListId = r.SecurityListId,
                        SecurityCode = r.Security.Code,
                        SecurityName = r.Security.Name ?? r.SecurityList.Name,
                        Allocation = r.Allocation

                    }).ToList(),
                    VersionDetail = new ProductVersionModel
                    {
                        ProductVersionID = s.ProductVersionID,
                        CombineVersion = s.MajorVersion + "." + s.MinorVersion,
                        StatusID = s.StatusID,
                        ProductVersionStatus = s.SecurityStatu.Status,
                        StartDate = s.StartDate,
                        IsPriced = s.IsPriced,
                        PricingSourceId = s.PricingSourceID,
                        PricingSource = s.PricingSource.Source,
                        SecurityMER = s.SecurityMER * 100,
                        ProductMER = s.ProductMER * 100,
                        IsOnlyVersion = s.Product.ProductVersions.Count(ww => ww.ProductID == s.ProductID && ww.IsDeleted == false) <= 1,
                        PrimaryBenchmarkProductID = s.PrimaryBenchmarkProductID,
                        SecondaryBenchmarkProductID = s.SecondaryBenchmarkProductID,
                        PrimaryPriceTypeId = s.PrimaryPriceTypeId,
                        SecondaryPriceTypeId = s.SecondaryPriceTypeId,
                        TargetReturnRate = s.TargetReturnRate
                    }

                }).ToList();
                query.ForEach(p => p.IsWarning = CheckWarnings(p));


            }
            else
            {
                query = _productVersionRepository.GetAll(w => w.IsDeleted == false && w.Product.IsDeleted == false && (model.DisplayStatus == 0 || (model.DisplayStatus == 1 && (w.StatusID == 1 || w.StatusID == 4)) || (model.DisplayStatus == 2 && w.StatusID == 11))).Select(s => new ProductModel
                {

                    ProductID = s.Product.ProductID,
                    Code = s.Product.Code ?? "",
                    Name = s.Product.Name ?? "",
                    ProductType = s.Product.ProductType.Type,
                    AssetClassId = s.Product.AssetClassId,
                    IsWarning = false,
                    SecurityAssociation = s.ProductAssociations.Where(w => w.IsDeleted == false && (w.SecurityId != null || w.SecurityListId != null)).Select(r => new ProductSecurityAssociationModel
                    {
                        SecurityId = r.SecurityId,
                        SecurityListId = r.SecurityListId,
                        SecurityCode = r.Security.Code,
                        SecurityName = r.Security.Name ?? r.SecurityList.Name,
                        Allocation = r.Allocation

                    }).ToList(),
                    VersionDetail = new ProductVersionModel
                    {
                        ProductVersionID = s.ProductVersionID,
                        CombineVersion = s.MajorVersion + "." + s.MinorVersion,
                        StatusID = s.StatusID,
                        ProductVersionStatus = s.SecurityStatu.Status,
                        StartDate = s.StartDate,
                        IsPriced = s.IsPriced,
                        PricingSourceId = s.PricingSourceID,
                        PricingSource = s.PricingSource.Source,
                        SecurityMER = s.SecurityMER * 100,
                        ProductMER = s.ProductMER * 100,
                        IsOnlyVersion = s.Product.ProductVersions.Count(ww => ww.ProductID == s.ProductID && ww.IsDeleted == false) <= 1,
                        PrimaryBenchmarkProductID = s.PrimaryBenchmarkProductID,
                        SecondaryBenchmarkProductID = s.SecondaryBenchmarkProductID,
                        PrimaryPriceTypeId = s.PrimaryPriceTypeId,
                        SecondaryPriceTypeId = s.SecondaryPriceTypeId,
                        TargetReturnRate = s.TargetReturnRate
                    }

                }).Where(s => s.Code != null && s.Code.ToLower().Contains(model.TableParam.sSearch.ToLower())
                    || s.Name != null && s.Name.ToLower().Contains(model.TableParam.sSearch.ToLower())
                    || s.ProductType != null && s.ProductType.ToLower().Contains(model.TableParam.sSearch.ToLower())
                    || s.VersionDetail.CombineVersion != null && s.VersionDetail.CombineVersion.ToString().ToLower().Contains(model.TableParam.sSearch.ToLower())
                   || s.VersionDetail.ProductVersionStatus != null && s.VersionDetail.ProductVersionStatus.ToLower().Contains(model.TableParam.sSearch.ToLower())).ToList();

                query.ForEach(p => p.IsWarning = CheckWarnings(p));



            }

            List<ProductModel> filtered = CustomFilter(model, query);
            totalRecords = filtered.Count;

            return model.IsDownload ? filtered : SortingTableData(model, filtered);

        }

        protected List<ProductModel> GetAllModelDropDownList(ProductModel model)
        {
            IQueryable<Product> query = _productRepository.GetAll(w => !w.IsDeleted && w.ProductVersions.Any(a => !a.IsDeleted));

            if (model.VersionDetail.IsPriced)
                query = query.Where(w => w.ProductVersions.FirstOrDefault(ww => ww.IsDeleted == false && ww.ProductID == w.ProductID && ww.StatusID == 1).IsPriced == model.VersionDetail.IsPriced || w.ProductVersions.Where(ww => ww.IsDeleted == false && ww.ProductID == w.ProductID && ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().IsPriced == model.VersionDetail.IsPriced);

            return query.Select(s => new ProductModel
            {
                ProductID = s.ProductID,
                Code = s.Code,
                Name = s.Name
            }).ToList();
        }

        private List<ProductModel> GetAllProductVersionDropDownList(bool getAllVersions, ProductModel model)
        {
            IQueryable<Product> query = _productRepository.GetAll(w => !w.IsDeleted && w.ProductVersions.Any(a => !a.IsDeleted));
            if (!string.IsNullOrEmpty(model.ProductType))
                query = query.Where(w => w.ProductType.Type.ToLower() != model.ProductType.ToLower());

            return query
                .Select(p =>
                    new ProductModel
                    {
                        ProductID = p.ProductID,
                        Code = p.Code,
                        Name = p.Name,
                        ProductVersionList = p.ProductVersions.Where(ss => ((ss.StatusID == 1) && (ss.IsDeleted == false))).Select(ss => new ProductVersionModel
                        {
                            ProductVersionID = ss.ProductVersionID,
                            CombineVersion = ss.MajorVersion + "." + ss.MinorVersion,
                            ProductVersionStatus = ss.SecurityStatu.Status

                        }).ToList()

                    }).ToList();
        }

        protected List<ProductModel> GetFilteredDropDownList(ProductModel model)
        {
            IQueryable<Product> query = _productRepository.GetAll(w => !w.IsDeleted && w.ProductVersions.Any(a=>!a.IsDeleted));
            if (string.IsNullOrEmpty(model.ProductType))
                query = query.Where(w => w.ProductType.Type.ToLower() != "index");
            else
                query = query.Where(w => w.ProductType.Type.ToLower() == model.ProductType.ToLower());
            return query.Select(s => new ProductModel
            {
                ProductID = s.ProductID,
                Code = s.Code,
                Name = s.Name
            }).ToList();
        }

        protected List<ProductModel> GetAllVersions(ProductModel model)
        {
            List<ProductModel> productModelList = new List<ProductModel>(); 
            IQueryable<Product> query = _productRepository.GetAll(w => !w.IsDeleted && w.ProductVersions.Any(a => !a.IsDeleted));

            foreach (var product in query)
            {
                ProductModel productModel = FromProduct(product);
                productModelList.Add(productModel); 
            }
            return productModelList; 
        }

        protected List<ProductModel> GetSecurityListByAssetClass(ProductModel model)
        {
            var productAssociation = uow.Repository<Database.ProductAssociation>();
            var securityListDetail = uow.Repository<Database.SecurityListDetail>().GetAll(w => w.IsDeleted == false);
            var securityAssetClass = uow.Repository<Database.SecurityAssetClass>().GetAll(w => w.IsDeleted == false);
            var security = uow.Repository<Database.Security>().GetAll(w => w.IsDeleted == false);
            List<ProductModel> result = new List<ProductModel>();

            if (model.SecurityIds != null)
            {
                foreach (var secId in model.SecurityIds)
                {
                    if (secId != 0)
                        result.AddRange(from s in security
                                        where
                                            s.Id == secId &&
                                            !
                                                (from sa in securityAssetClass
                                                 where
                                                     sa.SecurityId == secId && sa.IsDeleted != true
                                                 select new
                                                 {
                                                     sa.AssetClassId
                                                 }).Contains(new { AssetClassId = model.AssetClassId })
                                        select new ProductModel
                                        {
                                            SecName = s.Code
                                        });
                }
            }

            if (model.SecurityListIds != null)
            {
                foreach (var secListId in model.SecurityListIds)
                {
                    if (secListId != 0)
                        result.AddRange(from s in securityListDetail
                                        where
                                            s.SecurityListId == secListId &&
                                            !
                                                (from sa in securityAssetClass
                                                 where
                                                     sa.SecurityId == s.SecurityId && sa.IsDeleted != true
                                                 select new
                                                 {
                                                     sa.AssetClassId
                                                 }).Contains(new { AssetClassId = model.AssetClassId })
                                        group s by s.Security.Code into detail
                                        select new ProductModel
                                        {
                                            SecName = detail.Key,
                                            StatusDate = detail.Max(x => x.StatusDate)
                                        });
                }
            }

            return result;
        }

        private static List<ProductModel> SortingTableData(ProductModel model, List<ProductModel> filtered)
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
                    return filtered.OrderBy(p => p.ProductType).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "desc")
                {
                    return
                        filtered.OrderByDescending(p => p.ProductType)
                            .Skip(model.TableParam.iDisplayStart)
                            .Take(model.TableParam.iDisplayLength)
                            .ToList();
                }

                if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.VersionDetail.CombineVersion).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "desc")
                {
                    return
                        filtered.OrderByDescending(p => p.VersionDetail.CombineVersion)
                            .Skip(model.TableParam.iDisplayStart)
                            .Take(model.TableParam.iDisplayLength)
                            .ToList();
                }
                if (model.TableParam.iSortCol_0 == 4 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.VersionDetail.ProductVersionStatus).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 4 && model.TableParam.sSortDir_0 == "desc")
                {
                    return
                        filtered.OrderByDescending(p => p.VersionDetail.ProductVersionStatus)
                            .Skip(model.TableParam.iDisplayStart)
                            .Take(model.TableParam.iDisplayLength)
                            .ToList();
                }
                if (model.TableParam.iSortCol_0 == 5 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.IsWarning).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 5 && model.TableParam.sSortDir_0 == "desc")
                {
                    return
                        filtered.OrderByDescending(p => p.IsWarning)
                            .Skip(model.TableParam.iDisplayStart)
                            .Take(model.TableParam.iDisplayLength)
                            .ToList();
                }

                return filtered.OrderBy(x => x.Code).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
            }
            return filtered.OrderBy(od => od.Code).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
        }

        private static List<ProductModel> CustomFilter(ProductModel model, List<ProductModel> query)
        {
            List<ProductModel> filtered = query;

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
                filtered = filtered.Where(s => s.ProductType != null && s.ProductType.ToLower().Contains(model.TableParam.sSearch_2.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_3))
            {
                filtered = filtered.Where(s => s.VersionDetail.CombineVersion != null && s.VersionDetail.CombineVersion.ToLower().Contains(model.TableParam.sSearch_3.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_4))
            {
                filtered = filtered.Where(s => s.VersionDetail.ProductVersionStatus != null && s.VersionDetail.ProductVersionStatus.ToLower().Contains(model.TableParam.sSearch_4.ToLower())).ToList();
            }
            if (model.TableParam.sProductTypeSearch != null)
            {
                filtered = filtered.Where(s => s.ProductTypeId != null && model.TableParam.sProductTypeSearch.Contains(s.ProductTypeId)).ToList();
            }
            if (model.TableParam.sAssetClassSearch != null)
            {
                filtered = filtered.Where(s => s.AssetClassId != null && model.TableParam.sAssetClassSearch.Contains(s.AssetClassId)).ToList();
            }
            if (model.TableParam.sSubAssetClassSearch != null)
            {
                filtered = filtered.Where(s => s.SubAssetClassId != null && model.TableParam.sSubAssetClassSearch.Contains(s.SubAssetClassId)).ToList();
            }
            return filtered;
        }

        public IViewModel AddProduct(IViewModel baseModel)
        {
            var model = (ProductModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success,
            };
            try
            {
                // If user selected BaseProduct then create only ProductVersion 
                if (model.IsBaseProduct)
                {
                    var _productVersion = _productVersionRepository.Where(a => a.ProductVersionID == model.BaseProductVersionID);
                    var _productVersionDetail = _productVersionRepository.Where(w => w.ProductID == _productVersion.FirstOrDefault().ProductID).OrderByDescending(o => o.ProductVersionID).FirstOrDefault();
                    var product = _productRepository.GetById(_productVersion.FirstOrDefault().ProductID.Value);

                    product.Code = model.Code.Trim();
                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.ProductTypeId = model.ProductTypeId;
                    product.IndexTypeId = model.IndexTypeId;
                    product.ProductAPIR = model.ProductAPIR;
                    product.ProductISIN = model.ProductISIN;
                    product.InstitutionId = model.InstitutionId;
                    product.MarketId = model.MarketId;
                    product.CurrencyId = model.CurrencyId;
                    product.SubAssetClassId = model.SubAssetClassId;
                    product.RegionId = model.RegionId;
                    product.StatusId = model.StatusId;
                    product.AssetClassId = model.AssetClassId;
                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Product", ObjectID = product.ProductID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });

                    var productVersion = new ProductVersion
                    {
                        ProductID = _productVersion.FirstOrDefault().ProductID,
                        IsPriced = model.VersionDetail.IsPriced,
                        PrimaryBenchmarkProductID = model.VersionDetail.PrimaryBenchmarkProductID,
                        SecondaryBenchmarkProductID = model.VersionDetail.SecondaryBenchmarkProductID,
                        PrimaryPriceTypeId = model.VersionDetail.PrimaryPriceTypeId,
                        SecondaryPriceTypeId = model.VersionDetail.SecondaryPriceTypeId,
                        PricingSourceID = model.VersionDetail.PricingSourceId,
                        SecurityMER = model.VersionDetail.SecurityMER / 100,
                        ProductMER = model.VersionDetail.ProductMER / 100,
                        MajorVersion = _productVersionDetail.MajorVersion + 1,
                        MinorVersion = 0,
                        StatusID = 4,
                        ProductReimbursable = model.VersionDetail.ProductReimbursable / 100,
                        TargetReturnRate = model.VersionDetail.TargetReturnRate
                    };

                    _productVersionRepository.Add(productVersion);

                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductVersion", ObjectID = productVersion.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                    if (model.ProductBrokerIds != null)
                    {
                        foreach (var productBrokerId in model.ProductBrokerIds)
                        {
                            if (productBrokerId != null)
                            {
                                var proBrokerClass = new ProductBroker
                                {
                                    ProductId = _productVersion.FirstOrDefault().ProductID,
                                    InstitutionId = productBrokerId,
                                };
                                _productBrokeRepository.Add(proBrokerClass);
                                uow.Commit();
                                // Maintaining Log
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductBroker", ObjectID = proBrokerClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                            }
                        }
                    }


                    if (model.SecurityAssociation != null)
                    {
                        foreach (var secAssociation in model.SecurityAssociation)
                        {
                            if (secAssociation.SecurityId != null)
                            {
                                var securityClass = new ProductAssociation
                                {
                                    //ProductId = product.ProductID,
                                    SecurityId = secAssociation.SecurityId,
                                    Allocation = secAssociation.Allocation,
                                    ProductVersionID = productVersion.ProductVersionID
                                };
                                _productAssociationRepository.Add(securityClass);
                                uow.Commit();
                                // Maintaining Log
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = securityClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                            }


                            if (secAssociation.SecurityListId != null)
                            {
                                var securityListClass = new ProductAssociation
                                {
                                    //ProductId = product.ProductID,
                                    SecurityListId = secAssociation.SecurityListId,
                                    Allocation = secAssociation.Allocation,
                                    ProductVersionID = productVersion.ProductVersionID
                                };
                                _productAssociationRepository.Add(securityListClass);
                                uow.Commit();
                                // Maintaining Log
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = securityListClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                            }
                        }

                    }

                    // Updating Previous pending versions to inactive except active versions
                    var previousPendingversionsList = _productVersionRepository.Where(w => w.ProductID == _productVersion.FirstOrDefault().ProductID && !w.IsDeleted && w.StatusID == 4 && w.ProductVersionID != productVersion.ProductVersionID).ToList();
                    foreach (var item in previousPendingversionsList)
                    {
                        item.StatusID = 2;
                        uow.Commit();
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = item.ProductID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                    }

                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordAddedSuccessfully, "ProductVersion");

                }

                else
                {
                    var isExists = _productRepository.Any(a => a.Code == model.Code && !a.IsDeleted);
                    if (!isExists)
                    {
                        var product = new Product
                        {
                            Code = model.Code.Trim(),
                            Name = model.Name,
                            Description = model.Description,
                            InceptionDate = DateTime.Now,
                            ProductTypeId = model.ProductTypeId,
                            IndexTypeId = model.IndexTypeId,
                            ProductAPIR = model.ProductAPIR,
                            ProductISIN = model.ProductISIN,
                            InstitutionId = model.InstitutionId,
                            MarketId = model.MarketId,
                            CurrencyId = model.CurrencyId,
                            SubAssetClassId = model.SubAssetClassId,
                            RegionId = model.RegionId,
                            StatusId = model.StatusId,
                            AssetClassId = model.AssetClassId,
                        };
                        _productRepository.Add(product);
                        uow.Commit();

                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Product", ObjectID = product.ProductID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                        var productVersion = new ProductVersion
                        {
                            ProductID = product.ProductID,
                            IsPriced = model.VersionDetail.IsPriced,
                            PrimaryBenchmarkProductID = model.VersionDetail.PrimaryBenchmarkProductID,
                            SecondaryBenchmarkProductID = model.VersionDetail.SecondaryBenchmarkProductID,
                            PrimaryPriceTypeId = model.VersionDetail.PrimaryPriceTypeId,
                            SecondaryPriceTypeId = model.VersionDetail.SecondaryPriceTypeId,
                            PricingSourceID = model.VersionDetail.PricingSourceId,
                            SecurityMER = model.VersionDetail.SecurityMER / 100,
                            ProductMER = model.VersionDetail.ProductMER / 100,
                            MajorVersion = 1,
                            MinorVersion = 0,
                            //StartDate = DateTime.Now,
                            StatusID = 4,
                            ProductReimbursable = model.VersionDetail.ProductReimbursable / 100,
                            TargetReturnRate = model.VersionDetail.TargetReturnRate
                        };

                        _productVersionRepository.Add(productVersion);
                        uow.Commit();

                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductVersion", ObjectID = productVersion.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                        if (model.ProductBrokerIds != null)
                        {
                            foreach (var productBrokerId in model.ProductBrokerIds)
                            {
                                if (productBrokerId != null)
                                {
                                    var proBrokerClass = new ProductBroker
                                    {
                                        ProductId = product.ProductID,
                                        InstitutionId = productBrokerId,
                                    };
                                    _productBrokeRepository.Add(proBrokerClass);
                                    uow.Commit();
                                    // Maintaining Log
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductBroker", ObjectID = proBrokerClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                }
                            }
                        }

                        if (model.SecurityAssociation != null)
                        {
                            foreach (var secAssociation in model.SecurityAssociation)
                            {
                                if (secAssociation.SecurityId != null)
                                {
                                    var securityClass = new ProductAssociation
                                    {
                                        //ProductId = product.ProductID,
                                        SecurityId = secAssociation.SecurityId,
                                        Allocation = secAssociation.Allocation,
                                        ProductVersionID = productVersion.ProductVersionID
                                    };
                                    _productAssociationRepository.Add(securityClass);
                                    uow.Commit();
                                    // Maintaining Log
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = securityClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                }


                                if (secAssociation.SecurityListId != null)
                                {
                                    var securityListClass = new ProductAssociation
                                    {
                                        //ProductId = product.ProductID,
                                        SecurityListId = secAssociation.SecurityListId,
                                        Allocation = secAssociation.Allocation,
                                        ProductVersionID = productVersion.ProductVersionID
                                    };
                                    _productAssociationRepository.Add(securityListClass);
                                    uow.Commit();
                                    // Maintaining Log
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = securityListClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                }
                            }
                        }
                        uow.Dispose();
                        responseModel.Message = string.Format(MDA_Resource.Info_RecordAddedSuccessfully, "Product");
                    }
                    else
                    {
                        responseModel.Status = ResponseStatus.Failure;
                        responseModel.Message = "Product already exists.";
                    }
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

        public IViewModel UpdateProduct(IViewModel baseModel)
        {
            var model = (ProductModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success
            };
            try
            {

                if (model.MakeItActive)
                {
                    #region Make It Active

                    var productVersion = _productVersionRepository.Where(a => a.ProductID == model.ProductID && a.ProductVersionID == model.BaseProductVersionID).FirstOrDefault();
                    var isSelectedProductVersionIsNotLatestVersion = false;
                    var updatePreviousProductVersionStatus = false;

                    var activeProductVersion = _productVersionRepository.Where(w => w.ProductID == model.ProductID && w.StatusID == 1 && !w.IsDeleted).FirstOrDefault();

                    #region New Investment Template Veraion
                    
                    if (activeProductVersion != null)
                    {
                        var productVersionInvServiceList = _productVersionInvestmentProgramRepository.Where(w => w.ProductVersionID == activeProductVersion.ProductVersionID && !w.IsDeleted).ToList();
                        if (productVersionInvServiceList.Any())
                        {
                            var distinctInvServiceIDs = productVersionInvServiceList.GroupBy(m => m.InvestmentProgram.InvestmentProgramID).Select(group => group.First().InvestmentProgram).ToList();
                            foreach (var item in distinctInvServiceIDs)
                            {
                                var distinctInvService = _investmentProgramRepository.Where(w => !w.IsDeleted && w.InvestmentProgramID == item.InvestmentProgramID).FirstOrDefault();
                                if (distinctInvService != null)
                                {
                                    var invTempCom = new InvestmentProgramTemplateComponent();
                                    var afslId = invTempCom.GetAFSLByClientId(distinctInvService.EntityCoreId);

                                    //check for existing template if found then create a new updated version if not then create new first ver of template
                                    var invTemplateVersion = _investmentProgramTemplateRepository.Where(w => !w.IsDeleted && w.Name == item.Name).OrderByDescending(w => w.MajorVersion).FirstOrDefault();

                                    // create Template Detail
                                    var invProgTemplate = new InvestmentProgramTemplate
                                    {
                                        Name = distinctInvService.Name,
                                        TargetReturnRate = distinctInvService.TargetReturnRate,
                                        ServiceTypeId = distinctInvService.ServiceTypeId,
                                        RebalancingTypeId = distinctInvService.RebalancingTypeId,
                                        MaterialityTolerance = distinctInvService.MaterialityTolerance,
                                        MinTradeValue = distinctInvService.MinTradeValue,
                                        MinHoldingAmount = distinctInvService.MinHoldingAmount,
                                        FortyFiveDayRule = distinctInvService.FortyFiveDayRule,
                                        RebalanceToleranceAmount = distinctInvService.RebalanceToleranceAmount,
                                        RebalanceTolerancePercent = distinctInvService.RebalanceTolerancePercent,
                                        MajorVersion = invTemplateVersion != null ? invTemplateVersion.MajorVersion + 1 : 1,
                                        MinorVersion = 0,
                                        AFSLEntityCoreId = afslId ?? (invTemplateVersion != null ? invTemplateVersion.AFSLEntityCoreId : null),
                                        StatusId = 1,
                                        InvestmentTemplateCode = invTemplateVersion != null ? invTemplateVersion.InvestmentTemplateCode : null
                                    };
                                    _investmentProgramTemplateRepository.Add(invProgTemplate);
                                    uow.Commit();
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "InvestmentProgramTemplate", ObjectID = invProgTemplate.InvestmentProgramID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                                    if (invTemplateVersion == null)
                                    {
                                        var invTemplate = _investmentProgramTemplateRepository.Where(a => a.InvestmentProgramID == invProgTemplate.InvestmentProgramID).FirstOrDefault();
                                        invTemplate.InvestmentTemplateCode = "INV" + invProgTemplate.InvestmentProgramID.ToString().PadLeft(5, '0');
                                        uow.Commit();
                                    }

                                    // add Template Securities
                                    if (distinctInvService.SecuritiesInvestmentPrograms.Any())
                                    {
                                        foreach (var securityItem in distinctInvService.SecuritiesInvestmentPrograms)
                                        {
                                            var securityInvestmentProgramTemplate = new SecuritiesInvestmentProgramTemplate
                                            {
                                                SecurityID = securityItem.SecurityID,
                                                AssetClassId = securityItem.AssetClassId,
                                                TargetAllocation = securityItem.TargetAllocation,
                                                InvestmentProgramID = invProgTemplate.InvestmentProgramID
                                            };
                                            _securityInvestmentProgramTemplateRepository.Add(securityInvestmentProgramTemplate);
                                            uow.Commit();

                                            // Maintaining Log
                                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "SecuritiesInvestmentProgramTemplate", ObjectID = securityInvestmentProgramTemplate.SecuritiesInvestmentProgramID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                        }
                                    }

                                    // add Template Product Version
                                    if (distinctInvService.ProductVersionInvestmentPrograms.Any())
                                    {
                                        foreach (var productItem in distinctInvService.ProductVersionInvestmentPrograms)
                                        {
                                            var productVersionInvestmentProgramTemplate = new ProductVersionInvestmentProgramTemplate
                                            {
                                                ProductVersionID = productItem.ProductVersionID == activeProductVersion.ProductVersionID ? model.BaseProductVersionID : productItem.ProductVersionID,
                                                TargetAllocation = productItem.TargetAllocation,
                                                InvestmentProgramID = invProgTemplate.InvestmentProgramID
                                            };
                                            _productVersionInvestmentProgramTemplateRepository.Add(productVersionInvestmentProgramTemplate);
                                            uow.Commit();

                                            // Maintaining Log
                                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductVersionInvestmentProgramTemplate", ObjectID = productVersionInvestmentProgramTemplate.ProductVersionInvestmentProgramID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                        }
                                    }
                                    // add Template Model Version
                                    if (distinctInvService.ModelVersionInvestmentPrograms.Any())
                                    {
                                        foreach (var modelItem in distinctInvService.ModelVersionInvestmentPrograms)
                                        {
                                            var modelVersionInvestmentProgramTemplate = new ModelVersionInvestmentProgramTemplate
                                            {
                                                ModelVersionID = modelItem.ModelVersionID,
                                                TargetAllocation = modelItem.TargetAllocation,
                                                InvestmentProgramID = invProgTemplate.InvestmentProgramID
                                            };
                                            _modelVersionInvestmentProgramTemplateRepository.Add(modelVersionInvestmentProgramTemplate);
                                            uow.Commit();

                                            // Maintaining Log
                                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ModelVersionInvestmentProgramTemplate", ObjectID = modelVersionInvestmentProgramTemplate.ModelVersionInvestmentProgramID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                        }
                                    }
                                    // add Template Strategic Allocation
                                    if (distinctInvService.InvestmentProgramStrategicAllocations.Any())
                                    {
                                        foreach (var strategicItem in distinctInvService.InvestmentProgramStrategicAllocations)
                                        {
                                            var invProgStrategicAllocation = new InvestmentProgramStrategicAllocationTemplate()
                                            {
                                                InvestmentProgramId = invProgTemplate.InvestmentProgramID,
                                                AssetClassId = strategicItem.AssetClassId,
                                                MaxValue = strategicItem.MaxValue,
                                                MinValue = strategicItem.MinValue,
                                                StrategicAssetAllocation = strategicItem.StrategicAssetAllocation
                                            };
                                            _investmentProgramStrategicAllocationTemplateRepository.Add(invProgStrategicAllocation);
                                            uow.Commit();

                                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "InvestmentProgramStrategicAllocationTemplate", ObjectID = invProgStrategicAllocation.InvestmentProgramStrategicAllocationId, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                        }
                                    }

                                    // add Template Default Securities
                                    if (distinctInvService.InvestmentProgramDefaultSecurities.Any())
                                    {
                                        foreach (var defaultSecurityItem in distinctInvService.InvestmentProgramDefaultSecurities)
                                        {
                                            var defaultSecurities = new InvestmentProgramDefaultSecuritiesTemplate()
                                            {
                                                InvestmentProgramId = invProgTemplate.InvestmentProgramID,
                                                AssetClassId = defaultSecurityItem.AssetClassId,
                                                SecurityId = defaultSecurityItem.SecurityId
                                            };
                                            _investmentProgramDefaultSecuritiesTemplateRepository.Add(defaultSecurities);
                                            uow.Commit();
                                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "InvestmentProgramDefaultSecurities", ObjectID = defaultSecurities.InvestmentProgramDefaultSecuritiesId, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                        }
                                    }

                                    // check if template version already exist
                                    if (invTemplateVersion != null)
                                    {
                                        // add Template Screen detail
                                        if (invTemplateVersion.InvestmentProgramScreenTemplates.Any())
                                        {
                                            foreach (var screenItem in invTemplateVersion.InvestmentProgramScreenTemplates)
                                            {
                                                var invProgscreen = new InvestmentProgramScreenTemplate()
                                                {
                                                    InvestmentProgramId = invProgTemplate.InvestmentProgramID,
                                                    ScreenId = screenItem.ScreenId
                                                };
                                                _investmentProgramScreenTemplateRepository.Add(invProgscreen);
                                                uow.Commit();

                                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "InvestmentProgramScreenTemplate", ObjectID = invProgscreen.InvestmentProgramScreenId, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                            }
                                        }

                                        // add Template Constraint detail
                                        if (invTemplateVersion.InvestmentProgramConstraintTemplates.Any())
                                        {
                                            foreach (var constraintItem in invTemplateVersion.InvestmentProgramConstraintTemplates)
                                            {
                                                var invProgConstraintTemplate = new InvestmentProgramConstraintTemplate()
                                                {
                                                    SecurityId = constraintItem.SecurityId,
                                                    ConstraintId = constraintItem.ConstraintId,
                                                    ConstraintTypeId = constraintItem.ConstraintTypeId,
                                                    SubstitutedSecurityId = constraintItem.SubstitutedSecurityId,
                                                    InvestmentProgramId = invProgTemplate.InvestmentProgramID
                                                };
                                                _investmentProgramConstraintTemplateRepository.Add(invProgConstraintTemplate);
                                                uow.Commit();

                                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "InvestmentProgramConstraintTemplate", ObjectID = invProgConstraintTemplate.InvestmentProgramConstraintId, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                            }
                                        }

                                        //set previous template version to Inactive
                                        invTemplateVersion.StatusId = 2;
                                        uow.Commit();
                                        //Maintaining Log
                                        CreateEntityLog(new EntitiesEventLogModel
                                        {
                                            ObjectType = "InvestmentProgramTemplate",
                                            ObjectID = invTemplateVersion.InvestmentProgramID,
                                            DateTime = DateTime.Now,
                                            UserID = SessionObject.GetInstance.UserID,
                                            OperationType = "UPDATE"
                                        });
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Creating New ModelVersion if Product is Associated with Model

                    if (activeProductVersion != null)
                    {
                        var modelDetailList = _modelDetailRepository.Where(w => w.ProductVersionID == activeProductVersion.ProductVersionID && !w.IsDeleted).ToList();
                        var distinctModelIDs = modelDetailList.GroupBy(m => m.ModelVersion.ModelID).Select(group => group.First().ModelVersion).ToList();
                        foreach (var item in distinctModelIDs)
                        {
                            var distinctModelVersion = _modelVersionRepository.Where(w => w.ModelID == item.ModelID && w.ModelVersionID == item.ModelVersionID && w.StatusID == 1).FirstOrDefault();
                            if (distinctModelVersion == null)
                            {
                                distinctModelVersion = _modelVersionRepository.Where(w => w.ModelID == item.ModelID && w.ModelVersionID == item.ModelVersionID && w.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault();
                            }

                            if (distinctModelVersion != null)
                            {
                                var _modelVersion = new ModelVersion
                                {
                                    ModelID = distinctModelVersion.ModelID,
                                    MajorVersion = _modelVersionRepository.Where(w => w.ModelID == distinctModelVersion.ModelID).OrderByDescending(o => o.MajorVersion).FirstOrDefault().MajorVersion + 1,
                                    MinorVersion = distinctModelVersion.MinorVersion,
                                    StatusID = 4,
                                    PrimaryBenchmarkProductID = distinctModelVersion.PrimaryBenchmarkProductID,
                                    SecondaryBenchmarkProductID = distinctModelVersion.SecondaryBenchmarkProductID,
                                    SecurityICR = distinctModelVersion.SecurityICR,
                                    ModelMER = distinctModelVersion.ModelMER,
                                    TargetReturnRate = distinctModelVersion.TargetReturnRate,
                                    StartDate = distinctModelVersion.StartDate,
                                    EndDate = distinctModelVersion.EndDate,
                                };

                                _modelVersionRepository.Add(_modelVersion);
                                uow.Commit();
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ModelVersion", ObjectID = _modelVersion.ModelVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                                foreach (var modelDetail in distinctModelVersion.ModelDetails)
                                {
                                    if (!modelDetail.IsDeleted)
                                    {
                                        var _modelDetails = new ModelDetail
                                        {
                                            ModelVersionID = _modelVersion.ModelVersionID,
                                            ProductVersionID = modelDetail.SecurityId == null ? (modelDetail.ProductVersionID == activeProductVersion.ProductVersionID ? model.BaseProductVersionID : modelDetail.ProductVersionID) : (int?)null,
                                            SecurityId = modelDetail.SecurityId,
                                            TargetAllocation = modelDetail.TargetAllocation,
                                            AssetClassId = modelDetail.AssetClassId,
                                            IsDeleted = modelDetail.IsDeleted,
                                        };
                                        _modelDetailRepository.Add(_modelDetails);
                                        uow.Commit();
                                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ModelDetail", ObjectID = _modelDetails.ModelDetailID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                    }
                                }

                                foreach (var modelafslLicensee in distinctModelVersion.ModelAFSLLicenseeAssociations)
                                {
                                    var _modelAfslLicenseeAssociations = new ModelAFSLLicenseeAssociation
                                    {
                                        ModelVersionID = _modelVersion.ModelVersionID,
                                        EntityCoreID = modelafslLicensee.EntityCoreID,
                                        IsDeleted = modelafslLicensee.IsDeleted
                                    };

                                    _modelAFSLLicenseeAssociationRepositry.Add(_modelAfslLicenseeAssociations);
                                    uow.Commit();
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ModelAFSLLicenseeAssociation", ObjectID = _modelAfslLicenseeAssociations.ModelAFSLLicenseeAssociationID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                }

                                // Updating Previous pending versions to inactive except active versions
                                var previousPendingversionsList = _modelVersionRepository.Where(w => w.ModelID == distinctModelVersion.ModelID && !w.IsDeleted && w.StatusID == 4 && w.ModelVersionID != _modelVersion.ModelVersionID).ToList();
                                foreach (var ppl in previousPendingversionsList)
                                {
                                    ppl.StatusID = 2;
                                    uow.Commit();
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ModelVersion", ObjectID = ppl.ModelVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                                }
                            }

                        }

                    }

                    else
                    {
                        // Do Nothing
                    }

                    #endregion

                    #region Making Active n InActive

                    // If the selectedModelVersion is the latest one in his respective Model then change its status to active
                    var latestProductVersion = _productVersionRepository.Where(a => a.ProductID == model.ProductID).OrderByDescending(o => o.MajorVersion).FirstOrDefault();
                    if (productVersion.MajorVersion < latestProductVersion.MajorVersion)
                    {
                        isSelectedProductVersionIsNotLatestVersion = true;
                        responseModel.Status = ResponseStatus.Failure;
                        responseModel.Message = "Product Version is not the latest version.";
                        return responseModel;
                    }
                    else
                    {
                        updatePreviousProductVersionStatus = true;
                    }


                    // IF ONE VALUE IS FALSE AND ANOTHER IS TRUE IT MEANS WE NEED TO CHANGE THE STATUS OF PREVIOUS MODELVERSION TO "INACTIVE"
                    if (!isSelectedProductVersionIsNotLatestVersion && updatePreviousProductVersionStatus)
                    {
                        var previousActiveproductVersionsList = _productVersionRepository.Where(a => a.ProductID == model.ProductID && a.ProductVersionID < model.BaseProductVersionID && a.StatusID == 1).ToList();
                        foreach (var item in previousActiveproductVersionsList)
                        {
                            // StatusID = 2 // "Inactive"
                            item.StatusID = 2;
                            item.EndDate = DateTime.Now;
                            // Maintaining Log
                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductVersion", ObjectID = item.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                        }

                        // StatusID = 1 // "Active"

                        productVersion.StatusID = 1;
                        productVersion.StartDate = DateTime.Now;
                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ModelVersion", ObjectID = productVersion.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });

                        responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Product Version Status");
                        return responseModel;
                    }

                    #endregion

                    #endregion
                }
                else if (model.MakeItInActive)
                {
                    var archivedVersionDetail = _productVersionRepository.FirstOrDefault(a => a.ProductID == model.ProductID && a.ProductVersionID == model.BaseProductVersionID && a.StatusID == 11 && !a.IsDeleted);
                    if (archivedVersionDetail != null)
                    {
                        archivedVersionDetail.StatusID = 2;
                        archivedVersionDetail.StartDate = DateTime.Now;
                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ModelVersion", ObjectID = archivedVersionDetail.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });

                        responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Product Version Status");
                        return responseModel;
                    }
                }
                else if (model.MakeItArchived)
                {
                    var inactiveVersionDetail = _productVersionRepository.FirstOrDefault(a => a.ProductID == model.ProductID && a.ProductVersionID == model.BaseProductVersionID && a.StatusID == 2 && !a.IsDeleted);
                    if (inactiveVersionDetail != null)
                    {
                        inactiveVersionDetail.StatusID = 11;
                        inactiveVersionDetail.StartDate = DateTime.Now;
                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ModelVersion", ObjectID = inactiveVersionDetail.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });

                        responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Product Version Status");
                        return responseModel;
                    }
                }
                else
                {

                    #region Creating New Version and Update

                    var product = _productRepository.GetById(model.ProductID);
                    var thisproductVersion = _productVersionRepository.GetById(model.BaseProductVersionID);
                    var deletedProductList = new Dictionary<int, bool>();
                    var deletedProductAssociationList = new Dictionary<int, bool>();

                    if (product != null)
                    {
                        var isExists = _productRepository.Any(a => a.ProductID != model.ProductID && a.Code == model.Code && !a.IsDeleted);
                        if (!isExists)
                        {
                            if (thisproductVersion.StatusID == 1)
                            {
                                #region Create new version

                                #region Product

                                product.Code = model.Code.Trim();
                                product.Name = model.Name;
                                product.Description = model.Description;
                                product.ProductTypeId = model.ProductTypeId;
                                product.IndexTypeId = model.IndexTypeId;
                                product.ProductAPIR = model.ProductAPIR;
                                product.ProductISIN = model.ProductISIN;
                                product.InstitutionId = model.InstitutionId;
                                product.MarketId = model.MarketId;
                                product.CurrencyId = model.CurrencyId;
                                product.SubAssetClassId = model.SubAssetClassId;
                                product.RegionId = model.RegionId;
                                product.StatusId = model.StatusId;
                                product.AssetClassId = model.AssetClassId;
                                product.InceptionDate = model.InceptionDate;
                                uow.Commit();
                                // Maintaining Log
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Product", ObjectID = product.ProductID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });


                                #endregion

                                #region ProductVersion

                                var productVersion = new ProductVersion
                                {
                                    ProductID = model.ProductID,
                                    IsPriced = model.VersionDetail.IsPriced,
                                    PrimaryBenchmarkProductID = model.VersionDetail.PrimaryBenchmarkProductID,
                                    SecondaryBenchmarkProductID = model.VersionDetail.SecondaryBenchmarkProductID,
                                    PrimaryPriceTypeId = model.VersionDetail.PrimaryPriceTypeId,
                                    SecondaryPriceTypeId = model.VersionDetail.SecondaryPriceTypeId,
                                    PricingSourceID = model.VersionDetail.PricingSourceId,
                                    SecurityMER = model.VersionDetail.SecurityMER / 100,
                                    ProductMER = model.VersionDetail.ProductMER / 100,
                                    MajorVersion = product.ProductVersions.OrderByDescending(o => o.MajorVersion).FirstOrDefault().MajorVersion + 1,
                                    MinorVersion = 0,
                                    //StartDate = DateTime.Now,
                                    StatusID = 4,
                                    ProductReimbursable = model.VersionDetail.ProductReimbursable / 100,
                                    TargetReturnRate = model.VersionDetail.TargetReturnRate
                                };

                                _productVersionRepository.Add(productVersion);
                                uow.Commit();

                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductVersion", ObjectID = productVersion.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                                #endregion

                                #region ProductAssociation

                                if (model.SecurityAssociation != null)
                                {
                                    foreach (var secAssociation in model.SecurityAssociation)
                                    {
                                        if (secAssociation.SecurityId != null)
                                        {
                                            var securityClass = new ProductAssociation
                                            {
                                                SecurityId = secAssociation.SecurityId,
                                                Allocation = secAssociation.Allocation,
                                                ProductVersionID = productVersion.ProductVersionID
                                            };
                                            _productAssociationRepository.Add(securityClass);
                                            uow.Commit();
                                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = securityClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                        }


                                        if (secAssociation.SecurityListId != null)
                                        {
                                            var securityListClass = new ProductAssociation
                                            {
                                                SecurityListId = secAssociation.SecurityListId,
                                                Allocation = secAssociation.Allocation,
                                                ProductVersionID = productVersion.ProductVersionID
                                            };
                                            _productAssociationRepository.Add(securityListClass);
                                            uow.Commit();
                                            CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = securityListClass.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                        }
                                    }

                                }

                                #endregion

                                #region ProductBrooker

                                foreach (var productBrokerClass in product.ProductBrokers)
                                {
                                    if (productBrokerClass.IsDeleted == false)
                                    {
                                        deletedProductList.Add(productBrokerClass.Id, true);
                                    }
                                    productBrokerClass.IsDeleted = true;
                                }

                                if (model.ProductBrokerIds != null)
                                {
                                    foreach (var productBrokerId in model.ProductBrokerIds)
                                    {
                                        if (productBrokerId != null)
                                        {
                                            var productBroker = product.ProductBrokers.FirstOrDefault(a => a.InstitutionId == productBrokerId);
                                            if (productBroker != null)
                                            {
                                                //update
                                                productBroker.ProductId = model.ProductID;
                                                productBroker.InstitutionId = productBrokerId;
                                                productBroker.IsDeleted = false;
                                                uow.Commit();
                                                // Maintaining Log
                                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductBroker", ObjectID = productBroker.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                                                if (deletedProductList.ContainsKey(productBroker.Id))
                                                {
                                                    deletedProductList[productBroker.Id] = productBroker.IsDeleted;
                                                }
                                            }
                                            else
                                            {
                                                //add
                                                var proBroker = new ProductBroker
                                                {
                                                    ProductId = model.ProductID,
                                                    InstitutionId = productBrokerId,
                                                };
                                                _productBrokeRepository.Add(proBroker);
                                                uow.Commit();
                                                // Maintaining Log
                                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductBroker", ObjectID = proBroker.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                            }
                                        }

                                    }

                                    foreach (var item in deletedProductList.Where(w => w.Value == true))
                                    {
                                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductBroker", ObjectID = item.Key, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });
                                    }
                                }

                                #endregion

                                // Updating Previous pending versions to inactive except active versions
                                var previousPendingversionsList = _productVersionRepository.Where(w => w.ProductID == model.ProductID && !w.IsDeleted && w.StatusID == 4 && w.ProductVersionID != productVersion.ProductVersionID).ToList();
                                foreach (var item in previousPendingversionsList)
                                {
                                    item.StatusID = 2;
                                    uow.Commit();
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductVersion", ObjectID = item.ProductID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                                }

                                responseModel.Status = ResponseStatus.Success;
                                responseModel.Message = string.Format(MDA_Resource.Info_RecordAddedSuccessfully, "Active version detail cannot be updated, so a new product version with the given changes has been ");

                                #endregion
                            }

                            if (thisproductVersion.StatusID == 4)
                            {
                                #region Update Current Product and Version details

                                product.Code = model.Code.Trim();
                                product.Name = model.Name;
                                product.Description = model.Description;
                                product.ProductTypeId = model.ProductTypeId;
                                product.IndexTypeId = model.IndexTypeId;
                                product.ProductAPIR = model.ProductAPIR;
                                product.ProductISIN = model.ProductISIN;
                                product.InstitutionId = model.InstitutionId;
                                product.MarketId = model.MarketId;
                                product.CurrencyId = model.CurrencyId;
                                product.SubAssetClassId = model.SubAssetClassId;
                                product.RegionId = model.RegionId;
                                product.StatusId = model.StatusId;
                                product.AssetClassId = model.AssetClassId;
                                product.InceptionDate = model.InceptionDate;
                                uow.Commit();
                                // Maintaining Log
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Product", ObjectID = product.ProductID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });


                                var ProductVersion = _productVersionRepository.Where(a => a.ProductID == model.ProductID && a.ProductVersionID == model.BaseProductVersionID).OrderBy(o => o.StartDate).FirstOrDefault();
                                if (ProductVersion != null)
                                {
                                    //ProductVersion.StartDate = model.StartDate;
                                    ProductVersion.IsPriced = model.VersionDetail.IsPriced;
                                    ProductVersion.PrimaryBenchmarkProductID = model.VersionDetail.PrimaryBenchmarkProductID;
                                    ProductVersion.SecondaryBenchmarkProductID = model.VersionDetail.SecondaryBenchmarkProductID;
                                    ProductVersion.PrimaryPriceTypeId = model.VersionDetail.PrimaryPriceTypeId;
                                    ProductVersion.SecondaryPriceTypeId = model.VersionDetail.SecondaryPriceTypeId;
                                    ProductVersion.PricingSourceID = model.VersionDetail.PricingSourceId;
                                    ProductVersion.SecurityMER = model.VersionDetail.SecurityMER / 100;
                                    ProductVersion.ProductMER = model.VersionDetail.ProductMER / 100;
                                    ProductVersion.ProductReimbursable = model.VersionDetail.ProductReimbursable / 100;
                                    ProductVersion.TargetReturnRate = model.VersionDetail.TargetReturnRate;
                                    // Maintaining Log
                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductVersion", ObjectID = ProductVersion.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                                }

                                foreach (var productBrokerClass in product.ProductBrokers)
                                {
                                    if (productBrokerClass.IsDeleted == false)
                                    {
                                        deletedProductList.Add(productBrokerClass.Id, true);
                                    }
                                    productBrokerClass.IsDeleted = true;
                                }

                                if (model.ProductBrokerIds != null)
                                {
                                    foreach (var productBrokerId in model.ProductBrokerIds)
                                    {
                                        if (productBrokerId != null)
                                        {
                                            var productBroker = product.ProductBrokers.FirstOrDefault(a => a.InstitutionId == productBrokerId);
                                            if (productBroker != null)
                                            {
                                                //update
                                                productBroker.ProductId = model.ProductID;
                                                productBroker.InstitutionId = productBrokerId;
                                                productBroker.IsDeleted = false;
                                                uow.Commit();
                                                // Maintaining Log
                                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductBroker", ObjectID = productBroker.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                                                if (deletedProductList.ContainsKey(productBroker.Id))
                                                {
                                                    deletedProductList[productBroker.Id] = productBroker.IsDeleted;
                                                }
                                            }
                                            else
                                            {
                                                //add
                                                var proBroker = new ProductBroker
                                                {
                                                    ProductId = model.ProductID,
                                                    InstitutionId = productBrokerId,
                                                };
                                                _productBrokeRepository.Add(proBroker);
                                                uow.Commit();
                                                // Maintaining Log
                                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductBroker", ObjectID = proBroker.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                            }
                                        }

                                    }

                                    foreach (var item in deletedProductList.Where(w => w.Value == true))
                                    {
                                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductBroker", ObjectID = item.Key, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });
                                    }
                                }

                                //foreach (var productAssociationsClass in product.ProductAssociations)
                                // product.ProductVersions.Where(w => w.ProductID == product.ProductID && w.StatusID == 1).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductAssociations
                                foreach (var productAssociationsClass in product.ProductVersions.Where(w => w.ProductVersionID == ProductVersion.ProductVersionID).FirstOrDefault().ProductAssociations)
                                {
                                    if (productAssociationsClass.IsDeleted == false)
                                    {
                                        deletedProductAssociationList.Add(productAssociationsClass.Id, true);
                                    }
                                    productAssociationsClass.IsDeleted = true;
                                }

                                // Security id as Association id
                                if (model.SecurityAssociation != null)
                                {
                                    foreach (var secAssociation in model.SecurityAssociation)
                                    {
                                        if (secAssociation != null)
                                        {
                                            //for security
                                            if (secAssociation.SecurityId != null)
                                            {
                                                //var security = product.ProductAssociations.FirstOrDefault(a => a.SecurityId == secAssociation.SecurityId && product.ProductID == model.ProductID);
                                                var security = product.ProductVersions.Where(pv => pv.ProductVersionID == ProductVersion.ProductVersionID).FirstOrDefault().ProductAssociations.FirstOrDefault(a => a.SecurityId == secAssociation.SecurityId && product.ProductID == model.ProductID);
                                                if (security != null)
                                                {
                                                    //update
                                                    //security.ProductId = model.ProductID;
                                                    security.ProductVersionID = ProductVersion.ProductVersionID;
                                                    security.SecurityId = secAssociation.SecurityId;
                                                    security.Allocation = secAssociation.Allocation;
                                                    security.IsDeleted = false;
                                                    uow.Commit();
                                                    // Maintaining Log
                                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = security.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });

                                                    if (deletedProductAssociationList.ContainsKey(security.Id))
                                                    {
                                                        deletedProductAssociationList[security.Id] = security.IsDeleted;
                                                    }
                                                }
                                                else
                                                {
                                                    //add
                                                    var proSecurity = new ProductAssociation
                                                    {
                                                        //ProductId = model.ProductID,
                                                        ProductVersionID = ProductVersion.ProductVersionID,
                                                        SecurityId = secAssociation.SecurityId,
                                                        Allocation = secAssociation.Allocation,
                                                        //ProductVersionID = productVersion.ProductVersionID
                                                    };
                                                    _productAssociationRepository.Add(proSecurity);
                                                    uow.Commit();
                                                    // Maintaining Log
                                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = proSecurity.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                                }
                                            }


                                            // for securityListId
                                            if (secAssociation.SecurityListId != null)
                                            {
                                                var securityList = product.ProductVersions.Where(pv => pv.ProductVersionID == ProductVersion.ProductVersionID).FirstOrDefault().ProductAssociations.FirstOrDefault(a => a.SecurityListId == secAssociation.SecurityListId && product.ProductID == model.ProductID);
                                                if (securityList != null)
                                                {
                                                    //update
                                                    //securityList.ProductId = model.ProductID;
                                                    securityList.ProductVersionID = ProductVersion.ProductVersionID;
                                                    securityList.SecurityListId = secAssociation.SecurityListId;
                                                    securityList.Allocation = secAssociation.Allocation;
                                                    securityList.IsDeleted = false;
                                                    uow.Commit();
                                                    // Maintaining Log
                                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = securityList.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });

                                                    if (deletedProductAssociationList.ContainsKey(securityList.Id))
                                                    {
                                                        deletedProductAssociationList[securityList.Id] = securityList.IsDeleted;
                                                    }
                                                }
                                                else
                                                {
                                                    //add
                                                    var proSecurityList = new ProductAssociation
                                                    {
                                                        //ProductId = model.ProductID,
                                                        ProductVersionID = ProductVersion.ProductVersionID,
                                                        SecurityListId = secAssociation.SecurityListId,
                                                        Allocation = secAssociation.Allocation,
                                                    };
                                                    _productAssociationRepository.Add(proSecurityList);
                                                    uow.Commit();
                                                    // Maintaining Log
                                                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = proSecurityList.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                                                }
                                            }

                                        }

                                    }

                                    foreach (var item in deletedProductAssociationList.Where(w => w.Value == true))
                                    {
                                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductAssociation", ObjectID = item.Key, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });
                                    }
                                }

                                //uow.Commit();
                                uow.Dispose();
                                responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Product");

                                #endregion
                            }
                        }
                        else
                        {
                            responseModel.Status = ResponseStatus.Failure;
                            responseModel.Message = "Product already exists.";
                        }
                    }
                    else
                    {
                        responseModel.Status = ResponseStatus.Failure;
                        responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "Product");
                    }

                    #endregion
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

        private IViewModel DeleteProduct(IViewModel baseModel)
        {
            var model = (ProductModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success
            };
            try
            {
                if (model.BaseProductVersionID != 0)
                {
                    var productVersion = _productVersionRepository.GetById(model.BaseProductVersionID);
                    if (productVersion != null)
                    {
                        productVersion.IsDeleted = true;

                        uow.Commit();
                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductVersion", ObjectID = productVersion.ProductVersionID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });

                        uow.Dispose();
                        responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "ProductVersion");
                    }
                    else
                    {
                        responseModel.Status = ResponseStatus.Failure;
                        responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "ProductVersion");
                    }
                }

                else
                {
                    var product = _productRepository.GetById(model.ProductID);
                    if (product != null)
                    {
                        product.IsDeleted = true;

                        uow.Commit();
                        // Maintaining Log
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Product", ObjectID = product.ProductID, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });

                        uow.Dispose();
                        responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Product");
                    }
                    else
                    {
                        responseModel.Status = ResponseStatus.Failure;
                        responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "Product");
                    }
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

        protected bool CheckWarnings(ProductModel model)
        {
            var productAssociation = uow.Repository<Database.ProductAssociation>();
            var securityListDetail = uow.Repository<Database.SecurityListDetail>().GetAll(w => w.IsDeleted == false);
            var securityAssetClass = uow.Repository<Database.SecurityAssetClass>().GetAll(w => w.IsDeleted == false);
            var security = uow.Repository<Database.Security>().GetAll(w => w.IsDeleted == false);
            List<ProductModel> result = new List<ProductModel>();

            if (model.SecurityAssociation != null)
            {
                foreach (var secAssociation in model.SecurityAssociation)
                {
                    if (secAssociation.SecurityId != null)
                    {
                        result.AddRange(from s in security
                                        where
                                            s.Id == secAssociation.SecurityId &&
                                            !
                                                (from sa in securityAssetClass
                                                 where
                                                     sa.SecurityId == secAssociation.SecurityId && sa.IsDeleted != true
                                                 select new
                                                 {
                                                     sa.AssetClassId
                                                 }).Contains(new { AssetClassId = model.AssetClassId })
                                        select new ProductModel
                                        {
                                            SecName = s.Code
                                        });
                    }

                    if (secAssociation.SecurityListId != null)
                    {
                        result.AddRange(from s in securityListDetail
                                        where
                                            s.SecurityListId == secAssociation.SecurityListId &&
                                            !
                                                (from sa in securityAssetClass
                                                 where
                                                     sa.SecurityId == s.SecurityId && sa.IsDeleted != true
                                                 select new
                                                 {
                                                     sa.AssetClassId
                                                 }).Contains(new { AssetClassId = model.AssetClassId })
                                        group s by s.Security.Code into detail
                                        select new ProductModel
                                        {
                                            SecName = detail.Key,
                                            StatusDate = detail.Max(x => x.StatusDate)
                                        });

                    }
                }
            }

            decimal? totalAllocation = productAssociation.GetAll(w => w.IsDeleted == false && w.ProductVersionID == model.VersionDetail.ProductVersionID).Sum(s => s.Allocation);
            if (result.Count > 0 || totalAllocation > 100)
            {
                return true;
            }

            return false;
        }

        public class ProductWeight
        {
            public int ProductId { get; set; }
            public ICollection<ProductPrice> Prices { get; set; }
            public decimal? Index { get; set; }
        }

        // For Dashboard Purpose
        private List<ProductModel> GetProductDashboard(ProductModel model)
        {
            var productVersion = _productVersionRepository.Where(w => w.StatusID == 1 && !w.IsDeleted && w.Product.IsDeleted == false).ToList();
            if (!model.IsAll)
            {
                productVersion = productVersion.Where(n => n.Product.ProductTypeId != 3).ToList();
            }
            var productIds = productVersion.GroupBy(m => m.ProductID).Select(group => @group.First()).Select(item => item.ProductID).ToList();
            var returns = _productReturnRepository.GetAll();
            var statistics = _productStatisticRepository.GetAll();
            
            Dictionary<int, decimal?> indexes = new Dictionary<int,decimal?>();

            foreach (var item in _productRepository.GetAll(w => w.IsDeleted == false).Where(w => productIds.Contains(w.ProductID)))
            {
                indexes.Add(item.ProductID, (item.ProductPrices != null && item.ProductPrices.Count > 0) ? item.ProductPrices.OrderBy(o => o.Date).Last().TRPrice : null);
            }

            var result = _productRepository.GetAll(w => w.IsDeleted == false).Where(w => productIds.Contains(w.ProductID)).Select(p =>
                    new ProductModel
                    {
                        ProductID = p.ProductID,
                        Code = p.Code,
                        Name = p.Name,
                        ProductTypeId = p.ProductTypeId,
                        ProductType = p.ProductType.Type,
                        //Index = indexes.ContainsKey(p.ProductID) && indexes[p.ProductID].HasValue ? indexes[p.ProductID].Value : new Decimal(),
                        IndexTypeId = p.IndexTypeId,
                        IndexType = p.IndexType.Type,
                        ProductAPIR = p.ProductAPIR,
                        ProductISIN = p.ProductISIN,
                        InstitutionId = p.InstitutionId,
                        Institution = p.Institution.Code,
                        MarketId = p.MarketId,
                        Market = p.Market.Code,
                        CurrencyId = p.CurrencyId,
                        Currency = p.Currency.Code,
                        SubAssetClassId = p.SubAssetClassId,
                        SubAssetClass = p.SubAssetClass.Class,
                        RegionId = p.RegionId,
                        Region = p.Region.RegionName,
                        StatusId = p.StatusId,
                        Status = p.SecurityStatu.Status,
                        AssetClassId = p.AssetClassId,
                        ProductBrokerIds = p.ProductBrokers.Where(w => w.IsDeleted == false).Select(r => r.InstitutionId).ToList(),
                        YearReturn = returns.Where(item => item.ProductId == p.ProductID).Select(i => i.YearReturn == null ? i.YearReturn : Math.Round(i.YearReturn.Value * 100, 4)).FirstOrDefault(),
                        MonthReturn = returns.Where(item => item.ProductId == p.ProductID).Select(i => i.MonthReturn == null ? i.MonthReturn : Math.Round(i.MonthReturn.Value * 100, 4)).FirstOrDefault(),
                        
                        VersionDetail = new ProductVersionModel
                        {
                            ProductVersionID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductVersionID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductVersionID : 0,
                            ProductICR = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? (p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityMER + p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductMER) * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityMER + p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductMER) * 100 : 0,
                            ProductReimbursable = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? (p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductReimbursable) * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? (p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductReimbursable) * 100 : 0,
                            StartDate = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).StartDate : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().StartDate : null,
                            IsPriced = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).IsPriced : p.ProductVersions.Any(ww => ww.StatusID == 4) && p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().IsPriced,
                            PricingSourceId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PricingSourceID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PricingSourceID : null,
                            PricingSource = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PricingSource.Source : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PricingSource.Source : string.Empty,
                            SecurityMER = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityMER * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityMER * 100 : null,
                            ProductMER = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).ProductMER * 100 : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().ProductMER * 100 : null,
                            IsOnlyVersion = p.ProductVersions.Count(ww => ww.ProductID == p.ProductID && ww.IsDeleted == false) <= 1,
                            StatusID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).StatusID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().StatusID : null,
                            ProductVersionStatus = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecurityStatu.Status : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecurityStatu.Status : null,
                            PrimaryBenchmarkProductID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PrimaryBenchmarkProductID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PrimaryBenchmarkProductID : null,
                            SecondaryBenchmarkProductID = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecondaryBenchmarkProductID : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecondaryBenchmarkProductID : null,
                            PrimaryPriceTypeId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).PrimaryPriceTypeId : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().PrimaryPriceTypeId : null,
                            SecondaryPriceTypeId = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).SecondaryPriceTypeId : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().SecondaryPriceTypeId : null,
                            TargetReturnRate = p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1) != null ? p.ProductVersions.FirstOrDefault(ww => ww.StatusID == 1).TargetReturnRate : p.ProductVersions.Any(ww => ww.StatusID == 4) ? p.ProductVersions.Where(ww => ww.StatusID == 4).OrderByDescending(o => o.MajorVersion).FirstOrDefault().TargetReturnRate : null,

                        },
                        Statistic = statistics.Where(item => item.ProductId == p.ProductID).Select(i => new ProductStatisticModel
                        {
                            ProductId = i.ProductId,
                            Volatility = i.Volatility,
                            Valution = i.Valution,
                            DividendYield = i.DividendYield
                        }).FirstOrDefault(),
                    }).ToList();

            foreach (var item in result)
            {
                if (indexes.ContainsKey(item.ProductID) && indexes[item.ProductID].HasValue)
                {
                    item.Index = indexes[item.ProductID].Value;
                }
            }

            return result;
        }

    }
}