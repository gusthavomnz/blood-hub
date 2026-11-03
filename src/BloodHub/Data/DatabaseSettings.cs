namespace BloodHub.Data;

public sealed record DatabaseSettings(string Host, uint Port, string Database, string User, string Password)
{
    public static DatabaseSettings FromEnvironment() => new(
        Host: Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "localhost",
        Port: uint.Parse(Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306"),
        Database: Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "bloodhub",
        User: Environment.GetEnvironmentVariable("MYSQL_USER") ?? "bloodhub",
        Password: Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? "bloodhub");
}
