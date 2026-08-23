using System.Linq;
using System.Web.Mvc;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    //Authorize] For testing since i can't login
    public class NotificationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Notifications
        public ActionResult Index()
        {
            // Retrieve all global notifications ordered by newest first
            var notifications = db.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
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