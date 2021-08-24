using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;

namespace SuppliesPriceLister
{
    public class SupplierData
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
    public interface ISupplierDataReader
    {
        Task<IEnumerable<SupplierData>> ReadAsync(Stream input);
    }
}