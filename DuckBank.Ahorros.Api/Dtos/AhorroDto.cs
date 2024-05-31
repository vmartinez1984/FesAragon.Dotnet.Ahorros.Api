using System.ComponentModel.DataAnnotations;

namespace DuckBank.Ahorros.Api.Dtos
{
    public class AhorroConDetalleDto: AhorroDtoIn
    {
        public int Id { get; set; }        
        public decimal Total { get; set; }

        public decimal TotalDeDepositos { get; set; }
        public decimal TotalDeRetiros { get; set; }

        public List<MovimientoDto> Depositos { get; set; }
        public List<MovimientoDto> Retiros { get; set; }
    }

    public class MovimientoDto
    {
        public decimal Cantidad { get; set; }        

        public DateTime FechaDeRegistro { get; set; }

        public string Concepto { get; set; }

        public string Referencia { get; set; }

        public string Id { get; set; }
    }

    public class MovimientoDtoIn
    {
        public decimal Cantidad { get; set; }        

        public string Concepto { get; set; }

        public string Referencia { get; set; }

        public string Id { get; set; }
    }

    public class AhorroDto: AhorroDtoIn
    {
        public int Id { get; set; }
    }

    public class AhorroDtoIn
    {
        [Required]
        [MaxLength(36)]
        public string Guid { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClienteId { get; set; }

        [MaxLength(150)]
        public string ClienteNombre { get; set; }

        public int Interes { get; set; } = 0;

        public string Nota { get; set; }
    }
}