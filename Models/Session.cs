using Newtonsoft.Json;

using System.DirectoryServices.AccountManagement;

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

    public static System.DirectoryServices.AccountManagement.UserPrincipal Current { get; }

    public int? TokensUsed { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public List<Message> Messages { get; set; }

    public Session()
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Session);
        SessionId = this.Id;

        ///var context = new PrincipalContext(ContextType.Domain);
        ///UserID = UserPrincipal.Current.EmailAddress;
        UserID = "newTest";
        TokensUsed = 0;
        Name = "New Chat";
        Messages = new List<Message>();
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
