namespace Alloc8_web.ViewModels.Organisation
{
    public class OrganisationCreateViewModel
    {
        public string? organisationName { get; set; }
        public string? color { get; set; }
        public List<string>? tiles {  get; set; }
        public List<OrganisationProjectViewModel>? organisationProjectViewModel { get; set; }
    }
}
