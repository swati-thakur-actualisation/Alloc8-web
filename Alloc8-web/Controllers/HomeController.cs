using Alloc8_web.Filters;
using Alloc8_web.Models;
using Alloc8_web.Utilities;
using Alloc8_web.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using Alloc8.ef;
using Alloc8.ef.Entities.Dashboard;

namespace Alloc8_web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Alloc8DbContext _context;

        public HomeController(ILogger<HomeController> logger, Alloc8DbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int client)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC LogDatabaseStats");
            }
            catch (Exception ex)
            {
            }
            List<Tile> tiles = new List<Tile>();
            if (User.IsInRole(WebSiteRoles.Website_Admin))
            {
                if (client > 0)
                {
                    Organisation? organisation = await _context.organisations.Where(x => x.Id == client).FirstOrDefaultAsync();
                    if (organisation != null)
                    {
                        if (!string.IsNullOrEmpty(organisation.dashboardTiles))
                        {
                            tiles = Helper.getUserDashboardTiles(organisation.dashboardTiles, client);

                        }
                    }
                }
                else
                {
                    tiles = Helper.getAdminDashboardTiles(client);
                    var organisation = _context.organisations.Where(x => x.isDeleted == false).ToList();
                    if (organisation != null && organisation.Count() > 0)
                    {
                        ViewBag.OrganisationList = organisation;
                    }

                }
            }
            else
            {
                if (User.Identity != null)
                {
                    var user = await _context.users.Include(x => x.orgination).Where(x => x.Email == User.Identity.Name).FirstOrDefaultAsync();
                    if (user != null && user.orgination != null)
                    {
                        if (!string.IsNullOrEmpty(user.orgination.dashboardTiles))
                        {
                            tiles = Helper.getUserDashboardTiles(user.orgination.dashboardTiles, user.orgination.Id);
                        }
                        ViewBag.userOrgName = user.orgination.organizationName;
                    }
                    if (User.Identity.Name.Contains("@actualisation.ai"))
                    {
                        tiles = Helper.getAdminDashboardTiles(client);
                        var organisation = _context.organisations.Where(x => x.isDeleted == false).ToList();
                        if (organisation != null && organisation.Count() > 0)
                        {
                            ViewBag.OrganisationList = organisation;
                        }
                    }


                }
            }

            if (User.IsInRole(WebSiteRoles.Website_Admin))
            {
                // show according to selected

            }
            else
            {
                // get tiles for current user
            }
            if(User.Identity.Name == "sun@actualisation.ai" || User.Identity.Name == "anusha@actualisation.ai")
            {
                tiles.Add(new Tile { name = "JiraReport2Tile", title = "Daily work Hours Summary", organisation = 0, show = true });

            }
            return View(tiles);
        }
        public IActionResult getNotificationDetails()
        {
            return View();
        }
        public IActionResult notificationsTab()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
