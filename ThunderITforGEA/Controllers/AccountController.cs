using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using ThunderITforGEA.Models;
using System.Net.Mail;
using System.Web.Hosting;
using System.Net;

namespace ThunderITForGEA.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
       
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.login, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = new ApplicationUser();
                user.Email = model.Email;
                user.UserName = model.nazwisko + "." + model.imie.Substring(0, 2); //todo jakies zabezpieczenie przed imionami na 1 litere
                user.imie = model.imie;
                user.nazwisko = model.nazwisko;
                user.firma = model.firma;
                user.adres = model.adres;
                user.miasto = model.miasto;
                user.kraj = model.kraj;
                user.telefon = model.telefon;
                user.login = user.UserName; //dla pewności i kompatybilności z wygenerowanym email
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                 //   await SignInAsync(user, isPersistent: false);
                   await wyslijEmailDealer(user, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DodajKlienta(RegisterKlient model)
        {
            if (ModelState.IsValid)
            {

                var user = new ApplicationUser();
                user.Email = model.Email;
                user.UserName = model.nazwisko + "." + model.imie.Substring(0, 2); //todo jakies zabezpieczenie przed imionami na 1 litere
                user.imie = model.imie;
                user.nazwisko = model.nazwisko;
                user.firma = model.firma;
                user.adres = model.adres;
                user.miasto = model.miasto;
                user.kraj = model.kraj;
                user.telefon = model.telefon;
                user.login = user.UserName; //dla pewności i kompatyvbiności z wygenerowanym email
               
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {                                      
                    UserManager.AddToRole(user.Id, "rolnik");
                    Entities baza = new Entities();
                    var uzytkownik = baza.AspNetUsers.Find(user.Id);
                    var SG = baza.ServiceGuard.Find(model.serialnumber);
                    uzytkownik.ServiceGuard.Add(SG);
                    //   await SignInAsync(user, isPersistent: false);
                    await wyslijEmailRolnik(user, model.Password);
                    //var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
                    Logi nowyWpis = new Logi();
                    nowyWpis.id_uzytkownik = User.Identity.GetUserId();
                    nowyWpis.co_zrobil = "Został dodany klient o loginie " + user.UserName + "do SG o numerze " + model.serialnumber + "o " + DateTime.Now.ToString();
                    baza.Logi.Add(nowyWpis);
                    baza.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }


        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

    
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
     
        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }
        public async Task wyslijEmailDealer(ApplicationUser user, string haslo)
        {

            string calyHTML = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Content").ToString() + "/emailWzor.html");
            var message = new MailMessage();
            message.To.Add(new MailAddress(user.Email));  
            message.From = new MailAddress("powiadomienie@geaserviceguard.com");  
            message.Subject = "Witamy w serwisie GEA-ServiceGuard!";
            calyHTML = calyHTML.Replace("###LOGIN###", user.UserName);
            calyHTML = calyHTML.Replace("###HASLO###", haslo);
            calyHTML = calyHTML.Replace("###DANEDEALERA###", "");
            message.Body = calyHTML;//string.Format(body, model.FromName, model.FromEmail, model.Message);
            message.IsBodyHtml = true;
            message.Attachments.Add(new Attachment(HostingEnvironment.MapPath("~/Content").ToString() + "/Instrukcja obsługi.pdf".ToString()));
          
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "admin@geaserviceguard.com",
               // UserName = "admin",                  
                    Password = "KuVaFus7a"
                };
                smtp.Credentials = credential;
                smtp.Host = "ssl0.ovh.net";
                smtp.Port = 25;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }


        }
        public async Task wyslijEmailRolnik(ApplicationUser user, string haslo)
        {
            string calyHTML = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Content").ToString() + "/emailWzor.html");
            var message = new MailMessage();
            message.To.Add(new MailAddress(user.Email));  // replace with valid value 
            message.From = new MailAddress("powiadomienie@geaserviceguard.com");  // replace with valid value
            message.Subject = "Witamy w serwisie GEA-ServiceGuard!";
            calyHTML = calyHTML.Replace("###LOGIN###", user.UserName);
            calyHTML = calyHTML.Replace("###HASLO###", haslo);
            Entities baza = new Entities();
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            string daneDealera = "Twój dealer to" + uzytkownik.imie + " " + uzytkownik.nazwisko+" z firmy " + uzytkownik.firma +".";
            calyHTML = calyHTML.Replace("###DANEDEALERA###", daneDealera);
            message.Body = calyHTML;//string.Format(body, model.FromName, model.FromEmail, model.Message);
            message.IsBodyHtml = true;
            message.Attachments.Add(new Attachment(HostingEnvironment.MapPath("~/Content").ToString() + "/Instrukcja obsługi.pdf".ToString()));

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "admin@geaserviceguard.com",
                    // UserName = "admin",                  
                    Password = "KuVaFus7a"
                };
                smtp.Credentials = credential;
                smtp.Host = "ssl0.ovh.net";
                smtp.Port = 25;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }


        }
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}