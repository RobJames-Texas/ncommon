using System.Linq;
using NCommon.Data;
using NCommon.Data.EntityFramework;
using NCommon.EntityFramework4.Tests.Models;
using NCommon.Extensions;
using NUnit.Framework;
using Rhino.Mocks;

namespace NCommon.EntityFramework4.Tests.CodeOnly
{
    public class EFRepositoryEagerFetchingTests : EFRepositoryQueryTestsBase
    {
        [Test]
        public void Can_eager_fetch()
        {
            var testData = new EFTestData(Context);

            Order order = null;
            Order savedOrder = null;

            testData.Batch(x => order = x.CreateOrderForCustomer(x.CreateCustomer()));

            using (var scope = new UnitOfWorkScope())
            {
                savedOrder = new EFRepository<Order>()
                    .Fetch(o => o.Customer)
                    .Where(x => x.OrderID == order.OrderID)
                    .SingleOrDefault();
                scope.Commit();
            }

            Assert.That(savedOrder != null);
            Assert.That(savedOrder.Customer != null);
            Assert.DoesNotThrow(() => { var firstName = savedOrder.Customer.FirstName; });

        }

        [Test]
        public void Can_eager_fetch_many()
        {
            var testData = new EFTestData(Context);

            Customer customer = null;
            Customer savedCustomer = null;
            testData.Batch(x =>
            {
                customer = x.CreateCustomer();
                var order = x.CreateOrderForCustomer(customer);
                order.OrderItems.Add(x.CreateOrderItem(item => item.Order = order));
                order.OrderItems.Add(x.CreateOrderItem(item => item.Order = order));
                order.OrderItems.Add(x.CreateOrderItem(item => item.Order = order));
            });

            using (var scope = new UnitOfWorkScope())
            {
                savedCustomer = new EFRepository<Customer>()
                    .FetchMany(x => x.Orders)
                    .ThenFetchMany(x => x.OrderItems)
                    .ThenFetch(x => x.Product)
                    .Where(x => x.CustomerID == customer.CustomerID)
                    .SingleOrDefault();
                scope.Commit();
            }

            Assert.That(savedCustomer != null);
            Assert.That(savedCustomer.Orders != null);
            savedCustomer.Orders.ForEach(order =>
            {
                Assert.That(order.OrderItems != null);
                order.OrderItems.ForEach(orderItem => Assert.That(orderItem.Product != null));
            });
        }

        [Test]
        public void Can_eager_fetch_using_for()
        {
            Locator.Stub(x => x.GetAllInstances<IFetchingStrategy<Customer, EFRepositoryEagerFetchingTests>>())
                .Return(new[] { new FetchingStrategy() });

            var testData = new EFTestData(Context);
            Customer customer = null;
            Customer savedCustomer = null;
            testData.Batch(x =>
            {
                customer = x.CreateCustomer();
                var order = x.CreateOrderForCustomer(customer);
                order.OrderItems.Add(x.CreateOrderItem(item => item.Order = order));
                order.OrderItems.Add(x.CreateOrderItem(item => item.Order = order));
                order.OrderItems.Add(x.CreateOrderItem(item => item.Order = order));
            });

            using (var scope = new UnitOfWorkScope())
            {
                savedCustomer = new EFRepository<Customer>()
                    .For<EFRepositoryEagerFetchingTests>()
                    .Where(x => x.CustomerID == customer.CustomerID)
                    .SingleOrDefault();
                scope.Commit();
            }

            Assert.That(savedCustomer != null);
            Assert.That(savedCustomer.Orders != null);
            savedCustomer.Orders.ForEach(order =>
            {
                Assert.That(order.OrderItems != null);
                order.OrderItems.ForEach(orderItem => Assert.That(orderItem.Product != null));
            });
        }

        class FetchingStrategy : IFetchingStrategy<Customer, EFRepositoryEagerFetchingTests>
        {
            public IQueryable<Customer> Define(IRepository<Customer> repository)
            {
                return repository.FetchMany(x => x.Orders)
                    .ThenFetchMany(x => x.OrderItems)
                    .ThenFetch(x => x.Product);
            }
        }
    }
}