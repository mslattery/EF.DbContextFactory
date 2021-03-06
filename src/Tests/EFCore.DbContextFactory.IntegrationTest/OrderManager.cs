﻿using EFCore.DbContextFactory.Examples.Data.Entity;
using EFCore.DbContextFactory.Examples.Data.Repository;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.DbContextFactory.IntegrationTest
{
    public class OrderManager
    {
        private readonly IOrderRepository _orderRepository;

        public OrderManager(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Task Create(out List<Order> orders)
        {
            var order1Id = Guid.NewGuid();
            var newOrder1 = new Order
            {
                Date = DateTime.Now,
                Description = $"Order {order1Id}",
                Id = order1Id,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem {Id = Guid.NewGuid(), Name = "Item 1", Quantity = 1, UnitPrice = 1000},
                    new OrderItem {Id = Guid.NewGuid(), Name = "Item 2", Quantity = 4, UnitPrice = 5000}
                }
            };

            var order2Id = Guid.NewGuid();
            var newOrder2 = new Order
            {
                Date = DateTime.Now,
                Description = $"Order {order2Id}",
                Id = order2Id,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem {Id = Guid.NewGuid(), Name = "Item 1", Quantity = 1, UnitPrice = 1000},
                    new OrderItem {Id = Guid.NewGuid(), Name = "Item 2", Quantity = 4, UnitPrice = 5000}
                }
            };

            var order3Id = Guid.NewGuid();
            var newOrder3 = new Order
            {
                Date = DateTime.Now,
                Description = $"Order {order3Id}",
                Id = order3Id,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem {Id = Guid.NewGuid(), Name = "Item 1", Quantity = 1, UnitPrice = 1000},
                    new OrderItem {Id = Guid.NewGuid(), Name = "Item 2", Quantity = 4, UnitPrice = 5000}
                }
            };
            
            var task1 = Task.Factory.StartNew(() =>
            {
                _orderRepository.Add(newOrder2);
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.FromCurrentSynchronizationContext());

            var task2 = Task.Factory.StartNew(() =>
            {
                _orderRepository.Add(newOrder3);
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.FromCurrentSynchronizationContext());

            var task3 = Task.Factory.StartNew(() =>
            {
                _orderRepository.Add(newOrder1);
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.FromCurrentSynchronizationContext());

            orders = new List<Order> { newOrder1, newOrder2, newOrder3 };
            return Task.WhenAll(task1, task2, task3);
        }

        public Task GetAll()
        {
            return Task.Factory.StartNew(() =>
                _orderRepository.GetAllOrders()
            );
        }

        public Task<Order> GetOrderById(Guid id)
        {
            return Task.Factory.StartNew(() =>
                _orderRepository.GetOrderById(id)
            );
        }

        public Task Delete(List<Order> orders)
        {
            var tasks = new List<Task>();
            foreach (var order in orders)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    _orderRepository.DeleteById(order.Id);
                }));
            }

            return Task.WhenAll(tasks);
        }
    }
}
