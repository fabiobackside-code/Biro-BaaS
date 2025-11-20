namespace Biro.Core.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string AccountNumber { get; set; }
    public string BranchCode { get; set; }
    public ProductType ProductType { get; set; }
    public AccountStatus Status { get; set; }
    public Client Client { get; set; }
}
