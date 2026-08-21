using System.Collections.Generic;
using Last_Dance_System.Models;

namespace Last_Dance_System.ViewModels
{
    public class DashboardViewModel
    {
        public Registration Registration { get; set; }
        public IEnumerable<Booking> Bookings { get; set; }
        public IEnumerable<Payment> Payments { get; set; }
        public IEnumerable<LessonProgress> LessonProgresses { get; set; }
        public IEnumerable<Notification> Notifications { get; set; }
    }
}