using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ThunderITforGEA.Models;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Hosting;
using System.Net;
namespace ThunderITforGEA.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            Entities daneDoModelu = new Entities();          
            ViewBag.User_Id = User.Identity.GetUserId(); //wszystkie widoki będą znać ID zalogowanego użytkownika
            
            if (User.IsInRole("admin")) 
            {
                var wyniki = daneDoModelu.ServiceGuard.Include(u => u.AspNetUsers).OrderBy(u=>u.Alarmy.FirstOrDefault().wykonal==null).ToList();
                wyniki.Reverse();
                return View("PanelAdmina",wyniki);
            }
            if (User.IsInRole("rolnik"))
            {
                var uzytkownik = daneDoModelu.AspNetUsers.Find(User.Identity.GetUserId());
                var tablicaZalogowanegoUsera = daneDoModelu.AspNetUsers.Find(User.Identity.GetUserId());
                Logi nowyWpis = new Logi();
                nowyWpis.id_uzytkownik = User.Identity.GetUserId();
                nowyWpis.co_zrobil = "Rolnik o loginie " + uzytkownik.UserName + " zalogował się.";
                nowyWpis.czas = DateTime.Now;
                daneDoModelu.Logi.Add(nowyWpis);
                daneDoModelu.SaveChanges();
                return View("PanelRolnika",tablicaZalogowanegoUsera.ServiceGuard);
            }
            if(User.IsInRole("testyCE"))
            {
                return View("testyCE");
            }
            if (User.IsInRole("dealer"))
            {
                var uzytkownik = daneDoModelu.AspNetUsers.Find(User.Identity.GetUserId());
                DealerPrzyWyborzeUrzadzenia dane = new DealerPrzyWyborzeUrzadzenia();
                Logi nowyWpis = new Logi();
                nowyWpis.id_uzytkownik = User.Identity.GetUserId();
                nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " zalogował się.";
                nowyWpis.czas= DateTime.Now;
                daneDoModelu.Logi.Add(nowyWpis);
                List<string> dozwoloneSeriale = new List<string>();
               var wyniki= daneDoModelu.ServiceGuard
             .Where(f => f.firma==uzytkownik.firma).Include(u=>u.AspNetUsers);
               daneDoModelu.SaveChanges();
               return View("wybierzUrzadzenie", wyniki);
            }
            if (User.IsInRole("gea"))
            {
                var wyniki = daneDoModelu.ServiceGuard.Include(u => u.AspNetUsers);
                List<string> listaNazwisk = new List<string>();
                List<string> listaFirm = new List<string>();
                var uzytkownik = daneDoModelu.AspNetUsers.Find(User.Identity.GetUserId());
                Logi nowyWpis = new Logi();
                nowyWpis.id_uzytkownik = User.Identity.GetUserId();
                nowyWpis.co_zrobil = "Pracownik GEA o loginie " + uzytkownik.UserName + " zalogował się.";
                nowyWpis.czas = DateTime.Now;
                daneDoModelu.Logi.Add(nowyWpis);
                daneDoModelu.SaveChanges();
                return View("wybierzUrzadzenieGEA",wyniki);
            }
            else
                return View(); 
        }
      
        public ActionResult doPaneluDealera(string serialNumberSG)
        {
            Entities baza = new Entities();    
            ViewBag.nrServiceGuard = serialNumberSG;
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + "przegląda SG o numerze "+serialNumberSG+ "o " + DateTime.Now.ToString();
            baza.Logi.Add(nowyWpis);
            baza.SaveChanges();
            var serviceGuards = baza.ServiceGuard.Include(s => s.AspNetUsers).Where(f => f.serial_number==serialNumberSG);
            return View("Dealer", serviceGuards);         
        }
        public ActionResult doSGAdmina(string serialNumberSG)
        {
            Entities baza = new Entities();  
            ViewBag.nrServiceGuard = serialNumberSG;
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Admin o loginie " + uzytkownik.UserName + "przegląda SG o numerze " + serialNumberSG + "o " + DateTime.Now.ToString();
            baza.Logi.Add(nowyWpis);
            baza.SaveChanges();
            var serviceGuards = baza.ServiceGuard.Include(s => s.AspNetUsers).Where(f => f.serial_number==serialNumberSG);
            return View("SGAdmina", serviceGuards);           
        }
        
         public ActionResult doPaneluGEA(string serialNumberSG)
        {
            ViewBag.serialNumber = serialNumberSG;        
             Entities baza = new Entities();
             var serviceGuards = baza.ServiceGuard.Include(s => s.Klient);
             var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
             Logi nowyWpis = new Logi();
             nowyWpis.id_uzytkownik = User.Identity.GetUserId();
             nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + "przegląda SG o numerze " + serialNumberSG + "o " + DateTime.Now.ToString();
             baza.Logi.Add(nowyWpis);
             baza.SaveChanges();
            return View("GEA", serviceGuards);
        }
        
         public async Task wyslijEmailDoKlienta()
         {
             var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
             var message = new MailMessage();
             message.To.Add(new MailAddress("name@gmail.com")); 
             message.Subject = "Your email subject";
             message.IsBodyHtml = true;
             using (var smtp = new SmtpClient()) //dane do smtp brane z pliku webconfig (na dole)
             {
                 await smtp.SendMailAsync(message);
             }
         }
     
         public async Task wyslijEmailDoGEA(string htmlWiadomosci, string odbiorca, string temat)
         {
             var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
             var message = new MailMessage();
             message.To.Add(new MailAddress("name@gmail.com"));
             message.Subject = "Your email subject";
             //   message.Body = string.Format(body, model.FromName, model.FromEmail, model.Message);
             message.IsBodyHtml = true;
             using (var smtp = new SmtpClient()) //dane do smtp brane z pliku webconfig (na dole)
             {
                 await smtp.SendMailAsync(message);
             }
         }
         public void doBilingow()
         {
             BilingsController a = new BilingsController();
             a.Index();
         }
         public ActionResult doPaneluAdmina2()
         {
             ServiceGuard a = new ServiceGuard();
             return View("PanelAdmina2",a);
         }
         public ActionResult DodajKlienta()
         {
             Entities db = new Entities();
             var uzytkownik = db.AspNetUsers.Find(User.Identity.GetUserId());
             List<string> listaID = new List<string>();
             foreach (ServiceGuard u in db.ServiceGuard.Where(u => u.firma == uzytkownik.firma).OrderByDescending(u=>u.AspNetUsers.FirstOrDefault().Id ==null))
             {
                 listaID.Add(u.serial_number);
             }
             ViewBag.serialnumber = new SelectList(listaID); //droplist ze wszystkimi numerami seryjnymi SG
            return View("DodajKlienta");
         }
         public ActionResult DodajKlientaAdmin()
         {
             Entities db = new Entities();
             var uzytkownik = db.AspNetUsers.Find(User.Identity.GetUserId());
             List<string> listaID = new List<string>();
             foreach (ServiceGuard u in db.ServiceGuard.OrderByDescending(u => u.AspNetUsers.FirstOrDefault().Id == null))
             {
                 listaID.Add(u.serial_number);
             }
             ViewBag.serialnumber = new SelectList(listaID); //droplist ze wszystkimi numerami seryjnymi SG
             return View("DodajKlienta");
         }
    }
}