using System.ComponentModel.DataAnnotations;
using ShiftManagement.Models;

namespace ShiftManagement.ViewModels;

public class RegisterPatientViewModel
{
    [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
    public DocumentType DocumentType { get; set; } = DocumentType.NationalId;

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El documento no puede superar los 50 caracteres.")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    public string Fullname { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(200, ErrorMessage = "El correo no puede superar los 200 caracteres.")]
    public string? Email { get; set; }
}