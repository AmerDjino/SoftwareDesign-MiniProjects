using Expenses.Business;
using Expenses.Presentation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBusinessLayer();

var app = builder.Build();
app.MapExpenseEndpoints();
app.Run();
