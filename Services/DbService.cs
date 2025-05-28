using APBD_Task_10.Data;
using APBD_Task_10.DTOs;
using APBD_Task_10.Exceptions;
using APBD_Task_10.Models;
using Microsoft.EntityFrameworkCore;

namespace APBD_Task_10.Services;

public class DbService : IDbService
{
    private readonly MasterContext _context;

    public DbService(MasterContext context)
    {
        _context = context;
    }

    public async Task<object> GetTripsAsync(int pageNumber, int recordsPerPage)
    {
        var totalCount = await _context.Trips.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)recordsPerPage);

        var selectedTrips = await _context.Trips
            .Include(x => x.ClientTrips).ThenInclude(x => x.IdClientNavigation)
            .Include(x => x.IdCountries)
            .OrderByDescending(x => x.DateFrom)
            .Skip((pageNumber - 1) * recordsPerPage)
            .Take(recordsPerPage)
            .ToListAsync();

        var mappedTrips = selectedTrips.Select(x => new TripDto
        {
            Name = x.Name,
            Description = x.Description,
            DateFrom = x.DateFrom,
            DateTo = x.DateTo,
            MaxPeople = x.MaxPeople,
            Countries = x.IdCountries.Select(y => new CountryDto { Name = y.Name }).ToList(),
            Clients = x.ClientTrips.Select(z => new ClientDto
            {
                FirstName = z.IdClientNavigation.FirstName,
                LastName = z.IdClientNavigation.LastName
            }).ToList()
        });

        return new { pageNum = pageNumber, pageSize = recordsPerPage, allPages = totalPages, trips = mappedTrips };
    }

    public async Task AssignClientToTripAsync(int tripId, AssingDto data)
    {
        var tripEntity = await _context.Trips.FindAsync(tripId);
        if (tripEntity == null || tripEntity.DateFrom <= DateTime.Now)
            throw new ArgumentException("Wycieczka została już rozpoczęta");

        var foundClient = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == data.Pesel);

        if (foundClient != null)
        {
            var alreadyAssigned = await _context.ClientTrips.AnyAsync(ct => ct.IdClient == foundClient.IdClient && ct.IdTrip == tripId);
            if (alreadyAssigned)
                throw new ArgumentException("Klient o tym ID jest już zapisany na wycieczkę");
        }

        var newClient = foundClient ?? new Client
        {
            FirstName = data.FirstName,
            LastName = data.LastName,
            Email = data.Email,
            Telephone = data.Telephone,
            Pesel = data.Pesel
        };

        if (foundClient == null)
            _context.Clients.Add(newClient);

        await _context.SaveChangesAsync();

        var connection = new ClientTrip
        {
            IdClient = newClient.IdClient,
            IdTrip = tripId,
            RegisteredAt = DateTime.Now,
            PaymentDate = data.PaymentDate
        };

        _context.ClientTrips.Add(connection);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteClientAsync(int clientId)
    {
        var targetClient = await _context.Clients
            .Include(x => x.ClientTrips)
            .FirstOrDefaultAsync(x => x.IdClient == clientId);

        if (targetClient == null)
            throw new NotFoundException("Klient nie istnieje.");

        if (targetClient.ClientTrips.Any())
            throw new ArgumentException("Nie wolno usunąć klienta który ma przypisaną wycieczkę");

        _context.Clients.Remove(targetClient);
        await _context.SaveChangesAsync();
    }
}