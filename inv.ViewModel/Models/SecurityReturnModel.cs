using System.ComponentModel.DataAnnotations;

namespace Invest.ViewModel.Models
{
    public class SecurityReturnModel : BaseViewModel
    {
        public int Id { get; set; }
        public decimal? MonthReturn { get; set; }
        public decimal? YearReturn { get; set; }
    }
}

