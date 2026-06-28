using be.Shared;

namespace be.Interfaces;

public interface ICollectionRetriever
{
    Task<Collection> GetCollectionAsync();
}
