using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace CdkServerlessAppV2
{
    public class CdkServerlessAppV2Stack : Stack
    {
        internal CdkServerlessAppV2Stack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            #region iamroles
            var iamLambdaRole = new Role(this,"LambdaExecutionRole", new RoleProps
            {
                RoleName = "LambdaExecutionRole",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
            });
            
            iamLambdaRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonDynamoDBFullAccess"));
            iamLambdaRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("CloudWatchLogsFullAccess"));
            iamLambdaRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AWSXrayFullAccess"));
            iamLambdaRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("CloudWatchLambdaInsightsExecutionRolePolicy"));
            iamLambdaRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new [] {"cloudwatch:PutMetricData"},
                Resources = new [] {"*"}
            }));

            #endregion iamroles
            
            #region DynamoDB tables
            
            var departmentTable = new Table(this, "Department", new TableProps
            {
                TableName = "Department",
                PartitionKey = new Attribute
                {
                    Name = "DeptId",
                    Type = AttributeType.NUMBER
                },
                RemovalPolicy = RemovalPolicy.DESTROY,
                ContributorInsightsEnabled = true
            });
            
            #endregion
            
            #region Lambda 
            var DepartmentSvcLambda = new Function(this,"DepartmentSvc", new FunctionProps
            {
                FunctionName = "DepartmentSvc",
                Runtime = Runtime.DOTNET_6,
                Handler = "DepartmentSvc::DepartmentSvc.Function::FunctionHandler", 
                Role = iamLambdaRole,
                Code = Code.FromAsset("lambdas/DepartmentSvc.zip"),
                Timeout = Duration.Seconds(300),
                Tracing = Tracing.ACTIVE
            });
            
            #endregion
            
            #region API Gateway
            var api = new RestApi(this, "DeptAPI", new RestApiProps
            {
                RestApiName = "DeptAPI",
                Description = "This service triggers the department microservice workflow."
            });

            var deptResource =  api.Root.AddResource("dept");
            var DepartmentSvcIntegration =  new LambdaIntegration(DepartmentSvcLambda, new LambdaIntegrationOptions
            {
                Proxy = true,
                PassthroughBehavior = PassthroughBehavior.WHEN_NO_TEMPLATES,
                //Integration request
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "#set($inputRoot = $input.path(\'$\')) { \"DeptId\" : \"$inputRoot.DeptId\",  \"DeptName\" : \"$inputRoot.DeptName\"}"
                },
                //Integration response
                IntegrationResponses = new IIntegrationResponse[]
                {
                    new IntegrationResponse
                    {
                        StatusCode = "200",
                        ResponseTemplates = new Dictionary<string, string>
                        {
                            { "application/json", "" } 
                        }
                    }
                }
            });
            
            var anyMethod = deptResource.AddMethod("ANY", DepartmentSvcIntegration, new MethodOptions
            {
                //Method response
                MethodResponses = new[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200", ResponseModels = new Dictionary<string, IModel>()
                        {
                            ["application/json"] =Model.EMPTY_MODEL
                        }
                    }
                }

            });
            
            var mockIntegration = new MockIntegration(new IntegrationOptions
            {
                //Integration request
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                },
                //Integration response
                IntegrationResponses = new IIntegrationResponse[]
                {
                    new IntegrationResponse
                    {
                        StatusCode = "200",
                        ResponseTemplates = new Dictionary<string, string>
                        {
                            { "application/json", "" } 
                        }
                    }
                }
            });
            var mockMethod = deptResource.AddMethod("OPTIONS", mockIntegration, new MethodOptions
            {
                //Method response
                MethodResponses = new[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200", ResponseModels = new Dictionary<string, IModel>()
                        {
                            ["application/json"] = Model.EMPTY_MODEL
                        }
                    }
                }
            });
           
            #endregion
        }
    }
}
