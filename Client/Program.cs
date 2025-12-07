using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string proxyUrl =
    Environment.GetEnvironmentVariable("PROXY_URL") ?? "http://localhost:5000/proxy/employees";

    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("\n=== CLIENT MENU ===");
            Console.WriteLine("1. Get all employees");
            Console.WriteLine("2. Get employee by ID");
            Console.WriteLine("3. Add employee");
            Console.WriteLine("4. Update employee");
            Console.WriteLine("5. Delete employee");
            Console.WriteLine("0. Exit");
            Console.Write("Choose: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    var all = await client.GetStringAsync(proxyUrl);
                    Console.WriteLine(all);
                    break;

                case "2":
                    Console.Write("Enter ID: ");
                    var id = Console.ReadLine();
                    var emp = await client.GetStringAsync($"{proxyUrl}/{id}");
                    Console.WriteLine(emp);
                    break;

                case "3":
                    Console.Write("ID: ");
                    int newId = int.Parse(Console.ReadLine() ?? "0");
                    Console.Write("Name: ");
                    string? name = Console.ReadLine();
                    Console.Write("Position: ");
                    string? pos = Console.ReadLine();
                    Console.Write("Salary: ");
                    int sal = int.Parse(Console.ReadLine() ?? "0");

                    var newEmp = new { id = newId, name, position = pos, salary = sal };
                    var putResponse = await client.PutAsJsonAsync(proxyUrl, newEmp);
                    Console.WriteLine(await putResponse.Content.ReadAsStringAsync());
                    break;

                case "4":
                    Console.Write("ID to update: ");
                    int uid = int.Parse(Console.ReadLine() ?? "0");
                    Console.Write("New name: ");
                    string? newName = Console.ReadLine();
                    Console.Write("New position: ");
                    string? newPos = Console.ReadLine();
                    Console.Write("New salary: ");
                    int newSal = int.Parse(Console.ReadLine() ?? "0");

                    var updated = new { id = uid, name = newName, position = newPos, salary = newSal };
                    var postResponse = await client.PostAsJsonAsync($"{proxyUrl}/{uid}", updated);
                    Console.WriteLine(await postResponse.Content.ReadAsStringAsync());
                    break;

                case "5":
                    Console.Write("ID to delete: ");
                    var delId = Console.ReadLine();
                    var delResponse = await client.DeleteAsync($"{proxyUrl}/{delId}");
                    Console.WriteLine(await delResponse.Content.ReadAsStringAsync());
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }
}
