namespace infonetica_task.Models;

public class ActionTransition
{
    public string Id { get; set; } = "";
    public List<string> FromStates { get; set; } = new();
    public string ToState { get; set; } = "";
    public bool Enabled { get; set; } = true;
}