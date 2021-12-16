using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace TGBot.Models
{
    public class User
    {
        public long ID { get; set; }
        public string Currency { get; set; }
        public string Data { get; set; }

        public User()
        {

        }

        public User(long id)
        {
            ID = id;
        }

        public string GetCourse()
        {
            if (IsCorrectData())
            {
                var way = "https://api.privatbank.ua/p24api/exchange_rates?json&date=" + Data;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(way);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var answer = new Courses();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var search = streamReader.ReadToEnd();
                    var j = JObject.Parse(search);

                    IList<JToken> results = j["exchangeRate"].Children().ToList();
                    IList<Courses> searchResults = new List<Courses>();

                    foreach (JToken result in results)
                    {
                        var searchResult = result.ToObject<Courses>();
                        searchResults.Add(searchResult);
                    }

                    foreach (var item in searchResults)
                    {
                        if (item.Currency == Currency)
                        {
                            answer = item;
                        }
                    }

                }

                return "Data: " + Data + "\n 1 " + Currency + "= " + answer.SaleRateNB.ToString() + " UAH";
            }
            else
            {
                return "Could not get data for this date";
            }
        }

        private bool IsCorrectData()
        {
            DateTime data;
            var nowData = DateTime.Now;

            var isData = DateTime.TryParse(Data, out data);

            if (isData)
            {
                var result = DateTime.Compare(data, nowData);
                if (result <= 0)
                {
                    var diff = nowData.Subtract(data);

                    if (diff.TotalDays < 4360)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
