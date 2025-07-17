namespace infonetica_task.Models;

public class WorkflowInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkflowId { get; set; } = "";
    public string CurrentState { get; set; } = "";
    public List<TransitionHistory> History { get; set; } = new();
}
