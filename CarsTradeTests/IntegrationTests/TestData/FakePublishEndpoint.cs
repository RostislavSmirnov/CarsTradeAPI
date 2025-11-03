using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarsTradeTests.IntegrationTests.TestData
{
    /// <summary>
    /// Простая фиктивная реализация IPublishEndpoint для использования в тестах.
    /// Просто логирует публикации и завершает Task.CompletedTask.
    /// </summary>
    public class FakePublishEndpoint : IPublishEndpoint
    {
        public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish<{typeof(T).Name}> called (message).");
            return Task.CompletedTask;
        }

        public Task Publish<T>(T message, IPipe<PublishContext<T>> pipe, CancellationToken cancellationToken = default) where T : class
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish<{typeof(T).Name}> called (message + pipe).");
            return Task.CompletedTask;
        }

        public Task Publish<T>(T message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default) where T : class
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish<{typeof(T).Name}> called (message + non-generic pipe).");
            return Task.CompletedTask;
        }

        public Task Publish(object message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish (object) called (type={message?.GetType().Name}).");
            return Task.CompletedTask;
        }

        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish (object + Type) called (type={messageType?.Name}).");
            return Task.CompletedTask;
        }

        public Task Publish(object message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish (object + pipe) called (type={message?.GetType().Name}).");
            return Task.CompletedTask;
        }

        public Task Publish(object message, Type messageType, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish (object + Type + pipe) called (type={messageType?.Name}).");
            return Task.CompletedTask;
        }

        public Task Publish<T>(object message, CancellationToken cancellationToken = default) where T : class
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish<{typeof(T).Name}> (object) called.");
            return Task.CompletedTask;
        }

        public Task Publish<T>(object message, IPipe<PublishContext<T>> pipe, CancellationToken cancellationToken = default) where T : class
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish<{typeof(T).Name}> (object + pipe) called.");
            return Task.CompletedTask;
        }

        public Task Publish<T>(object message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default) where T : class
        {
            Console.WriteLine($"[FakePublishEndpoint] Publish<{typeof(T).Name}> (object + non-generic pipe) called.");
            return Task.CompletedTask;
        }

        // Подключение наблюдателя публикаций (MassTransit использует это для логирования/metrics)
        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            Console.WriteLine("[FakePublishEndpoint] ConnectPublishObserver called.");
            return new NoopConnectHandle();
        }

        // В некоторых версиях IPublishEndpoint также наследует IPublishObserverConnector
        // — реализовано через ConnectPublishObserver выше.

        // Вспомогательная реализация ConnectHandle (ничего не делает)
        class NoopConnectHandle : ConnectHandle
        {
            public void Disconnect()
            {
                // ничего не делаем, так как это фейковая реализация
            }

            public void Dispose()
            {
                // Ничего не нужно отключать
            }
        }
    }
}
