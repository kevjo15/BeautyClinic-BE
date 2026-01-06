using Domain_Layer.Models;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public BookingModel? Booking { get; }

        private CreateBookingResult(bool isSuccess, string message, BookingModel? booking = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Booking = booking;
        }

        public static CreateBookingResult SuccessResult(string message, BookingModel booking)
        {
            return new CreateBookingResult(true, message, booking);
        }

        public static CreateBookingResult FailureResult(string message)
        {
            return new CreateBookingResult(false, message);
        }
    }
}
