using APBD_Task_10.DTOs;
using APBD_Task_10.Exceptions;
using APBD_Task_10.Services;
using Microsoft.AspNetCore.Mvc;
namespace APBD_Task_10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IDbService _service;

    public ClientsController(IDbService service)
    {
        _service = service;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        try
        {
            await _service.DeleteClientAsync(idClient);
            return Ok("Klient usunięty.");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}