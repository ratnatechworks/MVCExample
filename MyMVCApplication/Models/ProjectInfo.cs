using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMVCApplication.Models
{

    [System.Diagnostics.DebuggerDisplay("Project {Id} - {Name}")]
    public class ProjectInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string IconUrl { get; set; }
        public DateTime? LastBuildDate { get; set; }
        public IEnumerable<TeamCityBuildConfiginfo> BuildConfigsInfo { get; set; }

    }
}