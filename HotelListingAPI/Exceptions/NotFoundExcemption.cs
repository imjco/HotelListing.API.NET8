namespace HotelListingAPI.Exceptions
{
    public class NotFoundExcemption :ApplicationException
    {
        public NotFoundExcemption(string name, object key) : base($"{name} ({key}) was not found")
        {
                
        }

    }
}
