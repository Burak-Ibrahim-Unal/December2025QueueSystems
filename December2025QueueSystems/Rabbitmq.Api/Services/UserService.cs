using Bus.Shared;
using Bus.Shared.Events;
using Rabbitmq.Api.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    public class UserService(IBusService busService,AppDbContext appDbContext)
    {
        public async Task CreateUser()
        {
            using var transaction = await appDbContext.Database.BeginTransactionAsync(); // Veritabanı işlemi başlatılır.

            User newUser = new User // Yeni bir kullanıcı nesnesi oluşturulur.
            {
                UserName = "Burak",
                Email = "burak@burak.com",
                Phone = "5000000000"
            };

            await appDbContext.Users.AddAsync(newUser); // Yeni kullanıcı veritabanına eklenir.
            await appDbContext.SaveChangesAsync(); // Değişiklikler veritabanına kaydedilmez.NewUser için veritabanında Id reserve edilir.

            var userCreatedEvent = new UserCreatedEvent( // Kullanıcı oluşturma olayı nesnesi oluşturulur.
                newUser.Id,
                newUser.UserName,
                newUser.Email,
                newUser.Phone
            );

            var eventData = System.Text.Json.JsonSerializer.Serialize(userCreatedEvent); // Olay verisi JSON formatına serialize edilir.

            var outboxEvent = new OutBox // Outbox olayı nesnesi oluşturulur.
            {
                IdempotencyKey = Guid.NewGuid(), // Benzersiz idempotency anahtarı oluşturulur.
                EventType = EventType.UserCreated,
                EventData = eventData,
                Created = DateTime.UtcNow,
                IsSent = false
            };

            await appDbContext.OutBoxes.AddAsync(outboxEvent); // Outbox olayı veritabanına eklenir.
            await appDbContext.SaveChangesAsync(); // Değişiklikler veritabanına kaydedilir.

            await transaction.CommitAsync(); // İşlem onaylanır.


            #region Esk Kod
            //User user = new User();

            //for (int i = 0; i <= 100; i++)
            //{
            //    user = new User
            //    {
            //        Id = i,
            //        UserName = $"BurakTest{i}",
            //        Email = $"BurakTest{i}@BurakTest{i}.com",
            //    };

            //    await busService.PublishWithNoAck(new UserCreatedEvent(
            //        UserId: user.Id,
            //        UserName: user.UserName,
            //        Email: user.Email
            //    ));
            //} 
            #endregion
        }
    }
}
