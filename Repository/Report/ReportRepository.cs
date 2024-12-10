using System;
using PototoTrade.DTO.ReportDTO;
using PototoTrade.Repositories;

namespace PototoTrade.Repository.Report;

public interface ReportRepository
{
    Task<List<SalesReportDTO>> GetSalesReport(TimeFrame timeFrame);
    Task<List<TransactionReportDTO>> GetTransactionReport(TimeFrame timeFrame);
    Task<List<UserStatusReportDTO>> GetUserStatusReport(TimeFrame? timeFrame = null);
    Task<List<ProductCategoryReportDTO>> GetProductCategoryReport(TimeFrame? timeFrame = null);
    Task<List<RevenueReportDTO>> GetRevenueReport(TimeFrame timeFrame);
    Task<List<ReviewReportDTO>> GetReviewReport(TimeFrame timeFrame);

}
