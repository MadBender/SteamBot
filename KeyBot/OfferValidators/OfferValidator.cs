using KeyBot.Models;

namespace KeyBot.OfferValidators
{
    public abstract class OfferValidator
    {
        public abstract bool IsValid(OfferModel o);
    }   
}
