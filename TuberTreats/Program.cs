using TuberTreats.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

List<TuberDriver> tuberDrivers = new List<TuberDriver>
{
    new TuberDriver
    {
        Id = 1,
        Name = "Sammy Guy",
        TuberDeliveries = new List<TuberOrder>()
    },

    new TuberDriver
    {
        Id = 2,
        Name = "Dog Bupkis",
        TuberDeliveries = new List<TuberOrder>()
    },
    new TuberDriver
    {
        Id = 3,
        Name = "One Punch",
        TuberDeliveries = new List<TuberOrder>()
    }
};

List<Customer> customers = new List<Customer>
{
  new Customer
  {
      Id = 1,
      Name = "Jane Doe",
      Address = "222 Place Over Here",
      TuberOrders = new List<TuberOrder>()
  },
  new Customer
  {
      Id = 2,
      Name = "Mike Michael",
      Address = "234 Last Dr",
      TuberOrders = new List<TuberOrder>()
  },
  new Customer
  {
      Id = 3,
      Name = "God Zilla",
      Address = "3982 Wallaby Way",
      TuberOrders = new List<TuberOrder>()
  },
  new Customer
  {
      Id = 4,
      Name = "Cat Kitty",
      Address = "1111 White House Dr",
      TuberOrders = new List<TuberOrder>()
  },
  new Customer
  {
      Id = 5,
      Name = "Brynn Wave",
      Address = "3478438 California Street",
      TuberOrders = new List<TuberOrder>()
  }
};

List<Topping> toppings = new List<Topping>
{
    new Topping
    {
        Id = 1,
        Name = "Cheese"
    },
    new Topping
    {
        Id = 2,
        Name = "Butter"
    },
    new Topping
    {
        Id = 3,
        Name = "Sour Cream"
    },
    new Topping
    {
        Id = 4,
        Name = "Bacon Bits"
    },
    new Topping
    {
        Id = 5,
        Name = "Green Onion"
    },
    new Topping
    {
        Id = 6,
        Name = "Chili"
    },
    new Topping
    {
        Id = 7,
        Name = "Beans"
    },
    new Topping
    {
        Id = 8,
        Name = "Cheddar Cheese Slice"
    }
};

List<TuberOrder> tuberOrders = new List<TuberOrder>
{
     new TuberOrder
    {
        Id = 1,
        OrderPlacedOnDate = DateTime.Now,
        CustomerId = 1,
        TuberDriverId = 1,
        DeliveredOnDate = null,
        Toppings = new List<Topping>()
    },
    new TuberOrder
    {
        Id = 2,
        OrderPlacedOnDate = DateTime.Now,
        CustomerId = 2,
        TuberDriverId = null,
        DeliveredOnDate = null,
        Toppings = new List<Topping>()
    },
    new TuberOrder
    {
        Id = 3,
        OrderPlacedOnDate = DateTime.Now,
        CustomerId = 3,
        TuberDriverId = 2,
        DeliveredOnDate = null,
        Toppings = new List<Topping>()
    }
};

List<TuberTopping> tuberToppings = new List<TuberTopping>
{
    new TuberTopping
    {
        Id = 1,
        TuberOrderId = 1,
        ToppingId = 1
    },
    new TuberTopping 
    { 
        Id = 2, 
        TuberOrderId = 1, 
        ToppingId = 2 
    },
    new TuberTopping 
    { 
        Id = 3, 
        TuberOrderId = 3, 
        ToppingId = 3 
    },
    new TuberTopping 
    { 
        Id = 4, 
        TuberOrderId = 3, 
        ToppingId = 5 
    }
};

app.UseAuthorization();

//add endpoints here

//tuberorders

// get all orders
app.MapGet("/tuberorders", () =>
{
    return tuberOrders.Select(order =>
    {
        order.Toppings = tuberToppings.Where(tuberTopping => tuberTopping.TuberOrderId == order.Id).Select(tuberTopping => toppings.First(topping => topping.Id == tuberTopping.ToppingId)).ToList();
        return order;
    });
});

//get order by id
app.MapGet("/tuberorders/{id}", (int id) =>
{
    TuberOrder order = tuberOrders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    order.Toppings = tuberToppings.Where(tuberTopping => tuberTopping.TuberOrderId == order.Id).Select(tuberTopping => toppings.First(topping => topping.Id == tuberTopping.ToppingId)).ToList();

    Customer customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
    TuberDriver driver = tuberDrivers.FirstOrDefault(d => d.Id == order.TuberDriverId);

    return Results.Ok(new 
    {
        Id = order.Id,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        CustomerId = order.CustomerId,
        TuberDriverId = order.TuberDriverId,
        DeliveredOnDate = order.DeliveredOnDate,
        Toppings = order.Toppings,
        Customer = customer == null ? null : new { customer.Id, customer.Name, customer.Address },
        Driver = driver == null ? null : new { driver.Id, driver.Name }
    });
});

//post new order (API adds orderplacedondate, return new order so client can see Id)
app.MapPost("/tuberorders", (TuberOrder order) =>
{
    order.Id = tuberOrders.Any() ? tuberOrders.Max(o => o.Id) + 1 : 1;
    order.OrderPlacedOnDate = DateTime.Now;
    order.Toppings = new List<Topping>();
    tuberOrders.Add(order);
    return Results.Created($"/tuberorders/{order.Id}", order);
});

//put tuberorders id
app.MapPut("/tuberorders/{id}", (int id, TuberOrder updateOrder) =>
{
    TuberOrder order = tuberOrders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    order.TuberDriverId = updateOrder.TuberDriverId;
    return Results.Ok(order);
});

//post order id complete
app.MapPost("/tuberorders/{id}/complete", (int id) =>
{
    TuberOrder order = tuberOrders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    order.DeliveredOnDate = DateTime.Now;
    return Results.Ok(order);
});

//toppings


//get all toppings
app.MapGet("/toppings", () =>
{
    return toppings;
});

//get topping by id
app.MapGet("/toppings/{id}", (int id) =>
{
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);
    if (topping == null) 
    return Results.NotFound();
    return Results.Ok(topping);
});


//tubertoppings


//get all tubertoppings
app.MapGet("/tubertoppings", () =>
{
    return tuberToppings;
});

//add a topping to tuberorder
app.MapPost("/tubertoppings", (TuberTopping tuberTopping) =>
{
    tuberTopping.Id = tuberToppings.Any() ? tuberToppings.Max(tt => tt.Id) + 1 : 1;
    tuberToppings.Add(tuberTopping);
    return Results.Created($"/tubertoppings/{tuberTopping.Id}", tuberTopping);
});

//remove a topping from tuberorder
app.MapDelete("/tubertoppings/{id}", (int id) =>
{
    TuberTopping tuberTopping = tuberToppings.FirstOrDefault(tt => tt.Id == id);
    if (tuberTopping == null) return Results.NotFound();
    tuberToppings.Remove(tuberTopping);
    return Results.Ok(tuberTopping);
});


//customers


//get all customers
app.MapGet("/customers", () =>
{
    return customers.Select(c => new 
    { 
        Id = c.Id, 
        Name = c.Name, 
        Address = c.Address 
    });
});

//get customer by id with orders
app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null) return Results.NotFound();

    customer.TuberOrders = tuberOrders.Where(o => o.CustomerId == id).ToList();

    return Results.Ok(customer);
});

//add a customer
app.MapPost("/customers", (Customer customer) =>
{
    customer.Id = customers.Any() ? customers.Max(c => c.Id) + 1 : 1;
    customer.TuberOrders = new List<TuberOrder>();
    customers.Add(customer);
    return Results.Created($"/customers/{customer.Id}", customer);
});

//delete a customer
app.MapDelete("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null) return Results.NotFound();
    customers.Remove(customer);
    return Results.Ok(customer);
});


//tuberdrivers


//get all employees
app.MapGet("/tuberdrivers", () =>
{
    return tuberDrivers.Select(d => new
    {
        Id = d.Id,
        Name = d.Name
    });
});

//get an employee by id with deliveries
app.MapGet("/tuberdrivers/{id}", (int id) =>
{
    TuberDriver driver = tuberDrivers.FirstOrDefault(d => d.Id == id);
    if (driver == null) return Results.NotFound();

    driver.TuberDeliveries = tuberOrders.Where(o => o.TuberDriverId == id).ToList();

    return Results.Ok(driver);
});

app.Run();
//don't touch or move this!
public partial class Program { }