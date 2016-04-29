using System;
using System.Collections.Generic;
using System.Web.Http;
using Invest.Common.Enumerators;
using Invest.ViewModel.Models;
using Invest.Web.Controllers;

namespace Invest.Web.Services
{
    public class InvestmentProgramController : BaseApiController
    {
        [HttpGet]
        public RebalancingTypeModel GetRebalancingTypeById(int id)
        {
            var modelView = new RebalancingTypeModel { Id = id, FilterOption = FilterOption.ModelByFields };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<RebalancingTypeModel>(viewJson);
            return responseModel.Model;
        }

        [HttpGet]
        public ServiceTypeModel GetServiceTypeById(int id)
        {
            var modelView = new ServiceTypeModel { Id = id, FilterOption = FilterOption.ModelByFields };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<ServiceTypeModel>(viewJson);
            return responseModel.Model;
        }

        [HttpGet]
        public List<RebalancingTypeModel> RebalancingTypeList()
        {
            var modelView = new RebalancingTypeModel { FilterOption = FilterOption.GetAll };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<RebalancingTypeModel>(viewJson);
            return responseModel.ModelList;
        }

        [HttpGet]
        public List<ServiceTypeModel> ServiceTypeList()
        {
            var modelView = new ServiceTypeModel { FilterOption = FilterOption.GetAll };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<ServiceTypeModel>(viewJson);
            return responseModel.ModelList;
        }

        [HttpGet]
        public InvestmentProgramModel GetInvestmentProgram(int invProgId)
        {
            var modelView = new InvestmentProgramModel { InvestmentProgramID = invProgId, FilterOption = FilterOption.ModelByFields };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramModel>(viewJson);
            return responseModel.Model;
        }

        [HttpGet]
        public List<InvestmentProgramModel> GetInvestmentProgramConstraint(int invProgId, int EntityCoreId)
        {
            var modelView = new InvestmentProgramModel { InvestmentProgramID = invProgId, EntityCoreId = EntityCoreId, FilterOption = FilterOption.GetInvestmentProgramConstraint };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramModel>(viewJson);
            return responseModel.ModelList;
        }
        [HttpGet]
        public List<InvestmentProgramModel> GetInvestmentProgramScreen(int invProgId, int EntityCoreId)
        {
            var modelView = new InvestmentProgramModel { InvestmentProgramID = invProgId, EntityCoreId = EntityCoreId, FilterOption = FilterOption.GetAllInvestmentProgramScreen };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramModel>(viewJson);
            return responseModel.ModelList;
        }

        public List<InvProgStrategicAllocationModel> GetAllStrategicAllocationByInvestmentProgramId(int id)
        {
            var modelView = new InvestmentProgramModel { InvestmentProgramID = id, FilterOption = FilterOption.GetInvestmentProgramStrategicAllocation };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramModel>(viewJson);
            return responseModel.Model.InvProgStrategicAllocationModel;
        }

        public List<InvestmentProgramModel> GetAllDefaultSecuritiesByInvestmentProgramId(int id)
        {
            var modelView = new InvestmentProgramModel { InvestmentProgramID = id, FilterOption = FilterOption.GetInvestmentProgramDefaultSecurities };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramModel>(viewJson);
            return responseModel.ModelList;
        }

        public List<InvestmentProgramTemplateModel> GetAllDefaultSecuritiesTemplateByInvestmentProgramId(int id)
        {
            var modelView = new InvestmentProgramTemplateModel { InvestmentProgramID = id, FilterOption = FilterOption.GetInvestmentProgramDefaultSecurities };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramTemplateModel>(viewJson);
            return responseModel.ModelList;
        }

        public List<InvProgStrategicAllocationModel> GetAllDefualtSecuritiesByInvestmentProgramId(int id)
        {
            var modelView = new InvestmentProgramModel { InvestmentProgramID = id, FilterOption = FilterOption.GetInvestmentProgramDefaultSecurities };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramModel>(viewJson);
            return responseModel.Model.InvProgStrategicAllocationModel;
        }

        [HttpGet]
        public InvestmentProgramTemplateModel GetInvestmentProgramTemplate(int invProgId)
        {
            var modelView = new InvestmentProgramTemplateModel { InvestmentProgramID = invProgId, FilterOption = FilterOption.ModelByFields };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramTemplateModel>(viewJson);
            return responseModel.Model;
        }

        public List<InvProgStrategicAllocationTemplateModel> GetAllStrategicAllocationTemplateByInvestmentProgramId(int id)
        {
            var modelView = new InvestmentProgramTemplateModel { InvestmentProgramID = id, FilterOption = FilterOption.GetInvestmentProgramStrategicAllocation };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramTemplateModel>(viewJson);
            return responseModel.Model.InvProgStrategicAllocationTemplateModel;
        }
        [HttpGet]
        public List<InvestmentProgramTemplateModel> GetAllInvestmentProgramTemplate()
        {
            var modelView = new InvestmentProgramTemplateModel { FilterOption = FilterOption.GetAll };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramTemplateModel>(viewJson);
            return responseModel.ModelList;
        }
        [HttpGet]
        public List<InvestmentProgramTemplateModel> GetInvestmentProgramTemplateConstraint(int invProgId, int EntityCoreId)
        {
            var modelView = new InvestmentProgramTemplateModel { InvestmentProgramID = invProgId, EntityCoreId = EntityCoreId, FilterOption = FilterOption.GetInvestmentProgramConstraint };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramTemplateModel>(viewJson);
            return responseModel.ModelList;
        }
        [HttpGet]
        public List<InvestmentProgramTemplateModel> GetInvestmentProgramTemplateScreen(int invProgId, int EntityCoreId)
        {
            var modelView = new InvestmentProgramTemplateModel { InvestmentProgramID = invProgId, EntityCoreId = EntityCoreId, FilterOption = FilterOption.GetAllInvestmentProgramScreen };
            var viewJson = Execute(Option.Get, modelView);
            var responseModel = GetResponseModel<InvestmentProgramTemplateModel>(viewJson);
            return responseModel.ModelList;
        }
        [HttpGet]
        public string GetCalculatedHolding(int clientId, DateTime? holdingDate = null)
        {
            var inves = new InvestmentController();
            return inves.CalculateHoldings(clientId, holdingDate);
        }
    }
}