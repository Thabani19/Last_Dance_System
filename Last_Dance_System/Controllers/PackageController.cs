using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;
using Stripe;
using Stripe.Checkout;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class PackageController : Controller
    {
        private readonly ApplicationDbContext db =
            new ApplicationDbContext();


        // =========================================================
        // AVAILABLE PACKAGES
        // =========================================================

        public ActionResult Index()
        {
            var packages = db.LessonPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToList();

            return View(packages);
        }


        // =========================================================
        // PACKAGE DETAILS
        // =========================================================

        public ActionResult Details(int id)
        {
            var package = db.LessonPackages
                .FirstOrDefault(p =>
                    p.LessonPackageId == id &&
                    p.IsActive);

            if (package == null)
            {
                return HttpNotFound();
            }

            return View(package);
        }


        // =========================================================
        // PURCHASE PACKAGE - GET
        // =========================================================

        [HttpGet]
        public ActionResult Purchase(int id)
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var package =
                db.LessonPackages
                    .FirstOrDefault(p =>
                        p.LessonPackageId == id &&
                        p.IsActive);

            if (package == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // CHECK EXISTING PACKAGE
            // =====================================================

            var existingPackage =
                db.StudentPackages
                    .FirstOrDefault(sp =>
                        sp.RegistrationId ==
                            student.RegistrationId &&

                        !sp.IsCancelled &&

                        (
                            sp.PaymentStatus == "Pending"

                            ||

                            (
                                sp.PaymentStatus == "Paid" &&
                                sp.IsActive &&
                                sp.LessonsRemaining > 0
                            )
                        ));

            if (existingPackage != null)
            {
                if (existingPackage.PaymentStatus == "Pending")
                {
                    TempData["Error"] =
                        "You already have a pending package purchase. " +
                        "Please complete the payment or remove the pending " +
                        "package before purchasing another package.";
                }
                else
                {
                    TempData["Error"] =
                        "You already have an active package with lesson " +
                        "credits remaining.";
                }

                return RedirectToAction(
                    "MyPackages");
            }

            return View(package);
        }


        // =========================================================
        // PURCHASE PACKAGE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Purchase(
            int id,
            string paymentMethod)
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var package =
                db.LessonPackages
                    .FirstOrDefault(p =>
                        p.LessonPackageId == id &&
                        p.IsActive);

            if (package == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // CHECK EXISTING PACKAGE
            // =====================================================

            var existingPackage =
                db.StudentPackages
                    .FirstOrDefault(sp =>
                        sp.RegistrationId ==
                            student.RegistrationId &&

                        !sp.IsCancelled &&

                        (
                            sp.PaymentStatus == "Pending"

                            ||

                            (
                                sp.PaymentStatus == "Paid" &&
                                sp.IsActive &&
                                sp.LessonsRemaining > 0
                            )
                        ));

            if (existingPackage != null)
            {
                if (existingPackage.PaymentStatus == "Pending")
                {
                    TempData["Error"] =
                        "You already have a pending package purchase. " +
                        "Please complete the payment or remove the pending " +
                        "package before purchasing another package.";
                }
                else
                {
                    TempData["Error"] =
                        "You already have an active package with lesson " +
                        "credits remaining.";
                }

                return RedirectToAction(
                    "MyPackages");
            }


            // =====================================================
            // VALIDATE PAYMENT METHOD
            // =====================================================

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["Error"] =
                    "Please select a payment method.";

                return RedirectToAction(
                    "Purchase",
                    new { id = id });
            }

            string[] allowedPaymentMethods =
            {
                "Card",
                "Cash",
                "EFT"
            };

            if (!allowedPaymentMethods.Contains(paymentMethod))
            {
                TempData["Error"] =
                    "Invalid payment method.";

                return RedirectToAction(
                    "Purchase",
                    new { id = id });
            }


            // =====================================================
            // CREATE PENDING PACKAGE
            // =====================================================

            var studentPackage =
                new StudentPackage
                {
                    RegistrationId =
                        student.RegistrationId,

                    LessonPackageId =
                        package.LessonPackageId,

                    LessonsPurchased =
                        package.NumberOfLessons,

                    LessonsRemaining =
                        package.NumberOfLessons,

                    PaymentMethod =
                        paymentMethod,

                    PaymentStatus =
                        "Pending",

                    PurchaseDate =
                        DateTime.Now,

                    IsActive =
                        false,

                    IsCancelled =
                        false,

                    CancellationDate =
                        null
                };

            db.StudentPackages.Add(
                studentPackage);

            db.SaveChanges();


            // =====================================================
            // CARD → STRIPE
            // =====================================================

            if (paymentMethod == "Card")
            {
                return RedirectToAction(
                    "Pay",
                    new
                    {
                        id =
                            studentPackage.StudentPackageId
                    });
            }


            // =====================================================
            // CASH / EFT
            // =====================================================

            TempData["Success"] =
                "Your package has been created. Please complete your " +
                paymentMethod +
                " payment. Your package will be activated once payment " +
                "is confirmed.";

            return RedirectToAction(
                "MyPackages");
        }


        // =========================================================
        // MY PACKAGES
        // =========================================================

        public ActionResult MyPackages()
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            // =====================================================
            // LOAD PACKAGE + LESSON PACKAGE + PAYMENTS
            // =====================================================

            var packages =
                db.StudentPackages

                    .Include(p =>
                        p.LessonPackage)

                    .Include(p =>
                        p.Payments)

                    .Where(p =>
                        p.RegistrationId ==
                            student.RegistrationId &&

                        !p.IsCancelled &&

                        (
                            (
                                p.PaymentStatus ==
                                    "Paid" &&

                                p.IsActive
                            )

                            ||

                            p.PaymentStatus ==
                                "Pending"
                        ))

                    .OrderByDescending(p =>
                        p.PurchaseDate)

                    .ToList();

            return View(packages);
        }


        // =========================================================
        // DELETE PENDING PACKAGE - GET
        // =========================================================

        [HttpGet]
        public ActionResult DeletePendingPackage(int id)
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var studentPackage =
                db.StudentPackages

                    .Include(p =>
                        p.LessonPackage)

                    .FirstOrDefault(p =>
                        p.StudentPackageId == id &&

                        p.RegistrationId ==
                            student.RegistrationId &&

                        p.PaymentStatus ==
                            "Pending" &&

                        !p.IsCancelled);

            if (studentPackage == null)
            {
                TempData["Error"] =
                    "The pending package could not be found.";

                return RedirectToAction(
                    "MyPackages");
            }

            return View(studentPackage);
        }


        // =========================================================
        // DELETE PENDING PACKAGE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePendingPackageConfirmed(
            int id)
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var studentPackage =
                db.StudentPackages
                    .FirstOrDefault(p =>
                        p.StudentPackageId == id &&

                        p.RegistrationId ==
                            student.RegistrationId &&

                        p.PaymentStatus ==
                            "Pending" &&

                        !p.IsCancelled);

            if (studentPackage == null)
            {
                TempData["Error"] =
                    "The pending package could not be found.";

                return RedirectToAction(
                    "MyPackages");
            }

            db.StudentPackages.Remove(
                studentPackage);

            db.SaveChanges();

            TempData["Success"] =
                "Your pending package has been removed successfully.";

            return RedirectToAction(
                "MyPackages");
        }


        // =========================================================
        // CANCEL PACKAGE - GET
        // =========================================================

        [HttpGet]
        public ActionResult CancelPackage(int id)
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var studentPackage =
                db.StudentPackages

                    .Include(p =>
                        p.LessonPackage)

                    .FirstOrDefault(p =>
                        p.StudentPackageId == id &&

                        p.RegistrationId ==
                            student.RegistrationId &&

                        p.IsActive &&

                        !p.IsCancelled);

            if (studentPackage == null)
            {
                TempData["Error"] =
                    "The package could not be found or has already been cancelled.";

                return RedirectToAction(
                    "MyPackages");
            }

            return View(studentPackage);
        }


        // =========================================================
        // CANCEL PACKAGE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelPackageConfirmed(
            int id)
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var studentPackage =
                db.StudentPackages
                    .FirstOrDefault(p =>
                        p.StudentPackageId == id &&

                        p.RegistrationId ==
                            student.RegistrationId &&

                        p.IsActive &&

                        !p.IsCancelled);

            if (studentPackage == null)
            {
                TempData["Error"] =
                    "The package could not be found or has already been cancelled.";

                return RedirectToAction(
                    "MyPackages");
            }


            // =====================================================
            // FIND BOOKINGS
            // =====================================================

            var bookings =
                db.Bookings

                    .Include(b =>
                        b.LessonSchedule)

                    .Where(b =>
                        b.StudentPackageId ==
                            studentPackage.StudentPackageId &&

                        b.RegistrationId ==
                            student.RegistrationId &&

                        b.Status !=
                            "Cancelled")

                    .ToList();


            // =====================================================
            // CANCEL BOOKINGS
            // =====================================================

            foreach (var booking in bookings)
            {
                booking.Status =
                    "Cancelled";

                if (booking.LessonSchedule != null)
                {
                    bool anotherActiveBooking =
                        db.Bookings.Any(b =>
                            b.BookingId !=
                                booking.BookingId &&

                            b.LessonScheduleId ==
                                booking.LessonScheduleId &&

                            b.Status !=
                                "Cancelled");

                    if (!anotherActiveBooking)
                    {
                        booking.LessonSchedule.Status =
                            "Available";
                    }
                }
            }


            // =====================================================
            // CANCEL PACKAGE
            // =====================================================

            studentPackage.IsActive =
                false;

            studentPackage.IsCancelled =
                true;

            studentPackage.CancellationDate =
                DateTime.Now;

            studentPackage.PaymentStatus =
                "Cancelled";

            db.SaveChanges();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            if (bookings.Any())
            {
                TempData["Success"] =
                    "Your package has been cancelled successfully. " +
                    bookings.Count +
                    " booked lesson(s) were also cancelled.";
            }
            else
            {
                TempData["Success"] =
                    "Your package has been cancelled successfully.";
            }

            return RedirectToAction(
                "MyPackages");
        }


        // =========================================================
        // PAY WITH STRIPE
        // =========================================================

        [HttpGet]
        public ActionResult Pay(int id)
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var studentPackage =
                db.StudentPackages

                    .Include(p =>
                        p.LessonPackage)

                    .FirstOrDefault(p =>
                        p.StudentPackageId == id &&

                        p.RegistrationId ==
                            student.RegistrationId);

            if (studentPackage == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // PACKAGE STATUS
            // =====================================================

            if (studentPackage.IsCancelled)
            {
                TempData["Error"] =
                    "This package has been cancelled and cannot be paid for.";

                return RedirectToAction(
                    "MyPackages");
            }

            if (studentPackage.PaymentStatus == "Paid" &&
                studentPackage.IsActive)
            {
                TempData["Success"] =
                    "This package has already been paid for.";

                return RedirectToAction(
                    "MyPackages");
            }

            if (studentPackage.PaymentStatus != "Pending")
            {
                TempData["Error"] =
                    "This package is not available for payment.";

                return RedirectToAction(
                    "MyPackages");
            }

            if (studentPackage.PaymentMethod != "Card")
            {
                TempData["Error"] =
                    "Stripe checkout is available for card payments only.";

                return RedirectToAction(
                    "MyPackages");
            }


            // =====================================================
            // STRIPE CONFIGURATION
            // =====================================================

            try
            {
                string stripeSecretKey =
                    ConfigurationManager
                        .AppSettings["StripeSecretKey"];

                if (string.IsNullOrWhiteSpace(
                    stripeSecretKey))
                {
                    TempData["Error"] =
                        "Stripe configuration is missing.";

                    return RedirectToAction(
                        "MyPackages");
                }

                StripeConfiguration.ApiKey =
                    stripeSecretKey;


                // =================================================
                // SUCCESS URL
                // =================================================

                string successUrl =
                    Url.Action(
                        "PaymentSuccess",
                        "Package",
                        null,
                        Request.Url.Scheme)

                    + "?session_id={CHECKOUT_SESSION_ID}";


                // =================================================
                // CANCEL URL
                // =================================================

                string cancelUrl =
                    Url.Action(
                        "PaymentCancel",
                        "Package",
                        new
                        {
                            id =
                                studentPackage
                                    .StudentPackageId
                        },
                        Request.Url.Scheme);


                // =================================================
                // STRIPE CHECKOUT OPTIONS
                // =================================================

                var options =
                    new SessionCreateOptions
                    {
                        Mode =
                            "payment",

                        SuccessUrl =
                            successUrl,

                        CancelUrl =
                            cancelUrl,

                        Metadata =
                            new Dictionary<
                                string,
                                string>
                            {
                                {
                                    "StudentPackageId",

                                    studentPackage
                                        .StudentPackageId
                                        .ToString()
                                },

                                {
                                    "RegistrationId",

                                    student.RegistrationId
                                }
                            },

                        LineItems =
                            new List<
                                SessionLineItemOptions>
                            {
                                new SessionLineItemOptions
                                {
                                    Quantity = 1,

                                    PriceData =
                                        new SessionLineItemPriceDataOptions
                                        {
                                            Currency =
                                                "zar",

                                            UnitAmountDecimal =
                                                studentPackage
                                                    .LessonPackage
                                                    .Price * 100,

                                            ProductData =
                                                new SessionLineItemPriceDataProductDataOptions
                                                {
                                                    Name =
                                                        studentPackage
                                                            .LessonPackage
                                                            .PackageName,

                                                    Description =
                                                        studentPackage
                                                            .LessonPackage
                                                            .Description
                                                }
                                        }
                                }
                            }
                    };


                // =================================================
                // CREATE STRIPE SESSION
                // =================================================

                var service =
                    new SessionService();

                Session session =
                    service.Create(options);

                return Redirect(
                    session.Url);
            }
            catch (StripeException ex)
            {
                TempData["Error"] =
                    "Stripe payment error: " +
                    ex.Message;

                return RedirectToAction(
                    "MyPackages");
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while starting the payment.";

                return RedirectToAction(
                    "MyPackages");
            }
        }


        // =========================================================
        // STRIPE PAYMENT SUCCESS
        // =========================================================

        [HttpGet]
        public ActionResult PaymentSuccess(
            string session_id)
        {
            if (string.IsNullOrWhiteSpace(
                session_id))
            {
                TempData["Error"] =
                    "Payment session could not be found.";

                return RedirectToAction(
                    "MyPackages");
            }

            try
            {
                // =================================================
                // STRIPE CONFIGURATION
                // =================================================

                string stripeSecretKey =
                    ConfigurationManager
                        .AppSettings["StripeSecretKey"];

                if (string.IsNullOrWhiteSpace(
                    stripeSecretKey))
                {
                    TempData["Error"] =
                        "Stripe configuration is missing.";

                    return RedirectToAction(
                        "MyPackages");
                }

                StripeConfiguration.ApiKey =
                    stripeSecretKey;


                // =================================================
                // GET STRIPE SESSION
                // =================================================

                var service =
                    new SessionService();

                Session session =
                    service.Get(
                        session_id);


                // =================================================
                // VERIFY PAYMENT STATUS
                // =================================================

                if (session.PaymentStatus !=
                    "paid")
                {
                    TempData["Error"] =
                        "Payment has not been confirmed by Stripe yet.";

                    return RedirectToAction(
                        "MyPackages");
                }


                // =================================================
                // CHECK STRIPE METADATA
                // =================================================

                if (session.Metadata == null ||
                    !session.Metadata.ContainsKey(
                        "StudentPackageId"))
                {
                    TempData["Error"] =
                        "Package information could not be found.";

                    return RedirectToAction(
                        "MyPackages");
                }

                if (session.Metadata == null ||
                    !session.Metadata.ContainsKey(
                        "RegistrationId"))
                {
                    TempData["Error"] =
                        "Student information could not be verified.";

                    return RedirectToAction(
                        "MyPackages");
                }


                // =================================================
                // GET PACKAGE ID
                // =================================================

                int studentPackageId;

                if (!int.TryParse(
                    session.Metadata[
                        "StudentPackageId"],
                    out studentPackageId))
                {
                    TempData["Error"] =
                        "Invalid package payment information.";

                    return RedirectToAction(
                        "MyPackages");
                }


                // =================================================
                // GET CURRENT STUDENT
                // =================================================

                string userId =
                    User.Identity.GetUserId();

                var student =
                    db.Registrations
                        .FirstOrDefault(r =>
                            r.ApplicationUserId ==
                                userId);

                if (student == null)
                {
                    return new HttpStatusCodeResult(
                        403,
                        "Student account not found.");
                }


                // =================================================
                // VERIFY STRIPE STUDENT
                // =================================================

                string stripeRegistrationId =
                    session.Metadata[
                        "RegistrationId"];

                if (stripeRegistrationId !=
                    student.RegistrationId)
                {
                    return new HttpStatusCodeResult(
                        403,
                        "This payment does not belong to the current student.");
                }


                // =================================================
                // GET STUDENT PACKAGE
                // =================================================

                var studentPackage =
                    db.StudentPackages

                        .Include(p =>
                            p.LessonPackage)

                        .FirstOrDefault(p =>
                            p.StudentPackageId ==
                                studentPackageId &&

                            p.RegistrationId ==
                                student.RegistrationId);

                if (studentPackage == null)
                {
                    return HttpNotFound();
                }


                // =================================================
                // CHECK CANCELLATION
                // =================================================

                if (studentPackage.IsCancelled)
                {
                    TempData["Error"] =
                        "This package has already been cancelled.";

                    return RedirectToAction(
                        "MyPackages");
                }


                // =================================================
                // PREVENT DUPLICATE PAYMENT
                // =================================================

                bool paymentAlreadyExists =
                    db.Payments.Any(p =>
                        p.StudentPackageId ==
                            studentPackage.StudentPackageId &&

                        p.PaymentStatus ==
                            "Paid");


                // =================================================
                // CREATE PAYMENT RECORD
                // =================================================

                if (!paymentAlreadyExists)
                {
                    var payment =
                        new Payment
                        {
                            RegistrationId =
                                student.RegistrationId,

                            BookingId =
                                null,

                            StudentPackageId =
                                studentPackage
                                    .StudentPackageId,

                            Amount =
                                studentPackage
                                    .LessonPackage
                                    .Price,

                            PaymentMethod =
                                "Card",

                            PaymentStatus =
                                "Paid",

                            TransactionReference =
                                session.PaymentIntentId ??
                                session.Id,

                            PaymentDate =
                                DateTime.Now,

                            Notes =
                                "Payment successfully processed " +
                                "through Stripe. Stripe Checkout " +
                                "Session: " +
                                session.Id
                        };

                    db.Payments.Add(
                        payment);
                }


                // =================================================
                // ACTIVATE PACKAGE
                // =================================================

                studentPackage.PaymentStatus =
                    "Paid";

                studentPackage.IsActive =
                    true;

                studentPackage.IsCancelled =
                    false;

                studentPackage.CancellationDate =
                    null;


                // =================================================
                // SAVE
                // =================================================

                db.SaveChanges();


                // =================================================
                // SUCCESS MESSAGE
                // =================================================

                TempData["Success"] =
                    "Payment successful! Your lesson package " +
                    "is now active.";

                return RedirectToAction(
                    "MyPackages");
            }
            catch (StripeException ex)
            {
                TempData["Error"] =
                    "Unable to verify the Stripe payment: " +
                    ex.Message;

                return RedirectToAction(
                    "MyPackages");
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while confirming the payment.";

                return RedirectToAction(
                    "MyPackages");
            }
        }


        // =========================================================
        // STRIPE PAYMENT CANCELLED
        // =========================================================

        [HttpGet]
        public ActionResult PaymentCancel(
            int id)
        {
            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var studentPackage =
                db.StudentPackages
                    .FirstOrDefault(p =>
                        p.StudentPackageId == id &&

                        p.RegistrationId ==
                            student.RegistrationId);

            if (studentPackage == null)
            {
                return HttpNotFound();
            }

            TempData["Error"] =
                "Payment was cancelled. Your package is still pending payment.";

            return RedirectToAction(
                "MyPackages");
        }


        // =========================================================
        // PROOF OF PAYMENT
        // =========================================================

        [HttpGet]
        public ActionResult ProofOfPayment(int id)
        {
            // =====================================================
            // GET CURRENT USER
            // =====================================================

            string userId =
                User.Identity.GetUserId();

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            // =====================================================
            // FIND STUDENT PACKAGE
            // =====================================================

            var studentPackage =
                db.StudentPackages

                    .Include(p =>
                        p.LessonPackage)

                    .FirstOrDefault(p =>
                        p.StudentPackageId == id &&

                        p.RegistrationId ==
                            student.RegistrationId);

            if (studentPackage == null)
            {
                TempData["Error"] =
                    "The student package could not be found.";

                return RedirectToAction(
                    "MyPackages");
            }


            // =====================================================
            // FIND PAID PAYMENT FOR THIS PACKAGE
            // =====================================================

            var payment =
                db.Payments

                    .Include(p =>
                        p.StudentPackage)

                    .Include(p =>
                        p.StudentPackage
                            .LessonPackage)

                    .FirstOrDefault(p =>
                        p.StudentPackageId ==
                            studentPackage.StudentPackageId &&

                        p.RegistrationId ==
                            student.RegistrationId &&

                        p.PaymentStatus ==
                            "Paid");

            if (payment == null)
            {
                TempData["Error"] =
                    "No completed payment was found for this package.";

                return RedirectToAction(
                    "MyPackages");
            }


            // =====================================================
            // SHOW PROOF OF PAYMENT
            // =====================================================

            return View(payment);
        }


        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}