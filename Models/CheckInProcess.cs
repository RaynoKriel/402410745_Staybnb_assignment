using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace _402410745_Staybnb_assignment.Models;

// The host's rules and required documents for ONE property (one-to-one with HostProperty).
public class CheckInProcess
{
    public int Id { get; set; }

    public int PropertyId { get; set; }
    public HostProperty Property { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; }

    // The steps/rules are stored as a JSON list of text, e.g. ["Upload your ID","Arrive after 14:00"]
    public string StepsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<string> GetSteps()
    {
        if (string.IsNullOrWhiteSpace(StepsJson)) return new List<string>();
        return JsonSerializer.Deserialize<List<string>>(StepsJson);
    }

    public void SetSteps(IEnumerable<string> steps)
    {
        StepsJson = JsonSerializer.Serialize(steps);
    }

    // Used when a property is approved so every property always has a check-in process.
    public static CheckInProcess CreateDefault()
    {
        var process = new CheckInProcess { Title = "Standard check-in" };
        process.SetSteps(new[]
        {
            "Upload a valid ID or Passport",
            "Check-in is from 14:00",
            "Please respect the house rules"
        });
        return process;
    }
}
