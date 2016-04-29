using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Invest.Common;
using Invest.Common.Enumerators;
using Invest.Common.Extensions;
using Invest.Database;
using Invest.ViewModel;
using Invest.ViewModel.Models;

namespace Invest.Service.Components
{
    public class SecurityPriceComponent : BaseComponent
    {
        private readonly IRepository<SecurityPrice> _securitypriceRepository;
        private readonly IRepository<Attachment> _attachamentRepository;
        private readonly IRepository<NAVSecurityAttachment> _navsecurityattachamentRepository;
        private readonly IRepository<AttachmentType> _attachamentTypeRepository; 
        public SecurityPriceComponent()
        {
            _securitypriceRepository = uow.Repository<SecurityPrice>();
            _attachamentRepository = uow.Repository<Attachment>();
            _navsecurityattachamentRepository = uow.Repository<NAVSecurityAttachment>();
            _attachamentTypeRepository = uow.Repository<AttachmentType>();
        }

        private IViewModel GetSecurityPrice(IViewModel baseModel, FilterOption filterOption)
        {
            var model = (SecurityPriceModel)baseModel;
            var responseModel = new ResponseModel<SecurityPriceModel>
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
                    case FilterOption.ModelListByFieldsDateRange:
                        responseModel.ModelList = GetModelListByModelFieldsDateRange(model);
                        break;
                    case FilterOption.GetAllPagination:
                        int totalRecords;
                        responseModel.ModelList = GetPaginatedData(model, out totalRecords);
                        responseModel.RecordCount = totalRecords;
                        break;
                    case FilterOption.GetAllById:
                        responseModel.ModelList = GetAllLastMonthPrices(model);
                        break;
                    case FilterOption.GetAllByIds:
                        responseModel.ModelList = GetAllByIds(model);
                        break;
                    case FilterOption.GetSecurityPriceBySecurityId:
                        responseModel.ModelList = GetAllBySecuirtyId(model);
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

        private List<SecurityPriceModel> GetPaginatedData(SecurityPriceModel model, out int totalRecords)
        {
            List<SecurityPriceModel> query;
            var result = GetModelListByModelFieldsDateRange(model);


            if (string.IsNullOrWhiteSpace(model.TableParam.sSearch))
            {
                query = result.Select(p => new SecurityPriceModel
                {
                    Id = p.Id,
                    SecurityCode = p.SecurityCode,
                    SecurityName = p.SecurityName,
                    Date = System.Convert.ToDateTime(p.Date),
                    UnitPrice = p.UnitPrice,
                    Currency = p.Currency,
                    InterestRate = p.InterestRate
                }).ToList();
            }
            else
            {
                query = result.Select(p => new SecurityPriceModel
                {
                    Id = p.Id,
                    SecurityCode = p.SecurityCode,
                    SecurityName = p.SecurityName,
                    Date = System.Convert.ToDateTime(p.Date),
                    UnitPrice = p.UnitPrice,
                    Currency = p.Currency,
                    InterestRate = p.InterestRate

                }).Where(s => s.SecurityCode != null && s.SecurityCode.ToLower().Contains(model.TableParam.sSearch.ToLower())
                    || s.SecurityName != null && s.SecurityName.ToLower().Contains(model.TableParam.sSearch.ToLower())
                    || s.Date != null && string.Format("{0:dd/MM/yyyy}", s.Date).Contains(model.TableParam.sSearch)
                    || s.UnitPrice.ToString(CultureInfo.InvariantCulture).Contains(model.TableParam.sSearch)
                    || s.Currency != null && s.Currency.ToLower().Contains(model.TableParam.sSearch.ToLower())
                    || s.InterestRate != null && s.InterestRate.Value.ToString(CultureInfo.InvariantCulture).Contains(model.TableParam.sSearch.ToLower())).ToList();

            }

            List<SecurityPriceModel> filtered = CustomFilter(model, query);
            totalRecords = filtered.Count;

            return model.IsDownload ? filtered : SortingTableData(model, filtered);

        }

        private static List<SecurityPriceModel> SortingTableData(SecurityPriceModel model, List<SecurityPriceModel> filtered)
        {
            if (model.TableParam.iSortingCols == 1)
            {
                if (model.TableParam.iSortCol_0 == 0 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(od => od.SecurityCode).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 0 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(od => od.SecurityCode).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 1 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.SecurityName).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 1 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.SecurityName).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }

                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "asc")
                {
                    return filtered.OrderBy(p => p.Date).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.TableParam.iSortCol_0 == 2 && model.TableParam.sSortDir_0 == "desc")
                {
                    return filtered.OrderByDescending(p => p.Date).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                }
                if (model.IsRateIndex)
                {
                    if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "asc")
                    {
                        return filtered.OrderBy(p => p.InterestRate).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                    }
                    if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "desc")
                    {
                        return filtered.OrderByDescending(p => p.InterestRate).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                    }
                }
                else
                {
                    if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "asc")
                    {
                        return filtered.OrderBy(p => p.UnitPrice).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                    }
                    if (model.TableParam.iSortCol_0 == 3 && model.TableParam.sSortDir_0 == "desc")
                    {
                        return filtered.OrderByDescending(p => p.UnitPrice).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                    }
                    if (model.TableParam.iSortCol_0 == 4 && model.TableParam.sSortDir_0 == "asc")
                    {
                        return filtered.OrderBy(p => p.Currency).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                    }
                    if (model.TableParam.iSortCol_0 == 4 && model.TableParam.sSortDir_0 == "desc")
                    {
                        return filtered.OrderByDescending(p => p.Currency).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
                    }
    
                }
            

                return filtered.OrderBy(x => x.SecurityCode).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
            }
            return filtered.OrderBy(od => od.SecurityCode).Skip(model.TableParam.iDisplayStart).Take(model.TableParam.iDisplayLength).ToList();
        }

        private static List<SecurityPriceModel> CustomFilter(SecurityPriceModel model, List<SecurityPriceModel> query)
        {
            List<SecurityPriceModel> filtered = query;

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_0))
            {
                filtered = filtered.Where(s => s.SecurityCode != null && s.SecurityCode.ToLower().Contains(model.TableParam.sSearch_0.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(model.TableParam.sSearch_1))
            {
                filtered = filtered.Where(s => s.SecurityName != null && s.SecurityName.ToLower().Contains(model.TableParam.sSearch_1.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(model.TableParam.sSearch_2))
            {
                filtered = filtered.Where(s => s.Date != null && string.Format("{0:dd/MM/yyyy}", s.Date).Contains(model.TableParam.sSearch_2)).ToList();
            }
            if (model.IsRateIndex)
            {
                if (!string.IsNullOrEmpty(model.TableParam.sSearch_3))
                {
                    filtered = filtered.Where(s => s.InterestRate.ToString().Contains(model.TableParam.sSearch_3)).ToList();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(model.TableParam.sSearch_3))
                {
                    filtered = filtered.Where(s => s.UnitPrice.ToString(CultureInfo.InvariantCulture).Contains(model.TableParam.sSearch_3)).ToList();
                }
                if (!string.IsNullOrEmpty(model.TableParam.sSearch_4))
                {
                    filtered = filtered.Where(s => s.Currency != null && s.Currency.ToLower().Contains(model.TableParam.sSearch_4.ToLower())).ToList();
                }
     
            }
          
            return filtered;
        }


        public override IViewModel Get(IViewModel model, FilterOption filterOption)
        {
            return GetSecurityPrice(model, filterOption);
        }

        public override IViewModel Add(IViewModel model)
        {
            return AddSecurityPrice(model);
        }

        public override IViewModel Update(IViewModel model)
        {
            return UpdateSecurityPrice(model);
        }

        public override IViewModel Delete(IViewModel model)
        {
            return DeleteSecurityPrice(model);
        }

        protected List<SecurityPriceModel> GetAllModelList()
        {
            return _securitypriceRepository.GetAll(w => w.IsDeleted == false && w.Security.IsDeleted == false).Select(s =>
                new SecurityPriceModel
                {
                    Id = s.Id,
                    SecurityId = s.SecurityId,
                    SecurityCode = s.Security.Code,
                    SecurityName = s.Security.Name,
                    Date = s.Date,
                    CurrencyId = s.CurrencyId,
                    Currency = s.Currency.Code ?? s.Security.Currency.Code,
                    UnitPrice = s.UnitPrice,
                    PriceNAV = s.PriceNAV,
                    PricePUR = s.PricePUR,
                    InterestRate = s.InterestRate,
                    ValuationTypeId = s.ValuationTypeId,
                    Valuer = s.Valuer,
                    Attachment = s.Attachment,
                    IsDeleted = s.IsDeleted,
                }).ToList();
        }

        internal List<SecurityPriceModel> GetAllByIds(SecurityPriceModel model)
        {
            List<int> securityIds = model.TableParam.sIdsSearch;
            List<SecurityPriceModel> result = new List<SecurityPriceModel>();

            result = _securitypriceRepository.Where(w => w.IsDeleted == false && w.Security.IsDeleted == false && securityIds.Contains(w.SecurityId) &&
                                                    w.Date >= (model.FromDate ?? new DateTime()) &&
                                                    w.Date <= (model.ToDate ?? DateTime.Now))
                                                    .Select(s => new SecurityPriceModel
                                                    {
                                                        Id = s.Id,
                                                        SecurityId = s.SecurityId,
                                                        Date = s.Date,
                                                        UnitPrice = s.UnitPrice,
                                                    }).OrderBy(i => i.SecurityId).ThenBy(i => i.Date).ToList();

            return result;
        }
        protected List<SecurityPriceModel> GetAllBySecuirtyId(SecurityPriceModel model)
        {
            List<SecurityPriceModel> result = new List<SecurityPriceModel>();

            result = _securitypriceRepository.Where(w => w.IsDeleted == false && w.Security.IsDeleted == false && w.SecurityId == model.SecurityId)
                                                    .Select(s => new SecurityPriceModel
                                                    {
                                                        Id = s.Id,
                                                        SecurityId = s.SecurityId,
                                                        Date = s.Date,
                                                        UnitPrice = s.UnitPrice,
                                                        Currency = s.Currency.CurrencyName,
                                                        Valuer = s.Valuer,
                                                        ValuationType = s.ValuationType.Type,
                                                        FileName = s.Security.UnitisedId == 1 ? s.NAVSecurityAttachments.FirstOrDefault().Attachment.FileName : string.Empty

                                                    }).OrderBy(i => i.SecurityId).ToList();

            return result;
        }
        protected List<SecurityPriceModel> GetAllLastMonthPrices(SecurityPriceModel template)
        {
            List<int> securityIds = template.TableParam.sIdsSearch;
            var rezult = new List<SecurityPriceModel>();
            var defaultDate = new DateTime();
            var allprices = _securitypriceRepository.GetAll(w => w.IsDeleted == false && w.Security.IsDeleted == false && securityIds.Contains(w.SecurityId) &&
                                                            w.Date >= (template.FromDate ?? defaultDate) &&
                                                            w.Date <= (template.ToDate ?? DateTime.Now)).Select(s =>
                    new SecurityPriceModel
                    {
                        Id = s.Id,
                        SecurityId = s.SecurityId,
                        Date = s.Date,
                        UnitPrice = s.UnitPrice,
                    }).OrderBy(i => i.SecurityId).ThenBy(i => i.Date).ToList();

            List<SecurityPriceModel> allprices2 = _securitypriceRepository.Where(w => w.IsDeleted == false && w.Security.IsDeleted == false && securityIds.Contains(w.SecurityId) &&
                                                            w.Date >= (template.FromDate ?? defaultDate) &&
                                                            w.Date <= (template.ToDate ?? DateTime.Now)).Select(s =>
                    new SecurityPriceModel
                    {
                        Id = s.Id,
                        SecurityId = s.SecurityId,
                        Date = s.Date,
                        UnitPrice = s.UnitPrice,
                    }).OrderBy(i => i.SecurityId).ThenBy(i => i.Date).ToList();

            var allprices22 = _securitypriceRepository.Where(w => w.IsDeleted == false && w.Security.IsDeleted == false && securityIds.Contains(w.SecurityId) &&
                                                            w.Date >= (template.FromDate ?? defaultDate) &&
                                                            w.Date <= (template.ToDate ?? DateTime.Now)).Select(s =>
                    new SecurityPriceModel
                    {
                        Id = s.Id,
                        SecurityId = s.SecurityId,
                        Date = s.Date,
                        UnitPrice = s.UnitPrice,
                    }).OrderBy(i => i.SecurityId).ThenBy(i => i.Date).ToList();



            foreach (var id in securityIds)
            {

                var sec = allprices22.Where(w => w.SecurityId == id).ToList();

                var allPricesById = allprices.Where(i => i.SecurityId == id).ToList();
                if (!allPricesById.Any())
                {
                    continue;
                }

                //DateTime 

                DateTime curtime = allPricesById.Where(i => i.Date != null).Select(i => i.Date.Value).FirstOrDefault();
                while (curtime != defaultDate && curtime < new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, DateTime.DaysInMonth(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month)))
                {
                    var lastprice = allPricesById.Where(i => i.Date != null && i.Date.Value.Month == curtime.Month &&
                                                        i.Date.Value.Year == curtime.Year).OrderBy(i => i.Date).LastOrDefault();
                    if (lastprice != null)
                        rezult.Add(lastprice);
                    curtime = curtime.AddMonths(1);
                }
            }
            return rezult;
        }

        protected List<SecurityPriceModel> GetModelListByModelFields(SecurityPriceModel modelWithValue)
        {
            return _securitypriceRepository.GetAll(w => w.IsDeleted == false && w.Security.IsDeleted == false).Where(Createfilter<SecurityPrice>(modelWithValue))
                .Select(s =>
                    new SecurityPriceModel
                    {
                        Id = s.Id,
                        SecurityId = s.SecurityId,
                        SecurityCode = s.Security.Code,
                        SecurityName = s.Security.Name,
                        Date = s.Date,
                        CurrencyId = s.CurrencyId,
                        Currency = s.Currency.Code ?? s.Security.Currency.Code,
                        UnitPrice = s.UnitPrice,
                        PriceNAV = s.PriceNAV,
                        PricePUR = s.PricePUR,
                        InterestRate = s.InterestRate,
                        ValuationTypeId = s.ValuationTypeId,
                        Valuer = s.Valuer,
                        Attachment = s.Attachment,
                        IsDeleted = s.IsDeleted,
                    }).ToList();
        }

        protected List<SecurityPriceModel> GetModelListByModelFieldsDateRange(SecurityPriceModel modelWithValue)
        {
            IQueryable<SecurityPrice> spList;

            if (modelWithValue.SecurityId == 0 && (modelWithValue.FromDate == null || modelWithValue.ToDate == null))
            {
                spList = _securitypriceRepository.GetAll(w => w.IsDeleted == false && w.Security.IsDeleted == false);
            }
            else if (modelWithValue.SecurityId == 0)
            {
                spList = _securitypriceRepository.GetAll(w => w.IsDeleted == false && w.Security.IsDeleted == false).Where(s => s.Date >= modelWithValue.FromDate && s.Date <= modelWithValue.ToDate);
            }
            else if (modelWithValue.FromDate == null || modelWithValue.ToDate == null)
            {
                spList = _securitypriceRepository.GetAll(w => w.IsDeleted == false && w.Security.IsDeleted == false).Where(Createfilter<SecurityPrice>(modelWithValue));
            }
            else
            {
                spList = _securitypriceRepository.GetAll(w => w.IsDeleted == false && w.Security.IsDeleted == false).Where(Createfilter<SecurityPrice>(modelWithValue)).Where(s => s.Date >= modelWithValue.FromDate && s.Date <= modelWithValue.ToDate);
            }
            if (modelWithValue.IsRateIndex)
            {
                spList = spList.Where(s => s.Security.Unitised.Unitised1=="Rate" );
            }
            else
            {
                spList = spList.Where(s => s.Security.Unitised.Unitised1 != "Rate" );
            }
            return spList.Select(s =>
                            new SecurityPriceModel
                            {
                                Id = s.Id,
                                SecurityId = s.SecurityId,
                                SecurityCode = s.Security.Code,
                                SecurityName = s.Security.Name,
                                Date = s.Date,
                                CurrencyId = s.CurrencyId,
                                Currency = s.Currency.Code ?? s.Security.Currency.Code,
                                UnitPrice = s.UnitPrice,
                                PriceNAV = s.PriceNAV,
                                PricePUR = s.PricePUR,
                                InterestRate = s.InterestRate,
                                ValuationTypeId = s.ValuationTypeId,
                                Valuer = s.Valuer,
                                Attachment = s.Attachment,
                                IsDeleted = s.IsDeleted,
                            }).ToList();

        }

        protected SecurityPriceModel GetModelByModelFields(SecurityPriceModel modelWithValue)
        {
            return _securitypriceRepository.GetAll(w => w.IsDeleted == false && w.Security.IsDeleted == false).Where(Createfilter<SecurityPrice>(modelWithValue))
                .Select(s =>
                    new SecurityPriceModel
                    {
                        Id = s.Id,
                        SecurityId = s.SecurityId,
                        SecurityCode = s.Security.Code,
                        SecurityName = s.Security.Name,
                        Date = s.Date,
                        CurrencyId = s.CurrencyId,
                        Currency = s.Currency.Code ?? s.Security.Currency.Code,
                        UnitPrice = s.UnitPrice,
                        PriceNAV = s.PriceNAV,
                        PricePUR = s.PricePUR,
                        InterestRate = s.InterestRate,
                        ValuationTypeId = s.ValuationTypeId,
                        Valuer = s.Valuer,
                        Attachment = s.Attachment,
                        IsDeleted = s.IsDeleted,
                        FName = s.NAVSecurityAttachments.FirstOrDefault().Attachment.FileName,
                        Extension = s.NAVSecurityAttachments.FirstOrDefault().Attachment.Extension.HasValue ? s.NAVSecurityAttachments.FirstOrDefault().Attachment.Extension.Value : 0,
                    }).FirstOrDefault();
        }

        public IViewModel AddSecurityPrice(IViewModel baseModel)
        {
            var model = (SecurityPriceModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success,

            };
            try
            {
                //Check and Update Existing Security Price by Date
                var securityPriceByDate = _securitypriceRepository.FirstOrDefault(w => w.SecurityId == model.SecurityId && w.Date == model.Date);
                if (securityPriceByDate != null)
                {
                    model.Id = securityPriceByDate.Id;
                    UpdateSecurityPrice(model);
                }
                else // New Record
                {
                    var securityPrice = new SecurityPrice
                    {
                        SecurityId = model.SecurityId,
                        Date = model.Date,
                        CurrencyId = model.CurrencyId,
                        UnitPrice = model.UnitPrice,
                        PriceNAV = model.PriceNAV,
                        PricePUR = model.PricePUR,
                        InterestRate = model.InterestRate,
                        ValuationTypeId = model.ValuationTypeId,
                        Valuer = model.Valuer,
                        Attachment = model.Attachment,
                    };
                    _securitypriceRepository.Add(securityPrice);
                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "SecurityPrice", ObjectID = securityPrice.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });


                    //NAV Security Attachment
                    if (model.FName != null)
                    {
                        var attachment = new Attachment
                        {
                            Name = model.FName,
                            FileName = model.FileName,
                            Permission = 2,
                            Extension = model.Extension,
                            AttachmentTypeId = _attachamentTypeRepository.Where(a => a.StringKey == "NAVDoc").FirstOrDefault().Id,//model.AttachmentTypeId,
                            IsVerified = false,
                            IsPhotoShown = false,
                            Created = DateTime.UtcNow,
                            CreatedBy = SessionObject.GetInstance.UserID,
                            IsDeleted = false
                        };

                        _attachamentRepository.Add(attachment);
                        uow.Commit();
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Attachment", ObjectID = attachment.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                        var navSecurityAttachment = new NAVSecurityAttachment
                        {
                            SecurityPriceId = securityPrice.Id,
                            AttachmentId = attachment.Id,
                            IsDeleted = false
                        };

                        _navsecurityattachamentRepository.Add(navSecurityAttachment);
                        uow.Commit();
                        CreateEntityLog(new EntitiesEventLogModel { ObjectType = "NAVSecurityAttachments", ObjectID = navSecurityAttachment.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                    }
                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordAddedSuccessfully, "Security Price");
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
        public IViewModel UpdateSecurityPrice(IViewModel baseModel)
        {
            var model = (SecurityPriceModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success
            };
            try
            {
                var securityPrice = _securitypriceRepository.GetById(model.Id);
                if (securityPrice != null)
                {
                    if (model.SecurityId != 0)
                    {
                        securityPrice.SecurityId = model.SecurityId;
                    }
                    securityPrice.Date = model.Date;
                    securityPrice.CurrencyId = model.CurrencyId;
                    securityPrice.UnitPrice = model.UnitPrice;
                    securityPrice.PriceNAV = model.PriceNAV;
                    securityPrice.PricePUR = model.PricePUR;
                    securityPrice.InterestRate = model.InterestRate;
                    securityPrice.ValuationTypeId = model.ValuationTypeId;
                    securityPrice.Valuer = model.Valuer;
                    securityPrice.Attachment = model.Attachment;
                    var NavSecurityAttachment = securityPrice.NAVSecurityAttachments.FirstOrDefault();
                    if (NavSecurityAttachment != null)
                    {
                        if (model.FName != null)
                        {
                            //file deletion code
                            //var path = Path.Combine(Invest.WorkContext.FilesPath, model.FileName);
                            //var filesPath = Server.MapPath(path);

                            //if (System.IO.File.Exists(filesPath))
                            //    System.IO.File.Delete(filesPath);

                            NavSecurityAttachment.Attachment.Name = model.FName;
                            NavSecurityAttachment.Attachment.FileName = model.FileName;
                            NavSecurityAttachment.Attachment.Permission = 2;
                            NavSecurityAttachment.Attachment.Extension = model.Extension;
                        }
                    }
                    else
                    {
                        if (model.FName != null)
                        {
                            if (model.FName != null)
                            {
                                var attachment = new Attachment
                                {
                                    Name = model.FName,
                                    FileName = model.FileName,
                                    Permission = 2,
                                    Extension = model.Extension,
                                    AttachmentTypeId = _attachamentTypeRepository.Where(a => a.StringKey == "NAVDoc").FirstOrDefault().Id,//model.AttachmentTypeId,
                                    IsVerified = false,
                                    IsPhotoShown = false,
                                    Created = DateTime.UtcNow,
                                    CreatedBy = SessionObject.GetInstance.UserID,
                                    IsDeleted = false
                                };

                                _attachamentRepository.Add(attachment);
                                uow.Commit();
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "Attachment", ObjectID = attachment.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                                var navSecurityAttachment = new NAVSecurityAttachment()
                                {
                                    SecurityPriceId = securityPrice.Id,
                                    AttachmentId = attachment.Id,
                                    IsDeleted = false
                                };

                                _navsecurityattachamentRepository.Add(navSecurityAttachment);
                                uow.Commit();
                                CreateEntityLog(new EntitiesEventLogModel { ObjectType = "NAVSecurityAttachments", ObjectID = navSecurityAttachment.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "CREATE" });

                            }
                        }
                    }
                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "SecurityPrice", ObjectID = securityPrice.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "UPDATE" });
                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Security Price");
                }
                else
                {
                    responseModel.Status = ResponseStatus.Failure;
                    responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "Security Price");
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

        private IViewModel DeleteSecurityPrice(IViewModel baseModel)
        {
            var model = (SecurityPriceModel)baseModel;
            var responseModel = new ResponseModel<GenericModel<string>>
            {
                Status = ResponseStatus.Success
            };
            try
            {
                var securityPrice = _securitypriceRepository.GetById(model.Id);
                if (securityPrice != null)
                {
                    securityPrice.IsDeleted = true;

                    uow.Commit();
                    // Maintaining Log
                    CreateEntityLog(new EntitiesEventLogModel { ObjectType = "SecurityPrice", ObjectID = securityPrice.Id, DateTime = DateTime.Now, UserID = SessionObject.GetInstance.UserID, OperationType = "DELETE" });
                    uow.Dispose();
                    responseModel.Message = string.Format(MDA_Resource.Info_RecordUpdatedSuccessfully, "Security Price");
                }
                else
                {
                    responseModel.Status = ResponseStatus.Failure;
                    responseModel.Message = string.Format("{0} not found or Invalid {0} id.", "Security Price");
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
    }
}
