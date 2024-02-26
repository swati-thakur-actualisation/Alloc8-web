using Alloc8_web.Utilities;
using Azure.AI.OpenAI;
using System.Security.Cryptography.Xml;

namespace Alloc8_web.ViewModels.Organisation
{
    public class OrganisationEditViewModel
    {
        public int id { get; set; }
        public string? organisationName { get; set; }
        public string? color { get; set; }
        public List<Tile>? tilesList { get; set; }
        public List<Tile>? selectedTilesList { get; set; }
        public List<string>? tiles { get; set; }
        public List<OrganisationProjectViewModel>? organisationProjectViewModel { get; set; }
        public int? orginationId { get; set; }

        public OrganisationEditViewModel()
        {
            
        }
        public OrganisationEditViewModel(Alloc8.ef.Entities.Dashboard.Organisation model)
        {
            id = model.Id;
            organisationName = model.organizationName;
            color = model.color;
            organisationProjectViewModel = new List<OrganisationProjectViewModel>();
            if (model.organisationProjects != null && model.organisationProjects.Count() > 0)
            {
                foreach (var project in model.organisationProjects)
                {
                    organisationProjectViewModel.Add(new OrganisationProjectViewModel { projectName = project.projectName, logoImage = project.logoImage });
                }
            }
        }


    }
    public class OrganisationProjectViewModel
    {
        public string? logoImage { get; set; }
        public string? projectName { get; set; }
    }

}
