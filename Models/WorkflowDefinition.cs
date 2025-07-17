namespace infonetica_task.Models;

public class WorkflowDefinition
{
    public string Id { get; set; } = "";
    public List<State> States { get; set; } = new();
    public List<ActionTransition> Actions { get; set; } = new();
}
