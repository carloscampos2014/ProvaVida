namespace ProvaVida.Maui.Services;

public record LocationResult(double? Latitude, double? Longitude);

public class LocationService
{
    public async Task<LocationResult> ObterLocalizacaoAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                return new LocationResult(null, null);

            var location = await Geolocation.Default.GetLastKnownLocationAsync()
                ?? await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium,
                        TimeSpan.FromSeconds(5)));

            return location is null
                ? new LocationResult(null, null)
                : new LocationResult(location.Latitude, location.Longitude);
        }
        catch
        {
            return new LocationResult(null, null);
        }
    }
}
