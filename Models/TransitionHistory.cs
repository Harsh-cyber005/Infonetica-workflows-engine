namespace infonetica_task.Models;

// Records a single state transition in a workflow instance's history
public class TransitionHistory {
    public string Action { get; set; } = ""; // ID of the action that caused the transition
    public string From { get; set; } = ""; // State ID before the transition
    public string To { get; set; } = ""; // State ID after the transition
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // When the transition occurred
}
