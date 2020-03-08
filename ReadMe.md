# Using DynamoDb with .NET Core

## Prerequisites

- Install [Docker Desktop](https://www.docker.com/products/docker-desktop).
  - To run Docker in Windows on Parallels for Mac [enable nested virtualization](https://kb.parallels.com/en/116239).
  - To run Docker on Windows [enable Hyper-V](https://docs.microsoft.com/en-us/virtualization/hyper-v-on-windows/quick-start/enable-hyper-v#enable-the-hyper-v-role-through-settings)
- Install [NoSQL Workbench for Amazon DynamoDB](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/workbench.settingup.html).
- Install [.NET Core SDK](https://dotnet.microsoft.com/download) 3.1 or greater.
- Install [dotnet aspnet-codegenerator](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/tools/dotnet-aspnet-codegenerator?view=aspnetcore-3.1).

## DynamoDb Local

- Run DynamoDb Local with Docker.
  - Start DynamoDb Local.
    ```
    docker run -p 8000:8000 -d amazon/dynamodb-local
    ```
  - Check to see if it is running.
    ```
    docker ps
    ```
    ![dynamodb-local](images/dynamodb-local-docker.png)

- Add connection in NoSQL Workbench to DynamoDb Local.
  - Select Operations Builder, click Add New Connection, select DynamoDb Local, enter connection name of `Local`.
\
    ![nosql-local](images/nosql-wb-dynamodb-local.png)

## Data Model

- For this tutorial we will use sample Employee Data Model that comes with NoSQL Workbench.
\
    ![sampple-models](images/sample-data-models.png)
- Select the Employee table to new the data model.
\
    ![employee-model](images/employee-data-model.png)
- Visualize the Employee data model.
\
    ![visuaize-model](images/visualize-data-model.png)
- Commit the Employee data model.
\
    ![commit-model](images/commit-data-model.png)
- Select Operations builder, Local connection, Employee data model.
\
    ![operations-model](images/operation-builder-model.png)

## Web API

- Create a new ASP.NET Core Web API project.
  ```
  dotnet new webapi -n HelloDynamoDb
  cd HelloDynamoDb
  ```
- Add required packages.
  ```
  dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
  dotnet add package AWSSDK.DynamoDBv2
  dotnet add package AWSSDK.Extensions.NETCore.Setup
  ```
- Add `DynamoDb` section to appsettings.json.
  ```json
  "DynamoDb": {
    "LocalMode": true,
    "LocalServiceUrl": "http://localhost:8000"
  }
  ```
- Update `ConfigureServices` in `Startup` to register `IAmazonDynamoDB` with `LocalServiceUrl`.
  ```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      var localServiceUrl = Configuration.GetValue<string>("DynamoDb:LocalServiceUrl");
      services.AddSingleton<IDynamoDBContext>(sp =>
          {
              var clientConfig = new AmazonDynamoDBConfig { ServiceURL = localServiceUrl };
              var client = new AmazonDynamoDBClient(clientConfig);
              return new DynamoDBContext(client);
          });
      services.AddControllers();
  }
  ```

## Model Controller

- Create Employee model class.
  - Add properties based on data model attributes.
  - Add [DynamoDB attributes](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DeclarativeTagsList.html).
  ```csharp
  [DynamoDBTable("Employee")]
  public class Employee
  {
      [DynamoDBHashKey]
      public string LoginAlias { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string ManagerLoginAlias { get; set; }
      public string Designation { get; set; }
      public ICollection<string> Skills { get; set; }
  }
  ```
- Scaffold Web API Controller.
```
dotnet aspnet-codegenerator controller -name EmployeeController -actions -api -outDir Controllers
```
- Inject `IDynamoDBContext` into controller constructor.
  ```csharp
  public IDynamoDBContext Context { get; }

  public EmployeeController(IDynamoDBContext context)
  {
      Context = context;
  }
  ```
- Write methods for Get, Post, Put and Delete.
  ```csharp
  // GET: api/Employee
  [HttpGet]
  public async Task<ActionResult<IEnumerable<Employee>>> Get()
  {
      var result = await Context
          .ScanAsync<Employee>(Enumerable.Empty<ScanCondition>())
          .GetRemainingAsync();
      return Ok(result);
  }

  // GET: api/Employee/john
  [HttpGet("{id}", Name = nameof(Get))]
  public async Task<ActionResult<Employee>> Get(string id)
  {
      var result = await Context.LoadAsync<Employee>(id);
      if (result == null) return NotFound();
      return Ok(result);
  }

  // POST: api/Employee
  [HttpPost]
  public async Task<IActionResult> Post([FromBody] Employee value)
  {
      await Context.SaveAsync(value);
      return CreatedAtAction(nameof(Get), new { id = value.LoginAlias });
  }

  // PUT: api/Employee/john
  [HttpPut("{id}")]
  public async Task<ActionResult<Employee>> Put(string id, [FromBody] Employee value)
  {
      if (string.Compare(id, value.LoginAlias, true) != 0) return BadRequest();
      await Context.SaveAsync(value);
      var result = await Context.LoadAsync<Employee>(id, new DynamoDBContextConfig{ ConsistentRead = true });
      return Ok(result);
  }

  // DELETE: api/ApiWithActions/john
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id)
  {
      var result = await Context.LoadAsync<Employee>(id);
      if (result == null) return NotFound();
      await Context.DeleteAsync<Employee>(result);
      return NoContent();
  }
  ```
