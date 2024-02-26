using Alloc8_web.Services.Azure;
using Alloc8_web.Utilities;
using Alloc8_web.Validations;
using Alloc8_web.ViewModels.Organisation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alloc8.ef;
using Alloc8.ef.Entities.Dashboard;


namespace Alloc8_web.Controllers
{
    public class OrganisationController : Controller
    {
        private readonly Alloc8DbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IAzureBlobService _azureBlob;
        private string _organisationLogoContainer = "organisationlogo";

        public OrganisationController(Alloc8DbContext context, UserManager<ApplicationUser> userManager, IAzureBlobService azureBlobService)
        {
            _context = context;
            _userManager = userManager;
            _azureBlob = azureBlobService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Organisation>? organisations = await _context.organisations.Include(x => x.user).Where(x=>(x.isDeleted==false || x.isDeleted == null)).ToListAsync();

            return View(organisations);
        }
        [HttpGet]
        public async Task<IActionResult> OrgnaisationTable()
        {
            List<Organisation>? organisations = await _context.organisations.Include(x => x.user).Where(x => (x.isDeleted == false || x.isDeleted == null)).ToListAsync();

            return PartialView(organisations);
        }
        [HttpGet]
        public IActionResult Create()
        {
            // get all tiles
            var tiles = Helper.getDashboardTiles(null);
            return View(tiles);
        }
        [HttpPost]
        public async Task<IActionResult> Create(OrganisationCreateViewModel model)
        {
            // Validation Checks 
            if (!ModelState.IsValid)
            {
                return ModelStateValidator.throwValidationErrors(this);
            }

            Organisation organisation = new Organisation();
            organisation.organizationName = model.organisationName;
            organisation.color = model.color;
            organisation.backgroundColor = $"{model.color}1a";
            organisation.borderColor = model.color;
            organisation.isDeleted = false;
            if (model.organisationProjectViewModel != null && model.organisationProjectViewModel.Count > 0)
            {
                foreach (var project in model.organisationProjectViewModel)
                {
                    if (!string.IsNullOrEmpty(project.projectName))
                    {
                        var projectNameExists = await _context.organisationProjects.Where(op => op.projectName == project.projectName).FirstOrDefaultAsync();

                        if (projectNameExists != null)
                        {
                            projectNameExists.logoImage = project.logoImage;
                            projectNameExists.projectName = project.projectName;
                        }
                        else
                        {
                            organisation.organisationProjects.Add(new OrganisationProjects { projectName = project.projectName, logoImage = project.logoImage });
                        }
                    }
                }
            }
            if (model.tiles != null && model.tiles.Count > 0)
            {
                organisation.dashboardTiles = string.Join(",", model.tiles);
            }
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                organisation.user = user;
            }
            organisation.lastUpdated = DateTime.UtcNow;
            _context.Add(organisation);
            await _context.SaveChangesAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(new { status = 1 });
        }
        public async Task<IActionResult> Edit(int id)
        {
            var organisation = _context.organisations.Include(x => x.organisationProjects).FirstOrDefault(x => x.Id == id);
            if (organisation == null)
            {
                return NotFound();
            }
            var viewModel = new OrganisationEditViewModel(organisation);
            viewModel.tilesList = Helper.getDashboardTiles(id);
            if (!string.IsNullOrEmpty(organisation.dashboardTiles))
            {
                viewModel.selectedTilesList = Helper.getUserDashboardTiles(organisation.dashboardTiles, id);
            }
            else
            {
                viewModel.selectedTilesList = Helper.getUserDashboardTiles("", id);
            }
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(OrganisationEditViewModel model)
        {
            // Validation Checks 
            if (!ModelState.IsValid)
            {
                return await ModelStateValidator.throwValidationErrorsAsync(this);
            }

            Organisation? organisation = await _context.organisations.FirstOrDefaultAsync(x => x.Id == model.id);
            if (organisation == null)
            {
                return BadRequest("No Organisation Found");
            }
            organisation.organizationName = model.organisationName;
            organisation.color = model.color;
            organisation.backgroundColor = $"{model.color}1a";
            organisation.borderColor = model.color;
            organisation.lastUpdated = DateTime.UtcNow;
            organisation.organisationProjects = new List<OrganisationProjects>();
            if (model.organisationProjectViewModel != null && model.organisationProjectViewModel.Count > 0)
            {
                foreach (var project in model.organisationProjectViewModel)
                {
                    if (!string.IsNullOrEmpty(project.projectName))
                    {
                        var projectNameExists = await _context.organisationProjects.Where(op => op.projectName == project.projectName).FirstOrDefaultAsync();
                        if (projectNameExists != null)
                        {
                            projectNameExists.logoImage = project.logoImage;
                            projectNameExists.projectName = project.projectName;
                        }
                        else
                        {
                            organisation.organisationProjects.Add(new OrganisationProjects { projectName = project.projectName, logoImage = project.logoImage });
                        }
                    }
                }
            }

            if (model.tiles != null && model.tiles.Count > 0)
            {
                organisation.dashboardTiles = string.Join(",", model.tiles);
            }
            _context.Entry(organisation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(new { status = 1 });
        }
        public async Task<IActionResult> DoDelete(int id)
        {
            Organisation? organisation = await _context.organisations.FirstOrDefaultAsync(x => x.Id == id);
            if (organisation == null)
            {
                return BadRequest(new { status = 0 });
            }
            
            return View(new OrganisationEditViewModel(organisation));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            Organisation? organisation = await _context.organisations.FirstOrDefaultAsync(x => x.Id == id);
            if (organisation == null)
            {
                return BadRequest(new { status = 0 });
            }
            organisation.isDeleted = true;
            _context.Entry(organisation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = 1 });
        }
        [HttpPost]
        public async Task<IActionResult> uploadProjectLogo(IFormFile logoImage)
        {

            try
            {
                if (logoImage == null)
                {
                    return BadRequest();
                }
                if (_azureBlob == null)
                {
                    return BadRequest("_azureBlob is not initialized.");
                }

                if (_organisationLogoContainer == null)
                {
                    return BadRequest("_organisationLogoContainer is not initialized.");
                }

                string profileImageUrl = await _azureBlob.UploadFileAsync(logoImage, _organisationLogoContainer);

                return Ok(new { profileImageUrl = profileImageUrl });

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> deleteProject(int organisationId, string projectName)
        {
            try
            {
                Organisation? organisation = await _context.organisations
                    .Include(o => o.organisationProjects)
                    .FirstOrDefaultAsync(o => o.Id == organisationId);

                if (organisation == null)
                {
                    return BadRequest(new { status = 0, message = "Organization not found." });
                }

                var projectToDelete = organisation.organisationProjects?.FirstOrDefault(p => p.projectName == projectName);

                if (projectToDelete == null)
                {
                    return BadRequest(new { status = 0, message = "Project not found." });
                }
                _context.organisationProjects.Remove(projectToDelete);
                await _context.SaveChangesAsync();

                return Ok(new { status = 1, message = "Project deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 0, message = ex.Message });
            }
        }
    }
}
