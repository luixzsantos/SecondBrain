using System.ComponentModel.DataAnnotations;

namespace SecondBrain.Application.DTOs;

public record TagDto(Guid Id, string Name);

public class CreateTagRequest
{
    [Required(ErrorMessage = "Name é obrigatório.")]
    [MaxLength(50, ErrorMessage = "Name deve ter no máximo 50 caracteres.")]
    public string Name { get; set; } = string.Empty;
}
