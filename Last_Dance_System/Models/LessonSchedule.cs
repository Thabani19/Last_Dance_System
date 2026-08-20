using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class LessonSchedule
    {
        [Key]
        public int LessonScheduleId { get; set; }

        [Required]
        public DateTime LessonDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // Lesson Type
        [Required]
        public int LessonTypeId { get; set; }

        [ForeignKey("LessonTypeId")]
        public virtual LessonType LessonType { get; set; }

        // Instructor
        [Required]
        public int InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }

        // Vehicle
        [Required]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
    }
}