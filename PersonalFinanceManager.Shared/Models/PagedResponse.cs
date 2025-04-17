using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Models
{
    public class PagedResponse<T>
    {
        [JsonPropertyName("items")]
        public List<T> Items { get; set; }

        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal? TotalAmount { get; set; } = null;

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
    }
}
