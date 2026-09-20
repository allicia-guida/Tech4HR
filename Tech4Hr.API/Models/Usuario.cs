namespace Tech4Hr.API.Models;

public class Usuario
{
    public int IdUsuario { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public string NivelUsuario { get; set; } = string.Empty;
}