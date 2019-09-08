using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using GZIPWinConsole.Helpers;

namespace GZIPWinConsole.Models
{
    public class ProcessModel : IValidatableObject
    {
        public ProcessType ProcessType { get; set; }

        [Required(ErrorMessage = "Source File was not specified")]
        public FileInfo SourceFile { get; set; }

        [Required(ErrorMessage = "Result File was not specified")]
        public FileInfo ResultFile { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProcessType == ProcessType.Undefined)
            {
                yield return new ValidationResult(
                    "Undefined process type.",
                    new[] { "ProcessType" });
            }

            if (!SourceFile.Exists)
            {
                yield return new ValidationResult(
                    $"The Source File is not exist",
                    new[] { "SourceFile" });
            }
        }
    }
}
