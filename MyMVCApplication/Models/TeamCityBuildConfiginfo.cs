using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMVCApplication.Models
{
    [System.Diagnostics.DebuggerDisplay("BuildConfig - {Id} - {Name} - CurrentBuildDate {CurrentBuildDate}")]
    public class TeamCityBuildConfiginfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool CurrentBuildIsSuccesfull { get; set; }
        public string Url { get; set; }
        public string BuildCommitedBy { get; set; }
        public DateTime? CurrentBuildDate { get; set; }
    }
}