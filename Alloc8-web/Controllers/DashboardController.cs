using Alloc8_web.Services.Admin;
using Alloc8_web.Utilities;
using Alloc8_web.ViewModels.Dashboard;
using Alloc8_web.ViewModels.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Alloc8.ef;
using System.Runtime.InteropServices;
using Azure.API.Models;
using Alloc8.ef.Entities.Dashboard;
using System.Text.Json;
using Newtonsoft.Json;

namespace Alloc8_web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly Alloc8DbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private UserManager<ApplicationUser> _userManager;

        public DashboardController(Alloc8DbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
             
            )
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
           
           
        }
        
        public async Task<IActionResult> getStorageData(int organisation)
        {
            switch (organisation)
            {
                case 0:
                    return Ok(new StorageViewModel());              
                default:
                    return Ok(new StorageViewModel());

            }
        }

        public async Task<IActionResult> getRecentLogins(int organisation)
        {
            List<RecentLoginsTileViewModel>? recentLogins = new List<RecentLoginsTileViewModel>();
            var today = DateTime.UtcNow.Date;

            if (User.IsInRole(WebSiteRoles.Website_Admin))
            {
                var query = _context.users.Where(u => u.loged_at.HasValue && u.loged_at.Value.Date == today);

                if (organisation > 0)
                {
                    query = query.Where(x => x.orginationId == organisation);
                }

                recentLogins = await query.Select(x => new RecentLoginsTileViewModel
                {
                    email = x.Email,
                    time = x.loged_at
                }).ToListAsync();
            }
            else if (User.IsInRole(WebSiteRoles.Website_Manager))
            {
                recentLogins = await _context.users.Include(i => i.users)
                    .Where(x => x.Email == User.Identity.Name)
                    .SelectMany(x => x.users)
                    .Select(x => new RecentLoginsTileViewModel
                    {
                        email = x.user.Email,
                        time = x.user.loged_at
                    }).ToListAsync();
            }

            return PartialView("Views/Dashboard/RecentLoginsTile/Content.cshtml", recentLogins);
        }
        public IActionResult getPredictedDrivers(int organisation)
        {
            List<TCPredictionsViewModel> model = new List<TCPredictionsViewModel>();
            model.Add(new TCPredictionsViewModel { jobNumber = "#1901", predictedDriver = "Stacy", actualDriver = "Stacy" });
            model.Add(new TCPredictionsViewModel { jobNumber = "#1902", predictedDriver = "Tom", actualDriver = "Tom" });
            model.Add(new TCPredictionsViewModel { jobNumber = "#1903", predictedDriver = "Prabhas", actualDriver = "Stacy" });
            model.Add(new TCPredictionsViewModel { jobNumber = "#1904", predictedDriver = "Lisa", actualDriver = "Lisa" });
            model.Add(new TCPredictionsViewModel { jobNumber = "#1905", predictedDriver = "Michal", actualDriver = "Michal" });
            return PartialView("Views/Dashboard/TraffioPredictionsTile/Content.cshtml", model);
        }
        public IActionResult getCoachingComplientsStatusTileData(int organisation)
        {
            List<CoachingComplientsStatusViewModel> model = new List<CoachingComplientsStatusViewModel>();
            model.Add(new CoachingComplientsStatusViewModel { coachName = "Alice Johnson", totalSessions = 50, totalComplaints = 2 });
            model.Add(new CoachingComplientsStatusViewModel { coachName = "Bob Anderson", totalSessions = 40, totalComplaints = 1 });
            model.Add(new CoachingComplientsStatusViewModel { coachName = "Charlie Brown", totalSessions = 60, totalComplaints = 4 });
            model.Add(new CoachingComplientsStatusViewModel { coachName = "David Davis", totalSessions = 35, totalComplaints = 1 });
            model.Add(new CoachingComplientsStatusViewModel { coachName = "Eva Evans", totalSessions = 48, totalComplaints = 2 });
            return PartialView("Views/Dashboard/CoachingComplientsStatusTile/Content.cshtml", model);
        }
        public IActionResult getUpcomingOrdersData(int organisation)
        {
            List<UpcomingOrdersViewModel> model = new List<UpcomingOrdersViewModel>();

            model.Add(new UpcomingOrdersViewModel { orderId = 1, productName = "Widget A", orderDate = DateTime.Now.AddDays(3) });
            model.Add(new UpcomingOrdersViewModel { orderId = 2, productName = "Gadget B", orderDate = DateTime.Now.AddDays(5) });
            model.Add(new UpcomingOrdersViewModel { orderId = 3, productName = "Tool C", orderDate = DateTime.Now.AddDays(7) });
            model.Add(new UpcomingOrdersViewModel { orderId = 4, productName = "Device D", orderDate = DateTime.Now.AddDays(10) });
            model.Add(new UpcomingOrdersViewModel { orderId = 5, productName = "Accessory E", orderDate = DateTime.Now.AddDays(12) });
            return PartialView("Views/Dashboard/UpcomingOrdersTile/Content.cshtml", model);
        }
        public IActionResult getExceptionsTile(int organisation)
        {
            List<ExceptionsViewModel> model = new List<ExceptionsViewModel>();
            model.Add(new ExceptionsViewModel { name= "Invoice no: #1167 ",title= "Invoice no: #1167 ",action= "6 Days Overdue" });
            model.Add(new ExceptionsViewModel { name= "Account no: #178765", title= "Account no: #178765", action= "Action Required" });
            model.Add(new ExceptionsViewModel { name= "Purchase order: #235", title= "Purchase order: #235", action= "Action Required" });
            model.Add(new ExceptionsViewModel { name= "Account no: #278654", title= "Account no: #278654", action= "Contact  Required" });
            model.Add(new ExceptionsViewModel { name= "Purchase order: #675", title= "Purchase order: #675", action= "Order Overdue, Action Required" });
            return PartialView("Views/Dashboard/ExceptionsTile/Content.cshtml",model);
        }
        public IActionResult getRevenuePredictionsPerMonthsData(int organisation)
        {

            List<RevenuePredictionsPerMonthsViewModel> model = new List<RevenuePredictionsPerMonthsViewModel>
            {
                new RevenuePredictionsPerMonthsViewModel { month = "January", revenue = 50000, expenses = 40000, profit = 10000 },
                new RevenuePredictionsPerMonthsViewModel { month = "February", revenue = 55000, expenses = 42000, profit = 13000 },
                new RevenuePredictionsPerMonthsViewModel { month = "March", revenue = 60000, expenses = 45000, profit = 15000 },
                new RevenuePredictionsPerMonthsViewModel { month = "April", revenue = 58000, expenses = 48000, profit = 10000 },
                new RevenuePredictionsPerMonthsViewModel { month = "May", revenue = 65000, expenses = 50000, profit = 15000 },
                new RevenuePredictionsPerMonthsViewModel { month = "June", revenue = 70000, expenses = 55000, profit = 15000 },
                new RevenuePredictionsPerMonthsViewModel { month = "July", revenue = 75000, expenses = 58000, profit = 17000 },
                new RevenuePredictionsPerMonthsViewModel { month = "August", revenue = 80000, expenses = 60000, profit = 20000 },
                new RevenuePredictionsPerMonthsViewModel { month = "September", revenue = 78000, expenses = 62000, profit = 16000 },
                new RevenuePredictionsPerMonthsViewModel { month = "October", revenue = 85000, expenses = 65000, profit = 20000 },
                new RevenuePredictionsPerMonthsViewModel { month = "November", revenue = 90000, expenses = 68000, profit = 22000 },
                new RevenuePredictionsPerMonthsViewModel { month = "December", revenue = 95000, expenses = 70000, profit = 25000 }
            };
            return PartialView("Views/Dashboard/RevenuePredictionsPerMonthsTile/Content.cshtml", model);
        }
        public IActionResult getTravelTimeSavedAndExpensesData(int organisation)
        {
            List<TravelTimeSavedAndExpensesViewModel> model = new List<TravelTimeSavedAndExpensesViewModel>();
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "January", travelTimeSaved = 20, expensesSaved = 1000 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "February", travelTimeSaved = 18, expensesSaved = 950 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "March", travelTimeSaved = 22, expensesSaved = 1200 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "April", travelTimeSaved = 15, expensesSaved = 800 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "May", travelTimeSaved = 25, expensesSaved = 1400 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "June", travelTimeSaved = 30, expensesSaved = 1600 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "July", travelTimeSaved = 28, expensesSaved = 1500 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "August", travelTimeSaved = 35, expensesSaved = 1800 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "September", travelTimeSaved = 32, expensesSaved = 1700 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "October", travelTimeSaved = 28, expensesSaved = 1600 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "November", travelTimeSaved = 24, expensesSaved = 1300 });
            model.Add(new TravelTimeSavedAndExpensesViewModel { month = "December", travelTimeSaved = 20, expensesSaved = 1100 });

            return PartialView("Views/Dashboard/TravelTimeSavedAndExpensesTile/Content.cshtml", model);
        }
        public async Task<IActionResult> getServiceLogs(string serviceName, string startTime, string jobStatus, string error, string logUrl)
        {
            ServiceLogsTileViewModel model = new ServiceLogsTileViewModel();

            var accessToken = await azureAccessToken();
            if (accessToken != null)
            {
                string logs = await Helper.getLatestLog(logUrl, accessToken);

                if (logs == null)
                {
                    logs = "Unavailable to fetch the logs. Please contact administrator";

                }
               model = new ServiceLogsTileViewModel { serviceName = serviceName, dateStarted = startTime, serviceStatus = jobStatus, error = error, logs = logs };
            }
            return View("Views/Dashboard/ServiceLogsModel/Index.cshtml", model);
        }
        public async Task<IActionResult> getDataLogs(int organisation)
        {
            switch (organisation)
            {
                case 0:
                    return Ok(new StorageViewModel());               
                default:
                    return Ok(new StorageViewModel());

            }
        }
        public async Task<IActionResult> getTableMetrices(int organisation)
        {
            List<TableInfoViewModel>? tables = new List<TableInfoViewModel>();

            try
            {
                switch (organisation)
                {
                   
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return PartialView("Views/Dashboard/TableMetricsTile/Content.cshtml", tables);
        }
        public async Task<IActionResult> getService(int organisation)
        {
            try {
                string filePath = $"{Environment.CurrentDirectory}\\WebJobs.json";

                // Navigate two directories back
                if (!System.IO.File.Exists(filePath))
                {
                    return null;
                }
                var jsonContent = System.IO.File.ReadAllText(filePath);

                // Deserialize JSON to your model
                var azureServiceStatus =  JsonConvert.DeserializeObject<WebJobs>(jsonContent);
                return PartialView("Views/Dashboard/ServicesTile/Content.cshtml", azureServiceStatus.value);
            }
            catch(Exception e)
            {
                return null;
            }
          
         
           


            //var azureServiceStatus = new List<WebJob>();
            var accessToken = await azureAccessToken();
            //if (accessToken != null)
            //{
            //azureServiceStatus = await Helper.getAzureJobs(accessToken);
            //if (organisation == 3)
            //{
            //    azureServiceStatus = azureServiceStatus
            //             .Where(job => job.properties.name.StartsWith("Reali"))
            //             .ToList();
            //}
            //    else if (organisation == 4)
            //    {
            //        azureServiceStatus = azureServiceStatus
            //                 .Where(job => job.properties.name.StartsWith("AhFencing"))
            //                 .ToList();
            //    }
            //    else if (organisation == 5)
            //    {
            //        azureServiceStatus = azureServiceStatus
            //                 .Where(job => job.properties.name.StartsWith("TSR"))
            //                 .ToList();
            //    }
            //    else if (organisation == 6)
            //    {
            //        azureServiceStatus = azureServiceStatus
            //                 .Where(job => job.properties.name.StartsWith("BB"))
            //                 .ToList();
            //    }
            //    else if (organisation == 7)
            //    {
            //        azureServiceStatus = azureServiceStatus
            //                             .Where(job => job.properties.name.StartsWith("Firefly"))
            //                             .ToList();
            //    }
            //    else if (organisation == 8)
            //    {
            //        azureServiceStatus = azureServiceStatus
            //                             .Where(job => job.properties.name.StartsWith("SpannerPlumbing"))
            //                             .ToList();
            //    }
            //    else if (organisation == 9)
            //    {
            //        azureServiceStatus = azureServiceStatus
            //                             .Where(job => job.properties.name.StartsWith("Moonyah"))
            //                             .ToList();
            //    }
            //    else if (organisation == 10)
            //    {
            //        azureServiceStatus = azureServiceStatus
            //                             .Where(job => job.properties.name.StartsWith("MouldMen"))
            //                             .ToList();
            //    }
            //    else
            //    {
            //        azureServiceStatus = new List<WebJob>();
            //    }
            //}

           
        }
        public async Task<IActionResult> activeTile()
        {
            return PartialView();
        }
        public async Task<IActionResult> upgradeTile()
        {
            return PartialView();
        }
        // commented till next update
        //public async Task<IActionResult> jiraReport1()
        //{
        //    List<(DateTime, DateTime)> weeks = Helper.getWeekStartAndEndDates(6);
        //    List<JiraReport1ViewModel> tables = new List<JiraReport1ViewModel>();
        //    int? totalDevelopers = _context.jiraUsers.Where(x => x.isSde == true).Count();
        //    foreach (var week in weeks)
        //    {
        //        var data = await _jiraService.getWeeklyReport(week.Item1, week.Item2);
        //        string title = $"{week.Item1.ToString("dd MMM")} - {week.Item2.ToString("dd MMM")}";
        //        tables.Add(new JiraReport1ViewModel
        //        {
        //            title = title,
        //            totalEstimationForWeek = data.Sum(x => x.totalTimeEstimation),
        //            totalTimeSpentInWeek = data.Sum(x => x.totalTimeTaken),
        //            totalDevelopersAssigned = totalDevelopers,
        //            authors = data
        //                    .SelectMany(issue => issue.uniqueWorkLogAuthors ?? Enumerable.Empty<JiraIssueStatusAuthor>())
        //                    .Where(author => author != null)
        //                    .GroupBy(author => author.id) // Group by id to ensure uniqueness
        //                    .Select(group => group.First()) // Select the first item from each group
        //                    .ToList()
        //        }
        //        );
        //    }
        //    return PartialView("Views/Dashboard/JiraReport1Tile/Content.cshtml", tables);
        //}
        //public async Task<IActionResult> jiraReport2(int day =0)
        //{
        //    if(day>0 || day < -2)
        //    {
        //        day = 0;
        //    }
        //    var tables = await _jiraService.getYesterdayWorkLogs(day);
        //    var users = _context.jiraUsers.Where(x => x.isSde == true).ToList();
        //    foreach (var user in users)
        //    {
        //        var userWorkLog = tables.data?.FirstOrDefault(x => x.authorId == user.id);
        //        if (userWorkLog == null)
        //        {
        //            tables.data?.Add(new JiraReport2ViewModel.WorkLog { authorId=user.id,name = user.fullName, totalTimeInSeconds = 0 });
        //        }
        //        else
        //        {
        //            userWorkLog.name = user.fullName;
        //        }
        //    }
        //    return PartialView("Views/Dashboard/JiraReport2Tile/Content.cshtml", tables);
        //}
        public async Task<string> azureAccessToken()
        {
            try
            {
                Azure.API.Handler _handler = new Azure.API.Handler();
                var fetchClientCredentials = _context.configuration.FirstOrDefault();
                if (fetchClientCredentials != null)
                {
                    if (fetchClientCredentials.accessTokenExpiresAt < DateTime.UtcNow)
                    {
                        var accessToken = await _handler.getAccessToken(fetchClientCredentials.clientId, fetchClientCredentials.clientSecret);
                        if (accessToken != null && accessToken.access_token != null)
                        {
                            fetchClientCredentials.accessToken = accessToken.access_token;
                            fetchClientCredentials.accessTokenExpiresAt = DateTime.UtcNow.AddSeconds(Convert.ToInt32(accessToken.expires_in));
                            _context.Entry(fetchClientCredentials).State = EntityState.Modified;
                            await _context.SaveChangesAsync();

                        }
                    }
                    return fetchClientCredentials.accessToken;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public async Task<IActionResult> triggerWebJob(string webJob)
        {
            try
            {
                var accessToken = await azureAccessToken();
                if (accessToken != null)
                {

                    var response = await Helper.webJobStart(accessToken, webJob);
                    if (response == true)
                    {
                        return Json(new { status = 200 });
                    }

                }
            }
            catch (Exception ex)
            {

            }

            return Json(new { status = 400 });
        }
    }
}
