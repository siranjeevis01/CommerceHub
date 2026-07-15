using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Modules.Payment.Infrastructure.Data;

public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        var conn = Environment.GetEnvironmentVariable("PAYMENT_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_payment;user=root;password=root";
        optionsBuilder.UseMySQL(conn);
        return new PaymentDbContext(optionsBuilder.Options);
    }
}
