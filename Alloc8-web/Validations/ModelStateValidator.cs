using Microsoft.AspNetCore.Mvc;

namespace Alloc8_web.Validations
{
    public class ModelStateValidator
    {
        public static IActionResult throwValidationErrors(ControllerBase controller)
        {
            
            return controller.BadRequest(new
            {
                Message = "Please Fill All Required Fields",
                Errors = controller.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
            });
            
        }
        public  static Task<IActionResult> throwValidationErrorsAsync(ControllerBase controller)
        {
            // Extracting errors from ModelState
            var errors = controller.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            // Returning a BadRequest response with a message and errors dictionary
            return Task.FromResult<IActionResult>(controller.BadRequest(new
            {
                Message = "Please Fill All Required Fields",
                Errors = errors
            }));
        }
    }
}
