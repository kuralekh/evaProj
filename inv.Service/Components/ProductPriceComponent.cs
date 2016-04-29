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
using Invest.Web.Models;

namespace Invest.Service.Components
{
    public class ProductPriceComponent : BaseComponent
    {
        private readonly IRepository<ProductPrice> _productPriceRepository;
        private readonly IRepository<Product> _productRepository;

        public ProductPriceComponent()
        {
            _productPriceRepository = uow.Repository<ProductPrice>();
            _productRepository = uow.Repository<Product>();
        }

        private IViewModel GetProductPrice(IViewModel baseModel, FilterOption filterOption)
        {
            var model = (ProductPriceModel)baseModel;
            var responseModel = new ResponseModel<ProductPriceModel>
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
                    case FilterOption.GetAllById:
                        responseModel.ModelList = GetAllById(model);
                        break;
                    case FilterOption.GetAllByIds:
                        responseModel.ModelList = GetAllByIds(model);
                        break;
                    case FilterOption.ModelListByFields:
                        responseModel.ModelList = GetModelListByModelFields(model);
                        break;
                    case FilterOption.ModelByFields:
                        responseModel.Model = GetModelByModelFields(model);
                        break;
                    case FilterOption.ModelListByFieldsDateRange:
                        responseModel.ModelList = GetModelListByModelFieldsDateRange(model);
                        break;
                    case FilterOption.GetAllPagination:
                        int totalRecords;
                        responseModel.ModelList = GetPaginatedData(model, out totalRecords);
                        responseModel.RecordCount = totalRecords;
                        break;
                    case FilterOption.CalculatePrice:
                        responseModel.Model = CalculateProductPrice(model);
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
        
        private List<ProductPriceModel> GetPaginatedData(ProductPriceModel model, out int totalRecords)
        {
            List<ProductPriceModel> query;
            var result = GetModelListByModelFieldsDateRange(model);


            if (string.IsNullOrWhiteSpace(model.TableParam.sSearch))
            {
                query = result.Select(p => new ProductPriceModel
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Date = System.Convert.ToDateTime(p.Date),
                    CapitalPrice = p.CapitalPrice,
                    IncomePrice = p.IncomePrice,
                    TRPrice = p.TRPrice

                }).ToList();
            }
            else
            {
                query = result.Select(p => new ProductPriceModel
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Date = System.Convert.ToDateTime(p.Date),
                    CapitalPrice = p.CapitalPrice,
                    IncomePrice = p.IncomePrice,
                    TRPrice = p.TRPrice

                }).Where(s => s.ProductCode != null && s.ProductCode.ToLower().Contains(model.TableParam.sSearch.ToLower())
                    || s.ProductName != null && s.ProductName.ToLower().Contains(model.TableParam.sSearch.ToLower())
                    || s.Date != null && string.Format("{0:dd/MM/yyyy}", s.Date).Contains(model.TableParam.sSearch)
                    || s.CapitalPrice != null && s.CapitalPrice.ToString().Contains(model.TableParam.sSearch)
                    || s.IncomePrice != null && s.IncomePrice.ToString().Contains(model.TableParam.sSearch)
                    || s.TRPrice != null && s.TRPrice.ToString().Contains(model.TableParam.sSearch)).ToList();

            }

            List<ProductPriceModel> filtered = CustomFilter(model, query);
            totalRecords = filtered.Count;

            return model.IsDownload ? filtered : SortingTableData(model, filtered);

        }

        private static List<ProductPriceModel> SortingTableData(ProductPriceModel model, List<ProductPriceModel> filtered)
        {
            if (model.TableParam.iSortingCols == 1)
            {
                if (model.TableParam.iSortCol_0 == 0 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(od => od.ProductCode).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 0 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(od => od.ProductCode).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 1 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(od => od.ProductName).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 1 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(od => od.ProductName).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.Date).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.Date).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.CapitalPrice).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "desc")
                {
                    return
                        filtered.OrderByDescending(p => p.CapitalPrice)
                            .Skip(model.TableParam.iDisplayStart)
                            .Take(model.TableParam.iDisplayLength)
                            .ToList();
                }

                if (model.TableParam.iSortCol_0 == 4 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.IncomePrice).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 4 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.IncomePrice).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 5 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.TRPrice).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 5 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.TRPrice).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }


                return filtered.OrderBy(x => x.ProductCode).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
            }
            return filtered.OrderBy(od => od.ProductCode).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
        }

        private static List<ProductPriceModel> CustomFilter(ProductPriceModel model, List<ProductPriceModel> query)
        {
            List<ProductPriceModel> filtered = query;

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_0))
            {
                filtered = filtered.Where(s => s.ProductCode != null && s.ProductCode.ToLower().Contains(model.TableParam.sSearch_0.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_1))
            {
                filtered = filtered.Where(s => s.ProductName != null && s.ProductName.ToLower().Contains(model.TableParam.sSearch_1.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_2))
            {
                filtered = filtered.Where(s => s.Date != null && string.Format("{0:dd/MM/yyyy}", s.Date).Contains(model.TableParam.sSearch_2)).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_3))
            {
                filtered = filtered.Where(s => s.CapitalPrice != null && s.CapitalPrice.ToString().Contains(model.TableParam.sSearch_3)).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_4))
            {
                filtered = filtered.Where(s => s.IncomePrice != null && s.IncomePrice.ToString().Contains(model.TableParam.sSearch_4)).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_5))
            {
                filtered = filtered.Where(s => s.TRPrice != null && s.TRPrice.ToString().Contains(model.TableParam.sSearch_5)).ToList();
            }

            return filtered;
        }

        public override IViewModel Get(IViewModel model, FilterOption filterOption)
        {
            return GetProductPrice(model, filterOption);
        }

        public override IViewModel Add(IViewModel model)
        {
            return AddProductPrice(model);
        }

        public override IViewModel Update(IViewModel model)
        {
            return UpdateProductPrice(model);
        }

        public override IViewModel Delete(IViewModel model)
        {
            return DeleteProductPrice(model);
        }

        protected List<ProductPriceModel> GetAllModelList()
        {
            return _productPriceRepository.GetAll(w => w.IsDeleted == false).Select(s =>
                new ProductPriceModel
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductCode = s.Product.Code,
                    ProductName = s.Product.Name,
                    Date = s.Date,
                    CapitalPrice = s.CapitalPrice,
                    IncomePrice = s.IncomePrice,
                    TRPrice = s.TRPrice,
                    IsDeleted = s.IsDeleted,
                }).ToList();
        }

        protected List<ProductPriceModel> GetAllById(ProductPriceModel model)
        {
            List<ProductPriceModel> query = GetModelListByModelFieldsDateRange(model);
            return query;
        }

        protected List<ProductPriceModel> GetAllByIds(ProductPriceModel model)
        {            
            List<int> productsIds = model.TableParam.sIdsSearch;
            List<ProductPriceModel> result = new List<ProductPriceModel>();

            result = _productPriceRepository.Where(w => w.IsDeleted == false && w.Product.IsDeleted == false && productsIds.Contains(w.ProductId.Value) &&
                                                    w.Date >= (model.FromDate ?? new DateTime()) &&
                                                    w.Date <= (model.ToDate ?? DateTime.Now))
                                                    .Select(s => new ProductPriceModel
                                                    {
                                                        Id = s.Id,
                                                        ProductId = s.ProductId,
                                                        Date = s.Date,
                                                        CapitalPrice = s.CapitalPrice,
                                                        TRPrice = s.TRPrice,
                                                    }).OrderBy(i => i.ProductId).ThenBy(i => i.Date).ToList();
            return result;
        }

        protected List<ProductPriceModel> GetModelListByModelFields(ProductPriceModel modelWithValue)
        {
            return _productPriceRepository.GetAll(w => w.IsDeleted == false).Where(Createfilter<ProductPrice>(modelWithValue))
                .Select(s =>
                    new ProductPriceModel
                    {
                        Id = s.Id,
                        ProductId = s.ProductId,
                        ProductName = s.Product.Name,
                        ProductCode = s.Product.Code,
                        Date = s.Date,
                        CapitalPrice = s.CapitalPrice,
                        IncomePrice = s.IncomePrice,
                        TRPrice = s.TRPrice,
                        IsDeleted = s.IsDeleted,
                    }).ToList();
        }

        protected List<ProductPriceModel> GetModelListByModelFieldsDateRange(ProductPriceModel modelWithValue)
        {
            IQueryable<ProductPrice> spList;

            if (modelWithValue.ProductId == 0 && (modelWithValue.FromDate == null || modelWithValue.ToDate == null))
            {
                spList = _productPriceRepository.GetAll(w => w.IsDeleted == false && w.Product.IsDeleted == false);
            }
            else if (modelWithValue.ProductId == 0)
            {
                spList = _productPriceRepository.GetAll(w => w.IsDeleted == false && w.Product.IsDeleted == false).Where(s => s.Date >= modelWithValue.FromDate && s.Date <= modelWithValue.ToDate);
            }
            else if (modelWithValue.FromDate == null || modelWithValue.ToDate == null)
            {
                spList = _productPriceRepository.GetAll(w => w.IsDeleted == false && w.Product.IsDeleted == false).Where(Createfilter<ProductPrice>(modelWithValue));
            }
            else
            {
                spList = _productPriceRepository.GetAll(w => w.IsDeleted == false && w.Product.IsDeleted == false).Where(Createfilter<ProductPrice>(modelWithValue)).Where(s => s.Date >= modelWithValue.FromDate && s.Date <= modelWithValue.ToDate);
            }

            return spList.Select(s =>
                            new ProductPriceModel
                            {
                                Id = s.Id,
                                ProductId = s.ProductId,
                                ProductCode = s.Product.Code,
                                ProductName = s.Product.Name,
                                Date = s.Date,
                                CapitalPrice = s.CapitalPrice,
                                IncomePrice = s.IncomePrice,
                                TRPrice = s.TRPrice,
                                IsDeleted = s.IsDeleted,
                            }).ToList();

        }

        protected ProductPriceModel GetModelByModelFields(ProductPriceModel modelWithValue)
        {
            return _productPriceRepository.GetAll(w => w.IsDeleted == false).Where(Createfilter<ProductPrice>(modelWithValue))
                .Select(s =>
                    new ProductPriceModel
                    {
                        Id = s.Id,
                        ProductId = s.ProductId,
                        ProductCode = s.Product.Code,
                        ProductName = s.Product.Name,
                        Date = s.Date,
                        CapitalPrice = s.CapitalPrice,
                        IncomePrice = s.IncomePrice,
                        TRPrice = s.TRPrice,
                        IsDeleted = s.IsDeleted,
                    }).FirstOrDefault();
        }

        public IViewModel AddProductPrice(IViewModel baseModel)
        {
            var model = (ProductPriceModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success,
            };

            try
            {
                var entry = _productPriceRepository.Where(w => w.ProductId == model.ProductId && w.Date == model.Date).FirstOrDefault();
                if (entry != null)
                {
                    entry.CapitalPrice = model.CapitalPrice;
                    entry.IncomePrice = model.IncomePrice;
                    entry.TRPrice = model.TRPrice;
                    entry.ImportSource = model.ImportSource;
                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductStatistic", ObjectID = entry.ProductId, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                    uow.Dispose();
                }
                else
                {
                    var productPrice = new ProductPrice
                    {
                        ProductId = model.ProductId,
                        Date = model.Date,
                        CapitalPrice = model.CapitalPrice,
                        IncomePrice = model.IncomePrice,
                        TRPrice = model.TRPrice,
                        ImportSource = model.ImportSource
                    };

                    _productPriceRepository.Add(productPrice);
                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductPrice", ObjectID = productPrice.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });
                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordAddedSuccessfully, "Product Price");
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
        public IViewModel UpdateProductPrice(IViewModel baseModel)
        {
            var model = (ProductPriceModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success
            };
            try
            {
                var productPrice = _productPriceRepository.GetById(model.Id);
                if (productPrice != null)
                {
                    productPrice.ProductId = model.ProductId;
                    productPrice.Date = model.Date;
                    productPrice.CapitalPrice = model.CapitalPrice;
                    productPrice.IncomePrice = model.IncomePrice;
                    productPrice.TRPrice = model.TRPrice;
                    productPrice.ImportSource = model.ImportSource;

                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductPrice", ObjectID = productPrice.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Product Price");
                }
                else
                {
                    responseModel.Status = ResponseStatus.Failure;
                    responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "Product Price");
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

        private IViewModel DeleteProductPrice(IViewModel baseModel)
        {
            var model = (ProductPriceModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success
            };
            try
            {
                var productPrice = _productPriceRepository.GetById(model.Id);
                if (productPrice != null)
                {
                    productPrice.IsDeleted = true;

                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "ProductPrice", ObjectID = productPrice.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });
                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Product Price");
                }
                else
                {
                    responseModel.Status = ResponseStatus.Failure;
                    responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "Product Price");
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

        #region ProductPrice
        private ProductPriceModel CalculateProductPrice(ProductPriceModel model)
        {
            if (!model.ProductId.HasValue)
                return new ProductPriceModel { PriceCalculationStatus = -1 };
           
            var productId = (int) model.ProductId;

            var product = _productRepository.Where(p => p.ProductID == productId).FirstOrDefault(w => w.IsDeleted == false);

            if (!product.InceptionDate.HasValue)
            {
                throw new Exception("Product InceptionDate is empty");
            }

            List<ProductPriceModel> productPricies = ProductPriceDataById(productId, null, null);

            List<int> secIds = new List<int>();
            Dictionary<int, decimal> securityIds = new Dictionary<int, decimal>();

            Dictionary<DateTime, ProductDayChange> newPricies2 = new Dictionary<DateTime, ProductDayChange>();

            DateTime fromDate = model.FromDate ?? new DateTime();
            DateTime toDate = model.ToDate ?? new DateTime();

            decimal currentCapitalPrice = 1000M;
            decimal currentTR = 1000M;

            List<Versions> vesionsForCalculation = new List<Versions>();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<ProductVersion> versions = product.ProductVersions.Where(w => w.IsDeleted == false).ToList().ToList();


            if (versions.Count > 0)
            {
                if (productPricies.Count == 0)
                {
                    currentCapitalPrice = 1000M;
                    currentTR = 1000M;

                    if (versions.First().StartDate.HasValue)
                    {
                        if (fromDate <= versions.First().StartDate.Value)
                            fromDate = versions.First().StartDate.Value;
                    }
                    else if (!versions.First().StartDate.HasValue)
                    {
                        if (product.InceptionDate.HasValue)
                            fromDate = product.InceptionDate.Value;
                        else
                            return new ProductPriceModel { PriceCalculationStatus = -3 }; //Version and InceptionDate don't have inceptions date
                    }
                    newPricies2.Add(fromDate, new ProductDayChange(currentCapitalPrice, currentTR));
                }

                if (productPricies.Count != 0)
                {
                    var prodPriceModel = productPricies.Where(w => w.Date <= fromDate).OrderByDescending(w => w.Date).FirstOrDefault();
                    if (prodPriceModel != null)
                    {
                        currentTR = prodPriceModel.TRPrice ?? 0;
                        fromDate = prodPriceModel.Date ?? new DateTime();
                    }

                }
            }

            //Filring
            vesionsForCalculation = FiltredVersion(versions, fromDate, toDate);

            if (vesionsForCalculation.Count < 1)
            {
                return new ProductPriceModel { PriceCalculationStatus = -2 }; ;
            }

            //vesionsForCalculation = FiltredVersion(versions, fromDate, toDate);

            foreach (var version in vesionsForCalculation)
            {
                var accociation = version.Version.ProductAssociations.Where(w => w.IsDeleted == false).ToList();

                foreach (var item in accociation)
                {
                    if (item.SecurityId.HasValue && item.Allocation != null)
                    {
                        if (securityIds.ContainsKey(item.SecurityId.Value))
                            securityIds[item.SecurityId.Value] += item.Allocation / 100 ?? 0.0M;
                        else
                            securityIds.Add(item.SecurityId.Value, item.Allocation / 100 ?? 0.0M);
                    }

                    if (item.SecurityListId.HasValue)
                    {
                        decimal alloc = item.Allocation / 100 ?? 0;

                        foreach (var s in item.SecurityList.SecurityListDetails.Where(w => w.IsDeleted == false).ToList())
                        {
                            if (s.SecurityId.HasValue)
                            {
                                if (securityIds.ContainsKey(s.SecurityId.Value))
                                    securityIds[s.SecurityId.Value] += s.Allocation / 100 * alloc ?? 0.0M;
                                else
                                    securityIds.Add(s.SecurityId.Value, s.Allocation / 100 * alloc ?? 0.0M);
                            }
                        }
                    }
                }
                List<SecReturns> list = new List<SecReturns>();

                List<SecurityPriceModel> securALL = GetPriciesByIds(new List<int>(securityIds.Keys), null, toDate);
                List<SecurityPriceModel> secItems = new List<SecurityPriceModel>();

                if (version.Version.MajorVersion > 1)
                {
                    foreach (var item in securityIds)
                    {
                        secItems.Add(securALL.Where(w => w.Date < version.From && w.SecurityId == item.Key).OrderByDescending(w => w.Date).FirstOrDefault());
                    }
                }

                secItems.AddRange(securALL.Where(w => w.Date >= version.From && w.Date <= version.To).ToList());

                foreach (var item in securityIds)
                {
                    int secId = item.Key;
                    decimal alloc = item.Value;
                    var l = secItems.Where(w => w.SecurityId == secId).ToList();

                    List<DistributionDividendDetailModel> dividendsList = GetDividendsList(secId, version.From, version.To);
                    list.Add(new SecReturns(secId, secItems.Where(w => w.SecurityId == secId).ToList(), dividendsList, alloc));
                }

                Dictionary<DateTime, ProductDayChange> t2 = new Dictionary<DateTime, ProductDayChange>();

                foreach (var item in list)
                {
                    foreach (var currentSecur in item.totalReturns.ToList())
                    {
                        if (t2.ContainsKey(currentSecur.Key))
                        {
                            t2[currentSecur.Key].Capital += currentSecur.Value.CapitalByWeight;
                            t2[currentSecur.Key].Total += currentSecur.Value.Total;
                        }
                        else
                        {
                            ProductDayChange p = new ProductDayChange(currentSecur.Value.CapitalByWeight, currentSecur.Value.Total);
                            t2.Add(currentSecur.Key, p);
                        }
                    }
                }

                foreach (var item in version.Version.MajorVersion > 1 ? t2.Where(w => w.Key >= version.From && w.Key < version.To).ToList() : t2.Where(w => w.Key > version.From && w.Key < version.To).ToList())
                {
                    currentCapitalPrice = currentCapitalPrice * (1 + item.Value.Capital); ;
                    currentTR = currentTR * (1 + item.Value.Total);

                    if (!newPricies2.ContainsKey(item.Key))
                    {
                        newPricies2.Add(item.Key, new ProductDayChange(currentCapitalPrice, currentTR));
                    }

                }

                securityIds.Clear();
            }
            sw.Stop();

            foreach (var item in newPricies2)
            {
                ProductPriceModel productPriceModel = new ProductPriceModel
                {
                    ProductId = productId,
                    CapitalPrice = item.Value.Capital,
                    IncomePrice = item.Value.Total - item.Value.Capital,
                    TRPrice = item.Value.Total,
                    Date = item.Key,
                    ImportSource = "Price Calculation"
                };

                AddProductPrice(productPriceModel);
            }

            return new ProductPriceModel { PriceCalculationStatus = newPricies2.Count() }; ;
        }

        public List<Versions> FiltredVersion(List<ProductVersion> versions, DateTime from, DateTime to)
        {
            List<Versions> versis = new List<Versions>();

            foreach (var version in versions)
            {
                DateTime localFrom = new DateTime();
                DateTime localto = new DateTime();

                if (version.StartDate.HasValue && to < version.StartDate.Value
                    || version.IsPriced == false
                    || !version.StartDate.HasValue)
                {
                    continue;
                }
                //set from

                if (version.StartDate.HasValue && from < version.StartDate.Value)
                {
                    localFrom = version.StartDate.Value;
                }
                else if (version.StartDate.HasValue && from == version.StartDate.Value)
                {
                    localFrom = version.StartDate.Value;
                }
                else if (version.StartDate.HasValue && from > version.StartDate.Value)
                {
                    localFrom = from;
                }
                else if (!version.StartDate.HasValue)
                {
                    localFrom = from;
                }

                //set to
                if (!version.EndDate.HasValue)
                {
                    localto = DateTime.Now;
                }
                else if (to > version.EndDate.Value || to == version.EndDate.Value)
                {
                    localto = version.EndDate.Value;
                }
                else if (to < version.EndDate.Value)
                {
                    localto = to;
                }

                localFrom = new DateTime(localFrom.Year, localFrom.Month, localFrom.Day);
                localto = new DateTime(localto.Year, localto.Month, localto.Day);

                Versions v = new Versions { Version = version, From = localFrom, To = localto };
                versis.Add(v);
            }
            return versis;
        }

        private List<ProductPriceModel> ProductPriceDataById(int? productId, DateTime? fromDate, DateTime? toDate)
        {
            JQueryDataTableParamModel param = new JQueryDataTableParamModel();
            param.FromDate = fromDate;
            param.ToDate = toDate;

            var modelView = new ProductPriceModel
            {
                FilterOption = FilterOption.GetAllById,
                TableParam = param,
                ProductId = productId ?? 0,
                FromDate = fromDate,
                ToDate = toDate
            };

            return GetAllById(modelView);
        }
        #endregion
    }
}
