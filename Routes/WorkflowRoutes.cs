using infonetica_task.Models;
using infonetica_task.Utils;

namespace infonetica_task.Routes;

public static class WorkflowRoutes {
    // Register all workflow definition-related HTTP endpoints
    public static void Register(WebApplication app, List<WorkflowDefinition> workflows, string workflowsPath) {
        // POST /workflows - Create a new workflow definition with validation
        app.MapPost("/workflows", async (WorkflowDefinition newWorkflow) => {
            // Check for duplicate workflow IDs
            foreach (var existingWorkflow in workflows) {
                if (existingWorkflow.Id == newWorkflow.Id) {
                    return Results.BadRequest($"Workflow with id '{newWorkflow.Id}' already exists.");
                }
            }

            // Validate exactly one initial state exists
            List<State> initialStates = [];
            foreach (var state in newWorkflow.States) {
                if (state.IsInitial) {
                    initialStates.Add(state);
                }
            }

            if (initialStates.Count != 1) {
                return Results.BadRequest("Workflow must have exactly one state with isInitial = true.");
            }

            // Check for duplicate state IDs within the workflow
            Dictionary<string, int> stateIdCounts = [];
            foreach (var state in newWorkflow.States) {
                if (stateIdCounts.ContainsKey(state.Id)) {
                    stateIdCounts[state.Id]++;
                }
                else {
                    stateIdCounts[state.Id] = 1;
                }
            }

            // Collect any duplicate state IDs found
            List<string> duplicateStateIds = [];
            foreach (var kvp in stateIdCounts) {
                if (kvp.Value > 1) {
                    duplicateStateIds.Add(kvp.Key);
                }
            }

            if (duplicateStateIds.Count > 0) {
                return Results.BadRequest($"Duplicate state IDs found: {string.Join(", ", duplicateStateIds)}");
            }

            // Create set of valid state IDs for action validation
            HashSet<string> validStateIds = [];
            foreach (var state in newWorkflow.States) {
                validStateIds.Add(state.Id);
            }

            // Validate all action state references are valid
            foreach (var action in newWorkflow.Actions) {
                if (!validStateIds.Contains(action.ToState)) {
                    return Results.BadRequest($"Action '{action.Id}' refers to unknown toState '{action.ToState}'.");
                }

                // Check all fromStates are valid
                List<string> invalidFromStates = [];
                foreach (var fromState in action.FromStates) {
                    if (!validStateIds.Contains(fromState)) {
                        invalidFromStates.Add(fromState);
                    }
                }

                if (invalidFromStates.Count > 0) {
                    return Results.BadRequest($"Action '{action.Id}' has invalid fromStates: {string.Join(", ", invalidFromStates)}");
                }
            }

            // Save validated workflow to collection and persist to file
            workflows.Add(newWorkflow);
            await FileStorage.SaveList(workflowsPath, workflows);

            return Results.Ok($"Workflow '{newWorkflow.Id}' created successfully.");
        });

        // GET /workflows - Retrieve all workflow definitions
        app.MapGet("/workflows", async () => {
            var workflows = await FileStorage.LoadList<WorkflowDefinition>(workflowsPath);
            return Results.Ok(workflows);
        });

        // GET /workflows/{id} - Retrieve a specific workflow definition by ID
        app.MapGet("/workflows/{id}", async (string id) => {
            var workflows = await FileStorage.LoadList<WorkflowDefinition>(workflowsPath);
            // Find workflow by ID
            foreach (var workflow in workflows) {
                if (workflow.Id == id) {
                    return Results.Ok(workflow);
                }
            }
            return Results.NotFound($"Workflow with id '{id}' not found.");
        });
    }
}