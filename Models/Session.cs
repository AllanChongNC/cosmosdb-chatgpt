using Newtonsoft.Json;

using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Http;

namespace Cosmos.Chat.GPT.Models;


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
        UserID = string.Empty;
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
