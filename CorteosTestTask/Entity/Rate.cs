using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorteosTestTask.Entity
{
    public class Rate
    {
        public Rate() 
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public List<RateValue>? RateValue { get; set; }
    }
}
