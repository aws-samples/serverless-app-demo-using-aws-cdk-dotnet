using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DepartmentSvc
{
    public class Function
    {
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private DynamoDBContext _dbContext = new DynamoDBContext(client, new DynamoDBContextConfig {ConsistentRead = true});
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            context.Logger.Log(apigProxyEvent.RequestContext.HttpMethod);
            context.Logger.Log(apigProxyEvent.Body);
            context.Logger.Log(apigProxyEvent.Path);
            var responseObj = new Department();

            switch (apigProxyEvent.RequestContext.HttpMethod)
            {
                case "GET":
                    context.Logger.Log(apigProxyEvent.QueryStringParameters["deptId"]);
                    var deptId = int.Parse(apigProxyEvent.QueryStringParameters["deptId"]);
                    var deptDetails = _dbContext.LoadAsync<Department>(deptId);
                    responseObj.DeptId = deptId;
                    responseObj.DeptName = deptDetails.Result != null ? deptDetails.Result.DeptName : String.Empty; 
                    break;
                case "POST":
                    var inputObj = JsonConvert.DeserializeObject<Department>(apigProxyEvent.Body);
                    context.Logger.Log(inputObj.DeptName);
                    await _dbContext.SaveAsync(inputObj);
                    responseObj = inputObj;
                    break;
                case "PUT":
                    var updateObj = JsonConvert.DeserializeObject<Department>(apigProxyEvent.Body);
                    var deptToUpdate = await _dbContext.LoadAsync<Department>(updateObj.DeptId);
                    deptToUpdate.DeptName = updateObj.DeptName;
                    await _dbContext.SaveAsync(deptToUpdate);
                    responseObj = deptToUpdate;
                    break;
                case "DELETE":
                    var deleteObj = JsonConvert.DeserializeObject<Department>(apigProxyEvent.Body);
                    context.Logger.Log(deleteObj.DeptId.ToString());
                    await _dbContext.DeleteAsync<Department>(deleteObj.DeptId);
                    responseObj.DeptId = deleteObj.DeptId;
                    break;
                default:
                    break;
            }
            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(responseObj),
                StatusCode = 200,
            };
        }
    }

    [DynamoDBTable("Department")]
    public class Department
    {
        [DynamoDBHashKey]
        public int DeptId { get; set; }
        public string DeptName { get; set; }
    }
}