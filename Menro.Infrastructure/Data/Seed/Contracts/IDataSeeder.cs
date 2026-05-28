namespace Menro.Infrastructure.Data.Seed.Contracts
{
    public interface IDataSeeder
    {
        int Order { get; }
        Task SeedAsync();
    }
}
