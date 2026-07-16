namespace Application_Layer.Common
{
    /// <summary>
    /// Hjälpare för Stripe-belopp. Delas av PaymentIntent-skapandet och serverside-
    /// verifieringen vid bokning så att samma belopp alltid gäller (klienten får inte
    /// bestämma beloppet — det härleds alltid från behandlingens pris).
    /// </summary>
    public static class PaymentAmounts
    {
        /// <summary>kr → minsta enhet (öre).</summary>
        public static long ToMinorUnit(decimal amountKr) => (long)Math.Round(amountKr * 100m);
    }
}
