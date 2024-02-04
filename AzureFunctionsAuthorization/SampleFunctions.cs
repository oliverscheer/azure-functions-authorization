using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Security.Claims;
using System.Text.Json;

namespace AzureFunctionsAuthorization
{
    public class SampleFunctions
    {
        private readonly JsonSerializerOptions jsonSerializationOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private enum Methods
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        [Function("Add")]
        public async Task<IActionResult> Add([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            var result = await GenericCalculationMethod(Methods.Add, req);
            return result;
        }

        [Function("Subtract")]
        public async Task<IActionResult> Subtract([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            var result = await GenericCalculationMethod(Methods.Subtract, req);
            return result;
        }

        [Function("Multiply")]
        public async Task<IActionResult> Multiply([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            var result = await GenericCalculationMethod(Methods.Multiply, req);
            return result;
        }

        [Function("Divide")]
        public async Task<IActionResult> Divide([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req, [FromQuery] int value1, [FromQuery] int value2)
        {
            var result = await GenericCalculationMethod(Methods.Divide, req);
            return result;
        }

        private async Task<IActionResult> GenericCalculationMethod(Methods method, HttpRequest req)
        {
            List<string> messages = [ $"{method} called"];

            try
            {
                ClaimsPrincipal? principal = ClaimsPrincipalHelper.ParseFromRequest(req);
                if (principal == null)
                {
                    messages.Add("Error: User is not authenticated.");
                    return new BadRequestObjectResult(messages);
                }

                #region Debug Output

                // Debug output
                //messages.Add($"User: '{principal.Identity?.Name}'");
                //messages.Add("Claims");
                //foreach (var claim in principal.Claims)
                //{
                //    messages.Add($"Claim: {claim.Type} - {claim.Value}");
                //}

                #endregion

                // The Calculation method
                
                string? requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                messages.Add($"Request body: '{requestBody}'");

                CalculationValues? data = JsonSerializer.Deserialize<CalculationValues>(requestBody, jsonSerializationOptions);
                if (data == null)
                {
                    messages.Add("Error: Request body is empty.");
                    return new BadRequestObjectResult(messages);
                }

                int result = 0;

                switch (method)
                {
                    case Methods.Add:
                        result = data.Value1 + data.Value2;
                        break;
                    case Methods.Subtract:
                        result = data.Value1 - data.Value2;
                        break;
                    case Methods.Multiply:
                        result = data.Value1 * data.Value2;
                        break;
                    case Methods.Divide:
                        result = data.Value1 / data.Value2;
                        break;
                }
                
                string userName = principal.Identity?.Name ?? "Unknown";
                string userEmail = principal.FindFirst("preferred_username")?.Value ?? "Unknown";

                var resultObject = new
                {
                    Result = result,
                    UserName = userName,
                    UserEmail = userEmail,
                    Messages = messages
                };
                return new OkObjectResult(resultObject);
            }
            catch (Exception ex)
            {
                messages.Add("Error: " + ex.Message);
                return new BadRequestObjectResult(messages);
            }
        }
    }
}