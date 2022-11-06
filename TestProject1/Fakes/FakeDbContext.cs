using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Data;

namespace TestProject1.Fakes
{
    internal class FakeDbContext : ApplicationDbContext
    {
        public static FakeDbContext ContextFactory()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;
            return new FakeDbContext(options);

        }
        public FakeDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public int SaveCount { get; set; }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount += 1;
            return Task.FromResult(SaveCount);
        }
    }
}
