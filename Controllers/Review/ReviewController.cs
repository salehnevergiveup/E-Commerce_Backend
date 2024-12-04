using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PotatoTrade.DTO.Review;
using PototoTrade.Service.Reivew;
namespace PotatoTrade.Controllers.Review

{
    [Route("api/[controller]/public")]
    [ApiController]
    public class ReviewController : CustomBaseController
    {

        private readonly ReviewService _reviewService;
        public ReviewController(ReviewService reviewService)
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
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDTO createReviewDto)
        {
            var response = await _reviewService.CreateReview(createReviewDto, User);
            return MakeResponse(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDTO updateReviewDto)
        {
            var response = await _reviewService.UpdateReview(id, updateReviewDto, User);
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
