namespace infonetica_task.Models;

// Defines the structure and behavior of a workflow template
public class WorkflowDefinition {
    public string Id { get; set; } = "";
    public List<State> States { get; set; } = []; // All possible states in this workflow
    public List<ActionTransition> Actions { get; set; } = []; // All possible transitions between states
}
