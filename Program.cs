using infonetica_task.Models;
using infonetica_task.Utils;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


const string workflowsPath = "data/workflows.json";
const string instancesPath = "data/instances.json";


List<WorkflowDefinition> workflows = FileStorage.LoadList<WorkflowDefinition>(workflowsPath);
List<WorkflowInstance> workflowInstances = FileStorage.LoadList<WorkflowInstance>(instancesPath);

app.MapGet("/", () => "Hello World!");

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

app.Run();
