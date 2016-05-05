using System;
using System.Diagnostics;
using Pipeline.Shared;

namespace Pipeline.Extraction
{
    static class ProductParser
    {
        public static Product Parse(String str)
        {
            Debug.Assert(!String.IsNullOrEmpty(str));
            var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(str);
            return new Product
            {
                Name = FieldMunger.Munge((string)jsonObj["product_name"]),
                Manufacturer = FieldMunger.Munge((string)jsonObj["manufacturer"]),
                Model = FieldMunger.Munge((string)jsonObj["model"]),
                Original = str
            };
        }
    }
}
