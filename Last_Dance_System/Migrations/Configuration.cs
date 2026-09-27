namespace Last_Dance_System.Migrations
{
    using Last_Dance_System.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using static System.Web.Razor.Parser.SyntaxConstants;

    internal sealed class Configuration :
        DbMigrationsConfiguration<Last_Dance_System.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(
            Last_Dance_System.Models.ApplicationDbContext context)
        {
            // ============================================================
            // CREATE ROLES
            // ============================================================

            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            string[] roles =
            {
                "Student",
                "Instructor",
                "Administrator"
            };

            foreach (var role in roles)
            {
                if (!roleManager.RoleExists(role))
                {
                    roleManager.Create(new IdentityRole(role));
                }
            }


            // ============================================================
            // CREATE ADMINISTRATOR ACCOUNT
            // ============================================================

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            var adminEmail = "admin@tkdrivingschool.co.za";

            var admin = userManager.FindByEmail(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = userManager.Create(
                    admin,
                    "Admin@12345");

                if (result.Succeeded)
                {
                    userManager.AddToRole(
                        admin.Id,
                        "Administrator");
                }
            }
            else
            {
                if (!userManager.IsInRole(
                    admin.Id,
                    "Administrator"))
                {
                    userManager.AddToRole(
                        admin.Id,
                        "Administrator");
                }
            }

           
// ============================================================
// LEARNER THEORY PACKAGE
// ============================================================
//
// One purchase gives the student access to ALL learner theory
// lessons, study material, images, quizzes and quiz results.
//
// NumberOfLessons is set to 1 because the existing
// LessonPackage model requires a value greater than 0.
//
// IMPORTANT:
// LessonsRemaining will NOT be used to control learner theory
// access. The IsLearnerTheoryPackage flag identifies this package.
// ============================================================

context.LessonPackages.AddOrUpdate(
    p => p.PackageName,

    new LessonPackage
    {
        PackageName =
            "Learner Theory Package",

        NumberOfLessons =
            1,

        Price =
            299.00m,

        Description =
            "Full access to all learner theory lessons, " +
            "study material, images, quizzes and quiz results.",

        IsActive =
            true,

        IsLearnerTheoryPackage =
            true
    }
);


            // ============================================================
            // LEARNER THEORY LESSONS
            // ============================================================

            context.LearnerLessons.AddOrUpdate(
                l => l.LearnerLessonId,


                // ========================================================
                // LESSON 1 - ROAD SIGNS
                // ========================================================

                new LearnerLesson
                {
                    LearnerLessonId = 1,

                    Title = "Road Signs",

                    Description =
                        "Learn the different types of road signs " +
                        "and understand what they mean.",

                    LessonOrder = 1,

                    IsActive = true,

                    Content = @"
<div class='lesson-content'>

    <h2>Road Signs</h2>

    <p>
        Road signs are used to give drivers important information
        about the road, traffic conditions, hazards, restrictions
        and directions.
    </p>

    <p>
        Every learner should understand the meaning of common road
        signs before driving on public roads.
    </p>

    <h3>1. Regulatory Signs</h3>

    <div class='sign-card'>
        <h3>STOP</h3>
        <p>
            A STOP sign means you must come to a complete stop.
            Stop before the stop line or before entering the
            intersection. Proceed only when it is safe.
        </p>
    </div>

    <div class='sign-card'>
        <h3>YIELD</h3>
        <p>
            A YIELD sign means you must give way to other road users.
            Slow down and only proceed when it is safe.
        </p>
    </div>

    <div class='sign-card'>
        <h3>NO ENTRY</h3>
        <p>
            No Entry means vehicles are not allowed to enter that
            road or area from that direction.
        </p>
    </div>

    <h3>2. Warning Signs</h3>

    <div class='sign-card'>
        <h3>DANGEROUS BEND</h3>
        <p>
            This warns drivers that there is a dangerous or sharp
            bend ahead. Reduce speed and prepare to steer safely
            through the bend.
        </p>
    </div>

    <div class='sign-card'>
        <h3>PEDESTRIANS</h3>
        <p>
            This sign warns drivers that pedestrians may be present.
            Slow down and watch carefully for people crossing the road.
        </p>
    </div>

    <h3>3. Information Signs</h3>

    <div class='sign-card'>
        <h3>PARKING</h3>
        <p>
            This sign indicates an area where vehicles may be parked,
            subject to the conditions displayed.
        </p>
    </div>

    <div class='sign-card'>
        <h3>HOSPITAL</h3>
        <p>
            This indicates the location or direction of a hospital.
            Drivers should drive carefully near hospitals.
        </p>
    </div>

    <div class='sign-card'>
        <h3>FUEL STATION</h3>
        <p>
            This sign indicates that a fuel station is available nearby.
        </p>
    </div>

    <div class='remember-box'>
        <h3>Remember</h3>
        <p>
            Learn the shape, colour and meaning of road signs.
            Always obey regulatory signs and respond appropriately
            to warning signs.
        </p>
    </div>

</div>"
                },


                // ========================================================
                // LESSON 2 - RULES OF THE ROAD
                // ========================================================

                new LearnerLesson
                {
                    LearnerLessonId = 2,

                    Title = "Rules of the Road",

                    Description =
                        "Learn the important rules that drivers " +
                        "must follow when using public roads.",

                    LessonOrder = 2,

                    IsActive = true,

                    Content = @"
<div class='lesson-content'>

    <h2>Rules of the Road</h2>

    <p>
        Rules of the road help drivers use public roads safely
        and responsibly.
    </p>

    <p>
        Drivers must understand these rules and apply them
        at all times.
    </p>

    <div class='sign-card'>
        <h3>KEEP LEFT</h3>
        <p>
            In South Africa, vehicles normally travel on the
            left-hand side of the road.
        </p>
    </div>

    <div class='sign-card'>
        <h3>OVERTAKING</h3>
        <p>
            Only overtake when it is legal and safe.
            Check mirrors, blind spots and the road ahead before
            changing position.
        </p>
    </div>

    <div class='sign-card'>
        <h3>SPEED LIMITS</h3>
        <p>
            Always obey the posted speed limit.
            A speed limit indicates the maximum permitted speed
            under normal conditions.
        </p>
    </div>

    <div class='sign-card'>
        <h3>50 km/h</h3>
        <p>
            Where a 50 km/h speed limit is displayed, do not
            exceed the posted limit.
        </p>
    </div>

    <div class='sign-card'>
        <h3>100 km/h</h3>
        <p>
            Where a 100 km/h speed limit is displayed, do not
            exceed the posted limit.
        </p>
    </div>

    <div class='sign-card'>
        <h3>FOLLOWING DISTANCE</h3>
        <p>
            Keep enough distance between your vehicle and the
            vehicle ahead so that you have enough time to react
            and stop safely.
        </p>
    </div>

    <div class='sign-card'>
        <h3>SEAT BELTS</h3>
        <p>
            The driver and passengers must wear seat belts where
            required. Always check that your seat belt is correctly
            fastened before driving.
        </p>
    </div>

    <div class='sign-card'>
        <h3>MOBILE PHONES</h3>
        <p>
            Do not use a mobile phone in a way that distracts you
            from driving. Your full attention should remain on the road.
        </p>
    </div>

    <div class='sign-card'>
        <h3>ALCOHOL AND DRIVING</h3>
        <p>
            Never drive while impaired by alcohol or another substance.
            Impairment can reduce reaction time, judgement and control.
        </p>
    </div>

    <div class='sign-card'>
        <h3>PEDESTRIANS</h3>
        <p>
            Watch carefully for pedestrians, especially near crossings,
            schools, shopping areas and residential areas.
        </p>
    </div>

    <div class='sign-card'>
        <h3>INTERSECTIONS</h3>
        <p>
            Approach intersections carefully.
            Observe traffic signs, signals and right-of-way rules
            before proceeding.
        </p>
    </div>

    <div class='remember-box'>
        <h3>Remember</h3>
        <p>
            Good driving means following the rules while constantly
            watching the road and other road users.
        </p>
    </div>

</div>"
                },


                // ========================================================
                // LESSON 3 - ROAD MARKINGS
                // ========================================================

                new LearnerLesson
                {
                    LearnerLessonId = 3,

                    Title = "Road Markings",

                    Description =
                        "Understand the different road markings " +
                        "and what they mean to drivers.",

                    LessonOrder = 3,

                    IsActive = true,

                    Content = @"
<div class='lesson-content'>

    <h2>Road Markings</h2>

    <p>
        Road markings are lines and symbols painted on the road.
        They help control traffic and show drivers where they may
        travel, stop, turn or change lanes.
    </p>

    <div class='sign-card'>
        <h3>SOLID LINE</h3>
        <p>
            A solid line generally indicates that crossing the line
            is restricted or should not be done unless permitted by
            the applicable road rules and markings.
        </p>
    </div>

    <div class='sign-card'>
        <h3>BROKEN LINE</h3>
        <p>
            A broken line can indicate that lane changing or crossing
            the line is permitted when it is safe and legal.
        </p>
    </div>

    <div class='sign-card'>
        <h3>STOP LINE</h3>
        <p>
            A stop line shows where a vehicle must stop when required
            by a STOP sign or traffic signal.
        </p>
    </div>

    <div class='sign-card'>
        <h3>YIELD LINE</h3>
        <p>
            A yield line indicates where a driver should give way
            to other traffic before proceeding.
        </p>
    </div>

    <div class='sign-card'>
        <h3>PEDESTRIAN CROSSING</h3>
        <p>
            A pedestrian crossing provides a designated area for
            pedestrians to cross the road.
        </p>
    </div>

    <div class='remember-box'>
        <h3>Remember</h3>
        <p>
            Road markings work together with road signs and traffic
            signals. Always observe all three when driving.
        </p>
    </div>

</div>"
                },


                // ========================================================
                // LESSON 4 - TRAFFIC SIGNALS
                // ========================================================

                new LearnerLesson
                {
                    LearnerLessonId = 4,

                    Title = "Traffic Signals",

                    Description =
                        "Learn how traffic lights and pedestrian " +
                        "signals control road users.",

                    LessonOrder = 4,

                    IsActive = true,

                    Content = @"
<div class='lesson-content'>

    <h2>Traffic Signals</h2>

    <p>
        Traffic signals control the movement of vehicles and
        pedestrians at intersections and other locations.
    </p>

    <div class='sign-card'>
        <h3>RED LIGHT</h3>
        <p>
            A red traffic light means you must stop.
            Do not proceed until the signal allows you to continue.
        </p>
    </div>

    <div class='sign-card'>
        <h3>YELLOW LIGHT</h3>
        <p>
            A yellow light warns that the signal is changing.
            Stop when it is safe to do so, unless you are already
            too close to stop safely.
        </p>
    </div>

    <div class='sign-card'>
        <h3>GREEN LIGHT</h3>
        <p>
            A green light allows you to proceed, provided that the
            intersection is clear and it is safe to continue.
        </p>
    </div>

    <div class='sign-card'>
        <h3>PEDESTRIAN SIGNALS</h3>
        <p>
            Pedestrian signals control when pedestrians may cross.
            Drivers should watch for pedestrians and give way where
            required.
        </p>
    </div>

    <div class='sign-card'>
        <h3>TRAFFIC SIGNAL CYCLE</h3>
        <p>
            Traffic signals normally change through a sequence.
            Drivers should watch the signal carefully and prepare
            to stop or proceed safely.
        </p>
    </div>

    <div class='important-box'>
        <h3>Important</h3>
        <p>
            Never assume that a green light means you can drive
            without looking. Always check that the intersection is
            safe before proceeding.
        </p>
    </div>

    <div class='remember-box'>
        <h3>Remember</h3>
        <p>
            Red means stop. Yellow means prepare to stop.
            Green means proceed when safe.
        </p>
    </div>

</div>"
                },


                // ========================================================
                // LESSON 5 - VEHICLE CONTROLS
                // ========================================================

                new LearnerLesson
                {
                    LearnerLessonId = 5,

                    Title = "Vehicle Controls",

                    Description =
                        "Learn the basic controls of a vehicle " +
                        "and how they are used safely.",

                    LessonOrder = 5,

                    IsActive = true,

                    Content = @"
<div class='lesson-content'>

    <h2>Vehicle Controls</h2>

    <p>
        Before driving, a learner must understand the basic controls
        of the vehicle and know how to operate them safely.
    </p>

    <div class='sign-card'>
        <h3>STEERING WHEEL</h3>
        <p>
            The steering wheel controls the direction of the vehicle.
            Use smooth steering movements and maintain proper control.
        </p>
    </div>

    <div class='sign-card'>
        <h3>ACCELERATOR</h3>
        <p>
            The accelerator controls the amount of power sent to
            the vehicle. Apply pressure smoothly and avoid sudden
            acceleration.
        </p>
    </div>

    <div class='sign-card'>
        <h3>BRAKE</h3>
        <p>
            The brake slows down or stops the vehicle.
            Apply the brake smoothly and progressively when possible.
        </p>
    </div>

    <div class='sign-card'>
        <h3>CLUTCH</h3>
        <p>
            In a manual vehicle, the clutch connects and disconnects
            the engine from the transmission.
        </p>
    </div>

    <div class='sign-card'>
        <h3>GEAR LEVER</h3>
        <p>
            The gear lever is used to select the appropriate gear.
            Select gears according to the vehicle speed and road
            conditions.
        </p>
    </div>

    <div class='sign-card'>
        <h3>HANDBRAKE</h3>
        <p>
            The handbrake helps keep the vehicle stationary when parked.
            Make sure it is properly released before driving.
        </p>
    </div>

    <div class='sign-card'>
        <h3>INDICATORS</h3>
        <p>
            Indicators communicate your intention to turn or change
            direction to other road users.
        </p>
    </div>

    <div class='sign-card'>
        <h3>MIRRORS</h3>
        <p>
            Mirrors help you observe traffic around your vehicle.
            Check mirrors regularly and also check blind spots.
        </p>
    </div>

    <div class='sign-card'>
        <h3>PRE-DRIVE CHECKS</h3>
        <p>
            Before moving, check your seat position, mirrors,
            seat belt, handbrake, gear selection and surroundings.
        </p>
    </div>

    <div class='remember-box'>
        <h3>Pre-Drive Checklist</h3>

        <p>
            Seat adjusted ✔
            <br />
            Mirrors adjusted ✔
            <br />
            Seat belt fastened ✔
            <br />
            Handbrake checked ✔
            <br />
            Gear selected correctly ✔
            <br />
            Surroundings checked ✔
        </p>
    </div>

</div>"
                },


                // ========================================================
                // LESSON 6 - SAFETY AND DEFENSIVE DRIVING
                // ========================================================

                new LearnerLesson
                {
                    LearnerLessonId = 6,

                    Title = "Safety and Defensive Driving",

                    Description =
                        "Learn how defensive driving can help " +
                        "reduce risks and improve road safety.",

                    LessonOrder = 6,

                    IsActive = true,

                    Content = @"
<div class='lesson-content'>

    <h2>Safety and Defensive Driving</h2>

    <p>
        Defensive driving means driving in a way that reduces
        the risk of collisions by anticipating hazards and the
        actions of other road users.
    </p>

    <div class='sign-card'>
        <h3>FOLLOWING DISTANCE</h3>
        <p>
            Keep a safe distance from the vehicle ahead.
            A greater following distance may be needed at higher
            speeds or in poor road conditions.
        </p>
    </div>

    <div class='sign-card'>
        <h3>BLIND SPOTS</h3>
        <p>
            Blind spots are areas around the vehicle that may not
            be visible in the mirrors.
        </p>
    </div>

    <div class='sign-card'>
        <h3>WATCH OTHER DRIVERS</h3>
        <p>
            Do not assume other drivers will always obey the rules.
            Watch their movements and be prepared to react safely.
        </p>
    </div>

    <div class='sign-card'>
        <h3>DRIVE ACCORDING TO CONDITIONS</h3>
        <p>
            Adjust your speed and driving behaviour according to
            traffic, weather, road surface, visibility and other
            conditions.
        </p>
    </div>

    <div class='sign-card'>
        <h3>AVOID DISTRACTIONS</h3>
        <p>
            Keep your attention on driving.
            Avoid activities that take your eyes, hands or
            concentration away from the road.
        </p>
    </div>

    <div class='sign-card'>
        <h3>STAY CALM</h3>
        <p>
            Remain calm when dealing with traffic, mistakes or
            aggressive road users.
        </p>
    </div>

    <div class='sign-card'>
        <h3>DRIVING IN RAIN</h3>
        <p>
            Rain can reduce visibility and road grip.
            Reduce speed, increase following distance and use
            appropriate lights.
        </p>
    </div>

    <div class='sign-card'>
        <h3>DRIVING AT NIGHT</h3>
        <p>
            Visibility is reduced at night.
            Drive at a safe speed, use headlights correctly and
            watch carefully for hazards.
        </p>
    </div>

    <div class='sign-card'>
        <h3>EMERGENCY SITUATIONS</h3>
        <p>
            In an emergency, remain calm and try to maintain
            control of the vehicle.
        </p>
    </div>

    <div class='remember-box'>
        <h3>Remember</h3>
        <p>
            Defensive driving is about seeing hazards early,
            maintaining control and making safe decisions.
        </p>
    </div>

</div>"
                },


                // ========================================================
                // LESSON 7 - LEARNER'S LICENCE TEST PREPARATION
                // ========================================================

                new LearnerLesson
                {
                    LearnerLessonId = 7,

                    Title = "Learner's Licence Test Preparation",

                    Description =
                        "Prepare for your learner's licence test " +
                        "by revising the most important theory topics.",

                    LessonOrder = 7,

                    IsActive = true,

                    Content = @"
<div class='lesson-content'>

    <h2>Learner's Licence Test Preparation</h2>

    <p>
        Preparing for the learner's licence test requires an
        understanding of road signs, rules of the road, road
        markings, traffic signals and basic vehicle controls.
    </p>

    <div class='sign-card'>
        <h3>ROAD SIGNS</h3>
        <p>
            Revise regulatory, warning and information signs.
            Make sure you understand what each sign means and
            what action a driver should take.
        </p>
    </div>

    <div class='sign-card'>
        <h3>RULES OF THE ROAD</h3>
        <p>
            Revise important rules such as keeping left,
            overtaking, speed limits, following distance,
            pedestrians and intersections.
        </p>
    </div>

    <div class='sign-card'>
        <h3>ROAD MARKINGS</h3>
        <p>
            Learn the difference between solid lines, broken lines,
            stop lines, yield lines and pedestrian crossings.
        </p>
    </div>

    <div class='sign-card'>
        <h3>TRAFFIC SIGNALS</h3>
        <p>
            Understand the meaning of red, yellow and green
            traffic signals as well as pedestrian signals.
        </p>
    </div>

    <div class='sign-card'>
        <h3>VEHICLE CONTROLS</h3>
        <p>
            Know the purpose of the steering wheel, accelerator,
            brake, clutch, gear lever, handbrake, indicators
            and mirrors.
        </p>
    </div>

    <div class='sign-card'>
        <h3>DEFENSIVE DRIVING</h3>
        <p>
            Revise safe following distances, blind spots,
            distractions, weather conditions and how to
            identify hazards.
        </p>
    </div>

    <div class='sign-card'>
        <h3>BEFORE THE TEST</h3>
        <p>
            Make sure you have studied the required material.
            Get enough rest and arrive at the test location on time.
        </p>
    </div>

    <div class='sign-card'>
        <h3>DURING THE TEST</h3>
        <p>
            Read each question carefully before answering.
            Do not rush and make sure you understand what the
            question is asking.
        </p>
    </div>

    <div class='sign-card'>
        <h3>FINAL REMINDER</h3>
        <p>
            Understanding the rules is more important than simply
            memorising answers. Use your knowledge to make safe
            decisions on the road.
        </p>
    </div>

    <div class='remember-box'>
        <h3>Good Luck!</h3>
        <p>
            Study consistently, understand the rules and practise
            answering learner's licence questions.
        </p>
    </div>

</div>"
                }
            );


            // ============================================================
            // QUIZ QUESTIONS - LESSON 1: ROAD SIGNS
            // ============================================================

            context.QuizQuestions.AddOrUpdate(
                q => q.QuizQuestionId,

                new QuizQuestion
                {
                    QuizQuestionId = 1,
                    LearnerLessonId = 1,
                    Question = "What does a STOP sign require a driver to do?",
                    OptionA = "Slow down only",
                    OptionB = "Stop completely before proceeding",
                    OptionC = "Speed up",
                    OptionD = "Sound the hooter",
                    CorrectAnswer = "B",
                    QuestionOrder = 1,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 2,
                    LearnerLessonId = 1,
                    Question = "What does a YIELD sign mean?",
                    OptionA = "You must always stop",
                    OptionB = "You have priority over all vehicles",
                    OptionC = "Give way to other road users when required",
                    OptionD = "Overtake immediately",
                    CorrectAnswer = "C",
                    QuestionOrder = 2,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 3,
                    LearnerLessonId = 1,
                    Question = "What does a NO ENTRY sign indicate?",
                    OptionA = "Parking is allowed",
                    OptionB = "Vehicles may not enter",
                    OptionC = "Only buses may enter",
                    OptionD = "The road is one-way in your direction",
                    CorrectAnswer = "B",
                    QuestionOrder = 3,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 4,
                    LearnerLessonId = 1,
                    Question = "What type of information does a warning sign generally provide?",
                    OptionA = "It warns drivers about possible hazards",
                    OptionB = "It shows fuel prices",
                    OptionC = "It gives a driver's licence number",
                    OptionD = "It shows vehicle registration details",
                    CorrectAnswer = "A",
                    QuestionOrder = 4,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 5,
                    LearnerLessonId = 1,
                    Question = "What does a pedestrian warning sign alert drivers to?",
                    OptionA = "A fuel station",
                    OptionB = "Possible pedestrians in or near the road",
                    OptionC = "A parking area",
                    OptionD = "A hospital entrance",
                    CorrectAnswer = "B",
                    QuestionOrder = 5,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 6,
                    LearnerLessonId = 1,
                    Question = "Which sign indicates that parking is available?",
                    OptionA = "Hospital sign",
                    OptionB = "No Entry sign",
                    OptionC = "Parking information sign",
                    OptionD = "STOP sign",
                    CorrectAnswer = "C",
                    QuestionOrder = 6,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 7,
                    LearnerLessonId = 1,
                    Question = "What does a hospital information sign help drivers identify?",
                    OptionA = "A place where medical services are available",
                    OptionB = "A compulsory stopping point",
                    OptionC = "A speed limit",
                    OptionD = "An overtaking area",
                    CorrectAnswer = "A",
                    QuestionOrder = 7,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 8,
                    LearnerLessonId = 1,
                    Question = "What does a fuel station sign indicate?",
                    OptionA = "A vehicle testing station",
                    OptionB = "A place where fuel can be obtained",
                    OptionC = "A parking prohibition",
                    OptionD = "A pedestrian crossing",
                    CorrectAnswer = "B",
                    QuestionOrder = 8,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 9,
                    LearnerLessonId = 1,
                    Question = "Why are road signs important?",
                    OptionA = "They decorate the roads",
                    OptionB = "They help communicate rules, warnings and information",
                    OptionC = "They increase vehicle speed",
                    OptionD = "They replace traffic officers",
                    CorrectAnswer = "B",
                    QuestionOrder = 9,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 10,
                    LearnerLessonId = 1,
                    Question = "A driver should do what when approaching a road sign?",
                    OptionA = "Ignore it if there is no traffic",
                    OptionB = "Read and obey the information shown",
                    OptionC = "Always stop regardless of the sign",
                    OptionD = "Accelerate past it",
                    CorrectAnswer = "B",
                    QuestionOrder = 10,
                    IsActive = true
                }
            );


            // ============================================================
            // QUIZ QUESTIONS - LESSON 2: RULES OF THE ROAD
            // ============================================================

            context.QuizQuestions.AddOrUpdate(
                q => q.QuizQuestionId,

                new QuizQuestion
                {
                    QuizQuestionId = 11,
                    LearnerLessonId = 2,
                    Question = "In South Africa, on which side of the road do vehicles normally travel?",
                    OptionA = "Right-hand side",
                    OptionB = "Left-hand side",
                    OptionC = "Centre of the road",
                    OptionD = "Either side",
                    CorrectAnswer = "B",
                    QuestionOrder = 1,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 12,
                    LearnerLessonId = 2,
                    Question = "What should a driver do before overtaking?",
                    OptionA = "Accelerate immediately",
                    OptionB = "Check that overtaking is legal and safe",
                    OptionC = "Drive onto the pavement",
                    OptionD = "Ignore the vehicle ahead",
                    CorrectAnswer = "B",
                    QuestionOrder = 2,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 13,
                    LearnerLessonId = 2,
                    Question = "What should a driver check before changing lanes?",
                    OptionA = "Mirrors and blind spots",
                    OptionB = "Only the speedometer",
                    OptionC = "Only the fuel gauge",
                    OptionD = "Only the horn",
                    CorrectAnswer = "A",
                    QuestionOrder = 3,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 14,
                    LearnerLessonId = 2,
                    Question = "Why should a driver maintain a safe following distance?",
                    OptionA = "To prevent all overtaking",
                    OptionB = "To allow time to react and stop",
                    OptionC = "To increase speed",
                    OptionD = "To avoid using mirrors",
                    CorrectAnswer = "B",
                    QuestionOrder = 4,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 15,
                    LearnerLessonId = 2,
                    Question = "What should you do when approaching a red traffic signal?",
                    OptionA = "Stop",
                    OptionB = "Speed up",
                    OptionC = "Overtake",
                    OptionD = "Continue without checking",
                    CorrectAnswer = "A",
                    QuestionOrder = 5,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 16,
                    LearnerLessonId = 2,
                    Question = "Why are speed limits important?",
                    OptionA = "They provide a maximum permitted speed for the applicable road conditions",
                    OptionB = "They require drivers to travel at maximum speed",
                    OptionC = "They apply only to pedestrians",
                    OptionD = "They replace road signs",
                    CorrectAnswer = "A",
                    QuestionOrder = 6,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 17,
                    LearnerLessonId = 2,
                    Question = "What should drivers do when approaching pedestrians near a crossing?",
                    OptionA = "Increase speed",
                    OptionB = "Watch carefully and give way where required",
                    OptionC = "Ignore the pedestrians",
                    OptionD = "Drive onto the pavement",
                    CorrectAnswer = "B",
                    QuestionOrder = 7,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 18,
                    LearnerLessonId = 2,
                    Question = "What should a driver do when an emergency vehicle approaches with its warning signals?",
                    OptionA = "Block its path",
                    OptionB = "Give way and allow it to pass safely",
                    OptionC = "Follow it closely",
                    OptionD = "Race ahead of it",
                    CorrectAnswer = "B",
                    QuestionOrder = 8,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 19,
                    LearnerLessonId = 2,
                    Question = "What is the purpose of using an indicator?",
                    OptionA = "To communicate an intended change of direction",
                    OptionB = "To increase engine power",
                    OptionC = "To replace mirror checks",
                    OptionD = "To control the brakes",
                    CorrectAnswer = "A",
                    QuestionOrder = 9,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 20,
                    LearnerLessonId = 2,
                    Question = "What should you do before entering a road from a driveway?",
                    OptionA = "Enter immediately",
                    OptionB = "Give way to traffic and pedestrians",
                    OptionC = "Only look behind you",
                    OptionD = "Accelerate onto the road",
                    CorrectAnswer = "B",
                    QuestionOrder = 10,
                    IsActive = true
                }
            );


            // ============================================================
            // QUIZ QUESTIONS - LESSON 3: ROAD MARKINGS
            // ============================================================

            context.QuizQuestions.AddOrUpdate(
                q => q.QuizQuestionId,

                new QuizQuestion
                {
                    QuizQuestionId = 21,
                    LearnerLessonId = 3,
                    Question = "What is the main purpose of road markings?",
                    OptionA = "To decorate roads",
                    OptionB = "To guide and regulate road users",
                    OptionC = "To increase vehicle speed",
                    OptionD = "To identify vehicle owners",
                    CorrectAnswer = "B",
                    QuestionOrder = 1,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 22,
                    LearnerLessonId = 3,
                    Question = "What does a solid road line generally indicate?",
                    OptionA = "Crossing it is always required",
                    OptionB = "Crossing it may be restricted",
                    OptionC = "Parking is compulsory",
                    OptionD = "The road is closed",
                    CorrectAnswer = "B",
                    QuestionOrder = 2,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 23,
                    LearnerLessonId = 3,
                    Question = "What does a broken lane line generally allow?",
                    OptionA = "Lane changing when safe and legal",
                    OptionB = "Driving against traffic",
                    OptionC = "Permanent parking",
                    OptionD = "Ignoring traffic rules",
                    CorrectAnswer = "A",
                    QuestionOrder = 3,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 24,
                    LearnerLessonId = 3,
                    Question = "What is the purpose of a stop line?",
                    OptionA = "It shows where a vehicle should stop",
                    OptionB = "It shows where fuel is available",
                    OptionC = "It marks a parking bay",
                    OptionD = "It marks a passing lane",
                    CorrectAnswer = "A",
                    QuestionOrder = 4,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 25,
                    LearnerLessonId = 3,
                    Question = "What does a pedestrian crossing marking indicate?",
                    OptionA = "An area where pedestrians may cross",
                    OptionB = "A vehicle repair area",
                    OptionC = "A compulsory parking area",
                    OptionD = "A fuel station",
                    CorrectAnswer = "A",
                    QuestionOrder = 5,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 26,
                    LearnerLessonId = 3,
                    Question = "What do directional arrows painted on a road indicate?",
                    OptionA = "Permitted directions of travel",
                    OptionB = "Fuel consumption",
                    OptionC = "Vehicle ownership",
                    OptionD = "Pedestrian speed",
                    CorrectAnswer = "A",
                    QuestionOrder = 6,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 27,
                    LearnerLessonId = 3,
                    Question = "What should drivers do when approaching a road marking requiring them to stop?",
                    OptionA = "Stop at the indicated position",
                    OptionB = "Drive across it first",
                    OptionC = "Ignore it when traffic is light",
                    OptionD = "Stop in the middle of the intersection",
                    CorrectAnswer = "A",
                    QuestionOrder = 7,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 28,
                    LearnerLessonId = 3,
                    Question = "Why should drivers understand road markings?",
                    OptionA = "They help communicate traffic rules and directions",
                    OptionB = "They increase engine power",
                    OptionC = "They identify vehicle brands",
                    OptionD = "They replace traffic signals",
                    CorrectAnswer = "A",
                    QuestionOrder = 8,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 29,
                    LearnerLessonId = 3,
                    Question = "What should a driver do when road markings are unclear?",
                    OptionA = "Ignore all other road users",
                    OptionB = "Drive carefully and follow applicable traffic controls",
                    OptionC = "Increase speed",
                    OptionD = "Drive on the shoulder",
                    CorrectAnswer = "B",
                    QuestionOrder = 9,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 30,
                    LearnerLessonId = 3,
                    Question = "Road markings should be considered together with what?",
                    OptionA = "Vehicle colour only",
                    OptionB = "Road signs and traffic signals",
                    OptionC = "Fuel prices only",
                    OptionD = "Vehicle registration numbers",
                    CorrectAnswer = "B",
                    QuestionOrder = 10,
                    IsActive = true
                }
            );


            // ============================================================
            // QUIZ QUESTIONS - LESSON 4: TRAFFIC SIGNALS
            // ============================================================

            context.QuizQuestions.AddOrUpdate(
                q => q.QuizQuestionId,

                new QuizQuestion
                {
                    QuizQuestionId = 31,
                    LearnerLessonId = 4,
                    Question = "What does a red traffic light mean?",
                    OptionA = "Proceed",
                    OptionB = "Stop",
                    OptionC = "Speed up",
                    OptionD = "Overtake",
                    CorrectAnswer = "B",
                    QuestionOrder = 1,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 32,
                    LearnerLessonId = 4,
                    Question = "What does a green traffic light generally allow?",
                    OptionA = "Proceed when it is safe",
                    OptionB = "Stop immediately",
                    OptionC = "Reverse",
                    OptionD = "Park",
                    CorrectAnswer = "A",
                    QuestionOrder = 2,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 33,
                    LearnerLessonId = 4,
                    Question = "What does a yellow traffic light indicate?",
                    OptionA = "The signal is changing",
                    OptionB = "Parking is compulsory",
                    OptionC = "The road is closed",
                    OptionD = "Overtaking is required",
                    CorrectAnswer = "A",
                    QuestionOrder = 3,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 34,
                    LearnerLessonId = 4,
                    Question = "What should a driver do at a red traffic light?",
                    OptionA = "Stop before the applicable stop line",
                    OptionB = "Continue if there are no pedestrians",
                    OptionC = "Accelerate",
                    OptionD = "Drive onto the pavement",
                    CorrectAnswer = "A",
                    QuestionOrder = 4,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 35,
                    LearnerLessonId = 4,
                    Question = "What is the purpose of pedestrian signals?",
                    OptionA = "To control pedestrian movement at crossings",
                    OptionB = "To control fuel consumption",
                    OptionC = "To indicate vehicle ownership",
                    OptionD = "To indicate parking prices",
                    CorrectAnswer = "A",
                    QuestionOrder = 5,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 36,
                    LearnerLessonId = 4,
                    Question = "Why should drivers obey traffic signals?",
                    OptionA = "To regulate traffic and improve safety",
                    OptionB = "To increase engine power",
                    OptionC = "To avoid using mirrors",
                    OptionD = "To increase fuel consumption",
                    CorrectAnswer = "A",
                    QuestionOrder = 6,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 37,
                    LearnerLessonId = 4,
                    Question = "What should a driver do if traffic lights are not operating?",
                    OptionA = "Drive through without checking",
                    OptionB = "Approach cautiously and follow applicable right-of-way rules",
                    OptionC = "Accelerate",
                    OptionD = "Ignore other traffic",
                    CorrectAnswer = "B",
                    QuestionOrder = 7,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 38,
                    LearnerLessonId = 4,
                    Question = "What should a driver check before proceeding through a green light?",
                    OptionA = "That the intersection is clear and safe",
                    OptionB = "Only the vehicle colour",
                    OptionC = "Only the fuel gauge",
                    OptionD = "Whether another driver is behind",
                    CorrectAnswer = "A",
                    QuestionOrder = 8,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 39,
                    LearnerLessonId = 4,
                    Question = "Who may direct traffic at an intersection?",
                    OptionA = "A traffic officer",
                    OptionB = "A passenger",
                    OptionC = "A parked vehicle",
                    OptionD = "A fuel station attendant",
                    CorrectAnswer = "A",
                    QuestionOrder = 9,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 40,
                    LearnerLessonId = 4,
                    Question = "What should drivers do when directed by a traffic officer?",
                    OptionA = "Follow the officer's lawful directions",
                    OptionB = "Ignore the officer",
                    OptionC = "Always reverse",
                    OptionD = "Accelerate through the intersection",
                    CorrectAnswer = "A",
                    QuestionOrder = 10,
                    IsActive = true
                }
            );


            // ============================================================
            // QUIZ QUESTIONS - LESSON 5: VEHICLE CONTROLS
            // ============================================================

            context.QuizQuestions.AddOrUpdate(
                q => q.QuizQuestionId,

                new QuizQuestion
                {
                    QuizQuestionId = 41,
                    LearnerLessonId = 5,
                    Question = "What is the main purpose of the brake pedal?",
                    OptionA = "To increase engine power",
                    OptionB = "To slow down or stop the vehicle",
                    OptionC = "To change direction",
                    OptionD = "To operate the headlights",
                    CorrectAnswer = "B",
                    QuestionOrder = 1,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 42,
                    LearnerLessonId = 5,
                    Question = "What is the purpose of the accelerator?",
                    OptionA = "To control engine power and vehicle speed",
                    OptionB = "To operate the wipers",
                    OptionC = "To operate the horn",
                    OptionD = "To apply the handbrake",
                    CorrectAnswer = "A",
                    QuestionOrder = 2,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 43,
                    LearnerLessonId = 5,
                    Question = "What does the steering wheel control?",
                    OptionA = "Vehicle direction",
                    OptionB = "Fuel level",
                    OptionC = "Engine temperature",
                    OptionD = "Tyre pressure",
                    CorrectAnswer = "A",
                    QuestionOrder = 3,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 44,
                    LearnerLessonId = 5,
                    Question = "What is the purpose of the clutch in a manual vehicle?",
                    OptionA = "To connect and disconnect the engine from the transmission",
                    OptionB = "To operate headlights",
                    OptionC = "To control the horn",
                    OptionD = "To operate the windscreen",
                    CorrectAnswer = "A",
                    QuestionOrder = 4,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 45,
                    LearnerLessonId = 5,
                    Question = "What is the purpose of the handbrake?",
                    OptionA = "To help keep a stationary vehicle from moving",
                    OptionB = "To increase acceleration",
                    OptionC = "To turn on headlights",
                    OptionD = "To change gears",
                    CorrectAnswer = "A",
                    QuestionOrder = 5,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 46,
                    LearnerLessonId = 5,
                    Question = "What do indicators communicate to other road users?",
                    OptionA = "An intended change of direction",
                    OptionB = "The fuel level",
                    OptionC = "The engine temperature",
                    OptionD = "The tyre pressure",
                    CorrectAnswer = "A",
                    QuestionOrder = 6,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 47,
                    LearnerLessonId = 5,
                    Question = "What is the purpose of the rear-view mirror?",
                    OptionA = "To observe traffic behind the vehicle",
                    OptionB = "To measure fuel consumption",
                    OptionC = "To control the brakes",
                    OptionD = "To increase engine power",
                    CorrectAnswer = "A",
                    QuestionOrder = 7,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 48,
                    LearnerLessonId = 5,
                    Question = "What is the purpose of windscreen wipers?",
                    OptionA = "To improve visibility when the windscreen is wet or dirty",
                    OptionB = "To increase vehicle speed",
                    OptionC = "To control steering",
                    OptionD = "To operate the brakes",
                    CorrectAnswer = "A",
                    QuestionOrder = 8,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 49,
                    LearnerLessonId = 5,
                    Question = "Why should a learner understand vehicle controls before driving?",
                    OptionA = "To operate the vehicle safely",
                    OptionB = "To increase maximum speed",
                    OptionC = "To avoid checking mirrors",
                    OptionD = "To avoid traffic rules",
                    CorrectAnswer = "A",
                    QuestionOrder = 9,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 50,
                    LearnerLessonId = 5,
                    Question = "What should you do if an important vehicle control does not operate correctly?",
                    OptionA = "Continue driving normally",
                    OptionB = "Have the problem checked and repaired",
                    OptionC = "Drive faster",
                    OptionD = "Ignore warning signs",
                    CorrectAnswer = "B",
                    QuestionOrder = 10,
                    IsActive = true
                }
            );


            // ============================================================
            // QUIZ QUESTIONS - LESSON 6:
            // SAFETY AND DEFENSIVE DRIVING
            // ============================================================

            context.QuizQuestions.AddOrUpdate(
                q => q.QuizQuestionId,

                new QuizQuestion
                {
                    QuizQuestionId = 51,
                    LearnerLessonId = 6,
                    Question = "What is defensive driving?",
                    OptionA = "Driving aggressively",
                    OptionB = "Anticipating hazards and the actions of other road users",
                    OptionC = "Driving as fast as possible",
                    OptionD = "Ignoring other road users",
                    CorrectAnswer = "B",
                    QuestionOrder = 1,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 52,
                    LearnerLessonId = 6,
                    Question = "Why is wearing a seat belt important?",
                    OptionA = "It helps protect occupants during a collision",
                    OptionB = "It increases engine power",
                    OptionC = "It improves fuel economy",
                    OptionD = "It makes the vehicle stop faster",
                    CorrectAnswer = "A",
                    QuestionOrder = 2,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 53,
                    LearnerLessonId = 6,
                    Question = "What should a driver do when visibility is poor?",
                    OptionA = "Reduce speed and increase caution",
                    OptionB = "Increase speed",
                    OptionC = "Drive without headlights",
                    OptionD = "Follow closely",
                    CorrectAnswer = "A",
                    QuestionOrder = 3,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 54,
                    LearnerLessonId = 6,
                    Question = "Why should drivers avoid distractions?",
                    OptionA = "Distraction reduces attention to the road",
                    OptionB = "Distraction improves reaction time",
                    OptionC = "Distraction improves steering",
                    OptionD = "Distraction reduces stopping distance",
                    CorrectAnswer = "A",
                    QuestionOrder = 4,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 55,
                    LearnerLessonId = 6,
                    Question = "What should you do when you feel tired while driving?",
                    OptionA = "Increase speed",
                    OptionB = "Stop somewhere safe and rest",
                    OptionC = "Drive closer to the vehicle ahead",
                    OptionD = "Ignore the tiredness",
                    CorrectAnswer = "B",
                    QuestionOrder = 5,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 56,
                    LearnerLessonId = 6,
                    Question = "Why should drivers maintain a safe following distance?",
                    OptionA = "It gives more time to react to hazards",
                    OptionB = "It prevents all overtaking",
                    OptionC = "It increases speed",
                    OptionD = "It eliminates the need for brakes",
                    CorrectAnswer = "A",
                    QuestionOrder = 6,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 57,
                    LearnerLessonId = 6,
                    Question = "What is a major danger of driving while impaired by alcohol?",
                    OptionA = "It can impair judgement and reaction ability",
                    OptionB = "It improves concentration",
                    OptionC = "It improves night vision",
                    OptionD = "It improves braking ability",
                    CorrectAnswer = "A",
                    QuestionOrder = 7,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 58,
                    LearnerLessonId = 6,
                    Question = "What should a driver do when approaching a potential hazard?",
                    OptionA = "Ignore it",
                    OptionB = "Identify it early and adjust driving appropriately",
                    OptionC = "Accelerate toward it",
                    OptionD = "Close the following distance",
                    CorrectAnswer = "B",
                    QuestionOrder = 8,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 59,
                    LearnerLessonId = 6,
                    Question = "Why is it important to check mirrors regularly?",
                    OptionA = "To remain aware of surrounding traffic",
                    OptionB = "To increase engine power",
                    OptionC = "To reduce tyre pressure",
                    OptionD = "To control fuel consumption",
                    CorrectAnswer = "A",
                    QuestionOrder = 9,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 60,
                    LearnerLessonId = 6,
                    Question = "What is a safe response to an aggressive driver?",
                    OptionA = "Respond aggressively",
                    OptionB = "Remain calm and avoid escalating the situation",
                    OptionC = "Race the driver",
                    OptionD = "Follow the driver closely",
                    CorrectAnswer = "B",
                    QuestionOrder = 10,
                    IsActive = true
                }
            );


            // ============================================================
            // QUIZ QUESTIONS - LESSON 7:
            // LEARNER'S LICENCE TEST PREPARATION
            // ============================================================

            context.QuizQuestions.AddOrUpdate(
                q => q.QuizQuestionId,

                new QuizQuestion
                {
                    QuizQuestionId = 61,
                    LearnerLessonId = 7,
                    Question = "Which topics are important when preparing for a learner's licence test?",
                    OptionA = "Only vehicle appearance",
                    OptionB = "Road signs, rules, markings, signals and vehicle controls",
                    OptionC = "Only vehicle colour",
                    OptionD = "Only parking",
                    CorrectAnswer = "B",
                    QuestionOrder = 1,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 62,
                    LearnerLessonId = 7,
                    Question = "Why should learners study road signs?",
                    OptionA = "Signs communicate important information and instructions",
                    OptionB = "Signs determine vehicle prices",
                    OptionC = "Signs identify fuel brands",
                    OptionD = "Signs replace traffic laws",
                    CorrectAnswer = "A",
                    QuestionOrder = 2,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 63,
                    LearnerLessonId = 7,
                    Question = "What is an effective way to prepare for a theory test?",
                    OptionA = "Study once and avoid revision",
                    OptionB = "Study the material and practise questions",
                    OptionC = "Memorise vehicle colours",
                    OptionD = "Ignore road signs",
                    CorrectAnswer = "B",
                    QuestionOrder = 3,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 64,
                    LearnerLessonId = 7,
                    Question = "Why is understanding road rules important?",
                    OptionA = "It helps drivers use roads safely and lawfully",
                    OptionB = "It allows drivers to ignore signs",
                    OptionC = "It removes the need for observation",
                    OptionD = "It guarantees traffic will never stop",
                    CorrectAnswer = "A",
                    QuestionOrder = 4,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 65,
                    LearnerLessonId = 7,
                    Question = "What should a learner do after getting a practice question wrong?",
                    OptionA = "Ignore the mistake",
                    OptionB = "Review the topic and understand the correct answer",
                    OptionC = "Stop studying",
                    OptionD = "Guess the same answer next time",
                    CorrectAnswer = "B",
                    QuestionOrder = 5,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 66,
                    LearnerLessonId = 7,
                    Question = "Which topic helps learners understand how to operate a vehicle?",
                    OptionA = "Vehicle controls",
                    OptionB = "Road names only",
                    OptionC = "Fuel station advertising",
                    OptionD = "Vehicle colours",
                    CorrectAnswer = "A",
                    QuestionOrder = 6,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 67,
                    LearnerLessonId = 7,
                    Question = "Why should learners practise identifying road markings?",
                    OptionA = "Road markings provide information and regulate road use",
                    OptionB = "Road markings determine vehicle ownership",
                    OptionC = "Road markings replace all traffic signs",
                    OptionD = "Road markings are only decorative",
                    CorrectAnswer = "A",
                    QuestionOrder = 7,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 68,
                    LearnerLessonId = 7,
                    Question = "Why should learners revise traffic signals?",
                    OptionA = "To understand what different signals require drivers to do",
                    OptionB = "To learn vehicle prices",
                    OptionC = "To identify vehicle manufacturers",
                    OptionD = "To learn fuel brands",
                    CorrectAnswer = "A",
                    QuestionOrder = 8,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 69,
                    LearnerLessonId = 7,
                    Question = "What should a learner focus on when answering theory questions?",
                    OptionA = "Read carefully and apply the relevant rule",
                    OptionB = "Choose the longest answer",
                    OptionC = "Choose the first option automatically",
                    OptionD = "Ignore important words",
                    CorrectAnswer = "A",
                    QuestionOrder = 9,
                    IsActive = true
                },

                new QuizQuestion
                {
                    QuizQuestionId = 70,
                    LearnerLessonId = 7,
                    Question = "What is the main purpose of learner's licence preparation?",
                    OptionA = "To help learners understand knowledge needed to drive safely",
                    OptionB = "To teach learners to drive without rules",
                    OptionC = "To increase vehicle speed",
                    OptionD = "To avoid learning road signs",
                    CorrectAnswer = "A",
                    QuestionOrder = 10,
                    IsActive = true
                }
            );
        }
    }
}