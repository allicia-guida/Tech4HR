namespace Tech4Hr.API.Models
{
    public class RegistroPonto
    {
        public int Id { get; set; }
        public int FuncionarioId { get; set; }
        public DateTime DataHoraRegistro { get; set; }
        public string Tipo { get; set; } = string.Empty;
    }
}
