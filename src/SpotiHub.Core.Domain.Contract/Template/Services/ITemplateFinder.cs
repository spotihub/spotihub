namespace SpotiHub.Core.Domain.Contract.Template.Services;

public interface ITemplateFinder
{
    public Task<Entity.Template> GetRandomDefaultTemplate(CancellationToken cancellationToken = default);
}