namespace ApplicationLogic.Dtos;

public class UpdateDto {
    public Guid DocumentId { get; set; }
    public string Update { get; set; } = null!;
}