using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;

namespace ProxyServer.Controllers
{
    [ApiController]
    [Route("proxy/[controller]")]
    public class EmployeesController : ControllerBase
    {
        // Lista DWServer (endpoints)
        private static readonly string[] dwServers =
{
    Environment.GetEnvironmentVariable("DWServer1_URL") ?? "http://localhost:5001/employees",
    Environment.GetEnvironmentVariable("DWServer2_URL") ?? "http://localhost:5002/employees"
};


        private static int counter = 0;
        private static readonly object lockObj = new();
        private static readonly Dictionary<string, (string value, DateTime time)> cache = new();

        private string GetNextServer()
        {
            lock (lockObj)
            {
                var server = dwServers[counter % dwServers.Length];
                counter++;
                return server;
            }
        }

        private bool IsCacheValid(string key) =>
            cache.ContainsKey(key) && (DateTime.Now - cache[key].time).TotalSeconds < 30;

        // Creează un nou HttpClient pentru fiecare cerere, cu timeout de 5 secunde
        private HttpClient CreateClient()
        {
            return new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }


        // ---------- GET All ----------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            const string key = "all_employees";
            try
            {
                if (IsCacheValid(key))
                    return Ok(cache[key].value);

                var server = GetNextServer();
                using var client = CreateClient();
                var response = await client.GetAsync(server);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, $"DWServer returned {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                cache[key] = (content, DateTime.Now);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Proxy error: {ex.Message}");
            }
        }

        // ---------- GET by ID ----------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var key = $"employee_{id}";
            try
            {
                if (IsCacheValid(key))
                    return Ok(cache[key].value);

                var server = GetNextServer();
                using var client = CreateClient();
                var response = await client.GetAsync($"{server}/{id}");

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, $"DWServer returned {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                cache[key] = (content, DateTime.Now);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Proxy error: {ex.Message}");
            }
        }

        // ---------- PUT (add employee) ----------
        [HttpPut]
        public async Task<IActionResult> Add([FromBody] object data)
        {
            try
            {
                var server = GetNextServer();
                using var client = CreateClient();
                var response = await client.PutAsJsonAsync(server, data);
                cache.Clear();
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Proxy error: {ex.Message}");
            }
        }

        // ---------- POST (update employee) ----------
        [HttpPost("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] object data)
        {
            try
            {
                var server = GetNextServer();
                using var client = CreateClient();
                var response = await client.PostAsJsonAsync($"{server}/{id}", data);
                cache.Clear();
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Proxy error: {ex.Message}");
            }
        }

        // ---------- DELETE ----------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var server = GetNextServer();
                using var client = CreateClient();
                var response = await client.DeleteAsync($"{server}/{id}");
                cache.Clear();
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Proxy error: {ex.Message}");
            }
        }
    }
}
