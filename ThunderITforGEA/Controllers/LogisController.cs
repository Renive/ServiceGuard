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
    public class LogisController : Controller
    {
        private Entities db = new Entities();

        // GET: Logis
        public ActionResult Index()
        {
            return View(db.Logi.ToList());
        }

        // GET: Logis/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Logi logi = db.Logi.Find(id);
            if (logi == null)
            {
                return HttpNotFound();
            }
            return View(logi);
        }

        // GET: Logis/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Logis/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,id_uzytkownik,co_zrobil,czas")] Logi logi)
        {
            if (ModelState.IsValid)
            {
                db.Logi.Add(logi);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(logi);
        }

        // GET: Logis/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Logi logi = db.Logi.Find(id);
            if (logi == null)
            {
                return HttpNotFound();
            }
            return View(logi);
        }

        // POST: Logis/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,id_uzytkownik,co_zrobil,czas")] Logi logi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(logi).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(logi);
        }

        // GET: Logis/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Logi logi = db.Logi.Find(id);
            if (logi == null)
            {
                return HttpNotFound();
            }
            return View(logi);
        }

        // POST: Logis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Logi logi = db.Logi.Find(id);
            db.Logi.Remove(logi);
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
