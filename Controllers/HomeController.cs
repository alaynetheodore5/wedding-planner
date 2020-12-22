using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context { get; set; }
        
        public HomeController(MyContext context)
        {
            _context = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {

            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User reg)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(u => u.Email == reg.Email))
                {
                    ModelState.AddModelError("Email", "Email Address is already taken!");
                    return View("Index");
                }
                else
                {
                    //These two lines will hash our password for us.
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    reg.Password = Hasher.HashPassword(reg, reg.Password);
                    _context.Users.Add(reg);
                    _context.SaveChanges();
                    HttpContext.Session.SetInt32("userId", reg.UserId);
                    return Redirect("/success");
                } 
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult LoginUser(LoginUser log)
        {
            if(ModelState.IsValid)
            {
                var userInDb = _context.Users.FirstOrDefault( u => u.Email == log.LoginEmail);
                if( userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Password and/or Email.");
                    return View("Index");
                }
                else
                {
                   //These two lines will compare our hashed passwords.
                    var hash = new PasswordHasher<LoginUser>();
                    var result = hash.VerifyHashedPassword(log, userInDb.Password, log.LoginPassword);
                    //Result will either be 0 or 1.
                    if(result == 0)
                    {
                        //Handle rejection and send them back to the form.
                        ModelState.AddModelError("LoginEmail", "Invalid Password and/or Email.");
                        return View("Index");
                    }             
                    HttpContext.Session.SetInt32("userId", userInDb.UserId);
                    return Redirect("/success");
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("success")]
        public IActionResult Success(int userId)
        {
            var userInDb = _context.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("userId"));
            if(userInDb == null)
            {
                return View("Index");
            }
            ViewBag.Wedding = _context.Weddings.ToList();
            // User userInDb = _context.Users
            //     .Include( u => u.PlannedWeddings )
            //     .FirstOrDefault( u => u.UserId == userId);
            // ViewBag.myVacays = userInDb.PlannedWeddings
            //     .OrderBy(w => w.WeddingDate).ToList();
            ViewBag.User = userInDb;
            List<Wedding> AllAttendees = _context.Weddings
                                            .Include( v => v.WeddingAttendees  )
                                            .ThenInclude( a => a.RSVPd )
                                            .Include( v => v.Planner )
                                            .ToList();
            return View("Dashboard");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }
        
        [HttpGet("newwedding")]
        public IActionResult NewWedding()
        {
            return View("CreateWedding");
        }

        [HttpPost("/createwedding")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if(ModelState.IsValid)
            {
                int? userId = HttpContext.Session.GetInt32("userId");
                if(userId == null)
                {
                    return Redirect("/");
                }
                List<Wedding> PlannedWeddings = _context.Users
                    .Include(u => u.PlannedWeddings)
                    .FirstOrDefault(u => u.UserId == (int) userId)
                    .PlannedWeddings.ToList();
                if(IsAlreadyBooked(newWedding, PlannedWeddings))
                {
                    ModelState.AddModelError("StartDate", "You are already attending a wedding at this time");
                    return Redirect("/createwedding");
                }
                newWedding.UserId = (int)userId;
                _context.Weddings.Add(newWedding);
                _context.SaveChanges();
                return Redirect("/success");
                // return Redirect($"/wedding/{userId}");
            }
            else
            {
                return View("CreateWedding");
            }
        }

        public static bool IsAlreadyBooked(Wedding newWedding, List<Wedding> weddings)
        {
            DateTime date = newWedding.WeddingDate;
            DateTime newdate = newWedding.WeddingDate;
            foreach(var w in weddings)
            {
                DateTime start = w.WeddingDate;
                // no part of the date or newdate can overlap this block of time
                if(start == newdate )
                {
                    return true;
                }
            }
            return false;
        }

        [HttpGet("rsvp/{weddingId}")]
        public IActionResult RSVP( int weddingId)
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            Association rsvp = new Association();
            rsvp.WeddingId = weddingId;
            rsvp.UserId = (int)userId;
            _context.Associations.Add(rsvp);
            _context.SaveChanges();
            return Redirect("/success");
        }

        [HttpGet("leave/{weddingId}")]
        public IActionResult UnRSVP( int weddingId)
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            Association unrsvp =  _context.Associations.FirstOrDefault( a => a.WeddingId == weddingId && a.UserId == userId );
            _context.Associations.Remove(unrsvp);
            _context.SaveChanges();
            return Redirect("/success");
        }

        [HttpGet("delete/{weddingID}")]
        public IActionResult DeleteWedding(Wedding wedding)
        {
            _context.Weddings.Remove(wedding);
            _context.SaveChanges();
            return Redirect("/success");
        }

        [HttpGet("show/{weddingID}")]
        public IActionResult ShowWedding(int weddingId)
        {
            Wedding wedding = _context.Weddings
                                        .Include( v => v.WeddingAttendees )
                                        .ThenInclude( v => v.Guest )
                                        .FirstOrDefault( v => v.WeddingId == weddingId);
            // ViewBag.Wedding = _context.Weddings.ToList();
            // List<Wedding> AllAttendees = _context.Weddings
            //                                 .Include( v => v.WeddingAttendees  )
            //                                 .ThenInclude( a => a.RSVPd )
            //                                 .Include( v => v.Planner )
            //                                 .ToList();
            return View("ShowWedding", wedding);
        }
    }
}
