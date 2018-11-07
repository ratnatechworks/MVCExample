using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMVCApplication.Models;
using MyMVCApplication.Services;

namespace MyMVCApplication.Controllers
{
    public class HomeController : Controller
    {
        private static CollectProjectsData CollectProjectsData = null;

        public HomeController()
        {
            if (CollectProjectsData == null)
            {
                CollectProjectsData = new CollectProjectsData(
                    ConfigurationManager.AppSettings["teamcity.baseUrl"],
                    ConfigurationManager.AppSettings["teamcity.username"],
                    ConfigurationManager.AppSettings["teamcity.password"]
                );
            }
        }
       
    public ActionResult Index()
        {
           // List<ProjectInfo> ProjectInfos = new List<ProjectInfo>();
            var projectsWithImpDetails = from proj in CollectProjectsData.GetActiveProjects()
                select new Models.ProjectInfo()
                {
                    Id = proj.Id,
                    Name = proj.Name,
                    BuildConfigsInfo = proj.BuildConfigsInfo,
                    Url = proj.Url,
                    LastBuildDate = proj.LastBuildDate,
                    IconUrl = proj.IconUrl
                };

            //return new JsonResult()
            //{
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            //    ContentEncoding = System.Text.Encoding.UTF8,
            //    Data = projectsWithImpDetails
            //};
            return View(projectsWithImpDetails);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}