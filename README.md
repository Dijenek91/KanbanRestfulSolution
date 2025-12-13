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

Authenticaton\Authorization:
  - Provided through use of JWT token generated in the Api\Auth service
  - Username\password verification for requesteing the token is a dummy implementation and not a realworld implementation
  - SCALAR UI doesnt support JWT testing, so verification of this functionality needs to be done through Postman project

Testing and verification envrioment:
  - Postman project: https://web.postman.co/workspace/My-Workspace~3be1e0e9-c3e8-4732-800c-bb6ad975a485/collection/6602988-e533ff4c-6fa5-47d1-a054-6f2004b6fc6d?action=share&source=copy-link&creator=6602988
  - Make sure to set the enviroment to: https://web.postman.co/workspace/My-Workspace~3be1e0e9-c3e8-4732-800c-bb6ad975a485/environment/6602988-ca9ebc0d-1512-40a8-8145-8495ebf0b111?action=share&source=copy-link&creator=6602988
  - The enviroment needs to contain a "jwt_token" variable for setting after running the **POST method "api\Auth\login"**
