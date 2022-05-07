using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using DepartmentSvc;

namespace DepartmentSvc.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestToUpperFunction()
        {
            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var proxyResponse = function.FunctionHandler(new APIGatewayProxyRequest{ HttpMethod = "GET"}, context);

            Assert.Equal(200, proxyResponse.Result.StatusCode);
        }
    }
}