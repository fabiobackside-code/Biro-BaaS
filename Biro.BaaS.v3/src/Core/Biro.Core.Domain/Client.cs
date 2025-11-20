namespace Biro.Core.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Document { get; set; }
    public List<Account> Accounts { get; set; } = new();
}
