using System.Collections.Generic;
using Last_Dance_System.Models;

namespace Last_Dance_System.ViewModels
{
    public class BookingCreateViewModel
    {
        public int LessonTypeId { get; set; }
        public IEnumerable<LessonSchedule> AvailableSchedules { get; set; }
    }
}