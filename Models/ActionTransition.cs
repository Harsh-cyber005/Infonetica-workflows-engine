namespace infonetica_task.Models;

// Represents an action that can transition a workflow between states
public class ActionTransition {
    public string Id { get; set; } = "";
    public List<string> FromStates { get; set; } = []; // States from which this action can be triggered
    public string ToState { get; set; } = ""; // Target state after action execution
    public bool Enabled { get; set; } = true; // Whether this action is currently available
}