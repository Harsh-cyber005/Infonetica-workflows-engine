using infonetica_task.Models;
using infonetica_task.Utils;
using infonetica_task.DTO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


const string workflowsPath = "data/workflows.json";
const string instancesPath = "data/instances.json";


List<WorkflowDefinition> workflows = FileStorage.LoadList<WorkflowDefinition>(workflowsPath);
List<WorkflowInstance> instances = FileStorage.LoadList<WorkflowInstance>(instancesPath);

app.MapGet("/", () => "Hello World!");

// workflow routes
app.MapPost("/workflows", (WorkflowDefinition newWorkflow) => {
    foreach (var existingWorkflow in workflows) {
        if (existingWorkflow.Id == newWorkflow.Id) {
            return Results.BadRequest($"Workflow with id '{newWorkflow.Id}' already exists.");
        }
    }

    List<State> initialStates = [];
    foreach (var state in newWorkflow.States) {
        if (state.IsInitial) {
            initialStates.Add(state);
        }
    }

    if (initialStates.Count != 1) {
        return Results.BadRequest("Workflow must have exactly one state with isInitial = true.");
    }

    Dictionary<string, int> stateIdCounts = [];
    foreach (var state in newWorkflow.States) {
        if (stateIdCounts.ContainsKey(state.Id)) {
            stateIdCounts[state.Id]++;
        }
        else {
            stateIdCounts[state.Id] = 1;
        }
    }

    List<string> duplicateStateIds = [];
    foreach (var kvp in stateIdCounts) {
        if (kvp.Value > 1) {
            duplicateStateIds.Add(kvp.Key);
        }
    }

    if (duplicateStateIds.Count > 0) {
        return Results.BadRequest($"Duplicate state IDs found: {string.Join(", ", duplicateStateIds)}");
    }

    HashSet<string> validStateIds = [];
    foreach (var state in newWorkflow.States) {
        validStateIds.Add(state.Id);
    }

    foreach (var action in newWorkflow.Actions) {
        if (!validStateIds.Contains(action.ToState)) {
            return Results.BadRequest($"Action '{action.Id}' refers to unknown toState '{action.ToState}'.");
        }

        List<string> invalidFromStates = new List<string>();
        foreach (var fromState in action.FromStates) {
            if (!validStateIds.Contains(fromState)) {
                invalidFromStates.Add(fromState);
            }
        }

        if (invalidFromStates.Count > 0) {
            return Results.BadRequest($"Action '{action.Id}' has invalid fromStates: {string.Join(", ", invalidFromStates)}");
        }
    }

    workflows.Add(newWorkflow);
    FileStorage.SaveList(workflowsPath, workflows);

    return Results.Ok($"Workflow '{newWorkflow.Id}' created successfully.");
});

app.MapGet("/workflows", () => {
    var workflows = FileStorage.LoadList<WorkflowDefinition>(workflowsPath);
    return Results.Ok(workflows);
});

app.MapGet("/workflows/{id}", (string id) => {
    var workflows = FileStorage.LoadList<WorkflowDefinition>(workflowsPath);
    foreach (var workflow in workflows) {
        if (workflow.Id == id) {
            return Results.Ok(workflow);
        }
    }
    return Results.NotFound($"Workflow with id '{id}' not found.");
});

//instance routes
app.MapPost("/instances", (StartInstanceRequest request) => {
    string workflowId = request.WorkflowId;
    if (string.IsNullOrEmpty(workflowId)) {
        return Results.BadRequest("WorkflowId cannot be empty.");
    }
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

    var instance = new WorkflowInstance {
        WorkflowId = workflow.Id,
        CurrentState = initialState.Id,
        History = []
    };

    instances.Add(instance);
    FileStorage.SaveList(instancesPath, instances);

    return Results.Ok(new { instanceId = instance.Id });
});

app.MapPost("/instances/{instanceId}/trigger", (string instanceId, TriggerActionRequest request) => {
    string actionId = request.ActionId;
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

    // just some defense if someone mentioned a final state in the action fromStates. logically even if final state is mentioned, it should not be possible to trigger an action from it
    if (currentState.IsFinal) {
        return Results.BadRequest($"Cannot execute actions from final state '{currentState.Id}'.");
    }

    string oldState = instance.CurrentState;
    instance.CurrentState = action.ToState;

    instance.History.Add(new TransitionHistory {
        Action = action.Id,
        From = oldState,
        To = action.ToState,
        Timestamp = DateTime.UtcNow
    });

    FileStorage.SaveList(instancesPath, instances);

    return Results.Ok($"Instance '{instanceId}' moved from '{oldState}' to '{action.ToState}' via action '{action.Id}'.");
});

app.Run();
