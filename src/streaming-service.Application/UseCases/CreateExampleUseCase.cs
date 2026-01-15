using System.Threading;
using System.Threading.Tasks;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;
using System;

namespace streaming_service.Application.UseCases
{
    public class CreateExampleRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateExampleResponse
    {
        public Guid Id { get; set; }
    }

    public class CreateExampleUseCase
    {
        private readonly IExampleRepository _repo;

        public CreateExampleUseCase(IExampleRepository repo)
        {
            _repo = repo;
        }

        public async Task<CreateExampleResponse> Handle(CreateExampleRequest req, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(req.Name)) throw new ArgumentException("Name required", nameof(req.Name));
            var entity = new ExampleAggregate(req.Name);
            await _repo.AddAsync(entity, ct);
            return new CreateExampleResponse { Id = entity.Id };
        }
    }
}
