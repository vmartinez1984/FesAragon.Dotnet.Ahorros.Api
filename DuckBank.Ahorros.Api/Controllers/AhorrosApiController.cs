using Microsoft.AspNetCore.Mvc;
using DuckBank.Ahorros.Api.Dtos;
using DuckBank.Ahorros.Api.Services;
using DuckBank.Ahorros.Api.Persistence;
using DuckBank.Ahorros.Api.Entities;
using Serilog;

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

            _logger.LogInformation(new EventId(), "Hola mundo");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AhorroDtoIn ahorroDtoIn)
        {
            int id;
            string clabe;
            string tarjeta;
            Ahorro ahorro;

            ahorro = await _repositorio.ObtenerPorIdAsync(ahorroDtoIn.Guid);
            if (ahorro is not null)
                return Ok(ahorro);
            id = await _repositorio.AgregarAsync(new Entities.Ahorro
            {
                Guid = string.IsNullOrEmpty(ahorroDtoIn.Guid) ? Guid.NewGuid().ToString() : ahorroDtoIn.Guid.ToString(),
                Nombre = ahorroDtoIn.Nombre,
                ClienteId = ahorroDtoIn.ClienteId
            });
            // clabe = _clabeService.ObtenerClabe(id.ToString());
            // tarjeta = _tarjetaDeDebitoService.ObtenerTarjeta();
            ahorro = await _repositorio.ObtenerPorIdAsync(id.ToString());
            //ahorro.Otros.Add("clabe", clabe);
            //ahorro.Otros.Add("tarjeta", tarjeta);
            ahorro.Otros.Add("clienteNombre", ahorroDtoIn.ClienteNombre);
            await _repositorio.ActualizarAsync(ahorro);

            return Created($"Ahorros/{id}", new { id });
        }

        [HttpGet("{ahorroId}")]
        public async Task<IActionResult> Get(string ahorroId)
        {
            _logger.LogInformation("Hola mundo");
            Log.CloseAndFlush();

            AhorroConDetalleDto ahorroDto;
            Ahorro ahorro;

            ahorro = await _repositorio.ObtenerPorIdAsync(ahorroId.ToString());
            if (ahorro == null)
                return NotFound(new { Mensaje = "Ahorro no encontrado" });
            ahorroDto = new AhorroConDetalleDto
            {
                Id = ahorro.Id,
                Nombre = ahorro.Nombre,
                Total = ahorro.Total,
                Guid = ahorro.Guid,
                ClienteId = ahorro.ClienteId,
                ClienteNombre = ahorro.Otros.Count > 0 ? ahorro.Otros.Where(x => x.Key == "clienteNombre").First().Value : string.Empty,
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
                }).ToList(),
                Otros = ahorro.Otros
            };

            return Ok(ahorroDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {          
            List<Ahorro> ahorros;
            List<AhorroDto> ahorroDto;

            ahorros = await _repositorio.ObtenerAsync();            
            ahorroDto = ahorros.Select( ahorro => new AhorroDto
            {
                Id = ahorro.Id,
                Nombre = ahorro.Nombre,
                Total = ahorro.Total,
                Guid = ahorro.Guid,
                ClienteId = ahorro.ClienteId,
                ClienteNombre = ahorro.Otros.Count > 0 ? ahorro.Otros.Where(x => x.Key == "clienteNombre").First().Value : string.Empty,                
                Otros = ahorro.Otros
            }).ToList();

            return Ok(ahorroDto);
        }

        /// <summary>
        /// Obtener lista de ahorros por cliente Id
        /// </summary>
        /// <param name="clienteId"></param>
        /// <returns></returns>
        [HttpGet("Clientes/{clienteId}")]
        public async Task<IActionResult> ObtenerListaDeAhorrosPorClienteIdAsync(string clienteId)
        {
            List<AhorroDto> lista;

            lista = (await _repositorio.ObtenerListaDeAhorrosPorClienteIdAsync(clienteId))
                .Select(x => new AhorroDto
                {
                    ClienteId = x.ClienteId,
                    ClienteNombre = x.Otros.Count > 0 ? x.Otros.Where(x => x.Key == "clienteNombre").First().Value : string.Empty,
                    Guid = x.Guid,
                    Id = x.Id,
                    Interes = x.Interes,
                    Nombre = x.Nombre
                })
                .ToList();

            return Ok(lista);
        }

        [HttpPost("{id}/Depositos")]
        public async Task<IActionResult> Depositar(string id, [FromBody] MovimientoDtoIn movimiento)
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

        [HttpPost("{id}/Retiros")]
        public async Task<IActionResult> Retirar(string id, [FromBody] MovimientoDtoIn movimiento)
        {
            Ahorro ahorro;
            Movimiento movimientoEntity;

            ahorro = await _repositorio.ObtenerPorIdAsync(id.ToString());
            if (movimiento.Cantidad > ahorro.Total)
            {
                return StatusCode(428, new
                {
                    Mensaje = "No hay chivo"
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
            ahorro.Retiros.Add(movimientoEntity);
            ahorro.Total = ahorro.Depositos.Sum(x => x.Cantidad) - ahorro.Retiros.Sum(x => x.Cantidad);
            await _repositorio.ActualizarAsync(ahorro);

            return Created("", movimiento);
        }

    }
}