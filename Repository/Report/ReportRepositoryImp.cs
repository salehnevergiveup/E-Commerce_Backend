using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.DTO.ReportDTO;
using PototoTrade.Models.Product;
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
            DateTime endDate = DateTime.Now.Date; // Today's date without time component
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

        public async Task<List<RegistrationReportDTO>> GetRegistrationReport(TimeFrame timeFrame)
        {

            var (startDate, endDate) = GetDateRange(timeFrame);
            var timeFrameLower = timeFrame.ToString().ToLower();

            var baseQuery = _context.UserActivitiesLogs
                .Where(log =>
                    log.ActivityDate >= startDate &&
                    log.ActivityDate <= endDate &&
                    log.ActivityName.ToLower() == "registration" // Adjust based on collation
                );

            List<RegistrationReportDTO> registrationReports = new List<RegistrationReportDTO>();

            if (timeFrameLower == "annually" || timeFrameLower == "fiveyears")
            {
                registrationReports = await baseQuery
                    .GroupBy(log => log.ActivityDate.Year)
                    .Select(group => new RegistrationReportDTO
                    {
                        GroupingKey = group.Key.ToString(),
                        TotalRegistrations = group.Count()
                    })
                    .ToListAsync();
            }
            else if (timeFrameLower == "monthly")
            {
                registrationReports = await baseQuery
                    .GroupBy(log => new { log.ActivityDate.Year, log.ActivityDate.Month })
                    .Select(group => new RegistrationReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}",
                        TotalRegistrations = group.Count()
                    })
                    .ToListAsync();
            }
            else 
            {
                registrationReports = await baseQuery
                    .GroupBy(log => new { log.ActivityDate.Year, log.ActivityDate.Month, log.ActivityDate.Day })
                    .Select(group => new RegistrationReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}-{group.Key.Day:00}",
                        TotalRegistrations = group.Count()
                    })
                    .ToListAsync();
            }

            return registrationReports;

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
            var (startDate, endDate) = GetDateRange(timeFrame);
            var timeFrameLower = timeFrame.ToString().ToLower();

            var baseQuery = _context.PurchaseOrders
                .Where(order =>
                    order.OrderCreatedAt >= startDate &&
                    order.OrderCreatedAt <= endDate &&
                    order.Status.ToLower() == "completed"
                );

            var joinedQuery = baseQuery
                .Join(
                    _context.BuyerItems,
                    order => order.Id,
                    item => item.OrderId,
                    (order, item) => new { order, item }
                )
                .Where(orderItem =>
                    orderItem.item.ValidRefundDate.HasValue &&
                    orderItem.item.ValidRefundDate.Value.ToDateTime(TimeOnly.MinValue) < DateTime.Now
                )
                .Join(
                    _context.Products,
                    orderItem => orderItem.item.ProductId,
                    product => product.Id,
                    (orderItem, product) => new { orderItem.order, product }
                )
                .Join(
                    _context.ProductCategories,
                    orderProduct => orderProduct.product.CategoryId,
                    category => category.Id,
                    (orderProduct, category) => new
                    {
                        orderProduct.order,
                        Revenue = orderProduct.order.TotalAmount * ((decimal)category.ChargeRate / 100m)
                    }
                );

            List<RevenueReportDTO> revenueReports = new List<RevenueReportDTO>();

            if (timeFrameLower == "annually" || timeFrameLower == "fiveyears")
            {
                revenueReports = await joinedQuery
                    .GroupBy(o => o.order.OrderCreatedAt.Year)
                    .Select(group => new RevenueReportDTO
                    {
                        GroupingKey = group.Key.ToString(),
                        TotalRevenue = group.Sum(o => o.Revenue)
                    })
                    .ToListAsync();
            }
            else if (timeFrameLower == "monthly")
            {
                revenueReports = await joinedQuery
                    .GroupBy(o => new { o.order.OrderCreatedAt.Year, o.order.OrderCreatedAt.Month })
                    .Select(group => new RevenueReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}",
                        TotalRevenue = group.Sum(o => o.Revenue)
                    })
                    .ToListAsync();
            }
            else
            {
                revenueReports = await joinedQuery
                    .GroupBy(o => new { o.order.OrderCreatedAt.Year, o.order.OrderCreatedAt.Month, o.order.OrderCreatedAt.Day })
                    .Select(group => new RevenueReportDTO
                    {
                        GroupingKey = $"{group.Key.Year}-{group.Key.Month:00}-{group.Key.Day:00}",
                        TotalRevenue = group.Sum(o => o.Revenue)
                    })
                    .ToListAsync();
            }

            return revenueReports;
        }

    }
}