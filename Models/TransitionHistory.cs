namespace infonetica_task.Models;

public class TransitionHistory
{
    public string Action { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
