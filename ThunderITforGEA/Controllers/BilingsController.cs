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
    public class BilingsController : Controller
    {
        private Entities db = new Entities();

        // GET: Bilings
        public ActionResult Index()
        {
            var bilings = db.Biling.Include(b => b.ServiceGuard);
            return View(bilings.ToList());
        }

        // GET: Bilings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Biling biling = db.Biling.Find(id);
            if (biling == null)
            {
                return HttpNotFound();
            }
            return View(biling);
        }

        // GET: Bilings/Create
        public ActionResult Create()
        {
            ViewBag.ServiceGuard_id_sg = new SelectList(db.ServiceGuard, "serial_number", "nr_karty");
            return View();
        }

        // POST: Bilings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_b,data,tresc,login,ServiceGuard_id_sg")] Biling biling)
        {
            if (ModelState.IsValid)
            {
                db.Biling.Add(biling);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ServiceGuard_id_sg = new SelectList(db.ServiceGuard, "serial_number", "nr_karty", biling.ServiceGuard_id_sg);
            return View(biling);
        }

        // GET: Bilings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Biling biling = db.Biling.Find(id);
            if (biling == null)
            {
                return HttpNotFound();
            }
            ViewBag.ServiceGuard_id_sg = new SelectList(db.ServiceGuard, "serial_number", "nr_karty", biling.ServiceGuard_id_sg);
            return View(biling);
        }

        // POST: Bilings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_b,data,tresc,login,ServiceGuard_id_sg")] Biling biling)
        {
            if (ModelState.IsValid)
            {
                db.Entry(biling).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ServiceGuard_id_sg = new SelectList(db.ServiceGuard, "serial_number", "nr_karty", biling.ServiceGuard_id_sg);
            return View(biling);
        }

        // GET: Bilings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Biling biling = db.Biling.Find(id);
            if (biling == null)
            {
                return HttpNotFound();
            }
            return View(biling);
        }

        // POST: Bilings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Biling biling = db.Biling.Find(id);
            db.Biling.Remove(biling);
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
