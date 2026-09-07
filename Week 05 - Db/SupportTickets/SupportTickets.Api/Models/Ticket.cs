using System;
using System.Collections.Generic;

namespace SupportTickets.Api.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int Priority { get; set; }

    public DateTime CreatedUtc { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
