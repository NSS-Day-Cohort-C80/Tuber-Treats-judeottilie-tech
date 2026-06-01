using TuberTreats.Models;
using TuberTreats.Models.DTOs;

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

//reusable function so i dont have to keep repeating it inline for each endpoint. filters jointable tuberToppings to rows where TuberOrderId matches, looks up Topping obj from the toppings list using ToppingId, maps it to ToppingDTO and returns it in the List<ToppingDTO>.
//this is why GetToppingsForOrder(order.Id) is where i wouldve put the inline stuff now, replaces that
List<ToppingDTO> GetToppingsForOrder(int orderId) =>
    tuberToppings
        .Where(tt => tt.TuberOrderId == orderId)
        .Select(tt =>
        {
            Topping t = toppings.First(t => t.Id == tt.ToppingId);
            return new ToppingDTO 
            { 
                Id = t.Id, 
                Name = t.Name 
            };
        })
        .ToList();

//add endpoints here

//tuberorders

// get all orders
//looks thru tuberToppings to find rows matching orders Id. looks up Topping object from the toppings list for each row. 
app.MapGet("/tuberorders", () =>
{
    return tuberOrders.Select(o => new TuberOrderDTO
    {
        Id = o.Id,
        OrderPlacedOnDate = o.OrderPlacedOnDate,
        CustomerId = o.CustomerId,
        TuberDriverId = o.TuberDriverId,
        DeliveredOnDate = o.DeliveredOnDate,
        Toppings = GetToppingsForOrder(o.Id)
    });
});

//get order by id
//same as above but also looks up customer and tuberdriver objects and returns as anonymous obj. if driver is unassigned, returns null
app.MapGet("/tuberorders/{id}", (int id) =>
{
    TuberOrder order = tuberOrders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    Customer customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
    TuberDriver driver = tuberDrivers.FirstOrDefault(d => d.Id == order.TuberDriverId);

    return Results.Ok(new TuberOrderDTO
    {
        Id = order.Id,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        CustomerId = order.CustomerId,
        TuberDriverId = order.TuberDriverId,
        DeliveredOnDate = order.DeliveredOnDate,
        Toppings = GetToppingsForOrder(order.Id),
        Customer = customer == null ? null : new CustomerDTO
        { Id = customer.Id, Name = customer.Name, Address = customer.Address },
        Driver = driver == null ? null : new TuberDriverDTO
        { Id = driver.Id, Name = driver.Name }
    });
});

//post new order (API adds orderplacedondate, return new order so client can see Id)
//auto increments ID since we dont have database. finds current max and adds 1. DateTime.Now is the API setting OrderPlacedOnDate Results.Created returns 201 and sets Location header to new resource URL
app.MapPost("/tuberorders", (TuberOrder order) =>
{
    order.Id = tuberOrders.Any() ? tuberOrders.Max(o => o.Id) + 1 : 1;
    order.OrderPlacedOnDate = DateTime.Now;
    order.Toppings = new List<Topping>();
    tuberOrders.Add(order);

    return Results.Created($"/tuberorders/{order.Id}", new TuberOrderDTO
    {
        Id = order.Id,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        CustomerId = order.CustomerId,
        TuberDriverId = order.TuberDriverId,
        DeliveredOnDate = order.DeliveredOnDate,
        Toppings = new List<ToppingDTO>()
    });
});

//put tuberorders id
//finds order and updates TuberDriverId. thats IT. just updates that field
app.MapPut("/tuberorders/{id}", (int id, TuberOrder updateOrder) =>
{
    TuberOrder order = tuberOrders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    order.TuberDriverId = updateOrder.TuberDriverId;

    return Results.Ok(new TuberOrderDTO
    {
        Id = order.Id,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        CustomerId = order.CustomerId,
        TuberDriverId = order.TuberDriverId,
        DeliveredOnDate = order.DeliveredOnDate,
        Toppings = GetToppingsForOrder(order.Id)
    });
});

//post order id complete
//DeliveredOnDate = DateTime.Now is showing that the delivery happened successfully. adds sub route /complete 
app.MapPost("/tuberorders/{id}/complete", (int id) =>
{
    TuberOrder order = tuberOrders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    order.DeliveredOnDate = DateTime.Now;

    return Results.Ok(new TuberOrderDTO
    {
        Id = order.Id,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        CustomerId = order.CustomerId,
        TuberDriverId = order.TuberDriverId,
        DeliveredOnDate = order.DeliveredOnDate,
        Toppings = GetToppingsForOrder(order.Id)
    });
});

//toppings


//get all toppings
app.MapGet("/toppings", () =>
{
    return toppings.Select(t => new ToppingDTO { Id = t.Id, Name = t.Name });
});

//get topping by id
//both toppings endpoints are read only and bring the data back, not editable or deletable 
app.MapGet("/toppings/{id}", (int id) =>
{
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);
    if (topping == null) return Results.NotFound();
    return Results.Ok(new ToppingDTO { Id = topping.Id, Name = topping.Name });
});


//tubertoppings
//these manage the many-to-many relationship 

//get all tubertoppings
app.MapGet("/tubertoppings", () =>
{
    return tuberToppings.Select(tt => new TuberToppingDTO
    {
        Id = tt.Id,
        TuberOrderId = tt.TuberOrderId,
        ToppingId = tt.ToppingId
    });
});

//add a topping to tuberorder
//adds row to the join table (adding a topping to an order)
app.MapPost("/tubertoppings", (TuberTopping tuberTopping) =>
{
    tuberTopping.Id = tuberToppings.Any() ? tuberToppings.Max(tt => tt.Id) + 1 : 1;
    tuberToppings.Add(tuberTopping);

    return Results.Created($"/tubertoppings/{tuberTopping.Id}", new TuberToppingDTO
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        ToppingId = tuberTopping.ToppingId
    });
});

//remove a topping from tuberorder
//needs to know TuberTopping.Id (not the toppingId or orderId) to successfully delete it. 
app.MapDelete("/tubertoppings/{id}", (int id) =>
{
    TuberTopping tuberTopping = tuberToppings.FirstOrDefault(tt => tt.Id == id);
    if (tuberTopping == null) return Results.NotFound();
    tuberToppings.Remove(tuberTopping);
    return Results.Ok(new TuberToppingDTO
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        ToppingId = tuberTopping.ToppingId
    });
});


//customers


//get all customers
//uses anonymous obj to avoid serializing TuberOrders because it would nest a ton of data
app.MapGet("/customers", () =>
{
    return customers.Select(c => new CustomerDTO 
    { 
        Id = c.Id, 
        Name = c.Name, 
        Address = c.Address 
    });
});

//get customer by id with orders
//filters tuberOrders for matching CustomerId to populate TuberOrders
app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null) return Results.NotFound();

    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        TuberOrders = tuberOrders
            .Where(o => o.CustomerId == id)
            .Select(o => new TuberOrderDTO
            {
                Id = o.Id,
                OrderPlacedOnDate = o.OrderPlacedOnDate,
                CustomerId = o.CustomerId,
                TuberDriverId = o.TuberDriverId,
                DeliveredOnDate = o.DeliveredOnDate,
                Toppings = GetToppingsForOrder(o.Id)
            })
            .ToList()
    });
});

//add a customer
app.MapPost("/customers", (Customer customer) =>
{
    customer.Id = customers.Any() ? customers.Max(c => c.Id) + 1 : 1;
    customer.TuberOrders = new List<TuberOrder>();
    customers.Add(customer);

    return Results.Created($"/customers/{customer.Id}",
        new CustomerDTO { Id = customer.Id, Name = customer.Name, Address = customer.Address });
});

//delete a customer
app.MapDelete("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null) return Results.NotFound();
    customers.Remove(customer);
    return Results.Ok(new CustomerDTO 
    { 
        Id = customer.Id, 
        Name = customer.Name, 
        Address = customer.Address 
    });
});


//tuberdrivers


//get all employees
app.MapGet("/tuberdrivers", () =>
{
    return tuberDrivers.Select(d => new TuberDriverDTO 
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

    return Results.Ok(new TuberDriverDTO
    {
        Id = driver.Id,
        Name = driver.Name,
        TuberDeliveries = tuberOrders
            .Where(o => o.TuberDriverId == id)
            .Select(o => new TuberOrderDTO
            {
                Id = o.Id,
                OrderPlacedOnDate = o.OrderPlacedOnDate,
                CustomerId = o.CustomerId,
                TuberDriverId = o.TuberDriverId,
                DeliveredOnDate = o.DeliveredOnDate,
                Toppings = GetToppingsForOrder(o.Id)
            })
            .ToList()
    });
});
app.Run();
//don't touch or move this!
public partial class Program { }