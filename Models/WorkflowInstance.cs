namespace infonetica_task.Models;

// Represents a running instance of a workflow definition
public class WorkflowInstance {
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Unique identifier for this instance
    public string WorkflowId { get; set; } = ""; // Reference to the workflow definition
    public string CurrentState { get; set; } = ""; // Current state ID in the workflow
    public List<TransitionHistory> History { get; set; } = []; // Complete history of state transitions
}
