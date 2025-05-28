using APBD_Task_10.DTOs;
using APBD_Task_10.Exceptions;
using APBD_Task_10.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Task_10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly IDbService _service;

    public TripsController(IDbService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetTripsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClient(int idTrip, AssingDto request)
    {
        try
        {
            await _service.AssignClientToTripAsync(idTrip, request);
            return Ok("Nowy klient został dodany do wycieczki.");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}