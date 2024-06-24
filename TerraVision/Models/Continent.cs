using System.Collections.Generic;

namespace TerraVision.Models
{
    public class Continent : IContinent
    {
        public string Name { get; set; }
        public List<ICountry> Countries { get; set; }
    }
}