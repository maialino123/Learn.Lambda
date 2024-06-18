using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using System.Text.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Learn.Lambda;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandlerAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        

        return request.RequestContext.Http.Method.ToUpper() switch
        {
            "GET" => await HandleGetRequestAsync(request),
            "POST" => await HandlePostRequestAsync(request),
            "DELETE" => await HandleDeleteRequestAsync(request),
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleDeleteRequestAsync(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("userId", out var userIdString);
        if (Guid.TryParse(userIdString, out var userId))
        {
            var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
            var user = await dbContext.LoadAsync<User>(userId);
            if (user != null)
            {
                await dbContext.DeleteAsync(user);
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Deleted user",
                    StatusCode = 200,
                };
            }
        }

        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = "User not found!",
            StatusCode = 404
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequestAsync(APIGatewayHttpApiV2ProxyRequest request)
    {
        try
        {
            var usr = JsonSerializer.Deserialize<User>(request.Body);
            if (usr != null)
            {
                var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
                await dbContext.SaveAsync(usr);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = usr?.Name,
                    StatusCode = 200
                };
            }

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = request.Body,
                StatusCode = 400
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = ex.Message,
                StatusCode = 400
            };
        }
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequestAsync(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("userId", out var userIdString);
        if (Guid.TryParse(userIdString, out var userId))
        {
            var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
            var user = await dbContext.LoadAsync<User>(userId);
            if (user != null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(user),
                    StatusCode = 200,
                };
            }
        }

        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = "Invalid UserID in path",
            StatusCode = 404,
        };
    }
}

public class User
{
    public Guid id { get; set; }
    public string Name { get; set; }
}