using SE4458_Midterm_20070006074.Models;

namespace SE4458_Midterm_20070006074.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure context is not null
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Create and save subscribers
            if (!context.Subscribers.Any())
            {
                var subscribers = new Subscriber[]
                {
                    new Subscriber { Name = "John", Surname = "Doe", Address = "123 Main St" },
                    new Subscriber { Name = "Jane", Surname = "Smith", Address = "456 Oak Ave" }
                };

                foreach (var s in subscribers)
                {
                    context.Subscribers.Add(s);
                }
                context.SaveChanges();

                // Create and save usages
                var usages = new Usage[]
                {
                    new Usage { UsageType = "Phone", Amount = 100, Date = DateTime.Now, SubscriberId = subscribers[0].SubscriberId },
                    new Usage { UsageType = "Internet", Amount = 500, Date = DateTime.Now, SubscriberId = subscribers[0].SubscriberId },
                    new Usage { UsageType = "Phone", Amount = 200, Date = DateTime.Now, SubscriberId = subscribers[1].SubscriberId },
                    new Usage { UsageType = "Internet", Amount = 1000, Date = DateTime.Now, SubscriberId = subscribers[1].SubscriberId }
                };

                context.Usages.AddRange(usages);
                context.SaveChanges();

                // Create and save bills
                var bills = new Bill[]
                {
                    new Bill { IssueDate = DateTime.Now, DueDate = DateTime.Now.AddDays(30), TotalAmount = 150, Status = "Pending", SubscriberId = subscribers[0].SubscriberId },
                    new Bill { IssueDate = DateTime.Now, DueDate = DateTime.Now.AddDays(30), TotalAmount = 250, Status = "Pending", SubscriberId = subscribers[1].SubscriberId }
                };

                context.Bills.AddRange(bills);
                context.SaveChanges();

                // Create and save bill details
                var billDetails = new BillDetail[]
                {
                    new BillDetail { UsageType = "Phone", Amount = 100, UnitPrice = 0.5m, TotalPrice = 50, BillId = bills[0].BillId },
                    new BillDetail { UsageType = "Internet", Amount = 500, UnitPrice = 0.2m, TotalPrice = 100, BillId = bills[0].BillId },
                    new BillDetail { UsageType = "Phone", Amount = 200, UnitPrice = 0.5m, TotalPrice = 100, BillId = bills[1].BillId },
                    new BillDetail { UsageType = "Internet", Amount = 1000, UnitPrice = 0.2m, TotalPrice = 200, BillId = bills[1].BillId }
                };

                context.BillDetails.AddRange(billDetails);
                context.SaveChanges();

                // Create and save payments
                var payments = new Payment[]
                {
                    new Payment { PaymentDate = DateTime.Now, Amount = 150, PaymentStatus = "Completed", BillId = bills[0].BillId },
                    new Payment { PaymentDate = DateTime.Now, Amount = 250, PaymentStatus = "Completed", BillId = bills[1].BillId }
                };

                context.Payments.AddRange(payments);
                context.SaveChanges();
            }
        }
    }
} 