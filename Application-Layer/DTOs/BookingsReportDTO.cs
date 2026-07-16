namespace Application_Layer.DTOs
{
    public class BookingsReportRowDTO
    {
        public DateTime StartTime { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>Avbokad bokning. Räknas inte in i omsättning eller antal aktiva.</summary>
        public bool IsCancelled { get; set; }

        /// <summary>Utebliven (no-show). Räknas inte in i behandlingsomsättningen — kunden betalade aldrig behandlingen.</summary>
        public bool IsNoShow { get; set; }
    }

    public class BookingsReportServiceLineDTO
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class BookingsReportDTO
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        /// <summary>Antal aktiva (ej avbokade) bokningar i intervallet.</summary>
        public int TotalBookings { get; set; }

        /// <summary>Bokat värde: pris för aktiva bokningar (prognos, ej nödvändigtvis inbetalt).</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Faktiskt inbetalt online (hela priset), exkl. återbetalt.</summary>
        public decimal AmountCollected { get; set; }

        /// <summary>Antal avbokade bokningar i intervallet.</summary>
        public int CancelledBookings { get; set; }

        /// <summary>Antal uteblivna (no-show) bokningar i intervallet.</summary>
        public int NoShowBookings { get; set; }

        public List<BookingsReportServiceLineDTO> PerService { get; set; } = [];

        /// <summary>Alla rader i intervallet, både aktiva och avbokade (se <see cref="BookingsReportRowDTO.IsCancelled"/>).</summary>
        public List<BookingsReportRowDTO> Rows { get; set; } = [];
    }
}
