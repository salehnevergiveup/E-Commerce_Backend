using System;
using PototoTrade.DTO.Common;
using PototoTrade.Models.Media;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Service.Utilities.Exceptions;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Service.Product
{
    public class MediaSrv
    {

        private readonly MediaRepository _mediaRepository;

        public MediaSrv(MediaRepository mediaRepository, ILogger<ProductSrv> logger)
        {
            _mediaRepository = mediaRepository;
        }

       public async Task CreateMedia(int sourceId, string sourceType, string mediaUrl)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(mediaUrl) || sourceId <= 0 || string.IsNullOrWhiteSpace(sourceType))
            {
                throw new CustomException<GeneralMessageDTO>(
                    ExceptionEnum.GetException("INVALID_MEDIA_INPUT"),
                    new GeneralMessageDTO
                    {
                        Message = "Invalid media input.",
                        Success = false
                    }
                );
            }

            var media = new Media
            {
                SourceId = sourceId,
                SourceType = sourceType,
                MediaUrl = mediaUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.CreateMedia(media);
        }

        public async Task DeleteMedia(int sourceId, string sourceType)
        {
            var mediaList = await _mediaRepository.GetMediaListBySourceIdAndType(sourceId, sourceType);

            if (mediaList == null || !mediaList.Any())
            {
                throw new CustomException<GeneralMessageDTO>(
                    ExceptionEnum.GetException("MEDIA_NOT_FOUND"),
                    new GeneralMessageDTO
                    {
                        Message = "No media found for the given source ID and type.",
                        Success = false
                    }
                );
            }

            await _mediaRepository.DeleteMedia(mediaList);
        }

        public async Task<Media?> GetFirstMediaBySourceIdAndType(int sourceId, string sourceType)
        {
            return await _mediaRepository.GetFirstMediaBySourceIdAndType(sourceId, sourceType);
        }

    }
}
