// include necessary namespaces
using infonetica_task.Models;
using infonetica_task.Utils;
using infonetica_task.Routes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Define file paths for persistent data storage
const string workflowsPath = "data/workflows.json";
const string instancesPath = "data/instances.json";

// Load existing workflow definitions and instances from file storage
List<WorkflowDefinition> workflows = await FileStorage.LoadList<WorkflowDefinition>(workflowsPath);
List<WorkflowInstance> instances = await FileStorage.LoadList<WorkflowInstance>(instancesPath);

// Basic health check endpoint
app.MapGet("/", () => "Hello World!");

// Register all API routes for workflow engine
InstanceRoutes.Register(app, workflows, instances, instancesPath);
WorkflowRoutes.Register(app, workflows, workflowsPath);

// Start the web application
app.Run();