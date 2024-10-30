using PototoTrade.Models.Media;

namespace PototoTrade.Repository.MediaRepo;

public interface MediaRepository
{
    public Task <List<Media>> GetMediaBySourceId(int sourceId);  
    public Task <Media?> GetMediaBySourceIdAndType(int sourceId, string sourceType);  

    public Task  CreateMedias(int sourceId, List<Media> medias); 

    public Task DeleteMediaBySourceId(int sourceId);

    public Task<bool> UpdateMedias(int sourceId , List<Media> medias);
}
