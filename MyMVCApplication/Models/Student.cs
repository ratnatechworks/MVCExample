using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMVCApplication.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int Age { get; set; }
    }

    public class StudentSemester
    {
        public Semester _SemesterOne { get; set; }
        public Semester _SemesterTwo { get; set; }
        public Semester _SemesterThree { get; set; }
    }

    public class Semester
    {
        public string Subject { get; set; }
        public string Status { get; set; }
        public int Marks { get; set; }
    }

    public class StudentFullDetails
    {
        //Student s = new Student();
       // StudentSemester ss = new StudentSemester();
        public new Student _Student { get; set; }
        public new StudentSemester _StudentSemester { get; set; }        
    }

    public class StudentsDb
    {
        public List<StudentFullDetails> GetStudents()
        {
            List<Student> students = new List<Student>();
            List<StudentFullDetails> studentsFullDetails = new List<StudentFullDetails>();
            for (int i=1; i<=5; i++)
            {
                Student stdnt = new Student();
                Semester seme = new Semester();
                StudentSemester stSem = new StudentSemester();
                StudentFullDetails SFD = new StudentFullDetails();
                stdnt.StudentId = i;
                stdnt.StudentName = "StdtName" + i;
                stdnt.Age = 10 + i;
                SFD._Student = stdnt;//Assign Student to StudentFull
                seme.Marks = 90 + i;
                seme.Subject = "Maths" + i;
                if(i%2==0)
                {
                    seme.Status = "PASS";
                }
                else
                {
                    seme.Status = "Fail";
                }
                stSem._SemesterOne = seme;//Assign Semester One
                //Semester Two
                seme.Marks = 80 + i;
                seme.Subject = "Maths" + i;
                if (i % 2 == 0)
                {
                    seme.Status = "PASS";
                }
                else
                {
                    seme.Status = "Fail";
                }

                stSem._SemesterTwo = seme;//Assign Semester Two

                //Semester Three
                seme.Marks = 70 + i;
                seme.Subject = "Maths" + i;
                if (i % 2 == 0)
                {
                    seme.Status = "PASS";
                }
                else
                {
                    seme.Status = "Fail";
                }
                stSem._SemesterTwo = seme;//Assign Semester Three
                studentsFullDetails.Add(SFD);
            }

            return studentsFullDetails;
            
        }
        
    }

    //******************************************************//

    //public class ExternalLoginConfirmationViewModel
    //{
    //    [Required]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }
    //}

    //public class ExternalLoginListViewModel
    //{
    //    public string ReturnUrl { get; set; }
    //}

    //public class SendCodeViewModel
    //{
    //    public string SelectedProvider { get; set; }
    //    public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    //    public string ReturnUrl { get; set; }
    //    public bool RememberMe { get; set; }
    //}

    //public class VerifyCodeViewModel
    //{
    //    [Required]
    //    public string Provider { get; set; }

    //    [Required]
    //    [Display(Name = "Code")]
    //    public string Code { get; set; }
    //    public string ReturnUrl { get; set; }

    //    [Display(Name = "Remember this browser?")]
    //    public bool RememberBrowser { get; set; }

    //    public bool RememberMe { get; set; }
    //}

    //public class ForgotViewModel
    //{
    //    [Required]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }
    //}

    //public class LoginViewModel
    //{
    //    [Required]
    //    [Display(Name = "Email")]
    //    [EmailAddress]
    //    public string Email { get; set; }

    //    [Required]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [Display(Name = "Remember me?")]
    //    public bool RememberMe { get; set; }
    //}

    //public class RegisterViewModel
    //{
    //    [Required]
    //    [EmailAddress]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }

    //    [Required]
    //    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm password")]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }
    //}

    //public class ResetPasswordViewModel
    //{
    //    [Required]
    //    [EmailAddress]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }

    //    [Required]
    //    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm password")]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }

    //    public string Code { get; set; }
    //}

    //public class ForgotPasswordViewModel
    //{
    //    [Required]
    //    [EmailAddress]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }
    //}
}