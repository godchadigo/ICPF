using ICPFWebAPI.Services;
using PFC;

var pfcService = new PFCService("45.32.56.98:5000");
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IPFC>(pfcService);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Allow-Orgin
builder.Services.AddCors(options =>

{

    options.AddPolicy("Open", builder =>

            builder.SetIsOriginAllowed(_ => true).

    AllowAnyMethod().

    AllowAnyHeader().

    AllowCredentials());

});
#endregion

// ²K¥[ IHttpContextAccessor ªA°È
//builder.Services.AddHttpContextAccessor();
//builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

//app.UseSwagger();
//app.UseSwaggerUI();

app.UseCors("Open");

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
