using System;

namespace Pipeline.Shared
{
    public class Product
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Family { get; set; }
        public string Original { get; set; }

        public override string ToString()
        {
            return "{"
                + "Name: " + Name + ","
                + "Manufacturer: " + Manufacturer + ","
                + "Model: " + Model + ","
                + "Family: " + Family + ","
                + "}";
        }
    }
}
