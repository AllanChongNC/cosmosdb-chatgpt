using Newtonsoft.Json;

using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Cosmos.Chat.GPT.Models;

public static class StaticWebAppsAuth()
{
  private class ClientPrincipal
  {
      public string IdentityProvider { get; set; }
      public string UserId { get; set; }
      public string UserDetails { get; set; }
      public IEnumerable<string> UserRoles { get; set; }
  }
  public static ClaimsPrincipal Parse(HttpRequest req)
  {
      var principal = new ClientPrincipal();
      if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
      {
          var data = header[0];
          var decoded = Convert.FromBase64String(data);
          var json = Encoding.UTF8.GetString(decoded);
          principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
      }
      principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);
      if (!principal.UserRoles?.Any() ?? true)
      {
          return new ClaimsPrincipal();
      }
      var identity = new ClaimsIdentity(principal.IdentityProvider);
      identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
      identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
      identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));
      return new ClaimsPrincipal(identity);
  }
}



/// using(var context = new PrincipalContext(ContextType.Domain)) {};


public record Session
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; }

    public string Type { get; set; }

    /// <summary>
    /// Partition key
    /// </summary>
    public string SessionId { get; set; }

    public string UserID { get; set; }

    ///public static System.DirectoryServices.AccountManagement.UserPrincipal Current { get; }

    ///public System.Security.Principal.IPrincipal User { get; set; }

    public int? TokensUsed { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public List<Message> Messages { get; set; }

    public Session()
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Session);
        SessionId = this.Id;

        ///PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

        ///UserPrincipal testuser = UserPrincipal.FindByIdentity(ctx, HttpContext.User.Identity.Name);

        ///UserID = GetLocalIPAddress();

        ///var context = new PrincipalContext(ContextType.Domain);

        ///HttpContext httpContext = HttpContext.Current;
        ///string authHeader = this.HttpContext.Request.Headers["Authorization"];
        UserID = StaticWebAppsAuth();
        ///UserID = HttpContext.User.Identity.Name;
        ///Principal.Current.EmailAddress;
        ///UserID = System.Environment.UserName;
        ///UserID = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).Identity.Name;
        ///UserID = "newTest";
        TokensUsed = 0;
        Name = "New Chat";
        Messages = new List<Message>();
    }

    private string GetIpAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress[] addresses = host.AddressList;
        IPAddress firstIpAddress = addresses[1];

        ///return host.ToString();
        return firstIpAddress.ToString();
    }

    public static string GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip.ToString();
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}

    public void AddMessage(Message message)
    {
        Messages.Add(message);
    }

    public void UpdateMessage(Message message)
    {
        var match = Messages.Single(m => m.Id == message.Id);
        var index = Messages.IndexOf(match);
        Messages[index] = message;
    }
}
