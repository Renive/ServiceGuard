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
    public class ServiceManagersController : Controller
    {
        private Entities db = new Entities();

        // GET: ServiceManagers
        public ActionResult Index()
        {
            return View(db.ServiceManager.ToList());
        }

        // GET: ServiceManagers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceManager serviceManager = db.ServiceManager.Find(id);
            if (serviceManager == null)
            {
                return HttpNotFound();
            }
            return View(serviceManager);
        }

        // GET: ServiceManagers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ServiceManagers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_sm,nr_tel,serial,desc")] ServiceManager serviceManager)
        {
            if (ModelState.IsValid)
            {
                db.ServiceManager.Add(serviceManager);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(serviceManager);
        }

        // GET: ServiceManagers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceManager serviceManager = db.ServiceManager.Find(id);
            if (serviceManager == null)
            {
                return HttpNotFound();
            }
            return View(serviceManager);
        }

        // POST: ServiceManagers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_sm,nr_tel,serial,desc")] ServiceManager serviceManager)
        {
            if (ModelState.IsValid)
            {
                db.Entry(serviceManager).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(serviceManager);
        }

        // GET: ServiceManagers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceManager serviceManager = db.ServiceManager.Find(id);
            if (serviceManager == null)
            {
                return HttpNotFound();
            }
            return View(serviceManager);
        }

        // POST: ServiceManagers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ServiceManager serviceManager = db.ServiceManager.Find(id);
            db.ServiceManager.Remove(serviceManager);
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
