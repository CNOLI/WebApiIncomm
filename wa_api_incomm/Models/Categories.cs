using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Categories
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public List<Category> categories { get; set; }

        public class Category
        {
            public int id { get; set; }
            public string name { get; set; }
            public string productImage { get; set; }
            public string status { get; set; }
            public List<Product> products { get; set; }
        }

        public class Product
        {
            public string type { get; set; }
            public string subType { get; set; }
            public string name { get; set; }
            public string productCode { get; set; }
            public string eanCode { get; set; }
            public int amount { get; set; }
            public string regExp { get; set; }
            public string productImage { get; set; }
            public bool calculateIva { get; set; }
            public string status { get; set; }
        }

    }
}
