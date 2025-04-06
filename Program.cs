using Microsoft.EntityFrameworkCore;
using SalonBellezaCatalogo.Data;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Usar SQL Server como base de datos
var connectionString = builder.Configuration.GetConnectionString("conexion");
Console.WriteLine("Usando SQL Server como base de datos");
Console.WriteLine($"Cadena de conexión: {connectionString}");

// Configurar el contexto de base de datos para usar SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicializar la base de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("Creando base de datos si no existe...");
        
        // Asegurarse de que la base de datos esté creada y aplicar las migraciones pendientes
        bool databaseCreated = context.Database.EnsureCreated();
        
        if (databaseCreated)
        {
            Console.WriteLine("Base de datos creada correctamente.");
        }
        else
        {
            Console.WriteLine("La base de datos ya existía.");
        }
        
        // Verificar si las tablas existen
        try 
        {
            var serviciosCount = context.Servicios.Count();
            Console.WriteLine($"Tabla Servicios verificada. Contiene {serviciosCount} registros.");
            
            var clientesCount = context.Clientes.Count();
            Console.WriteLine($"Tabla Clientes verificada. Contiene {clientesCount} registros.");
        }
        catch (Exception tableEx)
        {
            Console.WriteLine($"Error al verificar las tablas: {tableEx.Message}");
            Console.WriteLine("Intentando recrear la base de datos...");
            
            // Si hay un problema con las tablas, intentar borrar y recrear
            try 
            {
                context.Database.EnsureDeleted();
                Console.WriteLine("Base de datos eliminada.");
                context.Database.EnsureCreated();
                Console.WriteLine("Base de datos recreada correctamente.");
            }
            catch (Exception recreateEx)
            {
                Console.WriteLine($"Error al recrear la base de datos: {recreateEx.Message}");
            }
        }
        
        Console.WriteLine("Proceso de inicialización de base de datos completado.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ocurrió un error al inicializar la base de datos: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

app.Run();