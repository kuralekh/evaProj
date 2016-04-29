using Invest.Common.Enumerators;
using Invest.Service.Components;
using Invest.ViewModel;
using Invest.ViewModel.Models;


namespace Invest.Service
{
    public sealed class ComponentFactory
    {
        public string InvokeAction(Option option, IViewModel model)
        {
            IViewModel result = new ResponseModel<GenericModel<string>> { Model = new GenericModel<string> { Value = "No component implemented." } };
            var component = GetComponent(model);
            switch (option)
            {
                case Option.Get:
                    result = component.Get(model, model.FilterOption);
                    break;
                case Option.Add:
                    result = component.Add(model);
                    break;

                case Option.Update:
                    result = component.Update(model);
                    break;

                case Option.Delete:
                    result = component.Delete(model);
                    break;
                case Option.Login:
                    result = new UserComponent().ValidateUser(model);
                    break;
            }

            var json = ModelUtilities.SerializeModel(result);
            return json;
        }

        public BaseComponent GetComponent(IViewModel model)
        {
            if (model is UserModel) return new UserComponent();
            if (model is SystemSettingsModel) return new SettingsComponent();
            if (model is SecurityModel) return new SecurityComponent();
            if (model is SecurityCategoryModel) return new SecurityCategoryComponent();
            if (model is SecurityTypeModel) return new SecurityTypeComponent();
            if (model is MarketModel) return new MarketComponent();
            if (model is CurrencyModel) return new CurrencyComponent();
            if (model is SubAssetClassModel) return new SubAssetClassComponent();
            if (model is RegionModel) return new RegionComponent();
            if (model is GICSModel) return new GICSComponent();
            if (model is GICSTypeModel) return new GICSTypeComponent();
            if (model is RatingModel) return new RatingComponent();
            if (model is SecurityStatusModel) return new SecurityStatusComponent();
            if (model is PricingSourceModel) return new PricingSourceComponent();
            if (model is DistributionTypeModel) return new DistributionTypeComponent();
            if (model is DistributionFrequencyModel) return new DistributionFrequencyComponent();
            if (model is UnitisedModel) return new UnitisedComponent();
            if (model is SecurityPriceModel) return new SecurityPriceComponent();
            if (model is DistributionDividendDetailModel) return new DistributionDividendDetailComponent();
            if (model is AssetClassModel) return new AssetClassComponent();
            if (model is SecurityHoldingTypeModel) return new SecurityHoldingTypeComponent();
            if (model is ValuationTypeModel) return new ValuationTypeComponent();
            if (model is TransactionTypeModel) return new TransactionTypeComponent();
            if (model is IncomeTypeModel) return new IncomeTypeComponent();
            if (model is RateListTypeModel) return new RateListTypeComponent();
            if (model is IndexTypeModel) return new IndexTypeComponent();
            if (model is ProductTypeModel) return new ProductTypeComponent();
            if (model is InstitutionModel) return new InstitutionComponent();
            if (model is ProductModel) return new ProductComponent();
            if (model is RateListModel) return new RateListComponent();
            if (model is SecurityListModel) return new SecurityListComponent();
            if (model is ProductPriceModel) return new ProductPriceComponent();
            if (model is SecurityListDetailModel) return new SecurityListDetailComponent();
            if (model is AFSLLicenseeModel) return new AFSLLicenseeComponent();
            if (model is ApprovedProductListModel) return new ApprovedProductListComponent();
            if (model is CoreSatelliteModel) return new CoreSatelliteComponent();
            if (model is ModelsModel) return new ModelsComponent();
            if (model is ModelsDetailListModel) return new ModelsDetailListComponent();
            if (model is TransactionModel) return new TransactionComponent();
            if (model is AccountTypeModel) return new AccountTypeComponent();
            if (model is ModelPriceModel) return new ModelPriceComponent();
            if (model is ModelDetailListDetailModel) return new ModelDetailListDetailComponent();
            if (model is ContactModel) return new ContactComponent();
            if (model is RolesModel) return new RolesComponent();
            if (model is RolesDefinitionModel) return new RolesDefinitionComponent();
            if (model is ContactDetailModel) return new ContactDetailComponent();
            if (model is AddressModel) return new AddressComponent();
            if (model is LinkAddressModel) return new LinkAddressComponent();
            if (model is TypeAddressModel) return new TypeAddressComponent();
            if (model is EntityCoreModel) return new EntityCoreComponent();
            if (model is EntityTypeModel) return new EntityTypeComponent();
            if (model is AccountsModel) return new AccountsComponent();
            if (model is EntitiesEventLogModel) return new EntitiesEventLogComponent();
            if (model is PracticeModel) return new PracticeComponent();
            if (model is AdviserModel) return new AdviserComponent();
            if (model is ModelTypeModel) return new ModelTypeComponent();
            if (model is ClientAccountTypeModel) return new ClientAccountTypeComponent();
            if (model is ClientAccountModel) return new ClientAccountComponent();
            if (model is ClientAccountReportModel) return new ClientAccountReportComponent();
            if (model is ClientGroupModel) return new ClientGroupComponent();
            if (model is LogicalStructureModel) return new LogicalStructureComponent();
            if (model is ServiceTypeModel) return new ServiceTypeComponent();
            if (model is RebalancingTypeModel) return new RebalancingTypeComponent();
            if (model is InvestmentProgramModel) return new InvestmentProgramComponent();
            if (model is ScreenModel) return new ScreenComponent();
            if (model is ScreenDetailModel) return new ScreenDetailComponent();
            if (model is AttachmentModel) return new AttachmentComponent();
            if (model is AttachmentTypeModel) return new AttachmentTypeComponent();
            if (model is PriceTypeModel) return new PriceTypeComponent();
            if (model is OptionsTypeModel) return new OptionsTypeComponent();
            if (model is OptionsStyleModel) return new OptionsStyleComponent();
            if (model is OptionsProductTypeModel) return new OptionsProductTypeComponent();
            if (model is HoldingModel) return new HoldingComponent();
            if (model is ThirdPartyModel) return new ThirdPartyComponent();
            if (model is LinkThirdPartyModel) return new LinkThirdPartyComponent();
            if (model is FeeModel) return new FeeComponent();
            if (model is TemplateTypeModel) return new TemplateTypeComponent();
            if (model is TemplateFeeModel) return new TemplateFeeComponent();
            if (model is TemplateFeeVersionModel) return new TemplateFeeVersionComponent();
            if (model is FeesTieredModel) return new FeesTieredComponent();
            if (model is PartTemplateFeeModel) return new PartTemplateFeeComponent();
            if (model is ProductStatisticModel) return new ProductStatisticComponent();
            if (model is NavSecurityModel) return new NavSecurityComponent();
            if (model is ProductSecuritiesModel) return new ProductSecuritiesComponent();
            if (model is InvestmentProgramTemplateModel) return new InvestmentProgramTemplateComponent();
            if (model is FeesAndAdjustmentTypeModel) return new FeesAndAdjustmentTypeComponent();
            if (model is WorkFlowProcessModel) return new WorkFlowProcessComponent();
            if (model is UserGroupModel) return new UserGroupComponent();
            if (model is AccessListModel) return new AccessListComponent();
            if (model is WorkflowTaskModel) return new WorkFlowTaskComponent();
            if (model is NotificationModel) return new NotificationComponent();
            if (model is ApprovedInstitutionModel) return new ApprovedInstitutionComponent();
            if (model is PriceAnalyticModel) return new PricesComponent();
            return null;
        }
    }
}
