namespace infonetica_task.Models;

// Represents a state in a workflow definition
public class State {
    public string Id { get; set; } = "";
    public bool IsInitial { get; set; } // Whether this is the starting state of the workflow
    public bool IsFinal { get; set; } // Whether this is a terminal state (no actions can be executed)
    public bool Enabled { get; set; } = true; // Whether this state is currently active
}