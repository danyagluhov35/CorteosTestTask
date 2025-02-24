using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorteosTestTask.Entity
{
    public class RateValue
    {
        public RateValue() 
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        public decimal Value { get; set; }
        public DateTime? Date { get; set; }

        public Rate? Currency { get; set; }
        public string? CurrencyId { get; set; }
    }
}
