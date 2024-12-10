using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PototoTrade.DTO.ReportDTO;
using PototoTrade.Service.Report;

namespace PototoTrade.Controllers.Report
{
    [Route("api/report")]
    [ApiController]
    public class ReportsController : CustomBaseController
    {
        private readonly ReportsService _reportService;

        public ReportsController(ReportsService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("sales")]
        public async Task<IActionResult> GetSalesReport(ReportRequestDTO request)
        {
            return MakeResponse(await _reportService.GetSalesReport(User, request));
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> GetTransactionReport( ReportRequestDTO request)
        {
            return MakeResponse(await _reportService.GetTransactionReport(User, request));
        }

        [HttpPost("reviews")]
        public async Task<IActionResult> GetReviewReport([FromBody] ReportRequestDTO request)
        {
            var result = await _reportService.GetReviewReport(User, request);
            return MakeResponse(result);
        }

        [HttpPost("user-status")]
        public async Task<IActionResult> GetUserStatusReport(ReportRequestDTO request)
        {
            return MakeResponse(await _reportService.GetUserStatusReport(User, request));
        }

        [HttpPost("product-categories")]
        public async Task<IActionResult> GetProductCategoryReport(ReportRequestDTO request)
        {
            return MakeResponse(await _reportService.GetProductCategoryReport(User, request));
;
        }

        [HttpPost("revenue")]
        public async Task<IActionResult> GetRevenueReport([FromBody] ReportRequestDTO request)
        {
            return MakeResponse(await _reportService.GetRevenueReport(User, request));
        }

    }
}
