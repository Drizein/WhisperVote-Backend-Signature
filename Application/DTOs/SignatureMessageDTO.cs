using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record SignatureMessageDTO(
    [Required] string message,
    [Required] string surveyId
);