using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PotatoTrade.DTO.Review;
using PototoTrade.Service.Review;
namespace PotatoTrade.Controllers.Review

{
    [Route("api/review")]
    [ApiController]
    public class ReviewController : CustomBaseController
    {

        private readonly ProductReviewService _reviewService;
        public ReviewController(ProductReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            var response = await _reviewService.GetAllReviews();
            return MakeResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviews(int id, [FromQuery] string type)
        {
            var response = await _reviewService.GetReviews(id , type);
            return MakeResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(CreateReviewDTO createReviewDto)
        {
            var response = await _reviewService.CreateReview(createReviewDto, User);
            return MakeResponse(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var response = await _reviewService.DeleteReview(id, User);
            return MakeResponse(response);
        }
    }
}
