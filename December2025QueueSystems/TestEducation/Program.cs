// See https://aka.ms/new-console-template for more information
using TestEducation.Examples;
using TestEducation.Examples.Observer;

Console.WriteLine("Hello, World!");

UserSubject userSubject = new UserSubject();

userSubject.RegisteredObserver(new EmailService());
userSubject.RegisteredObserver(new SmsService());
userSubject.RegisteredObserver(new DiscountService());

UserService userService=new UserService(new UserRepository(), userSubject);

userService.Register(new User { Id = 1, Email = "a@a.com", Phone = "123321" });