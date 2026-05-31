namespace ClassManagement.Domain.Common.Interfaces;

public interface IFullyAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    long? CreatedBy { get; set; }
    long? UpdatedBy { get; set; }
}
