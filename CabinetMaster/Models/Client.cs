namespace CabinetMaster.Models;

public class Client
{
    public int Id { get; set; }
    public string ClientName { get; set; }
    public string PhoneNumber { get; set; }
    public string Comment { get; set; }
    public override string ToString() => ClientName;
}