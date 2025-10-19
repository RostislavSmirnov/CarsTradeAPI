namespace CarsTradeAPI.Features.BuyerOperation.BuyerDto
{
    /// <summary>
    /// Класс описывает DTO для показа информации о покупателе
    /// </summary>
    public class ShowBuyerDto
    {
        public required Guid BuyerId { get; set; }

        public required string BuyerName { get; set; }

        public required string BuyerSurname { get; set; }

        public required string BuyerMiddlename { get; set; }

        public required string BuyerEmail { get; set; }

        public required string PhoneNumber { get; set; }

        public required string BuyerAddress { get; set; }
    }
}
