namespace CinemaBooking.Api;

// What kind of failure this was, in the service's own words. It is not an
// HTTP status code: the service does not know it is being called over HTTP.
// Program.cs is the only place that turns these into 404, 400 and 409.
public enum ErrorKind
{
    None = 0,

    // The thing you asked for does not exist.
    NotFound,

    // The request itself was malformed or out of range.
    Validation,

    // The request was well formed, but it conflicts with current state.
    Conflict
}
