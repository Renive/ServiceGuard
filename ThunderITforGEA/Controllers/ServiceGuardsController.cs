using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ThunderITforGEA.Models;

namespace ThunderITforGEA.Controllers
{
    public class ServiceGuardsController : Controller
    {
        private Entities db = new Entities();

        // GET: ServiceGuards
        public ActionResult Index()
        {
            var serviceGuards = db.ServiceGuard.Include(s => s.ServiceManager);
            return View(serviceGuards.ToList());
        }

        // GET: ServiceGuards/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceGuard serviceGuard = db.ServiceGuard.Find(id);
            if (serviceGuard == null)
            {
                return HttpNotFound();
            }
            return View(serviceGuard);
        }

        // GET: ServiceGuards/Create
        public ActionResult Create()
        {
            ViewBag.ServiceManager_id_sm = new SelectList(db.ServiceManager, "Id_sm", "nr_tel");
            return View();
        }

        // POST: ServiceGuards/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "serial_number,nr_karty,pin,nr_tel,lokalizacja,aktualny_czas,przedzial_serwisowy,do_serwisu,ostatni_serwis,ostatni_alarm,licznik_sms,ServiceManager_id_sm,user_id_u,jezyk,firma,nrSuperVisor,nrServiceCenter")] ServiceGuard serviceGuard)
        {
            if (ModelState.IsValid)
            {
                db.ServiceGuard.Add(serviceGuard);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ServiceManager_id_sm = new SelectList(db.ServiceManager, "Id_sm", "nr_tel", serviceGuard.ServiceManager_id_sm);
            return View(serviceGuard);
        }

        // GET: ServiceGuards/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceGuard serviceGuard = db.ServiceGuard.Find(id);
            if (serviceGuard == null)
            {
                return HttpNotFound();
            }
            ViewBag.ServiceManager_id_sm = new SelectList(db.ServiceManager, "Id_sm", "nr_tel", serviceGuard.ServiceManager_id_sm);
            return View(serviceGuard);
        }

        // POST: ServiceGuards/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "serial_number,nr_karty,pin,nr_tel,lokalizacja,aktualny_czas,przedzial_serwisowy,do_serwisu,ostatni_serwis,ostatni_alarm,licznik_sms,ServiceManager_id_sm,user_id_u,jezyk,firma,nrSuperVisor,nrServiceCenter")] ServiceGuard serviceGuard)
        {
            if (ModelState.IsValid)
            {
                db.Entry(serviceGuard).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ServiceManager_id_sm = new SelectList(db.ServiceManager, "Id_sm", "nr_tel", serviceGuard.ServiceManager_id_sm);
            return View(serviceGuard);
        }

        // GET: ServiceGuards/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceGuard serviceGuard = db.ServiceGuard.Find(id);
            if (serviceGuard == null)
            {
                return HttpNotFound();
            }
            return View(serviceGuard);
        }

        // POST: ServiceGuards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ServiceGuard serviceGuard = db.ServiceGuard.Find(id);
            db.ServiceGuard.Remove(serviceGuard);
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
