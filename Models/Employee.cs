using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace HelloDynamoDb.Models
{
    [DynamoDBTable("Employee")]
    public class Employee
    {
        [DynamoDBHashKey]
        public string LoginAlias { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ManagerLoginAlias { get; set; }
        public string Designation { get; set; }
        public List<string> Skills { get; set; }
    }
}
