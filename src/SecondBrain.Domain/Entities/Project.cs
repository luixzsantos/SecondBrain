namespace SecondBrain.Domain.Entities;

// Um projeto real (ex: "Notification Engine") onde Concepts foram aplicados na prática.
public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ConceptProject> ConceptProjects { get; set; } = [];
}
