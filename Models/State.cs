namespace infonetica_task.Models;

public class State
{
    public string Id { get; set; } = "";
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public bool Enabled { get; set; } = true;
}