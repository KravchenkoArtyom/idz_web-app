using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using idz.Models.Entities;
using idz.Models.ViewModels;

namespace idz.Controllers
{
    public class mainController : Controller
    {

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListOfClients()
        {
            List<Clients> clients = new List<Clients>();
            using (var db = new Entities())
            {
                clients = db.Clients.OrderByDescending(x => x.Surname)
                                                .ThenBy(x => x.Name)
                                                .ThenBy(x => x.Patronymic).ToList();
            }
            return View(clients);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListOfPets()
        {
            List<Pets> pets = new List<Pets>();
            using (var db = new Entities())
            {
                pets = db.Pets.OrderByDescending(x => x.Name).ThenBy(x => x.Age).ThenBy(x => x.Additional_Info).ThenBy(x => x.Type).ToList();
            }
            return View(pets);
        }


        [HttpGet]
        [Authorize(Roles ="Admin,Client")]
        public ActionResult ClientDetails(Guid clientID)
        {
            using (var db = new Entities())
            {
                var client = db.Clients
                               .Include("Pets")
                               .Include("Pets.Bookings")
                               .Include("Pets.Bookings.Rooms")
                               .FirstOrDefault(c => c.Client_ID == clientID);

                if (client == null)
                {
                    return HttpNotFound();
                }

                var model = new ClientDetails
                {
                    Client = client,
                    RoomNumbers = client.Pets?.SelectMany(p => p.Bookings)?.Select(b => b.Rooms.Number)?.Distinct()?.ToList() ?? new List<long>(),
                    RoomBookings = client.Pets?.SelectMany(p => p.Bookings)?.Select(b => new RoomBookingInfo
                    {
                        RoomNumber = b.Rooms.Number,
                        BookingStartDate = b.Date_Start,
                        BookingEndDate = b.Date_End
                    })?.Distinct()?.ToList() ?? new List<RoomBookingInfo>(),
                    Pets = client.Pets?.Select(p => new PetInfo
                    {
                        PetID = p.Pet_ID,
                        PetName = p.Name,
                        PetAdditionalInfo = p.Additional_Info,
                        BookingStartDate = p.Bookings.Any() ? p.Bookings.Min(b => b.Date_Start) : (DateTime?)null, 
                        BookingEndDate = p.Bookings.Any() ? p.Bookings.Max(b => b.Date_End) : (DateTime?)null 
                    })?.ToList() ?? new List<PetInfo>()


                };

                return View(model);
            }
        }


        [HttpGet]
        [Authorize(Roles="Admin,Employee,Client")]
        public ActionResult PetDetails(Guid petID)
        {
            using (var db = new Entities())
            {
                var pet = db.Pets
                    .Include("CareLogs")
                    .Include("CareLogs.Employees")
                    .Include("CareLogs.CareTypes")
                    .FirstOrDefault(p => p.Pet_ID == petID);

                if (pet == null)
                {
                    return HttpNotFound();
                }

                // CareServices берем из CareLogs
                var careServices = pet.CareLogs.Select(cl => new CareService
                {
                    Name = cl.CareTypes.Name,
                    Description = cl.CareTypes.Description,
                    Price = cl.CareTypes.Price,
                    EmployeeName = cl.Employees.Name,
                    EmployeeSurname = cl.Employees.Surname,
                    EmployeeNumber = cl.Employees.Number,
                    EmployeePatronymyc = cl.Employees.Patronymic
                }).ToList();

                var model = new PetDetails
                {
                    PetName = pet.Name,
                    PetType = pet.Type,
                    CareServices = careServices
                };
                return View(model);
            }
        }


        //добавление клиентов
        [HttpGet]
        [Authorize(Roles="Admin")]
        public ActionResult CreateClient()
        {         
            return View();      
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClient(CreateClientVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    // Создаем новый питомец на основе данных, предоставленных в модели
                    Clients newClient = new Clients
                    {
                        Client_ID = Guid.NewGuid(),
                        Name = model.Name,
                        Surname = model.Surname,
                        Patronymic = model.Patronymic,
                        Email = model.Email,
                        Number = model.Number,
                        
                    };

                    db.Clients.Add(newClient); // Добавляем питомца в контекст
                    db.SaveChanges(); // Сохраняем изменения в базу данных

                    return RedirectToAction("ListOfClients"); 
                }
            }


            return View(model);

        }


        //добавление питомца

        [HttpGet]
        [Authorize(Roles="Admin")]
        public ActionResult CreatePet()
        {
            using (var db = new Entities())
            {
                // Приводим результат запроса к списку, чтобы материализовать его и перенести обработку на клиент
                var clients = db.Clients.ToList().Select(c => new SelectListItem
                {
                    Value = c.Client_ID.ToString(),
                    Text = $"{c.Name} {c.Surname} {c.Patronymic}"
                }).ToList();

                var model = new CreatePetVM
                {
                    // Используем обработанный локально список клиентов
                    ClientsList = clients
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePet(CreatePetVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    // Создаем новый питомец на основе данных, предоставленных в модели
                    Pets newPet = new Pets
                    {
                        Pet_ID = Guid.NewGuid(), 
                        Name = model.Name,
                        Type = model.Type,
                        Age = model.Age,
                        Additional_Info = model.Additional_Info,
                        Client_ID = model.Client_ID
                    };

                    db.Pets.Add(newPet); 
                    db.SaveChanges(); 

                    return RedirectToAction("ListOfPets"); // Перенаправляем на нужную страницу после успешного добавления
                }
            }

            using (var db = new Entities())
            {
                // Если модель не валидна, возвращаем её обратно в представление
                var clientsInMemory = db.Clients.ToList();

                model.ClientsList = clientsInMemory.Select(c => new SelectListItem
                {
                    Value = c.Client_ID.ToString(),
                    Text = $"{c.Name} {c.Surname} {c.Patronymic}"
                }).ToList();
            }

            return View(model);

        }


        //Добавление нового лога

        [HttpGet]
        [Authorize(Roles="Admin,Employee")]
        public ActionResult CreateCareLog()
        {
            using (var db = new Entities())
            {
                // Приводим результат запроса к списку, чтобы материализовать его и перенести обработку на клиент
                var pets = db.Pets.ToList().Select(c => new SelectListItem
                {
                    Value = c.Pet_ID.ToString(),
                    Text = $"Имя: {c.Name} \n Тип: {c.Type} \n Доп.инфа: {c.Additional_Info} \n Возраст: {c.Age}"
                }).ToList();

                var employees = db.Employees.Include("Positions_Employees").Include("Positions_Employees.Positions").ToList().Select(c => new SelectListItem
                {
                    Value = c.Employee_ID.ToString(),
                    Text = $"ФИО: {c.Surname} {c.Name} {c.Patronymic}  Номер: {c.Number} "
                }).ToList();

                var caretypes = db.CareTypes.ToList().Select(c => new SelectListItem
                {
                    Value = c.CareType_ID.ToString(),
                    Text = $"Название: {c.Name}  Стоимость: {c.Price}"
                }).ToList();

                var model = new CreateCareLogVM
                {
                    PetsList = pets,
                    EmployeesList = employees,
                    CareTypesList = caretypes
                };

                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCareLog(CreateCareLogVM model)
        {

            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {

                    // Создаем новый питомец на основе данных, предоставленных в модели
                    CareLogs log = new CareLogs
                    {
                        CareLog_ID = Guid.NewGuid(),
                        Time = model.Time,
                        Pet_ID = model.Pet_ID,
                        Employee_ID = model.Employee_ID,
                        CareType_ID = model.CareType_ID
                    };
                    try
                    {
                        db.CareLogs.Add(log); 
                        db.SaveChanges(); 
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }

                    if (ModelState.IsValid)
                    {
                        return RedirectToAction("CareLogsList"); // Перенаправляем на нужную страницу после успешного добавления
                    }



                    //return RedirectToAction("CareLogsList"); // Перенаправляем на нужную страницу после успешного добавления
                }



            }


            using (var db = new Entities())
            {
                // Если модель не валидна, возвращаем её обратно в представление
                var PetsInMemory = db.Pets.ToList();
                var EmployeesInMemory = db.Employees.ToList();
                var CareTypesInMemory = db.CareTypes.ToList();

                model.PetsList = PetsInMemory.Select(c => new SelectListItem
                {
                    Value = c.Pet_ID.ToString(),
                    Text = $"{c.Name} {c.Type} {c.Additional_Info} {c.Age}"
                }).ToList();

                model.EmployeesList = EmployeesInMemory.Select(c => new SelectListItem
                {
                    Value = c.Employee_ID.ToString(),
                    Text = $"{c.Surname} {c.Name} {c.Patronymic} {c.Number}"
                }).ToList();

                model.CareTypesList = CareTypesInMemory.Select(c => new SelectListItem
                {
                    Value = c.CareType_ID.ToString(),
                    Text = $"{c.Name} {c.Price}"
                }).ToList();

            }

            return View(model);

        }

        [HttpGet]
        [Authorize(Roles="Admin,Employee")]
        public ActionResult CareLogsList()
        {
            List<CareLogs> logs = new List<CareLogs>();
            //для отображения данных из других таблиц не нужно удалять объект по завершению операции
            var db = new Entities();
            logs = db.CareLogs.OrderByDescending(x => x.Pet_ID).ThenBy(x => x.CareType_ID).ThenBy(x => x.Employee_ID).ThenBy(x => x.Time).ToList();
            return View(logs);
        }



        //Реализация обновления таблиц




        //редактирование питомцев
        [HttpGet]
        [Authorize(Roles="Admin")]
        public ActionResult EditPet(Guid petID)
        {
            CreatePetVM model;
            using (var db = new Entities())
            {

                Pets pet = db.Pets.Find(petID);
                var clients = db.Clients.ToList().Select(c => new SelectListItem
                {
                    Value = c.Client_ID.ToString(),
                    Text = $"{c.Name} {c.Surname} {c.Patronymic}"
                }).ToList();



                model = new CreatePetVM
                {
                    Pet_ID = pet.Pet_ID,
                    Name = pet.Name,
                    Additional_Info = pet.Additional_Info,
                    Age = pet.Age,
                    Type = pet.Type,
                    Client_ID = pet.Client_ID,
                    ClientsList = clients

                };
            }

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult EditPet(CreatePetVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    Pets editedPet = new Pets
                    {
                        Pet_ID = model.Pet_ID,
                        Name = model.Name,
                        Type = model.Type,
                        Age = model.Age,
                        Client_ID = model.Client_ID,
                        Additional_Info = model.Additional_Info,

                    };

                    db.Pets.Attach(editedPet);
                    db.Entry(editedPet).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("ListOfPets");
            }
            return View(model);
        }




        /*редактирование логов*/


        [HttpGet]
        [Authorize(Roles="Admin,Employee")]
        public ActionResult EditLog(Guid careLogID)
        {
            CreateCareLogVM model;
            using (var db = new Entities())
            {
                CareLogs log = db.CareLogs.Find(careLogID);

                var pets = db.Pets.ToList().Select(c => new SelectListItem
                {
                    Value = c.Pet_ID.ToString(),
                    Text = $"Имя: {c.Name} \n Тип: {c.Type} \n Доп.инфа: {c.Additional_Info} \n Возраст: {c.Age}"
                }).ToList();

                var employees = db.Employees.Include("Positions_Employees").Include("Positions_Employees.Positions").ToList().Select(c => new SelectListItem
                {
                    Value = c.Employee_ID.ToString(),
                    Text = $"ФИО: {c.Surname} {c.Name} {c.Patronymic}  Номер: {c.Number} "
                }).ToList();

                var caretypes = db.CareTypes.ToList().Select(c => new SelectListItem
                {
                    Value = c.CareType_ID.ToString(),
                    Text = $"Название: {c.Name}  Стоимость: {c.Price}"
                }).ToList();

                model = new CreateCareLogVM
                {
                    CareType_ID = log.CareType_ID, 
                    Pet_ID = log.Pet_ID, 
                    Time = log.Time, 
                    Employee_ID = log.Employee_ID, 
                    PetsList = pets, 
                    CareTypesList = caretypes, 
                    EmployeesList = employees 
                };
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken()]

        public ActionResult EditLog(CreateCareLogVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    CareLogs editedLog = new CareLogs
                    {
                        CareLog_ID = model.careLogID,
                        CareType_ID = model.CareType_ID,
                        Employee_ID = model.Employee_ID,
                        Pet_ID = model.Pet_ID,
                        Time = model.Time,
                       

                    };


                    try
                    {
                        db.CareLogs.Attach(editedLog);
                        db.Entry(editedLog).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }

                    if (ModelState.IsValid)
                    {
                        return RedirectToAction("CareLogsList");
                    }


                }
                //return RedirectToAction("CareLogsList");
            }


            using (var db = new Entities())
            {
                // Если модель не валидна, возвращаем её обратно в представление
                var PetsInMemory = db.Pets.ToList();
                var EmployeesInMemory = db.Employees.ToList();
                var CareTypesInMemory = db.CareTypes.ToList();

                model.PetsList = PetsInMemory.Select(c => new SelectListItem
                {
                    Value = c.Pet_ID.ToString(),
                    Text = $"{c.Name} {c.Type} {c.Additional_Info} {c.Age}"
                }).ToList();

                model.EmployeesList = EmployeesInMemory.Select(c => new SelectListItem
                {
                    Value = c.Employee_ID.ToString(),
                    Text = $"{c.Surname} {c.Name} {c.Patronymic} {c.Number}"
                }).ToList();

                model.CareTypesList = CareTypesInMemory.Select(c => new SelectListItem
                {
                    Value = c.CareType_ID.ToString(),
                    Text = $"{c.Name} {c.Price}"
                }).ToList();

            }


            return View(model);
        }






        //редактирование клиентов
        [HttpGet]
        [Authorize(Roles="Admin")]
        public ActionResult EditClient(Guid clientID)
        {
            CreateClientVM model;
            using (var db = new Entities())
            {

                Clients cl = db.Clients.Find(clientID);




                model = new CreateClientVM
                {
                    Client_ID = cl.Client_ID,
                    Name = cl.Name,
                    Surname = cl.Surname,
                    Patronymic = cl.Patronymic,
                    Number = cl.Number,
                    Email = cl.Email

                };
            }

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult EditClient(CreateClientVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    Clients editedClient = new Clients
                    {
                        
                        Client_ID = model.Client_ID,
                        Name = model.Name,
                        Surname = model.Surname,
                        Patronymic = model.Patronymic,
                        Number = model.Number,
                        Email = model.Email

                    };

                    db.Clients.Attach(editedClient);
                    db.Entry(editedClient).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("ListOfClients");
            }
            return View(model);
        }



        //реализация удалений


        //удаление клиента

        [HttpGet]
        [Authorize(Roles="Admin")]
        public ActionResult DeleteClient(Guid clientID)
        {
            Clients clientToDelete;
            using(var db = new Entities())
            {
                clientToDelete = db.Clients.Find(clientID);
            }
            
            return View(clientToDelete);
        }

        [HttpPost,ActionName("DeleteClient")]

        public ActionResult DeleteClientConfirmed(Guid clientID)
        {
            using(var db = new Entities())
            {


                Clients clientToDelete = db.Clients.Find(clientID);
                db.Clients.Remove(clientToDelete);
                db.SaveChanges();
               

            }
            return RedirectToAction("ListOfClients");
        }

        //удаление питомца



        [HttpGet]
        [Authorize(Roles="Admin")]
        public ActionResult DeletePet(Guid petID)
        {
            Pets petToDelete;
            using (var db = new Entities())
            {
                petToDelete = db.Pets.Find(petID);

            }
            

            return View(petToDelete);
        }

        [HttpPost, ActionName("DeletePet")]

        public ActionResult DeletePetConfirmed(Guid petID)
        {
            using (var db = new Entities())
            {
                Pets petToDelete = db.Pets.Find(petID);
                db.Pets.Remove(petToDelete);
                db.SaveChanges();
            }
            
            return RedirectToAction("ListOfPets");
        }


        //удаление логов

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]

        public ActionResult DeleteLog(Guid careLogID)
        {
            CareLogs logToDelete;
            var db = new Entities();
            
            logToDelete = db.CareLogs.Find(careLogID);

            


            return View(logToDelete);
        }

        [HttpPost, ActionName("DeleteLog")]
        public ActionResult DeleteLogConfirmed(Guid careLogID)
        {
            var db = new Entities();
            
                CareLogs logToDelete = db.CareLogs.Find(careLogID);
                db.CareLogs.Remove(logToDelete);
                db.SaveChanges();
            

            return RedirectToAction("CareLogsList");
        }




        //создание методов для авторизации 



        string ReturnHashCode(string loginAndSalt)
        {
            string hash = "";
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] data = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(loginAndSalt));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                hash = sBuilder.ToString().ToUpper();
            }
            return hash;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(UserVM webUser)
        {
            if (ModelState.IsValid)
            {
                using (Entities context = new Entities())
                {
                    //идентификация
                    User user = null;
                    user = context.User.Where(u => u.Login == webUser.Login).FirstOrDefault();
                    if (user != null)
                    {
                        //аутентификация
                        string passwordHash = ReturnHashCode(webUser.Password + user.Salt.ToString().ToUpper());
                        if (passwordHash == user.PasswordHash)
                        {
                            string userRole = "";

                            switch (user.UserRole)
                            {
                                case 1:
                                    userRole = "Admin";
                                    break;

                                case 2:
                                    userRole = "Employee";
                                    break;
                                case 3:
                                    userRole = "Client";
                                    break;

                            }

                          

                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, user.Login, DateTime.Now, DateTime.Now.AddDays(1), false, userRole);
                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));

                            return RedirectToAction("ListOfClients", "main");

                        }
                    }
                }
            }


            ViewBag.Error = "Пользователя с таким логином и паролем не существует, попробуйте еще";
            return View(webUser);
        }


        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("ListOfClients", "main");
        }




    }
}