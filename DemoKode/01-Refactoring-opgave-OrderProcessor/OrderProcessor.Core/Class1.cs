using System.Net.Mail;

namespace OrderProcessor.Core
{
    public class Order
    {
        public int Id { get; private set; }
        public decimal Amount { get; private set; }
        public string CustomerEmail { get; private set; }

        public Order(int id, decimal amount, string customerEmail)
        {
            if (amount <= 0)
                throw new Exception("Invalid amount");
            Id = id;
            Amount = amount;
            CustomerEmail = customerEmail;
        }
    }

    public interface IOrderRepository
    {

        void Save(Order order);
    }

    public class OrderRepository : IOrderRepository
    {
        void IOrderRepository.Save(Order order)
        {
            // 2. Save to database
            using (var connection = new SqlConnection("connectionstring"))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = $"INSERT INTO Orders VALUES ({order.Id}, {order.Amount}, '{order.CustomerEmail}')";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public interface IMail
    {
        void Send(Order order);
        //// 4. Send confirmation email
        //var smtp = new SmtpClient("smtp.server.com");
        //smtp.Send("shop@company.com", order.CustomerEmail, "Order Confirmation", "Thanks for your order!");

    }

    public interface IPayment
        {
        void Charge(decimal amount);
        //    // 3. Charge payment
        //    if (order.Amount > 1000)
        //{
        //    // charge with PayPal
        //}
        //else
        //{
        //    // charge with CreditCard
        //}
    }

    public class OrderProcessor
    {
        private readonly IOrderRepository _repository;
        private readonly IPayment _payment;
        private readonly IMail _mail;
        public OrderProcessor(IOrderRepository repository, IPayment payment, IMail mail)
        {
            _repository = repository;
            _payment = payment;
            _mail = mail;
        }
        public void Process(Order order)
        {

            _repository.Save(order);
            _payment.Charge(order.Amount);
            _mail.Send(order);

        }
    }
}
