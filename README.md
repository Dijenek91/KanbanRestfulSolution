# KanbanRestfulSolution
Java Interview question reimagined as a NET10 framework service

The implementation shall provide a rest API for a Kanban board front end UI.
It shall include:
  - REST CRUD operations - which include validation, pagination, filtering and sorting
  - Persist data in a database (i chose SQL management studio to work with)
  - Have SignalR to push events and notifications to the UI in regards to created/updated/deleted Tasks
  - OpenAPI3/SCALAR
  - Unit test + integration tests with 80+% coverage
  - Docker envrioment - app+db
  - Authentication with JWT (must find net correlation)

Performance requirement:
  - on GET /api/tasks?page=0&size50 <= 150 ms on local laptop (whatever that means)

CRUD operations:
  -GET - /api/tasks
  -GET - /api/tasks/{id}
  -POST - /api/tasks
  -PUT - /api/tasks/{id}
  -PATCH - /api/tasks/{id}
  -DELETE - /api/tasks/{id}
