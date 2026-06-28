using be.Interfaces;
using be.Shared;

namespace be.GetCollection;

public class GetCollectionService(ICollectionRetriever collectionRetriever)
{
    public async Task<Result<Collection>> GetCollectionAsync()
    {
        try
        {
            return Result<Collection>.Success(await collectionRetriever.GetCollectionAsync());
        }
        catch
        {
            return Result<Collection>.Failure("Unable to retrieve collection");
        }
    }
}
