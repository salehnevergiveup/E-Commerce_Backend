using PototoTrade.Models.Media;

namespace PototoTrade.Repository.MediaRepo;

public interface MediaRepository
{
    public Task<List<Media>> GetMediaBySourceId(int sourceId);
    public Task<Media?> GetMediaBySourceIdAndType(int sourceId, string sourceType);

    public Task  CreateMedias(int sourceId, List<Media> medias); 

    Task CreateMedia(Media media);

    public Task<List<Media>> GetMediaListBySourceIdAndType(int sourceId, string sourceType);


    public Task DeleteMediaBySourceIdAndType(int sourceId, string sourceType);

    public Task<bool> UpdateMedias(int sourceId , List<Media> medias);

    Task DeleteMedia(List<Media> mediaList);

    Task<Media?> GetFirstMediaBySourceIdAndType(int sourceId, string sourceType);
    
    public Task DeleteMedia(Media media);

    public Task<List<Media>> GetMediaListBySourceIdsAndType(List<int> sourceIds, string sourceType);

}
