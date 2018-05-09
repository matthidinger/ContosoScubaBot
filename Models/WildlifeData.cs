using System.Collections.Generic;

namespace ContosoScuba.Bot.Models
{

    public class WildlifeData
    {
        public WildlifeData()
        {
            Images = new List<ViewImage>();
        }
        public string Introduction { get; set; }
        public string Instructions { get; set; }
        public List<ViewImage> Images { get; set; }
    }
}