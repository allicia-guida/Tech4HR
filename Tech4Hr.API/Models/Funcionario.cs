namespace Tech4Hr.API.Models
{
    public class Funcionario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime DataAdmissao { get; set; }
    }
}
