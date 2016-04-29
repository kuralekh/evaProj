using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Text;
using Invest.Common;
using Invest.Common.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using Invest.Common.Extensions;
using Invest.Web.Framework;
using Invest.Web.Models;
using Invest.Web.Models.Model;
using Invest.Web.Helpers;
using Invest.Web.Services;
using Invest.Web.Utility;
using Invest.Analytics;
using Invest.Convert;
using Invest.ViewModel.Models;
using Invest.ViewModel.ModelsConvertor;
using Invest.Web.Models.Common;
using Invest.Database;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.IO;

using System.Web.Helpers;
using Invest.Service.Components;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing;
using Hangfire;

//using Invest.Web.Services.BaseApiController;

namespace Invest.Web.Controllers
{
    [UserAuthorize]
    public class InvestmentController : BaseController
    {
        static double MtdPrice = 0d;
        static double MtdGlobal = 0d;
        static DateTime MtdDateGlobal = new DateTime();
        static Dictionary<string, byte[]> pdfByGuid = new Dictionary<string, byte[]>();
        #region Screen
        public ActionResult GetScreenPageData(JQueryDataTableParamModel param)
        {
            var responseViewModle = ScreenPageData(param);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        private ResponseModel<ScreenModel> ScreenPageData(JQueryDataTableParamModel param, bool isDownlaod = false)
        {
            var modelView = new ScreenModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ScreenModel>(json);
        }
        public void DownloadScreenCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = ScreenPageData(Params, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetScreenCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=Screen.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetScreenCsvString(List<ScreenModel> screenModel)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Code, Name");

            foreach (var product in screenModel)
            {
                var code = product.Code;
                var name = product.Name;
                csv.Append(CsvUtility.MakeValueCsvFriendly(code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.AppendLine();
            }
            return csv.ToString();
        }
        public ActionResult CreateScreen()
        {
            return PartialView("_CreateScreen");
        }
        public ActionResult GetScreen(int Id)
        {
            var model = new ProductController().GetScreen(Id);
            return PartialView("_EditScreen", model);
        }
        [HttpPost]
        public ActionResult DeleteScreen(ScreenModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }
        public ActionResult Screen()
        {
            return View("Screen");
        }
        public ActionResult ScreenDetail()
        {
            return View("ScreenDetail");
        }
        [HttpPost]
        public ActionResult AddScreen(ScreenModel model)
        {
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }
        [HttpPost]
        public ActionResult EditScreen(ScreenModel model)
        {
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }
        public ActionResult GetScreenDetailListPage(int? screenId, bool IsPopUp = false)
        {
            var screenList = ListsMaintenance.GetScreenList();
            if (screenId != null && screenId > 0)
            {
                var item = screenList.Find(f => f.Value == screenId.ToString());
                if (item != null)
                    item.Selected = true;
                if (item != null) ViewData["screenName"] = item.Text;
            }

            if (!IsPopUp)
            {
                ViewData["screenList"] = screenList;
                return PartialView("_ScreenDetailList");
            }
            else
            {
                ViewData["screenId"] = screenId;
                return PartialView("_ScreenDetailPopup");
            }
        }
        public ActionResult GetScreenDetailListPageData(JQueryDataTableParamModel param, int? screenId)
        {
            if (screenId == 0)
            {
                return null;
            }
            var responseViewModle = ScreenDetailListPageData(param, screenId);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        private ResponseModel<ScreenDetailModel> ScreenDetailListPageData(JQueryDataTableParamModel param, int? screenId, bool isDownlaod = false)
        {
            var modelView = new ScreenDetailModel
            {
                ScreenId = screenId ?? 0,
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ScreenDetailModel>(json);
        }
        public void DownloadScreenDetailCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = ScreenDetailListPageData(Params, Params.Id, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetScreenDetailCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ScreenDetail.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetScreenDetailCsvString(List<ScreenDetailModel> screenDetailModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Security Code, Security Name, Security Type");
            foreach (var models in screenDetailModel)
            {
                var securityCode = models.SecurityCode;
                var securityName = models.SecurityName;
                var securityType = models.SecurityType;
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityCode));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityName));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityType));
                csv.AppendLine();
            }
            return csv.ToString();
        }
        public ActionResult CreateScreenDetailList()
        {
            var screenList = ListsMaintenance.GetScreenList();
            ViewData["screenList"] = screenList;
            ViewData["securityList"] = new List<SelectListItem>();
            return PartialView("_CreateScreenDetail");
        }
        public ActionResult GetScreenDetailList(int id)
        {
            var screenList = ListsMaintenance.GetScreenList();
            ViewData["screenList"] = screenList;
            var model = new ProductController().GetScreenListDetail(id);
            var securityList = ListsMaintenance.GetFilteredSecurityList(model.SecurityCode, model.SecurityId ?? 0);
            ViewData["securityList"] = securityList;
            return PartialView("_EditScreenDetail", model);
        }
        [HttpPost]
        public ActionResult DeleteScreenDetail(ScreenDetailModel model)
        {
            var result = new ModelsController().Delete(model);
            return Json(result.Content);
        }
        [HttpPost]
        public ActionResult AddScreenDetail(ScreenDetailModel model)
        {
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }
        [HttpPost]
        public ActionResult EditScreenDetail(ScreenDetailModel model)
        {
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }

        #endregion

        #region Asset
        public ActionResult Asset()
        {
            return View("Asset");
        }
        #endregion

        #region Securities

        #region Securities-Dashboard

        public ActionResult SecuritiesDashboard()
        {
            List<Tuple<string, string, string, string>> topHoldins = GetTopHoldings2();
            var json = new JavaScriptSerializer().Serialize(topHoldins);
            ViewBag.TopHoldins = topHoldins;
            ViewBag.TopHoldinsJson = json;
            return View("SecuritiesDashboard");
        }

        #endregion

        #region Securities-Details

        public ActionResult GetSecurity(int id, string requestType)
        {
            var securityCategoryList = ListsMaintenance.GetSecurityCategoryList();
            ViewData["securitycategorylist"] = securityCategoryList;

            var securityTypeList = ListsMaintenance.GetSecurityTypeList();
            var unitisedList = ListsMaintenance.GetUnitisedList();
            if (requestType == "OptionSecurities")
            {
                securityTypeList = securityTypeList.Where(s => s.Value == "31").ToList();
                unitisedList = unitisedList.Where(s => s.Value != "3").ToList();
            }
            else if (requestType == "NonOptionSecurities")
            {
                securityTypeList = securityTypeList.Where(s => s.Value != "31").ToList();
            }
            ViewData["securitytypelist"] = securityTypeList;
            ViewData["unitisedList"] = unitisedList;

            var marketList = ListsMaintenance.GetMarketList();
            ViewData["marketlist"] = marketList;

            var currencyList = ListsMaintenance.GetCurrencyList();
            ViewData["currencylist"] = currencyList;

            var assetClassList = ListsMaintenance.GetAssetClassList();
            ViewData["assetClassList"] = assetClassList;

            var marketOptionsDetailList = ListsMaintenance.GetMarketList();
            ViewData["marketOptionsDetailList"] = marketOptionsDetailList;

            var subAssetClassList = ListsMaintenance.GetSubAssetClassList();
            ViewData["subAssetClassList"] = subAssetClassList;

            var regionList = ListsMaintenance.GetRegionList();
            ViewData["regionList"] = regionList;

            var gicsList = ListsMaintenance.GetGICSList();
            ViewData["gicsList"] = gicsList;

            var gicsTypeList = ListsMaintenance.GetGICSTypeList();
            ViewData["gicsTypeList"] = gicsTypeList;

            var ratingList = ListsMaintenance.GetRatingList();
            ViewData["ratingList"] = ratingList;

            var securityStatusList = ListsMaintenance.GetSecurityStatusList();
            ViewData["securityStatusList"] = securityStatusList;

            var pricingSourceList = ListsMaintenance.GetPricingSourceList();
            ViewData["pricingSourceList"] = pricingSourceList;

            var distributionTypeList = ListsMaintenance.GetDistributionTypeList();
            ViewData["distributionTypeList"] = distributionTypeList;

            var distributionFrequencyList = ListsMaintenance.GetDistributionFrequencyList();
            ViewData["distributionFrequencyList"] = distributionFrequencyList;

            var model = new SecuritiesController().GetSecurity(id);

            ViewData["assetClassIds"] = model.AssetClassIds;

            var securityHoldingList = new SecuritiesController().GetSecurityHoldingType();
            ViewData["securityHoldingList"] = securityHoldingList.ToList();

            var primaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["primaryBenchmarkList"] = primaryBenchmarkList;

            var secondaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["secondaryBenchmarkList"] = secondaryBenchmarkList;

            var priceTypeList = ListsMaintenance.GetPriceTypeList();
            ViewData["priceTypeList"] = priceTypeList;

            var optionsTypeList = ListsMaintenance.GetOptionsType();
            ViewData["optionsTypeList"] = optionsTypeList;

            var optionStyleList = ListsMaintenance.GetOptionsStyle();
            ViewData["optionStyleList"] = optionStyleList;

            var optionProductTypeList = ListsMaintenance.GetOptionsProductType();
            ViewData["optionProductTypeList"] = optionProductTypeList;

            var underlyingTypeList = ListsMaintenance.GetUnderlyingType();
            ViewData["underlyingTypeList"] = underlyingTypeList;
            UpdateLocation(model);
            if (model.OptionsDetail != null)
            {
                List<SelectListItem> underlyingList = new List<SelectListItem>();
                underlyingList.Add(new SelectListItem { Text = model.OptionsDetail.UnderlyingValue, Value = model.OptionsDetail.Underlying.ToString() });
                ViewData["underlyingList"] = underlyingList;
            }
            else
            {
                ViewData["underlyingList"] = new List<SelectListItem>();
            }

            var institutionIdList = ListsMaintenance.GetInstitutionList();
            ViewData["institutionIdList"] = institutionIdList;
            var termList = ListsMaintenance.GetTermList("0");
            ViewData["termList"] = termList;
            var productBrokerList = ListsMaintenance.GetProductBrokerList();
            ViewData["brokerlist"] = productBrokerList;
            var clientAccountTypeList = ListsMaintenance.GetClientAccountTypeList();
            ViewData["clientTypelist"] = clientAccountTypeList;
            return PartialView("_EditSecurity", model);
        }

        public ActionResult SecuritiesDetails()
        {
            List<SelectListItem> DisplayStatusList = new List<SelectListItem>();
            DisplayStatusList.Add(new SelectListItem { Text = "Non Matured/Terminated", Value = "1", Selected = true });
            DisplayStatusList.Add(new SelectListItem { Text = "All", Value = "0" });
            ViewData["StatusList"] = DisplayStatusList;
            return View("SecuritiesDetails");
        }

        public ActionResult GetSecurityPageData(JQueryDataTableParamModel param, string requestType, bool DisplayStatus)
        {
            var responseViewModle = SecurityPageData(param, requestType, DisplayStatus);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<SecurityModel> SecurityPageData(JQueryDataTableParamModel param, string requestType, bool DisplayStatus = false, bool isDownlaod = false)
        {
            var modelView = new SecurityModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                RequestType = requestType,
                DisplayStatus = DisplayStatus
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<SecurityModel>(json);
        }

        public void DownloadSecurityCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = SecurityPageData(Params, Params.RequestType, Params.IsAll, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetSecurityCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=Security.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetSecurityCsvString(List<SecurityModel> securitesModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Code, Name, Security Category, Market, Security Type, Unitised, Currency, Asset Class, Sub Asset Class, Region, GICS - Sector, GICS - Industry Group, Rating, Security Status, APIR Code, ISIN Code, Units Held, Pricing Source, Distribution Type, Distribution Frequency, Expense Ratio, Liquidity(days), Primary Benchmark, Secondary Benchmark");
            foreach (var security in securitesModel)
            {
                var code = security.Code;
                var name = security.Name;
                var securityCategory = security.SecurityCategory;
                var market = security.Market;
                var secType = security.SecurityType;
                var unitised = security.Unitised;
                var currency = security.Currency;
                var assetClass = string.Join("; ", security.AssetClass.ToArray());
                var subAssetClass = security.SubAssetClass;
                var region = security.Region;
                var gics = security.GICS;
                var gicsType = security.GICSType;
                var rating = security.Rating;
                var staus = security.SecurityStatus;
                var apir = security.APIRCode;
                var isin = security.ISINCode;
                var unitsHeld = security.UnitsHeld.ToString();
                var pricingSource = security.PricingSource;
                var distType = security.DistributionType;
                var distFreq = security.DistributionFrequency;
                var expenseRation = security.ExpenseRatio.ToString();
                var liquidity = security.Liquidity.ToString();
                var primaryBenchmark = security.PrimaryBenchmark;
                var secondaryBenchmark = security.SecondaryBenchmark;
                csv.Append(CsvUtility.MakeValueCsvFriendly(code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityCategory));
                csv.Append(CsvUtility.MakeValueCsvFriendly(market));
                csv.Append(CsvUtility.MakeValueCsvFriendly(secType));
                csv.Append(CsvUtility.MakeValueCsvFriendly(unitised));
                csv.Append(CsvUtility.MakeValueCsvFriendly(currency));
                csv.Append(CsvUtility.MakeValueCsvFriendly(assetClass));
                csv.Append(CsvUtility.MakeValueCsvFriendly(subAssetClass));
                csv.Append(CsvUtility.MakeValueCsvFriendly(region));
                csv.Append(CsvUtility.MakeValueCsvFriendly(gics));
                csv.Append(CsvUtility.MakeValueCsvFriendly(gicsType));
                csv.Append(CsvUtility.MakeValueCsvFriendly(rating));
                csv.Append(CsvUtility.MakeValueCsvFriendly(staus));
                csv.Append(CsvUtility.MakeValueCsvFriendly(apir));
                csv.Append(CsvUtility.MakeValueCsvFriendly(isin));
                csv.Append(CsvUtility.MakeValueCsvFriendly(unitsHeld));
                csv.Append(CsvUtility.MakeValueCsvFriendly(pricingSource));
                csv.Append(CsvUtility.MakeValueCsvFriendly(distType));
                csv.Append(CsvUtility.MakeValueCsvFriendly(distFreq));
                csv.Append(CsvUtility.MakeValueCsvFriendly(expenseRation));
                csv.Append(CsvUtility.MakeValueCsvFriendly(liquidity));
                csv.Append(CsvUtility.MakeValueCsvFriendly(primaryBenchmark));
                csv.Append(CsvUtility.MakeValueCsvFriendly(secondaryBenchmark));
                csv.AppendLine();
            }
            return csv.ToString();
        }

        [HttpPost]
        public ActionResult EditSecurity(SecurityModel model)
        {
            var result = new SecuritiesController().Update(model);
            BackgroundJob.Enqueue<PriceUtility>(x => x.GeneratePricesForModel(model.Id));
            return Json(result.Content);
        }

        public ActionResult CreateSecurity(string requestType)
        {
            SecurityModel model = new SecurityModel();
            var securityTypeList = ListsMaintenance.GetSecurityTypeList();
            var unitisedList = ListsMaintenance.GetUnitisedList();
            if (requestType == "OptionSecurities")
            {
                securityTypeList = securityTypeList.Where(s => s.Value == "31").ToList();
                unitisedList = unitisedList.Where(s => s.Value != "3").ToList();
            }
            else if (requestType == "NonOptionSecurities")
            {
                securityTypeList = securityTypeList.Where(s => s.Value != "31").ToList();
            }
            ViewData["securitytypelist"] = securityTypeList;
            ViewData["unitisedList"] = unitisedList;

            var securityCategoryList = ListsMaintenance.GetSecurityCategoryList();
            ViewData["securitycategorylist"] = securityCategoryList;

            var marketList = ListsMaintenance.GetMarketList();
            ViewData["marketlist"] = marketList;

            var currencyList = ListsMaintenance.GetCurrencyList();
            ViewData["currencylist"] = currencyList;

            var assetClassList = ListsMaintenance.GetAssetClassList();
            ViewData["assetClassList"] = assetClassList;

            var subAssetClassList = ListsMaintenance.GetSubAssetClassList();
            ViewData["subAssetClassList"] = subAssetClassList;


            var regionList = ListsMaintenance.GetRegionList();
            ViewData["regionList"] = regionList;


            var gicsList = ListsMaintenance.GetGICSList();
            ViewData["gicsList"] = gicsList;

            var gicsTypeList = ListsMaintenance.GetGICSTypeList();
            ViewData["gicsTypeList"] = gicsTypeList;

            var ratingList = ListsMaintenance.GetRatingList();
            ViewData["ratingList"] = ratingList;

            var securityStatusList = ListsMaintenance.GetSecurityStatusList();
            ViewData["securityStatusList"] = securityStatusList;

            var pricingSourceList = ListsMaintenance.GetPricingSourceList();
            ViewData["pricingSourceList"] = pricingSourceList;

            var distributionTypeList = ListsMaintenance.GetDistributionTypeList();
            ViewData["distributionTypeList"] = distributionTypeList;

            var distributionFrequencyList = ListsMaintenance.GetDistributionFrequencyList();
            ViewData["distributionFrequencyList"] = distributionFrequencyList;

            var securityHoldingList = new SecuritiesController().GetSecurityHoldingType();
            ViewData["securityHoldingList"] = securityHoldingList.ToList();

            var primaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["primaryBenchmarkList"] = primaryBenchmarkList;

            var secondaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["secondaryBenchmarkList"] = secondaryBenchmarkList;

            var priceTypeList = ListsMaintenance.GetPriceTypeList();
            ViewData["priceTypeList"] = priceTypeList;

            var optionsTypeList = ListsMaintenance.GetOptionsType();
            ViewData["optionsTypeList"] = optionsTypeList;

            var optionStyleList = ListsMaintenance.GetOptionsStyle();
            ViewData["optionStyleList"] = optionStyleList;

            var optionProductTypeList = ListsMaintenance.GetOptionsProductType();
            ViewData["optionProductTypeList"] = optionProductTypeList;

            var underlyingTypeList = ListsMaintenance.GetUnderlyingType();
            ViewData["underlyingTypeList"] = underlyingTypeList;

            var institutionIdList = ListsMaintenance.GetInstitutionList();
            ViewData["institutionIdList"] = institutionIdList;

            var termList = ListsMaintenance.GetTermList("0");

            ViewData["termList"] = termList;

            var productBrokerList = ListsMaintenance.GetProductBrokerList();
            ViewData["brokerlist"] = productBrokerList;

            var clientAccountTypeList = ListsMaintenance.GetClientAccountTypeList();
            ViewData["clientTypelist"] = clientAccountTypeList;

            UpdateLocation(model);
            return PartialView("_CreateSecurity", model);
        }
        private void UpdateLocation(SecurityModel model)
        {
            model.CountryAll = Location.Contries(model.Country);
            model.StateAll = Location.States(model.Country, model.State);
        }
        [HttpPost]
        public ActionResult Create(SecurityModel model)
        {
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteSecurity(SecurityModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }

        public ActionResult GetFilteredSecurities(string code, int id = 0, string type = "All")
        {
            var responseViewModle = new SecuritiesController().GetFilteredSecurities(code, id, type);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    data = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetFilteredUnderlyingList(string searchString, string searchunderlyingType)
        {
            var responseViewModle = new SecuritiesController().GetFilteredUnderlyingList(searchString, searchunderlyingType);
            var model = responseViewModle.Model;
            return new CustomJsonResult()
            {
                Data = new
                {
                    data = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #endregion

        #region Securities-Home

        public double CorrelationCount(List<double> firstList, List<double> secondList)
        {
            double mX = firstList.Sum() / firstList.Count;
            double mY = secondList.Sum() / secondList.Count;
            double mXY = 0d;
            double sXX = 0d;
            double sYY = 0d;
            double sXsY = 0d;
            double r = 0d;
            for (int i = 0; i < firstList.Count; i++)
            {
                sXX += Math.Pow((firstList[i]), 2);
                sYY += Math.Pow((secondList[i]), 2);

                mXY += firstList[i] * secondList[i];
            }

            mXY /= firstList.Count;

            sXX = sXX / firstList.Count - Math.Pow(mX, 2);
            sYY = sYY / secondList.Count - Math.Pow(mY, 2);
            sXsY = Math.Sqrt(sXX * sYY);

            r = (mXY - mX * mY) / sXsY;

            return r;
        }
        public double CovariationCount(List<double> firstList, List<double> secondList)
        {
            double sumX = firstList.Sum() / firstList.Count;
            double sumY = secondList.Sum() / firstList.Count;
            double cor = 0d;

            for (int i = 0; i < firstList.Count; i++)
            {
                cor += (firstList[i] - sumX) * (secondList[i] - sumY);
            }
            return cor / firstList.Count;
        }

        public double VariationCount(List<double> firstList)
        {
            double avr = firstList.Sum() / firstList.Count;
            double variation = 0d;

            for (int i = 0; i < firstList.Count; i++)
            {
                variation += Math.Pow((firstList[i] - avr), 2);
            }
            variation = variation / firstList.Count;

            return variation;
        }

        public List<double> DownsideCountRisk(List<double> firstList)
        {
            List<double> result = new List<double>();
            int iteration = 0;

            if (firstList.Count > 36)
            {
                while (firstList.Count >= 36 + iteration)
                {
                    result.Add(DownsideAnnualCount(firstList.Skip(firstList.Count - 36 - iteration).Take(36).ToList()));
                    iteration++;
                }
            }
            else
            {
                result.Add(DownsideAnnualCount(firstList.Skip(firstList.Count - 36 - iteration).Take(36).ToList()));
            }
            return result;
        }

        public double SortinoRatioCount(List<double> firstList)
        {
            const double annulReturn = 0.00d;
            double annulCoef = (double)1 / 12;
            double monthReturn = Math.Pow((1 + annulReturn), annulCoef) - 1;
            double avrArithmeticReturn = firstList.Average();
            double avrExcessReturn = avrArithmeticReturn - monthReturn;
            double downsideMonthlyDeviation = DownsideMonthlyCount(firstList);
            double sortinoRatioMonth = avrExcessReturn / downsideMonthlyDeviation;
            double sortinoRatioAnnual = sortinoRatioMonth * Math.Sqrt(12);
            return sortinoRatioAnnual;
        }

        public double SharpeRatioCount(List<double> firstList, List<double> SPBDABBTList)
        {
            double avr = firstList.Sum() / firstList.Count;
            double sharpeRatioCoef = 0d;
            List<double> aboveRiskFreeRate = new List<double>();
            for (int i = 0; i < firstList.Count; i++)
            {
                aboveRiskFreeRate.Add(firstList[i] - SPBDABBTList[i]);
            }
            avr = aboveRiskFreeRate.Sum() / aboveRiskFreeRate.Count;
            for (int i = 0; i < aboveRiskFreeRate.Count; i++)
            {
                sharpeRatioCoef += Math.Pow(aboveRiskFreeRate[i] - avr, 2);
            }
            sharpeRatioCoef = sharpeRatioCoef / (aboveRiskFreeRate.Count - 1);
            sharpeRatioCoef = Math.Sqrt(sharpeRatioCoef);
            sharpeRatioCoef = avr / sharpeRatioCoef;
            sharpeRatioCoef = sharpeRatioCoef * Math.Sqrt(12);

            return sharpeRatioCoef;
        }

        public double DownsideAnnualCount(List<double> firstList)
        {
            double result = DownsideMonthlyCount(firstList) * Math.Sqrt(12);
            return result;
        }

        public double DownsideMonthlyCount(List<double> firstList)
        {
            const double annulReturn = 0.00d;
            double monthReturn = 0.00;

            double sumMult = 0d;
            double sumMultAvr = 0d;
            double result = 0d;

            for (int i = 0; i < firstList.Count; i++)
            {
                if (firstList[i] - monthReturn < 0)
                    sumMult += Math.Pow(firstList[i] - monthReturn, 2);
            }
            sumMultAvr = sumMult / firstList.Count;
            result = Math.Sqrt(sumMultAvr);
            return result;
        }

        public double VolatilityCount(List<double> firstList)
        {
            if (firstList == null)
                return 0;

            double volatil = 0d;
            double avr = firstList.Sum() / firstList.Count;
            volatil = 0d;
            for (int i = 0; i < firstList.Count; i++)
            {
                volatil += Math.Pow((firstList[i] - avr), 2);
            }
            volatil = volatil / (firstList.Count - 1);
            volatil = Math.Sqrt(volatil);
            volatil = volatil * Math.Sqrt(12);
            return Math.Round(volatil, 4);
        }

        public DateTime GetPreMonthLastDay()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.AddMonths(-1).Month;
            if (month == 12)
            {
                year = year - 1;
            }
            return new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }


        public class Returns
        {
            public DateTime Date { get; set; }
            public decimal Capital { get; set; }
            public decimal Total { get; set; }
            public decimal CR { get; set; }
            public decimal TR { get; set; }

            public Returns()
            {
                Date = new DateTime();
                Capital = 0.0M;
                Total = 0.0M;
                CR = 0.0M;
                TR = 0.0M;
            }
            public Returns(DateTime date, decimal capital, decimal total, decimal cr, decimal tr)
            {
                Date = date;
                Capital = capital;
                Total = total;
                CR = cr;
                TR = tr;
            }
        }

        public class ChartsLines
        {
            public DateTime date { get; set; }
            public decimal securityReturn { get; set; }
            public decimal securityIndex { get; set; }

            public decimal? primaryBenchmarkReturn { get; set; }
            public decimal primaryBenchmarkIndex { get; set; }

            public decimal? secondaryBenchmarkReturn { get; set; }
            public decimal secondaryBenchmarkIndex { get; set; }
        }

       
        public int UpdateReturnsProc()
        {
            using (SqlConnection connect = new SqlConnection(constr))
            using (SqlCommand command = new SqlCommand("SecurityReturnCalc", connect) { CommandType = CommandType.StoredProcedure })
            {
                connect.Open();
                command.CommandTimeout = 300;
                command.ExecuteNonQuery();
                connect.Close();
            }
            return 1;
        }
        #endregion

        #region Securities-Price
        public ActionResult GetPriceDetail(bool IsRateIndex = false)
        {
            ViewBag.RateIndex = IsRateIndex;
            return PartialView("_SecurityPriceList");
        }

        public ActionResult GetPriceDetailByFilter(JQueryDataTableParamModel param, int? securityId,
            DateTime? fromDate, DateTime? toDate, bool isRateIndex = false)
        {
            if (securityId == 0 && fromDate == null && toDate == null)
            {
                return null;
            }

            var responseViewModle = SecurityPriceData(param, securityId, fromDate, toDate, isRateIndex);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetPriceDate(JQueryDataTableParamModel param, int? securityId, DateTime? fromDate, DateTime? toDate)
        {
            return PartialView("_SecurityPriceList");
        }

        private ResponseModel<SecurityPriceModel> SecurityPriceData(JQueryDataTableParamModel param, int? securityId, DateTime? fromDate, DateTime? toDate, bool isRateIndex, bool isDownlaod = false)
        {
            var modelView = new SecurityPriceModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                SecurityId = securityId ?? 0,
                FromDate = fromDate,
                ToDate = toDate,
                IsRateIndex = isRateIndex
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<SecurityPriceModel>(json);
        }

        public void DownloadSecurityPriceCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = SecurityPriceData(Params, Params.Id, Params.FromDate, Params.ToDate, Params.IsAll, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetSecurityPriceCsvString(model, Params.IsAll);
            //Return the file content with response body. 
            Response.ContentType = "text/csv";
            if (Params.IsAll)
            {
                Response.AddHeader("Content-Disposition", "attachment;filename=RateIndexSecurityPrice.csv");
            }
            else
            {
                Response.AddHeader("Content-Disposition", "attachment;filename=SecurityPrice.csv");
            }
            Response.Write(facsCsv);
            Response.End();
        }

        private string GetSecurityPriceCsvString(List<SecurityPriceModel> securityPriceModel, bool RateIndex)
        {
            var csv = new StringBuilder();
            if (RateIndex)
            {
                csv.AppendLine("Code, Security Name, Date,Intrest Rate");
            }
            else
            {
                csv.AppendLine("Code, Security Name, Date, Unit Price, Currency");
            }
            foreach (var price in securityPriceModel)
            {
                var code = price.SecurityCode;
                var name = price.SecurityName;
                var date = price.Date;
                csv.Append(CsvUtility.MakeValueCsvFriendly(code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.Append(CsvUtility.MakeValueCsvFriendly(date));
                if (RateIndex)
                {
                    var intrestRate = price.InterestRate;
                    csv.Append(CsvUtility.MakeValueCsvFriendly(intrestRate));
                }
                else
                {
                    var unitPrice = price.UnitPrice;
                    var currency = price.Currency;
                    csv.Append(CsvUtility.MakeValueCsvFriendly(unitPrice));
                    csv.Append(CsvUtility.MakeValueCsvFriendly(currency));
                }
                csv.AppendLine();
            }
            return csv.ToString();
        }

        [HttpPost]
        public ActionResult EditSecurityPrice(SecurityPriceModel model, FormCollection formData)
        {
            if (model.File != null)
            {
                var documentobj = new DocumentsController();
                foreach (string file in Request.Files)
                {
                    var upload = Request.Files[file];
                    if (upload != null)
                    {
                        string filePath = model.SecurityId != 0 ?
                           Path.Combine(WorkContext.NavSecurityDocumentsPholder, model.SecurityId.ToString(CultureInfo.InvariantCulture) + "/") :
                           WorkContext.CommonDocumentsPholder;

                        var resultSave = SaveFile(model.File, filePath);
                        if (resultSave == null)
                            return null;

                        model.FName = upload.FileName;
                        model.FileName = string.Concat(filePath, resultSave);
                        model.Extension = (int)documentobj.GetExtension(resultSave);
                        model.AttachmentTypeId = 17;//For Nav Document
                        model.File = null;
                    }
                }
            }
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }

        public ActionResult CreatePrice(bool IsRateIndex = false)
        {
            var currencyList = ListsMaintenance.GetCurrencyList();
            ViewData["currencylist"] = currencyList;
            var valuationTypeList = ListsMaintenance.GetValuationTypeList();
            ViewData["valuationTypeList"] = valuationTypeList;
            ViewBag.IsRateIndex = IsRateIndex;
            return PartialView("_CreateSecurityPrice");
        }

        [HttpPost]
        public ActionResult CreateSecurityPrice(SecurityPriceModel model, FormCollection formData)
        {
            //var securityList = ListsMaintenance.GetSecurityList();
            //ViewData["securityList"] = securityList;
            if (model.File != null)
            {
                var documentobj = new DocumentsController();
                foreach (string file in Request.Files)
                {
                    var upload = Request.Files[file];
                    if (upload != null)
                    {
                        string filePath = model.SecurityId != 0 ?
                           Path.Combine(WorkContext.NavSecurityDocumentsPholder, model.SecurityId.ToString(CultureInfo.InvariantCulture) + "/") :
                           WorkContext.CommonDocumentsPholder;

                        var resultSave = SaveFile(model.File, filePath);
                        if (resultSave == null)
                            return null;

                        model.FName = upload.FileName;
                        model.FileName = string.Concat(filePath, resultSave);
                        model.Extension = (int)documentobj.GetExtension(resultSave);
                        model.AttachmentTypeId = 17;//For Nav Document
                        model.File = null;
                    }
                }
            }
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }

        public string SaveFile(HttpPostedFileBase file, string folder)
        {
            if (file == null)
                return null;
            var extension = Path.GetExtension(file.FileName);
            if (extension != null)
            {
                var fileName = Guid.NewGuid() + extension;
                var relativePath = WorkContext.FilesPath;
                if (!String.IsNullOrWhiteSpace(folder))
                    relativePath = VirtualPathUtility.Combine(relativePath, folder);
                var relativeFilePath = VirtualPathUtility.Combine(relativePath, fileName);
                var absolutePath = Server.MapPath(relativePath);
                if (!Directory.Exists(absolutePath))
                    Directory.CreateDirectory(absolutePath);
                var absoluteFilePath = Path.Combine(absolutePath, fileName);
                file.SaveAs(absoluteFilePath);
                return fileName;
            }
            return null;
        }
        public ActionResult GetSecurityPrice(int id, bool IsRateIndex = false)
        {
            var currencyList = ListsMaintenance.GetCurrencyList();
            ViewData["currencylist"] = currencyList;
            var valuationTypeList = ListsMaintenance.GetValuationTypeList();
            ViewData["valuationTypeList"] = valuationTypeList;
            var model = new SecuritiesController().GetSecurityPrice(id);
            var securityList = ListsMaintenance.GetFilteredSecurityList(model.SecurityCode, model.SecurityId);
            ViewData["securityList"] = securityList;
            model.IsRateIndex = IsRateIndex;
            return PartialView("_EditSecurityPrice", model);
        }

        public ActionResult SecuritiesPrice()
        {
            return View("SecuritiesPrice");
        }

        public ActionResult RateIndex()
        {
            return View("RateIndex");
        }

        public StringBuilder ProductInfo()
        {
            string securityCode = Request["securityCode"];
            SecurityModel model = new SecuritiesController().GetSecurity(4);
            StringBuilder JsonString = new StringBuilder();
            JsonString.Append("{");
            JsonString.Append(GetSummaries(model.Code, model));
            JsonString.Append("}");
            ViewBag.DataPrice = JsonString;
            return JsonString;
        }

        public StringBuilder SecurityInfo()
        {
            string securityCode = Request["securityCode"];
            int securityId = GetSecurityIdByCode(securityCode);
            //int securityId = System.Convert.ToInt16(Request["securityCode"]);
            SecurityModel model = new SecuritiesController().GetSecurity(securityId);
            StringBuilder JsonString = new StringBuilder();
            JsonString.Append("{");
            JsonString.Append(GetSummaries(model.Code, model));
            JsonString.Append("}");
            ViewBag.DataPrice = JsonString;
            return JsonString;
        }

        public class SummaryReturns
        {
            public string MonthReturn { get; set; }
            public string QuarterReturn { get; set; }
            public string HalfYearReturn { get; set; }
            public string YearReturn { get; set; }
            public string TwoYearsReturn { get; set; }
            public string ThreeYearsReturn { get; set; }
            public string FifeYearsReturn { get; set; }
        }

        public class AllSummaryReturns
        {
            public SummaryReturns Capital { get; set; }
            public SummaryReturns Income { get; set; }
            public SummaryReturns Total { get; set; }
            public SummaryReturns BenchTR { get; set; }
            public SummaryReturns SecondBenchTR { get; set; }

            public AllSummaryReturns()
            {
                Capital = new SummaryReturns();
                Income = new SummaryReturns();
                Total = new SummaryReturns();
                BenchTR = new SummaryReturns();
                SecondBenchTR = new SummaryReturns();
            }
        }

        public SummaryReturns CreateSummaryReturns(AnalyticsDataSet analyticsDataSet, string code, string returnType)
        {
            SummaryReturns summaryReturns = new SummaryReturns();
            var monthesCBA = analyticsDataSet.Performance.Where(w => w.Code == code);
            summaryReturns.MonthReturn = monthesCBA.Where(w => w.Key == "One Month " + returnType + " Return").First().Value;
            summaryReturns.QuarterReturn = monthesCBA.Where(w => w.Key == "3 Months " + returnType + " Return").First().Value;
            summaryReturns.HalfYearReturn = monthesCBA.Where(w => w.Key == "6 Months " + returnType + " Return").First().Value;
            summaryReturns.YearReturn = monthesCBA.Where(w => w.Key == "12 Months " + returnType + " Return").First().Value;
            summaryReturns.TwoYearsReturn = monthesCBA.Where(w => w.Key == "24 Months " + returnType + " Return").First().Value;
            summaryReturns.ThreeYearsReturn = monthesCBA.Where(w => w.Key == "36 Months " + returnType + " Return").First().Value;
            summaryReturns.FifeYearsReturn = monthesCBA.Where(w => w.Key == "60 Months " + returnType + " Return").First().Value;
            return summaryReturns;
        }

        public StringBuilder GetPerfomanceSummaryReturns(AnalyticsDataSet analyticsDataSet)
        {
            StringBuilder json = new StringBuilder();
            AllSummaryReturns allSummaryReturns = new AllSummaryReturns();
            allSummaryReturns.Capital = CreateSummaryReturns(analyticsDataSet, "CBA", "Capital");
            allSummaryReturns.Income = CreateSummaryReturns(analyticsDataSet, "CBA", "Income");
            allSummaryReturns.Total = CreateSummaryReturns(analyticsDataSet, "CBA", "Total");
            allSummaryReturns.BenchTR = CreateSummaryReturns(analyticsDataSet, "XTL", "Total");
            allSummaryReturns.SecondBenchTR = CreateSummaryReturns(analyticsDataSet, "XJO", "Total");
            return json;
        }

        public StringBuilder UpdateProductCalculation(int productVersionId, int sortType = 0)
        {
            StringBuilder jsonResponce = new StringBuilder();
            jsonResponce.Append("{");
            jsonResponce.Append(GetAllocationList(productVersionId, sortType));
            jsonResponce.Append("}");
            return jsonResponce;
        }

        public StringBuilder ProductCalculation(int productVersionId, int sortType = 1)
        {
            StringBuilder jsonResponce = new StringBuilder();
            Dictionary<int, SecurityItem> securityList = new Dictionary<int, SecurityItem>();
            Dictionary<string, List<SecurityItem>> securityListByGics = new Dictionary<string, List<SecurityItem>>();
            securityList = ProductCalculationList(productVersionId);
            if (securityList.Count != 0)
                securityList = CalculateSecuritiesReturns(securityList);
            securityListByGics = sortType == 0 ? GetSecurityByGics(securityList) : GetSecurityByAssetClass(securityList);
            //jsonResponce.Append("{");
            jsonResponce.Append(CreateSecurityListJson(securityListByGics));
            //jsonResponce.Append("}");
            return jsonResponce;
        }

       

        public Dictionary<string, List<SecurityItem>> GetSecurityByAssetClass(Dictionary<int, SecurityItem> securitiesList)
        {
            Dictionary<int, string> assetClassTypes = new Dictionary<int, string>();
            Dictionary<string, List<SecurityItem>> securityByGics = new Dictionary<string, List<SecurityItem>>();
            string assetClassName = "";
            string tableName = UtilsConvert.GetNameTable<AssetClass>();
            string id = UtilsConvert.GetPropertyName<AssetClass>(x => x.Id);
            string className = UtilsConvert.GetPropertyName<AssetClass>(x => x.Class);
            string isDeleted = UtilsConvert.GetPropertyName<AssetClass>(x => x.IsDeleted);
            string req = "select " + id + ", " + className +
                         " from " + tableName;
            //+ " where " + isDeleted + " = 0";
            using (SqlConnection connect = new SqlConnection(constr))
            {
                connect.Open();
                using (SqlCommand sqlcomcount = new SqlCommand(req, connect))
                {
                    SqlDataReader reader = sqlcomcount.ExecuteReader();
                    while (reader.Read())
                    {
                        assetClassTypes.Add((int)reader[0], reader.GetString(1));
                    }
                }
                connect.Close();
            }

            foreach (var item in securitiesList)
            {
                foreach (var assets in item.Value.assetClassList)
                {
                    if (assets == 0)
                        assetClassName = "Unlisted Asset Class";
                    else
                        assetClassName = assetClassTypes[assets];

                    if (securityByGics.ContainsKey(assetClassName))
                    {
                        securityByGics[assetClassName].Add(item.Value);
                    }
                    else
                    {
                        securityByGics.Add(assetClassName, new List<SecurityItem>());
                        securityByGics[assetClassName].Add(item.Value);
                    }
                }
            }

            //Sorting Dictionary by Key i.e. Asset Class
            var sortedDic = new SortedDictionary<string, List<SecurityItem>>(securityByGics);
            securityByGics = new Dictionary<string, List<SecurityItem>>(sortedDic);
            return securityByGics;
        }
        public Dictionary<int, SecurityItem> ProductCalculationList
            (int productVersionId, Dictionary<int, SecurityItem> productVersion = null, double allocation = 1, ModelsModel model = null, int clientId = 0, string invServiceCode = null)
        {
            Dictionary<int, SecurityItem> securityList = productVersion ?? new Dictionary<int, SecurityItem>();
            var productModel = new ProductModel();

            string getProductDetail = "SELECT ProductVersion.ProductVersionID, Product.Code, ProductVersion.MajorVersion, ProductVersion.MinorVersion" +
                                       " FROM Product INNER JOIN" +
                                       " ProductVersion ON Product.ProductID = ProductVersion.ProductID" +
                                       " WHERE (Product.IsDeleted = 0) AND ProductVersion.ProductVersionId=" + productVersionId + " AND (ProductVersion.IsDeleted = 0)";

            using (SqlConnection connect = new SqlConnection(constr))
            {
                connect.Open();
                using (SqlCommand sqlcomcount = new SqlCommand(getProductDetail, connect))
                {
                    SqlDataReader reader = sqlcomcount.ExecuteReader();

                    while (reader.Read())
                    {
                        productModel.Code = reader.GetString(1);
                        if (reader[2] != System.DBNull.Value || reader[3] != System.DBNull.Value)
                        {
                            productModel.ProductVersion = ((reader[2] != System.DBNull.Value ? reader[2] : 0) + "." + (reader[3] != System.DBNull.Value ? reader[3] : 0));
                        }
                    }
                }
                connect.Close();
            }

            int productAssetClassId = 0;
            string getProductAssetClass =
               "SELECT [Product].AssetClassId" +
               " FROM [Invest].[dbo].[ProductVersion], [Invest].[dbo].[Product]" +
               " Where [ProductVersionID] = " + productVersionId +
               " And [ProductVersion].ProductID = [Product].ProductID" +
               " And [ProductVersion].[IsDeleted] = 0";

            using (var connect = new SqlConnection(constr))
            {
                connect.Open();
                using (var sqlcomcount = new SqlCommand(getProductAssetClass, connect))
                {
                    SqlDataReader reader = sqlcomcount.ExecuteReader();
                    while (reader.Read())
                    {
                        productAssetClassId = (int)reader[0];
                    }
                }
                connect.Close();
            }
            string getSecurityList =
                "SELECT [SecurityListId]" +
                " FROM [Invest].[dbo].[ProductAssociation]" +
                " Where [ProductVersionID] = " + productVersionId +
                " And [IsDeleted] = 0 And [SecurityListId] != 0";
            string req2 = "";
            int iteration = 0;
            using (SqlConnection connect = new SqlConnection(constr))
            {
                connect.Open();
                using (SqlCommand sqlcomcount = new SqlCommand(getSecurityList, connect))
                {
                    SqlDataReader reader = sqlcomcount.ExecuteReader();
                    while (reader.Read())
                    {
                        if (iteration > 0)
                        {
                            req2 += " Union ";
                        }
                        //Need to get only Max Status Date records
                        req2 += " SELECT  sl.SecurityId, s.Code, s.Name, s.GICSTypeId, sl.Allocation *" +
                                " (SELECT Allocation FROM  ProductAssociation" +
                                "  WHERE (IsDeleted = 0) AND (ProductVersionID =" + productVersionId +
                                " ) AND (SecurityListId = " + (int)reader[0] + ")) / 100" +
                                " ,'" + productModel.Code + "' as ProductCode," + productModel.ProductVersion +
                                "  as ProductVersion,'" + (model != null ? model.ModelCode : string.Empty) +
                                " ' as ModelCode," + (model != null ? model.ModelVersion : "0") + " as ModelVersion," +
                                "  SecurityType.Type, SecurityCategory.Category, TermDeposit.InstitutionId,s.UnitisedId" +
                                " FROM SecurityListDetail AS sl INNER JOIN" +
                                " (SELECT MAX(Id) AS MAXID, SecurityListId, SecurityId, MAX(StatusDate) AS MaxDateTime" +
                                " FROM  SecurityListDetail" +
                                " WHERE  (SecurityListId = " + (int)reader[0] + ") AND (IsDeleted = 0)" +
                                " GROUP BY SecurityListId, SecurityId) AS groupedtt ON sl.SecurityListId = groupedtt.SecurityListId AND sl.SecurityId = groupedtt.SecurityId AND sl.StatusDate = groupedtt.MaxDateTime AND " +
                                " sl.Id = groupedtt.MAXID INNER JOIN" +
                                " Security AS s ON s.Id = sl.SecurityId LEFT OUTER JOIN" +
                                " SecurityCategory ON s.SecurityCategoryId = SecurityCategory.Id LEFT OUTER JOIN" +
                                " SecurityType ON s.SecurityTypeId = SecurityType.Id LEFT OUTER JOIN" +
                                " TermDeposit ON s.Id = TermDeposit.SecurityId" +
                                " WHERE (sl.SecurityListId = " + (int)reader[0] + ") AND (sl.IsDeleted = 0) AND (SecurityCategory.IsDeleted = 0) AND (SecurityType.IsDeleted = 0)";

                        iteration++;
                    }
                }
                connect.Close();
            }
            if (iteration > 0)
            {
                req2 += " Union ";
            }
            req2 +=
                " SELECT ProductAssociation.SecurityId, Security.Code, Security.Name, Security.GICSTypeId, ProductAssociation.Allocation" +
                " ,'" + productModel.Code + "' as ProductCode," + productModel.ProductVersion + " as ProductVersion,'" +
                (model != null ? model.ModelCode : string.Empty) + "' as ModelCode," +
                (model != null ? model.ModelVersion : "0") + " as ModelVersion," +
                " SecurityType.Type, SecurityCategory.Category,TermDeposit.InstitutionId,Security.UnitisedId" +
                " FROM   ProductAssociation INNER JOIN" +
                " Security ON ProductAssociation.SecurityId = Security.Id LEFT OUTER JOIN" +
                " TermDeposit ON Security.Id = TermDeposit.SecurityId LEFT OUTER JOIN" +
                " SecurityCategory ON Security.SecurityCategoryId = SecurityCategory.Id LEFT OUTER JOIN" +
                " SecurityType ON Security.SecurityTypeId = SecurityType.Id" +
                " WHERE  (ProductAssociation.ProductVersionID = " + productVersionId + ") AND (ProductAssociation.SecurityId <> 0) AND (ProductAssociation.IsDeleted = 0) AND (Security.IsDeleted = 0) AND (SecurityCategory.IsDeleted = 0) AND " +
                " (SecurityType.IsDeleted = 0)";

            using (SqlConnection connect = new SqlConnection(constr))
            {
                connect.Open();
                using (SqlCommand sqlcomcount = new SqlCommand(req2, connect))
                {
                    SqlDataReader reader = sqlcomcount.ExecuteReader();
                    while (reader.Read())
                    {
                        SecurityItem securityItem = new SecurityItem();
                        if (reader[0] != System.DBNull.Value &&
                            reader[1] != System.DBNull.Value &&
                            reader[2] != System.DBNull.Value)
                        {
                            securityItem.id = (int)reader[0];
                            securityItem.code = reader.GetString(1);
                            securityItem.name = reader.GetString(2);
                            if (reader[3] != System.DBNull.Value)
                            {
                                securityItem.gics = (int)reader[3];
                            }

                            if (reader[4] != System.DBNull.Value)
                            {
                                securityItem.allocation = (double)(reader.GetDecimal(4)) * allocation;
                            }
                            securityItem.ProductCode = reader.GetString(5);
                            if (reader[6] != System.DBNull.Value)
                            {
                                securityItem.ProductVersion = reader[6].ToString();
                            }
                            securityItem.ModelCode = reader.GetString(7);
                            if (reader[8] != System.DBNull.Value)
                            {
                                securityItem.ModelVersion = reader[8].ToString();
                            }
                            if (reader[9] != System.DBNull.Value)
                            {
                                securityItem.securityType = reader.GetString(9);
                            }
                            if (reader[10] != System.DBNull.Value)
                            {
                                securityItem.securityCategory = reader.GetString(10);
                            }
                            if (reader[11] != System.DBNull.Value)
                            {
                                securityItem.InstitutionId = (int)reader[11];
                            }
                            if (reader[12] != System.DBNull.Value)
                            {
                                securityItem.UnitisedId = (int)reader[12];
                            }
                            securityItem.InvServiceCode = invServiceCode;
                            securityItem.clientId = clientId;
                            if (securityList.ContainsKey((int)reader[0]))
                                securityList[(int)reader[0]].allocation += securityItem.allocation;
                            else
                            {
                                securityList.Add((int)reader[0], securityItem);
                                //Adding Product Level Asset Class Id
                                securityList[(int)reader[0]].assetClassList.Add(productAssetClassId);
                            }
                        }
                    }
                }
            }

            return securityList;
        }

        public Dictionary<int, SecurityItem> GetSecurities(ProductModel productModel)
        {
            JQueryDataTableParamModel param = new JQueryDataTableParamModel();
            param.ToDate = DateTime.Now;
            Dictionary<int, SecurityItem> securityList = new Dictionary<int, SecurityItem>();
            for (int i = 0; i < productModel.SecurityAssociation.Count; i++)
            {
                if (productModel.SecurityAssociation[i].SecurityId != null)
                {
                    int securityId = 0;
                    if (productModel.SecurityAssociation[i].SecurityId != null)
                    {
                        securityId = productModel.SecurityAssociation[i].SecurityId ?? 0;
                        if (securityList.ContainsKey(securityId))
                        {
                            securityList[securityId].allocation += (double)(productModel.SecurityAssociation[i].Allocation ?? 0);
                        }
                        else
                        {
                            SecurityItem securityItem = new SecurityItem();
                            securityItem.id = securityId;
                            securityItem.allocation = (double)(productModel.SecurityAssociation[i].Allocation ?? 0);
                            securityItem.name = productModel.SecurityAssociation[i].SecurityName;
                            securityItem.code = productModel.SecurityAssociation[i].SecurityCode;
                            securityList.Add(securityId, securityItem);
                        }
                    }
                }
                else if (productModel.SecurityAssociation[i].SecurityListId != null)
                {
                    double productAllocation = (double)(productModel.SecurityAssociation[i].Allocation ?? 0);
                    param.Id = productModel.SecurityAssociation[i].SecurityListId;
                    var responseViewModle = ProductSecurityDetailPageData(param, param.Id, param.ToDate, param.IsAll, "", param.PFWeight, true);
                    var securityModelList = responseViewModle.ModelList;
                    for (int j = 0; j < securityModelList.Count; j++)
                    {
                        int securityId = securityModelList[j].Id;
                        if (securityList.ContainsKey(securityId))
                        {
                            securityList[securityId].allocation += (double)(productModel.SecurityAssociation[i].Allocation ?? 0);
                        }
                        else
                        {
                            SecurityItem securityItem = new SecurityItem();
                            securityItem.allocation = (double)(securityModelList[j].Allocation ?? 0) * productAllocation;
                            securityItem.name = securityModelList[j].SecurityName;
                            securityItem.code = securityModelList[j].SecurityCode;
                            securityItem.id = securityId;
                            securityList.Add(securityId, securityItem);
                        }
                    }
                }
            }
            return securityList;
        }       

        public string CalculateHoldings(int clientId, DateTime? holdingDate)
        {
            bool isFound = false;
            int recordAddCount = 0;
            int recordUpdateCount = 0;
            if (holdingDate == null)
            {
                holdingDate = DateTime.Now;
            }
            //no time
            holdingDate = System.Convert.ToDateTime(holdingDate.Value.ToShortDateString());
            string message = string.Empty;
            InvestContext context = new InvestContext(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            List<int> clientIds = context.ClientAccounts.Where(a => !a.EntityCore.IsDeleted && (clientId == 0 || a.EntityCoreId == clientId)).Select(a => a.EntityCoreId).ToList();
            foreach (var client in clientIds)
            {
                var responseViewModle = new HoldingController().GetFilteredHoldingsDataBySecurity(clientId, holdingDate);
                List<HoldingModel> holdingList = responseViewModle.ModelList;
                var invServices = context.InvestmentPrograms.Where(i => i.EntityCoreId == client && !i.IsDeleted).ToList();

                //Remove Existing recordd for a client of that day if any.
                var existingRecords = context.CalculatedHoldings.Where(w => w.ClientId == client && DbFunctions.TruncateTime(w.HoldingDate) == DbFunctions.TruncateTime(holdingDate)).ToList();
                if (existingRecords.Any())
                {
                    context.CalculatedHoldings.RemoveRange(existingRecords);
                    context.SaveChanges();
                }
                foreach (var invServiceItem in invServices)
                {
                    Dictionary<string, List<SecurityItem>> holdingsData = HoldingsCalculation(client, invServiceItem.InvestmentProgramID, invServiceItem.InvestmentServiceCode, holdingDate, holdingList);
                    try
                    {
                        if (holdingsData != null)
                        {
                            foreach (var holdingkey in holdingsData)
                            {
                                foreach (var holdingitem in holdingkey.Value)
                                {
                                    if (holdingitem.amount == 0)
                                    {
                                        continue;
                                    }

                                    var calculatedHolding = new CalculatedHolding();
                                    calculatedHolding.AccountId = holdingitem.AccountId;
                                    calculatedHolding.ClientId = holdingitem.clientId;
                                    calculatedHolding.HoldingDate = holdingDate;
                                    calculatedHolding.InvestmentServiceCode = holdingitem.InvServiceCode;
                                    calculatedHolding.InvestmentProgramId = invServiceItem.InvestmentProgramID;
                                    calculatedHolding.HoldingUnits = holdingitem.units;
                                    calculatedHolding.HoldingValue = holdingitem.amount;
                                    calculatedHolding.ModelCode = holdingitem.ModelCode;
                                    calculatedHolding.ModelVersion = holdingitem.ModelVersion;
                                    calculatedHolding.ProductCode = holdingitem.ProductCode;
                                    calculatedHolding.ProductVersion = holdingitem.ProductVersion;
                                    calculatedHolding.MarketValue = holdingitem.unitPrice;
                                    calculatedHolding.PriceDate = holdingitem.priceDate != null ? System.Convert.ToDateTime(holdingitem.priceDate.Value.ToShortDateString()) : (DateTime?)null;
                                    calculatedHolding.SecurityCode = holdingitem.code.Trim();
                                    calculatedHolding.MarketCode = holdingitem.MarketCode;
                                    context.CalculatedHoldings.Add(calculatedHolding);

                                    recordAddCount++;
                                    if (!isFound)
                                        isFound = true;

                                }
                            }
                            if (isFound)
                                context.SaveChanges();
                        }

                        //Holding Securities (Cash or non security) which are not yet associated with Inv Service but account is associated with inv service
                        if (holdingList.Any())
                        {
                            var nonSecCashList = holdingList.Where(a => a.SecurityId == 0 && a.InvestmentServiceId == invServiceItem.InvestmentProgramID).ToList();
                            if (nonSecCashList.Any())
                            {

                                foreach (var holdingitem in nonSecCashList)
                                {
                                    if (holdingitem.Amount == 0)
                                    {
                                        continue;
                                    }

                                    var calculatedHolding = new CalculatedHolding();
                                    calculatedHolding.AccountId = holdingitem.AccountID;
                                    calculatedHolding.ClientId = client;
                                    calculatedHolding.InvestmentServiceCode = invServiceItem.InvestmentServiceCode;
                                    calculatedHolding.InvestmentProgramId = invServiceItem.InvestmentProgramID;
                                    calculatedHolding.HoldingDate = holdingDate;
                                    calculatedHolding.HoldingUnits = holdingitem.Units;
                                    calculatedHolding.HoldingValue = holdingitem.Amount;
                                    calculatedHolding.MarketValue = holdingitem.UnitPrice;
                                    calculatedHolding.PriceDate = holdingitem.PriceDate != null ? System.Convert.ToDateTime(holdingitem.PriceDate.Value.ToShortDateString()) : (DateTime?)null;//holdingitem.PriceDate;
                                    calculatedHolding.SecurityCode = holdingitem.AssetCode.Trim();
                                    calculatedHolding.MarketCode = holdingitem.Market;

                                    context.CalculatedHoldings.Add(calculatedHolding);

                                    recordAddCount++;
                                    if (!isFound)
                                        isFound = true;

                                }
                                if (isFound)
                                    context.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        message = message + "-Exception Ocuured at ClientId: " + client + " .ExceptionMessage:" + ex.Message;
                    }
                }
                //For Dormants Accounts
                if (holdingList.Any())
                {
                    var dormantHoldings = holdingList.Where(a => a.InvestmentServiceId == null).ToList();
                    if (dormantHoldings.Any())
                    {
                        foreach (var holdingitem in dormantHoldings)
                        {
                            if (holdingitem.Amount == 0)
                            {
                                continue;
                            }

                            var calculatedHolding = new CalculatedHolding();
                            calculatedHolding.AccountId = holdingitem.AccountID;
                            calculatedHolding.ClientId = client;
                            calculatedHolding.HoldingDate = holdingDate;
                            calculatedHolding.HoldingUnits = holdingitem.Units;
                            calculatedHolding.HoldingValue = holdingitem.Amount;
                            calculatedHolding.MarketValue = holdingitem.UnitPrice;
                            calculatedHolding.PriceDate = System.Convert.ToDateTime(holdingitem.PriceDate.Value.ToShortDateString()); //holdingitem.PriceDate;
                            calculatedHolding.SecurityCode = holdingitem.AssetCode.Trim();
                            calculatedHolding.MarketCode = holdingitem.Market;
                            context.CalculatedHoldings.Add(calculatedHolding);
                            recordAddCount++;
                            if (!isFound)
                                isFound = true;
                        }
                        if (isFound)
                            context.SaveChanges();
                    }
                }
            }

            if (isFound)
            {
                message = message + string.Format("{0} Record(s) Added Sucessfully; {1} Record(s) Updated Successfully.", recordAddCount, recordUpdateCount);
            }
            else
            {
                message = "No Record Found";
            }
            return message;
        }

        public StringBuilder CreateSecurityListJsonFromInvService(Dictionary<string, List<SecurityItem>> list, int invServiceId, decimal invServiceHoldingSum)
        {
            StringBuilder jsonResponce = new StringBuilder();
            List<Bands> brandItem = new List<Bands>();
            string invService = " SELECT InvestmentProgramBands.BandName, InvestmentProgramBands.BandMinValue, InvestmentProgramBands.BandMaxValue" +
                                 " FROM InvestmentPrograms INNER JOIN" +
                                 " InvestmentProgramBands ON InvestmentPrograms.InvestmentProgramID = InvestmentProgramBands.InvestmentProgramId" +
                                 " WHERE InvestmentPrograms.InvestmentProgramID =" + invServiceId + " AND InvestmentPrograms.IsDeleted = 0";

            using (SqlConnection connect = new SqlConnection(constr))
            {
                connect.Open();
                using (SqlCommand sqlcomcount = new SqlCommand(invService, connect))
                {
                    SqlDataReader reader = sqlcomcount.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader[0] != DBNull.Value)
                        {
                            var bandItem = new Bands()
                            {
                                BandName = reader.GetString(0),
                                BandMinValue = (reader[1] != DBNull.Value ? (decimal?)(reader.GetDecimal(1)) : 0),
                                BandMaxValue = (reader[2] != DBNull.Value ? (decimal?)(reader.GetDecimal(2)) : 0)
                            };
                            brandItem.Add(bandItem);
                        }
                    }
                }
                connect.Close();
            }


            jsonResponce.Append("\"gics\":{");
            foreach (var item in list)
            {
                //jsonResponce.Append("\"gics" + item.Key + "\":  [");
                jsonResponce.Append("\"item" + item.Key + "\":{"); //startItem
                //Sorting by Code
                var sortedList = item.Value.OrderBy(o => o.code).ToList();
                int secDataCount = 0;
                var sectionTotalAmount = item.Value.Sum(s => s.amount) ?? 0;
                double allocation = item.Value.Sum(s => s.allocation);
                double monthReturn = item.Value.Sum(s => s.monthAllocation);
                double yearReturn = item.Value.Sum(s => s.yearAllocation);
                for (int i = 0; i < sortedList.Count(); i++)
                {
                    //don't display zero value records
                    if (sortedList[i].amount == 0)
                    {
                        continue;
                    }

                    decimal amount = sortedList[i].amount ?? 0;
                    jsonResponce.Append("\"securityData" + secDataCount + "\":{");
                    jsonResponce.Append("\"id\":\"" + sortedList[i].id + "\",");
                    jsonResponce.Append("\"name\":\"" + sortedList[i].name + "\",");
                    jsonResponce.Append("\"code\":\"" + sortedList[i].code + "\",");
                    jsonResponce.Append("\"units\":\"" + sortedList[i].units + "\",");
                    jsonResponce.Append("\"unitPrice\":\"" + sortedList[i].unitPrice + "\",");
                    jsonResponce.Append("\"amount\":\"" + amount + "\",");
                    jsonResponce.Append("\"allocation\":\"" + sortedList[i].allocation + "\",");
                    jsonResponce.Append("\"monthReturn\":\"" + sortedList[i].monthAllocation + "\",");
                    jsonResponce.Append("\"yearReturn\":\"" + sortedList[i].yearAllocation + "\",");
                    decimal holdingWeight = 0;

                    if (invServiceHoldingSum > 0)
                    {
                        holdingWeight = (amount / invServiceHoldingSum) * 100;
                    }
                    var difference = holdingWeight - (decimal)sortedList[i].allocation;

                    jsonResponce.Append("\"holdingWeight\":\"" + holdingWeight + "\",");
                    //Traffic Light
                    string lightColor = "Red";
                    foreach (var brand in brandItem)
                    {

                        if ((Math.Abs(difference) >= (decimal)brand.BandMinValue) && (Math.Abs(difference) <= (decimal)brand.BandMaxValue))
                        {
                            lightColor = brand.BandName;
                        }
                    }


                    jsonResponce.Append("\"traficLight\":\"" + lightColor + "\"");
                    jsonResponce.Append("},");
                    //Increament the sec data count
                    secDataCount++;
                }
                jsonResponce.Append("\"gicsData\":{");
                jsonResponce.Append("\"name\":\"" + item.Key + "\",");
                jsonResponce.Append("\"weight\":\"" + allocation + "\",");
                jsonResponce.Append("\"yearReturn\":\"" + yearReturn + "\",");
                jsonResponce.Append("\"monthReturn\":\"" + monthReturn + "\",");
                jsonResponce.Append("\"totalAmount\":\"" + sectionTotalAmount + "\"");
                jsonResponce.Append("}");
                jsonResponce.Append("},"); //end Item
            }

            if (list.Count > 0)
                jsonResponce.Remove(jsonResponce.Length - 1, 1);
            jsonResponce.Append("}");//end Item
            return jsonResponce;
        }

      

        //For Investment Report
        #region InvestmentServiceReport

        [HttpGet]
        public InvestmentProgramModel GetRangeByInvProgIdAssetClass(int inprogId, int AssetClassId)
        {
            var modelView = new InvestmentProgramModel { InvestmentProgramID = inprogId, AssetClassId = AssetClassId, FilterOption = FilterOption.GetSecurityAssetClassRange };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramModel>(viewJson);
            return responseModel.Model;
        }
        [HttpGet]
        public InvestmentProgramTemplateModel GetRangeTemplateByInvProgIdAssetClass(int inprogId, int AssetClassId)
        {
            var modelView = new InvestmentProgramTemplateModel { InvestmentProgramID = inprogId, AssetClassId = AssetClassId, FilterOption = FilterOption.GetSecurityAssetClassRange };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramTemplateModel>(viewJson);
            return responseModel.Model;
        }
        public List<SecurityItem> CreateSecurityListFromInvService(Dictionary<string, List<SecurityItem>> list, int invServiceId)
        {
            List<SecurityItem> SecuirtyItem = new List<SecurityItem>();
            foreach (var item in list)
            {
                //Sorting by Code
                var sortedList = item.Value.OrderBy(o => o.code).ToList();
                if (sortedList.Count == 0)
                {
                    SecurityItem model = new SecurityItem();
                    model.KeyName = item.Key;
                    continue;
                }
                for (int i = 0; i < sortedList.Count(); i++)
                {
                    SecurityItem model = new SecurityItem();
                    model.KeyName = item.Key;
                    //don't display zero value records
                    //if (sortedList[i].amount == 0)
                    //{
                    //    continue;
                    //}
                    var Range = GetRangeByInvProgIdAssetClass(invServiceId, sortedList[i].assetClassList[0]);
                    decimal amount = sortedList[i].amount ?? 0;
                    model.name = sortedList[i].name;
                    model.code = sortedList[i].code;
                    model.amount = amount;
                    model.minRange = (Range.InvprogStretagicAllocation != null ? Range.InvprogStretagicAllocation.MinValue : -1);
                    model.maxRange = (Range.InvprogStretagicAllocation != null ? Range.InvprogStretagicAllocation.MaxValue : -1);
                    model.allocation = sortedList[i].allocation;
                    model.strategicAllocation = (Range.InvprogStretagicAllocation != null ? Range.InvprogStretagicAllocation.StrategicAssetAllocation : null);
                    SecuirtyItem.Add(model);
                }
            }
            return SecuirtyItem;
        }
        public List<SecurityItem> CreateSecurityTemplateListFromInvService(Dictionary<string, List<SecurityItem>> list, int invServiceId)
        {
            List<SecurityItem> SecuirtyItem = new List<SecurityItem>();
            foreach (var item in list)
            {
                //Sorting by Code
                var sortedList = item.Value.OrderBy(o => o.code).ToList();
                if (sortedList.Count == 0)
                {
                    SecurityItem model = new SecurityItem();
                    model.KeyName = item.Key;
                    continue;
                }
                for (int i = 0; i < sortedList.Count(); i++)
                {
                    SecurityItem model = new SecurityItem();
                    model.KeyName = item.Key;
                    var Range = GetRangeTemplateByInvProgIdAssetClass(invServiceId, sortedList[i].assetClassList[0]);
                    decimal amount = sortedList[i].amount ?? 0;
                    model.name = sortedList[i].name;
                    model.code = sortedList[i].code;
                    model.amount = amount;
                    model.minRange = (Range.InvprogStretagicAllocation != null ? Range.InvprogStretagicAllocation.MinValue : -1);
                    model.maxRange = (Range.InvprogStretagicAllocation != null ? Range.InvprogStretagicAllocation.MaxValue : -1);
                    model.allocation = sortedList[i].allocation;
                    model.strategicAllocation = (Range.InvprogStretagicAllocation != null ? Range.InvprogStretagicAllocation.StrategicAssetAllocation : null);
                    SecuirtyItem.Add(model);
                }
            }
            return SecuirtyItem;
        }
       

        
        #endregion


        [HttpPost]
        public ActionResult DeleteSecurityPrice(SecurityPriceModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult FileUploadHandlerValid()
        {
            ActionResult result = null;
            foreach (var fileKey in Request.Files.AllKeys)
            {
                var file = Request.Files[fileKey];
                try
                {
                    if (file != null && !string.IsNullOrWhiteSpace(file.FileName) && !string.IsNullOrEmpty(file.FileName))
                    {
                        var fileName = System.IO.Path.GetFileName(file.FileName);
                        var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                        file.SaveAs(path);
                        string conwork = ConfigurationManager.ConnectionStrings["WFConnection"].ConnectionString;
                        var conv = new ConvertXASXUseSQL();
                        using (var con = new SqlConnection(conwork))
                        {
                            con.Open();
                            var newcode = conv.ManualGetNewCodeSecurity(path, con);
                            if (newcode != null && newcode.Any())
                            {
                                var model = new CodeListAndPathViewModel { ListCode = newcode, PathFile = path };
                                result = PartialView("_NewSecurityCodeModel", model);
                            }
                            else
                            {
                                return RedirectToAction("FileUploadHandler", "Investment", new { pathfile = path });
                            }
                        }
                    }
                    else
                    {
                        var ex = new Exception("Not file to upload");
                        var m = new HandleErrorInfo(ex, "Investment", "FileUploadHandlerValid");
                        result = View("Error", m);
                    }
                }
                catch (Exception ex)
                {
                    var m = new HandleErrorInfo(ex, "Investment", "FileUploadHandlerValid");
                    result = View("Error", m);
                }
            }
            return result;
        }

        [HttpGet]
        public ActionResult FileUploadHandler(string pathfile)
        {
            ActionResult result;
            try
            {
                var conwork = ConfigurationManager.ConnectionStrings["WFConnection"].ConnectionString;
                var conv = new ConvertXASXUseSQL();
                var res = new ResultConvert();
                using (SqlConnection connect = new SqlConnection(conwork))
                {
                    connect.Open();
                    conv.ManualConvertSecurityPrice(pathfile, res, connect);
                }
                //result = View("SecuritiesPrice");
                result = PartialView("_FileUploadProduct", res);
            }
            catch (Exception ex)
            {
                var m = new HandleErrorInfo(ex, "Investment", "FileUploadHandler");
                result = View("Error", m);
            }

            return result;
        }

        public ActionResult DownLoadTemplateSecurityPrice()
        {
            ActionResult result = null;
            var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), "SecurityPriceTemplate.csv");
            if (System.IO.File.Exists(path))
            {
                byte[] contents = System.IO.File.ReadAllBytes(path);
                result = File(contents, "text/csv", "SecurityPriceTemplate.csv");
            }
            else
            {
                Exception ex = new Exception("File SecurityPriceTemplate.csv not found");
                HandleErrorInfo m = new HandleErrorInfo(ex, "Investment", "DownLoadTemplateSecurityPrice");
                result = View("Error", m);
            }
            return result;
        }

        #endregion

        #region Securities-Income

        [HttpPost]
        public ActionResult FileUploadHandlerValidIncome()
        {
            ActionResult result = null;
            foreach (var fileKey in Request.Files.AllKeys)
            {
                var file = Request.Files[fileKey];
                try
                {
                    if (file != null && !string.IsNullOrWhiteSpace(file.FileName) && !string.IsNullOrEmpty(file.FileName))
                    {
                        var fileName = System.IO.Path.GetFileName(file.FileName);
                        var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                        file.SaveAs(path);
                        string conwork = ConfigurationManager.ConnectionStrings["WFConnection"].ConnectionString;
                        var conv = new ConvertXASXUseSQL();
                        using (var con = new SqlConnection(conwork))
                        {
                            con.Open();
                            var newcode = conv.ManualGetNewCodeSecurity(path, con);
                            if (newcode != null && newcode.Any())
                            {
                                var model = new CodeListAndPathViewModel { ListCode = newcode, PathFile = path };
                                result = PartialView("_NewSecurityCodeModel", model);
                            }
                            else
                            {
                                return RedirectToAction("FileUploadHandlerIncome", "Investment", new { pathfile = path });
                            }
                        }
                    }
                    else
                    {
                        var ex = new Exception("Not file to upload");
                        var m = new HandleErrorInfo(ex, "Investment", "FileUploadHandlerValidIncome");
                        result = View("Error", m);
                    }
                }
                catch (Exception ex)
                {
                    var m = new HandleErrorInfo(ex, "Investment", "FileUploadHandlerValidIncome");
                    result = View("Error", m);
                }
            }
            return result;
        }

        [HttpGet]
        public ActionResult FileUploadHandlerIncome(string pathfile)
        {
            ActionResult result;
            try
            {
                var conwork = ConfigurationManager.ConnectionStrings["WFConnection"].ConnectionString;
                var conv = new ConvertPersian();
                var res = new ResultConvert();
                using (SqlConnection connect = new SqlConnection(conwork))
                {
                    connect.Open();
                    conv.RunDividend(pathfile, res, connect);
                }
                //result = View("SecuritiesPrice");
                result = PartialView("_FileUploadProduct", res);
            }
            catch (Exception ex)
            {
                var m = new HandleErrorInfo(ex, "Investment", "FileUploadHandlerIncome");
                result = View("Error", m);
            }

            return result;
        }

        public ActionResult DownLoadTemplateSecurityIncome()
        {
            ActionResult result;
            var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), "SecurityIncomeTemplate.csv");
            if (System.IO.File.Exists(path))
            {
                byte[] contents = System.IO.File.ReadAllBytes(path);
                result = File(contents, "text/csv", "SecurityIncomeTemplate.csv");
            }
            else
            {
                Exception ex = new Exception("File SecurityIncomeTemplate.csv not found");
                HandleErrorInfo m = new HandleErrorInfo(ex, "Investment", "DownLoadTemplateSecurityIncome");
                result = View("Error", m);
            }
            return result;
        }

        public ActionResult GetIncomeDetail()
        {
            return PartialView("_SecurityIncomeList");
        }

        public ActionResult GetIncomeDetailByFilter(JQueryDataTableParamModel param, int? securityId, DateTime? fromDate, DateTime? toDate)
        {
            if (securityId == 0 && fromDate == null && toDate == null)
            {
                return null;
            }

            var responseViewModle = SecurityIncomeData(param, securityId, fromDate, toDate);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSecurityDashboardDetail() //Security Dashboard
        {
            List<SelectListItem> InsulinResistance = new List<SelectListItem>();
            InsulinResistance.Add(new SelectListItem { Text = "", Value = "0" });
            InsulinResistance.Add(new SelectListItem { Text = "Asset Class", Value = "ddlAssetClass" });
            InsulinResistance.Add(new SelectListItem { Text = "Sub Asset Class", Value = "ddlSubAssetClass" });
            InsulinResistance.Add(new SelectListItem { Text = "Region", Value = "ddlRegionId" });
            InsulinResistance.Add(new SelectListItem { Text = "GICS - Sector", Value = "ddlGicsList" });
            InsulinResistance.Add(new SelectListItem { Text = "GICS - Industry Growth", Value = "ddlGicsTypeList" });
            ViewBag.InsulinResistance = InsulinResistance;
            List<SelectListItem> TopHoldingsCount = new List<SelectListItem>();
            TopHoldingsCount.Add(new SelectListItem { Text = "Top 5", Value = "showTop5" });
            TopHoldingsCount.Add(new SelectListItem { Text = "Top 10", Value = "showTop10" });
            TopHoldingsCount.Add(new SelectListItem { Text = "Top 20", Value = "showTop20" });
            ViewBag.TopHoldingsCount = TopHoldingsCount;

            var regionList = ListsMaintenance.GetRegionList();
            ViewData["regionList"] = regionList;

            var marketList = ListsMaintenance.GetMarketList();
            ViewData["marketlist"] = marketList;

            var subAssetClassList = ListsMaintenance.GetSubAssetClassList();
            ViewData["subAssetClassList"] = subAssetClassList;

            var assetClassList = ListsMaintenance.GetAssetClassList();
            ViewData["assetClassList"] = assetClassList;

            var gicsList = ListsMaintenance.GetGICSList();
            ViewData["gicsList"] = gicsList;

            var gicsTypeList = ListsMaintenance.GetGICSTypeList();
            ViewData["gicsTypeList"] = gicsTypeList;

            return PartialView("_SecurityDashboard");
        }

        //MyDashboard
        public ActionResult GetSecurityDashboardData(JQueryDataTableParamModel param, string filterType, int? filterValue, int? securityId, DateTime? fromDate, DateTime? toDate)
        {
            switch (filterType)
            {
                case "name":
                    break;
                case "ddlAssetClass":
                    param.sAssetClassSearch = new List<int?>() { filterValue };
                    break;
                case "ddlSubAssetClass":
                    param.sSubAssetClassSearch = new List<int?>() { filterValue };
                    break;
                case "ddlRegionId":
                    param.sRegionIdSearch = filterValue;
                    break;
                case "ddlGicsList":
                    param.sGicsSectorSearch = filterValue;
                    break;
                case "ddlGicsTypeList":
                    param.sGicsIndustryGrowthSearch = filterValue;
                    break;
            }

            var responseViewModle = SecurityPageData(param, "All");
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public List<DistributionDividendDetailModel> GetDividendsList(int? securityId, DateTime? fromDate, DateTime? toDate)
        {

            JQueryDataTableParamModel param = new JQueryDataTableParamModel();
            param.FromDate = fromDate ?? new DateTime();
            param.ToDate = toDate ?? DateTime.Now;

            var modelView = new DistributionDividendDetailModel
            {
                FilterOption = FilterOption.GetAllById,
                TableParam = param,
                SecurityId = securityId ?? 0,
                FromDate = param.FromDate,
                ToDate = param.ToDate
            };

            var json = Execute(Option.Get, modelView);

            List<DistributionDividendDetailModel> model = GetResponseModel<DistributionDividendDetailModel>(json).ModelList;
            return model;
        }

        public List<SecurityPriceModel> GetSecurityiesMonthlyPriceList(List<int> securityIds, DateTime? fromDate, DateTime? toDate)
        {
            JQueryDataTableParamModel param = new JQueryDataTableParamModel();
            param.sIdsSearch = securityIds;
            param.FromDate = fromDate;
            param.ToDate = toDate;

            var modelView = new SecurityPriceModel
            {
                FilterOption = FilterOption.GetAllById,
                TableParam = param,
                FromDate = fromDate,
                ToDate = toDate
            };

            var json = Execute(Option.Get, modelView);

            List<SecurityPriceModel> model = GetResponseModel<SecurityPriceModel>(json).ModelList;
            return model;
        }

        private ResponseModel<DistributionDividendDetailModel> SecurityIncomeData(JQueryDataTableParamModel param, int? securityId, DateTime? fromDate, DateTime? toDate, bool isDownlaod = false)
        {
            var modelView = new DistributionDividendDetailModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                SecurityId = securityId ?? 0,
                FromDate = fromDate,
                ToDate = toDate
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<DistributionDividendDetailModel>(json);
        }

        public void DownloadSecurityIncomeCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = SecurityIncomeData(Params, Params.Id, Params.FromDate, Params.ToDate, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetSecurityIncomeCsvString(model);
            //Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=SecurityIncome.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetSecurityIncomeCsvString(List<DistributionDividendDetailModel> securityIncomeModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Code, Security Name, Transaction Type, Currency, Ex Dividend Date, Payment Date, Income Amount");
            foreach (var price in securityIncomeModel)
            {
                var code = price.SecurityCode;
                var name = price.SecurityName;
                var transactionType = price.TransactionType;
                var currency = price.Currency;
                var exDividendDate = price.ExDividendDate;
                var paymentDate = price.PaymentDate;
                var incomeAmount = price.IncomeAmount;
                csv.Append(CsvUtility.MakeValueCsvFriendly(code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.Append(CsvUtility.MakeValueCsvFriendly(transactionType));
                csv.Append(CsvUtility.MakeValueCsvFriendly(currency));
                csv.Append(CsvUtility.MakeValueCsvFriendly(exDividendDate));
                csv.Append(CsvUtility.MakeValueCsvFriendly(paymentDate));
                csv.Append(CsvUtility.MakeValueCsvFriendly(incomeAmount));
                csv.AppendLine();
            }

            return csv.ToString();
        }

        public ActionResult CreateIncome()
        {
            var transactionTypeList = ListsMaintenance.GetTransactionTypeList();
            ViewData["transactionTypeList"] = transactionTypeList;

            var currencylist = ListsMaintenance.GetCurrencyList();
            ViewData["currencylist"] = currencylist;

            var incomeTypeList = ListsMaintenance.GetIncomeTypeList();
            ViewData["incomeTypeList"] = incomeTypeList;

            return PartialView("_CreateSecurityIncome");
        }

        [HttpPost]
        public ActionResult CreateSecurityIncome(DistributionDividendDetailModel model)
        {
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }

        public ActionResult GetSecurityIncome(int id)
        {
            var transactionTypeList = ListsMaintenance.GetTransactionTypeList();
            ViewData["transactionTypeList"] = transactionTypeList;

            var currencylist = ListsMaintenance.GetCurrencyList();
            ViewData["currencylist"] = currencylist;

            var incomeTypeList = ListsMaintenance.GetIncomeTypeList();
            ViewData["incomeTypeList"] = incomeTypeList;

            var model = new SecuritiesController().GetSecurityIncome(id);

            var securityList = ListsMaintenance.GetFilteredSecurityList(model.SecurityCode, model.SecurityId);
            ViewData["securityList"] = securityList;

            return PartialView("_EditSecurityIncome", model);
        }
        public ActionResult SecuritiesIncome()
        {
            return View("SecuritiesIncome");
        }

        [HttpPost]
        public ActionResult EditSecurityIncome(DistributionDividendDetailModel model)
        {
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteSecurityIncome(DistributionDividendDetailModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }

        #endregion

        #region OptionSecurities

        public ActionResult OptionSecurities()
        {
            List<SelectListItem> DisplayStatusList = new List<SelectListItem>();
            DisplayStatusList.Add(new SelectListItem { Text = "Non Matured/Terminated", Value = "1", Selected = true });
            DisplayStatusList.Add(new SelectListItem { Text = "All", Value = "0" });
            ViewData["StatusList"] = DisplayStatusList;
            return View("SecuritiesOption");
        }

        public ActionResult GetOptionSecurityPageData(JQueryDataTableParamModel param, string requestType, bool DisplayStatus, int? securityId)
        {
            var responseViewModle = OptionSecuritiesPageData(param, requestType, DisplayStatus, securityId);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<SecurityModel> OptionSecuritiesPageData(JQueryDataTableParamModel param, string requestType, bool DisplayStatus, int? securityId, bool isDownlaod = false)
        {
            var modelView = new SecurityModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                RequestType = requestType,
                DisplayStatus = DisplayStatus,
                Id = securityId ?? 0
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<SecurityModel>(json);
        }

        public void DownloadOptionSecuritiesCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));
            var responseViewModle = OptionSecuritiesPageData(Params, Params.RequestType, Params.IsAll, Params.Id, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetSecurityCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=OptionSecurities.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        #endregion

        #endregion

        #region Product

        #region Product-Securities
        public ActionResult ProductSecuritiesDetail()
        {
            return View("ProductSecuritiesDetail");
        }

        public ActionResult GetProductSecListDetailPage()
        {
            return PartialView("_ProductSecuritiesDetailList");
        }

        public ActionResult GetProductSecListDetailPageData(JQueryDataTableParamModel param, int? productVersionId, int? productId)
        {
            var responseViewModle = ProductSecDetailPageData(param, productVersionId, productId);
            var model = responseViewModle.ModelList;

            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public void DownloadProductSecDetailCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));
            var responseViewModle = ProductSecDetailPageData(Params, Params.Id_1, Params.Id);
            var model = responseViewModle.ModelList;
            string facsCsv = GetProductSecDetailCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ProductSecuritiesDetail.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetProductSecDetailCsvString(List<ProductSecuritiesModel> modelsDetailListModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Code, Name, Product Weight");
            foreach (var models in modelsDetailListModel)
            {
                var securityCode = models.SecurityCode;
                var securityName = models.SecurityName;
                var productWeight = string.Format("{0:N2}%", models.Allocation ?? 0);
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityCode));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityName));
                csv.Append(CsvUtility.MakeValueCsvFriendly(productWeight));
                csv.AppendLine();
            }

            var modelDetailListDetailModel = modelsDetailListModel.FirstOrDefault();
            if (modelDetailListDetailModel == null) return csv.ToString();
            csv.AppendLine(string.Format(",,Total Allocation:{0:N2}%", modelDetailListDetailModel.TotalAllocation));
            return csv.ToString();
        }

        private ResponseModel<ProductSecuritiesModel> ProductSecDetailPageData(JQueryDataTableParamModel param, int? productVersionId, int? productId)
        {
            var modelView = new ProductSecuritiesModel
            {
                ProductId = productId ?? 0,
                ProductVersionId = productVersionId ?? 0,
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ProductSecuritiesModel>(json);
        }
        #endregion

        #region Product-Details
        public ActionResult ProductDetails()
        {
            List<SelectListItem> DisplayStatusList = new List<SelectListItem>();
            DisplayStatusList.Add(new SelectListItem { Text = "Active/Pending", Value = "1", Selected = true });
            DisplayStatusList.Add(new SelectListItem { Text = "Archived", Value = "2" });
            DisplayStatusList.Add(new SelectListItem { Text = "All", Value = "0" });

            ViewData["StatusList"] = DisplayStatusList;
            return View("ProductDetails");
        }

        public ActionResult GetProduct(int productVersionId)
        {
            var productTypelist = ListsMaintenance.GetProductTypeList();
            ViewData["productTypelist"] = productTypelist;

            var indexTypelist = ListsMaintenance.GetIndexTypeList();
            ViewData["indexTypelist"] = indexTypelist;

            var institutionlist = ListsMaintenance.GetInstitutionList();
            ViewData["institutionlist"] = institutionlist;

            var marketList = ListsMaintenance.GetMarketList();
            ViewData["marketlist"] = marketList;

            var currencyList = ListsMaintenance.GetCurrencyList();
            ViewData["currencylist"] = currencyList;

            var subAssetClassList = ListsMaintenance.GetSubAssetClassList();
            ViewData["subAssetClassList"] = subAssetClassList;

            var regionList = ListsMaintenance.GetRegionList();
            ViewData["regionList"] = regionList;

            var securityStatusList = ListsMaintenance.GetSecurityStatusList();
            ViewData["securityStatusList"] = securityStatusList;

            var pricingSourceList = ListsMaintenance.GetPricingSourceList();
            ViewData["pricingSourceList"] = pricingSourceList;

            var assetClassList = ListsMaintenance.GetAssetClassList();
            ViewData["assetClassList"] = assetClassList;

            var productBrokerList = ListsMaintenance.GetProductBrokerList();
            ViewData["productBrokerList"] = productBrokerList;


            var productSecurityList = ListsMaintenance.GetProductSecurityList();
            ViewData["productSecurityList"] = productSecurityList;

            var primaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["primaryBenchmarkList"] = primaryBenchmarkList;

            var secondaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["secondaryBenchmarkList"] = secondaryBenchmarkList;

            var priceTypeList = ListsMaintenance.GetPriceTypeList();
            ViewData["priceTypeList"] = priceTypeList;

            var model = new ProductController().GetProductByProductVersionFields(productVersionId);

            ViewData["productBrokerIds"] = model.ProductBrokerIds;
            //ViewData["securityListIds"] = model.SecurityListIds;
            model.BaseProductVersionID = productVersionId;
            return PartialView("_EditProduct", model);
        }

        public ActionResult GetProductPageData(JQueryDataTableParamModel param, int DisplayStatus)
        {
            var responseViewModle = ProductPageData(param, DisplayStatus);
            var model = responseViewModle.ModelList;

            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        private ResponseModel<ProductModel> ProductPageData(JQueryDataTableParamModel param, int DisplayStatus, bool isDownlaod = false)
        {
            var modelView = new ProductModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                DisplayStatus = DisplayStatus
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ProductModel>(json);
        }

        [HttpPost]
        public ActionResult GetAssociatedSecData(string productId, string productVersionID)
        {
            var responseViewModle = new ProductController().GetAssociatedSecData(int.Parse(productId), int.Parse(productVersionID));
            var model = responseViewModle.ModelList;

            return new CustomJsonResult()
            {
                Data = new
                {
                    data = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public void DownloadProductCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = ProductPageData(Params, Params.Id_1??0, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetProductCsvString(model);

            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=Product.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        public void DownloadProductAllocationCsv(int productId)
        {
            StringBuilder facsCsv = GetProductAllocationCsvString(productId, 1);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ProductAllocation.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        private StringBuilder GetProductAllocationCsvString(int productID, int sortType)
        {
            InvestContext context = new InvestContext(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            //int productID = model.ProductId ?? 0;
            //int productID = 1001;
            Dictionary<int, SecurityModel> securitiesDict = new Dictionary<int, SecurityModel>();

            var product = context.Products.Where(p => p.ProductID == productID).Where(w => w.IsDeleted == false).FirstOrDefault();

            if (product.ProductVersions.Where(w => w.IsDeleted == false).ToList().Count > 0)
            {
                var accociation = product.ProductVersions.ElementAt(product.ProductVersions.Count - 1)
                    .ProductAssociations.Where(w => w.IsDeleted == false).ToList();
                //var accociation = product.ProductVersions.Where(w => w.IsDeleted == false).LastOrDefault();

                foreach (var item in accociation)
                {
                    if (item.SecurityId.HasValue && item.Allocation != null)
                    {
                        if (securitiesDict.ContainsKey(item.SecurityId.Value))
                            securitiesDict[item.SecurityId.Value].Allocation += item.Allocation;
                        else
                        {
                            SecurityModel security = new SecurityModel
                            {
                                Id = item.Security.Id,
                                Code = item.Security.Code,
                                Name = item.Security.Name,
                                Allocation = item.Allocation,
                                AssetClass = item.Security.SecurityAssetClasses.Select(sel => sel.AssetClass.Class).ToList(),
                                AssetClassIds = item.Security.SecurityAssetClasses.Select(sel => sel.AssetClassId).ToList(),

                                GICSId = item.Security.GICSId,
                                GICSTypeId = item.Security.GICSTypeId,
                                GICSType = item.Security.GICSType != null ? item.Security.GICSType.Type : string.Empty,
                                SecurityType = item.Security.SecurityType != null ? item.Security.SecurityType.Type : string.Empty,

                                SecurityReturn = (item.Security.SecurityReturn) == null ? null : new SecurityReturnModel
                                {
                                    Id = item.Security.SecurityReturn.SecurityId,
                                    MonthReturn = item.Security.SecurityReturn.MonthReturn,
                                    YearReturn = item.Security.SecurityReturn.YearReturn
                                }
                            };

                            securitiesDict.Add(item.SecurityId.Value, security);
                        }
                    }

                    if (item.SecurityListId.HasValue)
                    {
                        decimal alloc = item.Allocation / 100 ?? 0;

                        var securityListDetails = item.SecurityList.SecurityListDetails.Where(w => !w.IsDeleted).GroupBy(n => new
                        {
                            n.SecurityListId,
                            n.SecurityId
                        }).Select(g => g.OrderByDescending(t => t.StatusDate).ThenBy(t => t.Id).FirstOrDefault());

                        foreach (var s in securityListDetails)
                        {
                            if (securitiesDict.ContainsKey(s.SecurityId.Value))
                                securitiesDict[s.SecurityId.Value].Allocation += s.Allocation;
                            else
                            {
                                SecurityModel security = new SecurityModel
                                {
                                    Allocation = s.Allocation,

                                    Id = s.Security.Id,
                                    Code = s.Security.Code,
                                    Name = s.Security.Name,
                                    AssetClass = s.Security.SecurityAssetClasses.Select(sel => sel.AssetClass.Class).ToList(),
                                    AssetClassIds = s.Security.SecurityAssetClasses.Select(sel => sel.AssetClassId).ToList(),

                                    GICSId = s.Security.GICSId,
                                    GICSTypeId = s.Security.GICSTypeId,
                                    GICSType = s.Security.GICSType != null ? s.Security.GICSType.Type : string.Empty,
                                    SecurityType = s.Security.SecurityType != null ? s.Security.SecurityType.Type : string.Empty,

                                    SecurityReturn = (s.Security.SecurityReturn) == null ? null : new SecurityReturnModel
                                    {
                                        Id = s.Security.SecurityReturn.SecurityId,
                                        MonthReturn = s.Security.SecurityReturn.MonthReturn,
                                        YearReturn = s.Security.SecurityReturn.YearReturn
                                    }
                                };
                                securitiesDict.Add(s.SecurityId.Value, security);
                            }
                        }
                    }
                }
            }

            var csv = new StringBuilder();

            csv.AppendLine("Code, Name, Asset class or GICS, Security type, Weight, Month return, Year return");

            foreach (var item in securitiesDict.OrderBy(o => o.Value.Code))
            {
                var code = item.Value.Code;
                var name = item.Value.Name;
                var AssetClass = string.Join("; ", item.Value.AssetClass.ToArray());
                var gics = item.Value.GICS;
                var securityType = item.Value.SecurityType;
                var weight = item.Value.Allocation;
                decimal monthYeturn = item.Value.SecurityReturn != null ? item.Value.SecurityReturn.MonthReturn ?? 0 : 0.00M;
                decimal yearYeturn = item.Value.SecurityReturn != null ? item.Value.SecurityReturn.YearReturn ?? 0 : 0.00M;

                csv.Append(CsvUtility.MakeValueCsvFriendly(code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.Append(CsvUtility.MakeValueCsvFriendly(AssetClass));
                csv.Append(CsvUtility.MakeValueCsvFriendly(gics));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityType));
                csv.Append(CsvUtility.MakeValueCsvFriendly(weight));
                csv.Append(CsvUtility.MakeValueCsvFriendly(monthYeturn));
                csv.Append(CsvUtility.MakeValueCsvFriendly(yearYeturn));
                csv.AppendLine();
            }

            return csv;
        }

        private string GetProductCsvString(List<ProductModel> productModel)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Code, Name, Product Type, Version, Status");
            foreach (var product in productModel)
            {
                var code = product.Code;
                var name = product.Name;
                var productType = product.ProductType;
                var version = product.VersionDetail.CombineVersion;
                var status = product.VersionDetail.ProductVersionStatus;
                csv.Append(CsvUtility.MakeValueCsvFriendly(code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.Append(CsvUtility.MakeValueCsvFriendly(productType));
                csv.Append(CsvUtility.MakeValueCsvFriendly(version));
                csv.Append(CsvUtility.MakeValueCsvFriendly(status));
                csv.AppendLine();
            }

            return csv.ToString();
        }

        [HttpPost]
        public ActionResult EditProduct(ProductModel model)
        {
            var result = new SecuritiesController().Update(model);
            BackgroundJob.Enqueue<PriceUtility>(x => x.GeneratePricesForModel(model.ProductID));
            return Json(result.Content);
        }

        public ActionResult CreateProduct()
        {
            var productTypelist = ListsMaintenance.GetProductTypeList();
            ViewData["productTypelist"] = productTypelist;

            var indexTypelist = ListsMaintenance.GetIndexTypeList();
            ViewData["indexTypelist"] = indexTypelist;

            var institutionlist = ListsMaintenance.GetInstitutionList();
            ViewData["institutionlist"] = institutionlist;

            var marketList = ListsMaintenance.GetMarketList();
            ViewData["marketlist"] = marketList;

            var currencyList = ListsMaintenance.GetCurrencyList();
            ViewData["currencylist"] = currencyList;

            var subAssetClassList = ListsMaintenance.GetSubAssetClassList();
            ViewData["subAssetClassList"] = subAssetClassList;

            var regionList = ListsMaintenance.GetRegionList();
            ViewData["regionList"] = regionList;

            var securityStatusList = ListsMaintenance.GetSecurityStatusList();
            ViewData["securityStatusList"] = securityStatusList;

            var pricingSourceList = ListsMaintenance.GetPricingSourceList();
            ViewData["pricingSourceList"] = pricingSourceList;

            var assetClassList = ListsMaintenance.GetAssetClassList();
            ViewData["assetClassList"] = assetClassList;

            var productBrokerList = ListsMaintenance.GetProductBrokerList();
            ViewData["productBrokerList"] = productBrokerList;

            var productSecurityList = ListsMaintenance.GetProductSecurityList();
            ViewData["productSecurityList"] = productSecurityList;

            var productList = ListsMaintenance.GetProductListWithVersions();
            ViewData["productList"] = productList;

            var primaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["primaryBenchmarkList"] = primaryBenchmarkList;

            var secondaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["secondaryBenchmarkList"] = secondaryBenchmarkList;

            var priceTypeList = ListsMaintenance.GetPriceTypeList();
            ViewData["priceTypeList"] = priceTypeList;

            return PartialView("_CreateProduct");
        }

        [HttpPost]
        public ActionResult CreateProduct(ProductModel model)
        {
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteProduct(ProductModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteProductVersion(ProductModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }

        public ActionResult GetAssetClassSecuritiesByIdsOnAssetClassChange(int? assetClassId, string securityIds, string secIds)
        {
            if (assetClassId != null)
            {
                var responseViewModle = new SecuritiesController().GetAssetClassSecuritiesByIds(assetClassId.Value, securityIds, secIds);
                var model = responseViewModle.ModelList;
                return new CustomJsonResult()
                {
                    Data = new
                    {
                        data = model
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            else { return null; }
        }

        [HttpGet]
        public ActionResult GetProductVersionDetail(int productVersionID)
        {
            var result = new Services.ProductController().GetProductByProductVersionFields(productVersionID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult MakeItActiveProductVersion(ProductModel model)
        {
            var result = new Services.ModelsController().Update(model);
            return Json(result.Content);
        }

        #endregion

        #region Product-RateList
        public ActionResult ProductRates()
        {
            return View("ProductRates");
        }
        public ActionResult GetRate(int id)
        {
            var productList = ListsMaintenance.GetProductList();
            ViewData["productList"] = productList;

            var rateTypeList = ListsMaintenance.GetRateTypeList();
            ViewData["rateTypeList"] = rateTypeList;

            var model = new ProductController().GetRate(id);
            return PartialView("_EditRateList", model);
        }

        public ActionResult GetRatePageData(JQueryDataTableParamModel param)
        {
            var responseViewModle = RatePageData(param);
            var model = responseViewModle.ModelList;

            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },

                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<RateListModel> RatePageData(JQueryDataTableParamModel param, bool isDownlaod = false)
        {
            var modelView = new RateListModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod
            };
            var json = Execute(Option.Get, modelView);
            return GetResponseModel<RateListModel>(json);
        }

        public void DownloadRateCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));
            var responseViewModle = RatePageData(Params, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetRateCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=Rate.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        private string GetRateCsvString(List<RateListModel> rateModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Product Id, Product Code, Rate List Type, Date, Min Deposit, Max Deposit, At Call, 30 Days, 60 Days, 90 Days, 120 Days, 150 Days, 180 Days, 270 Days, 1 Year, 2 Year, 3 Year, 4 Year, 5 Year");
            foreach (var rate in rateModel)
            {
                var productId = rate.ProductId;
                var productCode = rate.ProductCode;
                var rateListType = rate.RateListType;
                var date = rate.Date;
                var minDeposit = rate.MinDeposit == null ? "" : string.Format("{0:C2}", rate.MinDeposit);
                var maxDeposit = rate.MaxDeposit == null ? "" : string.Format("{0:C2}", rate.MaxDeposit);
                var atCall = rate.AtCall == null ? "" : string.Format("{0:N4}%", rate.AtCall);
                var days30 = rate.days30 == null ? "" : string.Format("{0:N4}%", rate.days30);
                var days60 = rate.days60 == null ? "" : string.Format("{0:N4}%", rate.days60);
                var days90 = rate.days90 == null ? "" : string.Format("{0:N4}%", rate.days90);
                var days120 = rate.days120 == null ? "" : string.Format("{0:N4}%", rate.days120);
                var days150 = rate.days150 == null ? "" : string.Format("{0:N4}%", rate.days150);
                var days180 = rate.days180 == null ? "" : string.Format("{0:N4}%", rate.days180);
                var days270 = rate.days270 == null ? "" : string.Format("{0:N4}%", rate.days270);
                var oneYear = rate.OneYear == null ? "" : string.Format("{0:N4}%", rate.OneYear);
                var twoYear = rate.TwoYear == null ? "" : string.Format("{0:N4}%", rate.TwoYear);
                var threeYear = rate.ThreeYear == null ? "" : string.Format("{0:N4}%", rate.ThreeYear);
                var fourYear = rate.FourYear == null ? "" : string.Format("{0:N4}%", rate.FourYear);
                var fiveYear = rate.FiveYear == null ? "" : string.Format("{0:N4}%", rate.FiveYear);
                csv.Append(CsvUtility.MakeValueCsvFriendly(productId));
                csv.Append(CsvUtility.MakeValueCsvFriendly(productCode));
                csv.Append(CsvUtility.MakeValueCsvFriendly(rateListType));
                csv.Append(CsvUtility.MakeValueCsvFriendly(date));
                csv.Append(CsvUtility.MakeValueCsvFriendly(minDeposit));
                csv.Append(CsvUtility.MakeValueCsvFriendly(maxDeposit));
                csv.Append(CsvUtility.MakeValueCsvFriendly(atCall));
                csv.Append(CsvUtility.MakeValueCsvFriendly(days30));
                csv.Append(CsvUtility.MakeValueCsvFriendly(days60));
                csv.Append(CsvUtility.MakeValueCsvFriendly(days90));
                csv.Append(CsvUtility.MakeValueCsvFriendly(days120));
                csv.Append(CsvUtility.MakeValueCsvFriendly(days150));
                csv.Append(CsvUtility.MakeValueCsvFriendly(days180));
                csv.Append(CsvUtility.MakeValueCsvFriendly(days270));
                csv.Append(CsvUtility.MakeValueCsvFriendly(oneYear));
                csv.Append(CsvUtility.MakeValueCsvFriendly(twoYear));
                csv.Append(CsvUtility.MakeValueCsvFriendly(threeYear));
                csv.Append(CsvUtility.MakeValueCsvFriendly(fourYear));
                csv.Append(CsvUtility.MakeValueCsvFriendly(fiveYear));
                csv.AppendLine();
            }

            return csv.ToString();
        }

        [HttpPost]
        public ActionResult EditRate(RateListModel model)
        {
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }

        public ActionResult CreateRate()
        {
            var productList = ListsMaintenance.GetProductList();
            ViewData["productList"] = productList;
            var rateTypeList = ListsMaintenance.GetRateTypeList();
            ViewData["rateTypeList"] = rateTypeList;
            return PartialView("_CreateRateList");
        }

        [HttpPost]
        public ActionResult CreateRate(RateListModel model)
        {
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteRate(RateListModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }
        #endregion

        #region Product-SecurityList
        public ActionResult ProductSecurity()
        {
            return View("ProductSecurity");
        }

        public ActionResult GetProductSecurity(int id)
        {
            var securityListStatusList = ListsMaintenance.GetSecurityListStatusList();
            ViewData["securityListStatusList"] = securityListStatusList;
            var model = new ProductController().GetProductSecurity(id);
            return PartialView("_EditProductSecurity", model);
        }

        public ActionResult GetProductSecurityPageData(JQueryDataTableParamModel param)
        {
            var responseViewModle = ProductSecurityPageData(param);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<SecurityListModel> ProductSecurityPageData(JQueryDataTableParamModel param, bool isDownlaod = false)
        {
            var modelView = new SecurityListModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<SecurityListModel>(json);
        }

        public void DownloadProductSecurityCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));
            var responseViewModle = ProductSecurityPageData(Params, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetProductSecurityCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ProductSecurityList.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetProductSecurityCsvString(List<SecurityListModel> securityModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Name, Security List Status");
            foreach (var security in securityModel)
            {
                var name = security.Name;
                var securityListStatus = security.SecurityListStatus;
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityListStatus));
                csv.AppendLine();
            }
            return csv.ToString();
        }

        [HttpPost]
        public ActionResult EditProductSecurity(SecurityListModel model)
        {
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }

        public ActionResult CreateProductSecurity()
        {
            var securityListStatusList = ListsMaintenance.GetSecurityListStatusList();
            ViewData["securityListStatusList"] = securityListStatusList;
            return PartialView("_CreateProductSecurity");
        }

        [HttpPost]
        public ActionResult CreateProductSecurity(SecurityListModel model)
        {
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteProductSecurity(SecurityListModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }
        #endregion

        #region Product-SecurityListDetail

        public ActionResult ProductSecurityDetail()
        {
            return View("ProductSecurityDetail");
        }

        public ActionResult GetProductSecurityDetailPage(int? sListId)
        {
            var productSecurityList = ListsMaintenance.GetProductSecurityList();
            if (sListId != null && sListId > 0)
            {
                var item = productSecurityList.Find(f => f.Value == sListId.ToString());
                if (item != null)
                    item.Selected = true;
            }
            ViewData["productSecurityList"] = productSecurityList;
            return PartialView("_ProductSecurityDetailList");
        }
        public ActionResult GetProductSecurityDetail(int id)
        {
            var productSecurityList = ListsMaintenance.GetProductSecurityList();
            ViewData["productSecurityList"] = productSecurityList;

            var securityListStatusList = ListsMaintenance.GetSecurityListStatusList();
            ViewData["securityListStatusList"] = securityListStatusList;

            var model = new ProductController().GetProductSecurityDetail(id);
            var securityList = ListsMaintenance.GetFilteredSecurityList(model.SecurityCode, model.SecurityId ?? 0);
            ViewData["securityList"] = securityList;

            return PartialView("_EditProductSecurityDetail", model);
        }

        public ActionResult GetProductSecurityListForAllocationOrStatus(int id)
        {
            var productSecurityList = ListsMaintenance.GetProductSecurityList();
            ViewData["productSecurityList"] = productSecurityList;

            var securityListStatusList = ListsMaintenance.GetSecurityListStatusList();
            ViewData["securityListStatusList"] = securityListStatusList;

            var model = new ProductController().GetProductSecurityDetail(id);
            ViewData["securityIds"] = model.SecurityId;

            var securityList = ListsMaintenance.GetFilteredSecurityList(model.SecurityCode, model.SecurityId ?? 0);
            ViewData["securityList"] = securityList;

            return PartialView("_CreateProductSecurityDetail", model);
        }

        public ActionResult GetProductSecurityDetailPageData(JQueryDataTableParamModel param, int? sListId, DateTime? fromDate, DateTime? toDate, string requestFrom, bool isAll = false, decimal? pfWeight = null)
        {

            if (sListId == 0 && toDate == null)
            {
                return null;
            }
            var responseViewModle = ProductSecurityDetailPageData(param, sListId, toDate, isAll, requestFrom, pfWeight);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },

                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<SecurityListDetailModel> ProductSecurityDetailPageData(JQueryDataTableParamModel param, int? sListId, DateTime? toDate, bool isAll, string requestFrom, decimal? pfWeight = null, bool isDownlaod = false)
        {
            var modelView = new SecurityListDetailModel
            {
                SecurityListId = sListId ?? 0,
                ToDate = toDate,
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                MDPortfolioWeight = pfWeight,
                RequestFrom = requestFrom,
                IsAll = isAll
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<SecurityListDetailModel>(json);
        }

        public void DownloadProductSecurityDetailCsvFromModel()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));
            var responseViewModle = ProductSecurityDetailPageData(Params, Params.Id_2, Params.ToDate, Params.IsAll, "ModelDetailListDetail", Params.PFWeight, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetProductSecurityDetailCsvString(model, Params.IsAll);

            JQueryDataTableParamModel params2 = new JQueryDataTableParamModel();
            params2.Id = 4;
            params2.ToDate = DateTime.Now;
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=SecurityListDetails.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        public void DownloadProductSecurityDetailCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));
            var responseViewModle = ProductSecurityDetailPageData(Params, Params.Id, Params.ToDate, Params.IsAll, "ProductSecurityDetail", Params.PFWeight, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetProductSecurityDetailCsvString(model, Params.IsAll);

            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ProductSecurityListDetails.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        private string GetProductSecurityDetailCsvString(List<SecurityListDetailModel> securityModel, bool isAll = false)
        {
            var csv = new StringBuilder();
            var securityListDetailModel = securityModel.FirstOrDefault();
            if (securityListDetailModel != null)
            {
                csv.AppendLine(string.Format("Security List Name:,{0}", securityListDetailModel.SecurityListName));
            }

            csv.AppendLine("Security Code, Status Code, Security Name, Security Type, Status Date, Allocation, Portfolio Weight");

            foreach (var security in securityModel)
            {
                var securityCode = security.SecurityCode;
                var statusCode = security.StatusCode;
                var securityName = security.SecurityName;
                var securityType = security.SecurityType;
                var statusDate = security.StatusDate;
                var allocation = security.Allocation == null ? "" : string.Format("{0:N2}%", security.Allocation);
                var portfolioWeight = security.PortfolioWeight == null ? "" : string.Format("{0:N2}%", security.PortfolioWeight);

                csv.Append(CsvUtility.MakeValueCsvFriendly(securityCode));
                csv.Append(CsvUtility.MakeValueCsvFriendly(statusCode));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityName));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityType));
                csv.Append(CsvUtility.MakeValueCsvFriendly(statusDate));
                csv.Append(CsvUtility.MakeValueCsvFriendly(allocation));
                csv.Append(CsvUtility.MakeValueCsvFriendly(portfolioWeight));
                csv.AppendLine();
            }
            if (!isAll)
            {
                if (securityListDetailModel != null)
                {
                    csv.AppendLine(string.Format(",,,,Total Allocation:,{0:N2}%", securityListDetailModel.TotalAllocation));
                    csv.AppendLine(string.Format(",,,,,Total Portfolio Weight:,{0:N2}%", securityListDetailModel.TotalPortfolioWeight));
                }
            }
            return csv.ToString();
        }

        [HttpPost]
        public ActionResult EditProductSecurityDetail(SecurityListDetailModel model)
        {
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }

        public ActionResult CreateProductSecurityDetail()
        {
            var productSecurityList = ListsMaintenance.GetProductSecurityList();
            ViewData["productSecurityList"] = productSecurityList;

            ViewData["securityList"] = new List<SelectListItem>();

            var securityListStatusList = ListsMaintenance.GetSecurityListStatusList();
            ViewData["securityListStatusList"] = securityListStatusList;

            return PartialView("_CreateProductSecurityDetail");
        }

        [HttpPost]
        public ActionResult CreateProductSecurityDetail(SecurityListDetailModel model)
        {
            var result = new SecuritiesController().Create(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteProductSecurityDetail(SecurityListDetailModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }


        public ActionResult GetSecurityListDetailPage()
        {
            return PartialView("_SecurityListDetail");
        }


        [HttpPost]
        public ActionResult FileUploadSecurityListDetailHandler()
        {
            ActionResult result = null;
            try
            {
                foreach (string file in Request.Files)
                {
                    var upload = Request.Files[file];
                    if (upload != null)
                    {
                        var fileName = System.IO.Path.GetFileName(upload.FileName);
                        var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                        upload.SaveAs(path);

                        string conwork = ConfigurationManager.ConnectionStrings["WFConnection"].ConnectionString;
                        using (var connect = new SqlConnection(conwork))
                        {
                            connect.Open();
                            var conv = new Convert.ConvertPersian();
                            var res = new Convert.ResultConvert();
                            conv.RunSecurityList(path, res, connect);
                            result = PartialView("_FileUploadProduct", res);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //return Json(new { Message = "Error in saving file" });
                var m = new HandleErrorInfo(ex, "Investment", "FileUploadSecurityListDetailHandler");
                result = View("Error", m);
            }
            return result;
        }

        public ActionResult DownLoadTemplateSecurityListDetail()
        {
            ActionResult result = null;
            var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), "SecurityListDetailTemplate.csv");
            if (System.IO.File.Exists(path))
            {
                byte[] contents = System.IO.File.ReadAllBytes(path);
                result = File(contents, "text/csv", "SecurityListDetailTemplate.csv");
            }
            else
            {
                Exception ex = new Exception("File SecurityListDetailTemplate.csv not found");
                HandleErrorInfo m = new HandleErrorInfo(ex, "Investment", "DownLoadTemplateSecurityListDetail");
                result = View("Error", m);
            }
            return result;
        }
        #endregion

        #region Product-Price
        public ActionResult ProductPrice()
        {
            return View("ProductPrice");
        }

        [HttpPost]
        public ActionResult DeleteProductPrice(ProductPriceModel model)
        {
            var result = new SecuritiesController().Delete(model);
            return Json(result.Content);
        }
        public ActionResult GetProductPrice(int id)
        {
            var productList = ListsMaintenance.GetProductList(true);
            ViewData["productList"] = productList;

            var model = new ProductController().GetProductPrice(id);
            return PartialView("_EditProductPrice", model);
        }
        [HttpPost]
        public ActionResult CreateProductPrice(ProductPriceModel model)
        {
            var result = new ProductController().Create(model);
            return Json(result.Content);
        }

        public ActionResult CreateProductPrice()
        {
            var productList = ListsMaintenance.GetProductList(true);
            ViewData["productList"] = productList;
            return PartialView("_CreateProductPrice");
        }

        [HttpPost]
        public ActionResult CalculateProductPrice(ProductPriceModel model)
        {
            int status = ProductPriceTest(model);
            var result = new SecuritiesController().CreatePricies(model, true);
            return Json(result.Content);
        }

        public ActionResult CalculateProductPrice()
        {
            var productList = ListsMaintenance.GetNonIndexProductList();
            ViewData["productList"] = productList;
            return PartialView("_CalculateProductPrice");
        }
        [HttpPost]
        public ActionResult EditProductPrice(ProductPriceModel model)
        {
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }
        public void DownloadProductPriceCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = ProductPriceData(Params, Params.Id, Params.FromDate, Params.ToDate, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetProductPriceCsvString(model);

            //Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ProductPrice.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        public void DownloadModelPriceCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = ModelPriceData(Params, Params.Id, Params.FromDate, Params.ToDate, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GeModelPriceCsvString(model);
            //Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ModelPrice.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        private string GeModelPriceCsvString(List<ModelPriceModel> securityPriceModel)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Model Code, Model Name, Date, Capital Price, Income Price, TR Price");
            foreach (var price in securityPriceModel)
            {
                var modelCode = price.ModelCode;
                var modelName = price.ModelName;
                var date = price.Date;
                var capitalPrice = price.CapitalPrice;
                var incomePrice = price.IncomePrice;
                var trPrice = price.TRPrice;
                csv.Append(CsvUtility.MakeValueCsvFriendly(modelCode));
                csv.Append(CsvUtility.MakeValueCsvFriendly(modelName));
                csv.Append(CsvUtility.MakeValueCsvFriendly(date));
                csv.Append(CsvUtility.MakeValueCsvFriendly(capitalPrice));
                csv.Append(CsvUtility.MakeValueCsvFriendly(incomePrice));
                csv.Append(CsvUtility.MakeValueCsvFriendly(trPrice));
                csv.AppendLine();
            }
            return csv.ToString();
        }

        private string GetProductPriceCsvString(List<ProductPriceModel> securityPriceModel)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Product Code, Product Name, Date, Capital Price, Income Price, TR Price");

            foreach (var price in securityPriceModel)
            {
                var productCode = price.ProductCode;
                var productName = price.ProductName;
                var date = price.Date;
                var capitalPrice = price.CapitalPrice;
                var incomePrice = price.IncomePrice;
                var trPrice = price.TRPrice;
                csv.Append(CsvUtility.MakeValueCsvFriendly(productCode));
                csv.Append(CsvUtility.MakeValueCsvFriendly(productName));
                csv.Append(CsvUtility.MakeValueCsvFriendly(date));
                csv.Append(CsvUtility.MakeValueCsvFriendly(capitalPrice));
                csv.Append(CsvUtility.MakeValueCsvFriendly(incomePrice));
                csv.Append(CsvUtility.MakeValueCsvFriendly(trPrice));
                csv.AppendLine();
            }

            return csv.ToString();
        }
        private ResponseModel<ProductPriceModel> ProductPriceData(JQueryDataTableParamModel param, int? productId, DateTime? fromDate, DateTime? toDate, bool isDownlaod = false)
        {
            var modelView = new ProductPriceModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                ProductId = productId ?? 0,
                FromDate = fromDate,
                ToDate = toDate
            };
            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ProductPriceModel>(json);
        }

        public ActionResult GetProductPriceDetailByFilter(JQueryDataTableParamModel param, int? productId, DateTime? fromDate, DateTime? toDate)
        {
            if (productId == 0 && fromDate == null && toDate == null)
            {
                return null;
            }
            var responseViewModle = ProductPriceData(param, productId, fromDate, toDate);
            var model = responseViewModle.ModelList;

            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }
        public ActionResult GetProductPriceDetail()
        {
            var productList = ListsMaintenance.GetProductList(true);
            ViewData["productList"] = productList;

            return PartialView("_ProductPriceList");
        }


        [HttpPost]
        public ActionResult FileUploadProductValid()
        {
            ActionResult result = null;

            try
            {
                foreach (string file in Request.Files)
                {
                    var upload = Request.Files[file];
                    if (upload != null)
                    {
                        var fileName = System.IO.Path.GetFileName(upload.FileName);
                        var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                        upload.SaveAs(path);
                        string conwork = ConfigurationManager.ConnectionStrings["WFConnection"].ConnectionString;
                        using (var connect = new SqlConnection(conwork))
                        {
                            connect.Open();
                            var conv = new ConvertProduct();
                            //var res = new ResultConvert();
                            var newcode = conv.GetNewCodeProduct(path, connect, ',');
                            if (newcode != null && newcode.Any())
                            {
                                var model = new CodeListAndPathViewModel { ListCode = newcode, PathFile = path };
                                result = PartialView("_NewProductCodeModel", model);
                            }
                            else
                                return RedirectToAction("FileUploadProductHandler", "Investment", new { pathfile = path });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //return Json(new { Message = "Error in saving file" });
                var m = new HandleErrorInfo(ex, "Investment", "FileUploadProductValid");
                result = View("Error", m);
            }
            return result;
        }

        public ActionResult FileUploadProductHandler(string pathfile)
        {
            ActionResult result;

            try
            {
                string conwork = ConfigurationManager.ConnectionStrings["WFConnection"].ConnectionString;
                using (var connect = new SqlConnection(conwork))
                {
                    connect.Open();
                    var conv = new ConvertProduct();
                    var res = new ResultConvert();
                    conv.RunProductPrice(pathfile, res, connect, ',');
                    result = PartialView("_FileUploadProduct", res);
                }
            }
            catch (Exception ex)
            {
                //return Json(new { Message = "Error in saving file" });
                var m = new HandleErrorInfo(ex, "Investment", "FileUploadProductHandler");
                result = View("Error", m);
            }
            return result;
        }


        public ActionResult DownLoadTemplateProductPrice()
        {
            ActionResult result = null;
            var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), "ProductPriceTemplate.csv");
            if (System.IO.File.Exists(path))
            {
                byte[] contents = System.IO.File.ReadAllBytes(path);
                result = File(contents, "text/csv", "ProductPriceTemplate.csv");
            }
            else
            {
                Exception ex = new Exception("File ProductPriceTemplate.csv not found");
                HandleErrorInfo m = new HandleErrorInfo(ex, "Investment", "DownLoadTemplateProductPrice");
                result = View("Error", m);
            }
            return result;
        }

        #endregion

        #region ProductDashboard

        public ActionResult ProductDashboard()
        {
            List<SelectListItem> DisplayStatusList = new List<SelectListItem>();
            DisplayStatusList.Add(new SelectListItem { Text = "All", Value = "1", Selected = true });
            DisplayStatusList.Add(new SelectListItem { Text = "Non Index", Value = "0" });
            ViewData["StatusList"] = DisplayStatusList;
            return View("ProductDashboard");
        }

        private ResponseModel<ProductModel> GetProductForDashboard(bool status)
        {
            var modelView = new ProductModel
            {
                IsAll = status,
                FilterOption = FilterOption.GetDashboard,
            };
            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ProductModel>(json);
        }

        public ActionResult GetProductGridListData(string type, bool status)
        {
            var responseViewModle = GetProductForDashboard(status);
            var model = responseViewModle.ModelList;

            if (type.ToLower() == "grid")
            {
                return PartialView("_ViewProductGridModel", model);
            }
            else
            {
                return PartialView("_ViewProductListModel", model);
            }
        }

        public ActionResult GetProductDashboard(int productID)
        {
            var model = new ProductController().GetProduct(productID);
            var productDetails = ProductCalculationList(model.VersionDetail.ProductVersionID).Select(i => i.Value).ToList().OrderByDescending(i => i.allocation);

            bool hasOther = false;
            decimal otherAllocation = 0M;

            if (productDetails.Count() > 5)
            {
                hasOther = true;
                otherAllocation = System.Convert.ToDecimal(productDetails.Sum(s => s.allocation))
                    - System.Convert.ToDecimal(productDetails.Take(5).Sum(s => s.allocation));
            }

            ViewBag.HasOtherSecurities = hasOther;
            ViewBag.OtherAllocation = otherAllocation;
            ViewBag.TopAllocations = productDetails.Take(5);
            Stopwatch sw = new Stopwatch(); //old time 00:01:28
            InvestContext context =
                new InvestContext(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            var productVersion = context.ProductVersions.Where(w => w.ProductVersionID == model.VersionDetail.ProductVersionID).FirstOrDefault();

            if (productVersion == null)
            {
                throw new Exception("Product Version is null.");
            }

            //productVersion.ProductAssociations

            return PartialView("_ViewProduct", model);
        }

        #endregion

        #endregion

        #region Model

        #region ModelDashboard
        public ActionResult ModelsDashboard()
        {
            var modeltypeList = ListsMaintenance.GetModelTypeList();
            ViewData["modeltypeList"] = modeltypeList;

            return View("ModelsDashboard");
        }

        private ResponseModel<ModelsModel> GetModelDashboard()
        {
            JQueryDataTableParamModel param = new JQueryDataTableParamModel();
            param.sModelTypeSearch = new List<int?>() { 2, 3 };

            var modelView = new ModelsModel
            {
                FilterOption = FilterOption.ModelListByFields,
                TableParam = param
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ModelsModel>(json);
        }

        public ActionResult GetModelDashboard(int modelVersionID)
        {
            var model = new ModelsController().GetModelByModelVersionFields(modelVersionID);

            var allocations = new ModelAllocationWeights(modelVersionID).ModelAllocationDetails;
            var topAllocations = allocations.OrderByDescending(o => o.Weight).Take(5);

            ViewBag.TopAllocations = topAllocations;
            if (allocations.Count > 5)
            {
                ViewBag.OtherAllocationWeight = allocations.Select(s => s.Weight).Sum() - topAllocations.Select(s => s.Weight).Sum();
            }
            return PartialView("_ViewModel", model);
        }

        public ActionResult ViewModelDetail()
        {
            return View("ViewModelDetail");
        }

        public ActionResult GetViewModelDetail(JQueryDataTableParamModel param, string requestType, int modelId)
        {
            var responseViewModle = ViewModelDetailPageData(param, requestType, modelId, false);
            var model = responseViewModle.ModelList;

            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<ModelsModel> ViewModelDetailPageData(JQueryDataTableParamModel param, string requestType, int modelId, bool isDownlaod = false)
        {
            var modelView = new ModelsModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                RequestType = requestType,
                ModelID = modelId,
                IsDownload = isDownlaod
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ModelsModel>(json);
        }

        public ActionResult GetGridListData(string type, string modelTypes = "")
        {
            string[] result;
            string modelTypes2 = modelTypes;
            modelTypes2 = modelTypes2.Trim();
            result = modelTypes2.Split(' ');
            var responseViewModle = GetModelDashboard();
            var model = responseViewModle.ModelList;
            if (type.ToLower() == "grid")
            {
                return PartialView("_ViewGridModel", model);
            }
            else
            {
                return PartialView("_ViewListModel", model);
            }
        }


        #endregion

        #region ModelDetails

        public ActionResult ModelsDetails()
        {
            List<SelectListItem> DisplayStatusList = new List<SelectListItem>();
            DisplayStatusList.Add(new SelectListItem { Text = "Active/Pending", Value = "1", Selected = true });
            DisplayStatusList.Add(new SelectListItem { Text = "Archived", Value = "2" });
            DisplayStatusList.Add(new SelectListItem { Text = "All", Value = "0" });
            ViewData["StatusList"] = DisplayStatusList;
            return View("ModelsDetails");
        }

        public ActionResult GetModelsPageData(JQueryDataTableParamModel param, int DisplayStatus)
        {
            var responseViewModle = ModelsPageData(param, DisplayStatus);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<ModelsModel> ModelsPageData(JQueryDataTableParamModel param, int DisplayStatus, bool isDownlaod = false)
        {
            var modelView = new ModelsModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                DisplayStatus = DisplayStatus,
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ModelsModel>(json);
        }

        public void DownloadModelsCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));
            var responseViewModle = ModelsPageData(Params, Params.Id_1??0, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetModelsCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ModelsList.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetModelsCsvString(List<ModelsModel> modelsModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Model Code, Model Name, Type, Description, Version, Status");
            foreach (var models in modelsModel)
            {
                var code = models.ModelCode;
                var name = models.ModelName;
                var type = models.ModelType;
                var desc = models.ModelDescription;
                var version = models.VersionDetail.CombineVersion;
                var status = models.VersionDetail.ModelVersionStatus;
                csv.Append(CsvUtility.MakeValueCsvFriendly(code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.Append(CsvUtility.MakeValueCsvFriendly(type));
                csv.Append(CsvUtility.MakeValueCsvFriendly(desc));
                csv.Append(CsvUtility.MakeValueCsvFriendly(version));
                csv.Append(CsvUtility.MakeValueCsvFriendly(status));
                csv.AppendLine();
            }
            return csv.ToString();
        }

        public ActionResult CreateModels()
        {
            var modeltypeList = ListsMaintenance.GetModelTypeList();
            ViewData["modeltypeList"] = modeltypeList;

            var primaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["primaryBenchmarkList"] = primaryBenchmarkList;

            var secondaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["secondaryBenchmarkList"] = secondaryBenchmarkList;

            var afslLicenseeList = ListsMaintenance.GetAFSLLicenseeList();
            ViewData["afslLicenseeList"] = afslLicenseeList;

            var modelList = ListsMaintenance.GetModelListWithVersions();
            ViewData["modelList"] = modelList;

            var priceTypeList = ListsMaintenance.GetPriceTypeList();
            ViewData["priceTypeList"] = priceTypeList;

            return PartialView("_CreateModel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateModel(string imagebyte, ModelsModel model)
        {
            if (!string.IsNullOrEmpty(imagebyte))
            {
                model.ModelImage = System.Convert.FromBase64String(imagebyte.Substring(imagebyte.IndexOf(",") + 1));
            }
            var result = new ModelsController().Create(model);
            return Json(result.Content);
        }

        public ActionResult GetModels(int modelVersionID)
        {
            var modeltypeList = ListsMaintenance.GetModelTypeList();
            ViewData["modeltypeList"] = modeltypeList;

            var primaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["primaryBenchmarkList"] = primaryBenchmarkList;

            var secondaryBenchmarkList = ListsMaintenance.GetBenchmarkList();
            ViewData["secondaryBenchmarkList"] = secondaryBenchmarkList;

            var afslLicenseeList = ListsMaintenance.GetAFSLLicenseeList();
            ViewData["afslLicenseeList"] = afslLicenseeList;

            var priceTypeList = ListsMaintenance.GetPriceTypeList();
            ViewData["priceTypeList"] = priceTypeList;

            var model = new Services.ModelsController().GetModelByModelVersionFields(modelVersionID);
            return PartialView("_EditModel", model);
        }

        [HttpGet]
        public ActionResult GetModelVersionDetail(int modelVersionID)
        {
            var result = new Services.ModelsController().GetModelByModelVersionFields(modelVersionID);
            var modelImage = result.ModelImage.Length > 0 ? String.Format("data:image/gif;base64,{0}", System.Convert.ToBase64String(result.ModelImage)) : string.Empty;
            result.ModelImageBase64 = modelImage;
            result.ModelImage = new Byte[] { };
            var json = Json(result, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = 50000000;
            return json;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditModel(string imagebyte, ModelsModel model)
        {
            if (!string.IsNullOrEmpty(imagebyte))
            {
                model.ModelImage = System.Convert.FromBase64String(imagebyte.Substring(imagebyte.IndexOf(",") + 1));
            }
            var result = new Services.ModelsController().Update(model);
            BackgroundJob.Enqueue<PriceUtility>(x => x.GeneratePricesForModel(model.ModelID));
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult MakeItActive(ModelsModel model)
        {
            var result = new Services.ModelsController().Update(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteModel(ModelsModel model)
        {
            var result = new ModelsController().Delete(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteModelVersion(ModelsModel model)
        {
            var result = new ModelsController().Delete(model);
            return Json(result.Content);
        }

        #endregion

        #region ModelDetailList
        public ActionResult ModelsDetailList()
        {
            return View("ModelsDetailList");
        }

        public ActionResult ModelsDetailListDetail()
        {
            return View("ModelDetailListDetail");
        }

        public ActionResult GetModelsDetailListPage(int? modelVersionId)
        {
            var modelsList = ListsMaintenance.GetModelListWithVersions();
            if (modelVersionId != null && modelVersionId > 0)
            {
                var item = modelsList.Find(f => f.Value == modelVersionId.ToString());
                if (item != null)
                    item.Selected = true;
            }

            ViewData["modelsList"] = modelsList;
            return PartialView("_ModelsDetailList");
        }

        public ActionResult GetModelsDetailListDetailPage()
        {
            return PartialView("_ModelDetailListDetail");
        }

        public ActionResult GetModelsDetailListPageData(JQueryDataTableParamModel param, int? modelVersionId, bool isAll = false)
        {
            if (modelVersionId == 0)
            {
                return null;
            }
            var responseViewModle = ModelsDetailListPageData(param, modelVersionId, isAll);
            var model = responseViewModle.ModelList;

            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<ModelDetailListDetailModel> ModelsDetailListDetailPageData(JQueryDataTableParamModel param, int? modelDetailID, int? productVersionId)
        {
            var modelView = new ModelDetailListDetailModel
            {
                ModelDetailID = modelDetailID ?? 0,
                ProductVersionId = productVersionId,
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param
            };
            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ModelDetailListDetailModel>(json);
        }

        private ResponseModel<ModelsDetailListModel> ModelsDetailListPageData(JQueryDataTableParamModel param, int? modelVersionId, bool isAll, bool isDownlaod = false)
        {
            var modelView = new ModelsDetailListModel
            {
                ModelVersionId = modelVersionId ?? 0,
                //ToDate = toDate,
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                IsAll = isAll
            };
            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ModelsDetailListModel>(json);
        }

        public void DownloadModelsDetailListCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = ModelsDetailListPageData(Params, Params.Id, Params.IsAll, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetModelsDetailListCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ModelsDetailList.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetModelsDetailListCsvString(List<ModelsDetailListModel> modelsDetailListModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Asset Class, Code, Name, Type, Product or Security, Target Allocation");

            foreach (var models in modelsDetailListModel)
            {
                var assetClass = models.AssetClassName;
                var code = models.Code;
                var name = models.Name;
                var type = models.Type;
                var productorSecurity = models.ProductOrSecurity;
                var targetAllocation = models.TargetAllocation == null ? "" : string.Format("{0:N2}%", models.TargetAllocation);
                csv.Append(CsvUtility.MakeValueCsvFriendly(assetClass));
                csv.Append(CsvUtility.MakeValueCsvFriendly(code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(name));
                csv.Append(CsvUtility.MakeValueCsvFriendly(type));
                csv.Append(CsvUtility.MakeValueCsvFriendly(productorSecurity));
                csv.Append(CsvUtility.MakeValueCsvFriendly(targetAllocation));
                csv.AppendLine();
            }

            var model = modelsDetailListModel.FirstOrDefault();
            if (model != null)
            {
                csv.AppendLine(string.Format(",,,,,"));
                csv.AppendLine(string.Format(",,,,Total Allocation:,{0:N2}%", model.TotalTargetAllocation));
            }

            return csv.ToString();
        }

        public ActionResult GetModelsDetailListDetailPageData(JQueryDataTableParamModel param, int? modelDetailID, int? productVersionId)
        {
            if (modelDetailID == 0)
            {
                return null;
            }
            var responseViewModle = ModelsDetailListDetailPageData(param, modelDetailID, productVersionId);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public void DownloadModelsDetailListDetailCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));
            var responseViewModle = ModelsDetailListDetailPageData(Params, Params.Id, Params.Id_1);
            var model = responseViewModle.ModelList;
            string facsCsv = GetModelsDetailListDetailCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=ModelsDetailListDetail.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetModelsDetailListDetailCsvString(List<ModelDetailListDetailModel> modelsDetailListModel)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Code, Name, Product Weight, Portfolio Weight");
            foreach (var models in modelsDetailListModel)
            {
                var securityCode = models.SecurityCode;
                var securityName = models.SecurityName;
                var productWeight = string.Format("{0:N2}%", models.ProductWeight ?? 0);
                var portfolioWeight = string.Format("{0:N2}%", models.PortfolioWeight);
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityCode));
                csv.Append(CsvUtility.MakeValueCsvFriendly(securityName));
                csv.Append(CsvUtility.MakeValueCsvFriendly(productWeight));
                csv.Append(CsvUtility.MakeValueCsvFriendly(portfolioWeight));
                csv.AppendLine();
            }
            var modelDetailListDetailModel = modelsDetailListModel.FirstOrDefault();
            if (modelDetailListDetailModel == null) return csv.ToString();
            csv.AppendLine(string.Format(",,Total Allocation:{0:N2}%", modelDetailListDetailModel.TotalAllocation));
            csv.AppendLine(string.Format(",,,Total Portfolio Weight:{0:N2}%", modelDetailListDetailModel.TotalPortfolioWeight));
            return csv.ToString();
        }

        public ActionResult CreateModelsDetailList()
        {
            var modelsList = ListsMaintenance.GetModelListWithVersions();
            ViewData["modelsList"] = modelsList;

            var productList = ListsMaintenance.GetNonIndexProductVersionList();
            ViewData["productList"] = productList;

            return PartialView("_CreateModelsDetail");
        }

        public ActionResult GetModelsDetailList(int id)
        {
            var modelsList = ListsMaintenance.GetModelListWithVersions();
            ViewData["modelsList"] = modelsList;

            var productList = ListsMaintenance.GetNonIndexProductVersionList();
            ViewData["productList"] = productList;

            var model = new ModelsController().GetModelsListDetail(id);
            var securityList = ListsMaintenance.GetFilteredSecurityList(model.SecurityCode, model.SecurityId ?? 0);
            ViewData["securityList"] = securityList;


            var securityAssetClassList = new SecuritiesController().GetSecurityAssetClass(model.SecurityId ?? 0);
            var _securityAssetClassList = securityAssetClassList.Select(selectListItem => new SelectListItem
            {
                Text = selectListItem.Class,
                Value = selectListItem.Id.ToString(CultureInfo.InvariantCulture)
            }).ToList();
            _securityAssetClassList.Insert(0, new SelectListItem { Value = null, Text = "" });

            ViewData["securityAssetClassList"] = _securityAssetClassList;

            return PartialView("_EditModelsDetail", model);
        }

        [HttpPost]
        public ActionResult CreateModelsDetail(ModelsDetailListModel model)
        {
            var result = new ModelsController().Create(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult EditModelsDetail(ModelsDetailListModel model)
        {
            var result = new ModelsController().Update(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult DeleteModelsDetailList(ModelsDetailListModel model)
        {
            var result = new ModelsController().Delete(model);
            return Json(result.Content);
        }

        [HttpPost]
        public ActionResult GetSecurityAssetClass(int id)
        {
            var result = new SecuritiesController().GetSecurityAssetClass(id);
            var response = Json(result);
            return response;
        }

        #endregion

        #region ModelPrice
        public ActionResult ModelPrice()
        {
            return View("ModelPrice");
        }

        public ActionResult GetModelPriceDetail()
        {
            var modelList = ListsMaintenance.GetModelList();
            ViewData["modelList"] = modelList;

            return PartialView("_ModelPriceList");
        }

        public ActionResult CreateModelPrice()
        {
            var modelList = ListsMaintenance.GetModelList();
            ViewData["modelList"] = modelList;

            return PartialView("_CreateModelPrice");
        }

        [HttpPost]
        public ActionResult CreateModelPrice(ModelPriceModel model)
        {
            var result = new ModelsController().Create(model);
            return Json(result.Content);
        }

        private ResponseModel<ModelPriceModel> ModelPriceData(JQueryDataTableParamModel param, int? modelId, DateTime? fromDate, DateTime? toDate, bool isDownlaod = false)
        {
            var modelView = new ModelPriceModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                ModelId = modelId ?? 0,
                FromDate = fromDate,
                ToDate = toDate
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ModelPriceModel>(json);
        }

        public ActionResult GetModelPriceDetailByFilter
            (JQueryDataTableParamModel param, int? modelId, DateTime? fromDate, DateTime? toDate)
        {
            if (modelId == 0 && fromDate == null && toDate == null)
            {
                return null;
            }

            var responseViewModle = ModelPriceData(param, modelId, fromDate, toDate);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },

                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        [HttpPost]
        public ActionResult DeleteModelPrice(ModelPriceModel model)
        {
            var result = new ModelsController().Delete(model);
            return Json(result.Content);
        }

        public ActionResult GetModelPrice(int id)
        {
            var model = new ModelsController().GetModelPrice(id);

            var modelList = ListsMaintenance.GetModelList();
            ViewData["modelList"] = modelList;

            return PartialView("_EditModelPrice", model);
        }

        [HttpPost]
        public ActionResult EditModelPrice(ModelPriceModel model)
        {
            var result = new SecuritiesController().Update(model);
            return Json(result.Content);
        }
        public ActionResult CalculateModelPrice()
        {
            var modelList = ListsMaintenance.GetModelList();
            ViewData["modelList"] = modelList;

            return PartialView("_CalculateModelPrice");
        }

        #endregion

        #endregion

        public StringBuilder GetSecurityDashboardIncome(int securityId)
        {
            var model = new SecuritiesController().GetSecurity(securityId);

            StringBuilder result = new StringBuilder();
            result.Append("{");
            result.Append(IncomeData(model.Id, model.Currency));
            result.Append("}");

            return result;
        }

        public string SplitDecimal(decimal number)
        {
            number = Math.Round(number, 2);

            string result = "";
            string numberLenght = System.Convert.ToString(number);
            int blocks = numberLenght.Length / 3;

            int outBlockElem = numberLenght.Length - blocks * 3;
            List<string> ranks = new List<string>();

            if (outBlockElem != 0)
                ranks.Add(numberLenght.Substring(0, numberLenght.Length - blocks * 3));

            for (int i = 1; i <= blocks; i++)
            {
                ranks.Add(numberLenght.Substring((i - 1) * 3 + outBlockElem, 3));
            }

            foreach (var item in ranks)
            {
                if (item.First() != '.')
                    result += item + ",";
                else
                {
                    result = result.Remove(result.Length - 1, 1) + item;
                }
            }

            return result;
        }

        private int ProductPriceTest(ProductPriceModel productPriceModel)
        {
            productPriceModel.FilterOption = FilterOption.CalculatePrice;
            var json = Execute(Option.Get, productPriceModel);

            var model = GetResponseModel<ModelPriceModel>(json).Model;
            return model.PriceCalculationStatus;
        }

        public ActionResult GetSecurityPriceDetail(int sId)
        {
            var modelView = new SecurityPriceModel
            {
                SecurityId = sId,
                FilterOption = FilterOption.GetSecurityPriceBySecurityId,
            };
            var json = Execute(Option.Get, modelView);

            var test = GetResponseModel<SecurityPriceModel>(json);
            List<SecurityPriceModel> model = GetResponseModel<SecurityPriceModel>(json).ModelList;
            return PartialView("_DetailSecurityPriceList", model);
        }

        public ProductStatistics ProductPriceById(int productId, int? priceTypeId)
        {
            DateTime? fromDate = DateTime.Now.AddYears(-6);
            DateTime? toDate = DateTime.Now;

            List<ProductPriceModel> prodPrices = ProductPriceDataById(productId, fromDate, toDate).ModelList.OrderBy(o => o.Date).ToList();
            ProductStatistics productStatistics = new ProductStatistics();

            if (prodPrices.Count > 3)
            {
                var monthlyResult = new List<ProductPriceModel>();

                DateTime curtime = new DateTime();
                DateTime defaultDate = new DateTime();
                curtime = prodPrices.First().Date ?? new DateTime();

                while (curtime != defaultDate && curtime < DateTime.Now)
                {
                    var lastprice = prodPrices.Where(i => i.Date != null && i.Date.Value.Month == curtime.Month &&
                                                        i.Date.Value.Year == curtime.Year).OrderBy(i => i.Date).LastOrDefault();
                    if (lastprice != null)
                        monthlyResult.Add(lastprice);
                    curtime = curtime.AddMonths(1);
                }

                productStatistics = new ProductStatistics(monthlyResult, priceTypeId ?? 2);
            }

            return productStatistics;
        }

        private ResponseModel<ProductPriceModel> ProductPriceDataById(int? productId, DateTime? fromDate, DateTime? toDate)
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

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<ProductPriceModel>(json);
        }

        public JsonResult CheckNavSecurity(int securityId)
        {
            var model = new SecuritiesController().GetSecurity(securityId);
            bool IsNavSecurity = false;
            if (model.UnitisedId == 1)
            {
                IsNavSecurity = true;
            }
            return Json(IsNavSecurity, JsonRequestBehavior.AllowGet);
        }


        #region TDSSecurities
        public ActionResult TermDeposit()
        {
            var securityCategoryList = ListsMaintenance.GetSecurityCategoryList().Where(a => a.Text == "At Call" || a.Text == "Term Deposit" || a.Text == "").ToList();
            ViewData["securitycategorylist"] = securityCategoryList;

            var termList = ListsMaintenance.GetTermList("0").Where(a => a.Value != "CMA");
            ViewData["termList"] = termList;

            var institutionIdList = ListsMaintenance.GetTermDepositInstitutionList();
            ViewData["institutionIdList"] = institutionIdList;

            var amountList = ListsMaintenance.GetAmountList();
            ViewData["amountList"] = amountList;

            return View("TermDeposit");
        }
        public ActionResult GetTermDepositPageData(JQueryDataTableParamModel param, int? Type, decimal? AmountInvest, string Term, string Institution)
        {

            var responseViewModle = TermDepositPageData(param, Type, AmountInvest, Term, Institution);
            var model = responseViewModle.ModelList;
            return new CustomJsonResult()
            {
                Data = new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = responseViewModle.RecordCount,
                    iTotalDisplayRecords = responseViewModle.RecordCount,
                    aaData = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ResponseModel<SecurityModel> TermDepositPageData(JQueryDataTableParamModel param, int? Type, decimal? AmountInvest, string Term, string Institution, bool isDownlaod = false)
        {
            var modelView = new SecurityModel
            {
                FilterOption = FilterOption.GetTermDepositPaginatedData,
                TableParam = param,
                IsDownload = isDownlaod,
                SecurityCategoryId = Type,
                AmountInvest = AmountInvest,
                Term = (Term != null ? Term.Split(',').ToList() : new List<string>()),
                Institution = (Institution != "null" && Institution != null ? Institution.Split(',').Select(Int32.Parse).Cast<int?>().ToList() : new List<int?>()),
                FilteredTermList = ListsMaintenance.GetFilteredTermList(),
                FilteredIntTermList = ListsMaintenance.GetFilteredTermList(false),
            };

            var json = Execute(Option.Get, modelView);
            return GetResponseModel<SecurityModel>(json);
        }

        public void DownloadTermDepositCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = TermDepositPageData(Params, Params.Id, (decimal?)Params.Id_1, Params.TermSearch, Params.InstitutionIdsSearch, true);
            var model = responseViewModle.ModelList;
            string facsCsv = GetTermDepositCsvString(model);
            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=TermDeposit.csv");
            Response.Write(facsCsv);
            Response.End();
        }
        private string GetTermDepositCsvString(List<SecurityModel> securitesModel)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Institution, Min. Deposit, Max Deposit, Term,Interest Rate,Broker,Client Type");

            foreach (var security in securitesModel)
            {
                csv.Append(CsvUtility.MakeValueCsvFriendly(security.TermDeposite.Institution));
                csv.Append(CsvUtility.MakeValueCsvFriendly(security.TermDeposite.MinDeposite.HasValue ? String.Format("{0:C}", security.TermDeposite.MinDeposite) : ""));
                csv.Append(CsvUtility.MakeValueCsvFriendly(security.TermDeposite.MaxDeposite.HasValue ? String.Format("{0:C}", security.TermDeposite.MaxDeposite) : ""));
                csv.Append(CsvUtility.MakeValueCsvFriendly(security.TermDeposite.Term));
                csv.Append(CsvUtility.MakeValueCsvFriendly(security.IntrestRate.HasValue ? String.Format("{0:0.00}{1}", security.IntrestRate.Value, "%") : ""));
                csv.Append(CsvUtility.MakeValueCsvFriendly(security.TermDeposite.Broker));
                csv.Append(CsvUtility.MakeValueCsvFriendly(security.TermDeposite.ClientType));
                csv.AppendLine();
            }

            return csv.ToString();
        }
        #endregion

        #region AnalyticSecurity
        [HttpGet]
        public ActionResult AnalyticHome(string code, string type, int verId = 0)
        {
            ViewBag.CodeName = code;
            ViewBag.SecurityId = "";
            ViewBag.SecurityName = "";
            ViewBag.Type = type;
            List<SelectListItem> SortList = new List<SelectListItem>();
            SortList.Add(new SelectListItem { Text = "Sort by Gics", Value = "0" });
            SortList.Add(new SelectListItem { Text = "Sort by Asset Class", Value = "1" });
            ViewBag.SortList = SortList;
            ViewBag.VersionId = verId;
            return View("AnalyticHome");
        }
        public StringBuilder AnalyticHomeData(string code, string type)
        {
            DateTime now = DateTime.Now.AddMonths(-1);// DateTime now = new DateTime(2016,01,31);
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime endDate = new DateTime(now.Year, now.Month, daysInMonth);
            DateTime startDate = endDate.AddYears(-5);
            var analyticsEngine = new AnalyticsEngine(type, code, startDate, endDate);
            var sevenDaysData = analyticsEngine.AnalyticsDataSet.DailyPriceRange.Where(w => w.Code == code && w.PriceDate >= DateTime.Now.AddDays(-7));
            var oneMonthData = analyticsEngine.AnalyticsDataSet.DailyPriceRange.Where(w => w.Code == code && w.PriceDate >= DateTime.Now.AddMonths(-1));
            var threeMonthData = analyticsEngine.AnalyticsDataSet.DailyPriceRange.Where(w => w.Code == code && w.PriceDate >= DateTime.Now.AddMonths(-3));
            var oneYearData = analyticsEngine.AnalyticsDataSet.DailyPriceRange.Where(w => w.Code == code && w.PriceDate >= DateTime.Now.AddYears(-1));
            var twoYearData = analyticsEngine.AnalyticsDataSet.DailyPriceRange.Where(w => w.Code == code && w.PriceDate >= DateTime.Now.AddYears(-2));
            var threeYearData = analyticsEngine.AnalyticsDataSet.DailyPriceRange.Where(w => w.Code == code && w.PriceDate >= DateTime.Now.AddYears(-3));
            var sevenDays = sevenDaysData.Any() ? sevenDaysData.CopyToDataTable() : analyticsEngine.AnalyticsDataSet.DailyPriceRange.Clone();
            sevenDays.TableName = "SevenDaysPrice";
            var oneMonth = oneMonthData.Any() ? oneMonthData.CopyToDataTable() : analyticsEngine.AnalyticsDataSet.DailyPriceRange.Clone();
            oneMonth.TableName = "OneMonthPrice";
            var threeMonth = threeMonthData.Any() ? threeMonthData.CopyToDataTable() : analyticsEngine.AnalyticsDataSet.DailyPriceRange.Clone();
            threeMonth.TableName = "ThreeMonthsPrice";
            var oneYear = oneYearData.Any() ? oneYearData.CopyToDataTable() : analyticsEngine.AnalyticsDataSet.DailyPriceRange.Clone();
            oneYear.TableName = "OneYearPrice";
            var twoYear = twoYearData.Any() ? twoYearData.CopyToDataTable() : analyticsEngine.AnalyticsDataSet.DailyPriceRange.Clone();
            twoYear.TableName = "TwoYearsPrice";
            var threeYear = threeYearData.Any() ? threeYearData.CopyToDataTable() : analyticsEngine.AnalyticsDataSet.DailyPriceRange.Clone();
            threeYear.TableName = "ThreeYearsPrice";
            var summaryOneYear = oneYear.Copy();
            summaryOneYear.TableName = "SummaryOneYearPrice";
            analyticsEngine.AnalyticsDataSet.Tables.Add(sevenDays);
            analyticsEngine.AnalyticsDataSet.Tables.Add(oneMonth);
            analyticsEngine.AnalyticsDataSet.Tables.Add(threeMonth);
            analyticsEngine.AnalyticsDataSet.Tables.Add(oneYear);
            analyticsEngine.AnalyticsDataSet.Tables.Add(twoYear);
            analyticsEngine.AnalyticsDataSet.Tables.Add(threeYear);
            analyticsEngine.AnalyticsDataSet.Tables.Add(summaryOneYear);
            var performanceChartModel = new List<PerformanceChartModel>();
            var grpData = analyticsEngine.AnalyticsDataSet.DailyPriceRange.Where(w => w.SortOrder == 1).GroupBy(g => g.PriceDate).OrderBy(o => o.Key).ToList();
            foreach (var item in grpData)
            {
                var date = item.Key;
                var mainValue = item.Where(w => w.Type == "Main").Select(s => s.T_TotalValue10K).FirstOrDefault();
                var pValue = item.Where(w => w.Type == "Primary").Select(s => s.T_TotalValue10K).FirstOrDefault();
                var sValue = item.Where(w => w.Type == "Secondary").Select(s => s.T_TotalValue10K).FirstOrDefault();
                var rValue = item.Where(w => w.Type == "RiskFreeRateIndex").Select(s => s.T_TotalValue10K).FirstOrDefault();

                performanceChartModel.Add(new PerformanceChartModel(date, mainValue, pValue, sValue, rValue));
            }

            var performanceChartTable = performanceChartModel.Where(w => w.MainValue > 0 && w.PrimaryValue > 0 && (analyticsEngine.AnalyticsDataSet.AnalyticsEntities.Count() == 3 || w.SecondaryValue > 0) && w.RiskValue > 0).ToList().ToDataTable();
            performanceChartTable.TableName = "PerformanceChartTable";
            analyticsEngine.AnalyticsDataSet.Tables.Add(performanceChartTable);
            var riskAnalyticFiltered = analyticsEngine.AnalyticsDataSet.RiskAnalytics.OrderBy(o => o.Key).ThenBy(t => t.BenchmarkType).ThenBy(t => t.SortOrder).CopyToDataTable();
            riskAnalyticFiltered.TableName = "RiskAnalyticFiltered";
            analyticsEngine.AnalyticsDataSet.Tables.Add(riskAnalyticFiltered);
            analyticsEngine.AnalyticsDataSet.Tables.Remove("DailyPriceRange");
            analyticsEngine.AnalyticsDataSet.Tables.Remove("MonthlyReturns");
            analyticsEngine.AnalyticsDataSet.Tables.Remove("RiskAnalytics");
            analyticsEngine.AnalyticsDataSet.Tables.Remove("Performance");
            var jsonString = new StringBuilder(analyticsEngine.GetJsonData());
            ViewBag.DataPrice = jsonString;
            if (jsonString.Length > 0)
            {
                jsonString = jsonString.Replace('\n', ' ');
                jsonString = jsonString.Replace('\r', ' ');
                jsonString = jsonString.Replace('\t', ' ');
                jsonString = jsonString.Replace("NaN", "0,00");
            }

            return jsonString;
        }

        #endregion

        
        public ActionResult Prices()
        {
            return View("Prices");
        }


        public void DownloadPricesCsv()
        {
            var Params = (JQueryDataTableParamModel)JsonConvert.DeserializeObject(Request.Params[0], typeof(JQueryDataTableParamModel));

            var responseViewModle = PricesData(Params, Params.TermSearch, Params.FromDate, Params.ToDate, true);
            var model = new List<PriceModel>();
            var priceAnalyticModel = responseViewModle.ModelList.FirstOrDefault();
            if (priceAnalyticModel != null)
            {
                model = priceAnalyticModel.PriceList;
            }
            string facsCsv = GetPricesCsvString(model);

            //Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=Prices.csv");
            Response.Write(facsCsv);
            Response.End();
        }

        private string GetPricesCsvString(List<PriceModel> PriceModel)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Code, Date , Capital Price, Income Price, Total Price, Capital Return, Income Return, Total Return");

            foreach (var price in PriceModel)
            {
                csv.Append(CsvUtility.MakeValueCsvFriendly(price.Code));
                csv.Append(CsvUtility.MakeValueCsvFriendly(price.Date));
                csv.Append(CsvUtility.MakeValueCsvFriendly(price.CapitalPrice));
                csv.Append(CsvUtility.MakeValueCsvFriendly(price.IncomePrice));
                csv.Append(CsvUtility.MakeValueCsvFriendly(price.TotalPrice));
                csv.Append(CsvUtility.MakeValueCsvFriendly(price.CapitalReturn));
                csv.Append(CsvUtility.MakeValueCsvFriendly(price.IncomeReturn));
                csv.Append(CsvUtility.MakeValueCsvFriendly(price.TotalReturn));

                csv.AppendLine();
            }

            return csv.ToString();
        }
        private ResponseModel<PriceAnalyticModel> PricesData(JQueryDataTableParamModel param, string mpsCode, DateTime? fromDate, DateTime? toDate, bool isDownlaod = false)
        {
            var code = mpsCode.Substring(0, mpsCode.IndexOf('|'));
            var price = new PriceUtility();
            var priceList = new PriceListModel(code);
            if (mpsCode.Contains("Security"))
            {
                priceList = price.GetSecPrice(code);
            }
            else if (mpsCode.Contains("Model"))
            {
                priceList = price.GetModelPrice(code);
            }
            else if (mpsCode.Contains("Product"))
            {
                priceList = price.GetProductPrice(code);
            }

            param.FromDate = fromDate;
            param.ToDate = toDate;

            var modelView = new PriceAnalyticModel
            {
                FilterOption = FilterOption.GetAllPagination,
                TableParam = param,
                IsDownload = isDownlaod,
                PriceList = priceList.ToList()
            };
            var json = Execute(Option.Get, modelView);
            return GetResponseModel<PriceAnalyticModel>(json);

        }

        public ActionResult GetPricesDetailByFilter(JQueryDataTableParamModel param, string mpsCode, DateTime? fromDate, DateTime? toDate)
        {
            if ((string.IsNullOrEmpty(mpsCode)) && fromDate == null && toDate == null)
            {
                return null;
            }
            var responseViewModle = PricesData(param, mpsCode, fromDate, toDate);
            var model = new List<PriceModel>();
            var priceAnalyticModel = responseViewModle.ModelList.FirstOrDefault();
            if (priceAnalyticModel != null)
            {
                model = priceAnalyticModel.PriceList;
            }
            return new CustomJsonResult()
                {
                    Data = new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = responseViewModle.RecordCount,
                        iTotalDisplayRecords = responseViewModle.RecordCount,
                        aaData = model
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
        }
        public ActionResult GetPriceAnalytic()
        {
            ViewData["MPSList"] = new List<SelectListItem>();
            return PartialView("_PricesAnalyticList");
        }
    }
}

