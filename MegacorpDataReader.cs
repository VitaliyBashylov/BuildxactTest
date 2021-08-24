using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuppliesPriceLister
{
    public class MegacorpDataReader: ISupplierDataReader
    {
        private readonly ICurrencyConverter _currencyConverter;

        class MegacorpData
        {
            public List<Partner> Partners { get; set; }
            public enum PartnerType
            {
                Internal,
            }
            public class Partner
            {
                public string Name { get; set; }
                public PartnerType PartnerType { get; set; }
                public string PartnerAddress { get; set; }
                public List<Item> Supplies { get; set; }
            }

            public class Item
            {
                public string Id { get; set; }
                public string Description { get; set; }
                public string Uom { get; set; }
                public int PriceInCents { get; set; }
                public Guid ProviderId { get; set; }
                public string MaterialType { get; set; }
            }
        }

        public MegacorpDataReader(ICurrencyConverter currencyConverter)
        {
            _currencyConverter = currencyConverter;
        }

        public async Task<IEnumerable<SupplierData>> ReadAsync(Stream input)
        {
            using var tr = new StreamReader(input);
            var reader = new JsonTextReader(tr);
            var ser = new JsonSerializer();
            var data = ser.Deserialize<MegacorpData>(reader);

            return data.Partners
                .SelectMany(p => p.Supplies)
                .Select(ConvertData);
            }

        private SupplierData ConvertData(MegacorpData.Item item)
        {
            return new SupplierData()
            {
                Description = item.Description,
                Id = item.Id,
                Price = _currencyConverter.Convert("USD", "AUD", item.PriceInCents / (decimal) 100)
            };
        }
    }
}