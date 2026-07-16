using Application_Layer.Common;
using Application_Layer.Interfaces;
using Application_Layer.Queries.BookingQueries.GetBookingsReport;
using Domain_Layer.Models;
using FakeItEasy;

namespace Test_Layer.BookingTests;

[TestFixture]
public class GetBookingsReportQueryHandlerTests
{
    private IBookingRepository _bookingRepository = null!;
    private GetBookingsReportQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = A.Fake<IBookingRepository>();
        _handler = new GetBookingsReportQueryHandler(_bookingRepository);
    }

    private static BookingModel Booking(string serviceName, decimal price, DateTime start) => new()
    {
        Id = Guid.NewGuid(),
        StartTime = start,
        EndTime = start.AddMinutes(30),
        Service = new ServiceModel { Name = serviceName, Price = price },
        User = new UserModel { FirstName = "Karin", LastName = "Karlsson" },
        Employee = new UserModel { FirstName = "Emma", LastName = "Andersson" },
    };

    [Test]
    public async Task Handle_AggregatesTotalsAndPerService()
    {
        var day = new DateTime(2026, 7, 1);
        A.CallTo(() => _bookingRepository.GetByDateRangeAsync(A<DateTime>._, A<DateTime>._, A<bool>._))
            .Returns(
            [
                Booking("Botox Panna", 2000m, day.AddHours(10)),
                Booking("Botox Panna", 2000m, day.AddHours(12)),
                Booking("Läppfillers 1 ml", 2500m, day.AddHours(14)),
            ]);

        var report = await _handler.Handle(
            new GetBookingsReportQuery(day, day.AddDays(6)), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.TotalBookings, Is.EqualTo(3));
            Assert.That(report.TotalRevenue, Is.EqualTo(6500m));
            Assert.That(report.CancelledBookings, Is.EqualTo(0));
            Assert.That(report.PerService, Has.Count.EqualTo(2));
            Assert.That(report.PerService[0].ServiceName, Is.EqualTo("Botox Panna"));
            Assert.That(report.PerService[0].Revenue, Is.EqualTo(4000m));
            Assert.That(report.Rows, Has.Count.EqualTo(3));
            Assert.That(report.Rows[0].CustomerName, Is.EqualTo("Karin Karlsson"));
        });
    }

    [Test]
    public async Task Handle_ExcludesCancelledFromTotals_ButCountsAndListsThem()
    {
        var day = new DateTime(2026, 7, 1);
        var cancelled = Booking("Botox Panna", 2000m, day.AddHours(9));
        cancelled.Status = BookingStatus.Cancelled;

        A.CallTo(() => _bookingRepository.GetByDateRangeAsync(A<DateTime>._, A<DateTime>._, A<bool>._))
            .Returns(
            [
                Booking("Botox Panna", 2000m, day.AddHours(10)),
                Booking("Läppfillers 1 ml", 2500m, day.AddHours(14)),
                cancelled,
            ]);

        var report = await _handler.Handle(
            new GetBookingsReportQuery(day, day), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.TotalBookings, Is.EqualTo(2), "avbokade räknas inte som aktiva");
            Assert.That(report.TotalRevenue, Is.EqualTo(4500m), "avbokade ger ingen omsättning");
            Assert.That(report.CancelledBookings, Is.EqualTo(1));
            Assert.That(report.Rows, Has.Count.EqualTo(3), "alla rader listas, även avbokade");
            Assert.That(report.Rows.Count(r => r.IsCancelled), Is.EqualTo(1));
            Assert.That(report.PerService.Sum(l => l.Count), Is.EqualTo(2),
                "per-behandling räknar bara aktiva");
        });
    }

    [Test]
    public async Task Handle_ExcludesNoShowFromTotals_ButCountsAndListsThem()
    {
        var day = new DateTime(2026, 7, 1);
        var noShow = Booking("Botox Panna", 2000m, day.AddHours(9));
        noShow.Status = BookingStatus.NoShow;

        A.CallTo(() => _bookingRepository.GetByDateRangeAsync(A<DateTime>._, A<DateTime>._, A<bool>._))
            .Returns(
            [
                Booking("Botox Panna", 2000m, day.AddHours(10)),
                Booking("Läppfillers 1 ml", 2500m, day.AddHours(14)),
                noShow,
            ]);

        var report = await _handler.Handle(
            new GetBookingsReportQuery(day, day), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.TotalBookings, Is.EqualTo(2), "uteblivna räknas inte som aktiva");
            Assert.That(report.TotalRevenue, Is.EqualTo(4500m),
                "uteblivna ger ingen behandlingsintäkt (no-show-avgiften bokförs hos Stripe)");
            Assert.That(report.NoShowBookings, Is.EqualTo(1));
            Assert.That(report.CancelledBookings, Is.EqualTo(0));
            Assert.That(report.Rows, Has.Count.EqualTo(3), "alla rader listas, även uteblivna");
            Assert.That(report.Rows.Count(r => r.IsNoShow), Is.EqualTo(1));
            Assert.That(report.PerService.Sum(l => l.Count), Is.EqualTo(2),
                "per-behandling räknar bara aktiva");
        });
    }

    [Test]
    public async Task Csv_ShowsNoShowStatusAndCount()
    {
        var day = new DateTime(2026, 7, 1);
        var noShow = Booking("Botox Panna", 2000m, day.AddHours(9));
        noShow.Status = BookingStatus.NoShow;

        A.CallTo(() => _bookingRepository.GetByDateRangeAsync(A<DateTime>._, A<DateTime>._, A<bool>._))
            .Returns([Booking("Läppfillers 1 ml", 2500m, day.AddHours(10)), noShow]);

        var report = await _handler.Handle(new GetBookingsReportQuery(day, day), CancellationToken.None);
        var csv = BookingsReportCsvBuilder.Build(report);

        Assert.Multiple(() =>
        {
            Assert.That(csv, Does.Contain(";Utebliven"), "utebliven rad får status Utebliven");
            Assert.That(csv, Does.Contain("Antal uteblivna;1"));
        });
    }

    [Test]
    public async Task Handle_RequestsCancelledFromRepository_WithInclusiveEndDate()
    {
        var from = new DateTime(2026, 7, 1);
        var to = new DateTime(2026, 7, 31);

        await _handler.Handle(new GetBookingsReportQuery(from, to), CancellationToken.None);

        A.CallTo(() => _bookingRepository.GetByDateRangeAsync(from, to.AddDays(1), true))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenServiceOrEmployeeIsMissing_UsesFallbackText()
    {
        var booking = new BookingModel
        {
            Id = Guid.NewGuid(),
            StartTime = new DateTime(2026, 7, 1, 10, 0, 0),
            Service = null,
            User = null,
            Employee = null,
        };
        A.CallTo(() => _bookingRepository.GetByDateRangeAsync(A<DateTime>._, A<DateTime>._, A<bool>._))
            .Returns([booking]);

        var report = await _handler.Handle(
            new GetBookingsReportQuery(new DateTime(2026, 7, 1), new DateTime(2026, 7, 2)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.Rows[0].ServiceName, Is.EqualTo("Okänd behandling"));
            Assert.That(report.Rows[0].Price, Is.EqualTo(0m));
            Assert.That(report.Rows[0].CustomerName, Is.EqualTo("–"));
        });
    }

    [Test]
    public async Task CsvBuilder_ProducesSwedishExcelFriendlyCsv()
    {
        var day = new DateTime(2026, 7, 1);
        var cancelled = Booking("Läppfillers 1 ml", 2500m, day.AddHours(14));
        cancelled.Status = BookingStatus.Cancelled;

        A.CallTo(() => _bookingRepository.GetByDateRangeAsync(A<DateTime>._, A<DateTime>._, A<bool>._))
            .Returns([Booking("Botox; extra", 1999.50m, day.AddHours(10)), cancelled]);

        var report = await _handler.Handle(
            new GetBookingsReportQuery(day, day), CancellationToken.None);
        var csv = BookingsReportCsvBuilder.Build(report);

        Assert.Multiple(() =>
        {
            Assert.That(csv, Does.StartWith("Datum;Tid;Behandling;Pris (kr);Kund;Personal;Status"));
            Assert.That(csv, Does.Contain("\"Botox; extra\""), "semikolon i namn ska citeras");
            Assert.That(csv, Does.Contain("1999,5"), "svensk decimalkomma");
            Assert.That(csv, Does.Contain(";Aktiv"), "aktiv rad får status Aktiv");
            Assert.That(csv, Does.Contain(";Avbokad"), "avbokad rad får status Avbokad");
            Assert.That(csv, Does.Contain("Bokat värde (kr);1999,5"));
            Assert.That(csv, Does.Contain("Antal avbokningar;1"));
        });
    }
}
