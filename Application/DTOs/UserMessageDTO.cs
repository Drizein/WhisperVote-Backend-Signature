using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UserMessageDTO(
    [Required] string message,
    [Required] string userId
);