using System.ComponentModel.DataAnnotations;

namespace Alloc8_web.ViewModels.Dashboard
{
    public class DataLogsViewModel
    {
        public int? minValue { get; set; }
        public int? maxValue { get; set; }
        public string label { get; set; }
        public List<List<string>> data {  get; set; }  
    }
}
