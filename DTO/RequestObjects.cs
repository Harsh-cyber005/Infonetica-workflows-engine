using System.ComponentModel.DataAnnotations;
namespace infonetica_task.DTO;

// Request DTO for starting a new workflow instance
public class StartInstanceRequest {
    [Required(ErrorMessage = "WorkflowId is required")]
    public string WorkflowId { get; set; } = "";
}

// Request DTO for triggering an action on a workflow instance
public class TriggerActionRequest {
    [Required(ErrorMessage = "ActionId is required")]
    public string ActionId { get; set; } = "";
}