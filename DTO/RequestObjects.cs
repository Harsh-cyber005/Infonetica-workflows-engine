namespace infonetica_task.DTO;

public class StartInstanceRequest {
    public string WorkflowId { get; set; } = "";
}

public class TriggerActionRequest {
    public string InstanceId { get; set; } = "";
    public string ActionId { get; set; } = "";
}