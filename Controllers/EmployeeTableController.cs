using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HelloDynamoDb.Controllers
{
    [Route("api/employee/table")]
    [ApiController]
    public class EmployeeTableController : ControllerBase
    {
        private const string TableName = "Employee";

        public IWebHostEnvironment HostEnvironment { get; }
        public IAmazonDynamoDB Client { get; }

        public EmployeeTableController(IWebHostEnvironment hostEnvironment, IAmazonDynamoDB client)
        {
            HostEnvironment = hostEnvironment;
            Client = client;
        }

        // GET: api/employee/table/create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var response = await Client.ListTablesAsync(new ListTablesRequest { Limit = 10 });
            if (!response.TableNames.Contains(TableName))
            {
                var createRequest = new CreateTableRequest
                {
                    TableName = TableName,
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "LoginAlias",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "FirstName",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "LastName",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "ManagerLoginAlias",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "Designation",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "Skills",
                            AttributeType = "SS" // Only S is acceptable (String Set)
                        },
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "LoginAlias",
                            KeyType = "HASH"  // Partition key
                        }
                    },
                    GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                    {
                        new GlobalSecondaryIndex
                        {
                            IndexName = "Name",
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement
                                {
                                    AttributeName = "FirstName",
                                    KeyType = "HASH" // Partition key
                                },
                                new KeySchemaElement
                                {
                                    AttributeName = "LastName",
                                    KeyType = "RANGE" // Sort key
                                }
                            },
                            Projection = new Projection
                            {
                                ProjectionType = "ALL"
                            }
                        },
                        new GlobalSecondaryIndex
                        {
                            IndexName = "DirectReports",
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement
                                {
                                    AttributeName = "ManagerLoginAlias",
                                    KeyType = "HASH" // Partition key
                                }
                            },
                            Projection = new Projection
                            {
                                ProjectionType = "INCLUDE",
                                NonKeyAttributes = { "LoginAlias", "FirstName", "LastName" }
                            }
                        }
                    }

                };
                await Client.CreateTableAsync(createRequest);
            }
            return NoContent();
        }

        // GET: api/employee/table/load
        [HttpGet("load")]
        public async Task<IActionResult> Load()
        {
            var table = Table.LoadTable(Client, TableName);
            var contentRoot = HostEnvironment.ContentRootPath;
            var jsonArray = await System.IO.File.ReadAllTextAsync(Path.Combine(HostEnvironment.ContentRootPath, "Models", TableName + ".json"));
            var jsonItems = JArray.Parse(jsonArray);
            foreach (var json in jsonItems)
            {
                var item = Document.FromJson(json.ToString());
                await table.PutItemAsync(item);
            }
            return NoContent();
        }
    }
}
