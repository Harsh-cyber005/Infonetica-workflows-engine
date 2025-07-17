# Infonetica Workflow Engine

A minimal, configurable state-machine API built with .NET 8 that allows you to define workflows, start instances, and execute state transitions with full validation.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Any IDE/editor (Visual Studio, VS Code, etc.)

### Running the Application

```bash
# Clone the repository
git clone https://github.com/Harsh-cyber005/Infonetica-workflows-engine.git
cd infonetica-task

# Run the application
dotnet run

# Or build and run
dotnet build
dotnet run --project infonetica-task.csproj
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7172`

### Verify Installation
Visit `http://localhost:5000` in your browser - you should see "Hello World!" confirming the service is running.

## 📋 API Documentation

### Workflow Definition Management

#### Create Workflow Definition
```http
POST /workflows
Content-Type: application/json

{
  "id": "leaveApproval",
  "states": [
    {
      "id": "requested",
      "isInitial": true,
      "isFinal": false,
      "enabled": true
    },
    {
      "id": "approved",
      "isInitial": false,
      "isFinal": true,
      "enabled": true
    },
    {
      "id": "rejected",
      "isInitial": false,
      "isFinal": true,
      "enabled": true
    }
  ],
  "actions": [
    {
      "id": "approve",
      "fromStates": ["requested"],
      "toState": "approved",
      "enabled": true
    },
    {
      "id": "reject",
      "fromStates": ["requested"],
      "toState": "rejected",
      "enabled": true
    }
  ]
}
```

#### Get All Workflow Definitions
```http
GET /workflows
```

#### Get Specific Workflow Definition
```http
GET /workflows/{workflowId}
```

### Workflow Instance Management

#### Start New Workflow Instance
```http
POST /instances
Content-Type: application/json

{
  "workflowId": "leaveApproval"
}
```

#### Execute Action on Instance
```http
POST /instances/{instanceId}/action
Content-Type: application/json

{
  "actionId": "approve"
}
```

#### Get Instance Details
```http
GET /instances/{instanceId}
```

#### Get All Instances
```http
GET /instances
```

#### Get Instances by Workflow
```http
GET /instances/byWorkflow/{workflowId}
```

## 🏗️ Architecture

### Project Structure
```
infonetica-task/
├── Models/                    # Domain models
│   ├── ActionTransition.cs   # Action/transition definition
│   ├── State.cs              # Workflow state definition
│   ├── TransitionHistory.cs  # Historical transition record
│   ├── WorkflowDefinition.cs # Complete workflow template
│   └── WorkflowInstance.cs   # Running workflow instance
├── Routes/                    # API endpoint definitions
│   ├── InstanceRoutes.cs     # Instance management endpoints
│   └── WorkflowRoutes.cs     # Definition management endpoints
├── DTO/                       # Data transfer objects
│   └── RequestObjects.cs     # API request models
├── Utils/                     # Utility classes
│   └── FileStorage.cs        # JSON file persistence
├── data/                      # Persistent storage
│   ├── workflows.json        # Workflow definitions
│   └── instances.json        # Running instances
└── Program.cs                 # Application entry point
```

### Core Concepts

**State**: Represents a point in a workflow with properties:
- `id`: Unique identifier
- `isInitial`: Whether this is the starting state (exactly one required)
- `isFinal`: Whether this is a terminal state (no actions can be executed)
- `enabled`: Whether this state is currently active

**Action (Transition)**: Defines how to move between states:
- `id`: Unique identifier
- `fromStates`: Array of state IDs from which this action can be triggered
- `toState`: Target state after execution
- `enabled`: Whether this action is currently available

**Workflow Definition**: Template containing all states and actions for a workflow type

**Workflow Instance**: Running instance of a definition with current state and full transition history

## ✅ Validation Rules

### Workflow Definition Validation
- ✅ Unique workflow IDs (no duplicates)
- ✅ Exactly one initial state per workflow
- ✅ Unique state IDs within workflow
- ✅ All action `fromStates` must reference existing states
- ✅ All action `toState` must reference existing states

### Runtime Validation
- ✅ Action must belong to instance's workflow definition
- ✅ Action must be enabled
- ✅ Current state must be in action's `fromStates`
- ✅ Cannot execute actions from final states
- ✅ Current state must be enabled

## 💾 Data Persistence

The application uses lightweight JSON file storage in the `data/` directory:
- `workflows.json`: Stores all workflow definitions
- `instances.json`: Stores all running workflow instances

Data is automatically loaded on startup and persisted after each modification.

## 🔧 Configuration

### Default Ports
- HTTP: 5000
- HTTPS: 7172

### Environment Variables
The application respects standard ASP.NET Core configuration:
- `ASPNETCORE_ENVIRONMENT`: Set to "Development" for development mode
- `ASPNETCORE_URLS`: Override default URLs

## 📝 Design Decisions & Assumptions

### Architecture Choices
1. **Minimal API**: Used ASP.NET Core Minimal APIs for simplicity and reduced boilerplate
2. **File-based Storage**: JSON files provide simple persistence without database complexity
3. **In-Memory Collections**: Loaded once at startup for fast access, suitable for small-scale usage
4. **Synchronous Validation**: All business rules checked before state changes

### Assumptions
1. **Small Scale**: Designed for moderate numbers of workflows and instances
2. **Single Instance**: No clustering or multi-server support
3. **Manual JSON**: Workflow definitions expected to be created via API calls
4. **UTF-8 Encoding**: All JSON files use UTF-8 encoding

### Known Limitations
1. **Concurrency**: No thread-safety for concurrent instance modifications
2. **Memory Usage**: All data loaded in memory - not suitable for large datasets
3. **Backup**: No automated backup or recovery mechanisms
4. **Authentication**: No security or access control implemented
5. **Validation**: Limited business rule validation beyond structural requirements

## 🧪 Testing

### Manual Testing Examples

1. **Create a simple workflow**:
```bash
curl -X POST http://localhost:5000/workflows \
  -H "Content-Type: application/json" \
  -d '{"id":"test","states":[{"id":"start","isInitial":true,"isFinal":false,"enabled":true},{"id":"end","isInitial":false,"isFinal":true,"enabled":true}],"actions":[{"id":"finish","fromStates":["start"],"toState":"end","enabled":true}]}'
```

2. **Start an instance**:
```bash
curl -X POST http://localhost:5000/instances \
  -H "Content-Type: application/json" \
  -d '{"workflowId":"test"}'
```

3. **Execute an action**:
```bash
curl -X POST http://localhost:5000/instances/{instanceId}/action \
  -H "Content-Type: application/json" \
  -d '{"actionId":"finish"}'
```

### Sample Data
The application includes a sample "Leave Approval" workflow demonstrating the basic concepts.

## 🛠️ Development

### Adding New Features
The codebase is structured for easy extension:
- Add new validation rules in route handlers
- Extend models for additional properties
- Replace `FileStorage` for different persistence mechanisms
- Add middleware for cross-cutting concerns

### Code Style
- Follows standard C# conventions
- Uses modern C# features (collection expressions, string interpolation)
- Comprehensive error handling with descriptive messages
- Clear separation of concerns between models, routes, and utilities

## 🤝 Contributing

This is a take-home exercise, but the structure supports future enhancements:
- Unit tests can be added in a separate test project
- Integration tests for API endpoints
- Performance optimization for larger datasets
- Additional workflow features (parallel states, conditional transitions)

---

**Note**: This implementation prioritizes clarity and correctness over performance optimization, making it suitable for understanding workflow concepts and moderate-scale usage.
