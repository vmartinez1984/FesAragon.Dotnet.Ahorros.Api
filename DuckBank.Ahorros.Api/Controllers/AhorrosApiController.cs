using Microsoft.AspNetCore.Mvc;
using DuckBank.Ahorros.Api.Dtos;
using DuckBank.Ahorros.Api.Services;
using DuckBank.Ahorros.Api.Persistence;
using DuckBank.Ahorros.Api.Entities;
using System.Linq;

namespace DuckBank.Ahorros.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AhorrosController : ControllerBase
    {
        private readonly ClabeService _clabeService;
        private readonly TarjetaDeDebitoService _tarjetaDeDebitoService;
        private readonly ILogger<AhorrosController> _logger;
        private readonly AhorroRepositorio _repositorio;

        public AhorrosController(
            ILogger<AhorrosController> logger,
            AhorroRepositorio repositorio,
            ClabeService clabeService,
            TarjetaDeDebitoService tarjetaDeDebitoService
        )
        {
            _clabeService = clabeService;
            _tarjetaDeDebitoService = tarjetaDeDebitoService;
            _logger = logger;
            _repositorio = repositorio;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AhorroDtoIn ahorroDtoIn)
        {
            int id;
            string clabe;
            string tarjeta;

            id = await _repositorio.AgregarAsync(new Entities.Ahorro
            {
                Guid = ahorroDtoIn.Guid == Guid.Empty ? Guid.NewGuid().ToString() : ahorroDtoIn.Guid.ToString(),
                Nombre = ahorroDtoIn.Nombre,
                Nota = ahorroDtoIn.Nota,
                ClienteId = ahorroDtoIn.ClienteId,
                ClienteNombre = ahorroDtoIn.ClienteNombre
            });
            clabe = _clabeService.ObtenerClabe(id.ToString());
            tarjeta = _tarjetaDeDebitoService.ObtenerTarjeta();
            Ahorro ahorro;

            ahorro = await _repositorio.ObtenerPorIdAsync(id.ToString());
            ahorro.Otros.Add("clabe", clabe);
            ahorro.Otros.Add("tarjeta", tarjeta);
            await _repositorio.ActualizarAsync(ahorro);

            return Created($"Ahorros/{id}", new { id });
        }

        [HttpGet("{ahorroId}")]
        public async Task<IActionResult> Get(string ahorroId)
        {
            AhorroDto ahorroDto;
            Ahorro ahorro;

            ahorro = await _repositorio.ObtenerPorIdAsync(ahorroId.ToString());
            if (ahorro == null)
                return NotFound(new { Mensaje = "Ahorro no encontrado" });
            ahorroDto = new AhorroDto
            {
                Id = ahorro.Id,
                Nombre = ahorro.Nombre,
                Total = ahorro.Total,
                TotalDeDepositos = ahorro.TotalDeDepositos,
                TotalDeRetiros = ahorro.TotalDeRetiros,
                Depositos = ahorro.Depositos.Select(x => new MovimientoDto
                {
                    Cantidad = x.Cantidad,
                    FechaDeRegistro = x.FechaDeRegistro,
                    Id = x.Id,
                }).ToList(),
                Retiros = ahorro.Retiros.Select(x => new MovimientoDto
                {
                    Cantidad = x.Cantidad,
                    FechaDeRegistro = x.FechaDeRegistro,
                    Id = x.Id
                }).ToList()
            };

            return Ok(ahorroDto);
        }

        //[HttpGet("{id}/clabes")]
        //public async Task<IActionResult> ObtenerCuentaClabe(string id)
        //{
        //    string clabe;
        //    Ahorro ahorro;

        //    ahorro = await _repositorio.ObtenerPorIdAsync(id);
        //    if (ahorro == null)
        //        return NotFound(new { Mensaje = "Ahorro no encontrado" });
        //    if (ahorro.Otros is null)
        //    {
        //        clabe = null;
        //        ahorro.Otros = new Dictionary<string, string> { };
        //    }
        //    else
        //        clabe = ahorro.Otros.GetValueOrDefault("clabe");
        //    if (string.IsNullOrEmpty(clabe))
        //    {
        //        clabe = _clabeService.ObtenerClabe(id);
        //        ahorro.Otros.Add("clabe", clabe);
        //        await _repositorio.ActualizarAsync(ahorro);
        //    }

        //    return Created("", new { clabe });
        //}

        [HttpPost("{id}/Deposito")]
        public async Task<IActionResult> Depositar(string id, [FromBody] MovimientoDto movimiento)
        {
            Ahorro ahorro;
            Movimiento movimientoEntity;

            ahorro = await _repositorio.ObtenerPorIdAsync(id.ToString());
            movimientoEntity = new Movimiento
            {
                Cantidad = movimiento.Cantidad,
                Concepto = movimiento.Concepto,
                FechaDeRegistro = DateTime.Now,
                Id = movimiento.Id,
                Referencia = movimiento.Referencia,
            };
            ahorro.Depositos.Add(movimientoEntity);
            ahorro.Total = ahorro.Depositos.Sum(x => x.Cantidad) - ahorro.Retiros.Sum(x => x.Cantidad);
            await _repositorio.ActualizarAsync(ahorro);

            return Created("", movimiento);
        }

        [HttpPost("{id}/Retiro")]
        public async Task<IActionResult> Retirar(string id,[FromBody] MovimientoDto movimiento)
        {
            Ahorro ahorro;
            Movimiento movimientoEntity;

            ahorro = await _repositorio.ObtenerPorIdAsync(id.ToString());
            if(movimiento.Cantidad > ahorro.Total)
            {
                return StatusCode(428, new
                {
                    Mensaje= "No hay chivo"
                });
            }
            movimientoEntity = new Movimiento
            {
                Cantidad = movimiento.Cantidad,
                Concepto = movimiento.Concepto,
                FechaDeRegistro = DateTime.Now,
                Id = movimiento.Id,
                Referencia = movimiento.Referencia,
            };
            ahorro.Depositos.Add(movimientoEntity);
            ahorro.Total = ahorro.Depositos.Sum(x => x.Cantidad) - ahorro.Retiros.Sum(x => x.Cantidad);
            await _repositorio.ActualizarAsync(ahorro);

            return Created("", movimiento);
        }

    }
}