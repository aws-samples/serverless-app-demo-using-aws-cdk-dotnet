# Build a serverless application using AWS CDK for .NET


## Solution Overview

![00_architecture](./images/00_architecture.jpg)

## Step 1: Setting up the environment

### Install .NET 6.0

Execute the following command to add the Microsoft package signing key to your list of trusted keys and add the Microsoft package repository before installing .NET Core 6:

```bash
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
```

Execute the following command to install the.NET SDK:

```bash
sudo yum install dotnet-sdk-6.0
dotnet new tool-manifest
dotnet tool install Amazon.Lambda.Tools
#dotnet tool install -g Amazon.Lambda.Tools
```

## Step 2: Clone and setup the AWS CDK application

On your local machine, clone the AWS CDK application with the following command:

```shell
git clone https://github.com/aws-samples/serverless-app-demo-using-aws-cdk-dotnet.git
```

Directory structure after cloning:

![01_directory](./images/01_directory.jpg)


## Step 3: Package the NET 6 lambda function

The DepartmentSvc lambda function in the ServerlessApp directory must be packaged and copied to the CdkServerlessApp\lambdas folder.

```bash
cd serverless-app-demo-using-aws-cdk-dotnet/ServerlessApp/DepartmentSvc/src/DepartmentSvc/
dotnet lambda package
cp bin/Release/net6.0/DepartmentSvc.zip ../../../../CdkServerlessApp/lambdas
```


## Step 3: Run AWS CDK Application

Build the CDK code before deploying to the console:

```bash
cd ../../../../CdkServerlessAppV2/
dotnet build
```

#### A quick overview of the AWS CDK application

Before we deploy the application, let's look at the code. First look at the directory structure:

```shell
.
├── CdkServerlessApp.csproj
├── CdkServerlessApp.sln
├── CdkServerlessStack.cs
├── Program.cs
├── cdk.json
├── lambdas
    └── DepartmentSvc.zip   
```

The cdk.json file tells the AWS CDK Toolkit how to execute your app.


Before you deploy any AWS CDK application, you need to bootstrap a space in your account and region you are deploying into. To bootstrap in your default region, issue the following command:

```bash
cdk bootstrap
```


If you want to deploy into a specific account and region, issue the following command:

```bash
cdk bootstrap aws://ACCOUNT-NUMBER/REGION
```

Replace ACCOUNT-NUMBER/REGION with your account number and region respectively. For more information, visit [Getting started with the AWS CDK](https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html)


At this point you can now synthesize the [AWS CloudFormation](https://aws.amazon.com/cloudformation/) template for this code.

```shell
cdk synth
cdk deploy
```

CDK deploys the environment to AWS.


## Step 4: Verify resources in console

You can monitor the progress using the CloudFormation console.

![02_CFNConsole](./images/02_CFNConsole.jpg)


## Step 5: Review the resource creation code

#### Create IAM roles

```cs
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
```

#### Create DynamoDB table

The following snippet creates a DynamoDB table with AWS CDK for .NET:

```cs
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
```

#### Create Lambda function

```cs
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
```

## Step 6: Test the serverless app

1. Open API Gateway in AWS console.
1. Under APIs, click on DeptAPI to open the _Resources_ page.
1. Select **ANY** under /dept resource, and click on **TEST**.

![03_API_Gateway_1](./images/03_API_Gateway_1.jpg)

##### POST
Select the method as POST and enter the request body as below and click on Test button. This will create an item in the DynamoDB table.
```json
{
    "DeptId": 1,
    "DeptName": "Human Resources"
}
```

##### GET
Select the method as GET and enter the query string as below and click on Test button. This will retrieve the item with the deptId provided from the DynamoDB table.

Query string:
deptId=1

##### PUT
Select the method as PUT and enter the request body as below and click on Test button. This will update the item with the department name provided in the DynamoDB table.
Request body:
```json
{
    "DeptId": 1,
    "DeptName": "Finance"
}
```

##### DELETE
Select the method as DELETE and enter the request body as below and click on Test button. This will delete the item with the deptId provided in the DynamoDB table.
Request body:
```json
{
    "DeptId": 1
}
```

##### POST
Select the method as POST and enter the request body as below and click on Test button. This will delete the item with the deptId provided in the DynamoDB table.
Request body:
```json
{
    "DeptId": 2,
    "DeptName": "Marketing"
}
```


## Step 8: View data in DynamoDB table

1. Open DynamoDB console, and click on _Tables_ on the left menu.
1. Click on **Department** in the listed tables.
1. Select the _Explore table items_ on the top right corner of the page.

![04_DynamoDB](./images/04_DynamoDB.jpg)


## Step 7: Cleaning up

To avoid incurring additional charges, clean up all the resources that have been created. Run the following command from a terminal window. This deletes all the resources that were created as part of this example.

```bash
cdk destroy
```



## License Summary

This sample code is made available under a modified MIT license. See the LICENSE file.



## Feedback Survey

Thank you for your interest in this lab. We would love your feedback. Kindly fill out this [survey](https://eventbox.dev/survey/Q9O3WQU).
