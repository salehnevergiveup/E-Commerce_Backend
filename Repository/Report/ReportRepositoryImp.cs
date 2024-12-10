using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.DTO.ReportDTO;
using PototoTrade.Repository.Report;

namespace PototoTrade.Repositories
{
    public enum TimeFrame
    {
        Weekly,
        Monthly,
        Annually,
        FiveYears
    }

    public class ReportRepositoryImp : ReportRepository
    {
        private readonly DBC _context;

        public ReportRepositoryImp(DBC context)
        {
            _context = context;
        }

        private (DateTime StartDate, DateTime EndDate) GetDateRange(TimeFrame timeFrame)
        {
            DateTime endDate = DateTime.Now.Date; 
            DateTime startDate;

            switch (timeFrame)
            {
                case TimeFrame.Weekly:
                    startDate = endDate.AddDays(-7);
                    break;

                case TimeFrame.Monthly:
                    startDate = endDate.AddMonths(-12);
                    break;

                case TimeFrame.Annually:
                    startDate = endDate.AddYears(-7);
                    break;

                case TimeFrame.FiveYears:
                    startDate = endDate.AddYears(-5);
                    break;

                default:
                    throw new ArgumentException("Invalid time frame.");
            }

            return (startDate, endDate);
        }


        public async Task<List<SalesReportDTO>> GetSalesReport(TimeFrame timeFrame)
        {
            var (startDate, endDate) = GetDateRange(timeFrame);
            var timeFrameLower = timeFrame.ToString().ToLower();

            var query = _context.PurchaseOrders
                .Where(order => order.OrderCreatedAt >= startDate
                             && order.OrderCreatedAt <= endDate
                             && order.Status.ToLower() == "completed");
            if (timeFrameLower == "annually" || timeFrameLower == "fiveyears")
            {
                var sales = await query
                    .GroupBy(order => order.OrderCreatedAt.Year)
                    .Select(group => new SalesReportDTO
                    {
                        GroupingKey = group.Key.ToString(),
                        TotalSales = group.Count(),
                        TotalSalesAmount = group.Sum(o => o.TotalAmount)
                    })
                    .ToListAsync();

                return sales;
            }
            else if (timeFrameLower == "monthly")
            {

                var sales = await query
                    .GroupBy(order => new { order.OrderCreatedAt.Year, order.OrderCreatedAt.Month })
                    .Select(group => new SalesReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}",
                        TotalSales = group.Count(),
                        TotalSalesAmount = group.Sum(o => o.TotalAmount)
                    })
                    .ToListAsync();

                return sales;
            }
            else
            {

                var sales = await query
                    .GroupBy(order => new { order.OrderCreatedAt.Year, order.OrderCreatedAt.Month, order.OrderCreatedAt.Day })
                    .Select(group => new SalesReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}-{group.Key.Day:00}",
                        TotalSales = group.Count(),
                        TotalSalesAmount = group.Sum(o => o.TotalAmount)
                    })
                    .ToListAsync();

                return sales;
            }
        }


        public async Task<List<TransactionReportDTO>> GetTransactionReport(TimeFrame timeFrame)
        {
            var (startDate, endDate) = GetDateRange(timeFrame);
            var timeFrameLower = timeFrame.ToString().ToLower();

            var query = _context.WalletTransactions
                .Where(tx => tx.CreatedAt >= startDate
                         && tx.CreatedAt <= endDate);

            if (timeFrameLower == "annually" || timeFrameLower == "fiveyears")
            {
                var transactions = await query
                    .GroupBy(tx => tx.CreatedAt.Year)
                    .Select(group => new TransactionReportDTO
                    {
                        GroupingKey = group.Key.ToString(),
                        TotalTransactions = group.Count(),
                        TotalTransactionAmount = group.Sum(tx => tx.Amount)
                    })
                    .ToListAsync();

                return transactions;
            }
            else if (timeFrameLower == "monthly")
            {
                var transactions = await query
                    .GroupBy(tx => new { tx.CreatedAt.Year, tx.CreatedAt.Month })
                    .Select(group => new TransactionReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}",
                        TotalTransactions = group.Count(),
                        TotalTransactionAmount = group.Sum(tx => tx.Amount)
                    })
                    .ToListAsync();

                return transactions;
            }
            else // Weekly
            {
                var transactions = await query
                    .GroupBy(tx => new { tx.CreatedAt.Year, tx.CreatedAt.Month, tx.CreatedAt.Day })
                    .Select(group => new TransactionReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}-{group.Key.Day:00}",
                        TotalTransactions = group.Count(),
                        TotalTransactionAmount = group.Sum(tx => tx.Amount)
                    })
                    .ToListAsync();

                return transactions;
            }
        }
        public async Task<List<ReviewReportDTO>> GetReviewReport(TimeFrame timeFrame)
        {
            var (startDate, endDate) = GetDateRange(timeFrame);
            var timeFrameLower = timeFrame.ToString().ToLower();

            // Query ProductReview for reviews within the date range
            var reviewsQuery = _context.ProductReviews
                .AsNoTracking() // Improves performance for read-only operations
                .Where(review =>
                    review.ReviewDate >= startDate &&
                    review.ReviewDate <= endDate
                );

            // Project the necessary data
            var projectedReviews = reviewsQuery
                .Select(review => new
                {
                    review.ReviewDate,
                    review.Rating
                });

            List<ReviewReportDTO> reviewReports = new List<ReviewReportDTO>();

            if (timeFrameLower == "annually" || timeFrameLower == "fiveyears")
            {
                reviewReports = await projectedReviews
                    .GroupBy(r => r.ReviewDate.Year)
                    .Select(group => new ReviewReportDTO
                    {
                        GroupingKey = group.Key.ToString(),
                        GoodReviews = group.Count(r => r.Rating >= 3),
                        BadReviews = group.Count(r => r.Rating <= 2)
                    })
                    .ToListAsync();
            }
            else if (timeFrameLower == "monthly")
            {
                reviewReports = await projectedReviews
                    .GroupBy(r => new { r.ReviewDate.Year, r.ReviewDate.Month })
                    .Select(group => new ReviewReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}",
                        GoodReviews = group.Count(r => r.Rating >= 3),
                        BadReviews = group.Count(r => r.Rating <= 2)
                    })
                    .ToListAsync();
            }
            else // Assuming "daily" or any other TimeFrame
            {
                reviewReports = await projectedReviews
                    .GroupBy(r => new { r.ReviewDate.Year, r.ReviewDate.Month, r.ReviewDate.Day })
                    .Select(group => new ReviewReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}-{group.Key.Day:00}",
                        GoodReviews = group.Count(r => r.Rating >= 3),
                        BadReviews = group.Count(r => r.Rating <= 2)
                    })
                    .ToListAsync();
            }

            return reviewReports;
        }

        public async Task<List<UserStatusReportDTO>> GetUserStatusReport(TimeFrame? timeFrame = null)
        {
            DateTime? startDate = null;
            DateTime endDate = DateTime.Now;

            if (timeFrame.HasValue)
            {
                var (sDate, eDate) = GetDateRange(timeFrame.Value);
                startDate = sDate;
                endDate = eDate;
            }

            var query = _context.UserAccounts.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= startDate.Value && u.CreatedAt <= endDate);
            }

            var userStatusReport = await query
                .GroupBy(u => u.Status)
                .Select(group => new UserStatusReportDTO
                {
                    Status = group.Key,
                    TotalUsers = group.Count()
                })
                .ToListAsync();

            return userStatusReport;
        }

        public async Task<List<ProductCategoryReportDTO>> GetProductCategoryReport(TimeFrame? timeFrame = null)
        {
            DateTime? startDate = null;
            DateTime endDate = DateTime.Now;

            if (timeFrame.HasValue)
            {
                var (sDate, eDate) = GetDateRange(timeFrame.Value);
                startDate = sDate;
                endDate = eDate;
            }

            var query = _context.Products.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= startDate.Value && p.CreatedAt <= endDate);
            }

            var productCategoryReport = await query
                .GroupBy(p => p.CategoryId)
                .Select(group => new ProductCategoryReportDTO
                {
                    CategoryName = _context.ProductCategories
                        .Where(c => c.Id == group.Key)
                        .Select(c => c.ProductCategoryName)
                        .FirstOrDefault(),
                    TotalProducts = group.Count(),
                    AveragePrice = group.Average(p => p.Price)
                })
                .ToListAsync();

            return productCategoryReport;
        }

        public async Task<List<RevenueReportDTO>> GetRevenueReport(TimeFrame timeFrame)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(timeFrame);
                var timeFrameLower = timeFrame.ToString().ToLower();

                var transactionsQuery = _context.WalletTransactions
                    .AsNoTracking() 
                    .Where(txn =>
                        txn.WalletId == 1 &&
                        txn.CreatedAt >= startDate &&
                        txn.CreatedAt <= endDate &&
                        (txn.TransactionType.ToLower() == "revenue" || txn.TransactionType.ToLower() == "cost")
                    );

                List<RevenueReportDTO> revenueReports = new List<RevenueReportDTO>();

                if (timeFrameLower == "annually" || timeFrameLower == "fiveyears")
                {
                    revenueReports = await transactionsQuery
                        .GroupBy(txn => txn.CreatedAt.Year)
                        .Select(group => new RevenueReportDTO
                        {
                            GroupingKey = group.Key.ToString(),
                            TotalRevenue = group.Sum(txn => txn.Amount) // Net Income
                        })
                        .ToListAsync();
                }
                else if (timeFrameLower == "monthly")
                {
                    revenueReports = await transactionsQuery
                        .GroupBy(txn => new { txn.CreatedAt.Year, txn.CreatedAt.Month })
                        .Select(group => new RevenueReportDTO
                        {
                            GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}",
                            TotalRevenue = group.Sum(txn => txn.Amount) // Net Income
                        })
                        .ToListAsync();
                }
                else 
                {
                    revenueReports = await transactionsQuery
                        .GroupBy(txn => new { txn.CreatedAt.Year, txn.CreatedAt.Month, txn.CreatedAt.Day })
                        .Select(group => new RevenueReportDTO
                        {
                            GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}-{group.Key.Day:00}",
                            TotalRevenue = group.Sum(txn => txn.Amount) // Net Income
                        })
                        .ToListAsync();
                }

                return revenueReports;
            }
            catch (Exception ex)
            {
                throw; 
            }
        }

    }
}