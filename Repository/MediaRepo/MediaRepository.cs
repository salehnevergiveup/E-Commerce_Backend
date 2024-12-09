using PototoTrade.Models.Media;

namespace PototoTrade.Repository.MediaRepo;

public interface MediaRepository
{
    public Task <List<Media>> GetMediaBySourceId(int sourceId);  
    public Task <Media?> GetMediaBySourceIdAndType(int sourceId, string sourceType);  

    public Task  CreateMedias(int sourceId, List<Media> medias); 

    Task CreateMedia(Media media);

    public Task DeleteMediaBySourceId(int sourceId);

    public Task<bool> UpdateMedias(int sourceId , List<Media> medias);

    Task<List<Media>> GetMediaListBySourceIdAndType(int sourceId, string sourceType);

    Task DeleteMedia(List<Media> mediaList);

    Task<Media?> GetFirstMediaBySourceIdAndType(int sourceId, string sourceType);
    
}
