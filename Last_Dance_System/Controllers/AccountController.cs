using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;


        // =========================================================
        // CONSTRUCTORS
        // =========================================================

        public AccountController()
        {
        }

        public AccountController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }


        // =========================================================
        // SIGN IN MANAGER
        // =========================================================

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ??
                    HttpContext
                        .GetOwinContext()
                        .Get<ApplicationSignInManager>();
            }

            private set
            {
                _signInManager = value;
            }
        }


        // =========================================================
        // USER MANAGER
        // =========================================================

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ??
                    HttpContext
                        .GetOwinContext()
                        .GetUserManager<ApplicationUserManager>();
            }

            private set
            {
                _userManager = value;
            }
        }


        // =========================================================
        // LOGIN - GET
        // =========================================================

        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // =========================================================
        // LOGIN - POST
        // =========================================================

        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(
            LoginViewModel model,
            string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =====================================================
            // ATTEMPT LOGIN
            // =====================================================

            var result =
                await SignInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    shouldLockout: false);


            switch (result)
            {
                // =================================================
                // LOGIN SUCCESSFUL
                // =================================================

                case SignInStatus.Success:

                    // ---------------------------------------------
                    // Find the Identity user
                    // ---------------------------------------------

                    var user =
                        await UserManager.FindByEmailAsync(
                            model.Email);

                    if (user == null)
                    {
                        ModelState.AddModelError(
                            "",
                            "User account could not be found.");

                        return View(model);
                    }


                    // ---------------------------------------------
                    // INSTRUCTOR
                    // ---------------------------------------------

                    if (await UserManager.IsInRoleAsync(
                        user.Id,
                        "Instructor"))
                    {
                        return RedirectToAction(
                            "Index",
                            "InstructorDashboard");
                    }


                    // ---------------------------------------------
                    // ADMINISTRATOR
                    // ---------------------------------------------

                    if (await UserManager.IsInRoleAsync(
                        user.Id,
                        "Administrator"))
                    {
                        return RedirectToAction(
                            "Index",
                            "Dashboard");
                    }


                    // ---------------------------------------------
                    // STUDENT
                    // ---------------------------------------------

                    if (await UserManager.IsInRoleAsync(
                        user.Id,
                        "Student"))
                    {
                        return RedirectToAction(
                            "Index",
                            "Dashboard");
                    }


                    // ---------------------------------------------
                    // FALLBACK
                    // ---------------------------------------------

                    return RedirectToLocal(returnUrl);


                // =================================================
                // ACCOUNT LOCKED
                // =================================================

                case SignInStatus.LockedOut:

                    return View("Lockout");


                // =================================================
                // TWO-FACTOR AUTHENTICATION
                // =================================================

                case SignInStatus.RequiresVerification:

                    return RedirectToAction(
                        "SendCode",
                        new
                        {
                            ReturnUrl = returnUrl,
                            RememberMe = model.RememberMe
                        });


                // =================================================
                // LOGIN FAILURE
                // =================================================

                case SignInStatus.Failure:

                default:

                    ModelState.AddModelError(
                        "",
                        "Invalid login attempt.");

                    return View(model);
            }
        }


        // =========================================================
        // VERIFY CODE - GET
        // =========================================================

        // GET: /Account/VerifyCode

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(
            string provider,
            string returnUrl,
            bool rememberMe)
        {
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }

            return View(
                new VerifyCodeViewModel
                {
                    Provider = provider,
                    ReturnUrl = returnUrl,
                    RememberMe = rememberMe
                });
        }


        // =========================================================
        // VERIFY CODE - POST
        // =========================================================

        // POST: /Account/VerifyCode

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(
            VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var result =
                await SignInManager.TwoFactorSignInAsync(
                    model.Provider,
                    model.Code,
                    isPersistent: model.RememberMe,
                    rememberBrowser: model.RememberBrowser);


            switch (result)
            {
                case SignInStatus.Success:

                    return RedirectToLocal(
                        model.ReturnUrl);


                case SignInStatus.LockedOut:

                    return View("Lockout");


                case SignInStatus.Failure:

                default:

                    ModelState.AddModelError(
                        "",
                        "Invalid code.");

                    return View(model);
            }
        }


        // =========================================================
        // REGISTER - GET
        // =========================================================

        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }


        // =========================================================
        // REGISTER - POST
        // =========================================================

        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =====================================================
            // CHECK ID NUMBER
            // =====================================================

            using (var context =
                new ApplicationDbContext())
            {
                var existingRegistration =
                    context.Registrations
                        .FirstOrDefault(
                            r => r.RegistrationId ==
                                 model.RegistrationId);

                if (existingRegistration != null)
                {
                    ModelState.AddModelError(
                        "RegistrationId",
                        "This ID number is already registered.");

                    return View(model);
                }
            }


            // =====================================================
            // CHECK EMAIL
            // =====================================================

            var existingUser =
                await UserManager.FindByEmailAsync(
                    model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email address is already registered.");

                return View(model);
            }


            // =====================================================
            // CREATE IDENTITY ACCOUNT
            // =====================================================

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };


            var result =
                await UserManager.CreateAsync(
                    user,
                    model.Password);


            if (!result.Succeeded)
            {
                AddErrors(result);

                return View(model);
            }


            // =====================================================
            // ASSIGN STUDENT ROLE
            // =====================================================

            var roleResult =
                await UserManager.AddToRoleAsync(
                    user.Id,
                    "Student");


            if (!roleResult.Succeeded)
            {
                await UserManager.DeleteAsync(user);

                AddErrors(roleResult);

                return View(model);
            }


            // =====================================================
            // CREATE REGISTRATION RECORD
            // =====================================================

            using (var context =
                new ApplicationDbContext())
            {
                var registration =
                    new Registration
                    {
                        RegistrationId =
                            model.RegistrationId,

                        FirstName =
                            model.FirstName,

                        LastName =
                            model.LastName,

                        Gender =
                            model.Gender,

                        DateOfBirth =
                            model.DateOfBirth,

                        Phone =
                            model.Phone,

                        Email =
                            model.Email,

                        Address =
                            model.Address,

                        RegistrationDate =
                            DateTime.Now,

                        // Link Registration to Identity
                        ApplicationUserId =
                            user.Id
                    };


                context.Registrations.Add(
                    registration);

                await context.SaveChangesAsync();
            }


            // =====================================================
            // LOG STUDENT IN
            // =====================================================

            await SignInManager.SignInAsync(
                user,
                isPersistent: false,
                rememberBrowser: false);


            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================================
        // CONFIRM EMAIL
        // =========================================================

        // GET: /Account/ConfirmEmail

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(
            string userId,
            string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }


            var result =
                await UserManager.ConfirmEmailAsync(
                    userId,
                    code);


            return View(
                result.Succeeded
                    ? "ConfirmEmail"
                    : "Error");
        }


        // =========================================================
        // FORGOT PASSWORD - GET
        // =========================================================

        // GET: /Account/ForgotPassword

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        // =========================================================
        // FORGOT PASSWORD - POST
        // =========================================================

        // POST: /Account/ForgotPassword

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user =
                    await UserManager.FindByNameAsync(
                        model.Email);


                if (user == null ||
                    !(await UserManager.IsEmailConfirmedAsync(
                        user.Id)))
                {
                    return View(
                        "ForgotPasswordConfirmation");
                }

                // Password reset email can be added here.
            }


            return View(model);
        }


        // =========================================================
        // FORGOT PASSWORD CONFIRMATION
        // =========================================================

        // GET: /Account/ForgotPasswordConfirmation

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        // =========================================================
        // RESET PASSWORD - GET
        // =========================================================

        // GET: /Account/ResetPassword

        [AllowAnonymous]
        public ActionResult ResetPassword(
            string code)
        {
            return code == null
                ? View("Error")
                : View();
        }


        // =========================================================
        // RESET PASSWORD - POST
        // =========================================================

        // POST: /Account/ResetPassword

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var user =
                await UserManager.FindByNameAsync(
                    model.Email);


            if (user == null)
            {
                return RedirectToAction(
                    "ResetPasswordConfirmation",
                    "Account");
            }


            var result =
                await UserManager.ResetPasswordAsync(
                    user.Id,
                    model.Code,
                    model.Password);


            if (result.Succeeded)
            {
                return RedirectToAction(
                    "ResetPasswordConfirmation",
                    "Account");
            }


            AddErrors(result);

            return View(model);
        }


        // =========================================================
        // RESET PASSWORD CONFIRMATION
        // =========================================================

        // GET: /Account/ResetPasswordConfirmation

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        // =========================================================
        // EXTERNAL LOGIN
        // =========================================================

        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(
            string provider,
            string returnUrl)
        {
            return new ChallengeResult(
                provider,
                Url.Action(
                    "ExternalLoginCallback",
                    "Account",
                    new
                    {
                        ReturnUrl = returnUrl
                    }));
        }


        // =========================================================
        // SEND CODE - GET
        // =========================================================

        // GET: /Account/SendCode

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(
            string returnUrl,
            bool rememberMe)
        {
            var userId =
                await SignInManager.GetVerifiedUserIdAsync();


            if (userId == null)
            {
                return View("Error");
            }


            var userFactors =
                await UserManager.GetValidTwoFactorProvidersAsync(
                    userId);


            var factorOptions =
                userFactors
                    .Select(
                        purpose =>
                            new SelectListItem
                            {
                                Text = purpose,
                                Value = purpose
                            })
                    .ToList();


            return View(
                new SendCodeViewModel
                {
                    Providers = factorOptions,
                    ReturnUrl = returnUrl,
                    RememberMe = rememberMe
                });
        }


        // =========================================================
        // SEND CODE - POST
        // =========================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(
            SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }


            if (!await SignInManager.SendTwoFactorCodeAsync(
                model.SelectedProvider))
            {
                return View("Error");
            }


            return RedirectToAction(
                "VerifyCode",
                new
                {
                    Provider =
                        model.SelectedProvider,

                    ReturnUrl =
                        model.ReturnUrl,

                    RememberMe =
                        model.RememberMe
                });
        }


        // =========================================================
        // EXTERNAL LOGIN CALLBACK
        // =========================================================

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(
            string returnUrl)
        {
            var loginInfo =
                await AuthenticationManager
                    .GetExternalLoginInfoAsync();


            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }


            var result =
                await SignInManager.ExternalSignInAsync(
                    loginInfo,
                    isPersistent: false);


            switch (result)
            {
                case SignInStatus.Success:

                    return RedirectToLocal(returnUrl);


                case SignInStatus.LockedOut:

                    return View("Lockout");


                case SignInStatus.RequiresVerification:

                    return RedirectToAction(
                        "SendCode",
                        new
                        {
                            ReturnUrl = returnUrl,
                            RememberMe = false
                        });


                case SignInStatus.Failure:

                default:

                    ViewBag.ReturnUrl =
                        returnUrl;

                    ViewBag.LoginProvider =
                        loginInfo.Login.LoginProvider;

                    return View(
                        "ExternalLoginConfirmation",
                        new ExternalLoginConfirmationViewModel
                        {
                            Email = loginInfo.Email
                        });
            }
        }


        // =========================================================
        // LOG OFF
        // =========================================================

        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie);

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================================
        // EXTERNAL LOGIN FAILURE
        // =========================================================

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }


        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }


                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }


            base.Dispose(disposing);
        }


        // =========================================================
        // HELPERS
        // =========================================================

        #region Helpers


        private const string XsrfKey = "XsrfId";


        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext
                    .GetOwinContext()
                    .Authentication;
            }
        }


        private void AddErrors(
            IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error);
            }
        }


        private ActionResult RedirectToLocal(
            string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }


            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================================
        // CHALLENGE RESULT
        // =========================================================

        internal class ChallengeResult
            : HttpUnauthorizedResult
        {
            public ChallengeResult(
                string provider,
                string redirectUri)
                : this(
                    provider,
                    redirectUri,
                    null)
            {
            }


            public ChallengeResult(
                string provider,
                string redirectUri,
                string userId)
            {
                LoginProvider =
                    provider;

                RedirectUri =
                    redirectUri;

                UserId =
                    userId;
            }


            public string LoginProvider
            {
                get;
                set;
            }


            public string RedirectUri
            {
                get;
                set;
            }


            public string UserId
            {
                get;
                set;
            }


            public override void ExecuteResult(
                ControllerContext context)
            {
                var properties =
                    new AuthenticationProperties
                    {
                        RedirectUri =
                            RedirectUri
                    };


                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] =
                        UserId;
                }


                context.HttpContext
                    .GetOwinContext()
                    .Authentication
                    .Challenge(
                        properties,
                        LoginProvider);
            }
        }


        #endregion
    }
}