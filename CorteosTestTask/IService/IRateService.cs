using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorteosTestTask.Entity;

namespace CorteosTestTask.IService
{
    public interface IRateService
    {
        Task LoadRate();
        Task Save(string xmlData, string date);
        Task FetchRates(string dateTime);
        Task<RateValue> GetLastRate();
        Task<List<Rate>> GetAllRate();
        Task<List<RateValue>> GetSingleRate(string code);
    }
}
