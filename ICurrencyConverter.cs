using System;
using Microsoft.Extensions.Configuration;

namespace SuppliesPriceLister
{
    public interface ICurrencyConverter
    {
        decimal Convert(string from, string to, decimal value);
    }

    //DUMMY implemetation
    class SimpleCurrencyConverter: ICurrencyConverter
    {
        private readonly decimal _rate;

        public SimpleCurrencyConverter(IConfiguration config)
        {
            _rate = decimal.Parse(config["audUsdExchangeRate"]);
        }
        public decimal Convert(string @from, string to, decimal value)
        {
            if (@from == "USD" && to == "AUD")
                return value / _rate;

            throw new ApplicationException("can convert only AUD to USD");
        }
    }
}