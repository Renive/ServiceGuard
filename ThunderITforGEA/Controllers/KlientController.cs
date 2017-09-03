using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ThunderITforGEA.Models;
using Microsoft.AspNet.Identity;


namespace ThunderITforGEA.Controllers
{
    public class KlientController : Controller
    {
        private Entities db = new Entities();

        // GET: Klient
        public ActionResult Index()
        {
            var klient = db.Klient.Include(k => k.ServiceGuard);
            return View(klient.ToList());
        }

        // GET: Klient/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Klient klient = db.Klient.Find(id);
            if (klient == null)
            {
                return HttpNotFound();
            }
            return View(klient);
        }

        // GET: Klient/Create
        public ActionResult Create()
        {
             List<string> listaID = new List<string>();
            foreach(ServiceGuard u in db.ServiceGuard)
            {
               listaID.Add(u.serial_number);
            }
            ViewBag.Id_k_SG = new SelectList(listaID); //droplist ze wszystkimi numerami seryjnymi SG
           
      
            return View();
        }
        public ActionResult Dodaj()
        {
            List<string> listaID = new List<string>();
            foreach (ServiceGuard u in db.ServiceGuard)
            {
                listaID.Add(u.serial_number);
            }
            ViewBag.Id_k_SG = new SelectList(listaID); //droplist ze wszystkimi numerami seryjnymi SG
            Entities baza = new Entities();
            AspNetUsers aktualnyuzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            ViewBag.firmaUzytkownika = aktualnyuzytkownik.firma;
            return View();
        }

        // POST: Klient/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Dodaj([Bind(Include = "Id_k,imie,nazwisko,firma,adres,miasto,kraj,telefon,Email,Id_k_SG,ilosc_krow,udoj_per_day,wydajnosc_stada,manager_fs,techniczny,manager_sprzedazy,typ_obory,obora_dlugosc,obora_szerokosc,obora_rokbudowy,rodzaj_sciolki,typ_obory2")] Klient klient)
        {
            if (ModelState.IsValid)
            {
                klient.Id_k = Guid.NewGuid().ToString();
                db.Klient.Add(klient);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Id_k_SG = new SelectList(db.ServiceGuard, "serial_number", "nr_karty", klient.Id_k_SG);
            return View(klient);
        }
        // POST: Klient/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_k,imie,nazwisko,firma,adres,miasto,kraj,telefon,Email,Id_k_SG,ilosc_krow,udoj_per_day,wydajnosc_stada,manager_fs,techniczny,manager_sprzedazy,typ_obory,obora_dlugosc,obora_szerokosc,obora_rokbudowy,rodzaj_sciolki,typ_obory2")] Klient klient)
        {
            if (ModelState.IsValid)
            {
                klient.Id_k = Guid.NewGuid().ToString();
                db.Klient.Add(klient);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Id_k_SG = new SelectList(db.ServiceGuard, "serial_number", "nr_karty", klient.Id_k_SG);
            return View(klient);
        }

        // GET: Klient/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Klient klient = db.Klient.Find(id);
            if (klient == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id_k_SG = new SelectList(db.ServiceGuard, "serial_number", "nr_karty", klient.Id_k_SG);
            return View(klient);
        }

        // POST: Klient/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_k,imie,nazwisko,firma,adres,miasto,kraj,telefon,Email,Id_k_SG,ilosc_krow,udoj_per_day,wydajnosc_stada,manager_fs,techniczny,manager_sprzedazy,typ_obory,obora_dlugosc,obora_szerokosc,obora_rokbudowy,rodzaj_sciolki,typ_obory2")] Klient klient)
        {
            if (ModelState.IsValid)
            {
                db.Entry(klient).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id_k_SG = new SelectList(db.ServiceGuard, "serial_number", "nr_karty", klient.Id_k_SG);
            return View(klient);
        }

        // GET: Klient/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Klient klient = db.Klient.Find(id);
            if (klient == null)
            {
                return HttpNotFound();
            }
            return View(klient);
        }

        // POST: Klient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Klient klient = db.Klient.Find(id);
            db.Klient.Remove(klient);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
