namespace DWServer.Models;
using System.Text.Json.Serialization;

public class Employee
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("salary")]
    public int Salary { get; set; }
}

