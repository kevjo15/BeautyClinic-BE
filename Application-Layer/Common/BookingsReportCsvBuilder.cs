using System.Globalization;
using System.Text;
using Application_Layer.DTOs;

namespace Application_Layer.Common
{
    /// <summary>
    /// Bygger CSV för bokningsrapporten. Semikolon som avgränsare och
    /// decimalkomma — det format svensk Excel öppnar korrekt direkt.
    /// </summary>
    public static class BookingsReportCsvBuilder
    {
        private static readonly CultureInfo Swedish = CultureInfo.GetCultureInfo("sv-SE");

        public static string Build(BookingsReportDTO report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Datum;Tid;Behandling;Pris (kr);Kund;Personal;Status");

            foreach (var row in report.Rows)
            {
                sb.Append(row.StartTime.ToString("yyyy-MM-dd", Swedish)).Append(';');
                sb.Append(row.StartTime.ToString("HH:mm", Swedish)).Append(';');
                sb.Append(Escape(row.ServiceName)).Append(';');
                sb.Append(row.Price.ToString("0.##", Swedish)).Append(';');
                sb.Append(Escape(row.CustomerName)).Append(';');
                sb.Append(Escape(row.EmployeeName)).Append(';');
                sb.AppendLine(row.IsCancelled ? "Avbokad" : "Aktiv");
            }

            sb.AppendLine();
            sb.Append("Totalt antal;").AppendLine(report.TotalBookings.ToString(Swedish));
            sb.Append("Total omsättning (kr);").AppendLine(report.TotalRevenue.ToString("0.##", Swedish));
            sb.Append("Antal avbokningar;").AppendLine(report.CancelledBookings.ToString(Swedish));

            return sb.ToString();
        }

        // Tecken som får ett kalkylprogram att tolka cellen som en formel.
        private static readonly char[] FormulaTriggers = ['=', '+', '-', '@', '\t', '\r'];

        private static string Escape(string value)
        {
            // Skydd mot CSV-/formelinjektion: en cell som börjar med t.ex. '=' kan
            // exekveras när admin öppnar filen i Excel. Prefixa med ' så tolkas den
            // som text. Kundnamn kan komma ovaliderat från Google-inloggning.
            if (value.Length > 0 && Array.IndexOf(FormulaTriggers, value[0]) >= 0)
            {
                value = "'" + value;
            }

            if (value.Contains(';') || value.Contains('"') ||
                value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
