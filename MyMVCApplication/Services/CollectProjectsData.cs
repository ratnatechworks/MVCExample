using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using MyMVCApplication.Models;

namespace MyMVCApplication.Services
{
    public class CollectProjectsData
    {
        protected readonly string BaseUrl;
        protected readonly string UserName;
        protected readonly string Password;


        // protected readonly ICacheService CacheService;
        public CollectProjectsData(string baseUrl, string username, string password)
        {
            this.BaseUrl = baseUrl;
            this.UserName = username;
            this.Password = password;
        }

        /// url to retrieve list of projects in TeamCity

        private const string URL_PROJECTS_LIST = @"/httpAuth/app/rest/projects";

       
        /// url to retrieve details of given {0} project (buildtypes etc)
        
        private const string URL_PROJECT_DETAILS = @"/httpAuth/app/rest/projects/id:{0}";

     
        /// retrieve the first 100 builds of the given buildconfig and retrieve the status of it
        
        private const string URL_BUILDS_LIST = @"/httpAuth/app/rest/buildTypes/id:{0}/builds";

       
        /// retrieve details of the given build ({0}) and verify that the /buildType/settings/property[@name='allowExternalStatus'] == 'true'
        
        private const string URL_BUILD_DETAILS = @"/httpAuth/app/rest/buildTypes/id:{0}";

       
        /// url to retrieve the changes commited in the given {0} buildrunId
        
        private const string URL_BUILD_CHANGES = @"/httpAuth/app/rest/changes?build=id:{0}";

        
        /// Url to retrieve the details of the given {0} changeId
        
        private const string URL_CHANGE_DETAILS = @"/httpAuth/app/rest/changes/id:{0}";

        
        /// url to retrieve the emailaddress of the given {0} userId
        
        private const string URL_USER_EMAILADDRESS = @"/httpAuth/app/rest/users/id:{0}/email";

        public IEnumerable<ProjectInfo> GetActiveProjects()
        {
            var projects = getNonArchivedProjects().ToList();

            var failing = projects.Where(p => p.BuildConfigsInfo.Any(c => !c.CurrentBuildIsSuccesfull));
            var success = projects.Where(p => p.BuildConfigsInfo.All(c => c.CurrentBuildIsSuccesfull));

            int amountToTake = Math.Max(25, failing.Count());

            //only display the most recent 15 build projects together with the failing ones OR if we have more failing display those
            return failing.Concat(success.OrderByDescending(p => p.LastBuildDate)).Take(amountToTake);
        }

        private IEnumerable<ProjectInfo> getNonArchivedProjects()
        {
            XmlDocument projectsPageContent = GetPageContents(URL_PROJECTS_LIST);
            if (projectsPageContent == null)
                yield break;

            foreach (XmlElement el in projectsPageContent.SelectNodes("//project"))
            {
                var project = ParseProjectDetails(el.GetAttribute("id"), el.GetAttribute("name"));
                if (project == null)
                    continue;
                yield return project;
            }
        }

        private ProjectInfo ParseProjectDetails(string projectId, string projectName)
        {
            //determine details, archived? buildconfigs
            //XmlDocument projectDetails = CacheService.Get<XmlDocument>("project-details-" + projectId, () => {
            //    return GetPageContents(string.Format(URL_PROJECT_DETAILS, projectId));
            //}, 15 * 60);

            XmlDocument projectDetails = GetPageContents(string.Format(URL_PROJECT_DETAILS, projectId));

            if (projectDetails == null)
                return null;

            if (projectDetails.DocumentElement.GetAttribute("archived") == "true")
                return null;//not needed

            List<TeamCityBuildConfiginfo> buildConfigs = new List<TeamCityBuildConfiginfo>();
            foreach (XmlElement buildType in projectDetails.SelectNodes("project/buildTypes/buildType"))
            {
                var buildConfigDetails = ParseBuildConfigDetails(buildType.GetAttribute("id"), buildType.GetAttribute("name"));
                if (buildConfigDetails != null)
                    buildConfigs.Add(buildConfigDetails);
            }

            if (buildConfigs.Count == 0)
                return null;//do not report 'empty' projects' *******************************************************************************

            return new ProjectInfo
            {
                Id = projectId,
                Name = projectName,
                Url = projectDetails.DocumentElement.GetAttribute("webUrl"),
                IconUrl = parseProjectProperty(projectDetails, "dashboard.project.logo.url"),
                //SonarProjectKey = parseProjectProperty(projectDetails, "sonar.project.key"),
                BuildConfigsInfo = buildConfigs,
                LastBuildDate = (buildConfigs.Where(b => b.CurrentBuildDate.HasValue).Max(b => b.CurrentBuildDate.Value))
            };
        }

        private TeamCityBuildConfiginfo ParseBuildConfigDetails(string id, string name)
        {
            //do we need to show this buildCOnfig?
            // bool isVisibleExternally = CacheService.Get<ProjectVisible>("build-visible-widgetinterface-" + id, () => IsBuildVisibleOnExternalWidgetInterface(id), CACHE_DURATION).Visible;
            // if (!isVisibleExternally)
            //  return null;

            ///retrieve details of last 100 builds and find out if the last (=first row) was succesfull or iterate untill we found the first breaker?
            XmlDocument buildResultsDoc = GetPageContents(string.Format(URL_BUILDS_LIST, id));
            XmlElement lastBuild = buildResultsDoc.DocumentElement.FirstChild as XmlElement;


            DateTime? currentBuildDate = DateTime.Now;
            bool currentBuildSuccesfull = true;
            IEnumerable<string> buildBreakerUserName = new List<string>();

            if (lastBuild != null)
            {
                currentBuildSuccesfull = lastBuild.GetAttribute("status") == "SUCCESS";//we default to true

                //try to parse last date
                string buildDate = lastBuild.GetAttribute("startDate");
                DateTime theDate;

                //TeamCity date format => 20121101T134409+0100
                //if (DateTime.TryParseExact(buildDate, @"yyyyMMdd\THHmmsszz\0\0", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out theDate))
                //{
                //    currentBuildDate = theDate;
                //}
                //currentBuildDate = DateTime.TryParseExact(buildDate, @"yyyyMMdd\THHmmsszz\0\0", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
                

                //if the last build was not successfull iterate back untill we found one which was successfull so we know who might have broke it.
                if (!currentBuildSuccesfull)
                {
                    XmlNode lastSuccessfullBuild = buildResultsDoc.DocumentElement.SelectSingleNode("build[@status='SUCCESS']");
                    if (lastSuccessfullBuild != null)
                    {
                        XmlElement breakingBuild = lastSuccessfullBuild.PreviousSibling as XmlElement;
                        if (breakingBuild == null)
                            buildBreakerUserName.ToList().Add("no-breaking-build-after-succes-should-not-happen");
                        else
                            buildBreakerUserName = ParseBuildBreakerDetails(breakingBuild.GetAttribute("id"));
                        if (!buildBreakerUserName.LastOrDefault().Contains("Build Date Not Found"))
                        {
                            currentBuildDate = DateTime.ParseExact(buildBreakerUserName.LastOrDefault(), @"yyyyMMdd\THHmmsszz\0\0", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
                        }
                        
                        //buildBreakerEmailaddress = CacheService.Get<IEnumerable<string>>(
                        //    "buildbreakers-build-" + breakingBuild.GetAttribute("id"),
                        //    () => ParseBuildBreakerDetails(breakingBuild.GetAttribute("id")),
                        //    CACHE_DURATION
                        //).Distinct().ToList();
                    }
                    else
                    {
                        //IF NO previous pages with older builds available then we can assume this is the first build and it broke. show image of that one.
                        //TODO we could iterate older builds to find the breaker via above logic
                    }
                }
            }

            return new TeamCityBuildConfiginfo()
            {
                Id = id,
                Name = name,
                Url = new Uri(string.Format("{0}/viewType.html?buildTypeId={1}&tab=buildTypeStatusDiv", BaseUrl, id)).ToString(),
                CurrentBuildIsSuccesfull = currentBuildSuccesfull,
                CurrentBuildDate = currentBuildDate,
                BuildCommitedBy = buildBreakerUserName.FirstOrDefault()
            };
        }

        private static string parseProjectProperty(XmlDocument projectDetails, string propertyName)
        {
            var propertyElement = projectDetails.SelectSingleNode(string.Format("project/parameters/property[@name='{0}']/@value", propertyName));
            if (propertyElement != null)
                return propertyElement.Value;

            return null;
        }

        private IEnumerable<string> ParseBuildBreakerDetails(string buildId)
        {
            List<string> BuildDateAndCommitedBy = new List<string>();
            string userId = "User Not Found";
            string builddate = "Build Date Not Found";
            //retrieve changes
            XmlDocument buildChangesDoc = GetPageContents(string.Format(URL_BUILD_CHANGES, buildId));
            foreach (XmlElement el in buildChangesDoc.SelectNodes("//change"))
            {
                //retrieve change details
                string changeId = el.GetAttribute("id");
                if (string.IsNullOrEmpty(changeId))
                    throw new ArgumentNullException(string.Format("@id of change within buildId {0} should not be NULL", buildId));

                //retrieve userid who changed something
                XmlDocument changeDetailsDoc = GetPageContents(string.Format(URL_CHANGE_DETAILS, changeId));
                XmlElement userDetails = (changeDetailsDoc.SelectSingleNode("//change") as XmlElement);
                
                //retrieve userId
                if (userDetails != null) // Change is not linked to user who commited it
                {
                    userId = userDetails.GetAttribute("username");
                    builddate = userDetails.GetAttribute("date");
                }
            }

            BuildDateAndCommitedBy.Add(userId);
            BuildDateAndCommitedBy.Add(builddate);
            return BuildDateAndCommitedBy;
        }

        private string ParseBuilddate(string buildId)
        {
            string builddate = null;
            //retrieve changes
            XmlDocument buildChangesDoc = GetPageContents(string.Format(URL_BUILD_CHANGES, buildId));
            foreach (XmlElement el in buildChangesDoc.SelectNodes("//change"))
            {
                //retrieve change details
                string changeId = el.GetAttribute("id");

                //retrieve change Build Date
                builddate = el.GetAttribute("date");
                
                if (string.IsNullOrEmpty(changeId) || builddate.Length <= 0)
                {
                    builddate = "Build Date Not Found";
                }
            }
            return builddate;
        }

        protected XmlDocument GetPageContents(string relativeUrl)
        {
            XmlDocument result = new XmlDocument();
            result.LoadXml(GetContents(relativeUrl));
            return result;
        }

        /// <summary>
        /// retrieve the content of given url. throws httpexception if raised
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        protected string GetContents(string relativeUrl)
        {
            try
            {
                Uri uri = new Uri(string.Format("{0}{1}", BaseUrl, relativeUrl));
                WebRequest myWebRequest = HttpWebRequest.Create(uri);

                HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

                NetworkCredential myNetworkCredential = new NetworkCredential(UserName, Password);
                CredentialCache myCredentialCache = new CredentialCache();
                myCredentialCache.Add(uri, "Basic", myNetworkCredential);

                myHttpWebRequest.PreAuthenticate = true;
                myHttpWebRequest.Credentials = myCredentialCache;

                using (WebResponse myWebResponse = myWebRequest.GetResponse())
                {
                    using (Stream responseStream = myWebResponse.GetResponseStream())
                    {
                        StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
                        return myStreamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                throw new HttpException(string.Format("Error while retrieving url '{0}': {1}", relativeUrl, e.Message), e);
            }
        }
    }
}