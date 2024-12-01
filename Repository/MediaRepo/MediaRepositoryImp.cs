using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.Media;

namespace PototoTrade.Repository.MediaRepo;

public class MediaRepositoryImp : MediaRepository
{
    private readonly DBC _context;

    public MediaRepositoryImp(DBC context)
    {
        _context = context;
    }

    public async Task CreateMedias(int sourceId, List<Media> medias)
    {
        foreach (var media in medias)
        {
            media.SourceId = sourceId;
        }
        await _context.Media.AddRangeAsync(medias);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMediaBySourceId(int sourceId)
    {
        var mediaToDelete = _context.Media.Where(m => m.SourceId == sourceId).ToList();

        _context.Media.RemoveRange(mediaToDelete);

        await _context.SaveChangesAsync();
    }

    public async Task<List<Media>> GetMediaBySourceId(int sourceId)
    {
        return await _context.Media.Where(m => m.SourceId == sourceId).ToListAsync();
    }

    public async Task<Media?> GetMediaBySourceIdAndType(int sourceId, string sourceType)
    {
        return await _context.Media
            .FirstOrDefaultAsync(m => m.SourceId == sourceId && m.SourceType == sourceType);
    }

    public async Task<bool> UpdateMedias(int sourceId, List<Media> medias)
    {
        _context.Media.UpdateRange(medias);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Media>> GetMediaListBySourceIdAndType(int sourceId, string sourceType)
    {
        return await _context.Media
            .Where(m => m.SourceId == sourceId && m.SourceType == sourceType)
            .ToListAsync();
    }

    public async Task CreateMedia(Media media)
    {
        _context.Media.Add(media);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteMedia(List<Media> mediaList)
    {
        _context.Media.RemoveRange(mediaList);
        await _context.SaveChangesAsync();
    }

    public async Task<Media?> GetFirstMediaBySourceIdAndType(int sourceId, string sourceType)
    {
        return await _context.Media
            .Where(m => m.SourceId == sourceId && m.SourceType == sourceType)
            .OrderBy(m => m.Id) // Ensure the media with the smallest ID is selected
            .FirstOrDefaultAsync();
    }

}
