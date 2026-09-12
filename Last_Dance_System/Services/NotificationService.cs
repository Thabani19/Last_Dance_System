using System;
using System.Linq;
using Last_Dance_System.Models;

namespace Last_Dance_System.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext db;

        public NotificationService(ApplicationDbContext context)
        {
            db = context;
        }


        // =========================================================
        // NOTIFY STUDENT
        // =========================================================

        public void NotifyStudent(
            string registrationId,
            string title,
            string message,
            string notificationType)
        {
            if (string.IsNullOrWhiteSpace(registrationId))
            {
                return;
            }

            var student =
                db.Registrations
                .FirstOrDefault(r =>
                    r.RegistrationId == registrationId);

            if (student == null)
            {
                return;
            }

            var notification =
                new Notification
                {
                    RegistrationId =
                        student.RegistrationId,

                    InstructorId =
                        null,

                    Title =
                        title,

                    Message =
                        message,

                    NotificationType =
                        notificationType,

                    IsRead =
                        false,

                    CreatedAt =
                        DateTime.Now,

                    ReadAt =
                        null
                };

            db.Notifications.Add(notification);

            db.SaveChanges();
        }


        // =========================================================
        // NOTIFY INSTRUCTOR
        // =========================================================

        public void NotifyInstructor(
            int instructorId,
            string title,
            string message,
            string notificationType)
        {
            var instructor =
                db.Instructors
                .FirstOrDefault(i =>
                    i.InstructorId == instructorId);

            if (instructor == null)
            {
                return;
            }

            var notification =
                new Notification
                {
                    RegistrationId =
                        null,

                    InstructorId =
                        instructor.InstructorId,

                    Title =
                        title,

                    Message =
                        message,

                    NotificationType =
                        notificationType,

                    IsRead =
                        false,

                    CreatedAt =
                        DateTime.Now,

                    ReadAt =
                        null
                };

            db.Notifications.Add(notification);

            db.SaveChanges();
        }


        // =========================================================
        // GET UNREAD STUDENT NOTIFICATIONS
        // =========================================================

        public IQueryable<Notification>
            GetUnreadStudentNotifications(
                string registrationId)
        {
            return db.Notifications
                .Where(n =>
                    n.RegistrationId ==
                        registrationId &&

                    !n.IsRead)
                .OrderByDescending(n =>
                    n.CreatedAt);
        }


        // =========================================================
        // GET UNREAD INSTRUCTOR NOTIFICATIONS
        // =========================================================

        public IQueryable<Notification>
            GetUnreadInstructorNotifications(
                int instructorId)
        {
            return db.Notifications
                .Where(n =>
                    n.InstructorId ==
                        instructorId &&

                    !n.IsRead)
                .OrderByDescending(n =>
                    n.CreatedAt);
        }
    }
}