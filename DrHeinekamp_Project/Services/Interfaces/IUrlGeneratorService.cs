namespace DrHeinekamp_Project.Services.Interfaces
{
    public interface IUrlGeneratorService
    {
        string GeneratePermanentUrl(string key);
        string GenerateTemporaryUrl(string key, DateTime expiryDateTime);
    }
}
