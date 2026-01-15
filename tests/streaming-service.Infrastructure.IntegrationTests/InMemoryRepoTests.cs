using System;
using System.Threading.Tasks;
using Xunit;
using streaming_service.Infrastructure.Repositories;
using streaming_service.Domain.Entities;

namespace streaming_service.Infrastructure.IntegrationTests
{
    public class InMemoryRepoTests
    {
        [Fact]
        public async Task AddAndGet_ReturnsEntity()
        {
            var repo = new InMemoryExampleRepository();
            var agg = new ExampleAggregate("name");

            await repo.AddAsync(agg);
            var fetched = await repo.GetByIdAsync(agg.Id);

            Assert.NotNull(fetched);
            Assert.Equal(agg.Id, fetched!.Id);
        }
    }
}
