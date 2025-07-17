using infonetica_task.Models;
using infonetica_task.Utils;
using infonetica_task.DTO;

namespace infonetica_task.Routes;

public static class InstanceRoutes {
    // Register all workflow instance-related HTTP endpoints
    public static void Register(WebApplication app, List<WorkflowDefinition> workflows, List<WorkflowInstance> instances, string instancesPath) {
        // POST /instances - Create and start a new workflow instance
        app.MapPost("/instances", async (StartInstanceRequest request) => {
            string workflowId = request.WorkflowId;
            if (string.IsNullOrEmpty(workflowId)) {
                return Results.BadRequest("WorkflowId cannot be empty.");
            }
            // Find the workflow definition by ID
            WorkflowDefinition? workflow = null;
            foreach (var w in workflows) {
                if (w.Id == workflowId) {
                    workflow = w;
                    break;
                }
            }

            if (workflow == null) {
                return Results.NotFound($"Workflow with id '{workflowId}' not found.");
            }

            // Find the initial state to start the workflow instance
            State? initialState = null;
            // JUST some sanity checks even though we validate this in the workflow creation
            if (workflow.States.Count == 0) {
                return Results.BadRequest("Workflow has no states defined.");
            }
            foreach (var state in workflow.States) {
                if (state.IsInitial) {
                    initialState = state;
                    break;
                }
            }

            if (initialState == null) {
                return Results.BadRequest("Workflow does not have an initial state.");
            }

            // Create new workflow instance with initial state
            var instance = new WorkflowInstance {
                WorkflowId = workflow.Id,
                CurrentState = initialState.Id,
                History = []
            };

            // Save instance to collection and persist to file
            instances.Add(instance);
            await FileStorage.SaveList(instancesPath, instances);

            return Results.Ok(new { instanceId = instance.Id });
        });

        // POST /instances/{instanceId}/action - Execute an action to transition instance state
        app.MapPost("/instances/{instanceId}/action", async (string instanceId, TriggerActionRequest request) => {
            string actionId = request.ActionId;
            // Find the workflow instance by ID
            WorkflowInstance? instance = null;
            foreach (var ins in instances) {
                if (ins.Id == instanceId) {
                    instance = ins;
                    break;
                }
            }
            if (instance == null) {
                return Results.NotFound($"Instance with id '{instanceId}' not found.");
            }

            // Find the workflow definition for this instance
            WorkflowDefinition? workflow = null;
            foreach (var w in workflows) {
                if (w.Id == instance.WorkflowId) {
                    workflow = w;
                    break;
                }
            }
            if (workflow == null) {
                return Results.BadRequest($"Workflow definition '{instance.WorkflowId}' not found.");
            }

            // Find the action definition in the workflow
            ActionTransition? action = null;
            foreach (var act in workflow.Actions) {
                if (act.Id == actionId) {
                    action = act;
                    break;
                }
            }
            if (action == null) {
                return Results.BadRequest($"Action '{actionId}' not found in workflow.");
            }

            if (!action.Enabled) {
                return Results.BadRequest($"Action '{actionId}' is disabled.");
            }

            // Find current state definition and validate it
            State? currentState = null;
            foreach (var state in workflow.States) {
                if (state.Id == instance.CurrentState) {
                    currentState = state;
                    break;
                }
            }
            if (currentState == null) {
                return Results.BadRequest($"Current state '{instance.CurrentState}' not found in workflow states.");
            }
            if (!currentState.Enabled) {
                return Results.BadRequest($"Current state '{instance.CurrentState}' is disabled.");
            }

            // Check if action can be triggered from current state
            bool canTrigger = false;
            foreach (var fromState in action.FromStates) {
                if (fromState == instance.CurrentState) {
                    canTrigger = true;
                    break;
                }
            }
            if (!canTrigger) {
                return Results.BadRequest($"Action '{actionId}' cannot be executed from current state '{instance.CurrentState}'.");
            }

            if (currentState.IsFinal) {
                return Results.BadRequest($"Cannot execute actions from final state '{currentState.Id}'.");
            }

            // Execute the transition and update instance state
            string oldState = instance.CurrentState;
            instance.CurrentState = action.ToState;

            // Record the transition in history
            instance.History.Add(new TransitionHistory {
                Action = action.Id,
                From = oldState,
                To = action.ToState,
                Timestamp = DateTime.UtcNow
            });

            // Persist changes to file
            await FileStorage.SaveList(instancesPath, instances);

            return Results.Ok($"Instance '{instanceId}' moved from '{oldState}' to '{action.ToState}' via action '{action.Id}'.");
        });

        // GET /instances/{instanceId} - Retrieve a specific workflow instance
        app.MapGet("/instances/{instanceId}", (string instanceId) => {
            // Find the instance by ID
            WorkflowInstance? instance = null;
            foreach (var ins in instances) {
                if (ins.Id == instanceId) {
                    instance = ins;
                    break;
                }
            }

            if (instance == null)
                return Results.NotFound($"Instance with id '{instanceId}' not found.");

            return Results.Ok(instance);
        });

        // GET /instances/byWorkflow/{workflowId} - Get all instances for a specific workflow
        app.MapGet("/instances/byWorkflow/{workflowId}", (string workflowId) => {
            // Filter instances by workflow ID
            var filteredInstances = new List<WorkflowInstance>();
            foreach (var ins in instances) {
                if (ins.WorkflowId == workflowId) {
                    filteredInstances.Add(ins);
                }
            }
            return Results.Ok(filteredInstances);
        });

        // GET /instances - Retrieve all workflow instances
        app.MapGet("/instances", () => {
            return Results.Ok(instances);
        });
    }
}