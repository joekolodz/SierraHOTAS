using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SierraHOTAS.Models
{
    public class HOTASAxis
    {
        public Dictionary<int, HOTASMap> MapRanges { get; set; }
        public bool IsDirectional { get; set; }
    }
}
