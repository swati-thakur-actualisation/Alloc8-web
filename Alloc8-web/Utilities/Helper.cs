using Alloc8_web.ViewModels.Dashboard;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using SendGrid.Helpers.Mail;
using System.Drawing;
using System.Text.Json;
using Azure.API.Models;
using Alloc8.ef.Entities.Dashboard;

namespace Alloc8_web.Utilities
{
    public class Tile
    {
        public string? name { get; set; }
        public string? title { get; set; }
        public string? desc { get; set; }
        public int? organisation { get; set; }
        public bool show { get; set; } = false;
        public List<Tile>? subTiles {  get; set; }
    }
    public class SystemTimezone
    {
        public string TimeZoneId { set; get; }
        public string TimeZoneDisplayName { set; get; }

    }
    public static class Helper
    {
        public static List<string> permissionsList = new List<string> { "dashboard","users", "chatgpt", "apps"};
        public static string HashPassword(string plainPassword)
        {
            var passwordHasher = new PasswordHasher<object>(); // You can replace 'object' with your user class if applicable
            var hashedPassword = passwordHasher.HashPassword(null, plainPassword);

            return hashedPassword;
        }

        public static async Task<string> getLatestLog(string url, string accessToken)
        {
            try
            {
                Azure.API.Handler azureWebJobs = new Azure.API.Handler();
                if (accessToken != null && url !=null)
                {
                    var logsData = await azureWebJobs.getLogsApi(url, accessToken);
                    return logsData;
                }
            }
            catch(Exception ex)
            {

            }
            return null;
        }
        public static async Task<List<WebJob>> getAzureJobs(string accessToken)
        {
            try
            {
                Azure.API.Handler azureWebJobs = new Azure.API.Handler();
                if (accessToken != null)
                {
                    var webJobsData = await azureWebJobs.getWebJobs(accessToken);
                    if (webJobsData != null)
                    {
                        return webJobsData.value;
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return null;
        }
        public static List<UserPermission> getPermissions()
        {
            var permissions = new List<UserPermission>();
            foreach(string permission in permissionsList)
            {
                permissions.Add(new UserPermission
                {
                    name = permission,
                    view = false,
                    edit = false,
                    remove = false,
                });
            }
            return permissions;
            
        }
        public static Color parseColorCode(string colorCode)
        {
            if (colorCode.StartsWith("#") && colorCode.Length == 7)
            {
                try
                {
                    // Parse the color code and create a Color object
                    return ColorTranslator.FromHtml(colorCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing color code: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid color code format.");
            }

            // Return a default color if parsing fails
            return Color.Black;
        }
        public static List<SystemTimezone> Timezones()
        {
            var TimezoneList = TimeZoneInfo.GetSystemTimeZones().ToList()
                .Select(x => new SystemTimezone { TimeZoneId = x.Id, TimeZoneDisplayName = x.DisplayName });
            var a = TimeZoneInfo.GetSystemTimeZones().ToList();
            return TimezoneList.ToList();
        }
        public static string addTransparency(Color color, int alpha)
        {
            // Ensure that alpha is within the valid range (0 to 255)
            int newAlpha = Math.Max(0, Math.Min(255, alpha));

            // Create a new Color with the original RGB components and the modified alpha value
            return ColorTranslator.ToHtml(Color.FromArgb(newAlpha, color.R, color.G, color.B));
        }
        public static List<Tile> getUserDashboardTiles(string userTilesString,int? organisation)
        {
            List<string> tilesArray = userTilesString.Split(',').ToList();
            List<Tile> tiles = new List<Tile>();
            List<Tile> algorithmSubtiles = new List<Tile>();
            tiles.Add(new Tile { name = "StorageTile", title = "DataBase Storage", organisation = organisation, show = tilesArray.Any(x=>x == "StorageTile") });
            if(organisation == 9)
            {
                tiles.Add(new Tile { name = "TraffioPredictionsTile", title = "Traffic Control Predictions", organisation = organisation, show = tilesArray.Any(x => x == "TraffioPredictionsTile") });
            }
            tiles.Add(new Tile { name = "CoachingTile", title = "Coaching Tile", organisation = organisation, show = tilesArray.Any(x => x == "CoachingTile") });
            tiles.Add(new Tile { name = "RecentLoginsTile", title = "Recent Logins", organisation = organisation, show = tilesArray.Any(x => x == "RecentLoginsTile") });
            tiles.Add(new Tile { name = "DataLogsTile", title = "Database Storage Logs", organisation = organisation, show = tilesArray.Any(x => x == "DataLogsTile") });
            tiles.Add(new Tile { name = "ServicesTile", title = "Azure Service Status", organisation = organisation, show = tilesArray.Any(x => x == "ServicesTile") });
            tiles.Add(new Tile { name = "TableMetricsTile", title = "Database Table Record Count", organisation = organisation, show = tilesArray.Any(x => x == "TableMetricsTile") });
            tiles.Add(new Tile { name = "CoachingComplientsStatusTile", title = "Coaching Compliance Statistics", organisation = organisation, show = tilesArray.Any(x => x == "CoachingComplientsStatusTile") });
            tiles.Add(new Tile { name = "UpcomingOrdersTile", title = "Upcoming Orders", organisation = organisation, show = tilesArray.Any(x => x == "UpcomingOrdersTile") });
            tiles.Add(new Tile { name = "ExceptionsTile", title = "Action Required", organisation = organisation, show = tilesArray.Any(x => x == "ExceptionsTile") });
            tiles.Add(new Tile { name = "RevenuePredictionsPerMonthsTile", title = "Revenue Predictions", organisation = organisation, show = tilesArray.Any(x => x == "RevenuePredictionsPerMonthsTile") });
            tiles.Add(new Tile { name = "TravelTimeSavedAndExpensesTile", title = "Travel Time and Expenses Saved", organisation = organisation, show = tilesArray.Any(x => x == "TravelTimeSavedAndExpensesTile") });
            algorithmSubtiles.Add(new Tile { name = "AnomalyDetection", title = "Anomaly Detection", organisation = organisation, show = tilesArray.Any(x => x == "AnomalyDetection") });
            algorithmSubtiles.Add(new Tile { name = "AutomatedCoaching", title = "Automated Coaching", organisation = organisation, show = tilesArray.Any(x => x == "AutomatedCoaching") });
            algorithmSubtiles.Add(new Tile { name = "AutomatedQuoting", title = "Automated Quoting", organisation = organisation, show = tilesArray.Any(x => x == "AutomatedQuoting") });
            algorithmSubtiles.Add(new Tile { name = "AutomatedScheduling", title = "Automated Scheduling", organisation = organisation, show = tilesArray.Any(x => x == "AutomatedScheduling") });
            algorithmSubtiles.Add(new Tile { name = "CustomChatbot", title = "Custom Chatbot", organisation = organisation, show = tilesArray.Any(x => x == "CustomChatbot") });
            algorithmSubtiles.Add(new Tile { name = "CustomAutomation", title = "Custom Automation", organisation = organisation, show = tilesArray.Any(x => x == "CustomAutomation") });
            algorithmSubtiles.Add(new Tile { name = "CancellationPrediction", title = "Cancellation Prediction", organisation = organisation, show = tilesArray.Any(x => x == "CancellationPrediction") });
            algorithmSubtiles.Add(new Tile { name = "GuidedFreightProcurement", title = "Guided Freight Procurement", organisation = organisation, show = tilesArray.Any(x => x == "GuidedFreightProcurement") });
            algorithmSubtiles.Add(new Tile { name = "LeadCapture", title = "Lead Capture", organisation = organisation, show = tilesArray.Any(x => x == "LeadCapture") });
            tiles.Add(new Tile { name = "Upgrades", title = "Upgrades", organisation = organisation, show = true, subTiles = algorithmSubtiles });
            return tiles;
        }
        public static string convertSecondsToString(double? valueInSeconds)
        {
            // Convert the double seconds into an integer value
            int totalSeconds = (int)valueInSeconds;

            // Calculate hours
            int hours = totalSeconds / 3600;
            // Calculate remaining seconds after calculating hours
            int remainingSeconds = totalSeconds % 3600;
            // Calculate minutes
            int minutes = remainingSeconds / 60;

            // Format the result
            string result = $"{hours}h {minutes}m";

            return result;
        }
        // replicate convertSecondsToString function but we need only total hours with minutes

        public static string convertSecondsToStringJira(double? valueInSeconds)
        {
            if (valueInSeconds == null || valueInSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(valueInSeconds), "Seconds cannot be null or negative.");

            double totalHours = (double)valueInSeconds / 3600; // Convert seconds to hours

            int remainingHours = (int)totalHours % 24; // Calculate remaining hours

            string result = "";

            if (totalHours > 0)
                result += $"{(int)totalHours}h ";

            if (remainingHours > 0)
                result += $"{remainingHours}h ";

            return result.Trim();
        }
        public static List<Tile> getDashboardTiles(int? organisation)
        {
            List<Tile> tiles = new List<Tile>();
            List<Tile> algorithmSubtiles = new List<Tile>();

            tiles.Add(new Tile { name = "StorageTile", title = "DataBase Storage", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "TraffioPredictionsTile", title = "Traffic Control Predictions", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "CoachingTile", title = "Coaching Tile", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "RecentLoginsTile", title = "Recent Logins", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "DataLogsTile", title = "Database Storage Logs", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "ServicesTile", title = "Azure Service Status", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "TableMetricsTile", title = "Database Table Record Counts", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "UpcomingOrdersTile", title = "Upcoming Orders", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "CoachingComplientsStatusTile", title = "Compliance Score", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "RevenuePredictionsPerMonthsTile", title = "Revenue Predictions", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "TravelTimeSavedAndExpensesTile", title = "Travel Time and Expenses Saved", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "ExceptionsTile", title = "Action Required", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "AnomalyDetection", title = "Anomaly Detection", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "AutomatedCoaching", title = "Automated Coaching", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "AutomatedQuoting", title = "Automated Quoting", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "AutomatedScheduling", title = "Automated Scheduling", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "CustomChatbot", title = "Custom Chatbot", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "CustomAutomation", title = "Custom Automation", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "CancellationPrediction", title = "Cancellation Prediction", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "GuidedFreightProcurement", title = "Guided Freight Procurement", organisation = organisation, show = true });
            algorithmSubtiles.Add(new Tile { name = "LeadCapture", title = "Lead Capture", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "Upgrades", title = "Upgrades", organisation = organisation, show = true, subTiles = algorithmSubtiles });

            // algorithh tile subtiles

            return tiles;
        }
        public static List<Tile> getAdminDashboardTiles(int? organisation)
        {
            List<Tile> tiles = new List<Tile>();
            List<Tile> algorithmSubtiles = new List<Tile>();
            // commented till next update

            tiles.Add(new Tile { name = "StorageTile", title = "DataBase Storage", organisation = organisation, show = true });
            //if (organisation == 9)
            //{
            //    tiles.Add(new Tile { name = "TraffioPredictionsTile", title = "Traffic Control Predictions", organisation = organisation, show = true });
            //}
            tiles.Add(new Tile { name = "CoachingTile", title = "Coaching Tile", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "RecentLoginsTile", title = "Recent Logins", organisation = organisation, show = true });    
            tiles.Add(new Tile { name = "DataLogsTile", title = "Database Storage Logs", organisation = organisation, show = false });
            tiles.Add(new Tile { name = "ServicesTile", title = "Azure Service Status", organisation = organisation, show = true });
            tiles.Add(new Tile { name = "TableMetricsTile", title = "Database Table Record Count", organisation = organisation, show = false });
            //tiles.Add(new Tile { name = "UpcomingOrdersTile", title = "Upcoming Orders", organisation = organisation, show = false });
            //tiles.Add(new Tile { name = "CoachingComplientsStatusTile", title = "Compliance Score", organisation = organisation, show = false });
            //tiles.Add(new Tile { name = "ExceptionsTile", title = "Action Required", organisation = organisation, show = false });
            //tiles.Add(new Tile { name = "RevenuePredictionsPerMonthsTile", title = "Revenue Predictions", organisation = organisation, show = true });
            //tiles.Add(new Tile { name = "TravelTimeSavedAndExpensesTile", title = "Travel Time and Expenses Saved", organisation = organisation, show = true });
            //algorithmSubtiles.Add(new Tile { name = "AutomatedCoaching", title = "Automated Coaching", organisation = organisation, show = false });
            //algorithmSubtiles.Add(new Tile { name = "AutomatedQuoting", title = "Automated Quoting", organisation = organisation, show = false });
            //algorithmSubtiles.Add(new Tile { name = "AutomatedScheduling", title = "Automated Scheduling", organisation = organisation, show = false });
            //algorithmSubtiles.Add(new Tile { name = "CustomChatbot", title = "Custom Chatbot", organisation = organisation, show = false });
            //algorithmSubtiles.Add(new Tile { name = "CustomAutomation", title = "Custom Automation", organisation = organisation, show = false });
            //algorithmSubtiles.Add(new Tile { name = "CancellationPrediction", title = "Cancellation Prediction", organisation = organisation, show = false });
            //algorithmSubtiles.Add(new Tile { name = "AnomalyDetection", title = "Anomaly Detection", organisation = organisation, show = false });
            //algorithmSubtiles.Add(new Tile { name = "GuidedFreightProcurement ", title = "Guided Freight Procurement", organisation = organisation, show = false });
            //algorithmSubtiles.Add(new Tile { name = "LeadCapture ", title = "Lead Capture", organisation = organisation, show = false });
            //tiles.Add(new Tile { name = "Upgrades", title = "Upgrades", organisation = organisation, show = false, subTiles = algorithmSubtiles });

            return tiles;
        }
        public static string getCurrentYear()
        {
            return DateTime.UtcNow.ToString("yyyy");
        }

        public static List<LeadCaptureViewModel> getAutomatedLeadCaptureAlgorithmViewModelTileData()
        {
            
            List<LeadCaptureViewModel> tileData = new List<LeadCaptureViewModel>();
            tileData.Add(new LeadCaptureViewModel { companyCode = "14D", companyName = "1414 DEGREES LIMITED", industry = "Capital Goods", listDate = new DateTime(2018, 9, 12), marketCap = "$14.29M$14.29M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "29M", companyName = "29METALS LIMITED", industry = "Materials", listDate = new DateTime(2021, 7, 2), marketCap = "$490.92M$490.92M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "T3D*", companyName = "333D LIMITED", industry = "Commercial & Professional Services", listDate = new DateTime(2006, 12, 27), marketCap = "$3.10M$3.10M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "TGP*", companyName = "360 CAPITAL GROUP", industry = "Financial Services", listDate = new DateTime(2005, 7, 26), marketCap = "$133.33M$133.33M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "TCF", companyName = "360 CAPITAL MORTGAGE REIT", industry = "Not Applic", listDate = new DateTime(2006, 10, 17), marketCap = "$21.40M$21.40M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "TOT*", companyName = "360 CAPITAL REIT", industry = "Equity Real Estate Investment Trusts (REITs)", listDate = new DateTime(2015, 4, 22), marketCap = "$79.44M$79.44M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "TDO", companyName = "3D ENERGI LIMITED", industry = "Energy", listDate = new DateTime(2007, 5, 22), marketCap = "$14.59M$14.59M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "3PL", companyName = "3P LEARNING LIMITED..", industry = "Consumer Services", listDate = new DateTime(2014, 7, 9), marketCap = "$358.55M$358.55M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "4DX", companyName = "4DMEDICAL LIMITED", industry = "Health Care Equipment & Services", listDate = new DateTime(2020, 8, 7), marketCap = "$277.52M$277.52M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "4DS", companyName = "4DS MEMORY LIMITED", industry = "Semiconductors & Semiconductor Equipment", listDate = new DateTime(2010, 12, 9), marketCap = "$134.67M$134.67M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "5EA", companyName = "5E ADVANCED MATERIALS INC.", industry = "Materials", listDate = new DateTime(2022, 3, 1), marketCap = "$97.32M$97.32M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "5GN*", companyName = "5G NETWORKS LIMITED.", industry = "Software & Services", listDate = new DateTime(1999, 12, 14), marketCap = "$77.06M$77.06M" });
            tileData.Add(new LeadCaptureViewModel { companyCode = "88E", companyName = "88 ENERGY LIMITED", industry = "Energy", listDate = new DateTime(2000, 1, 20), marketCap = "$98.56M" });
            return tileData;
        }
        public static string getVersion()
        {
            return "1.0.0";
        }
        public static async Task<bool> webJobStart(string accessToken, string webJobName)
        {
            try
            {
                Azure.API.Handler handler = new Azure.API.Handler();
                bool response = await handler.runWebJobs(webJobName, accessToken);
                return response;
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        public static List<(DateTime, DateTime)> getWeekStartAndEndDates(int numberOfWeeks)
        {
            List<(DateTime, DateTime)> weeks = new List<(DateTime, DateTime)>();

            // Get the current date
            DateTime currentDate = DateTime.Today;

            // Move to the previous Monday
            DateTime currentWeekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);

            // Loop through the number of weeks
            for (int i = 0; i < numberOfWeeks; i++)
            {
                // Find the Monday of the current week
                DateTime weekStart = currentWeekStart.AddDays(-7 * i);

                // Find the Friday of the current week
                DateTime weekEnd = weekStart.AddDays(4).AddHours(23).AddMinutes(59).AddSeconds(59); // Assuming Friday is the end of the week

                // Add the start and end dates to the list
                weeks.Add((weekStart, weekEnd));
            }

            return weeks;
        }
        public static List<(DateTime, DateTime)> getMonthStartAndEndDates(int noOfmonts)
        {
            List<(DateTime, DateTime)> months = new List<(DateTime, DateTime)>();

            // Get the current date
            DateTime currentDate = DateTime.Today;

            // Loop through the number of months
            for (int i = 0; i < noOfmonts; i++)
            {
                // Find the first day of the current month
                DateTime monthStart = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-i);

                // Find the last day of the current month
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

                // Add the start and end dates to the list
                months.Add((monthStart, monthEnd));
            }

            return months;
        }
    }
}
