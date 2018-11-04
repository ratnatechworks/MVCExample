using MyMVCApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyMVCApplication.Controllers
{
    public class StudentController : Controller
    {
        //// GET: Student
        //public ActionResult Index()
        //{
        //    return View();
        //}


        // HOW TO CREAT VIEW, So, open a StudentController class -> right click inside Index method -> click Add View..
        //GET: Student
        public ActionResult Index(int id)
        {
            //StudentsDb StDb = new StudentsDb();            
            //return View(StDb.GetStudents());
            //return "This is Index action method of StudentController";
            //******************************//
            // get student from the database 
            StudentsDb StDb = new StudentsDb();
            StudentFullDetails studentFD = new StudentFullDetails();
            Student st = new Student();
            List<Student> stud = new List<Student>();
            List<StudentFullDetails> studFull = new List<StudentFullDetails>();
            studentFD = StDb.GetStudents().Where(s => s._Student.StudentId == id).FirstOrDefault();
            stud.Add(studentFD._Student);
            studFull.Add(studentFD);
            // return View(stud);
            return View(studFull);
        }


        // request should be http://localhost/student/find/1 instead of http://localhost/student/getbyid/1
        [ActionName("find")]
        public ActionResult GetById(int id)
        {
            // get student from the database 
            StudentsDb StDb = new StudentsDb();
            Student st = new Student();
            List<Student> stud = new List<Student>();
            //st = StDb.GetStudents().Where(s => s.StudentId == id).FirstOrDefault();
            //stud.Add(st);
            return View(stud);
        }

        //Use NonAction attribute when you want public method in a controller but do not want to treat it as an action method. 
        [NonAction]
        public StudentFullDetails GetStudnet(int id)
        {
            StudentsDb StDb = new StudentsDb();
            return StDb.GetStudents().Where(s => s._Student.StudentId == id).FirstOrDefault();
        }
    }
}