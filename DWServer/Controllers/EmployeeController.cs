using Microsoft.AspNetCore.Mvc;
using DWServer.Models;
using System.Text.Json;

namespace DWServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeesController : ControllerBase
    {
        private static readonly string filePath = "employees.json";
        private static List<Employee> employees = LoadEmployees();
        private static readonly object locker = new object();

        private static List<Employee> LoadEmployees()
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return new List<Employee>();

                var json = System.IO.File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<Employee>>(json) ?? new List<Employee>();
            }
            catch
            {
                return new List<Employee>();
            }
        }

        private static void SaveEmployees()
        {
            lock (locker)
            {
                try
                {
                    var json = JsonSerializer.Serialize(employees, new JsonSerializerOptions { WriteIndented = true });

                    // folosim fișier temporar pentru a evita coruperea
                    var tempFile = $"{filePath}.tmp";
                    System.IO.File.WriteAllText(tempFile, json);
                    System.IO.File.Copy(tempFile, filePath, true);
                    System.IO.File.Delete(tempFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing employees.json: {ex.Message}");
                }
            }
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            lock (locker)
            {
                employees ??= new List<Employee>();
                return Ok(employees);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            lock (locker)
            {
                employees ??= new List<Employee>();
                var emp = employees.FirstOrDefault(e => e.Id == id);
                if (emp == null)
                    return NotFound(new { message = $"Employee with ID {id} not found." });

                return Ok(emp);
            }
        }

        [HttpPut]
        public IActionResult Add([FromBody] Employee emp)
        {
            if (emp == null)
                return BadRequest(new { message = "Invalid employee data." });

            lock (locker)
            {
                employees ??= new List<Employee>();
                employees.Add(emp);
                SaveEmployees();
            }
            return Ok(new { message = "Employee added successfully." });
        }

        [HttpPost("{id}")]
        public IActionResult Update(int id, [FromBody] Employee updated)
        {
            if (updated == null)
                return BadRequest(new { message = "Invalid data." });

            lock (locker)
            {
                employees ??= new List<Employee>();
                var emp = employees.FirstOrDefault(e => e.Id == id);
                if (emp == null)
                    return NotFound(new { message = $"Employee with ID {id} not found." });

                emp.Name = updated.Name;
                emp.Position = updated.Position;
                emp.Salary = updated.Salary;
                SaveEmployees();
            }
            return Ok(new { message = "Employee updated successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            lock (locker)
            {
                employees ??= new List<Employee>();
                var emp = employees.FirstOrDefault(e => e.Id == id);
                if (emp == null)
                    return NotFound(new { message = $"Employee with ID {id} not found." });

                employees.Remove(emp);
                SaveEmployees();
            }
            return Ok(new { message = "Employee deleted successfully." });
        }
    }
}
