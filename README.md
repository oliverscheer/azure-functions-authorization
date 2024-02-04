# Enabling Authorization in Azure Functions

During a project I faced the challenge to use authentication for Azure Functions to 
with Azure Entra Id and multiple providers, like personal and work accounts, and also 
with social accounts like Google, Facebook, and Twitter.

## Problem

Get the user's identity and use it to authorize the user to access the Azure Function.
Because it just doesn't work as expected, like in ASP.NET Core web applications.

## Solution

Read the authentication token and use it to get the user's identity. 
Then, use the identity to authorize the user to access the Azure Function.

## How to do it

The `ClaimsPrincipalParser` class is used to get the user's identity from the current requests authorization token. 
It returns a `ClaimsPrincipal` object that contains the user's identity.

What is not working in Azure Functions correctly is the following code.

```csharp
var identity = (ClaimsIdentity)req.HttpContext.User.Identity;
return new ClaimsPrincipal(identity);
```

To make it work, you need to use the following code.

```csharp	
 ClaimsPrincipal? principal = ClaimsPrincipalHelper.ParseFromRequest(req);
if (principal == null)
{
    messages.Add("Error: User is not authenticated.");
    return new BadRequestObjectResult(messages);
}

string userName = principal.Identity?.Name ?? "Unknown";
string userEmail = principal.FindFirst("preferred_username")?.Value ?? "Unknown";
```

You can also access much more claims instead of 'preferred_username', like oid, based on your needs.

I hope this helps you to enable authorization in your Azure Functions.