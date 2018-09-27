using EFCore.DbContextFactory.Examples.Data.Entity;
using EFCore.DbContextFactory.Examples.Data.Repository;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EFCore.DbContextFactory.IntegrationTest
{
    [Trait("Category", "EFCore")]
    // ReSharper disable once InconsistentNaming
    public class EFCoreTests
    {
        private readonly TestServer _server;

        public EFCoreTests()
        {
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
        }

        [Fact(DisplayName = "EFCore_add_orders_without_EF_DbContextFactory")]
        public async Task EFCore_add_orders_without_EF_DbContextFactory()
        {
            var repo = (OrderRepository)_server.Host.Services.GetService(typeof(OrderRepository));
            var orderManager = new OrderManager(repo);
            ResetDataBase(repo);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
#pragma warning disable 168
                await orderManager.Create(out List<Order> orders);
#pragma warning restore 168
            });
        }

        [Fact(DisplayName = "EFCore_add_orders_with_EF_DbContextFactory")]
        public void EFCore_add_orders_with_EF_DbContextFactory()
        {
            var repo = (OrderRepositoryWithFactory)_server.Host.Services.GetService(typeof(OrderRepositoryWithFactory));
            var orderManager = new OrderManager(repo);
            ResetDataBase(repo);

#pragma warning disable 168
            var task = orderManager.Create(out List<Order> orders);
#pragma warning restore 168
            task.Wait();

            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(3, repo.GetAllOrders().Count());
        }

        [Fact(DisplayName = "EFCore_get_all_orders_with_EF_DbContextFactory")]
        public void EFCore_get_all_orders_with_EF_DbContextFactory()
        {
            var repo = (OrderRepositoryWithFactory)_server.Host.Services.GetService(typeof(OrderRepositoryWithFactory));
            var orderManager = new OrderManager(repo);
            ResetDataBase(repo);

            var taskCreateOrders = orderManager.Create(out List<Order> orders);
            taskCreateOrders.Wait();

            var task = orderManager.GetAll();
            task.Wait();

            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(orders.Count, repo.GetAllOrders().Count());
        }

        [Fact(DisplayName = "EFCore_get_order_by_Id_with_EF_DbContextFactory")]
        public void EFCore_get_order_by_Id_with_EF_DbContextFactory()
        {
            var repo = (OrderRepositoryWithFactory)_server.Host.Services.GetService(typeof(OrderRepositoryWithFactory));
            var orderManager = new OrderManager(repo);
            ResetDataBase(repo);

            var taskCreateOrders = orderManager.Create(out List<Order> orders);
            taskCreateOrders.Wait();

            var task = orderManager.GetOrderById(orders[0].Id);
            task.Wait();
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);

            var result = task.Result;

            result.Should().BeEquivalentTo(orders[0], options => options.IgnoringCyclicReferences());

            Assert.Equal(result.Id, orders[0].Id);
            Assert.Equal(result.Description, orders[0].Description);
        }


        [Fact(DisplayName = "EFCore_delete_orders_with_EF_DbContextFactory")]
        public async Task EFCore_delete_orders_with_EF_DbContextFactory()
        {
            var repo = (OrderRepositoryWithFactory)_server.Host.Services.GetService(typeof(OrderRepositoryWithFactory));
            var orderManager = new OrderManager(repo);
            ResetDataBase(repo);

            await orderManager.Create(out List<Order> orders);
            var task = orderManager.Delete(orders);
            task.Wait();

            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Empty(repo.GetAllOrders());
        }

        [Fact(DisplayName = "EFCore_delete_orders_without_EF_DbContextFactory")]
        public async Task EFCore_delete_orders_without_EF_DbContextFactory()
        {
            var repo = (OrderRepository)_server.Host.Services.GetService(typeof(OrderRepository));
            var repoWithFactory =
                (OrderRepositoryWithFactory)_server.Host.Services.GetService(typeof(OrderRepositoryWithFactory));
            var orderManager = new OrderManager(repo);
            var orderManagerWithFactory = new OrderManager(repoWithFactory);
            ResetDataBase(repo);

            List<Order> orders;
            await orderManagerWithFactory.Create(out orders);
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await orderManager.Delete(orders);
            });
        }

        private void ResetDataBase(IOrderRepository repo)
        {
            repo.DeleteAll();
        }
    }
}
