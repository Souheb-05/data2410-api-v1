using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using data2410_api_v1.Models;

namespace data2410_api_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController(IConfiguration config) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection")!;

    private static string GetGrade(int marks) => marks switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 60 => "C",
        _ => "D"
    };

    [HttpGet]
    public async Task<ActionResult<List<Student>>> GetAll()
    {
        var students = new List<Student>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT Id, Name, Course, Marks, Grade FROM Students", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            students.Add(new Student
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Course = reader.GetString(2),
                Marks = reader.GetInt32(3),
                Grade = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return students;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Student>> GetById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT Id, Name, Course, Marks, Grade FROM Students WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return NotFound();

        return new Student
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Course = reader.GetString(2),
            Marks = reader.GetInt32(3),
            Grade = reader.IsDBNull(4) ? null : reader.GetString(4)
        };
    }

    [HttpPost]
    public async Task<ActionResult<Student>> Create(Student student)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(
            "INSERT INTO Students (Name, Course, Marks) OUTPUT INSERTED.Id VALUES (@Name, @Course, @Marks)", conn);
        cmd.Parameters.AddWithValue("@Name", student.Name);
        cmd.Parameters.AddWithValue("@Course", student.Course);
        cmd.Parameters.AddWithValue("@Marks", student.Marks);

        student.Id = (int)await cmd.ExecuteScalarAsync();
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Student updated)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(
            "UPDATE Students SET Name = @Name, Course = @Course, Marks = @Marks WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Name", updated.Name);
        cmd.Parameters.AddWithValue("@Course", updated.Course);
        cmd.Parameters.AddWithValue("@Marks", updated.Marks);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows == 0 ? NotFound() : NoContent();
    }

   [HttpPost("calculate-grades")]
public async Task<ActionResult<List<Student>>> CalculateGrades()
{
    var studentsWithGrade = new List<Student>();

    using var conn = new SqlConnection(_connectionString);
    await conn.OpenAsync();

    // 1. Hent alle studenter
    using var selectCmd = new SqlCommand("SELECT Id, Name, Course, Marks FROM Students", conn);
    using var reader = await selectCmd.ExecuteReaderAsync();

    var students = new List<Student>();

    while (await reader.ReadAsync())
    {
        students.Add(new Student
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Course = reader.GetString(2),
            Marks = reader.GetInt32(3)
        });
    }

    reader.Close();

    // 2. Beregn karakter + oppdater DB
    foreach (var student in students)
    {
        var grade = GetGrade(student.Marks);

        using var updateCmd = new SqlCommand(
            "UPDATE Students SET Grade = @grade WHERE Id = @id", conn);

        updateCmd.Parameters.AddWithValue("@grade", grade);
        updateCmd.Parameters.AddWithValue("@id", student.Id);

        await updateCmd.ExecuteNonQueryAsync();

        student.Grade = grade;
        studentsWithGrade.Add(student);
    }

    return studentsWithGrade;
}

    [HttpGet("report")]
    public async Task<IActionResult> Report()
    {
        var report = new List<object>();

using var conn = new SqlConnection(_connectionString);
await conn.OpenAsync();

var query = @"
    SELECT 
        Course,
        COUNT(*) AS TotalStudents,
        AVG(CAST(Marks AS FLOAT)) AS AverageMarks,
        SUM(CASE WHEN Grade = 'A' THEN 1 ELSE 0 END) AS A,
        SUM(CASE WHEN Grade = 'B' THEN 1 ELSE 0 END) AS B,
        SUM(CASE WHEN Grade = 'C' THEN 1 ELSE 0 END) AS C,
        SUM(CASE WHEN Grade = 'D' THEN 1 ELSE 0 END) AS D
    FROM Students
    GROUP BY Course";

using var cmd = new SqlCommand(query, conn);
using var reader = await cmd.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    report.Add(new
    {
        courseName = reader.GetString(0),
        totalStudents = reader.GetInt32(1),
        averageMarks = reader.GetDouble(2),
        gradeDistribution = new
        {
            A = reader.GetInt32(3),
            B = reader.GetInt32(4),
            C = reader.GetInt32(5),
            D = reader.GetInt32(6)
        }
    });
}

return Ok(report);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("DELETE FROM Students WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows == 0 ? NotFound() : NoContent();
    }
}
