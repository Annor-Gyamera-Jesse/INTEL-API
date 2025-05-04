using Dapper;
using INTEL_API.ViewModels;
using INTEL_API.ViewModels.LEAVE_OF_ABSENCE;
using INTEL_API.ViewModels.LEAVE_OF_ABSENCE.INTEL_API.ViewModels;
using INTEL_API.ViewModels.PhotoRecord;
using INTEL_API.ViewModels.PhotoRecordVm;
using INTEL_API.ViewModels.SchoolTerm;
using INTEL_API.ViewModels.StudentInfoVm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Text;


namespace INTEL_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthService : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Login endpoint
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserViewModel request)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if the user exists
                string query = "SELECT UserID, UserName, Password FROM SchoolManagement.Users WHERE UserName = @UserName";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserName", request.UserName);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int userId = (int)reader["UserID"];
                    string storedPassword = reader["Password"].ToString();

                    if (storedPassword != request.Password)
                    {
                        return Unauthorized(new LoginResponse { IsAuthorized = false, Message = "Invalid credentials." });
                    }

                    reader.Close();

                    // Check the user's roles and if they are enabled
                    query = "SELECT RoleName, Enable, ClassID, MenuItem, CategoryName FROM SchoolManagement.MobileAppRoles WHERE UserID = @UserID";
                    cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string roleName = reader["RoleName"].ToString();
                        bool isEnabled = (bool)reader["Enable"];
                        string classID = reader["ClassID"].ToString();
                        string menuItem = reader["MenuItem"].ToString();
                        string categoryName = reader["CategoryName"].ToString();

                        if (isEnabled)
                        {
                            return Ok(new LoginResponse
                            {
                                IsAuthorized = true,
                                UserId = userId,  // Include UserId in the response
                                UserName = request.UserName,  // Include UserName in the response
                                Role = roleName,
                                ClassID = classID,
                                MenuItem = menuItem,
                                CategoryName = categoryName,
                                Message = "Login successful."
                            });
                        }
                    }

                    return Unauthorized(new LoginResponse { IsAuthorized = false, Message = "User is not enabled." });
                }
                else
                {
                    return Unauthorized(new LoginResponse { IsAuthorized = false, Message = "User does not exist." });
                }
            }
        }

        /*to get details for the Login screen and splashscreen*/
        [HttpGet("GetLoginScreenDetails")]
        public async Task<IActionResult> GetLoginScreenDetails()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = @"
        SELECT 
            Title,
            CompanyImage,
            SchoolName,
            CompanyRegisteredName,
            SoftWareVerssion,
            YEAR(GETDATE()) AS CompanyRegisteredDate
        FROM SchoolManagement.LoginScreenDetails";

            var result = await connection.QueryFirstOrDefaultAsync<LoginScreenDetailsViewModel>(query);

            if (result == null)
                return NotFound("No login screen details found.");

            return Ok(result);
        }

        // Endpoint to get users with roles
        [HttpGet("UsersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var usersWithRoles = new List<UserViewModel>();
                using (var command = new SqlCommand("SELECT ur.UserRoleID, u.UserID, u.UserName, u.Password, ur.RoleName, ur.Enable " +
                                                    "FROM SchoolManagement.MobileAppRoles ur " +
                                                    "JOIN SchoolManagement.Users u ON ur.UserID = u.UserID", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var userRoleID = reader.GetInt32(0);
                        var existingUser = usersWithRoles.Find(u => u.UserID == userRoleID);
                        if (existingUser == null)
                        {
                            var user = new UserViewModel
                            {
                                UserID = reader.GetInt32(1),
                                UserName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2), // Handle NULL
                                Password = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle NULL
                                Roles = new List<UserRoleViewModel>()
                            };
                            user.Roles.Add(new UserRoleViewModel
                            {
                                UserRoleID = reader.GetInt32(0),
                                RoleName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4), // Handle NULL
                                Enable = reader.GetBoolean(5)
                            });
                            usersWithRoles.Add(user);
                        }
                        else
                        {
                            existingUser.Roles.Add(new UserRoleViewModel
                            {
                                UserRoleID = reader.GetInt32(0),
                                RoleName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4), // Handle NULL
                                Enable = reader.GetBoolean(5)
                            });
                        }
                    }
                }

                return Ok(usersWithRoles);
            }
        }

        //this endpoint is responsible to handle Lesson note status per user or according to user
        [HttpGet("LessonNoteStatus/{userId}")]
        public async Task<IActionResult> GetLessonNoteStatusUpdates(int userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
            SELECT LessonnotesID, CASE Status 
                WHEN 1 THEN 'New'
                WHEN 2 THEN 'Approved'
                WHEN 3 THEN 'Cancelled'
            END AS Status, RecDateCreated
            FROM SchoolManagement.lessonnotes
            WHERE UserId = @UserId AND Status IN (1, 2, 3)
            ORDER BY RecDateCreated DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    var updates = new List<LessonNoteStatusUpdateViewModel>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            updates.Add(new LessonNoteStatusUpdateViewModel
                            {
                                LessonNoteId = reader.GetInt32(0),
                                Status = reader.GetString(1),
                                DateUpdated = reader.GetDateTime(2)
                            });
                        }
                    }

                    return Ok(updates);
                }
            }
        }

        //not in use 
        [HttpGet("ApprovedLessonNotes/{userId}")]
        public async Task<IActionResult> GetApprovedLessonNotes(int userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
            SELECT LessonnotesID, Status, RecDateCreated
            FROM SchoolManagement.lessonnotes
            WHERE UserId = @UserId AND Status = 2
            ORDER BY RecDateCreated DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    var lessonNotes = new List<LessonNoteStatusUpdateViewModel>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lessonNotes.Add(new LessonNoteStatusUpdateViewModel
                            {
                                LessonNoteId = reader.GetInt32(0),
                                Status = reader.GetString(1), // Assuming Status is an integer
                                DateUpdated = reader.GetDateTime(2)
                            });
                        }
                    }

                    return Ok(lessonNotes);
                }
            }
        }

        //[HttpGet("ApprovedLessonNotes")]
        //public async Task<IActionResult> GetApprovedLessonNotes()
        //{
        //    var connectionString = _configuration.GetConnectionString("DefaultConnection");
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        await connection.OpenAsync();

        //        var query = @"
        //    SELECT UserId, 
        //           CASE Status 
        //               WHEN 1 THEN 'New'
        //               WHEN 2 THEN 'Approved'
        //               WHEN 3 THEN 'Cancelled'
        //           END AS Status,
        //           RecDateCreated AS DateUpdated
        //    FROM SchoolManagement.lessonnotes
        //    WHERE Status = @ApprovedStatus
        //    ORDER BY RecDateCreated DESC";

        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.Parameters.AddWithValue("@ApprovedStatus", (int)LessonNoteStatus.Approved);

        //            var lessonNotes = new List<LessonNoteStatusUpdateViewModel>();
        //            using (var reader = await command.ExecuteReaderAsync())
        //            {
        //                while (await reader.ReadAsync())
        //                {
        //                    lessonNotes.Add(new LessonNoteStatusUpdateViewModel
        //                    {
        //                        LessonNoteId = reader.GetInt32(0),
        //                        Status = reader.GetString(1),
        //                        DateUpdated = reader.GetDateTime(2)
        //                    });
        //                }
        //            }

        //            return Ok(lessonNotes);
        //        }
        //    }
        //}

        //this is was the old lessonNot post api

        [HttpGet("ApprovedLessonNotes")]
        public async Task<IActionResult> GetApprovedLessonNotes()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT LessonnotesID,
               UserId, 
               CASE Status 
                   WHEN 1 THEN 'New'
                   WHEN 2 THEN 'Approved'
                   WHEN 3 THEN 'Cancelled'
               END AS Status,
               UpdatedOn AS DateUpdated
        FROM SchoolManagement.TEACHERSLESSONNOTES
        WHERE Status = @ApprovedStatus
        ORDER BY UpdatedOn DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ApprovedStatus", (int)LessonNoteStatus.Approved);

            var lessonNotes = new List<LessonNoteStatusUpdateViewModel>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lessonNotes.Add(new LessonNoteStatusUpdateViewModel
                {
                    LessonNoteId = reader.GetInt32(0), // LessonnotesID
                    UserId = reader.GetInt32(1),        // UserId
                    Status = reader.GetString(2),       // Status (string)
                    DateUpdated = reader.GetDateTime(3) // UpdatedOn
                });
            }

            return Ok(lessonNotes);
        }

        /*NOT IN USE*/
        [HttpPost("postLessonNote")]
        public async Task<IActionResult> PostLessonNote([FromBody] LessonNoteViewModel lessonNote)
        {
            if (lessonNote == null)
            {
                return BadRequest("Invalid lesson note data.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            INSERT INTO SchoolManagement.lessonnotes 
            (UserId, SchoolCourse, Topic, OBJECTIVES, TLMTLA, INTRODUCTION, COREPOINTS, EVALUATIONREMARKS, Status)
            VALUES 
            (@UserId, @SchoolCourse, @Topic, @OBJECTIVES, @TLMTLA, @INTRODUCTION, @COREPOINTS, @EVALUATIONREMARKS, @Status)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", lessonNote.UserId);
                    command.Parameters.AddWithValue("@SchoolCourse", lessonNote.SchoolCourse);
                    command.Parameters.AddWithValue("@Topic", lessonNote.Topic);
                    command.Parameters.AddWithValue("@OBJECTIVES", lessonNote.OBJECTIVES);
                    command.Parameters.AddWithValue("@TLMTLA", lessonNote.TLMTLA);
                    command.Parameters.AddWithValue("@INTRODUCTION", lessonNote.INTRODUCTION);
                    command.Parameters.AddWithValue("@COREPOINTS", lessonNote.COREPOINTS);
                    command.Parameters.AddWithValue("@EVALUATIONREMARKS", lessonNote.EVALUATIONREMARKS);
                    command.Parameters.AddWithValue("@Status", (int)LessonNoteStatus.New);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        return Ok("Lesson note posted successfully.");
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }
            }
        }

        //thi is the new lessonnote api
        //thi is the new lessonnote api
        [HttpPost("PostTEACHERSLESSONNOTES")]
        public async Task<IActionResult> PostTEACHERSLESSONNOTES([FromBody] TeachersLessonNoteViewModel lessonNote)
        {
            if (lessonNote == null)
            {
                return BadRequest("Invalid lesson note data.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
        INSERT INTO SchoolManagement.TEACHERSLESSONNOTES 
        (UserId, SchoolCourse, Strand, SubStrand, ContentStandard, Indicator, 
         TeachingLearningResources, TeachingLearningResourcePreparationNotes, 
         SourcesLearningResources, LearningGroup, LearnerExpectation, 
         ImportantGradeExpectation, LearningOutcomes, FormofAssessment, 
         LearnerEntryBehavior, SequenceofLesson, ClassID, Week_Ending, Status, TermID, Time_Period, LessonNoteDate)
        VALUES 
        (@UserId, @SchoolCourse, @Strand, @SubStrand, @ContentStandard, @Indicator, 
         @TeachingLearningResources, @TeachingLearningResourcePreparationNotes, 
         @SourcesLearningResources, @LearningGroup, @LearnerExpectation, 
         @ImportantGradeExpectation, @LearningOutcomes, @FormofAssessment, 
         @LearnerEntryBehavior, @SequenceofLesson, @ClassID, @Week_Ending, @Status, @TermID, @Time_Period, @LessonNoteDate)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", lessonNote.UserId);
                    command.Parameters.AddWithValue("@SchoolCourse", lessonNote.SchoolCourse);
                    command.Parameters.AddWithValue("@Strand", lessonNote.Strand);
                    command.Parameters.AddWithValue("@SubStrand", lessonNote.SubStrand);
                    command.Parameters.AddWithValue("@ContentStandard", lessonNote.ContentStandard);
                    command.Parameters.AddWithValue("@Indicator", lessonNote.Indicator);
                    command.Parameters.AddWithValue("@TeachingLearningResources", lessonNote.TeachingLearningResources);
                    command.Parameters.AddWithValue("@TeachingLearningResourcePreparationNotes", lessonNote.TeachingLearningResourcePreparationNotes);
                    command.Parameters.AddWithValue("@SourcesLearningResources", lessonNote.SourcesLearningResources);
                    command.Parameters.AddWithValue("@LearningGroup", lessonNote.LearningGroup);
                    command.Parameters.AddWithValue("@LearnerExpectation", lessonNote.LearnerExpectation);
                    command.Parameters.AddWithValue("@ImportantGradeExpectation", lessonNote.ImportantGradeExpectation);
                    command.Parameters.AddWithValue("@LearningOutcomes", lessonNote.LearningOutcomes);
                    command.Parameters.AddWithValue("@FormofAssessment", lessonNote.FormofAssessment);
                    command.Parameters.AddWithValue("@LearnerEntryBehavior", lessonNote.LearnerEntryBehavior);
                    command.Parameters.AddWithValue("@SequenceofLesson", lessonNote.SequenceofLesson);
                    command.Parameters.AddWithValue("@ClassID", lessonNote.ClassID);
                    command.Parameters.AddWithValue("@Week_Ending", lessonNote.WeekEnding);
                    command.Parameters.AddWithValue("@Status", 1); // Every new post has status = 1 thats new
                    command.Parameters.AddWithValue("@TermID", lessonNote.TermID);
                    command.Parameters.AddWithValue("@Time_Period", lessonNote.Time_Period);                    
                    command.Parameters.AddWithValue("@LessonNoteDate", lessonNote.LessonNoteDate);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        return Ok("Lesson note posted successfully.");
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }
            }
        }


        // Endpoint to get roles for a specific user
        [HttpGet("UserRoles/{userId}")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var userRoles = new List<UserRoleViewModel>();
                using (var command = new SqlCommand("SELECT UserRoleID, RoleName, Enable FROM SchoolManagement.MobileAppRoles WHERE UserID = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var userRole = new UserRoleViewModel
                            {
                                UserRoleID = reader.GetInt32(0),
                                RoleName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1), // Handle NULL value for RoleName
                                Enable = reader.GetBoolean(2)
                            };
                            userRoles.Add(userRole);
                        }
                    }
                }

                return Ok(userRoles);
            }
        }

        //Get Users with their userid to determain their the specific user menu
        [HttpGet("MenuItems/{userId}")]
        public async Task<IActionResult> GetMenuItems(int userId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Updated query to join MobileAppRoles with MobileAppMenuDisplay and filter by Enable
                string query = @"
            SELECT DISTINCT mad.CategoryName, mad.MenuItem
            FROM SchoolManagement.MobileAppRoles mar
            JOIN SchoolManagement.MobileAppMenuDisplay mad ON mar.MenuItem = mad.MenuItem
            WHERE mar.UserID = @UserID AND mar.Enable = 1";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userId);

                var menuItems = new List<MenuItem>();
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var categoryName = reader["CategoryName"].ToString();
                        var menuItemName = reader["MenuItem"].ToString();

                        menuItems.Add(new MenuItem
                        {
                            CategoryName = categoryName,
                            MenuItemName = menuItemName
                        });
                    }
                }

                return Ok(menuItems);
            }
        }

        /*to get teachers task*/
        [HttpGet]
        [Route("GetTeachersTasks")]
        public IActionResult GetTeachersTasks()
        {
            IEnumerable<TeachersTaskViewModel> tasks;
            string query = "SELECT TeachersName, TeacherTask, SwitchBar, StartDate, EndDate, Status FROM SchoolManagement.TeachersTask";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    tasks = db.Query<TeachersTaskViewModel>(query);
                    return Ok(tasks);
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, new { message = "An error occurred while retrieving tasks.", error = ex.Message });
                }
            }
        }

        // Endpoint to get all students
        [HttpGet("Students")]
        public async Task<IActionResult> GetStudents()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var students = new List<Student>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.Students", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var student = new Student
                        {
                            StudentID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            StudentFirstName = reader.IsDBNull(1) ? null : reader.GetString(1),
                            StudentLastName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            StudentDateOfBirth = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                            StudentGender = reader.IsDBNull(4) ? '\0' : reader.GetString(4)[0],
                            StudentAddress = reader.IsDBNull(5) ? null : reader.GetString(5),
                            StudentPhoneNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                            StudentEmail = reader.IsDBNull(7) ? null : reader.GetString(7),
                            ImageData = reader.IsDBNull(8) ? null : (byte[])reader.GetValue(8),
                            ClassID = reader.IsDBNull(9) ? null : reader.GetString(9),
                            GuardianFullName = reader.IsDBNull(10) ? null : reader.GetString(10),
                            GuardianGender = reader.IsDBNull(11) ? '\0' : reader.GetString(11)[0],
                            GuardianHouseAddress = reader.IsDBNull(12) ? null : reader.GetString(12),
                            GuardianWorkAddress = reader.IsDBNull(13) ? null : reader.GetString(13),
                            GuardianEmail = reader.IsDBNull(14) ? null : reader.GetString(14),
                            GuardianFirstContact = reader.IsDBNull(15) ? null : reader.GetString(15),
                            GuardianSecondContact = reader.IsDBNull(16) ? null : reader.GetString(16),
                            EnableSwitch = reader.IsDBNull(17) ? false : reader.GetBoolean(17)
                        };

                        students.Add(student);
                    }
                }

                return Ok(students);
            }
        }

        // Endpoint to get all teachers
        [HttpGet("Teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var teachers = new List<Teacher>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.Teacher", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            var teacher = new Teacher
                            {
                                TeacherID = reader.GetInt32(0),
                                TeacherFirstName = reader.GetString(1),
                                TeacherLastName = reader.GetString(2),
                                TeacherDateOfBirth = reader.GetDateTime(3),
                                TeacherGender = reader.GetString(4),
                                TeacherAddress = reader.GetString(5),
                                TeacherPhoneNumber = reader.GetString(6),
                                TeacherEmail = reader.GetString(7),
                                GuardianFullName = reader.GetString(8),
                                GuardianGender = reader.GetString(9),
                                GuardianHouseAddress = reader.GetString(10),
                                GuardianWorkAddress = reader.GetString(11),
                                GuardianEmail = reader.GetString(12),
                                GuardianFirstContact = reader.GetString(13),
                                GuardianSecondContact = reader.GetString(14),
                                EnableSwitch = reader.GetBoolean(15),
                                ClockIN = reader.GetDateTime(16),
                                ClockOUT = reader.GetDateTime(17)
                            };

                            // Add logging statements
                            Console.WriteLine($"TeacherID: {teacher.TeacherID}, ClockIN: {teacher.ClockIN}, ClockOUT: {teacher.ClockOUT}");

                            teachers.Add(teacher);
                        }
                        catch (Exception ex)
                        {
                            // Log or print the exception details for further investigation
                            Console.WriteLine($"Error retrieving data: {ex.Message}");
                        }
                    }
                }

                return Ok(teachers);
            }
        }

        // Endpoint to get all non-teaching staffs
        [HttpGet("NonTeachingStaffs")]
        public async Task<IActionResult> GetNonTeachingStaffs()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var nonTeachingStaffs = new List<NonTeachingStaff>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.NonTeachingStaffs", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var nonTeachingStaff = new NonTeachingStaff
                        {
                            NTSID = reader.GetInt32(0),
                            NonTeachingStaffsFirstName = reader.GetString(1),
                            NonTeachingStaffsLastName = reader.GetString(2),
                            NonTeachingStaffsDateOfBirth = reader.GetDateTime(3),
                            NonTeachingStaffsGender = reader.GetChar(4),
                            NonTeachingStaffsAddress = reader.GetString(5),
                            NonTeachingStaffsPhoneNumber = reader.GetString(6),
                            NonTeachingStaffsEmail = reader.GetString(7),
                            ImageData = reader.IsDBNull(8) ? null : (byte[])reader[8],
                            GuardianFullName = reader.GetString(9),
                            GuardianGender = reader.GetChar(10),
                            GuardianHouseAddress = reader.GetString(11),
                            GuardianWorkAddress = reader.GetString(12),
                            GuardianEmail = reader.GetString(13),
                            GuardianFirstContact = reader.GetString(14),
                            GuardianSecondContact = reader.GetString(15),
                            // Add property for Position if needed
                        };

                        nonTeachingStaffs.Add(nonTeachingStaff);
                    }
                }

                return Ok(nonTeachingStaffs);
            }
        }

        // Endpoint to get all courses
        [HttpGet("Courses")]
        public async Task<IActionResult> GetCourses()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var courses = new List<Course>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.Courses", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var course = new Course
                        {
                            CourseID = reader.GetInt32(0),
                            CourseName = reader.GetString(1),
                            Description = reader.GetString(2)
                        };
                        courses.Add(course);
                    }
                }

                return Ok(courses);
            }
        }

        // Endpoint to get all classes
        [HttpGet("Classes")]
        public async Task<IActionResult> GetClasses()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var classes = new List<Classes>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.Classes", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var classInfo = new Classes
                        {
                            ClassID = reader.GetString(0),
                            ClassName = reader.GetString(1),
                            TeacherFirstName = reader.GetString(2),
                            TeacherLastName = reader.GetString(3),
                            TeacherPhoneNumber = reader.GetString(4)
                        };
                        classes.Add(classInfo);
                    }
                }

                return Ok(classes);
            }
        }

        // Endpoint to get all class
        [HttpGet("Class")]
        public async Task<IActionResult> GetClass()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var classes = new List<Class>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.Class", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var schoolClass = new Class
                        {
                            ClassID = reader.GetString(0),
                        };
                        classes.Add(schoolClass);
                    }
                }

                return Ok(classes);
            }
        }

        // Endpoint to get all student course registrations
        [HttpGet("StudentCourseRegistrations")]
        public async Task<IActionResult> GetStudentCourseRegistrations()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var registrations = new List<StudentCourseRegistration>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.StudentCourseRegistrations", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var registration = new StudentCourseRegistration
                        {
                            RegistrationID = reader.GetInt32(0),
                            StudentID = reader.GetInt32(1),
                            CourseID = reader.GetInt32(2),
                            RegistrationDate = reader.GetDateTime(3),
                        };
                        registrations.Add(registration);
                    }
                }

                return Ok(registrations);
            }
        }

        // Endpoint to get all school courses
        [HttpGet("SchoolCourses")]
        public async Task<IActionResult> GetSchoolCourses()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var schoolCourses = new List<SchoolCourse>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.SchoolCourse", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var course = new SchoolCourse
                        {
                            SCID = reader.GetString(0),
                            CourseName = reader.GetString(1),
                        };
                        schoolCourses.Add(course);
                    }
                }

                return Ok(schoolCourses);
            }
        }

        // Endpoint to get all class scores
        [HttpGet("ClassScores")]
        public async Task<IActionResult> GetClassScores()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var classScores = new List<ClassScore>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.ClassScores", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var score = new ClassScore
                        {
                            ScoreID = reader.GetInt32(0),
                            StudentID = reader.GetInt32(1),
                            StudentFirstName = reader.GetString(2),
                            StudentLastName = reader.GetString(3),
                            ClassName = reader.GetString(4),
                            SchoolCourse = reader.GetString(5),
                            Score = reader.GetInt32(6),
                        };
                        classScores.Add(score);
                    }
                }

                return Ok(classScores);
            }
        }

        // Endpoint to get all school exams
        [HttpGet("SchoolExams")]
        public async Task<IActionResult> GetSchoolExams()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var schoolExams = new List<SchoolExam>();
                try
                {
                    using (var command = new SqlCommand("SELECT * FROM SchoolManagement.SchoolExams", connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var exam = new SchoolExam
                            {
                                ExamID = reader.GetInt32(0),
                                StudentName = reader.GetString(1),
                                ClassName = reader.GetString(2),
                                AcademicYear = reader.GetString(3),
                                VacationDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                PromotedTo = reader.GetString(5),
                                NumberOnRoll = reader.GetInt32(6),
                                TermID = reader.GetInt32(7),
                                Position = reader.GetString(8),
                                NextTermsBegins = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                                AttendanceOut = reader.GetInt32(10),
                                AttendanceIn = reader.GetInt32(11),
                                SchoolCourse = reader.GetString(12),
                                ClassScore = reader.GetInt32(13),
                                ExamsScore = reader.GetInt32(14),
                                TotalScore = reader.GetInt32(15),
                                SubjectsPositions = reader.GetString(16),
                                Grade = reader.GetString(17),
                                TeachersRemarks = reader.IsDBNull(18) ? null : reader.GetString(18),
                                Conduct = reader.IsDBNull(19) ? null : reader.GetString(19),
                                HeadmasterRemark = reader.IsDBNull(20) ? null : reader.GetString(20),
                                SchoolInformation = reader.IsDBNull(21) ? null : reader.GetString(21),
                                TeachersSignature = reader.IsDBNull(22) ? null : reader.GetString(22),
                                HeadMasterSignature = reader.IsDBNull(23) ? null : reader.GetString(23),
                            };

                            schoolExams.Add(exam);
                        }
                    }

                    return Ok(schoolExams);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetSchoolExams: {ex.Message}");
                    return StatusCode(500, "Internal Server Error");
                }
            }
        }


        // Endpoint to get all students attendance
        [HttpGet("StudentsAttendance")]
        public async Task<IActionResult> GetStudentsAttendance()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var studentsAttendance = new List<StudentsAttendance>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.StudentsAttendance", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var attendance = new StudentsAttendance
                        {
                            AttendanceID = reader.GetInt32(0),
                            StudentFirstName = reader.GetString(1),
                            StudentLastName = reader.GetString(2),
                            ClassID = reader.GetString(3),
                            EnableSwitch = reader.GetBoolean(4)
                        };
                        studentsAttendance.Add(attendance);
                    }
                }

                return Ok(studentsAttendance);
            }
        }

        // Endpoint to get all teachers attendance
        [HttpGet("TeachersAttendance")]
        public async Task<IActionResult> GetTeachersAttendance()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var teachersAttendance = new List<TeachersAttendance>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.TeachersAttendance", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var attendance = new TeachersAttendance
                        {
                            TeachersAttendanceID = reader.GetInt32(0),
                            TeacherFirstName = reader.GetString(1),
                            TeacherLastName = reader.GetString(2),
                            EnableSwitch = reader.GetBoolean(3),
                            ClockIN = reader.GetDateTime(4)
                        };
                        teachersAttendance.Add(attendance);
                    }
                }

                return Ok(teachersAttendance);
            }
        }

        [HttpGet("TeacherSubjectAssignments")]
        public async Task<IActionResult> GetTeacherSubjectAssignments()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var assignments = new List<TeacherSubjectAssignmentViewModel>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT tsa.AssignmentID, 
                           CONCAT(t.TeacherFirstName, ' ', t.TeacherLastName) AS TeacherName, 
                           tsa.SCID, 
                           tsa.ClassID, 
                           tsa.DayID, 
                           tsa.SubjectStartTime, 
                           tsa.SubjectEndTime, 
                           tsa.RecDateCreated
                    FROM SchoolManagement.TeacherSubjectAssignment tsa
                    INNER JOIN SchoolManagement.Teacher t ON tsa.TeacherID = t.TeacherID
                    INNER JOIN SchoolManagement.StudentTimetable_Days std ON tsa.DayID = std.DayID";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var assignment = new TeacherSubjectAssignmentViewModel
                            {
                                AssignmentID = reader.GetInt32(0),
                                TeacherName = reader.GetString(1),
                                SCID = reader.GetString(2),
                                ClassID = reader.GetString(3),
                                DayID = reader.GetInt32(4),
                                SubjectStartTime = reader.GetDateTime(5),
                                SubjectEndTime = reader.GetDateTime(6),
                                RecDateCreated = reader.GetDateTime(7)
                            };
                            assignments.Add(assignment);
                        }
                    }
                }
            }

            return Ok(assignments);
        }

        // Endpoint to get all teachers attendance out
        [HttpGet("TeachersAttendanceOut")]
        public async Task<IActionResult> GetTeachersAttendanceOut()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var teachersAttendanceOut = new List<TeachersAttendanceOut>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.TeachersAttendanceOut", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var attendanceOut = new TeachersAttendanceOut
                        {
                            TeachersAttendanceOutID = reader.GetInt32(0),
                            TeacherFirstName = reader.GetString(1),
                            TeacherLastName = reader.GetString(2),
                            EnableSwitch = reader.GetBoolean(3),
                            ClockIN = reader.GetDateTime(4),
                            ClockOUT = reader.GetDateTime(5)
                        };
                        teachersAttendanceOut.Add(attendanceOut);
                    }
                }

                return Ok(teachersAttendanceOut);
            }
        }

        // Endpoint to get all non-teaching staffs attendance
        [HttpGet("NonTeachingStaffsAttendance")]
        public async Task<IActionResult> GetNonTeachingStaffsAttendance()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var nonTeachingStaffsAttendance = new List<NonTeachingStaffsAttendance>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.NonTeachingStaffsAttendance", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var attendance = new NonTeachingStaffsAttendance
                        {
                            NonTeachingStaffsAttendanceID = reader.GetInt32(0),
                            NonTeachingStaffsFirstName = reader.GetString(1),
                            NonTeachingStaffsLastName = reader.GetString(2),
                            EnableSwitch = reader.GetBoolean(3),
                            ClockIN = reader.GetDateTime(4),
                            ClockOUT = reader.GetDateTime(5)
                        };
                        nonTeachingStaffsAttendance.Add(attendance);
                    }
                }

                return Ok(nonTeachingStaffsAttendance);
            }
        }

        /*END-POINT TO GET ALL SCHOOL TERMS*/
        [HttpGet("GetAllTerms")]
        public IActionResult GetAllTerms()
        {
            var terms = new List<SchoolTerm>();

            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    string query = "SELECT TermID, Term FROM SchoolManagement.SchoolTerm";

                    using (var command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                terms.Add(new SchoolTerm
                                {
                                    TermID = Convert.ToInt32(reader["TermID"]),
                                    Term = reader["Term"].ToString()
                                });
                            }
                        }
                    }
                }

                return Ok(terms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error fetching terms: " + ex.Message);
            }
        }
        
        // Endpoint to get all report view pages
        [HttpGet("ReportViewPages")]
        public async Task<IActionResult> GetReportViewPages()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var reportViewPages = new List<ReportViewPage>();
                using (var command = new SqlCommand("SELECT * FROM SchoolManagement.ReportViewPage", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var reportViewPage = new ReportViewPage
                        {
                            ReportViewPageID = reader.GetInt32(0),
                            ReportViewPageContent = reader.GetString(1)
                        };
                        reportViewPages.Add(reportViewPage);
                    }
                }

                return Ok(reportViewPages);
            }
        }

        // Endpoint to add a new school exam
        [HttpPost("AddSchoolExam")]
        public async Task<IActionResult> AddSchoolExam([FromBody] SchoolExam schoolExam)
        {
            if (schoolExam == null)
            {
                return BadRequest("Invalid school exam data");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert exam record
                        using (var command = new SqlCommand(
                            "INSERT INTO SchoolManagement.SchoolExams (StudentName, ClassName, AcademicYear, VacationDate, PromotedTo, NumberOnRoll, TermID, " +
                            "Position, NextTermsBegins, AttendanceOut, AttendanceIn, SchoolCourse, ClassScore, ExamsScore, TotalScore, SubjectsPositions, " +
                            "Grade, TeachersRemarks, Conduct, HeadmasterRemark, SchoolInformation, TeachersSignature, HeadMasterSignature, UserID) " +
                            "VALUES (@StudentName, @ClassName, @AcademicYear, @VacationDate, @PromotedTo, @NumberOnRoll, @TermID, @Position, @NextTermsBegins, " +
                            "@AttendanceOut, @AttendanceIn, @SchoolCourse, @ClassScore, @ExamsScore, @TotalScore, @SubjectsPositions, @Grade, @TeachersRemarks, " +
                            "@Conduct, @HeadmasterRemark, @SchoolInformation, @TeachersSignature, @HeadMasterSignature, @UserID)",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@StudentName", schoolExam.StudentName);
                            command.Parameters.AddWithValue("@ClassName", schoolExam.ClassName);
                            command.Parameters.AddWithValue("@AcademicYear", schoolExam.AcademicYear);
                            command.Parameters.AddWithValue("@VacationDate", (object)schoolExam.VacationDate ?? DBNull.Value);
                            command.Parameters.AddWithValue("@PromotedTo", schoolExam.PromotedTo);
                            command.Parameters.AddWithValue("@NumberOnRoll", schoolExam.NumberOnRoll);
                            command.Parameters.AddWithValue("@TermID", schoolExam.TermID);
                            command.Parameters.AddWithValue("@Position", schoolExam.Position);
                            command.Parameters.AddWithValue("@NextTermsBegins", (object)schoolExam.NextTermsBegins ?? DBNull.Value);
                            command.Parameters.AddWithValue("@AttendanceOut", (object)schoolExam.AttendanceOut ?? DBNull.Value);
                            command.Parameters.AddWithValue("@AttendanceIn", (object)schoolExam.AttendanceIn ?? DBNull.Value);
                            command.Parameters.AddWithValue("@SchoolCourse", schoolExam.SchoolCourse);
                            command.Parameters.AddWithValue("@ClassScore", schoolExam.ClassScore);
                            command.Parameters.AddWithValue("@ExamsScore", schoolExam.ExamsScore);
                            command.Parameters.AddWithValue("@TotalScore", schoolExam.TotalScore);
                            command.Parameters.AddWithValue("@SubjectsPositions", schoolExam.SubjectsPositions);
                            command.Parameters.AddWithValue("@Grade", schoolExam.Grade);
                            command.Parameters.AddWithValue("@TeachersRemarks", schoolExam.TeachersRemarks);
                            command.Parameters.AddWithValue("@Conduct", schoolExam.Conduct);
                            command.Parameters.AddWithValue("@HeadmasterRemark", (object)schoolExam.HeadmasterRemark ?? DBNull.Value);
                            command.Parameters.AddWithValue("@SchoolInformation", (object)schoolExam.SchoolInformation ?? DBNull.Value);
                            command.Parameters.AddWithValue("@TeachersSignature", (object)schoolExam.TeachersSignature ?? DBNull.Value);
                            command.Parameters.AddWithValue("@HeadMasterSignature", (object)schoolExam.HeadMasterSignature ?? DBNull.Value);
                            command.Parameters.AddWithValue("@UserID", schoolExam.UserId);
                            await command.ExecuteNonQueryAsync();
                        }

                        // Update student class
                        using (var updateCommand = new SqlCommand(
                            "UPDATE SchoolManagement.Students SET ClassID = @PromotedTo WHERE StudentFirstName + ' ' + StudentLastName = @StudentName",
                            connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@PromotedTo", schoolExam.PromotedTo);
                            updateCommand.Parameters.AddWithValue("@StudentName", schoolExam.StudentName);
                            await updateCommand.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        return Ok("School exam added and student promoted successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, $"Error: {ex.Message}");
                    }
                }
            }
        }


        // Endpoint to add a new student attendance
        [HttpPost("AddStudentAttendance")]
        public async Task<IActionResult> AddStudentAttendance([FromBody] List<StudentsAttendance> studentAttendances)
        {
            if (studentAttendances == null || !studentAttendances.Any())
            {
                return BadRequest("Invalid student attendance data");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Check if attendance has already been submitted for the selected class and date
                var userId = studentAttendances.First().UserId;
                var classId = studentAttendances.First().ClassID;
                var currentDate = DateTime.Today;

                using (var checkCommand = new SqlCommand(
                    "SELECT COUNT(1) FROM SchoolManagement.StudentsAttendance WHERE UserID = @UserID AND ClassID = @ClassID AND CAST(RecDateCreated AS DATE) = @CurrentDate",
                    connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserID", userId);
                    checkCommand.Parameters.AddWithValue("@ClassID", classId);
                    checkCommand.Parameters.AddWithValue("@CurrentDate", currentDate);

                    var count = (int)await checkCommand.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        return BadRequest("Attendance has already been submitted for today.");
                    }
                }

                foreach (var studentAttendance in studentAttendances)
                {
                    using (var command = new SqlCommand(
                        "INSERT INTO SchoolManagement.StudentsAttendance (StudentFirstName, StudentLastName, ClassID, EnableSwitch, UserID, RecDateCreated) " +
                        "VALUES (@StudentFirstName, @StudentLastName, @ClassID, @EnableSwitch, @UserID, GETDATE())",
                        connection))
                    {
                        command.Parameters.AddWithValue("@StudentFirstName", studentAttendance.StudentFirstName);
                        command.Parameters.AddWithValue("@StudentLastName", studentAttendance.StudentLastName);
                        command.Parameters.AddWithValue("@ClassID", studentAttendance.ClassID);
                        command.Parameters.AddWithValue("@EnableSwitch", studentAttendance.EnableSwitch);
                        command.Parameters.AddWithValue("@UserID", studentAttendance.UserId);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok("Student attendance added successfully");
            }
        }

        [HttpPost("logerror")]
        public async Task<IActionResult> LogError([FromBody] ErrorLogEntry logEntry)
        {
            if (logEntry == null)
            {
                return BadRequest("Invalid log entry.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(
                    "INSERT INTO SchoolManagementSecurity.ErrorLog (LogDate, ErrorMessage, StackTrace, ControllerName, ActionName, UserID) " +
                    "VALUES (@LogDate, @ErrorMessage, @StackTrace, @ControllerName, @ActionName, @UserID)",
                    connection
                );
                command.Parameters.Add(new SqlParameter("@LogDate", SqlDbType.DateTime) { Value = logEntry.LogDate });
                command.Parameters.Add(new SqlParameter("@ErrorMessage", SqlDbType.NVarChar) { Value = logEntry.ErrorMessage });
                command.Parameters.Add(new SqlParameter("@StackTrace", SqlDbType.NVarChar) { Value = logEntry.StackTrace ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@ControllerName", SqlDbType.NVarChar) { Value = logEntry.ControllerName ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@ActionName", SqlDbType.NVarChar) { Value = logEntry.ActionName ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = logEntry.UserID ?? (object)DBNull.Value });  // Add UserID parameter

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }

            return Ok("Error logged successfully.");
        }

        // later update GET: api/NoticeBoard the initial idea was to display message to user with the help of their role so like message for role parents only parent get to view that message
        [HttpGet("GetNotices")]
        public async Task<IActionResult> GetNotices([FromQuery] int userId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var notices = await connection.QueryAsync<NoticeBoardViewModel>(
                    @"SELECT n.*, COALESCE(unrs.IsRead, 0) AS IsRead
              FROM SchoolManagement.NoticeBoard n
              LEFT JOIN SchoolManagement.UserNoticeReadStatus unrs
              ON n.NoticeID = unrs.NoticeID AND unrs.UserID = @UserID
              WHERE n.IsActive = 1",
                    new { UserID = userId }
                );

                return Ok(notices);
            }
        }

        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead([FromBody] UserNoticeReadStatusDto dto)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                // Insert or update the read status
                var result = await connection.ExecuteAsync(
                    @"IF EXISTS (SELECT 1 FROM SchoolManagement.UserNoticeReadStatus WHERE UserID = @UserID AND NoticeID = @NoticeID)
              UPDATE SchoolManagement.UserNoticeReadStatus SET IsRead = 1, ReadDate = GETDATE() WHERE UserID = @UserID AND NoticeID = @NoticeID
              ELSE
              INSERT INTO SchoolManagement.UserNoticeReadStatus (UserID, NoticeID, IsRead, ReadDate) VALUES (@UserID, @NoticeID, 1, GETDATE())",
                    new { dto.UserID, dto.NoticeID }
                );

                if (result > 0)
                {
                    return Ok();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Error marking notice as read.");
            }
        }

        /*to get notice board message for all user to see*/
        [HttpGet]
        [Route("GetNoticeBoard")]
        public IActionResult GetNotices()
        {
            IEnumerable<NoticeViewModel> notices;
            string query = "SELECT Title, Content, Author FROM SchoolManagement.NoticeBoard";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    notices = db.Query<NoticeViewModel>(query);
                    return Ok(notices);
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, new { message = "An error occurred while retrieving notices.", error = ex.Message });
                }
            }
        }

        /*get student by their class*/
        [HttpGet]
        [Route("GetStudentsByClass")]
        public IActionResult GetStudentsByClass(string classID)
        {
            IEnumerable<object> students;
            string query = "SELECT StudentID, CONCAT(StudentFirstName, ' ', StudentLastName) AS FullName, ClassID FROM SchoolManagement.Students WHERE ClassID = @ClassID";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    students = db.Query(query, new { ClassID = classID });
                    return Ok(students);
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, new { message = "An error occurred while retrieving students.", error = ex.Message });
                }
            }
        }


        [HttpPost("SaveAssessment")]
        public IActionResult SaveAssessment([FromBody] TeachersAssessmentViewModel model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Invalid assessment data provided." });
            }

            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    var query = @"
                INSERT INTO SchoolManagement.TeachersAssesment 
                (StudentName, ClassID, TEST1, TEST2, GROUPWORK, HOMEWORK, CLASSTEST, TOTAL_X, EXAMS_SCORE, Y, X_Y, POSITION, UserID, TermID)
                VALUES (@StudentName, @ClassID, @TEST1, @TEST2, @GROUPWORK, @HOMEWORK, @CLASSTEST, @TOTAL_X, @EXAMS_SCORE, @Y, @X_Y, @POSITION, @UserID, @TermID)";

                    var parameters = new
                    {
                        StudentName = $"{model.StudentFirstName} {model.StudentLastName}", // Combine first and last name
                        model.ClassID,
                        model.TEST1,
                        model.TEST2,
                        model.GROUPWORK,
                        model.HOMEWORK,
                        model.CLASSTEST,
                        model.TOTAL_X,
                        model.EXAMS_SCORE,
                        model.Y,
                        model.X_Y,
                        model.POSITION,
                        model.UserID,
                        model.TermID
                    };

                    connection.Execute(query, parameters);
                }

                return Ok(new { message = "Assessment saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while saving the assessment.", error = ex.Message });
            }
        }


        /*Fees*/
        // Endpoint to get fee details for a student by StudentID
        [HttpGet("GetFeeDetails/{studentId}")]
        public async Task<IActionResult> GetFeeDetails(int studentId)
        {
            // Create the SQL query to fetch the fee details for the given StudentID
            string sqlQuery = @"
                SELECT 
                    f.FeeID,
                    f.StudentID,
                    f.StudentName,
                    f.FeeTypeName,
                    f.ClassID,
                    f.AmountPaid,
                    f.AmountLeft,
                    f.PaymentDate,
                    f.DueDate,
                    f.Note
                FROM SchoolManagement.StudentFees f
                WHERE f.StudentID = @StudentID
            ";

            try
            {
                // Open the database connection
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    // Query the database and retrieve the fee details for the student
                    var feeDetails = await connection.QueryAsync<StudentFee>(sqlQuery, new { StudentID = studentId });

                    // If no records found, return a NotFound response
                    if (feeDetails == null || !feeDetails.Any())
                    {
                        return NotFound($"No fee records found for student with ID {studentId}");
                    }

                    // Return the fee details as a response
                    return Ok(feeDetails);
                }
            }
            catch (Exception ex)
            {
                // Return a 500 error if something goes wrong
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Search Student by StudentID
        [HttpGet]
        [Route("SearchStudentById/{studentId}")]
        public IActionResult SearchStudentById(int studentId)
        {
            string query = @"SELECT s.StudentFirstName, s.StudentLastName, s.ClassID, s.ImageData
                             FROM SchoolManagement.Students s
                             WHERE s.StudentID = @StudentID";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    var student = db.QuerySingleOrDefault(query, new { StudentID = studentId });

                    if (student == null)
                    {
                        return NotFound(new { message = "Student not found." });
                    }

                    return Ok(student);
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, new { message = "An error occurred while retrieving the student.", error = ex.Message });
                }
            }
        }

        // Get Fee Types by ClassID
        [HttpGet]
        [Route("GetFeeTypes/{classId}")]
        public IActionResult GetFeeTypes(string classId)
        {
            string query = @"
        SELECT FeeTypeName, Amount, ClassID 
        FROM SchoolManagement.FeeTypes
        WHERE ClassID = @ClassID 
        AND DeletedBy IS NULL"; // Ensure the fee type has not been deleted

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    var feeTypes = db.Query(query, new { ClassID = classId });

                    return Ok(feeTypes);
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, new { message = "An error occurred while retrieving fee types.", error = ex.Message });
                }
            }
        }


        // Make Payment and Update StudentFees
        [HttpPost]
        [Route("MakePayment")]
        public IActionResult MakePayment([FromBody] PaymentViewModel payment)
        {
            string query = @"INSERT INTO SchoolManagement.StudentFees 
                             (StudentID, FeeTypeID, StudentName, FeeTypeName, ClassID, AmountPaid, AmountLeft, PaymentDate, UserID)
                             VALUES (@StudentID, @FeeTypeID, @StudentName, @FeeTypeName, @ClassID, @AmountPaid, @AmountLeft, @PaymentDate, @UserID)";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    db.Execute(query, new
                    {
                        StudentID = payment.StudentID,
                        FeeTypeID = payment.FeeTypeID,
                        StudentName = payment.StudentName,
                        FeeTypeName = payment.FeeTypeName,
                        ClassID = payment.ClassID,
                        AmountPaid = payment.AmountPaid,
                        AmountLeft = payment.AmountLeft,
                        PaymentDate = payment.PaymentDate,
                        UserID = payment.UserID
                    });

                    return Ok(new { message = "Payment processed successfully." });
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, new { message = "An error occurred while processing the payment.", error = ex.Message });
                }
            }
        }

        // Backend: Verify the Payment
        [HttpPost]
        [Route("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationRequest request)
        {
            var url = $"https://api.paystack.co/transaction/verify/{request.reference}";
            var secretKey = "sk_test_..."; // Your secret key

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + secretKey);

                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                var verificationData = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (verificationData?.status == "success")
                {
                    // Payment was successful
                    // Update your database with the payment status and transaction details
                    return Ok(new { status = "success", message = "Payment successful" });
                }
                else
                {
                    // Payment failed
                    return BadRequest(new { status = "failed", message = "Payment failed" });
                }
            }
        }

        // C# Example (ASP.NET Core)
        [HttpPost]
        [Route("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            var url = "https://api.paystack.co/transaction/initialize";
            var secretKey = "sk_test_..."; // Your secret key

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + secretKey);

                var data = new
                {
                    amount = request.Amount * 100, // Paystack expects amounts in kobo
                    email = request.Email,
                    callback_url = "https://your-callback-url.com",
                    // other necessary fields (like the transaction reference, etc.)
                };

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                return Ok(responseString); // Send the response to your Flutter app
            }
        }

        [HttpGet("GetStudentById")]
        public IActionResult GetStudentById(int studentId)
        {
            var query = @"SELECT StudentID, CONCAT(StudentFirstName, ' ', StudentLastName) AS FullName, ClassID 
                  FROM SchoolManagement.Students 
                  WHERE StudentID = @StudentID";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    var student = db.QueryFirstOrDefault(query, new { StudentID = studentId });
                    if (student == null)
                        return NotFound(new { message = "Student not found." });

                    return Ok(student);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }
        /**/
        [HttpGet("GetTerms")]
        public IActionResult GetTerms()
        {
            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var query = "SELECT TermID, Term FROM SchoolManagement.SchoolTerm";
                var terms = db.Query(query).Select(t => new {
                    TermID = t.TermID,
                    Term = t.Term
                });
                return Ok(terms);
            }
        }

        [HttpGet("GetStudentExamReport")]
        public IActionResult GetStudentExamReport(string studentName, string term)
        {
            var query = @"SELECT StudentName, ClassName, Term, SchoolCourse, ClassScore, ExamsScore, TotalScore, Grade 
                  FROM SchoolManagement.SchoolExams 
                  WHERE StudentName = @StudentName AND Term = @Term";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var examRecords = db.Query(query, new { StudentName = studentName, Term = term });

                if (!examRecords.Any())
                    return NotFound(new { message = "No exam records found." });

                return Ok(examRecords);
            }
        }

        [HttpGet] //am not using this at the moment
        [Route("GetExamReport")]
        public IActionResult GetExamReport(string studentName, string term)
        {
            string query = @"SELECT * FROM SchoolManagement.SchoolExams 
                     WHERE StudentName = @StudentName AND Term = @Term";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    var report = db.QueryFirstOrDefault(query, new { StudentName = studentName, Term = term });

                    if (report == null)
                        return NotFound(new { message = "No report found for this student and term." });

                    return Ok(report);
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, new { message = "Error fetching exam report.", error = ex.Message });
                }
            }
        }

        /*Apply for Leave*/
        [HttpPost("ApplyForLeave")]
        public async Task<IActionResult> ApplyForLeave([FromBody] LeaveApplicationViewModel leaveRequest)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);

            var query = @"
        INSERT INTO SchoolManagement.LeaveOfAbsence 
            (UserID, LeaveType, StartDate, EndDate, Reason, Status, DateRequested)
        VALUES 
            (@UserID, @LeaveType, @StartDate, @EndDate, @Reason, 'Pending', GETDATE());";

            var result = await connection.ExecuteAsync(query, new
            {
                leaveRequest.UserID,                
                leaveRequest.LeaveType,
                leaveRequest.StartDate,
                leaveRequest.EndDate,
                leaveRequest.Reason
            });

            if (result > 0)
            {
                return Ok(new { success = true, message = "Leave request submitted successfully." });
            }

            return BadRequest(new { success = false, message = "Leave request submission failed." });
        }

        [HttpGet("LeaveStatusList")]
        public async Task<IActionResult> GetApprovedOrRejectedLeaves()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);

            var query = @"
        SELECT 
            l.LeaveID,
            u.FullName AS UserFullName,
            l.LeaveType,
            l.StartDate,
            l.EndDate,
            l.Reason,
            l.Status,
            ISNULL(approver.FullName, 'Pending') AS ApprovedByName,
            l.DateRequested,
            l.DateApproved
        FROM SchoolManagement.LeaveOfAbsence l
        INNER JOIN SchoolManagement.Users u ON l.UserID = u.UserID
        LEFT JOIN SchoolManagement.Users approver ON l.ApprovedBy = approver.UserID
        WHERE l.Status IN ('Approved', 'Rejected')
        ORDER BY l.DateRequested DESC;";

            var leaves = await connection.QueryAsync<LeaveStatusViewModel>(query);

            return Ok(leaves);
        }

        [HttpGet("GetsStudentById/{studentId}")]
        public async Task<IActionResult> GetsStudentById(int studentId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var query = @"SELECT StudentID, (StudentFirstName + ' ' + StudentLastName) AS FullName, ClassID 
                  FROM SchoolManagement.Students WHERE StudentID = @studentId";

            var student = await connection.QueryFirstOrDefaultAsync<StudentInfoVm>(query, new { studentId });

            if (student == null)
                return NotFound("Student not found");

            return Ok(student);
        }

        [HttpGet("GetPhotoRecords")]
        public async Task<IActionResult> GetPhotoRecords(int studentId, int termId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var query = @"SELECT PhotoTitle, PhotoDescription, PhotoData, UploadDate 
                  FROM SchoolManagement.PhotoRecords 
                  WHERE StudentID = @studentId AND TermID = @termId";

            var records = await connection.QueryAsync<PhotoRecordVm>(query, new { studentId, termId });
            return Ok(records);
        }

        /*InsertPhotoRecord endpoin*/
        [HttpPost("InsertPhotoRecord")]
        public async Task<IActionResult> InsertPhotoRecord([FromBody] PhotoRecord photoDto)
        {
            if (photoDto == null || string.IsNullOrEmpty(photoDto.PhotoData))
                return BadRequest("Invalid data submitted.");

            var photoBytes = Convert.FromBase64String(photoDto.PhotoData);

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            INSERT INTO SchoolManagement.PhotoRecords
            (Class, StudentName, StudentID, PhotoTitle, PhotoDescription, PhotoData, TermID, UserID)
            VALUES (@Class, @StudentName, @StudentID, @PhotoTitle, @PhotoDescription, @PhotoData, @TermID, @UserID)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Class", photoDto.Class);
                    command.Parameters.AddWithValue("@StudentName", photoDto.StudentName);
                    command.Parameters.AddWithValue("@StudentID", photoDto.StudentId);
                    command.Parameters.AddWithValue("@PhotoTitle", photoDto.PhotoTitle);
                    command.Parameters.AddWithValue("@PhotoDescription", photoDto.PhotoDescription);
                    command.Parameters.AddWithValue("@PhotoData", photoBytes);
                    command.Parameters.AddWithValue("@TermID", photoDto.TermID);
                    command.Parameters.AddWithValue("@UserID", photoDto.UserID);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }

            return Ok(new { message = "Photo record inserted successfully." });
        }

    }
}
