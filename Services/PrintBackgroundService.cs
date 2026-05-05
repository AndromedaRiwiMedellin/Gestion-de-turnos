using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShiftManagement.Data;

namespace ShiftManagement.Services;

public class PrintBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private const string DISPOSITIVO = "/dev/usb/lp0";

    public PrintBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();

                var turnToPrint = await context.Turns
                    .Where(t => !t.IsPrinted)
                    .OrderBy(t => t.CreatedAt)
                    .FirstOrDefaultAsync(stoppingToken);

                if (turnToPrint != null)
                {
                    ImprimirTicket(turnToPrint.Code);
                    turnToPrint.IsPrinted = true;
                    await context.SaveChangesAsync(stoppingToken);
                }
            }

            await Task.Delay(2000, stoppingToken);
        }
    }

    private void ImprimirTicket(string code)
    {
        try
        {
            using var ms = new System.IO.MemoryStream();

            ms.Write(new byte[] { 0x1B, 0x40 });
            ms.Write(new byte[] { 0x1B, 0x61, 0x01 });
            ms.Write(new byte[] { 0x1D, 0x21, 0x00 });
            ms.Write(System.Text.Encoding.ASCII.GetBytes("========================\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("   SISTEMA DE TURNOS\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("========================\n"));
            ms.Write(new byte[] { 0x1D, 0x21, 0x22 });
            ms.Write(System.Text.Encoding.ASCII.GetBytes($"  {code}  \n"));
            ms.Write(new byte[] { 0x1D, 0x21, 0x00 });
            ms.Write(System.Text.Encoding.ASCII.GetBytes("========================\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes($"  {DateTime.Now:dd/MM/yyyy HH:mm}\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("  Espere su llamado\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("========================\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("\n\n\n"));
            ms.Write(new byte[] { 0x1D, 0x56, 0x00 });

            System.IO.File.WriteAllBytes(DISPOSITIVO, ms.ToArray());
            Console.WriteLine($"[Impresora] Turno {code} impreso correctamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Impresora] Error: {ex.Message}");
        }
    }
}