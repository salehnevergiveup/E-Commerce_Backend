using System;
using System.Security.Claims;
using PototoTrade.DTO.ReportDTO;
using PototoTrade.Enums;
using PototoTrade.Repositories;
using PototoTrade.Repository.Report;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Service.Report
{
    public class ReportsService
    {
        private readonly ReportRepository _reportRepository;

        public ReportsService(ReportRepository reportRepository, ILogger<ReportsService> logger)
        {
            _reportRepository = reportRepository;
        }

        private bool IsUserAdmin(ClaimsPrincipal userClaims)
        {
            if (userClaims == null)
                return false;

            return userClaims.IsInRole(UserRolesEnum.Admin.ToString()) ||
                   userClaims.IsInRole(UserRolesEnum.SuperAdmin.ToString());
        }

        private bool TryParseTimeFrame(string timeFrameStr, out TimeFrame timeFrame)
        {
            return Enum.TryParse<TimeFrame>(timeFrameStr, ignoreCase: true, out timeFrame);
        }

        public async Task<ResponseModel<List<SalesReportDTO>>> GetSalesReport(ClaimsPrincipal userClaims, ReportRequestDTO request)
        {
            var response = new ResponseModel<List<SalesReportDTO>>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve sales report."
            };

            try
            {
                if (!IsUserAdmin(userClaims))
                {
                    response.Message = "Unauthorized access. Admin privileges required.";
                    return response;
                }

                if (request == null)
                {
                    response.Message = "Invalid report request.";
                    return response;
                }

                if (!TryParseTimeFrame(request.TimeFrame, out TimeFrame timeFrame))
                {
                    response.Message = $"Invalid TimeFrame value: {request.TimeFrame}. Valid values are: Weekly, Monthly, Annually, FiveYears.";
                    return response;
                }

                var reportData = await _reportRepository.GetSalesReport(timeFrame);

                if (reportData == null || reportData.Count == 0)
                {
                    response.Message = "No sales data available for the selected time frame.";
                    return response;
                }

                response.Data = reportData;
                response.Success = true;
                response.Message = "Sales report retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred while retrieving the sales report.";
            }

            return response;
        }

        public async Task<ResponseModel<List<TransactionReportDTO>>> GetTransactionReport(ClaimsPrincipal userClaims, ReportRequestDTO request)
        {
            var response = new ResponseModel<List<TransactionReportDTO>>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve transaction report."
            };

            try
            {
                // if (!IsUserAdmin(userClaims))
                // {
                //     response.Message = "Unauthorized access. Admin privileges required.";
                //     return response;
                // }

                if (request == null)
                {
                    response.Message = "Invalid report request.";
                    return response;
                }

                if (!TryParseTimeFrame(request.TimeFrame, out TimeFrame timeFrame))
                {
                    response.Message = $"Invalid TimeFrame value: {request.TimeFrame}. Valid values are: Weekly, Monthly, Annually.";
                    return response;
                }

                var reportData = await _reportRepository.GetTransactionReport(timeFrame);

                if (reportData == null || reportData.Count == 0)
                {
                    response.Message = "No transaction data available for the selected time frame.";
                    return response;
                }

                response.Data = reportData;
                response.Success = true;
                response.Message = "Transaction report retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred while retrieving the transaction report.";
            }

            return response;
        }
        public async Task<ResponseModel<List<ReviewReportDTO>>> GetReviewReport(ClaimsPrincipal userClaims, ReportRequestDTO request)
        {
            var response = new ResponseModel<List<ReviewReportDTO>>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve review report."
            };

            try
            {
                // if (!IsUserAdmin(userClaims))
                // {
                //     response.Message = "Unauthorized access. Admin privileges required.";
                //     return response;
                // }

                if (request == null)
                {
                    response.Message = "Invalid report request.";
                    return response;
                }

                if (!TryParseTimeFrame(request.TimeFrame, out TimeFrame timeFrame))
                {
                    response.Message = $"Invalid TimeFrame value: {request.TimeFrame}. Valid values are: Daily, Monthly, Annually, FiveYears.";
                    return response;
                }

                var reportData = await _reportRepository.GetReviewReport(timeFrame);

                if (reportData == null || reportData.Count == 0)
                {
                    response.Message = "No review data available for the selected time frame.";
                    return response;
                }

                response.Data = reportData;
                response.Success = true;
                response.Message = "Review report retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred while retrieving the review report.";
            }

            return response;
        }

        public async Task<ResponseModel<List<UserStatusReportDTO>>> GetUserStatusReport(ClaimsPrincipal userClaims, ReportRequestDTO request)
        {
            var response = new ResponseModel<List<UserStatusReportDTO>>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve user status report."
            };

            try
            {
                if (!IsUserAdmin(userClaims))
                {
                    response.Message = "Unauthorized access. Admin privileges required.";
                    return response;
                }

                TimeFrame? timeFrame = null;
                if (!string.IsNullOrWhiteSpace(request?.TimeFrame))
                {
                    if (TryParseTimeFrame(request.TimeFrame, out TimeFrame parsedTimeFrame))
                    {
                        timeFrame = parsedTimeFrame;
                    }
                    else
                    {
                        response.Message = $"Invalid TimeFrame value: {request.TimeFrame}. Valid values are: Weekly, Monthly, Annually, FiveYears.";
                        return response;
                    }
                }

                var reportData = await _reportRepository.GetUserStatusReport(timeFrame);

                if (reportData == null || reportData.Count == 0)
                {
                    response.Message = "No user status data available for the selected time frame.";
                    return response;
                }

                response.Data = reportData;
                response.Success = true;
                response.Message = "User status report retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred while retrieving the user status report.";
            }

            return response;
        }

        public async Task<ResponseModel<List<ProductCategoryReportDTO>>> GetProductCategoryReport(ClaimsPrincipal userClaims, ReportRequestDTO request)
        {
            var response = new ResponseModel<List<ProductCategoryReportDTO>>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve product category report."
            };

            try
            {
                if (!IsUserAdmin(userClaims))
                {
                    response.Message = "Unauthorized access. Admin privileges required.";
                    return response;
                }

                TimeFrame? timeFrame = null;
                if (!string.IsNullOrWhiteSpace(request?.TimeFrame))
                {
                    if (TryParseTimeFrame(request.TimeFrame, out TimeFrame parsedTimeFrame))
                    {
                        timeFrame = parsedTimeFrame;
                    }
                    else
                    {
                        response.Message = $"Invalid TimeFrame value: {request.TimeFrame}. Valid values are: Weekly, Monthly, Annually, FiveYears.";
                        return response;
                    }
                }

                var reportData = await _reportRepository.GetProductCategoryReport(timeFrame);

                if (reportData == null || reportData.Count == 0)
                {
                    response.Message = "No product category data available for the selected time frame.";
                    return response;
                }

                response.Data = reportData;
                response.Success = true;
                response.Message = "Product category report retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred while retrieving the product category report.";
            }

            return response;
        }

        public async Task<ResponseModel<List<RevenueReportDTO>>> GetRevenueReport(ClaimsPrincipal userClaims, ReportRequestDTO request)
        {
            var response = new ResponseModel<List<RevenueReportDTO>>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve revenue report."
            };

            try
            {
                // if (!IsUserAdmin(userClaims))
                // {
                //     response.Message = "Unauthorized access. Admin privileges required.";
                //     return response;
                // }

                if (request == null)
                {
                    response.Message = "Invalid report request.";
                    return response;
                }

                if (!TryParseTimeFrame(request.TimeFrame, out TimeFrame timeFrame))
                {
                    response.Message = $"Invalid TimeFrame value: {request.TimeFrame}. Valid values are: Weekly, Monthly, Annually, FiveYears.";
                    return response;
                }

                var reportData = await _reportRepository.GetRevenueReport(timeFrame);

                if (reportData == null || reportData.Count == 0)
                {
                    response.Message = "No revenue data available for the selected time frame.";
                    return response;
                }

                response.Data = reportData;
                response.Success = true;
                response.Message = "Revenue report retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred while retrieving the revenue report.";
            }

            return response;
        }
    }
}
