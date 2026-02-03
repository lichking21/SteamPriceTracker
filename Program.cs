using Microsoft.Extensions.Configuration;
class Program
{
    static async Task Main()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration configuration = builder.Build();

        DBImport dbImport = new DBImport(configuration);
        GetGamesList getList = new GetGamesList();

        await dbImport.ImportDataToDB(await getList.ParsedJson());
    }
}