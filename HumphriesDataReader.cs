using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace SuppliesPriceLister
{
    class HumphriesData
    {
        public Guid Identifier { get; set; }
        public string Desc { get; set; }
        public string Unit { get; set; }
        public decimal CostAud { get; set; }
    }

    public class HumphriesDataReader : ISupplierDataReader
    {
        public async Task<IEnumerable<SupplierData>> ReadAsync(Stream input)
        {
            using var tr = new StreamReader(input);
            var reader = new CsvReader(tr, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = a => a.Header.ToLowerInvariant(), 
            });
            
            var result = await reader.GetRecordsAsync<HumphriesData>()
                .Select(ConvertData)
                .ToListAsync();

            return result;
        }

        private SupplierData ConvertData(HumphriesData data)
        {
            return new SupplierData()
            {
                Description  = data.Desc,
                Id = data.Identifier.ToString(),
                Price = data.CostAud
            };
        }
    }
}