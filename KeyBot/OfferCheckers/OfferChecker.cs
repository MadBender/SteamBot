using KeyBot.Models;

namespace KeyBot.OfferCheckers
{
    public abstract class OfferChecker
    {
        public abstract bool CheckOffer(OfferModel o);
    }   
}
