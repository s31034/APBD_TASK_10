using APBD_Task_10.DTOs;

namespace APBD_Task_10.Services;

public interface IDbService
{
    Task<object> GetTripsAsync(int page, int pageSize);
    Task AssignClientToTripAsync(int idTrip, AssingDto request);
    Task DeleteClientAsync(int idClient);
}