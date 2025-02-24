using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorteosTestTask.Entity;
using System.Xml.Linq;
using CorteosTestTask.IService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CorteosTestTask.Service
{
    public class RateService : IRateService
    {
        private const string LinkCb = "https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx";
        private ApplicationContext db;
        private ILogger Log;
        public RateService(ILogger logger)
        {
            db = new ApplicationContext();
            Log = logger;
            Log.LogInformation("Вызвался конструктор RateService");
        }
        /// <summary>
        ///     Отправляет запрос на сервер ЦБ, для получения курса валют по дате
        /// </summary>
        /// <param name="request">Формирование soap-запроса для получения курса валют, по определенной дате</param>
        /// <param name="client">Нужен для отправки запроса на сервер ЦБ, POST,GET и т.д</param>
        /// <param name="content">Формирование тела запроса, в конструктор которого передается сам запрос, кодировка, и в каком виде мы его отправляем</param>
        public async Task FetchRates(string dateTime)
        {
            try
            {
                string request = $"<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n  <soap:Body>\r\n    <GetCursOnDate xmlns=\"http://web.cbr.ru/\">\r\n      <On_date>{dateTime}</On_date>\r\n    </GetCursOnDate>\r\n  </soap:Body>\r\n</soap:Envelope>";

                using var client = new HttpClient();
                var content = new StringContent(request, Encoding.UTF8, "text/xml");
                content.Headers.Add("rates", "http://web.cbr.ru/GetCursOnDate");

                var response = await client.PostAsync(LinkCb, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Возникла какая-то ошибка запроса");
                    return;
                }
                string responseString = await response.Content.ReadAsStringAsync();
                await Save(responseString, dateTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка, {ex.Message}");
                Log.LogError(ex.Message);
            }
        }
        /// <summary>
        ///     Вернет валюту, которая была добавлена последней по дате
        /// </summary>
        public async Task<RateValue> GetLastRate() 
        {
            try
            {
                return db.RatesValues
                      .OrderByDescending(c => c.Date)
                      .FirstOrDefault()!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка, {ex.Message}");
                Log.LogError(ex.Message);
                return new RateValue();
            }
        }
        /// <summary>
        ///     Если курс валют в бд остутствует, то загружаем значения на сегоднящний, и последний месяц
        ///     Иначе, получаем последнюю добавленную валюту по дате, если сегодняшняя дата не соответсвует последней добавленной, то добавляем все 
        ///     курсы валют на сегодняшний день
        /// </summary>
        public async Task LoadRate()
        {
            try
            {
                var res = GetLastRate();
                if (db.RatesValues.Count() <= 0)
                    for (int i = 0; i < DateTime.Now.Day; i++)
                        await FetchRates(DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd"));
                else
                {
                    var result = await GetLastRate();
                    if (result?.Date!.Value.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
                        await FetchRates(DateTime.Now.ToString("yyyy-MM-dd"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка, {ex.Message}");
                Log.LogError(ex.Message);
            }
        }

        /// <summary>
        ///     Сохранит в БД код валюты, полное его название, и его курс за последний месяц
        /// </summary>
        /// <param name="xmlData">Содержит строковое состояение xml документа, в котором содержиться список валют на определенную дату</param>
        /// <param name="date">Дата курса валюты</param>
        public async Task Save(string xmlData, string date)
        {
            try
            {
                var doc = XDocument.Parse(xmlData);
                var rates = doc.Descendants("ValuteCursOnDate");

                foreach (var rate in rates)
                {
                    string code = rate.Element("VchCode")?.Value!;
                    string rateName = rate.Element("Vname")?.Value!.Trim()!;
                    var value = Convert.ToDecimal(rate.Element("Vcurs")?.Value.Replace(".", ","));


                    var curr = db.Rates.FirstOrDefault(c => c.Code == code);
                    if (curr == null)
                    {
                        var currency = new Entity.Rate
                        {
                            Code = code,
                            Name = rateName
                        };
                        var currencyValue = new RateValue
                        {
                            Value = value,
                            Date = DateTime.Parse(date),
                            CurrencyId = currency.Id
                        };
                        db.RatesValues.Add(currencyValue);
                        db.Rates.Add(currency);
                    }
                    else
                    {
                        var currencyValue = new RateValue
                        {
                            Value = value,
                            Date = DateTime.Parse(date),
                            CurrencyId = curr.Id
                        };
                        db.RatesValues.Add(currencyValue);
                    }
                }
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка, {ex.Message}");
                Log.LogError(ex.Message);
            }
        }
        /// <summary>
        ///     Вернет валюты, их кодовое значение, и полное название
        /// </summary>
        public async Task<List<Rate>> GetAllRate()
        {
            try
            {
                return await db.Rates.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка, {ex.Message}");
                Log.LogError(ex.Message);
                return new List<Rate>();
            }
        }
        /// <summary>
        ///     Вернет курс валюты за последнее время
        /// </summary>
        /// <param name="code">
        ///     Код валюты
        /// </param>
        public async Task<List<RateValue>> GetSingleRate(string code)
        {
            try
            {
                return await db.RatesValues
                    .Include(c => c.Currency)
                    .Where(c => c.Currency!.Code == code)
                    .OrderBy(c => c.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка, {ex.Message}");
                Log.LogError(ex.Message);
                return new List<RateValue>();
            }
        }
    }
}
