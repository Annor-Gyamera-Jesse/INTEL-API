using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using ViewModels;

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

        // Endpoint to get users with roles
        [HttpGet("UsersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var usersWithRoles = new List<User>();
                using (var command = new SqlCommand("SELECT ur.UserRoleID, u.UserID, u.UserName, u.Password, ur.RoleName, ur.Enable " +
                                                    "FROM SchoolManagement.UserRoles ur " +
                                                    "JOIN SchoolManagement.Users u ON ur.UserID = u.UserID", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var userRoleID = reader.GetInt32(0);
                        var existingUser = usersWithRoles.Find(u => u.UserID == userRoleID);
                        if (existingUser == null)
                        {
                            var user = new User
                            {
                                UserID = reader.GetInt32(1),
                                UserName = reader.GetString(2),
                                Password = reader.GetString(3),
                                Roles = new List<UserRole>()
                            };
                            user.Roles.Add(new UserRole
                            {
                                UserRoleID = reader.GetInt32(0),
                                RoleName = reader.GetString(4),
                                Enable = reader.GetBoolean(5)
                            });
                            usersWithRoles.Add(user);
                        }
                        else
                        {
                            existingUser.Roles.Add(new UserRole
                            {
                                UserRoleID = reader.GetInt32(0),
                                RoleName = reader.GetString(4),
                                Enable = reader.GetBoolean(5)
                            });
                        }
                    }
                }

                return Ok(usersWithRoles);
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

                var userRoles = new List<UserRole>();
                using (var command = new SqlCommand("SELECT UserRoleID, RoleName, Enable FROM SchoolManagement.UserRoles WHERE UserID = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            userRoles.Add(new UserRole
                            {
                                UserRoleID = reader.GetInt32(0),
                                RoleName = reader.GetString(1),
                                Enable = reader.GetBoolean(2)
                            });
                        }
                    }
                }

                return Ok(userRoles);
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
                            StudentID = reader.GetInt32(0),
                            StudentFirstName = reader.GetString(1),
                            StudentLastName = reader.GetString(2),
                            StudentDateOfBirth = reader.GetDateTime(3),
                            StudentGender = reader.GetString(4)[0],
                            StudentAddress = reader.GetString(5),
                            StudentPhoneNumber = reader.GetString(6),
                            StudentEmail = reader.GetString(7),
                            ImageData = (byte[])reader.GetValue(8),
                            ClassID = reader.GetString(9),
                            GuardianFullName = reader.GetString(10),
                            GuardianGender = reader.GetString(11)[0],
                            GuardianHouseAddress = reader.GetString(12),
                            GuardianWorkAddress = reader.GetString(13),
                            GuardianEmail = reader.GetString(14),
                            GuardianFirstContact = reader.GetString(15),
                            GuardianSecondContact = reader.GetString(16),
                            EnableSwitch = reader.GetBoolean(17)

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
                                Term = reader.GetString(7),
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

                using (var command = new SqlCommand(
                    "INSERT INTO SchoolManagement.SchoolExams (StudentName, ClassName, AcademicYear, VacationDate, PromotedTo, NumberOnRoll, Term, " +
                    "Position, NextTermsBegins, AttendanceOut, AttendanceIn, SchoolCourse, ClassScore, ExamsScore, TotalScore, SubjectsPositions, " +
                    "Grade, TeachersRemarks, Conduct, HeadmasterRemark, SchoolInformation, TeachersSignature, HeadMasterSignature) " +
                    "VALUES (@StudentName, @ClassName, @AcademicYear, @VacationDate, @PromotedTo, @NumberOnRoll, @Term, @Position, @NextTermsBegins, " +
                    "@AttendanceOut, @AttendanceIn, @SchoolCourse, @ClassScore, @ExamsScore, @TotalScore, @SubjectsPositions, @Grade, @TeachersRemarks, " +
                    "@Conduct, @HeadmasterRemark, @SchoolInformation, @TeachersSignature, @HeadMasterSignature)",
                    connection))
                {
                    command.Parameters.AddWithValue("@StudentName", schoolExam.StudentName);
                    command.Parameters.AddWithValue("@ClassName", schoolExam.ClassName);
                    command.Parameters.AddWithValue("@AcademicYear", schoolExam.AcademicYear);
                    command.Parameters.AddWithValue("@VacationDate", schoolExam.VacationDate);
                    command.Parameters.AddWithValue("@PromotedTo", schoolExam.PromotedTo);
                    command.Parameters.AddWithValue("@NumberOnRoll", schoolExam.NumberOnRoll);
                    command.Parameters.AddWithValue("@Term", schoolExam.Term);
                    command.Parameters.AddWithValue("@Position", schoolExam.Position);
                    command.Parameters.AddWithValue("@NextTermsBegins", schoolExam.NextTermsBegins);
                    command.Parameters.AddWithValue("@AttendanceOut", schoolExam.AttendanceOut);
                    command.Parameters.AddWithValue("@AttendanceIn", schoolExam.AttendanceIn);
                    command.Parameters.AddWithValue("@SchoolCourse", schoolExam.SchoolCourse);
                    command.Parameters.AddWithValue("@ClassScore", schoolExam.ClassScore);
                    command.Parameters.AddWithValue("@ExamsScore", schoolExam.ExamsScore);
                    command.Parameters.AddWithValue("@TotalScore", schoolExam.TotalScore);
                    command.Parameters.AddWithValue("@SubjectsPositions", schoolExam.SubjectsPositions);
                    command.Parameters.AddWithValue("@Grade", schoolExam.Grade);
                    command.Parameters.AddWithValue("@TeachersRemarks", schoolExam.TeachersRemarks);
                    command.Parameters.AddWithValue("@Conduct", schoolExam.Conduct);
                    command.Parameters.AddWithValue("@HeadmasterRemark", schoolExam.HeadmasterRemark);
                    command.Parameters.AddWithValue("@SchoolInformation", schoolExam.SchoolInformation);
                    command.Parameters.AddWithValue("@TeachersSignature", schoolExam.TeachersSignature);
                    command.Parameters.AddWithValue("@HeadMasterSignature", schoolExam.HeadMasterSignature);

                    await command.ExecuteNonQueryAsync();
                }

                return Ok("School exam added successfully");
            }
        }

        // Endpoint to add a new student attendance
        [HttpPost("AddStudentAttendance")]
        public async Task<IActionResult> AddStudentAttendance([FromBody] StudentsAttendance studentAttendance)
        {
            if (studentAttendance == null)
            {
                return BadRequest("Invalid student attendance data");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "INSERT INTO SchoolManagement.StudentsAttendance (StudentFirstName, StudentLastName, ClassID, EnableSwitch) " +
                    "VALUES (@StudentFirstName, @StudentLastName, @ClassID, @EnableSwitch)",
                    connection))
                {
                    command.Parameters.AddWithValue("@StudentFirstName", studentAttendance.StudentFirstName);
                    command.Parameters.AddWithValue("@StudentLastName", studentAttendance.StudentLastName);
                    command.Parameters.AddWithValue("@ClassID", studentAttendance.ClassID);
                    command.Parameters.AddWithValue("@EnableSwitch", studentAttendance.EnableSwitch);

                    await command.ExecuteNonQueryAsync();
                }

                return Ok("Student attendance added successfully");
            }
        }
    }
}
