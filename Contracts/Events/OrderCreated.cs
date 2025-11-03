using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Contracts.Events
{
    /// <summary>
    /// Контракт события "Создан заказ"
    /// </summary>
    /// <param name="OrderId"></param>
    /// <param name="BuyerId"></param>
    /// <param name="TotalAmount"></param>
    /// <param name="CreatedAt"></param>
    public record OrderCreated(Guid OrderId, Guid BuyerId, decimal TotalAmount, DateTime CreatedAt)
    {

    }
}
